using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.contollers.Winform.DTO;
using server.Models;

namespace server.contollers.Winform
{
    [ApiController]
    [Route("api/[controller]")]
    public class QLLichHocController : ControllerBase
    {
        MyDbContext db;
        public QLLichHocController(MyDbContext context)
        {
            db = context;
        }


        [HttpGet("GetAllKhoaLopLichHoc")]
        public async Task<IActionResult> GetAllKhoaLopLichHoc()
        {
            var result= db.KhoaHocs.Include(lh=>lh.LopHocs).ThenInclude(l=>l.LichHocs).ThenInclude(p=>p.IdPhongNavigation)
                
                .Select(k=> new {
                    idKhoaHoc= k.IdKhoaHoc,
                    tenKhoaHoc= k.TenKhoaHoc,
                    lopHocs= k.LopHocs.Where(tt=>tt.TrangThai==DungChung.trangThaiLopHoc_DangMo).Select(l=> new {
                        idLopHoc= l.IdLopHoc,
                        tenLopHoc= l.TenLopHoc,
                        giaoVien = db.GiaoVienDdayLops.Where(g => g.IdLopHoc == l.IdLopHoc).Include(gv => gv.IdGiaoVienNavigation).Select(gv => new
                        {
                            idGiaoVien = gv.IdGiaoVienNavigation.GiaoVienId,
                            tenGiaoVien = gv.IdGiaoVienNavigation.TenGv
                        }).ToList(),
                        
                        lichHocs= l.LichHocs.Where(t=>t.TrangThai==true).Select(t=> new {
                            idLichHoc= t.IdLichHoc,
                            ngayHoc= t.NgayHoc,
                            thoiGianBatDau= t.ThoiGianBatDau,
                            thoiGianKetThuc= t.ThoiGianKetThuc,
                            phongHoc= t.IdPhongNavigation==null ? null : new {
                                idPhong= t.IdPhongNavigation.IdPhong,
                                tenPhong= t.IdPhongNavigation.TenPhong
                            }
                        }).ToList()
                    }).ToList()
                }).ToList();

            return Ok(result);
        }


        [HttpGet("GetPhongTrong")]
        public async Task<IActionResult> GetPhongTrong(DateTime ngay, TimeSpan batDau, TimeSpan ketThuc)
        {
            try
            {
                // Convert sang kiểu dữ liệu DateOnly/TimeOnly của .NET 8
                DateOnly dateSearch = DateOnly.FromDateTime(ngay);
                TimeOnly startSearch = TimeOnly.FromTimeSpan(batDau);
                TimeOnly endSearch = TimeOnly.FromTimeSpan(ketThuc);

                // Lấy tất cả phòng học
                var allPhongs = await db.PhongHocs.ToListAsync();

                // Lấy danh sách các phòng ĐANG BẬN vào giờ đó
                var phongBanIds = await db.LichHocs
                    .Where(l => l.TrangThai == true 
                                && l.NgayHoc == dateSearch 
                                && l.IdPhong != null
                                // Logic giao nhau: (StartA < EndB) && (EndA > StartB)
                                && l.ThoiGianBatDau < endSearch 
                                && l.ThoiGianKetThuc > startSearch)
                    .Select(l => l.IdPhong.Value)
                    .Distinct()
                    .ToListAsync();

                // Lọc ra các phòng không nằm trong danh sách bận
                var phongTrongs = allPhongs
                    .Where(p => !phongBanIds.Contains(p.IdPhong))
                    .Select(p => new 
                    {
                        p.IdPhong,
                        p.TenPhong
                    })
                    .ToList();

                return Ok(phongTrongs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // =============================================================
        // PHẦN 2: HÀM HELPER - CHECK TRÙNG LỊCH (Private)
        // =============================================================
        private async Task<string?> CheckConflict(DateOnly ngayHoc, TimeOnly start, TimeOnly end, int? idPhong, int idLopHoc)
        {
            // 1. Kiểm tra Giáo viên: Lớp này có những ai dạy?
            var teacherIdsOfClass = await db.GiaoVienDdayLops
                                     .Where(gd => gd.IdLopHoc == idLopHoc && gd.IdGiaoVien != null)
                                     .Select(gd => gd.IdGiaoVien.Value)
                                     .ToListAsync();

            // 2. Tìm tất cả lịch học khác đang diễn ra cùng khung giờ này
            var conflictingSchedules = await db.LichHocs
                .Include(l => l.IdLopHocNavigation)
                    .ThenInclude(lop => lop.GiaoVienDdayLops)
                        .ThenInclude(gd => gd.IdGiaoVienNavigation)
                .Where(l => l.TrangThai == true 
                            && l.NgayHoc == ngayHoc
                            // Điều kiện giao nhau thời gian
                            && l.ThoiGianBatDau < end 
                            && l.ThoiGianKetThuc > start)
                .ToListAsync();

            foreach (var lich in conflictingSchedules)
            {
                // Check A: Trùng Phòng (Double check để an toàn tuyệt đối)
                if (idPhong.HasValue && lich.IdPhong == idPhong)
                {
                    return $"Phòng {idPhong} đã bị đặt cho lớp {lich.IdLopHocNavigation?.TenLopHoc} ({lich.ThoiGianBatDau}-{lich.ThoiGianKetThuc}).";
                }

                // Check B: Trùng Giáo Viên (Quan trọng)
                // Lấy danh sách GV của cái lớp "kia"
                var otherClassTeachers = lich.IdLopHocNavigation?.GiaoVienDdayLops;
                
                if (otherClassTeachers != null && teacherIdsOfClass.Any())
                {
                    foreach (var tOther in otherClassTeachers)
                    {
                        // Nếu giáo viên của lớp kia nằm trong danh sách giáo viên của lớp này
                        if (tOther.IdGiaoVien.HasValue && teacherIdsOfClass.Contains(tOther.IdGiaoVien.Value))
                        {
                            string tenGV = tOther.IdGiaoVienNavigation?.TenGv ?? "Giáo viên";
                            string tenLopKia = lich.IdLopHocNavigation?.TenLopHoc ?? "Lớp khác";
                            return $"Giáo viên {tenGV} đang bận dạy lớp {tenLopKia} vào khung giờ này.";
                        }
                    }
                }
            }

            return null; // Không có xung đột
        }

        // =============================================================
        // PHẦN 3: API CHÍNH - BÁO NGHỈ VÀ DẠY BÙ
        // =============================================================
        [HttpPut("BaoNghiVaDayBu")]
        public async Task<IActionResult> BaoNghiVaDayBu([FromBody] AddLichHoc req)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. Xử lý lịch cũ (Báo nghỉ)
                var oldLich = await db.LichHocs.FindAsync(req.idLichHoc);
                if (oldLich == null) return NotFound("Lịch học cũ không tồn tại.");

                oldLich.TrangThai = false; // Set False để hủy
                db.LichHocs.Update(oldLich);

                // 2. Xử lý lịch mới (Dạy bù)
                DateOnly ngayMoi = DateOnly.FromDateTime(req.ngayHoc);
                TimeOnly startMoi = TimeOnly.FromTimeSpan(req.thoiGianBatDau);
                TimeOnly endMoi = TimeOnly.FromTimeSpan(req.thoiGianKetThuc);

                // -- GỌI HÀM CHECK TRÙNG --
                string? conflictErr = await CheckConflict(ngayMoi, startMoi, endMoi, req.idPhong, req.idLopHoc);
                if (conflictErr != null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = conflictErr }); // Trả về lỗi cụ thể cho Client
                }

                var newLich = new server.Models.LichHoc
                {
                    IdLopHoc = req.idLopHoc, // ID lớp lấy từ DTO
                    IdPhong = req.idPhong,
                    NgayHoc = ngayMoi,
                    ThoiGianBatDau = startMoi,
                    ThoiGianKetThuc = endMoi,
                    TrangThai = true // Active
                };

                db.LichHocs.Add(newLich);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "Đã chuyển lịch thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        
        }
}