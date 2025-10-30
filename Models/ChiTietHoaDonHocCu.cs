using System;
using System.Collections.Generic;

namespace server.Models;

public partial class ChiTietHoaDonHocCu
{
    public int IdHoaDonHocCu { get; set; }

    public int IdHocCu { get; set; }

    public int? SoLuong { get; set; }

    public virtual HoaDonHocCu IdHoaDonHocCuNavigation { get; set; } = null!;

    public virtual HocCu IdHocCuNavigation { get; set; } = null!;
}
