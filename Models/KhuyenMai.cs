using System;
using System.Collections.Generic;

namespace server.Models;

public partial class KhuyenMai
{
    public int IdKhuyenMai { get; set; }

    public string? TenKhuyenMai { get; set; }

    public double? PhanTramKhuyenMai { get; set; }

    public virtual ICollection<CacHocCuKhuyenMai> CacHocCuKhuyenMais { get; set; } = new List<CacHocCuKhuyenMai>();

    public virtual ICollection<CacKhoaHocKhuyenMai> CacKhoaHocKhuyenMais { get; set; } = new List<CacKhoaHocKhuyenMai>();
}
