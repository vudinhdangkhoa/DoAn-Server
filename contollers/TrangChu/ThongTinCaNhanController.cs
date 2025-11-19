using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using server.contollers.TrangChu.DTO;
using server.contollers.XacThuc.DTO;
using server.Models;
using server.Services;

namespace server.contollers.TrangChu
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThongTinCaNhanController : ControllerBase
    {

        private readonly MyDbContext db;
        private readonly IConfiguration _configuration;
        private readonly JWT_Services _jwtServices;
        public ThongTinCaNhanController(MyDbContext context, IConfiguration configuration, JWT_Services jwtServices)
        {
            db = context;
            _configuration = configuration;
            _jwtServices = jwtServices;
        }

        [HttpGet("GetThongTinCaNhan/{phuHuynhId}")]
        public async Task<IActionResult> GetThongTinCaNhan(int phuHuynhId)
        {
            var userCheck = await db.Users.FirstOrDefaultAsync(u => u.UserId == phuHuynhId);
            var phuHuynh = await db.PhuHuynhs.Include(p => p.HocViens).FirstOrDefaultAsync(p => p.UserId == phuHuynhId);
            if (phuHuynh == null)
            {
                return NotFound(new { message = "Phụ huynh không tồn tại" });
            }

            var result = new
            {
                phuHuynh.UserId,
                phuHuynh.TenPh,
                phuHuynh.GioiTinh,
                phuHuynh.NgaySinh,
                phuHuynh.Sdt,
                hocVien = phuHuynh.HocViens.Where(hv=>hv.LaPhuHuynh==false).Select(hv => new
                {
                    hv.IdHocVien,
                    hv.TenHv,
                    hv.NgaySinh,
                    avatar = hv.Avartar.StartsWith("http") ? hv.Avartar : $"/image/imageHocVien/{hv.Avartar}",

                }),
                avatar = phuHuynh.Avatar.StartsWith("http") ? phuHuynh.Avatar : $"/image/imagePhuhuynh/{phuHuynh.Avatar}"
            };

            return Ok(result);
        }

        [HttpPut("UpdateThongTinCaNhan/{phuHuynhId}")]
        public async Task<IActionResult> UpdateThongTinCaNhan(int phuHuynhId, [FromForm] UpdatePhuHuynh updateDto)
        {

            var checkPH = await db.PhuHuynhs.FirstOrDefaultAsync(p => p.UserId == phuHuynhId);
            if (checkPH == null)
            {
                return NotFound(new { message = "Phụ huynh không tồn tại" });
            }

            checkPH.TenPh = updateDto.TenPh ?? checkPH.TenPh;
            if (!string.IsNullOrEmpty(updateDto.NgaySinh))
            {
                // Thử chuyển đổi chuỗi sang DateOnly
                if (DateOnly.TryParse(updateDto.NgaySinh, out DateOnly parsedDate))
                {
                    checkPH.NgaySinh = parsedDate;
                }
                else
                {
                    // Nếu chuỗi không hợp lệ, trả về lỗi
                    return BadRequest(new { message = "Định dạng ngày sinh không hợp lệ. Vui lòng sử dụng YYYY-MM-DD." });
                }
            }
            checkPH.Sdt = updateDto.Sdt ?? checkPH.Sdt;
            checkPH.GioiTinh = updateDto.GioiTinh ?? checkPH.GioiTinh;
            if (updateDto.avatar != null)
            {

                var folderPath = Path.Combine("wwwroot", "image", "imagePhuhuynh");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(updateDto.avatar.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await updateDto.avatar.CopyToAsync(stream);
                }

                checkPH.Avatar = fileName;

            }
            db.PhuHuynhs.Update(checkPH);
            await db.SaveChangesAsync();

            return Ok();

        }


        [HttpPost("AddTaiKhoanHocVien/{phuHuynhId}")]
        public async Task<IActionResult> AddHocVien(int phuHuynhId, [FromForm] List<AddTaiKhoanHocVien> LsthocVien)
        {
            var phuHuynh = await db.PhuHuynhs.FirstOrDefaultAsync(p => p.UserId == phuHuynhId);
            if (phuHuynh == null)
            {
                return NotFound(new { message = "Phụ huynh không tồn tại" });
            }

            foreach (var hocVienDto in LsthocVien)
            {
                var hocVien = new HocVien
                {
                    TenHv = hocVienDto.TenHv,

                    GioiTinh = hocVienDto.GioiTinh,
                    IdPhuHuynh = phuHuynhId,
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                };
                if (hocVienDto.NgaySinh != null)
                {
                    if (DateOnly.TryParse(hocVienDto.NgaySinh, out DateOnly parsedDate))
                    {
                        hocVien.NgaySinh = parsedDate;
                    }
                    else
                    {
                        // Nếu chuỗi không hợp lệ, trả về lỗi
                        return BadRequest(new { message = "Định dạng ngày sinh không hợp lệ. Vui lòng sử dụng YYYY-MM-DD." });
                    }
                }
                // Xử lý avatar nếu có
                if (hocVienDto.Avatar != null)
                {
                    var folderPath = Path.Combine("wwwroot", "image", "imageHocVien");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(hocVienDto.Avatar.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await hocVienDto.Avatar.CopyToAsync(stream);
                    }

                    hocVien.Avartar = fileName;
                }
                db.HocViens.Add(hocVien);
            }

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("DoiMatKhau/{idPhuHuynh}")]
        public async Task<IActionResult> DoiMatKhau(int idPhuHuynh, [FromBody] Dictionary<string, string> doiMatKhauDto)
        {
            var phuHuynh = await db.PhuHuynhs.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == idPhuHuynh);
            if (phuHuynh == null)
            {
                return NotFound(new { message = "Phụ huynh không tồn tại" });
            }

            var user = phuHuynh.User;
            if (user == null)
            {
                return NotFound(new { message = "Tài khoản người dùng không tồn tại" });
            }

            //nếu đăng nhập bằng google thì cho đổi mật khẩu
            if (user.MatKhau == null)
            {
                user.MatKhau = doiMatKhauDto["MatKhauMoi"];
                db.Users.Update(user);
                await db.SaveChangesAsync();
                return Ok(new { message = "Đổi mật khẩu thành công" });
            }

            // Kiểm tra mật khẩu cũ
            if (user.MatKhau != doiMatKhauDto["MatKhauCu"])
            {
                return BadRequest(new { message = "Mật khẩu cũ không đúng" });
            }

            // Cập nhật mật khẩu mới
            user.MatKhau = doiMatKhauDto["MatKhauMoi"];
            db.Users.Update(user);
            await db.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }

    }
}