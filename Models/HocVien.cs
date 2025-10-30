using System;
using System.Collections.Generic;

namespace server.Models;

public partial class HocVien
{
    public int IdHocVien { get; set; }

    public string? TenHv { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? Avartar { get; set; }

    public DateOnly? NgayTao { get; set; }

    public int? IdPhuHuynh { get; set; }

    public string? GioiTinh { get; set; }

    public virtual ICollection<HoaDonKhoaHoc> HoaDonKhoaHocs { get; set; } = new List<HoaDonKhoaHoc>();

    public virtual PhuHuynh? IdPhuHuynhNavigation { get; set; }

    public virtual ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();
}
