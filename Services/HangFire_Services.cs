using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using server.Models;
namespace server.Services
{
    public class HangFire_Services
    {

        private readonly MyDbContext _dbContext;
        private readonly Mail_Services mail_Services;
        private readonly ILogger<HangFire_Services> _logger;
        public HangFire_Services(MyDbContext dbContext, Mail_Services mailServices, ILogger<HangFire_Services> logger)
        {
            _dbContext = dbContext;
            mail_Services = mailServices;
            _logger = logger;
        }

        public async Task CheckAndUpdateClassStatus()
        {
            _logger.LogInformation("[Hangfire Job] Bắt đầu tác vụ kiểm tra trạng thái lớp học...");
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Tìm các lớp học cần cập nhật 
            var classesToUpdate = await _dbContext.LopHocs
                .Include(lh => lh.LichHocs)
                .Include(lh => lh.HoaDonKhoaHocs)
                    .ThenInclude(hd => hd.HocVien)
                        .ThenInclude(hv => hv.IdPhuHuynhNavigation)
                            .ThenInclude(ph => ph.User)
                .Where(lh =>
                    lh.TrangThai == DungChung.trangThaiLopHoc_DangMo &&
                    lh.LichHocs.Any() &&
                    lh.LichHocs.Max(lich => lich.NgayHoc) < today
                )
                .ToListAsync();

            if (!classesToUpdate.Any())
            {
                _logger.LogInformation("[Hangfire Job] Không có lớp học nào cần cập nhật trạng thái.");
                return;
            }

            _logger.LogInformation($"[Hangfire Job] Phát hiện {classesToUpdate.Count} lớp học cần cập nhật trạng thái.");

            // Duyệt qua các lớp và xử lý
            foreach (var lopHoc in classesToUpdate)
            {
                // Cập nhật trạng thái lớp học
                lopHoc.TrangThai = DungChung.trangThaiLopHoc_DaKetThuc;
                _logger.LogInformation($"[Hangfire Job] Cập nhật lớp: {lopHoc.TenLopHoc} (ID: {lopHoc.IdLopHoc}) thành 'Đã kết thúc'.");

                if (!lopHoc.HoaDonKhoaHocs.Any())
                {
                    _logger.LogWarning($"[Hangfire Job] Lớp {lopHoc.TenLopHoc} đã kết thúc nhưng không tìm thấy hóa đơn/học viên nào.");
                    continue;
                }

                // Gửi email thông báo riêng lẻ cho từng học viên qua phụ huynh

                foreach (var hoaDon in lopHoc.HoaDonKhoaHocs)
                {
                    var hocVien = hoaDon.HocVien;
                    if (hocVien == null)
                    {
                        _logger.LogWarning($"[Hangfire Job] Hóa đơn ID {hoaDon.IdHoaDon} không có thông tin học viên.");
                        continue;
                    }

                    var phuHuynh = hocVien.IdPhuHuynhNavigation;
                    var userPhuHuynh = phuHuynh?.User;

                    if (phuHuynh != null && userPhuHuynh != null && !string.IsNullOrEmpty(userPhuHuynh.Mail))
                    {

                        try
                        {
                            var emailPhuHuynh = userPhuHuynh.Mail;
                            var tenHocVien = hocVien.TenHv;
                            var tenPhuHuynh = phuHuynh.TenPh;

                            var subject = $"Thông báo hoàn thành lớp học: {lopHoc.TenLopHoc} (cho học viên {tenHocVien})";
                            var body = $@"
                        <html>
                        <body>
                            <p>Kính gửi Quý Phụ huynh {tenPhuHuynh},</p>
                            <p>Trung tâm Mỹ thuật Sáng tạo xin thông báo lớp học <strong>{lopHoc.TenLopHoc}</strong> mà con của Quý vị là <strong>{tenHocVien}</strong> đang theo học đã chính thức hoàn thành.</p>
                            <p>Cảm ơn Quý Phụ huynh và con đã tin tưởng, tham gia khóa học. Chúng tôi hy vọng con đã có những trải nghiệm học tập bổ ích và vui vẻ.</p>
                            <p>Quý Phụ huynh có thể đăng nhập vào hệ thống để xem các khóa học mới và khuyến khích con để lại phản hồi đánh giá về lớp học nhé.</p>
                            <a href='{DungChung.UrlFrontEnd}/feedback/{lopHoc.IdLopHoc}/{hocVien.IdHocVien}'> đánh giá lớp học</a>
                            <p>Trân trọng,</p>
                            <p>Trung tâm Mỹ thuật Sáng tạo.</p>
                        </body>
                        </html>";


                            await mail_Services.SendEmailToUser(emailPhuHuynh, subject, body);

                            _logger.LogInformation($"[Hangfire Job] Đã gửi email kết thúc lớp {lopHoc.TenLopHoc} tới Phụ huynh: {emailPhuHuynh} (cho học viên: {tenHocVien}).");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"[Hangfire Job] Lỗi khi gửi email đến Phụ huynh {userPhuHuynh.Mail} cho học viên {hocVien.TenHv} (Lớp ID {lopHoc.IdLopHoc}).");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"[Hangfire Job] Không thể gửi email: Học viên ID {hocVien.IdHocVien} (thuộc Hóa đơn ID {hoaDon.IdHoaDon}) không có thông tin Phụ huynh hoặc Phụ huynh không có Email.");
                    }
                }
            }

            // Lưu tất cả thay đổi vào CSDL
            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("[Hangfire Job] Đã lưu thành công tất cả thay đổi trạng thái lớp học vào CSDL.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Hangfire Job] Lỗi nghiêm trọng khi lưu thay đổi trạng thái lớp học vào CSDL.");
            }
        }

    }
}