using System;
using System.Collections.Generic;

namespace server.Models;

public partial class KhoaHoc
{
    public int IdKhoaHoc { get; set; }

    public int? IdChuyenMon { get; set; }

    public DateOnly? NgayTao { get; set; }

    public string? MoTa { get; set; }

    public string? MucTieu { get; set; }

    public string? LoTrinh { get; set; }

    public double? HocPhi { get; set; }

    public int? SoLuongBuoi { get; set; }

    public string? HinhAnh { get; set; }

    public string? TenKhoaHoc { get; set; }

    public virtual ICollection<CacKhoaHocKhuyenMai> CacKhoaHocKhuyenMais { get; set; } = new List<CacKhoaHocKhuyenMai>();

    public virtual ICollection<HoaDonKhoaHoc> HoaDonKhoaHocs { get; set; } = new List<HoaDonKhoaHoc>();

    public virtual ChuyenMon? IdChuyenMonNavigation { get; set; }

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
