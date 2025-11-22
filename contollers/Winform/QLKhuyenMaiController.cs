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
    public class QLKhuyenMaiController : ControllerBase
    {
        
        MyDbContext db;

        public QLKhuyenMaiController(MyDbContext context)
        {
            db = context;
        }

        [HttpGet("GetAllKhuyenMai")]
        public async Task<ActionResult> GetAllKhuyenMai()
        {
            try
            {
                var khuyenmais = db.KhuyenMais.Select(t => new
                {
                    t.IdKhuyenMai,
                    t.TenKhuyenMai,
                    t.PhanTramKhuyenMai,
                }).ToList();
                return Ok(
                    khuyenmais
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
        [HttpGet("GetKhoaHocVaHoaCu")]
        public async Task<ActionResult> GetKhoaHocVaHoaCu()
        {
            try
            {
                var khoaHocs = db.KhoaHocs.Select(t => new
                {
                    t.IdKhoaHoc,
                    t.TenKhoaHoc,
                }).ToList();

                var hocCus = db.HocCus.Select(t => new
                {
                    t.IdHocCu,
                    t.TenHocCu,
                }).ToList();

                return Ok(new
                {
                    khoaHocs = khoaHocs,
                    hocCus = hocCus
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddKhuyenMai")]
        public async Task<ActionResult> AddKhuyenMai([FromBody] AddKhuyenMai khuyenMai)
        {
            try
            {
                KhuyenMai newKhuyenMai = new KhuyenMai
                {
                    TenKhuyenMai = khuyenMai.tenKhuyenMai,
                    PhanTramKhuyenMai = khuyenMai.phanTramKhuyenMai,
                };

                db.KhuyenMais.Add(newKhuyenMai);
                await db.SaveChangesAsync();
                return Ok(new { message = "Thêm khuyến mãi thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("ApDungKhuyenMai")]
        public async Task<ActionResult> ApDungKhuyenMai([FromBody] ApdungKhuyenMai apdungKhuyenMai)
        {
            try
            {
                var khuyenMai = await db.KhuyenMais.FirstOrDefaultAsync(km => km.IdKhuyenMai == apdungKhuyenMai.idKhuyenMai);
                if (khuyenMai == null)
                {
                    return NotFound("Khuyến mãi không tồn tại");
                }

                foreach (var idHoaCu in apdungKhuyenMai.lstIdHoaCu)
                {
                    var hoaCu = await db.HocCus.FirstOrDefaultAsync(hc => hc.IdHocCu == idHoaCu);
                    if (hoaCu != null)
                    {
                        CacHocCuKhuyenMai khuyenMaiHoaCu = new CacHocCuKhuyenMai
                        {
                            IdHocCu = hoaCu.IdHocCu,
                            IdKhuyenMai = khuyenMai.IdKhuyenMai,
                            NgayBatDau= DateOnly.FromDateTime( apdungKhuyenMai.ngayBatDau),
                            NgayKetThuc= DateOnly.FromDateTime( apdungKhuyenMai.ngayKetThuc),
                            SoLuong= apdungKhuyenMai.soLuong
                            
                        };
                        db.CacHocCuKhuyenMais.Add(khuyenMaiHoaCu);
                    }
                }
                foreach (var idKhoaHoc in apdungKhuyenMai.lstIdKhoaHoc)
                {
                    var khoaHoc = await db.KhoaHocs.FirstOrDefaultAsync(kh => kh.IdKhoaHoc == idKhoaHoc);
                    if (khoaHoc != null)
                    {
                        CacKhoaHocKhuyenMai khuyenMaiKhoaHoc = new CacKhoaHocKhuyenMai
                        {
                            IdKhoaHoc = khoaHoc.IdKhoaHoc,
                            IdKhuyenMai = khuyenMai.IdKhuyenMai,
                            NgayBatDau= DateOnly.FromDateTime( apdungKhuyenMai.ngayBatDau),
                            NgayKetThuc= DateOnly.FromDateTime( apdungKhuyenMai.ngayKetThuc),
                            SoLuong= apdungKhuyenMai.soLuong
                        };
                        db.CacKhoaHocKhuyenMais.Add(khuyenMaiKhoaHoc);
                    }
                }
                await db.SaveChangesAsync();
                return Ok("Áp dụng khuyến mãi thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updateKhuyenMai/{id}")]
        public async Task<ActionResult> updateKhuyenMai(int id, [FromBody] AddKhuyenMai khuyenMai)
        {
            try
            {
                var existingKhuyenMai = await db.KhuyenMais.FirstOrDefaultAsync(km => km.IdKhuyenMai == id);
                if (existingKhuyenMai == null)
                {
                    return NotFound("Khuyến mãi không tồn tại");
                }

                existingKhuyenMai.TenKhuyenMai = khuyenMai.tenKhuyenMai;
                existingKhuyenMai.PhanTramKhuyenMai = khuyenMai.phanTramKhuyenMai;

                db.KhuyenMais.Update(existingKhuyenMai);
                await db.SaveChangesAsync();
                return Ok(new { message = "Cập nhật khuyến mãi thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetCacApDungKhuyenMai")]
        public async Task<ActionResult> GetCacApDungKhuyenMai()
        {
            try
            {
                var apDungKhuyenMais = db.CacKhoaHocKhuyenMais.Include(km=>km.IdKhuyenMaiNavigation)
                .Include(kh=>kh.IdKhoaHocNavigation)
                .Where(kt=>kt.NgayKetThuc>=DateOnly.FromDateTime(DateTime.Now))
                .Select(t => new
                {
                    t.Id,
                    t.IdKhoaHoc,
                    t.IdKhoaHocNavigation.TenKhoaHoc,
                    t.IdKhuyenMai,
                    t.IdKhuyenMaiNavigation.TenKhuyenMai,
                    t.IdKhuyenMaiNavigation.PhanTramKhuyenMai,
                    t.NgayBatDau,
                    t.NgayKetThuc,
                    t.SoLuong
                }).ToList();

                var hoaCuApDungKhuyenMais = db.CacHocCuKhuyenMais.Include(km=>km.IdKhuyenMaiNavigation)
                .Include(hc=>hc.IdHocCuNavigation)
                .Where(kt=>kt.NgayKetThuc>=DateOnly.FromDateTime(DateTime.Now))
                .Select(t => new
                {
                    t.Id,
                    t.IdHocCu,
                    t.IdHocCuNavigation.TenHocCu,
                    t.IdKhuyenMai,
                    t.IdKhuyenMaiNavigation.TenKhuyenMai,
                    t.IdKhuyenMaiNavigation.PhanTramKhuyenMai,
                    t.NgayBatDau,
                    t.NgayKetThuc,
                    t.SoLuong
                }).ToList();
                return Ok(
                    new
                    {
                        khoaHocApDungKhuyenMais = apDungKhuyenMais,
                        hoaCuApDungKhuyenMais = hoaCuApDungKhuyenMais
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateApDung")]
        public async Task<ActionResult> UpdateApDung([FromBody] UpdateApDung updateApDung)
        {
            try
            {
                if(updateApDung.type=="KhoaHoc")
                {
                    var existingApDung = await db.CacKhoaHocKhuyenMais.FirstOrDefaultAsync(km => km.Id == updateApDung.id);
                    if (existingApDung == null)
                    {
                        return NotFound("Áp dụng khuyến mãi không tồn tại");
                    }

                    existingApDung.NgayKetThuc = DateOnly.FromDateTime(updateApDung.ngayKetThuc);
                    existingApDung.SoLuong = updateApDung.soLuong;

                    db.CacKhoaHocKhuyenMais.Update(existingApDung);
                }
                else if(updateApDung.type=="HoaCu")
                {
                    var existingApDung = await db.CacHocCuKhuyenMais.FirstOrDefaultAsync(km => km.Id == updateApDung.id);
                    if (existingApDung == null)
                    {
                        return NotFound("Áp dụng khuyến mãi không tồn tại");
                    }

                    existingApDung.NgayKetThuc = DateOnly.FromDateTime(updateApDung.ngayKetThuc);
                    existingApDung.SoLuong = updateApDung.soLuong;

                    db.CacHocCuKhuyenMais.Update(existingApDung);
                }
                await db.SaveChangesAsync();
                return Ok(new { message = "Cập nhật áp dụng khuyến mãi thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}