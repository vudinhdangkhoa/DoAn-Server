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
    public class QLLopHocController : ControllerBase
    {
        private readonly MyDbContext db;
        public QLLopHocController(MyDbContext context)
        {
            db = context;
        }

        [HttpGet("GetAllLopHoc")]
        public async Task<IActionResult> GetAllLopHoc()
        {
            // Lấy dữ liệu trước
            var lopHocs = await db.LopHocs.Select(t => new
            {
                t.IdLopHoc,
                t.IdKhoaHoc,
                t.TenLopHoc,
                t.NgayTao,
                phong = t.IdPhongNavigation.TenPhong,
                giaoVien = t.GiaoVien.TenGv,
                t.SoLuongBuoi,
                t.SoBuoiTrenTuan,
                t.SoLuongHv,
                t.SoLuongToiThieu,
                t.SoLuongToiDa,
                t.ThoiGianBatDau,
                t.ThoiGianKetThuc,
                t.NgayKhaiGiang,
                hocCu = t.HocCuThuocLops,
                KhoaHoc = t.IdKhoaHocNavigation.TenKhoaHoc
            }).ToListAsync();

            // Filter và tính toán trên client
            var result = lopHocs
                .Where(w => w.NgayKhaiGiang.HasValue &&
                           w.SoLuongBuoi.HasValue &&
                           Helper.checkKhoaHocDate(w.NgayKhaiGiang.Value, w.SoBuoiTrenTuan, w.SoLuongBuoi.Value))
                .Select(t => new
                {
                    t.IdLopHoc,
                    t.IdKhoaHoc,
                    t.TenLopHoc,
                    t.NgayTao,
                    t.phong,
                    t.giaoVien,
                    t.SoLuongBuoi,
                    t.SoBuoiTrenTuan,
                    t.SoLuongHv,
                    t.SoLuongToiThieu,
                    t.SoLuongToiDa,
                    t.ThoiGianBatDau,
                    t.ThoiGianKetThuc,
                    t.NgayKhaiGiang,
                    dangDienRa = Helper.checkKhoaHocDate(t.NgayKhaiGiang.Value, t.SoBuoiTrenTuan, t.SoLuongBuoi.Value),
                    t.hocCu,
                    t.KhoaHoc
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("GetLopHocById/{idLophoc}")]
        public async Task<IActionResult> GetLopHocById(int idLophoc)
        {
            var lopHoc = await db.LopHocs
                .Where(l => l.IdLopHoc == idLophoc)
                .Include(t => t.IdPhongNavigation)
                .Include(t => t.HocCuThuocLops)
                .Select(t => new
                {
                    t.IdLopHoc,
                    t.IdKhoaHoc,
                    t.TenLopHoc,
                    t.NgayTao,
                    phong = t.IdPhongNavigation.TenPhong,
                    giaoVien = t.GiaoVien.TenGv,
                    t.SoLuongBuoi,
                    t.SoBuoiTrenTuan,
                    t.SoLuongHv,
                    t.SoLuongToiThieu,
                    t.SoLuongToiDa,
                    t.ThoiGianBatDau,
                    t.ThoiGianKetThuc,
                    t.NgayKhaiGiang,
                    hocCu = t.HocCuThuocLops,
                    KhoaHoc = t.IdKhoaHocNavigation.TenKhoaHoc,
                    hocViens = t.HoaDonKhoaHocs.Select(hv => new
                    {
                        hv.HocVien.IdHocVien,
                        hv.HocVien.TenHv,
                        hv.HocVien.GioiTinh,


                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (lopHoc == null)
            {
                return NotFound(new { message = "Lớp học không tồn tại" });
            }

            return Ok(lopHoc);
        }

        [HttpGet("GetGiaoVienKhongTrunglich")]
        public async Task<IActionResult> GetGiaoVienKhongTrunglich([FromBody] AddLopHoc lophoc)
        {

            var giaoVienKhongTrungLich = await new Helper().LayGiaoVienKhongTrungLich(db, lophoc.ThoiGianBatDau.Value, lophoc.ThoiGianKetThuc.Value, lophoc.SoBuoiTrenTuan);

            return Ok(new
            {
                tenGV=giaoVienKhongTrungLich.Select(gv=>gv.TenGv),
                idGV=giaoVienKhongTrungLich.Select(gv=>gv.GiaoVienId)
            });
        }

        [HttpPost("CreateLopHoc")]
        public async Task<IActionResult> CreateLopHoc([FromForm] AddLopHoc lopHoc)
        {

            LopHoc lh = new LopHoc
            {
                TenLopHoc = lopHoc.TenLopHoc,
                IdKhoaHoc = lopHoc.IdKhoaHoc,
                IdPhong = lopHoc.IdPhong,
                GiaoVienId = lopHoc.GiaoVienId,
                NgayKhaiGiang = lopHoc.NgayKhaiGiang,
                SoLuongBuoi = lopHoc.SoLuongBuoi,
                SoBuoiTrenTuan = lopHoc.SoBuoiTrenTuan,
                SoLuongToiThieu = lopHoc.SoLuongToiThieu,
                TrangThai = lopHoc.TrangThai,
                SoLuongToiDa = lopHoc.SoLuongToiDa,
                ThoiGianBatDau = lopHoc.ThoiGianBatDau,
                ThoiGianKetThuc = lopHoc.ThoiGianKetThuc,
                NgayTao = DateOnly.FromDateTime(DateTime.Now),
                SoLuongHv = 0
            };
            if(Helper.CheckTrungLichLopHoc(lh, db).Result == true)
            {
                return BadRequest(new { message = "Lịch học bị trùng với lớp học khác" });
            }
            db.LopHocs.Add(lh);
            await db.SaveChangesAsync();
            // Thêm học cụ vào lớp học
            if (lopHoc.DShocCu != null)
            {
                foreach (var item in lopHoc.DShocCu)
                {
                    HocCuThuocLop hctl = new HocCuThuocLop
                    {
                        IdLopHoc = lh.IdLopHoc,
                        IdHocCu = item.Key,
                        SoLuong = item.Value
                    };
                    db.HocCuThuocLops.Add(hctl);
                }
            }
            await db.SaveChangesAsync();

            return Ok(new { message = "Thêm lớp học thành công" });

        }

        [HttpPut("UpdateLopHoc/{idLopHoc}")]
        public async Task<IActionResult> UpdateLopHoc(int idLopHoc, [FromForm] AddLopHoc lopHoc)
        {
            var existingLopHoc = await db.LopHocs.FindAsync(idLopHoc);
            if (existingLopHoc == null)
            {
                return NotFound(new { message = "Lớp học không tồn tại" });
            }

            existingLopHoc.TenLopHoc = lopHoc.TenLopHoc;
            existingLopHoc.IdPhong = lopHoc.IdPhong;
            existingLopHoc.GiaoVienId = lopHoc.GiaoVienId;
            existingLopHoc.NgayKhaiGiang = lopHoc.NgayKhaiGiang;
            existingLopHoc.SoLuongBuoi = lopHoc.SoLuongBuoi;
            existingLopHoc.SoBuoiTrenTuan = lopHoc.SoBuoiTrenTuan;
            existingLopHoc.SoLuongToiThieu = lopHoc.SoLuongToiThieu;
            existingLopHoc.TrangThai = lopHoc.TrangThai;
            existingLopHoc.SoLuongToiDa = lopHoc.SoLuongToiDa;
            existingLopHoc.ThoiGianBatDau = lopHoc.ThoiGianBatDau;
            existingLopHoc.ThoiGianKetThuc = lopHoc.ThoiGianKetThuc;

            // Cập nhật học cụ
            if (lopHoc.DShocCu != null)
            {
                var existingHocCuThuocLops = db.HocCuThuocLops.Where(h => h.IdLopHoc == idLopHoc);
                db.HocCuThuocLops.RemoveRange(existingHocCuThuocLops);

                foreach (var item in lopHoc.DShocCu)
                {
                    HocCuThuocLop hctl = new HocCuThuocLop
                    {
                        IdLopHoc = idLopHoc,
                        IdHocCu = item.Key,
                        SoLuong = item.Value
                    };
                    db.HocCuThuocLops.Add(hctl);
                }
            }

            await db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật lớp học thành công" });
        }

    }
}