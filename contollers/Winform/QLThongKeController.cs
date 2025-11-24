using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.contollers.Winform.DTO;
using Microsoft.EntityFrameworkCore;

namespace server.contollers.Winform
{
    [ApiController]
    [Route("api/[controller]")]
    public class QLThongKeController : ControllerBase
    {
        private readonly MyDbContext _db;

        public QLThongKeController(MyDbContext context)
        {
            _db = context;
        }

        // ============================================================
        // TAB 1: THỐNG KÊ DOANH THU (Đã sửa logic DateOnly)
        // ============================================================
        [HttpGet("DoanhThu")]
        public async Task<IActionResult> GetThongKeDoanhThu([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            try
            {
                DateOnly start = DateOnly.FromDateTime(tuNgay);
                DateOnly end = DateOnly.FromDateTime(denNgay);

                // 1. Hóa đơn Học Phí (Tính Thực thu = Tổng - Giảm)
                var rawHocPhi = await _db.HoaDonKhoaHocs
                    .Where(x => x.Ngaytao >= start && x.Ngaytao <= end)
                    .Select(x => new { 
                        NgayTao = x.Ngaytao, 
                        ThucThu = (x.TongTien ?? 0) - (x.GiamGia ?? 0) 
                    })
                    .ToListAsync();

                // 2. Hóa đơn Họa Cụ (Tính Thực thu = Tổng - Giảm)
                var rawHoaCu = await _db.HoaDonHocCus
                    .Where(x => x.Ngaytao >= start && x.Ngaytao <= end)
                    .Select(x => new { 
                        NgayTao = x.Ngaytao, 
                        ThucThu = (x.TongTien ?? 0) - (x.GiamGia ?? 0) 
                    })
                    .ToListAsync();

                // 3. Phiếu Nhập Hàng (Chi phí đầu vào)
                var rawNhapHang = await _db.PhieuNhapHangs
                    .Where(x => x.NgayTao >= start && x.NgayTao <= end)
                    .Select(x => new { 
                        NgayTao = x.NgayTao, 
                        ChiPhi = x.TongTien ?? 0 
                    })
                    .ToListAsync();


                // 4. GroupBy trên RAM
                var groupHocPhi = rawHocPhi
                    .GroupBy(x => new { x.NgayTao.Value.Month, x.NgayTao.Value.Year })
                    .Select(g => new { g.Key.Month, g.Key.Year, Total = g.Sum(x => x.ThucThu) })
                    .ToList();

                var groupHoaCu = rawHoaCu
                    .GroupBy(x => new { x.NgayTao.Value.Month, x.NgayTao.Value.Year })
                    .Select(g => new { g.Key.Month, g.Key.Year, Total = g.Sum(x => x.ThucThu) })
                    .ToList();

                var groupNhapHang = rawNhapHang
                    .GroupBy(x => new { x.NgayTao.Value.Month, x.NgayTao.Value.Year })
                    .Select(g => new { g.Key.Month, g.Key.Year, Total = g.Sum(x => x.ChiPhi) })
                    .ToList();

                // 5. Tổng hợp
                var resultList = new List<DoanhThuTheoThangDTO>();
                var current = tuNgay;
                
                while (current <= denNgay)
                {
                    var t = current.Month;
                    var y = current.Year;

                    var hp = groupHocPhi.FirstOrDefault(x => x.Month == t && x.Year == y)?.Total ?? 0;
                    var hc = groupHoaCu.FirstOrDefault(x => x.Month == t && x.Year == y)?.Total ?? 0;
                    var nhap = groupNhapHang.FirstOrDefault(x => x.Month == t && x.Year == y)?.Total ?? 0;

                    resultList.Add(new DoanhThuTheoThangDTO
                    {
                        Thang = t,
                        Nam = y,
                        ThangNam = $"{t}/{y}",
                        TienHocPhi = hp,
                        TienHoaCu = hc,
                        TienNhapHang = nhap
                    });

                    current = current.AddMonths(1);
                }

                double tongThu = rawHocPhi.Sum(x => x.ThucThu) + rawHoaCu.Sum(x => x.ThucThu);
                double tongChi = rawNhapHang.Sum(x => x.ChiPhi);

                return Ok(new BaoCaoDoanhThuDTO
                {
                    TongDoanhThuThuan = tongThu,
                    TongChiPhiNhap = tongChi,
                    LoiNhuanRong = tongThu - tongChi,
                    ChiTietTheoThang = resultList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ============================================================
        // TAB 2: THỐNG KÊ HỌA CỤ (Đã sửa logic lấy giá bán)
        // ============================================================
        [HttpGet("HoaCu")]
        public async Task<IActionResult> GetThongKeHoaCu()
        {
            try
            {
                // 1. Top 5 Bán Chạy
                // Sửa logic: Tính doanh thu dựa trên (Số lượng * Giá bán hiện tại của Họa Cụ)
                var topBanChay = await _db.ChiTietHoaDonHocCus
                    .Include(ct => ct.IdHocCuNavigation)
                    .GroupBy(ct => new { ct.IdHocCu, ct.IdHocCuNavigation.TenHocCu, ct.IdHocCuNavigation.GiaBan }) // Group kèm giá bán
                    .Select(g => new TopBanChayDTO
                    {
                        TenHocCu = g.Key.TenHocCu,
                        SoLuongBan = g.Sum(x => x.SoLuong ?? 0),
                        // Lấy giá bán từ bảng cha (HocCu) nhân với số lượng
                        DoanhThu = g.Sum(x => (g.Key.GiaBan ?? 0) * (x.SoLuong ?? 0))
                    })
                    .OrderByDescending(x => x.SoLuongBan)
                    .Take(5)
                    .ToListAsync();

                // 2. Cảnh báo tồn kho (Giữ nguyên)
                var sapHetHang = await _db.HocCus
                    .Where(h => h.SoLuong < 10)
                    .OrderBy(h => h.SoLuong)
                    .Select(h => new CanhBaoTonKhoDTO
                    {
                        TenHocCu = h.TenHocCu,
                        DonViTinh = h.DonViTinh,
                        TonKho = h.SoLuong ?? 0
                    })
                    .Take(10)
                    .ToListAsync();

                return Ok(new BaoCaoHoaCuDTO
                {
                    TopBanChay = topBanChay,
                    SapHetHang = sapHetHang
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ============================================================
        // TAB 3: THỐNG KÊ ĐÀO TẠO (Lớp & Học viên)
        // ============================================================
        [HttpGet("DaoTao")]
        public async Task<IActionResult> GetThongKeDaoTao()
        {
            try
            {
                // 1. Tổng quan
                var tongHV = await _db.LopHocs.SumAsync(l => l.SoLuongHv ?? 0);
                var tongLop = await _db.LopHocs.CountAsync(l => l.TrangThai == DungChung.trangThaiLopHoc_DangMo);

                // 2. Phân bố học viên theo Khóa
                var phanBo = await _db.LopHocs
                    .Include(l => l.IdKhoaHocNavigation)
                    .GroupBy(l => l.IdKhoaHocNavigation.TenKhoaHoc)
                    .Select(g => new HocVienTheoKhoaDTO
                    {
                        TenKhoaHoc = g.Key,
                        SoLuongHocVien = g.Sum(x => x.SoLuongHv ?? 0)
                    })
                    .ToListAsync();

                // 3. Tỷ lệ lấp đầy (SỬA LẠI ĐOẠN NÀY)
                // Bước 3.1: Lấy dữ liệu thô về trước (Chưa sort)
                var rawTyLe = await _db.LopHocs
                    .Where(l => l.TrangThai == DungChung.trangThaiLopHoc_DangMo)
                    .Select(l => new TyLeLapDayDTO
                    {
                        TenLop = l.TenLopHoc,
                        SiSoHienTai = l.SoLuongHv ?? 0,
                        SiSoToiDa = l.SoLuongToiDa ?? 20
                    })
                    .ToListAsync(); // <--- Thực thi câu lệnh SQL tại đây để lấy dữ liệu về RAM

                // Bước 3.2: Sắp xếp trên RAM (Lúc này thuộc tính PhanTram đã được tính toán bởi C#)
                var tyLe = rawTyLe
                    .OrderByDescending(x => x.PhanTram)
                    .ToList();

                return Ok(new BaoCaoDaoTaoDTO
                {
                    TongHocVien = tongHV,
                    TongLopDangMo = tongLop,
                    PhanBoHocVien = phanBo,
                    TyLeLapDay = tyLe
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}