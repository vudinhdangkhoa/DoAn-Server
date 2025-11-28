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
                t.TenChuyenMon,
                t.MoTa,
                hinhAnh = t.HinhAnh != null ? $"/image/{t.HinhAnh}" : null
            }).ToListAsync();

            return Ok(chuyenMons);
        }

        [HttpPost("CreateChuyenMon")]
        public async Task<IActionResult> createChuyenMon([FromForm] AddChuyenMon chuyenMon)
        {


            ChuyenMon newChuyenMon = new ChuyenMon
            {
                TenChuyenMon = chuyenMon.tenChuyenMon,
                MoTa = chuyenMon.moTa,

            };

            //Xử lý hình ảnh
            if (chuyenMon.hinhAnh != null)
            {

                var fileName = $"{Guid.NewGuid()}_{Path.GetExtension(chuyenMon.hinhAnh.FileName)}";
                var filePath = Path.Combine("wwwroot/image", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await chuyenMon.hinhAnh.CopyToAsync(stream);
                }
                newChuyenMon.HinhAnh = fileName;

            }

            db.ChuyenMons.Add(newChuyenMon);
            await db.SaveChangesAsync();

            return Ok(new { message = "Thêm chuyên môn thành công" });
        }

        [HttpPut("UpdateChuyenMon/{idChuyenMon}")]
        public async Task<IActionResult> updateChuyenMon(int idChuyenMon, [FromForm] AddChuyenMon chuyenMon)
        {
            var existingChuyenMon = await db.ChuyenMons.FindAsync(idChuyenMon);
            if (existingChuyenMon == null)
            {
                return NotFound(new { message = "Chuyên môn không tồn tại" });
            }

            existingChuyenMon.TenChuyenMon = chuyenMon.tenChuyenMon;
            existingChuyenMon.MoTa = chuyenMon.moTa;
            if (chuyenMon.hinhAnh != null)
            {

                existingChuyenMon.HinhAnh = Helper.HandleImage(chuyenMon.hinhAnh, "wwwroot/image").Result;

            }

            db.ChuyenMons.Update(existingChuyenMon);
            await db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật chuyên môn thành công" });
        }

        [HttpGet("GetAllKhoaHoc")]
        public async Task<IActionResult> GetAllKhoaHoc()
        {
            var khoaHocs = await db.KhoaHocs.Include(cm => cm.IdChuyenMonNavigation).Include(t => t.LopHocs).Select(t => new
            {
                t.IdKhoaHoc,
                t.IdChuyenMon,
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
                hinhAnh = t.HinhAnh != null ? $"/image/imageKhoaHoc/{t.HinhAnh}" : null,
                LopHocs = t.LopHocs // Lấy tất cả trước
            }).ToListAsync();

            // Filter sau khi đã lấy dữ liệu về client
            var result = khoaHocs.Select(t => new
            {
                t.IdKhoaHoc,
                t.IdChuyenMon,
                t.TenKhoaHoc,
                t.MoTa,
                t.NgayTao,
                t.HocPhi,
                t.SoLuongBuoi,
                t.giamGia,
                t.hinhAnh,
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

        [HttpGet("GetKhoaHocById/{id}")]
        public async Task<IActionResult> GetKhoaHocById(int id)
        {
            try
            {
                var khoaHoc = await db.KhoaHocs
                    .Where(k => k.IdKhoaHoc == id)
                    .Select(k => new
                    {
                        IdKhoaHoc = k.IdKhoaHoc,
                        TenKhoaHoc = k.TenKhoaHoc,
                        MoTa = k.MoTa,
                        MucTieu = k.MucTieu,
                        HocPhi = k.HocPhi,
                        SoLuongBuoi = k.SoLuongBuoi,
                        LoTrinh = k.LoTrinh,
                        IdChuyenMon = k.IdChuyenMon, // Quan trọng để binding ComboBox
                        hinhAnh = k.HinhAnh != null ? $"/image/imageKhoaHoc/{k.HinhAnh}" : null,
                    })
                    .FirstOrDefaultAsync();

                if (khoaHoc == null)
                {
                    return NotFound(new { message = "Không tìm thấy khóa học" });
                }

                return Ok(khoaHoc);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("CreateKhoaHoc")]
        public async Task<IActionResult> createKhoaHoc([FromForm] AddKhoaHoc khoaHoc)
        {

            var check = await db.KhoaHocs.FirstOrDefaultAsync(t => t.TenKhoaHoc.Trim().ToLower() == khoaHoc.TenKhoaHoc.Trim().ToLower());
            if (check != null)
            {
                return BadRequest(new { message = "Tên khóa học đã tồn tại" });
            }
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
            var trungTen = await db.KhoaHocs.FirstOrDefaultAsync(t => t.TenKhoaHoc.Trim().ToLower() == khoaHoc.TenKhoaHoc.Trim().ToLower() && t.IdKhoaHoc != idKhoaHoc);
            if (trungTen != null)
            {
                return BadRequest(new { message = "Tên khóa học đã tồn tại" });
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

        [HttpGet("GetNextKhoaHocId")]
        public async Task<IActionResult> GetNextKhoaHocId()
        {
            try
            {
                var maxId = await db.KhoaHocs.MaxAsync(k => (int?)k.IdKhoaHoc) ?? 0;
                var nextId = maxId + 1;
                return Ok(new { nextId = nextId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }



}