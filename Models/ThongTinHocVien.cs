using System;
using System.Collections.Generic;

namespace server.Models;

public partial class ThongTinHocVien
{
    public int UserId { get; set; }

    public string? TenHv { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? Sdt { get; set; }

    public string? Avatar { get; set; }

    public virtual User User { get; set; } = null!;
}
