using System;
using System.Collections.Generic;

namespace server.Models;

public partial class PhongHoc
{
    public int IdPhong { get; set; }

    public string? TenPhong { get; set; }

    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
