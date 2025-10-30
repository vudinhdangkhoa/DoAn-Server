using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Models.Payment;
using server.Services;

namespace server.contollers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly MoMo_Services _momoService;
        private readonly VNPay_Services _vnpayService;
        private readonly MyDbContext _context;
        private readonly Mail_Services _mailService;
        private readonly IConfiguration _configuration;

        public PaymentController(MoMo_Services momoService, Mail_Services mailService, VNPay_Services vnpayService, MyDbContext context, IConfiguration configuration)
        {
            _momoService = momoService;
            _mailService = mailService;
            _vnpayService = vnpayService;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("CreatePayment")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            // 1. Kiểm tra nghiệp vụ (giữ nguyên như trước)
            var lopHoc = await _context.LopHocs.FindAsync(request.IdLopHoc);
            if (lopHoc == null) return NotFound(new { message = "Lớp học không tồn tại." });
            if (lopHoc.SoLuongHv >= lopHoc.SoLuongToiDa) return BadRequest(new { message = "Lớp học đã đầy." });

            if (request.HocVienId.HasValue)
            {
                var hocVien = await _context.HocViens.FindAsync(request.HocVienId.Value);
                if (hocVien == null)
                {
                    PhuHuynh phuHuynh = await _context.PhuHuynhs.FirstOrDefaultAsync(t => t.UserId == request.PhuHuynhId);
                    var newHocVien = new HocVien
                    {
                        TenHv = phuHuynh.TenPh,
                        NgaySinh = phuHuynh.NgaySinh,
                        Avartar = phuHuynh.Avatar,
                        NgayTao = DateOnly.FromDateTime(DateTime.Now),
                        IdPhuHuynh = phuHuynh.UserId,
                        GioiTinh = "Khác",

                    };
                    await _context.HocViens.AddAsync(newHocVien);
                    await _context.SaveChangesAsync();
                }
            }

            if (request.HocVienId.HasValue)
            {
                bool isDuplicate = await Helper.CheckTrungLichHoc(request.HocVienId.Value, request.IdLopHoc, _context);
                if (isDuplicate) return BadRequest(new { message = "Lịch học của học viên bị trùng." });
            }

            // 2. Tạo Hóa Đơn Khóa Học với trạng thái "Pending"
            var newHoaDonKhoaHoc = new HoaDonKhoaHoc
            {
                IdLopHoc = request.IdLopHoc,
                HocVienId = request.HocVienId,
                IdKhoaHoc = request.KhoaHocId,
                TongTien = request.Amount,
                GiamGia = 0,
                Ngaytao = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = false // Pending
            };
            _context.HoaDonKhoaHocs.Add(newHoaDonKhoaHoc);
            await _context.SaveChangesAsync();

            string uniqueOrderId = $"{newHoaDonKhoaHoc.IdHoaDon}_{DateTime.UtcNow.Ticks}";
            // 3. Logic rẽ nhánh để tạo link thanh toán
            string payUrl = string.Empty;
            if (request.PaymentMethod == DungChung.PaymentMoMo)
            {
                var MoMoRequest = new MoMoRequestModel
                {
                    Amount = request.Amount,
                    OrderId = uniqueOrderId,
                    OrderInfo = $"Thanh toan hoa don khoa hoc ID: {newHoaDonKhoaHoc.IdHoaDon}",

                };
                var MoMoResult = await _momoService.CreateMoMoPaymentAsync(MoMoRequest);
                var momoResponse = JsonSerializer.Deserialize<JsonElement>(MoMoResult);
                payUrl = momoResponse.GetProperty("payUrl").GetString();
            }
            else if (request.PaymentMethod == DungChung.PaymentVNPay)
            {
                var VNPayRequest = new VNPayRequestModel
                {
                    Amount = request.Amount,
                    OrderId = uniqueOrderId,
                    OrderInfo = $"Thanh toan hoa don khoa hoc ID: {newHoaDonKhoaHoc.IdHoaDon}",
                };
                var VNPayResult = _vnpayService.CreatePaymentUrl(VNPayRequest.OrderId, VNPayRequest.Amount, VNPayRequest.OrderInfo);
                payUrl = VNPayResult;
            }
            else
            {
                return BadRequest(new { message = "Phương thức thanh toán không hợp lệ." });
            }

            return Ok(new { payUrl });
        }

        [HttpPost("MomoNotify")]
        public async Task<IActionResult> MomoNotify([FromBody] MoMoIpnResponse response)
        {
            // Lấy các giá trị từ cấu hình
            string accessKey = _configuration["Momo:AccessKey"];
            string secretKey = _configuration["Momo:SecretKey"];

            string rawSignature =
                $"accessKey={accessKey}" +
                $"&amount={response.Amount}" +
                $"&extraData={response.ExtraData}" +
                $"&message={response.Message}" +
                $"&orderId={response.OrderId}" +
                $"&orderInfo={response.OrderInfo}" +
                $"&orderType={response.OrderType}" +
                $"&partnerCode={response.PartnerCode}" +
                $"&payType={response.PayType}" +
                $"&requestId={response.RequestId}" +
                $"&responseTime={response.ResponseTime}" +
                $"&resultCode={response.ResultCode}" +
                $"&transId={response.TransId}";

            var calculatedSignature = MoMo_Services.HmacSHA256(rawSignature, secretKey);

            // GHI LOG ĐỂ KIỂM TRA (Rất quan trọng)
            Console.WriteLine("--- MoMo IPN Verification ---");
            Console.WriteLine("Data received: " + JsonSerializer.Serialize(response));
            Console.WriteLine("Raw Signature String to Hash: " + rawSignature);
            Console.WriteLine("My Calculated Signature: " + calculatedSignature);
            Console.WriteLine("MoMo's Signature: " + response.Signature);
            Console.WriteLine("-----------------------------");

            // 1. So sánh chữ ký
            if (calculatedSignature != response.Signature)
            {
                Console.WriteLine("ERROR: Signature mismatch!");
                // Theo tài liệu MoMo, khi nhận IPN, không nên trả về lỗi mà chỉ cần không xử lý
                // Trả về OK để MoMo không gửi lại IPN nữa.
                return NoContent();
            }

            Console.WriteLine("SUCCESS: Signature is valid!");

            // 2. Cập nhật hóa đơn nếu thanh toán thành công
            if (response.ResultCode == 0)
            {
                await ProcessSuccessfulPayment(response.OrderId, "MoMo");
            }

            // Luôn trả về Ok() cho MoMo
            return NoContent();
        }

        // === XỬ LÝ RETURN URL TỪ VNPAY ===
        [HttpGet("VNPayReturn")]
        public async Task<IActionResult> VNPayReturn()
        {
            // Lấy chuỗi query thô (ví dụ: "?vnp_Amount=10000&vnp_OrderInfo=Thanh%20toan...")
            string rawQuery = HttpContext.Request.QueryString.Value;

            // Lấy IQueryCollection (đã giải mã) chỉ để lấy các giá trị riêng lẻ
            var vnpayData = HttpContext.Request.Query;
            var vnp_SecureHash = vnpayData["vnp_SecureHash"].ToString();
            var orderId = vnpayData["vnp_TxnRef"].ToString();
            string frontendUrl = _configuration["FrontendPaymentRedirectUrl"] ?? "http://localhost:3000/payment-result"; // Lấy từ config

            // 1. Xác thực chữ ký bằng CHUỖI THÔ
            var isValidSignature = _vnpayService.ValidateSignature(rawQuery, vnp_SecureHash);
            Console.WriteLine($"VNPay Return Signature Valid: {isValidSignature}");

            if (!isValidSignature)
            {
                // Chữ ký không hợp lệ
                return Redirect($"{frontendUrl}?status=fail&orderId={orderId}&message=InvalidSignature");
            }

            // 2. Cập nhật hóa đơn nếu thanh toán thành công
            if (vnpayData["vnp_ResponseCode"] == "00")
            {
                await ProcessSuccessfulPayment(orderId, "VNPay_Return");
                return Redirect($"{frontendUrl}?status=success&orderId={orderId}");
            }
            else
            {
                // Thanh toán thất bại
                return Redirect($"{frontendUrl}?status=fail&orderId={orderId}");
            }
        }

        // === API ĐỂ FRONTEND XÁC THỰC LẠI GIAO DỊCH ===
        [HttpPost("VerifyPayment")]
        public async Task<IActionResult> VerifyPayment([FromBody] Dictionary<string, string> request)
        {
            if (!request.ContainsKey("orderId") && !request.ContainsKey("vnp_TxnRef"))
            {
                return BadRequest(new { message = "Mã hóa đơn không hợp lệ." });
            }
             string orderId = "";
            if (request.ContainsKey("vnp_TxnRef"))
            {
                orderId = request["vnp_TxnRef"];
            }
            else
            {
                orderId = request["orderId"];
            }
           
            var idParts = orderId.Split('_');
            if (!int.TryParse(idParts[0], out int hoaDonId))
            {
                return BadRequest(new { message = "Định dạng mã hóa đơn không hợp lệ." });
            }

            // SỬA LỖI LOGIC:
            // 1. Xác định trạng thái thành công TỪ CLIENT
            bool isSuccess = false;
            if (request.ContainsKey("resultCode") && request["resultCode"] == "0") // Từ MoMo
            {
                isSuccess = true;
            }
            else if (request.ContainsKey("status") && request["status"] == "success") // Từ VNPay (hoặc logic tự định nghĩa)
            {
                isSuccess = true;
            }

            Console.WriteLine($"Verifying parameter: {JsonSerializer.Serialize(request)}. Client success: {isSuccess}");

            // 2. CHỈ gọi xử lý nếu client báo thành công
            // Hàm ProcessSuccessfulPayment đã có kiểm tra (idempotency)
            // nên việc gọi lại (nếu IPN/Return đã chạy) là an toàn.
            if (isSuccess)
            {
                await ProcessSuccessfulPayment(orderId, "Client_Verify");
            }

            // 3. Tải hóa đơn (SAU KHI ĐÃ CẬP NHẬT) để trả về trạng thái MỚI NHẤT
            // Đây chính là mấu chốt để "kịp cập nhật"
            var hoaDon = await _context.HoaDonKhoaHocs
                .Include(hd => hd.IdLopHocNavigation)
                .Include(hd => hd.IdKhoaHocNavigation)
                .FirstOrDefaultAsync(hd => hd.IdHoaDon == hoaDonId);

            if (hoaDon == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn." });
            }

            // 4. Tạo phản hồi
            // SỬA LỖI N+1: Sử dụng navigation properties đã .Include()
            var response = new
            {
                status = hoaDon.TrangThai == true ? "success" : "fail",
                invoiceDetails = new
                {
                    hoaDon.IdHoaDon,
                    hoaDon.IdLopHoc,
                    hoaDon.IdKhoaHoc,
                    // Dùng dữ liệu đã nạp, không query lại CSDL
                    tenKhoaHoc = hoaDon.IdKhoaHocNavigation?.TenKhoaHoc,
                    tenLopHoc = hoaDon.IdLopHocNavigation?.TenLopHoc,
                    hoaDon.TongTien
                }
            };

            return Ok(response);
        }

        // === HÀM DÙNG CHUNG ĐỂ XỬ LÝ THANH TOÁN THÀNH CÔNG ===
        private async Task ProcessSuccessfulPayment(string orderId, string paymentMethod)
        {
            Console.WriteLine($"Đã vào hàm ProcessSuccessfulPayment với orderId: {orderId} từ {paymentMethod}");
            var idParts = orderId.Split('_');
            if (!int.TryParse(idParts[0], out int hoaDonId))
            {
                Console.WriteLine($"Lỗi: Không thể parse hoaDonId từ orderId: {orderId}");
                return;
            }

            // SỬA LỖI THIẾU TRANSACTION: Đảm bảo toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tải hóa đơn và khóa nó (nếu CSDL hỗ trợ)
                var hoaDon = await _context.HoaDonKhoaHocs.FindAsync(hoaDonId);

                // Kiểm tra Idempotency (quan trọng): Chỉ xử lý hóa đơn chưa thanh toán
                if (hoaDon == null)
                {
                    Console.WriteLine($"Bỏ qua: Không tìm thấy hóa đơn {hoaDonId}.");
                    await transaction.RollbackAsync();
                    return;
                }

                if (hoaDon.TrangThai == true)
                {
                    Console.WriteLine($"Bỏ qua: Hóa đơn {hoaDonId} đã được xử lý trước đó.");
                    await transaction.RollbackAsync();
                    return;
                }

                // 1. Cập nhật trạng thái hóa đơn
                hoaDon.TrangThai = true;

                // 2. Tăng sĩ số lớp
                var lopHoc = await _context.LopHocs.FindAsync(hoaDon.IdLopHoc);
                if (lopHoc != null)
                {
                    lopHoc.SoLuongHv += 1;
                }

                // 3. Lưu tất cả thay đổi vào CSDL
                // Không cần gọi .Update() vì EF Core 8 đang theo dõi (track) các đối tượng này
                await _context.SaveChangesAsync();

                // 4. Commit transaction
                await transaction.CommitAsync();
                Console.WriteLine($"Thành công: Đã xử lý thanh toán cho hóa đơn {hoaDonId}.");

                // 5. Gửi Email (SAU KHI ĐÃ COMMIT)
                // Đặt trong try/catch riêng để nếu gửi mail lỗi, không ảnh hưởng đến thanh toán
                try
                {
                    // Tải thông tin người dùng để gửi mail
                    var emailNguoiNhan = await _context.HocViens
                        .Where(hv => hv.IdHocVien == hoaDon.HocVienId)
                        .Include(hv => hv.IdPhuHuynhNavigation.User)
                        .Select(hv => hv.IdPhuHuynhNavigation.User.Mail)
                        .FirstOrDefaultAsync();

                    if (emailNguoiNhan != null)
                    {
                        byte[] hoaDonPDF = await Helper.TaoHoaDonPDF(hoaDon, _context);
                        await _mailService.SendEmailToUser(
                            toEmail: emailNguoiNhan,
                            subject: "Hóa đơn khóa học",
                            body: $"Cảm ơn bạn đã đăng ký khóa học. Hóa đơn của bạn được đính kèm trong email này.",
                            hoaDonPDF: hoaDonPDF
                        );
                    }
                    else
                    {
                        Console.WriteLine($"Lỗi gửi mail: Không tìm thấy email cho HocVienId {hoaDon.HocVienId}");
                    }
                }
                catch (Exception ex_email)
                {
                    Console.WriteLine($"Lỗi gửi mail cho hóa đơn {hoaDonId} (Thanh toán đã thành công): {ex_email.Message}");
                    // Không rollback vì thanh toán đã thành công
                }
            }
            catch (Exception ex_db)
            {
                // Nếu có lỗi CSDL, rollback lại
                Console.WriteLine($"LỖI DB: {ex_db.Message}. Rollback transaction cho hóa đơn {hoaDonId}.");
                await transaction.RollbackAsync();
            }
        }
    }

}
