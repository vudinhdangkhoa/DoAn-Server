using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.contollers.Winform.DTO;
using server.Models;
using server.Services;

namespace server.contollers.Winform
{
    [ApiController]
    [Route("api/[controller]")]
    public class KhoaHocController : ControllerBase
    {
        private readonly MyDbContext db;
        private readonly JWT_Services _jwtServices;
        private readonly IConfiguration _configuration;
        public KhoaHocController(MyDbContext context, JWT_Services jwtServices, IConfiguration configuration)
        {
            db = context;
            _jwtServices = jwtServices;
            _configuration = configuration;
        }

        [HttpGet("GetAllChuyenMon")]
        public async Task<IActionResult> GetAllChuyenMon()
        {
            var chuyenMons = await db.ChuyenMons.Select(t => new
            {
                t.IdChuyenMon,
                t.TenChuyenMon
            }).ToListAsync();

            return Ok(chuyenMons);
        }

        [HttpGet("GetAllKhoaHoc")]
        public async Task<IActionResult> GetAllKhoaHoc()
        {
            var khoaHocs = await db.KhoaHocs.Include(t => t.LopHocs).Select(t => new
            {
                t.IdKhoaHoc,
                t.TenKhoaHoc,
                t.MoTa,
                t.NgayTao,
                t.HocPhi,
                giamGia = (t.HocPhi * db.CacKhoaHocKhuyenMais
                    .Include(ckh => ckh.IdKhuyenMaiNavigation)
                    .Where(ckh => ckh.IdKhoaHoc == t.IdKhoaHoc
                        && ckh.NgayBatDau <= DateOnly.FromDateTime(DateTime.Now)
                        && ckh.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Now)
                        && ckh.SoLuong > 0
                        && ckh.IdKhuyenMaiNavigation != null)
                    .Select(ckh => (double?)(ckh.IdKhuyenMaiNavigation.PhanTramKhuyenMai))
                    .Max() ?? 0),
                t.SoLuongBuoi,
                LopHocs = t.LopHocs // Lấy tất cả trước
            }).ToListAsync();

            // Filter sau khi đã lấy dữ liệu về client
            var result = khoaHocs.Select(t => new
            {
                t.IdKhoaHoc,
                t.TenKhoaHoc,
                t.MoTa,
                t.NgayTao,
                t.HocPhi,
                t.SoLuongBuoi,
                t.giamGia,
                lopHocs = t.LopHocs.Where(l =>
                    l.NgayKhaiGiang.HasValue &&
                    l.SoLuongBuoi.HasValue &&
                    Helper.checkKhoaHocDate(l.NgayKhaiGiang.Value, l.SoBuoiTrenTuan, l.SoLuongBuoi.Value)
                ).Select(s => new
                {
                    s.IdLopHoc,
                    s.TenLopHoc,
                    s.NgayKhaiGiang,
                    s.SoLuongBuoi,
                    s.SoBuoiTrenTuan
                }).ToList()
            }).ToList();

            return Ok(result);
        }


        [HttpPost("CreateKhoaHoc")]
        public async Task<IActionResult> createKhoaHoc([FromForm] AddKhoaHoc khoaHoc)
        {

            KhoaHoc newKhoaHoc = new KhoaHoc
            {
                IdChuyenMon = khoaHoc.IdChuyenMon,
                MoTa = khoaHoc.MoTa,
                MucTieu = khoaHoc.MucTieu,
                HocPhi = khoaHoc.HocPhi,
                SoLuongBuoi = khoaHoc.SoLuongBuoi,
                TenKhoaHoc = khoaHoc.TenKhoaHoc,
                NgayTao = DateOnly.FromDateTime(DateTime.Now),
                LoTrinh = khoaHoc.LoTrinh
            };

            //Xử lý hình ảnh
            if (khoaHoc.HinhAnh != null)
            {

                var fileName = $"{Guid.NewGuid()}_{Path.GetExtension(khoaHoc.HinhAnh.FileName)}";
                var filePath = Path.Combine("wwwroot/image/imageKhoaHoc", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await khoaHoc.HinhAnh.CopyToAsync(stream);
                }
                newKhoaHoc.HinhAnh = fileName;

            }

            db.KhoaHocs.Add(newKhoaHoc);
            await db.SaveChangesAsync();

            return Ok(new { message = "Thêm khóa học thành công" });

        }

        [HttpPut("UpdateKhoaHoc/{idKhoaHoc}")]
        public async Task<IActionResult> updateKhoaHoc(int idKhoaHoc, [FromForm] AddKhoaHoc khoaHoc)
        {
            var existingKhoaHoc = await db.KhoaHocs.FindAsync(idKhoaHoc);
            if (existingKhoaHoc == null)
            {
                return NotFound(new { message = "Khóa học không tồn tại" });
            }

            existingKhoaHoc.MoTa = khoaHoc.MoTa;
            existingKhoaHoc.MucTieu = khoaHoc.MucTieu;
            existingKhoaHoc.HocPhi = khoaHoc.HocPhi;
            existingKhoaHoc.SoLuongBuoi = khoaHoc.SoLuongBuoi;
            existingKhoaHoc.TenKhoaHoc = khoaHoc.TenKhoaHoc;
            
            if (khoaHoc.HinhAnh != null)
            {

                existingKhoaHoc.HinhAnh = Helper.HandleImage(khoaHoc.HinhAnh, "wwwroot/image/imageKhoaHoc").Result;

            }

            db.KhoaHocs.Update(existingKhoaHoc);
            await db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật khóa học thành công" });
        }

    }

    
   
}