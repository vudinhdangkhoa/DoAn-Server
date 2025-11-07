using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using server.Models;
using server.Services;

namespace server.contollers.XacThuc
{
    [ApiController]
    [Route("api/[controller]")]
    public class XacThucController : ControllerBase
    {
        MyDbContext db;
        private readonly JWT_Services _jwtService;
        private readonly Google_Services _googleService;
        private readonly Mail_Services _mailService;

        public XacThucController(MyDbContext context, JWT_Services jwtService, Google_Services googleService, Mail_Services mailService)
        {
            db = context;
            _jwtService = jwtService;
            _googleService = googleService;
            _mailService = mailService;
        }


        [HttpPost("DangNhap")]
        public async Task<IActionResult> DangNhap([FromBody] LoginDTO loginDto)
        {

            var user = await db.Users.FirstOrDefaultAsync(t => t.Mail.Trim() == loginDto.mail.Trim());
            if (user == null)
            {
                return BadRequest(new { message = "tài khoản không tồn tại" });
            }

            if (user.MatKhau?.Trim() != loginDto.matKhau.Trim())
            {
                Console.WriteLine(user.MatKhau);
                Console.WriteLine(loginDto.matKhau);
                return BadRequest(new { message = "mật khẩu không đúng" });
            }
            if (user.IsHocVien == false)
            {
                return Ok(new { message = "Đăng nhập thành công", status = "staff", UserId = user.UserId });
            }
            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefeshToken = refreshToken;
            user.RefreshTokenExpires = DateTime.Now.AddDays(7);
            user.RefeshTokenCreatedAt = DateTime.UtcNow;
            db.Users.Update(user);
            await db.SaveChangesAsync();
            return Ok(new { message = "Đăng nhập thành công", status = "student", token, refreshToken, UserId = user.UserId });


        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefeshToken([FromBody] refreshTokenDTO refreshTokenDto)
        {
            var refreshToken = refreshTokenDto.refreshToken;
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "Refresh token rỗng" });
            }

            var user = await db.Users.FirstOrDefaultAsync(t => t.RefeshToken == refreshTokenDto.refreshToken);
            if (user == null || user.RefreshTokenExpires < DateTime.Now)
            {
                return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn" });
            }

            var newAccessToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newRefreshTokenExpires = DateTime.Now.AddDays(7);
            user.RefeshToken = newRefreshToken;
            user.RefreshTokenExpires = newRefreshTokenExpires;
            db.Users.Update(user);
            await db.SaveChangesAsync();

            return Ok(new { token = newAccessToken, refreshToken = newRefreshToken });
        }


        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {

            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return Unauthorized(new { message = "Người dùng chưa đăng nhập" });
            }
            var user = await db.Users.FirstOrDefaultAsync(t => t.UserId.ToString() == userId);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }
            user.RefeshToken = null;
            user.RefreshTokenExpires = null;
            db.Users.Update(user);
            await db.SaveChangesAsync();

            return Ok(new { message = "Đăng xuất thành công" });
        }

        [HttpPost("SendOTP")]
        public async Task<IActionResult> SendOTP([FromBody] DangKyDTO loginDto)
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(t => t.Mail == loginDto.mail);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Tài khoản đã tồn tại" });
            }

            if (string.IsNullOrEmpty(loginDto.mail) || string.IsNullOrEmpty(loginDto.matKhau) ||
                string.IsNullOrEmpty(loginDto.tenHV) || string.IsNullOrEmpty(loginDto.sdt) ||
                loginDto.ngaySinh == null)
            {
                return BadRequest(new { message = "Vui lòng điền đầy đủ thông tin" });
            }

            // Lưu thông tin tạm thời (có thể dùng cache hoặc session)
            var otp = _mailService.GenerateOTP(loginDto.mail);
            _mailService.SendEmail(loginDto.mail, otp);

            return Ok(new { message = "OTP đã được gửi tới email của bạn" });
        }

        [HttpPost("DangKy")]
        public async Task<IActionResult> DangKy([FromBody] DangKyDTO loginDto)
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(t => t.Mail == loginDto.mail);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Tài khoản đã tồn tại" });
            }
            if (string.IsNullOrEmpty(loginDto.mail) || string.IsNullOrEmpty(loginDto.matKhau) || string.IsNullOrEmpty(loginDto.tenHV) || string.IsNullOrEmpty(loginDto.sdt) || loginDto.ngaySinh == null)
            {
                return BadRequest(new { message = "Vui lòng điền đầy đủ thông tin" });
            }

            // Xác thực OTP
            if(!_mailService.VerifyOTP(loginDto.mail, loginDto.otp))
            {
                return BadRequest(new { message = "OTP không hợp lệ hoặc đã hết hạn" });
            }

            var newUser = new User
            {
                Mail = loginDto.mail,
                MatKhau = loginDto.matKhau,
                RefeshToken = null,
                RefreshTokenExpires = null,
                RefeshTokenCreatedAt = null,
                IsHocVien = true,

            };

            await db.Users.AddAsync(newUser);
            await db.SaveChangesAsync();
            var hocVien = new PhuHuynh
            {
                TenPh = loginDto.tenHV,
                NgaySinh = loginDto.ngaySinh,
                UserId = newUser.UserId,
                Sdt = loginDto.sdt


            };
            // if (loginDto.avatar != null && loginDto.avatar.Length > 0)
            // {
            //     var filePath = Path.Combine("wwwroot", "avatars", $"{Guid.NewGuid()}_{loginDto.avatar.FileName}");
            //     using (var stream = new FileStream(filePath, FileMode.Create))
            //     {
            //         await loginDto.avatar.CopyToAsync(stream);
            //     }
            //     hocVien.Avatar = filePath;
            // }

            await db.PhuHuynhs.AddAsync(hocVien);

            await db.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công" });
        }


        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> googleLogin([FromBody] Google_loginDTO googleLoginDto)
        {

            var Payload = await _googleService.VerifyGoogleToken(googleLoginDto.idToken);

            if (Payload == null)
            {
                return BadRequest(new { message = "Token không hợp lệ" });
            }

            var newUser = db.Users.FirstOrDefault(u => u.Mail == Payload.Email);
            if (newUser == null)
            {
                newUser = new User
                {
                    Mail = Payload.Email,
                    
                    MatKhau = null,
                    RefeshToken = null,
                    RefreshTokenExpires = null,
                    RefeshTokenCreatedAt = null,
                    IsHocVien = true,

                };

                await db.Users.AddAsync(newUser);
                await db.SaveChangesAsync();
                var token = _jwtService.GenerateToken(newUser);
                var refreshToken = _jwtService.GenerateRefreshToken();
                newUser.RefeshToken = refreshToken;
                newUser.RefreshTokenExpires = DateTime.Now.AddDays(7);
                newUser.RefeshTokenCreatedAt = DateTime.UtcNow;
                db.Users.Update(newUser);
                await db.SaveChangesAsync();

                var hocVien = new PhuHuynh
                {
                    TenPh = Payload.Name,
                    GioiTinh = null,
                    NgaySinh = null,
                    UserId = newUser.UserId,
                    Sdt = null,
                    Avatar = Payload.Picture
                };
                await db.PhuHuynhs.AddAsync(hocVien);
                await db.SaveChangesAsync();

                return Ok(new { message = "Đăng nhập thành công", token, refreshToken, newUser.UserId, status = "new" });

            }
            else
            {
                var token = _jwtService.GenerateToken(newUser);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var existingHocVien = await db.PhuHuynhs.FirstOrDefaultAsync(hv => hv.UserId == newUser.UserId);
                if (existingHocVien.NgaySinh == null || existingHocVien.Sdt == null)
                {
                    return Ok(new { message = "Đăng nhập thành công", token, refreshToken, newUser.UserId, status = "new" });
                }
                newUser.RefeshToken = refreshToken;
                newUser.RefreshTokenExpires = DateTime.Now.AddDays(7);
                newUser.RefeshTokenCreatedAt = DateTime.UtcNow;
                db.Users.Update(newUser);
                await db.SaveChangesAsync();
                return Ok(new { message = "Đăng nhập thành công", token, refreshToken, UserId = newUser.UserId, status = "old" });
            }

        }

        [HttpPost("QuenMatKhau")]
        public async Task<IActionResult> QuenMatKhau([FromBody] LoginDTO mailUser)
        {
            Console.WriteLine(mailUser.mail);
            var user = await db.Users.FirstOrDefaultAsync(t => t.Mail == mailUser.mail);
            if (user == null)
            {
                return BadRequest(new { message = "tài khoản không tồn tại" });
            }

            // Tạo và gửi OTP

            var otp = _mailService.GenerateOTP(mailUser.mail);
            _mailService.SendEmail(mailUser.mail, otp);

            return Ok();
        }

        [HttpPost("XacNhanOTP")]
        public async Task<IActionResult> XacNhanOTP([FromBody] XacNhanOTPDTO xacNhanOTPDto)
        {

            var user = await db.Users.FirstOrDefaultAsync(t => t.Mail == xacNhanOTPDto.mail);
            if (user == null)
            {
                return BadRequest(new { message = "tài khoản không tồn tại" });
            }

            var isValid = _mailService.VerifyOTP(xacNhanOTPDto.mail, xacNhanOTPDto.otp);
            if (!isValid)
            {
                return BadRequest(new { message = "OTP không hợp lệ hoặc đã hết hạn" });
            }

            return Ok(new { message = "Xác nhận OTP thành công", userId = user.UserId });
        }

        [HttpPost("DoiMatKhau/{UserId}")]
        public async Task<IActionResult> DoiMatKhau(int UserId, LoginDTO MatKhauMoi)
        {

            var user = await db.Users.FirstOrDefaultAsync(t => t.UserId == UserId);
            if (user == null)
            {
                return BadRequest(new { message = "tài khoản không tồn tại" });
            }

            user.MatKhau = MatKhauMoi.matKhau;
            db.Users.Update(user);
            await db.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }
    }
}

