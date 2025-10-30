using System;
using System.Collections.Generic;

namespace server.Models;

public partial class HoaDonKhoaHoc
{
    public int IdHoaDon { get; set; }

    public int? HocVienId { get; set; }

    public int? IdLopHoc { get; set; }

    public int? IdKhoaHoc { get; set; }

    public double? TongTien { get; set; }

    public double? GiamGia { get; set; }

    public DateOnly? Ngaytao { get; set; }

    public bool? TrangThai { get; set; }

    public virtual HocVien? HocVien { get; set; }

    public virtual KhoaHoc? IdKhoaHocNavigation { get; set; }

    public virtual LopHoc? IdLopHocNavigation { get; set; }
}
