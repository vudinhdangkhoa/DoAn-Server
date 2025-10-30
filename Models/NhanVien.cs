using System;
using System.Collections.Generic;

namespace server.Models;

public partial class NhanVien
{
    public int UserId { get; set; }

    public int? IdQuyen { get; set; }

    public string? TenNv { get; set; }

    public string? Sdt { get; set; }

    public virtual ICollection<HoaDonHocCu> HoaDonHocCus { get; set; } = new List<HoaDonHocCu>();

    public virtual Quyen? IdQuyenNavigation { get; set; }

    public virtual ICollection<PhieuNhapHang> PhieuNhapHangs { get; set; } = new List<PhieuNhapHang>();

    public virtual User User { get; set; } = null!;
}
