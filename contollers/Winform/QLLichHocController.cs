using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.contollers.Winform
{
    [ApiController]
    [Route("api/[controller]")]
    public class QLLichHocController : ControllerBase
    {
        MyDbContext db;
        public QLLichHocController(MyDbContext context)
        {
            db = context;
        }


        [HttpGet("GetAllKhoaLopLichHoc")]
        public async Task<IActionResult> GetAllKhoaLopLichHoc()
        {
            var result= db.KhoaHocs.Include(lh=>lh.LopHocs).ThenInclude(l=>l.LichHocs).ThenInclude(p=>p.IdPhongNavigation)
                
                .Select(k=> new {
                    idKhoaHoc= k.IdKhoaHoc,
                    tenKhoaHoc= k.TenKhoaHoc,
                    lopHocs= k.LopHocs.Where(tt=>tt.TrangThai==DungChung.trangThaiLopHoc_DangMo).Select(l=> new {
                        idLopHoc= l.IdLopHoc,
                        tenLopHoc= l.TenLopHoc,
                        lichHocs= l.LichHocs.Where(t=>t.TrangThai==true).Select(t=> new {
                            idLichHoc= t.IdLichHoc,
                            ngayHoc= t.NgayHoc,
                            thoiGianBatDau= t.ThoiGianBatDau,
                            thoiGianKetThuc= t.ThoiGianKetThuc,
                            phongHoc= t.IdPhongNavigation==null ? null : new {
                                idPhong= t.IdPhongNavigation.IdPhong,
                                tenPhong= t.IdPhongNavigation.TenPhong
                            }
                        }).ToList()
                    }).ToList()
                }).ToList();

            return Ok(result);
        }

    }
}