using System;
using System.Collections.Generic;

namespace server.Models;

public partial class HocCuThuocLop
{
    public int IdHocCu { get; set; }

    public int IdLopHoc { get; set; }

    public int? SoLuong { get; set; }

    public virtual HocCu IdHocCuNavigation { get; set; } = null!;

    public virtual LopHoc IdLopHocNavigation { get; set; } = null!;
}
