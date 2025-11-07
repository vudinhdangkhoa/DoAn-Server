using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using server.Models;
using iText;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Font;

namespace server
{

    public class DungChung
    {
        static public string adminRole = "Admin";
        static public string nhanVienKhoRole = "Nhân Viên Kho";
        static public string trangThaiLopHoc_DangMo = "đang mở";
        static public string trangThaiLopHoc_TamNgung = "tạm ngừng";
        static public string trangThaiLopHoc_DaKetThuc = "kết thúc";
        static public string trangThaiLopHoc_Huy = "hủy";
        static public string PaymentMoMo = "MoMo";
        static public string PaymentVNPay = "VNPay";

        static public string UrlFrontEnd = "http://localhost:3000";

    }
    public class Helper
    {

        public static bool checkKhoaHocDate(DateOnly start, string soBuoiTrenTuan, int soLuongBuoi)
        {

            DateOnly end;
            int countBuoiTrenTuan = soBuoiTrenTuan.Split(',').Length;
            end = start.AddDays((soLuongBuoi / countBuoiTrenTuan) * 7 + (soLuongBuoi % countBuoiTrenTuan == 0 ? 0 : 7));
            if (end < DateOnly.FromDateTime(DateTime.Now))
                return false;
            return true;

        }

        public static async Task<bool> CheckTrungLichLopHoc(LopHoc lophoc, MyDbContext db)
        {
            var lopHocs = await db.LopHocs
                .Include(l => l.LichHocs)
                .Where(l => l.IdLopHoc != lophoc.IdLopHoc && l.TrangThai == DungChung.trangThaiLopHoc_DangMo)
                .ToListAsync();

            string[] ngayHocMoi = lophoc.SoBuoiTrenTuan.Split(',');

            foreach (var lop in lopHocs)
            {
                if (lop.IdPhong != lophoc.IdPhong)
                    continue;
                string[] ngayHocCu = lop.SoBuoiTrenTuan.Split(',');
                if (ngayHocMoi.Any(ngay => ngayHocCu.Contains(ngay)))
                {
                    if ((lophoc.ThoiGianBatDau < lop.ThoiGianKetThuc) && (lophoc.ThoiGianKetThuc > lop.ThoiGianBatDau) && (lophoc.NgayKhaiGiang <= lop.LichHocs.Max(lh => lh.NgayHoc)))
                        return true; // Trùng lịch học
                }
            }

            return false;

        }

        public async Task<List<GiaoVien>> LayGiaoVienKhongTrungLich(MyDbContext db, TimeOnly thoiGianBatDau, TimeOnly thoiGianKetThuc, string soBuoiTrenTuan)
        {
            List<GiaoVien> giaoViens = new List<GiaoVien>();
            var giaoVienAll = await db.GiaoViens
                .Include(gv => gv.GiaoVienDdayLops)
                .ThenInclude(gdl => gdl.IdLopHocNavigation)
                .Where(gv => gv.TrangThai == true)
                .ToListAsync();

            string[] ngayHocMoi = soBuoiTrenTuan.Split(',');
            var lopHocs = await db.LopHocs
                .Include(l => l.GiaoVienDdayLops)
                .ThenInclude(gdl => gdl.IdGiaoVienNavigation)
                .Include(l => l.LichHocs)
                .Where(l => l.TrangThai == DungChung.trangThaiLopHoc_DangMo)
                .ToListAsync();
            foreach (var gv in giaoVienAll)
            {

                bool isTrungLich = false;

                foreach (var lop in lopHocs)
                {

                    if (lop.GiaoVienDdayLops.All(g => g.IdGiaoVien != gv.GiaoVienId))
                        continue;

                    string[] ngayHocCu = lop.SoBuoiTrenTuan.Split(',');
                    if (ngayHocMoi.Any(ngay => ngayHocCu.Contains(ngay)))
                    {
                        if ((thoiGianBatDau < lop.ThoiGianKetThuc) && (thoiGianKetThuc > lop.ThoiGianBatDau) && (lop.NgayKhaiGiang <= lop.LichHocs.Max(lh => lh.NgayHoc)))
                            isTrungLich = true;
                    }

                }
                if (!isTrungLich)
                {
                    giaoViens.Add(gv);
                }

            }
            return giaoViens;

        }

        public static async Task<byte[]> TaoHoaDonPDF(HoaDonKhoaHoc hoaDon, MyDbContext db)
        {
            // Đường dẫn này đã đúng vì nó không ném FileNoteFound
            string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Front", "Roboto-Regular.ttf");

            if (!System.IO.File.Exists(fontPath))
            {
                throw new FileNotFoundException("Không tìm thấy file font tại: " + fontPath);
            }

            PdfFont vietnameseFont;
            try
            {
                // Tải font
                vietnameseFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
            }
            catch (Exception fontEx)
            {
                throw new InvalidOperationException($"Lỗi khi tải font: {fontPath}. Chi tiết: {fontEx.Message}", fontEx);
            }


            try
            {
                using (var memoryStream = new MemoryStream())
                {
                   
                    var writer = new PdfWriter(memoryStream);

                    using (var pdf = new PdfDocument(writer))
                    {
                        using (var document = new Document(pdf))
                        {
                            document.SetFont(vietnameseFont);

                            document.Add(new Paragraph("Hóa Đơn Khóa Học")
                                .SetFontSize(18));

                            document.Add(new Paragraph($"Mã Hóa Đơn: {hoaDon.IdHoaDon}"));

                            var hocVien = await db.HocViens.FindAsync(hoaDon.HocVienId);
                            var lopHoc = await db.LopHocs.FindAsync(hoaDon.IdLopHoc);

                            document.Add(new Paragraph($"Học Viên: {hocVien?.TenHv ?? "N/A"}"));
                            document.Add(new Paragraph($"Lớp Học: {lopHoc?.TenLopHoc ?? "N/A"}"));

                           
                            string soTienText = hoaDon.TongTien.HasValue ?
                                hoaDon.TongTien.Value + " VNĐ" :
                                "N/A";
                            document.Add(new Paragraph($"Số Tiền: {soTienText}"));

                            string ngayTaoText = hoaDon.Ngaytao.HasValue ?
                                hoaDon.Ngaytao.Value.ToString("dd/MM/yyyy") :
                                "N/A";
                            document.Add(new Paragraph($"Ngày Tạo: {ngayTaoText}"));

                            document.Add(new Paragraph("\nCảm ơn bạn đã đăng ký!"));
                        }
                    }
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("==============================");
                Console.WriteLine("Lỗi khi tạo PDF:");
                Console.WriteLine(ex.ToString()); // In toàn bộ thông tin
                Console.WriteLine("==============================");
                throw;
            }
        }

        public static async Task<bool> CheckTrungLichHoc(int idHocVien, int idLopHoc, MyDbContext db)
        {
            var check = await db.HoaDonKhoaHocs
                .Include(h => h.IdLopHocNavigation)
                .FirstOrDefaultAsync(x => x.HocVienId == idHocVien
                    && x.IdLopHocNavigation != null
                    && x.TrangThai == true
                    && x.IdLopHocNavigation.TrangThai == DungChung.trangThaiLopHoc_DangMo);

            if (check != null)
            {
                LopHoc lopHoc = db.LopHocs.Include(l => l.LichHocs).FirstOrDefault(l => l.IdLopHoc == idLopHoc);
                if (lopHoc != null)
                {
                    // Lấy ra mảng các ngày học trong tuần của lớp học mới
                    string[] ngayHocMoi = lopHoc.SoBuoiTrenTuan.Split(',');

                    // Lấy các lớp học đang mở mà học viên đã đăng ký
                    var lopHocDaDangKy = await db.HoaDonKhoaHocs
                        .Include(h => h.IdLopHocNavigation)
                        .Where(x => x.HocVienId == idHocVien
                            && x.IdLopHocNavigation != null
                            && x.IdLopHocNavigation.TrangThai == DungChung.trangThaiLopHoc_DangMo)
                        .Select(x => x.IdLopHocNavigation)
                        .ToListAsync();

                    // Kiểm tra xem có lớp học nào trùng lịch không
                    foreach (var lop in lopHocDaDangKy)
                    {
                        if (lop.SoBuoiTrenTuan != null)
                        {
                            // Lấy ra mảng các ngày học của lớp đã đăng ký
                            string[] ngayHocCu = lop.SoBuoiTrenTuan.Split(',');

                            // Kiểm tra xem có ngày nào trùng nhau không
                            if (ngayHocMoi.Any(ngay => ngayHocCu.Contains(ngay)))
                            {
                                if ((lopHoc.ThoiGianBatDau < lop.ThoiGianKetThuc) && (lopHoc.ThoiGianKetThuc > lop.ThoiGianBatDau)) return true; // Trùng lịch học

                                return false;
                            }
                        }
                    }
                }
            }
            return false;

        }

        public static async Task<string> HandleImage(IFormFile? imageFile, string folderPath)
        {
            if (imageFile != null && imageFile.Length > 0)
            {

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(folderPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return uniqueFileName;
            }

            return string.Empty;
        }

    }
}