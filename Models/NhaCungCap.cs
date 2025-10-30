using System;
using System.Collections.Generic;

namespace server.Models;

public partial class NhaCungCap
{
    public int IdNhaCungCap { get; set; }

    public string? TenNhaCungCap { get; set; }

    public string? Sdt { get; set; }

    public virtual ICollection<PhieuNhapHang> PhieuNhapHangs { get; set; } = new List<PhieuNhapHang>();
}
