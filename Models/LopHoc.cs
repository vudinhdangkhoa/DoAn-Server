using System;
using System.Collections.Generic;

namespace server.Models;

public partial class LopHoc
{
    public int IdLopHoc { get; set; }

    public string? TenLopHoc { get; set; }

    public int? IdKhoaHoc { get; set; }

    public int? IdPhong { get; set; }

    public DateOnly? NgayKhaiGiang { get; set; }

    public DateOnly? NgayTao { get; set; }

    public int? SoLuongBuoi { get; set; }

    public string? SoBuoiTrenTuan { get; set; }

    public int? SoLuongToiThieu { get; set; }

    public string? TrangThai { get; set; }

    public int? SoLuongToiDa { get; set; }

    public TimeOnly? ThoiGianBatDau { get; set; }

    public TimeOnly? ThoiGianKetThuc { get; set; }

    public int? SoLuongHv { get; set; }

    public virtual ICollection<GiaoVienDdayLop> GiaoVienDdayLops { get; set; } = new List<GiaoVienDdayLop>();

    public virtual ICollection<HoaDonKhoaHoc> HoaDonKhoaHocs { get; set; } = new List<HoaDonKhoaHoc>();

    public virtual ICollection<HocCuThuocLop> HocCuThuocLops { get; set; } = new List<HocCuThuocLop>();

    public virtual KhoaHoc? IdKhoaHocNavigation { get; set; }

    public virtual PhongHoc? IdPhongNavigation { get; set; }

    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();

    public virtual ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();
}
