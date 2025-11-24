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
    public class QLGiaoVienController : ControllerBase
    {

        MyDbContext db;
        public QLGiaoVienController(MyDbContext context)
        {
            db = context;
        }

        [HttpGet("GetAllTeacher")]
        public async Task<IActionResult> GetAllTeacher()
        {

            var teachers = await db.GiaoViens.Where(t => t.TrangThai == true).Select(g => new
            {
                id = g.GiaoVienId,
                tenGV = g.TenGv,
                soNamKinhNghiem = g.SoNamKinhNghiem,
                sdt = g.Sdt,
                ngaySinh = g.NgaySinh,
                trangThai = g.TrangThai,
                avatar = string.IsNullOrEmpty(g.Avatar) ? null : $"/image/teacher/" + g.Avatar,
            }).ToListAsync();

            return Ok(teachers);

        }

        [HttpPost("AddTeacher")]
        public async Task<IActionResult> AddTeacher([FromForm] AddGiaoVien teacher)
        {

            GiaoVien newTeacher = new GiaoVien
            {
                TenGv = teacher.TenGv,
                NgaySinh = teacher.NgaySinh,
                Sdt = teacher.Sdt,
                SoNamKinhNghiem = teacher.SoNamKinhNghiem,
                TrangThai = true
            };
            if (teacher.Avatar != null && teacher.Avatar.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{System.IO.Path.GetExtension(teacher.Avatar.FileName)}";
                var filePath = System.IO.Path.Combine("wwwroot/image/teacher", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await teacher.Avatar.CopyToAsync(stream);
                }

                newTeacher.Avatar = fileName;
            }
            db.GiaoViens.Add(newTeacher);
            await db.SaveChangesAsync();
            return Ok(new { message = "Thêm giáo viên thành công" });
        }

        [HttpPut("updateTeacher/{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromForm] AddGiaoVien teacher)
        {
            var existingTeacher = await db.GiaoViens.FirstOrDefaultAsync(g => g.GiaoVienId == id);
            if (existingTeacher == null || existingTeacher.TrangThai == false)
            {
                return NotFound(new { message = "Giáo viên không tồn tại" });
            }

            existingTeacher.TenGv = teacher.TenGv;
            existingTeacher.NgaySinh = teacher.NgaySinh;
            existingTeacher.Sdt = teacher.Sdt;
            existingTeacher.SoNamKinhNghiem = teacher.SoNamKinhNghiem;

            if (teacher.Avatar != null && teacher.Avatar.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{System.IO.Path.GetExtension(teacher.Avatar.FileName)}";
                var filePath = System.IO.Path.Combine("wwwroot/image/teacher", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await teacher.Avatar.CopyToAsync(stream);
                }

                existingTeacher.Avatar = fileName;
            }

            await db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật giáo viên thành công" });
        }

        [HttpDelete("deleteTeacher/{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var existingTeacher = await db.GiaoViens.FirstOrDefaultAsync(g => g.GiaoVienId == id);
            if (existingTeacher == null || existingTeacher.TrangThai == false)
            {
                return NotFound(new { message = "Giáo viên không tồn tại" });
            }

            existingTeacher.TrangThai = false;
            await db.SaveChangesAsync();

            return Ok(new { message = "Xóa giáo viên thành công" });
        }

        [HttpGet("GetLichDay/{idGiaoVien}")]
        public async Task<IActionResult> GetLichDay(int idGiaoVien)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Now);

                // BƯỚC 1: Lấy dữ liệu phẳng từ bảng LichHoc
                // Logic: Tìm các Lịch học thuộc Lớp học mà Giáo viên đó được phân công
                var rawSchedule = await db.LichHocs
                    .Include(lh => lh.IdLopHocNavigation)
                    .Include(lh => lh.IdPhongNavigation)
                    .Where(lh =>
                        // Điều kiện 1: Lớp học này phải có giáo viên ID này dạy
                        lh.IdLopHocNavigation.GiaoVienDdayLops.Any(gv => gv.IdGiaoVien == idGiaoVien)
                        // Điều kiện 2: Ngày học phải từ hôm nay trở đi
                        && lh.NgayHoc >= today
                        // Điều kiện 3: Lịch học đang hoạt động (nếu cần)
                        && lh.TrangThai == true
                    )
                    .OrderBy(lh => lh.NgayHoc)
                    .ThenBy(lh => lh.ThoiGianBatDau)
                    .Select(lh => new
                    {
                        NgayHoc = lh.NgayHoc,
                        // Lấy thông tin chi tiết từng buổi
                        ChiTietBuoiHoc = new
                        {
                            IdLopHoc = lh.IdLopHoc,
                            TenLopHoc = lh.IdLopHocNavigation.TenLopHoc,
                            PhongHoc = lh.IdPhongNavigation != null ? lh.IdPhongNavigation.TenPhong : "Chưa xếp phòng",
                            ThoiGianBatDau = lh.ThoiGianBatDau,
                            ThoiGianKetThuc = lh.ThoiGianKetThuc
                        }
                    })
                    .ToListAsync();

                // BƯỚC 2: Group By ở phía Client (Memory) để tạo cấu trúc lồng nhau
                // Lý do: GroupBy phức tạp trong EF Core đôi khi không dịch được sang SQL tối ưu
                var result = rawSchedule
                    .GroupBy(x => x.NgayHoc)
                    .Select(g => new
                    {
                        NgayDay = g.Key, // Đây là ngày dạy (Key của nhóm)
                        DanhSachLop = g.Select(x => x.ChiTietBuoiHoc).ToList() // List các lớp trong ngày đó
                    })
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}