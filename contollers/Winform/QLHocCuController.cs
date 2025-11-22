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
    public class QLHocCuController : ControllerBase
    {
        
        MyDbContext db;

        public QLHocCuController(MyDbContext _context)
        {
            db=_context;
        }

        [HttpGet("GetAllHocCu")]
        public async Task<ActionResult> GetAllHocCu()
        {
            try
            {
                var hocCus = db.HocCus.Include(t=>t.IdLoaiHocCuNavigation).Select(t => new
                {
                    t.IdLoaiHocCu,
                    t.IdLoaiHocCuNavigation.TenLoai,
                    t.IdHocCu,
                    t.TenHocCu,
                    t.GiaBan,
                    t.SoLuong,
                    t.DonViTinh,
                    giamGia = (t.GiaBan * db.CacHocCuKhuyenMais
                    .Include(ckh => ckh.IdKhuyenMaiNavigation)
                    .Where(ckh => ckh.IdHocCu == t.IdHocCu
                        && ckh.NgayBatDau <= DateOnly.FromDateTime(DateTime.Now)
                        && ckh.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Now)
                        && ckh.SoLuong > 0
                        && ckh.IdKhuyenMaiNavigation != null)
                    .Select(ckh => (double?)(ckh.IdKhuyenMaiNavigation.PhanTramKhuyenMai))
                    .Max() ?? 0),
                    
                }).ToList();

                var loaiHocCus = db.LoaiHocCus.Select(l => new
                {
                    l.IdLoaiHocCu,
                    l.TenLoai,
                }).ToList();
                return Ok(
                    new
                    {
                        hocCus = hocCus,
                        loaiHocCus = loaiHocCus
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddHoaCu")]
        public async Task<ActionResult> AddHoaCu(AddHoaCu hocCu)
        {
            try
            {
                HocCu newHocCu = new HocCu()
                {
                    IdLoaiHocCu = hocCu.IdLoaiHocCu,
                    TenHocCu = hocCu.TenHocCu,
                    SoLuong = hocCu.SoLuong,
                    DonViTinh = hocCu.DonViTinh,
                    GiaBan = hocCu.GiaBan,
                    
                };
                db.HocCus.Add(newHocCu);
                db.SaveChanges();
                return Ok("Thêm học cụ thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateHoaCu/{id}")]
        public async Task<ActionResult> UpdateHoaCu(int id,[FromBody] AddHoaCu hocCu)
        {
            try
            {
                var existingHocCu = await db.HocCus.FirstOrDefaultAsync(h => h.IdHocCu == id);
                if (existingHocCu == null)
                {
                    return NotFound("Học cụ không tồn tại");
                }

                
                existingHocCu.TenHocCu = hocCu.TenHocCu;
                existingHocCu.SoLuong = hocCu.SoLuong;
                existingHocCu.DonViTinh = hocCu.DonViTinh;
                existingHocCu.GiaBan = hocCu.GiaBan;

                await db.SaveChangesAsync();
                return Ok("Cập nhật học cụ thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("NhapKhoHocCu")]
        public async Task<ActionResult> NhapKhoHocCu([FromBody] DSNhapKhoHocCu dsNhapKhoHocCu)
        {
            try
            {

                PhieuNhapHang phieuNhapHang = new PhieuNhapHang
                {
                    NgayTao = DateOnly.FromDateTime(DateTime.Now),
                    IdNhaCungCap = dsNhapKhoHocCu.idNhaCungCap,
                    UserId = dsNhapKhoHocCu.userId,
                    TongTien= dsNhapKhoHocCu.danhSachNhapKhoHocCu.Sum(item => item.giaNhap * item.soLuong),
                    
                };
                await db.PhieuNhapHangs.AddAsync(phieuNhapHang);
                await db.SaveChangesAsync();
                foreach (var item in dsNhapKhoHocCu.danhSachNhapKhoHocCu)
                {
                    ChiTietPhieuNhap chiTietPhieuNhap = new ChiTietPhieuNhap
                    {
                        IdPhieuNhapHang = phieuNhapHang.IdPhieuNhapHang,
                        IdHocCu = item.idHocCu,
                        SoLuong = item.soLuong,
                        Gia = item.giaNhap
                    };
                    await db.ChiTietPhieuNhaps.AddAsync(chiTietPhieuNhap);

                    var hocCu = await db.HocCus.FirstOrDefaultAsync(h => h.IdHocCu == item.idHocCu);
                    if (hocCu != null)
                    {
                        hocCu.SoLuong += item.soLuong;
                    }
                }
                await db.SaveChangesAsync();
                return Ok("Nhập kho học cụ thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddLoaiHocCu")]
        public async Task<ActionResult> AddLoaiHocCu([FromBody] AddLoaiHocCu Loai)
        {
            try
            {
                LoaiHocCu newLoaiHocCu = new LoaiHocCu()
                {
                    TenLoai = Loai.tenLoai,
                };
                db.LoaiHocCus.Add(newLoaiHocCu);
                db.SaveChanges();
                return Ok("Thêm loại học cụ thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllNhaCungCap")]
        public async Task<ActionResult> GetAllNhaCungCap()
        {
            try
            {
                var nhaCungCaps = db.NhaCungCaps.Select(ncc => new
                {
                    ncc.IdNhaCungCap,
                    ncc.TenNhaCungCap,
                   
                    ncc.Sdt,
                    
                }).ToList();
                return Ok(nhaCungCaps);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddNhaCungCap")]
        public async Task<ActionResult> AddNhaCungCap([FromBody] AddNhaCungCap ncc)
        {
            try
            {
                NhaCungCap newNhaCungCap = new NhaCungCap()
                {
                    TenNhaCungCap = ncc.tenNhaCungCap,
                 
                    Sdt = ncc.soDienThoai,
                };
                db.NhaCungCaps.Add(newNhaCungCap);
                db.SaveChanges();
                return Ok("Thêm nhà cung cấp thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("BanHoaCu")]
        public IActionResult BanHoaCu([FromBody] BanHoaCu banHoaCu)
        {
            try
            {
                var phuHuynh = db.PhuHuynhs.FirstOrDefault(ph => ph.Sdt == banHoaCu.sdt);
                HoaDonHocCu hoaDon = new HoaDonHocCu
                {
                    Sdt = banHoaCu.sdt,
                    TongTien = banHoaCu.TongTien,
                    GiamGia = banHoaCu.GiamGia,
                    IdNhanVien = banHoaCu.IdNhanVien,
                    IdKhachHang= phuHuynh.UserId,
                    
                };
                db.HoaDonHocCus.Add(hoaDon);
                db.SaveChanges();

                foreach (var item in banHoaCu.dsHoaCu)
                {
                    foreach (var kvp in item)
                    {
                        ChiTietHoaDonHocCu chiTiet = new ChiTietHoaDonHocCu
                        {
                            IdHoaDonHocCu = hoaDon.IdHoaDonHocCu,
                            IdHocCu = kvp.Key,
                            SoLuong = kvp.Value
                        };
                        db.ChiTietHoaDonHocCus.Add(chiTiet);

                        var hocCu = db.HocCus.FirstOrDefault(h => h.IdHocCu == kvp.Key);
                        if (hocCu != null)
                        {
                            hocCu.SoLuong -= kvp.Value;
                        }
                    }
                }
                db.SaveChanges();
                return Ok("Bán học cụ thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}