using System;
using System.Collections.Generic;

namespace server.Models;

public partial class HocCu
{
    public int IdHocCu { get; set; }

    public int? IdLoaiHocCu { get; set; }

    public string? TenHocCu { get; set; }

    public int? SoLuong { get; set; }

    public string? DonViTinh { get; set; }

    public double? GiaBan { get; set; }

    public virtual ICollection<CacHocCuKhuyenMai> CacHocCuKhuyenMais { get; set; } = new List<CacHocCuKhuyenMai>();

    public virtual ICollection<ChiTietHoaDonHocCu> ChiTietHoaDonHocCus { get; set; } = new List<ChiTietHoaDonHocCu>();

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual ICollection<HocCuThuocLop> HocCuThuocLops { get; set; } = new List<HocCuThuocLop>();

    public virtual LoaiHocCu? IdLoaiHocCuNavigation { get; set; }
}
