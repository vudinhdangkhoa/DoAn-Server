using System;
using System.Collections.Generic;

namespace server.Models;

public partial class GiaoVien
{
    public int GiaoVienId { get; set; }

    public string? TenGv { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? Sdt { get; set; }

    public string? Avatar { get; set; }

    public int? SoNamKinhNghiem { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<GiaoVienDdayLop> GiaoVienDdayLops { get; set; } = new List<GiaoVienDdayLop>();
}
