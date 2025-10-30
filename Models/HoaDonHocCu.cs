using System;
using System.Collections.Generic;

namespace server.Models;

public partial class HoaDonHocCu
{
    public int IdHoaDonHocCu { get; set; }

    public double? TongTien { get; set; }

    public double? GiamGia { get; set; }

    public int? IdNhanVien { get; set; }

    public int? IdKhachHang { get; set; }

    public string? Sdt { get; set; }

    public string? TenKh { get; set; }

    public virtual ICollection<ChiTietHoaDonHocCu> ChiTietHoaDonHocCus { get; set; } = new List<ChiTietHoaDonHocCu>();

    public virtual PhuHuynh? IdKhachHangNavigation { get; set; }

    public virtual NhanVien? IdNhanVienNavigation { get; set; }
}
