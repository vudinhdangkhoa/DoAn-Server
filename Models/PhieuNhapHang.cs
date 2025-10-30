using System;
using System.Collections.Generic;

namespace server.Models;

public partial class PhieuNhapHang
{
    public int IdPhieuNhapHang { get; set; }

    public DateOnly? NgayTao { get; set; }

    public double? TongTien { get; set; }

    public int? IdNhaCungCap { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual NhaCungCap? IdNhaCungCapNavigation { get; set; }

    public virtual NhanVien? User { get; set; }
}
