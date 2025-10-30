using System;
using System.Collections.Generic;

namespace server.Models;

public partial class ChuyenMon
{
    public int IdChuyenMon { get; set; }

    public string? TenChuyenMon { get; set; }

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public virtual ICollection<KhoaHoc> KhoaHocs { get; set; } = new List<KhoaHoc>();
}
