using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using server.Models;

namespace server.contollers.LichSuThanhToan
{
    [ApiController]
    [Route("api/[controller]")]
    public class LichSuThanhToanController : ControllerBase
    {

        private readonly MyDbContext db;

        public LichSuThanhToanController(MyDbContext context)
        {
            db = context;
        }

        [HttpGet("GetLichSuThanhToan/{idPhuHuynh}")]
        public async Task<IActionResult> GetLichSuThanhToan(int idPhuHuynh)
        {

            var phuhuynh = db.PhuHuynhs.Where(t => t.UserId == idPhuHuynh).Select(

                ph => new
                {
                    ph.UserId,
                    ph.TenPh,
                    hocVien = db.HocViens.Where(hv => hv.IdPhuHuynh == ph.UserId).Select(
                        hv => new
                        {
                            hv.IdHocVien,
                            hv.TenHv,
                            avatar = hv.Avartar.StartsWith("http") ? hv.Avartar : $"/image/imageHocVien/{hv.Avartar}",
                            hoaDonKhoaHoc = db.HoaDonKhoaHocs.Where(hd => hd.HocVienId == hv.IdHocVien && hd.TrangThai == true).Select(
                                hd => new
                                {
                                    hd.IdHoaDon,
                                    hd.Ngaytao,
                                    hd.TongTien,

                                    lopHoc = db.LopHocs.Where(lh => lh.IdLopHoc == hd.IdLopHoc).Select(
                                        lh => new
                                        {
                                            lh.IdLopHoc,
                                            lh.TenLopHoc,
                                            lh.ThoiGianBatDau,
                                            lh.ThoiGianKetThuc
                                        }
                                    ).FirstOrDefault()
                                }
                            ).ToList()
                        }).ToList()
                }
            ).ToList();
            if (phuhuynh == null)
            {
                return NotFound("Phu huynh not found");
            }
            return Ok(phuhuynh);

        }

    }
}