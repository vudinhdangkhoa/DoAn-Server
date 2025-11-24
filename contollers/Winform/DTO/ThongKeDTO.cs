using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Winform.DTO
{
    public class BaoCaoDoanhThuDTO
{
    public double TongDoanhThuThuan { get; set; } // Đã trừ giảm giá
    public double TongChiPhiNhap { get; set; }
    public double LoiNhuanRong { get; set; } // Doanh thu - Chi phí
    public List<DoanhThuTheoThangDTO> ChiTietTheoThang { get; set; }
}

public class DoanhThuTheoThangDTO
{
    public string ThangNam { get; set; }
    public int Thang { get; set; }
    public int Nam { get; set; }
    public double TienHocPhi { get; set; } // Thực thu
    public double TienHoaCu { get; set; } // Thực thu
    public double TienNhapHang { get; set; } // Chi phí
    public double LoiNhuan => (TienHocPhi + TienHoaCu) - TienNhapHang;
}

    // --- TAB 2: HỌA CỤ ---
    public class BaoCaoHoaCuDTO
    {
        public List<TopBanChayDTO> TopBanChay { get; set; }
        public List<CanhBaoTonKhoDTO> SapHetHang { get; set; }
    }

    public class TopBanChayDTO
    {
        public string TenHocCu { get; set; }
        public int SoLuongBan { get; set; }
        public double DoanhThu { get; set; }
    }

    public class CanhBaoTonKhoDTO
    {
        public string TenHocCu { get; set; }
        public string DonViTinh { get; set; }
        public int TonKho { get; set; }
    }

    // --- TAB 3: ĐÀO TẠO ---
    public class BaoCaoDaoTaoDTO
    {
        public int TongHocVien { get; set; }
        public int TongLopDangMo { get; set; }
        public List<HocVienTheoKhoaDTO> PhanBoHocVien { get; set; }
        public List<TyLeLapDayDTO> TyLeLapDay { get; set; }
    }

    public class HocVienTheoKhoaDTO
    {
        public string TenKhoaHoc { get; set; }
        public int SoLuongHocVien { get; set; }
    }

    public class TyLeLapDayDTO
    {
        public string TenLop { get; set; }
        public int SiSoHienTai { get; set; }
        public int SiSoToiDa { get; set; }
        public double PhanTram => SiSoToiDa > 0 ? Math.Round((double)SiSoHienTai / SiSoToiDa * 100, 1) : 0;
    }
}