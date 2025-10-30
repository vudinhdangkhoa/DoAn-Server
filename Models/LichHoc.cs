using System;
using System.Collections.Generic;

namespace server.Models;

public partial class LichHoc
{
    public int IdLichHoc { get; set; }

    public int? IdPhong { get; set; }

    public int? IdLopHoc { get; set; }

    public DateOnly? NgayHoc { get; set; }

    public TimeOnly? ThoiGianBatDau { get; set; }

    public TimeOnly? ThoiGianKetThuc { get; set; }

    public bool? TrangThai { get; set; }

    public virtual LopHoc? IdLopHocNavigation { get; set; }

    public virtual PhongHoc? IdPhongNavigation { get; set; }
}
