using System;
using System.Collections.Generic;

namespace server.Models;

public partial class LoaiHocCu
{
    public int IdLoaiHocCu { get; set; }

    public string? TenLoai { get; set; }

    public virtual ICollection<HocCu> HocCus { get; set; } = new List<HocCu>();
}
