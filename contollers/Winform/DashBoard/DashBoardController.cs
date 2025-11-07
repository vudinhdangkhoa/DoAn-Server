using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.contollers.Winform.DashBoard
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashBoardController : ControllerBase
    {

        private readonly MyDbContext db;
        private readonly JWT_Services _jwtServices;
        private readonly IConfiguration _configuration;
        public DashBoardController(MyDbContext context, JWT_Services jwtServices, IConfiguration configuration)
        {
            db = context;
            _jwtServices = jwtServices;
            _configuration = configuration;
        }


        [HttpGet("GetThongTinDashBoard")]
        public async Task<IActionResult> GetThongTinDashBoard()
        {

            var soLuongKhoaHoc = await db.KhoaHocs.CountAsync();
            var soLuongHocVien = await db.HocViens.CountAsync();
            var soLuongGiaoVien = await db.GiaoViens.CountAsync();
            var doanhThuThang= await db.HoaDonKhoaHocs
                .Where(hd => hd.Ngaytao.Value.Month == DateTime.Now.Month && hd.Ngaytao.Value.Year == DateTime.Now.Year&& hd.TrangThai==true)
                .SumAsync(hd => (decimal?)hd.TongTien) ?? 0;
            return Ok(new
            {
                soLuongKhoaHoc,
                soLuongHocVien,
                soLuongGiaoVien,
                doanhThuThang
            });

        }

        [HttpGet("GetInfoStaff/{idUser}")]
        public async Task<IActionResult> GetInfoStaff(int idUser)
        {

            var staff = await db.NhanViens.FirstOrDefaultAsync(s => s.UserId == idUser);
            if (staff == null)
            {
                return NotFound(new { message = "Nhân viên không tồn tại" });
            }

            var result = new
            {

                staff.UserId,
                staff.TenNv,
                staff.Sdt,
                quyen = db.Quyens.Where(t => t.IdQuyen == staff.IdQuyen).Select(t => t.TenQuyen).FirstOrDefault(),

            };

            return Ok(result);
        }

        [HttpGet("GetSoLuongHocVienMoiTrong6Thang")]
        public async Task<IActionResult> GetSoLuongHocVienMoiTrong6Thang()
        {

            var sixMonthsAgo = DateTime.Now.AddMonths(-5);
            var hocVienMoiTrong6Thang = await db.HocViens
                .Where(hv => hv.NgayTao != null && hv.NgayTao >= new DateOnly(sixMonthsAgo.Year, sixMonthsAgo.Month, 1))
                .GroupBy(hv => new { hv.NgayTao.Value.Year, hv.NgayTao.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(hocVienMoiTrong6Thang);

        }

        [HttpGet("GetLopHocItem")]
        public async Task<IActionResult> GetLopHocItem()
        {

            var ListItem= db.LopHocs.Where(i=>i.NgayKhaiGiang > DateOnly.FromDateTime(DateTime.Now))
            .Include(lh=>lh.GiaoVienDdayLops).ThenInclude(gdl=>gdl.IdGiaoVienNavigation)
            .Select(t => new
            {
                t.IdLopHoc,
                t.TenLopHoc,
                t.SoLuongHv,
                t.SoLuongToiDa,
                giaoVien = t.GiaoVienDdayLops.Select(gdl => gdl.IdGiaoVienNavigation.TenGv).ToList(),
                khoaHoc = db.KhoaHocs.Where(kh => kh.IdKhoaHoc == t.IdKhoaHoc).Select(kh => kh.TenKhoaHoc).FirstOrDefault(),
            }).ToList();

            return Ok(ListItem);

        }
    }
}