using System;
using System.Collections.Generic;

namespace server.Models;

public partial class Quyen
{
    public int IdQuyen { get; set; }

    public string? TenQuyen { get; set; }

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}
