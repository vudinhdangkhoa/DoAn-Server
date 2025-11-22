using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.contollers.Feedback.DTO;
using Microsoft.EntityFrameworkCore;

namespace server.contollers.Feedback
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        
        private MyDbContext myDbContext;
        public FeedbackController(MyDbContext context)
        {
            myDbContext = context;
        }

        [HttpGet("GetDetailKhoaHocFeedback/{idLopHoc}")]
        public IActionResult GetDetailKhoaHocFeedback(int idLopHoc)
        {
            
            var result= myDbContext.LopHocs.Include(t=>t.IdKhoaHocNavigation)
            .Include(t=>t.GiaoVienDdayLops).ThenInclude(g=>g.IdGiaoVienNavigation)
                .Where(l=>l.IdLopHoc==idLopHoc)
                .Select(t=> new {
                    t.IdLopHoc,
                    t.TenLopHoc,
                    t.IdKhoaHocNavigation.TenKhoaHoc,
                    t.IdKhoaHocNavigation.MoTa,
                   giaoViens= t.GiaoVienDdayLops.Select(g=> new {
                        g.IdGiaoVienNavigation.TenGv,
                   }).ToList()
                }).FirstOrDefault();

            return Ok(result);

        }

        [HttpPost("SubmitFeedback")]
        public IActionResult SubmitFeedback([FromBody] AddFeedback feedbackDto)
        {

            var checkHoaDon = myDbContext.HoaDonKhoaHocs
                .FirstOrDefault(hd => hd.HocVienId == feedbackDto.IdHocVien && hd.IdLopHoc == feedbackDto.IdLopHoc&& hd.TrangThai==true);
            if (checkHoaDon == null)
            {
                return BadRequest(new { message = "bạn chưa mua khóa học này." });
            }

            var checkDaHoanThanh= myDbContext.HocViens.Include(t=>t.HoaDonKhoaHocs).ThenInclude(h=>h.IdLopHocNavigation).ThenInclude(lh=>lh.LichHocs)
                .FirstOrDefault(hv=>hv.IdHocVien==feedbackDto.IdHocVien)?
                .HoaDonKhoaHocs
                .FirstOrDefault(hd=>hd.IdLopHoc==feedbackDto.IdLopHoc && hd.TrangThai==true && DateOnly.FromDateTime(DateTime.Now) >= hd.IdLopHocNavigation.LichHocs.Max(l=>l.NgayHoc)
                );
                
            if (checkDaHoanThanh == null)
            {
                return BadRequest(new { message = "bạn chưa hoàn thành khóa học này." });
            }
            var feedback = new PhanHoi
            {
                IdHocVien = feedbackDto.IdHocVien,
                IdLopHoc = feedbackDto.IdLopHoc,
                SoSao = feedbackDto.SoSao,
                NoiDung = feedbackDto.NoiDung,
                NgayPh = DateOnly.FromDateTime(DateTime.Now)
            };

            myDbContext.PhanHois.Add(feedback);
            myDbContext.SaveChanges();

            return Ok(new { message = "Feedback submitted successfully." });
        }

    }
}