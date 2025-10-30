using System;
using System.Collections.Generic;

namespace server.Models;

public partial class PhanHoi
{
    public int IdPhanHoi { get; set; }

    public int? IdHocVien { get; set; }

    public int? IdLopHoc { get; set; }

    public int? SoSao { get; set; }

    public DateOnly? NgayPh { get; set; }

    public string? NoiDung { get; set; }

    public virtual HocVien? IdHocVienNavigation { get; set; }

    public virtual LopHoc? IdLopHocNavigation { get; set; }
}
