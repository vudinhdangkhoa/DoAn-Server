using System;
using System.Collections.Generic;

namespace server.Models;

public partial class PhuHuynh
{
    public int UserId { get; set; }

    public string? TenPh { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? Sdt { get; set; }

    public DateOnly? NgayTao { get; set; }

    public string? Avatar { get; set; }

    public string? GioiTinh { get; set; }

    public virtual ICollection<HoaDonHocCu> HoaDonHocCus { get; set; } = new List<HoaDonHocCu>();

    public virtual ICollection<HocVien> HocViens { get; set; } = new List<HocVien>();

    public virtual User User { get; set; } = null!;
}
