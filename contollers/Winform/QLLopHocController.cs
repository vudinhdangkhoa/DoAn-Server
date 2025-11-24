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
    public class QLLopHocController : ControllerBase
    {
        private readonly MyDbContext db;
        public QLLopHocController(MyDbContext context)
        {
            db = context;
        }

        [HttpGet("GetAllLopHoc")]
        public async Task<IActionResult> GetAllLopHoc()
        {
            // Lấy dữ liệu trước
            var lopHocs = await db.LopHocs.Include(t => t.GiaoVienDdayLops).ThenInclude(g => g.IdGiaoVienNavigation).Select(t => new
            {
                t.IdLopHoc,
                t.IdKhoaHoc,
                t.TenLopHoc,
                t.NgayTao,
                phong = t.IdPhongNavigation.TenPhong,
                giaoVien = t.GiaoVienDdayLops.Select(g => new { tenGV = g.IdGiaoVienNavigation.TenGv }).ToList(),
                t.SoLuongBuoi,
                t.SoBuoiTrenTuan,
                t.SoLuongHv,
                t.SoLuongToiThieu,
                t.SoLuongToiDa,
                t.ThoiGianBatDau,
                t.ThoiGianKetThuc,
                t.NgayKhaiGiang,
                hocCu = t.HocCuThuocLops,
                KhoaHoc = t.IdKhoaHocNavigation.TenKhoaHoc
            }).ToListAsync();

            // Filter và tính toán trên client
            var result = lopHocs
                .Where(w => w.NgayKhaiGiang.HasValue &&
                           w.SoLuongBuoi.HasValue &&
                           Helper.checkKhoaHocDate(w.NgayKhaiGiang.Value, w.SoBuoiTrenTuan, w.SoLuongBuoi.Value))
                .Select(t => new
                {
                    t.IdLopHoc,
                    t.IdKhoaHoc,
                    t.TenLopHoc,
                    t.NgayTao,
                    t.phong,
                    t.giaoVien,
                    t.SoLuongBuoi,
                    t.SoBuoiTrenTuan,
                    t.SoLuongHv,
                    t.SoLuongToiThieu,
                    t.SoLuongToiDa,
                    t.ThoiGianBatDau,
                    t.ThoiGianKetThuc,
                    t.NgayKhaiGiang,
                    dangDienRa = Helper.checkKhoaHocDate(t.NgayKhaiGiang.Value, t.SoBuoiTrenTuan, t.SoLuongBuoi.Value),
                    t.hocCu,
                    t.KhoaHoc
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("GetAllPhongHoc")]
        public async Task<IActionResult> GetAllPhongHoc()
        {
            var phongHocs = await db.PhongHocs.Select(t => new
            {
                t.IdPhong,
                t.TenPhong,
            }).ToListAsync();

            return Ok(phongHocs);
        }

        [HttpGet("GetAllKhoaHoc")]
        public async Task<IActionResult> GetAllKhoaHoc()
        {
            var khoaHocs = await db.KhoaHocs.Select(t => new
            {
                t.IdKhoaHoc,
                t.TenKhoaHoc,
            }).ToListAsync();

            return Ok(khoaHocs);
        }

        [HttpGet("GetAllGiaoVien")]
        public async Task<IActionResult> GetAllGiaoVien()
        {
            var giaoViens = await db.GiaoViens.Select(t => new
            {
                t.GiaoVienId,
                t.TenGv,
            }).ToListAsync();

            return Ok(giaoViens);
        }

        [HttpGet("GetAllHocCu")]
        public async Task<IActionResult> GetAllHocCu()
        {
            var hocCus = await db.HocCus.Select(t => new
            {
                t.IdHocCu,
                t.TenHocCu,
                t.GiaBan,
                t.SoLuong,
                t.DonViTinh,
            }).ToListAsync();

            return Ok(hocCus);
        }

        [HttpGet("GetAllChuyenMon")]
        public async Task<IActionResult> GetAllChuyenMon()
        {
            var chuyenMons = await db.ChuyenMons.Select(t => new
            {
                t.IdChuyenMon,
                t.TenChuyenMon,
            }).ToListAsync();

            return Ok(chuyenMons);
        }

        [HttpGet("GetLopHocById/{idLophoc}")]
        public async Task<IActionResult> GetLopHocById(int idLophoc)
        {
            var lopHoc = await db.LopHocs
                .Where(l => l.IdLopHoc == idLophoc)
                .Include(t => t.IdPhongNavigation)
                .Include(t => t.HocCuThuocLops).ThenInclude(h => h.IdHocCuNavigation)
                .Include(t => t.GiaoVienDdayLops).ThenInclude(g => g.IdGiaoVienNavigation)
                .Select(t => new
                {
                    t.IdLopHoc,
                    t.IdKhoaHoc,
                    t.TenLopHoc,
                    t.NgayTao,
                    IdPhong = t.IdPhongNavigation.IdPhong,
                    giaoVien = t.GiaoVienDdayLops.Select(g => new
                    {
                        TenGv = g.IdGiaoVienNavigation.TenGv,
                        GiaoVienId = g.IdGiaoVienNavigation.GiaoVienId
                    }
                    ).ToList(),
                    t.SoLuongBuoi,
                    t.SoBuoiTrenTuan,
                    t.SoLuongHv,
                    t.SoLuongToiThieu,
                    t.SoLuongToiDa,
                    t.ThoiGianBatDau,
                    t.ThoiGianKetThuc,
                    t.NgayKhaiGiang,
                    hocCu = t.HocCuThuocLops.Select(h => new { h.IdHocCuNavigation.IdHocCu, h.IdHocCuNavigation.TenHocCu, h.IdHocCuNavigation.GiaBan, h.SoLuong, h.IdHocCuNavigation.DonViTinh }).ToList(),
                    KhoaHoc = t.IdKhoaHocNavigation.TenKhoaHoc,
                    hocViens = t.HoaDonKhoaHocs.Select(hv => new
                    {
                        hv.HocVien.IdHocVien,
                        hv.HocVien.TenHv,
                        hv.HocVien.GioiTinh,


                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (lopHoc == null)
            {
                return NotFound(new { message = "Lớp học không tồn tại" });
            }

            return Ok(lopHoc);
        }

        [HttpPost("GetGiaoVienKhongTrunglich")]
        public async Task<IActionResult> GetGiaoVienKhongTrunglich([FromBody] AddLopHoc lophoc)
        {

            var giaoVienKhongTrungLich = await new Helper().LayGiaoVienKhongTrungLich(db, lophoc.ThoiGianBatDau.Value, lophoc.ThoiGianKetThuc.Value, lophoc.SoBuoiTrenTuan);

            return Ok(new
            {
                tenGV = giaoVienKhongTrungLich.Select(gv => gv.TenGv),
                idGV = giaoVienKhongTrungLich.Select(gv => gv.GiaoVienId)
            });
        }

        [HttpPost("CreateLopHoc")]
        public async Task<IActionResult> CreateLopHoc([FromBody] AddLopHoc lopHoc)
        {
            var khoaHoc = await db.KhoaHocs.FirstOrDefaultAsync(k => k.IdKhoaHoc == lopHoc.IdKhoaHoc);
            LopHoc lh = new LopHoc
            {
                TenLopHoc = lopHoc.TenLopHoc,
                IdKhoaHoc = lopHoc.IdKhoaHoc,
                IdPhong = lopHoc.IdPhong,
                NgayKhaiGiang = DateOnly.FromDateTime(lopHoc.NgayKhaiGiang.Value),
                SoLuongBuoi = khoaHoc.SoLuongBuoi,
                SoBuoiTrenTuan = lopHoc.SoBuoiTrenTuan,
                SoLuongToiThieu = lopHoc.SoLuongToiThieu,
                TrangThai = DungChung.trangThaiLopHoc_DangMo,
                SoLuongToiDa = lopHoc.SoLuongToiDa,
                ThoiGianBatDau = lopHoc.ThoiGianBatDau,
                ThoiGianKetThuc = lopHoc.ThoiGianKetThuc,
                NgayTao = DateOnly.FromDateTime(DateTime.Now),
                SoLuongHv = 0
            };
            if (Helper.CheckTrungLichLopHoc(lh, db).Result == true)
            {
                return BadRequest(new { message = "Lịch học bị trùng với lớp học khác" });
            }
            db.LopHocs.Add(lh);
            await db.SaveChangesAsync();

            //thêm giáo viên dạy lớp
            if (lopHoc.GiaoVienId != null)
            {

                foreach (var gvId in lopHoc.GiaoVienId)
                {
                    GiaoVienDdayLop gvdl = new GiaoVienDdayLop
                    {
                        IdGiaoVien = gvId,
                        IdLopHoc = lh.IdLopHoc
                    };
                    db.GiaoVienDdayLops.Add(gvdl);
                }

            }

            // Thêm học cụ vào lớp học
            if (lopHoc.DShocCu != null && lopHoc.DShocCu.Any())
            {
                var hocCuIds = lopHoc.DShocCu.Keys.ToList();

                // ✅ Query tất cả học cụ cần dùng 1 lần duy nhất
                var hocCus = await db.HocCus
                    .Where(h => hocCuIds.Contains(h.IdHocCu))
                    .ToListAsync();

                // Kiểm tra tất cả học cụ tồn tại
                var missingIds = hocCuIds.Except(hocCus.Select(h => h.IdHocCu)).ToList();
                if (missingIds.Any())
                {
                    return BadRequest(new { message = $"Không tìm thấy học cụ với ID: {string.Join(", ", missingIds)}" });
                }

                foreach (var item in lopHoc.DShocCu)
                {
                    var hocCu = hocCus.First(h => h.IdHocCu == item.Key);

                    // Kiểm tra số lượng
                    if (hocCu.SoLuong < item.Value)
                    {
                        return BadRequest(new { message = $"Không đủ số lượng học cụ: {hocCu.TenHocCu} (Còn: {hocCu.SoLuong}, Cần: {item.Value})" });
                    }

                    // Trừ số lượng từ kho
                    hocCu.SoLuong -= item.Value;

                    // Thêm quan hệ
                    db.HocCuThuocLops.Add(new HocCuThuocLop
                    {
                        IdLopHoc = lh.IdLopHoc,
                        IdHocCu = item.Key,
                        SoLuong = item.Value
                    });
                }
            }
            await db.SaveChangesAsync();

            return Ok(new { message = "Thêm lớp học thành công" });

        }

        [HttpPut("UpdateLopHoc/{idLopHoc}")]
        public async Task<IActionResult> UpdateLopHoc(int idLopHoc, [FromBody] AddLopHoc lopHoc)
        {
            var existingLopHoc = await db.LopHocs.FindAsync(idLopHoc);
            if (existingLopHoc == null)
            {
                return NotFound(new { message = "Lớp học không tồn tại" });
            }

            existingLopHoc.TenLopHoc = lopHoc.TenLopHoc;
            existingLopHoc.IdPhong = lopHoc.IdPhong;

            existingLopHoc.NgayKhaiGiang = DateOnly.FromDateTime(lopHoc.NgayKhaiGiang.Value);

            existingLopHoc.SoBuoiTrenTuan = lopHoc.SoBuoiTrenTuan;
            existingLopHoc.SoLuongToiThieu = lopHoc.SoLuongToiThieu;

            existingLopHoc.SoLuongToiDa = lopHoc.SoLuongToiDa;
            existingLopHoc.ThoiGianBatDau = lopHoc.ThoiGianBatDau;
            existingLopHoc.ThoiGianKetThuc = lopHoc.ThoiGianKetThuc;

            // Cập nhật giáo viên dạy lớp
            if (lopHoc.GiaoVienId != null)
            {
                var existingGiaoVienDdayLops = await db.GiaoVienDdayLops.Where(g => g.IdLopHoc == idLopHoc).ToListAsync();
                db.GiaoVienDdayLops.RemoveRange(existingGiaoVienDdayLops);

                foreach (var gvId in lopHoc.GiaoVienId)
                {
                    GiaoVienDdayLop gvdl = new GiaoVienDdayLop
                    {
                        IdGiaoVien = gvId,
                        IdLopHoc = idLopHoc
                    };
                    db.GiaoVienDdayLops.Add(gvdl);
                }
            }

            // Cập nhật học cụ
            if (lopHoc.DShocCu != null)
            {
                // 1. Lấy danh sách học cụ cũ
                var existingHocCuThuocLops = await db.HocCuThuocLops
                    .Where(h => h.IdLopHoc == idLopHoc)
                    .ToListAsync();

                // 2. ✅ TRẢ HỌC CỤ CŨ VỀ KHO (Query 1 lần)
                if (existingHocCuThuocLops.Any())
                {
                    var oldHocCuIds = existingHocCuThuocLops.Select(h => h.IdHocCu).Distinct().ToList();

                    // ✅ Query tất cả học cụ cũ 1 lần duy nhất
                    var oldHocCus = await db.HocCus
                        .Where(h => oldHocCuIds.Contains(h.IdHocCu))
                        .ToListAsync();

                    // Trả học cụ về kho
                    foreach (var hctl in existingHocCuThuocLops)
                    {
                        var hocCu = oldHocCus.FirstOrDefault(h => h.IdHocCu == hctl.IdHocCu);
                        if (hocCu != null)
                        {
                            hocCu.SoLuong += hctl.SoLuong; // ✅ Đã được track, sẽ lưu
                        }
                    }

                    // Xóa quan hệ cũ
                    db.HocCuThuocLops.RemoveRange(existingHocCuThuocLops);
                }

                // 3. ✅ THÊM HỌC CỤ MỚI (Query 1 lần)
                if (lopHoc.DShocCu.Any())
                {
                    var newHocCuIds = lopHoc.DShocCu.Keys.ToList();

                    // ✅ Query tất cả học cụ mới 1 lần duy nhất
                    var newHocCus = await db.HocCus
                        .Where(h => newHocCuIds.Contains(h.IdHocCu))
                        .ToListAsync();

                    // Kiểm tra tất cả học cụ tồn tại
                    var missingIds = newHocCuIds.Except(newHocCus.Select(h => h.IdHocCu)).ToList();
                    if (missingIds.Any())
                    {
                        return BadRequest(new { message = $"Không tìm thấy học cụ với ID: {string.Join(", ", missingIds)}" });
                    }

                    foreach (var item in lopHoc.DShocCu)
                    {
                        var hocCu = newHocCus.First(h => h.IdHocCu == item.Key);

                        // Kiểm tra số lượng
                        if (hocCu.SoLuong < item.Value)
                        {
                            return BadRequest(new { message = $"Không đủ số lượng học cụ: {hocCu.TenHocCu} (Còn: {hocCu.SoLuong}, Cần: {item.Value})" });
                        }

                        // Trừ số lượng từ kho
                        hocCu.SoLuong -= item.Value; // ✅ Đã được track, sẽ lưu

                        // Thêm quan hệ mới
                        db.HocCuThuocLops.Add(new HocCuThuocLop
                        {
                            IdLopHoc = idLopHoc,
                            IdHocCu = item.Key,
                            SoLuong = item.Value
                        });
                    }
                }
            }

            await db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật lớp học thành công" });
        }

    }
}