using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.contollers.TrangChu.DTO;
using server.Models;

namespace server.contollers.TrangChu
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrangChuController : ControllerBase
    {
        MyDbContext db;

        public TrangChuController(MyDbContext context)
        {
            db = context;
        }


        [Authorize]
        [HttpGet("GetThongTinUser/{userId}")]
        public async Task<IActionResult> GetThongTinUser(int userId)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            var thongTinHocVien = await db.PhuHuynhs.FirstOrDefaultAsync(t => t.UserId == userId);
            if (thongTinHocVien == null)
            {
                return NotFound(new { message = "Thông tin học viên không tồn tại" });
            }

            var result = new
            {
                user.UserId,
                user.Mail,
                thongTinHocVien.TenPh,
                thongTinHocVien.NgaySinh,
                thongTinHocVien.Sdt,

                avatar = string.IsNullOrEmpty(thongTinHocVien.Avatar)
                ? null
                : (thongTinHocVien.Avatar.StartsWith("http") ? thongTinHocVien.Avatar : $"/image/imagePhuhuynh/{thongTinHocVien.Avatar}")
            };

            return Ok(result);
        }



        [HttpGet("GetAllLoaiKhoaHoc")]
        public async Task<IActionResult> GetAllLoaiKhoaHoc()
        {
            var loaiKhoaHocs = await db.ChuyenMons.Select(c => new
            {
                tenLoai = c.TenChuyenMon,
                id = c.IdChuyenMon,
                moTa = c.MoTa,
                hinhAnh = $"/image/" + c.HinhAnh,
            }).ToListAsync();
            return Ok(loaiKhoaHocs);
        }


        [HttpGet("GetDetailLoaiKhoaHoc/{idChuyenMon}")]
        public async Task<IActionResult> GetDetailLoaiKhoaHoc(int idChuyenMon)
        {
            var chuyenMon = await db.ChuyenMons.FindAsync(idChuyenMon);
            if (chuyenMon == null)
            {
                return NotFound(new { message = "Chuyên môn không tồn tại" });
            }

            var result = new
            {
                chuyenMon.TenChuyenMon,
                chuyenMon.IdChuyenMon,
                chuyenMon.MoTa,
                hinhAnh = $"/image/" + chuyenMon.HinhAnh,
            };

            return Ok(result);
        }

        [HttpGet("GetAllTeacher")]
        public async Task<IActionResult> GetAllTeacher()
        {

            var teachers = await db.GiaoViens.Where(t => t.TrangThai == true).Select(g => new
            {
                id = g.GiaoVienId,
                tenGV = g.TenGv,
                soNamKinhNghiem = g.SoNamKinhNghiem,

                avatar = string.IsNullOrEmpty(g.Avatar) ? null : $"/image/teacher/" + g.Avatar,
            }).ToListAsync();

            return Ok(teachers);

        }


        [HttpGet("GetAllKhoaHocOfLoaiKhoaHoc/{idChuyenMon}")]
        public async Task<IActionResult> GetAllKhoaHocOfLoaiKhoaHoc(int idChuyenMon)
        {
            var khoaHocs = await db.KhoaHocs
            .Where(kh => kh.IdChuyenMon == idChuyenMon)
            .Select(kh => new
            {
                id = kh.IdKhoaHoc,
                tenKH = kh.TenKhoaHoc,
                hocPhi = kh.HocPhi,
                giamGia = (kh.HocPhi * db.CacKhoaHocKhuyenMais
                    .Include(ckh => ckh.IdKhuyenMaiNavigation)
                    .Where(ckh => ckh.IdKhoaHoc == kh.IdKhoaHoc
                        && ckh.NgayBatDau <= DateOnly.FromDateTime(DateTime.Now)
                        && ckh.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Now)
                        && ckh.SoLuong > 0
                        && ckh.IdKhuyenMaiNavigation != null)
                    .Select(ckh => (double?)(ckh.IdKhuyenMaiNavigation.PhanTramKhuyenMai))
                    .Max() ?? 0),
                thoiGianHoc = kh.SoLuongBuoi,
                moTa = kh.MoTa,
                mucTieu = kh.MucTieu,
                hinhAnh = $"/image/imageKhoaHoc/" + kh.HinhAnh,
            }).ToListAsync();

            return Ok(khoaHocs);
        }


        [HttpGet("GetDetailKhoaHoc/{idKhoaHoc}")]
        public async Task<IActionResult> GetDetailKhoaHoc(int idKhoaHoc)
        {
            var khoaHoc = await db.KhoaHocs.Include(kh => kh.IdChuyenMonNavigation).Include(kh => kh.LopHocs).ThenInclude(lh => lh.PhanHois).ThenInclude(hv=>hv.IdHocVienNavigation).FirstOrDefaultAsync(kh => kh.IdKhoaHoc == idKhoaHoc);

            if (khoaHoc == null)
            {
                return NotFound(new { message = "Khóa học không tồn tại" });
            }

            var result = new
            {
                khoaHoc.IdKhoaHoc,
                khoaHoc.TenKhoaHoc,
                khoaHoc.HocPhi,
                giamGia = (khoaHoc.HocPhi * db.CacKhoaHocKhuyenMais
                    .Include(ckh => ckh.IdKhuyenMaiNavigation)
                    .Where(ckh => ckh.IdKhoaHoc == khoaHoc.IdKhoaHoc
                        && ckh.NgayBatDau <= DateOnly.FromDateTime(DateTime.Now)
                        && ckh.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Now)
                        && ckh.SoLuong > 0
                        && ckh.IdKhuyenMaiNavigation != null)
                    .Select(ckh => (double?)(ckh.IdKhuyenMaiNavigation.PhanTramKhuyenMai))
                    .Max() ?? 0),
                khoaHoc.SoLuongBuoi,
                khoaHoc.MoTa,
                khoaHoc.MucTieu,
                hinhAnh = $"/image/imageKhoaHoc/" + khoaHoc.HinhAnh,
                khoaHoc.LoTrinh,
                chuyenMon = new
                {
                    khoaHoc.IdChuyenMonNavigation.IdChuyenMon,
                    khoaHoc.IdChuyenMonNavigation.TenChuyenMon,
                },
                phanHoi = khoaHoc.LopHocs.SelectMany(ph=>ph.PhanHois).Select(ph=>new
                {
                    ph.IdPhanHoi,
                    ph.NoiDung,
                    ph.SoSao,
                    tenHocVien= ph.IdHocVienNavigation.TenHv,
                    avatar= string.IsNullOrEmpty( ph.IdHocVienNavigation.Avartar) ? null : ( ph.IdHocVienNavigation.Avartar.StartsWith("http") ?  ph.IdHocVienNavigation.Avartar : $"/image/imageHocVien/{ ph.IdHocVienNavigation.Avartar}"),
                }).ToList(),
            };

            return Ok(result);
        }

        [HttpGet("GetAllLopHoc/{idKhoaHoc}")]
        public async Task<IActionResult> GetAllLopHoc(int idKhoaHoc)
        {
            var lopHocs = await db.LopHocs.Where(lh => lh.IdKhoaHoc == idKhoaHoc)
            .Include(lh => lh.GiaoVienDdayLops).ThenInclude(gdl => gdl.IdGiaoVienNavigation)
            .Select(lh => new
            {
                id = lh.IdLopHoc,
                tenLop = lh.TenLopHoc,
                soLuongHocVien = lh.SoLuongHv,
                soLuongToiDa = lh.SoLuongToiDa,
                thoiGianBatDau = lh.ThoiGianBatDau,
                thoiGianKetThuc = lh.ThoiGianKetThuc,
                giaoVien = lh.GiaoVienDdayLops.Select(gdl => new { tenGV = gdl.IdGiaoVienNavigation.TenGv }).ToList(),
                ngayKhaiGiang = lh.NgayKhaiGiang,
                soBuoiTrenTuan = lh.SoBuoiTrenTuan,


            }).Where(t => t.ngayKhaiGiang > DateOnly.FromDateTime(DateTime.Now)).ToListAsync();

            return Ok(lopHocs);
        }

        [HttpGet("GetAllHocVienCuaPhuHuynh/{phuHuynhId}")]
        public async Task<IActionResult> GetAllHocVienCuaPhuHuynh(int phuHuynhId)
        {
            var hocViens = await db.HocViens.Where(hv => hv.IdPhuHuynh == phuHuynhId && hv.LaPhuHuynh == false).Select(hv => new
            {
                id = hv.IdHocVien,
                tenHV = hv.TenHv,
                ngaySinh = hv.NgaySinh,
                gioiTinh = hv.GioiTinh,
                avatar = string.IsNullOrEmpty(hv.Avartar) ? null : $"{Request.Scheme}://{Request.Host}/image/student/" + hv.Avartar,
            }).ToListAsync();

            return Ok(hocViens);
        }

        [HttpPost("DangKyLopHoc")]
        public async Task<IActionResult> DangKyLopHoc([FromBody] DangKyLopHoc dangKyDto)
        {
            var checkExistHocVien = await db.HocViens.FindAsync(dangKyDto.HocVienId);
            if (checkExistHocVien == null)
            {
                return NotFound(new { message = "Học viên không tồn tại" });
            }

            bool checkTrungLich = await Helper.CheckTrungLichHoc(dangKyDto.HocVienId.Value, dangKyDto.IdLopHoc.Value, db);
            if (checkTrungLich)
            {
                return BadRequest(new { message = "Lịch học bị trùng với lớp học khác" });
            }



            return Ok(new { message = "Đăng ký lớp học thành công" });
        }

        [HttpGet("TimKiemKhoaHoc")]
        public async Task<IActionResult> TimKiemKhoaHoc([FromQuery] string searchTerm)
        {
            var khoaHocs = await db.KhoaHocs.Include(k => k.IdChuyenMonNavigation)
                .Where(kh => kh.TenKhoaHoc.Contains(searchTerm) || kh.IdChuyenMonNavigation.TenChuyenMon.Contains(searchTerm))
                .Select(kh => new
                {
                    id = kh.IdKhoaHoc,
                    tenKH = kh.TenKhoaHoc,
                    hocPhi = kh.HocPhi,
                    giamGia = (kh.HocPhi * db.CacKhoaHocKhuyenMais
                        .Include(ckh => ckh.IdKhuyenMaiNavigation)
                        .Where(ckh => ckh.IdKhoaHoc == kh.IdKhoaHoc
                            && ckh.NgayBatDau <= DateOnly.FromDateTime(DateTime.Now)
                            && ckh.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Now)
                            && ckh.SoLuong > 0
                            && ckh.IdKhuyenMaiNavigation != null)
                        .Select(ckh => (double?)(ckh.IdKhuyenMaiNavigation.PhanTramKhuyenMai))
                        .Max() ?? 0),
                    thoiGianHoc = kh.SoLuongBuoi,
                    moTa = kh.MoTa,
                    mucTieu = kh.MucTieu,
                    hinhAnh = $"/image/imageKhoaHoc/" + kh.HinhAnh,
                }).ToListAsync();

            return Ok(khoaHocs);
        }
    }
}