using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.contollers.Winform.DTO;
using server.Models;

namespace server.contollers.LichHoc
{
    [ApiController]
    [Route("api/[controller]")]
    public class LichHocController : ControllerBase
    {
        private readonly MyDbContext db;

        public LichHocController(MyDbContext context)
        {
            db = context;
        }

        [HttpGet("GetAllLopHocTaiKhoanLienKet/{idPhuHuynh}")]
        public async Task<IActionResult> GetAllLopHocTaiKhoanLienKet(int idPhuHuynh)
        {
           
            var result = db.HocViens.Where(hv => hv.IdPhuHuynh == idPhuHuynh)
            .Select(
                hv => new
                {
                    hv.TenHv,
                    hv.IdHocVien,
                    avatar = $"/image/imageHocVien/{hv.Avartar}",
                    hoaDon = db.HoaDonKhoaHocs.Where(hd => hd.HocVienId == hv.IdHocVien)
                    .Include(hd => hd.IdLopHocNavigation)
                    .Where(t=>t.IdLopHocNavigation.TrangThai==DungChung.trangThaiLopHoc_DangMo&& t.TrangThai==true)
                    .Select(hd => new
                    {
                        hd.IdHoaDon,
                        hd.IdLopHocNavigation.TenLopHoc,
                        hd.IdLopHocNavigation.IdLopHoc,
                        hd.IdLopHocNavigation.ThoiGianBatDau,
                        hd.IdLopHocNavigation.ThoiGianKetThuc,
                        lichHoc = db.LichHocs.Include(lh => lh.IdPhongNavigation).Where(lh => lh.IdLopHoc == hd.IdLopHocNavigation.IdLopHoc)
                        .Select(
                            lichhoc => new
                            {
                                lichhoc.IdLichHoc,
                                lichhoc.NgayHoc,
                                lichhoc.IdPhongNavigation.TenPhong,
                                lichhoc.IdPhong,
                                lichhoc.TrangThai,

                            }
                        )
                        .ToList()

                    }).ToList()
                }
            ).ToList();
            return Ok(result);

        }
    }
}