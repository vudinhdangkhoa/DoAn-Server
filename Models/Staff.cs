using System;
using System.Collections.Generic;

namespace server.Models;

public partial class Staff
{
    public int UserId { get; set; }

    public int? IdQuyen { get; set; }

    public string? TenNv { get; set; }

    public string? Sdt { get; set; }

    public virtual Quyen? IdQuyenNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
