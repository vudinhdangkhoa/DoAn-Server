using System;
using System.Collections.Generic;

namespace server.Models;

public partial class ChiTietPhieuNhap
{
    public int IdPhieuNhapHang { get; set; }

    public int IdHocCu { get; set; }

    public int? SoLuong { get; set; }

    public double? Gia { get; set; }

    public virtual HocCu IdHocCuNavigation { get; set; } = null!;

    public virtual PhieuNhapHang IdPhieuNhapHangNavigation { get; set; } = null!;
}
