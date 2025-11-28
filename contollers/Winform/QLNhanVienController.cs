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
    public class QLNhanVienController : ControllerBase
    {
        private readonly MyDbContext db;

        public QLNhanVienController(MyDbContext context)
        {
            db = context;
        }

        // Các phương thức khác của controller sẽ được định nghĩa ở đây

        [HttpGet("GetAllNhanVien")]
        public ActionResult<IEnumerable<NhanVien>> GetAllNhanVien()
        {
            var nhanViens = db.NhanViens.Include(q => q.IdQuyenNavigation).Include(tk => tk.User).Where(nv => nv.User.MatKhau != null && nv.User.Mail != null).Select(nv => new
            {
                nv.UserId,
                nv.TenNv,
                nv.Sdt,
                nv.IdQuyenNavigation.TenQuyen,
                nv.User.Mail,
                nv.User.MatKhau,
                nv.IdQuyenNavigation.IdQuyen
            }).ToList();
            return Ok(nhanViens);
        }

        [HttpGet("GetNhanVienById/{id}")]
        public async Task<IActionResult> GetNhanVienById(int id)
        {
            var nhanVien = await db.NhanViens.Include(q => q.IdQuyenNavigation).Select(nv => new
            {
                nv.UserId,
                nv.TenNv,
                nv.Sdt,
                nv.IdQuyenNavigation.TenQuyen,
                nv.IdQuyenNavigation.IdQuyen
            }).FirstOrDefaultAsync(nv => nv.UserId == id);
            if (nhanVien == null)
            {
                return NotFound();
            }
            return Ok(nhanVien);
        }

        [HttpGet("GetAllQuyen")]
        public async Task<IActionResult> GetAllQuyen()
        {
            var quyenList = await db.Quyens.Select(q => new
            {
                q.IdQuyen,
                q.TenQuyen
            }).ToListAsync();
            return Ok(quyenList);
        }

        [HttpPost("CreateNhanVien")]
        public async Task<IActionResult> CreateNhanVien([FromBody] AddNhanVien addNhanVien)
        {
            if (addNhanVien == null || string.IsNullOrEmpty(addNhanVien.Mail) || string.IsNullOrEmpty(addNhanVien.MatKhau))
            {
                return BadRequest(new { message = "Thông tin nhân viên không hợp lệ." });
            }
            var check = await db.Users.AnyAsync(u => u.Mail == addNhanVien.Mail);
            if (check)
            {
                return BadRequest(new { message = "Email đã tồn tại." });
            }
            var user = new User
            {
                Mail = addNhanVien.Mail,
                MatKhau = addNhanVien.MatKhau,
                IsHocVien = false
            };

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var nhanVien = new NhanVien
            {
                UserId = user.UserId,
                IdQuyen = addNhanVien.IdQuyen,
                TenNv = addNhanVien.TenNv,
                Sdt = addNhanVien.Sdt
            };

            await db.NhanViens.AddAsync(nhanVien);
            await db.SaveChangesAsync();

            return Ok(new { message = "Nhân viên đã được tạo thành công." });
        }

        [HttpPut("UpdateNhanVien/{id}")]
        public async Task<IActionResult> UpdateNhanVien(int id, [FromBody] AddNhanVien updateNhanVien)
        {
            var nhanVien = await db.NhanViens.FirstOrDefaultAsync(x => x.UserId == id);
            if (nhanVien == null) return NotFound(new { message = "Nhân viên không tồn tại." });

            var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == id);
            if (user == null) return NotFound(new { message = "Người dùng không tồn tại." });

            // --- BỔ SUNG LOGIC CHECK TRÙNG EMAIL ---
            if (!string.IsNullOrEmpty(updateNhanVien.Mail) && updateNhanVien.Mail != user.Mail)
            {
                bool isDuplicate = await db.Users.AnyAsync(u => u.Mail == updateNhanVien.Mail && u.UserId != id);
                if (isDuplicate)
                {
                    return BadRequest(new { message = "Email mới đã tồn tại trong hệ thống." });
                }
                user.Mail = updateNhanVien.Mail;
            }
            // ---------------------------------------

            // Cập nhật mật khẩu (nếu có nhập)
            if (!string.IsNullOrEmpty(updateNhanVien.MatKhau))
            {
                user.MatKhau = updateNhanVien.MatKhau;
            }

            db.Users.Update(user);

            nhanVien.TenNv = updateNhanVien.TenNv ?? nhanVien.TenNv;
            nhanVien.Sdt = updateNhanVien.Sdt ?? nhanVien.Sdt;
            nhanVien.IdQuyen = updateNhanVien.IdQuyen ?? nhanVien.IdQuyen;

            db.NhanViens.Update(nhanVien);
            await db.SaveChangesAsync();

            return Ok(new { message = "Nhân viên đã được cập nhật thành công." });
        }

        [HttpDelete("DeleteNhanVien/{id}")]
        public async Task<IActionResult> DeleteNhanVien(int id)
        {
            var nhanVien = await db.NhanViens.FirstOrDefaultAsync(x => x.UserId == id);
            if (nhanVien == null)
            {
                return NotFound(new { message = "Nhân viên không tồn tại." });
            }

            var User = await db.Users.FirstOrDefaultAsync(x => x.UserId == id);
            if (User == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại." });
            }



            User.MatKhau = null;
            User.Mail = null;

            db.Users.Update(User);

            await db.SaveChangesAsync();

            return Ok(new { message = "Nhân viên đã được xóa thành công." });
        }

    }
}