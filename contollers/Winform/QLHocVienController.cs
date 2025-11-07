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
    public class QLHocVienController : ControllerBase
    {

        private readonly MyDbContext db;
        private readonly JWT_Services _jwtServices;
        private readonly IConfiguration _configuration;
        public QLHocVienController(MyDbContext context, JWT_Services jwtServices, IConfiguration configuration)
        {
            db = context;
            _jwtServices = jwtServices;
            _configuration = configuration;
        }

        [HttpGet("GetAllHocVien")]
        public async Task<IActionResult> GetAllHocVien()
        {
            // Lấy dữ liệu trước, không filter với Helper method
            var hocViens = await db.HocViens.Include(t=>t.IdPhuHuynhNavigation).Select(t => new
            {
                t.TenHv,
                t.NgaySinh,
                phuHuynh = t.IdPhuHuynhNavigation.TenPh,
                t.GioiTinh,
                hoaDonKhoaHocs = t.HoaDonKhoaHocs.Select(hd => new
                {
                    hd.IdHoaDon,
                    hd.IdLopHoc,
                    lopHoc = hd.IdLopHocNavigation,
                    hd.Ngaytao,
                    hd.TongTien
                }).Where(w =>
                    w.lopHoc != null
                    && w.lopHoc.NgayKhaiGiang.HasValue
                    && w.lopHoc.SoLuongBuoi.HasValue
                ).ToList()
            }).ToListAsync();

            // Filter trên client side với Helper method
            var result = hocViens.Select(t => new
            {
                t.TenHv,
                t.NgaySinh,
                t.phuHuynh,
                t.GioiTinh,
                hoaDonKhoaHocs = t.hoaDonKhoaHocs.Where(w =>
                    Helper.checkKhoaHocDate(w.lopHoc.NgayKhaiGiang.Value, w.lopHoc.SoBuoiTrenTuan, w.lopHoc.SoLuongBuoi.Value)
                ).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpPost("AddHocVien")]
        public async Task<IActionResult> AddHocVien([FromBody] AddHocVien hocVien)
        {
            var user = new User
            {
                Mail = hocVien.email,
                IsHocVien = true,
                MatKhau = "123456"

            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var phuHuynh = new PhuHuynh
            {
                TenPh = hocVien.tenPH,
                Sdt = hocVien.sdt,
                NgayTao = DateOnly.FromDateTime(DateTime.Now),
                NgaySinh = hocVien.ngaySinhPH,
                UserId = user.UserId
            };
            db.PhuHuynhs.Add(phuHuynh);
            await db.SaveChangesAsync();

            foreach (var hv in hocVien.DSHocVien)
            {
                var hocVienEntity = new HocVien
                {
                    TenHv = hv.tenHv,
                    NgaySinh = hv.ngaySinh,
                    GioiTinh = hv.gioiTinh,
                    IdPhuHuynh = phuHuynh.UserId
                };
                db.HocViens.Add(hocVienEntity);
            }
            await db.SaveChangesAsync();
            return Ok(new { message = "Thêm học viên thành công" });
        }

    }
}