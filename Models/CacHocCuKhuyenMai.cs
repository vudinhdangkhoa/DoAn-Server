using System;
using System.Collections.Generic;

namespace server.Models;

public partial class CacHocCuKhuyenMai
{
    public int? IdKhuyenMai { get; set; }

    public int? IdHocCu { get; set; }

    public int Id { get; set; }

    public virtual HocCu? IdHocCuNavigation { get; set; }

    public virtual KhuyenMai? IdKhuyenMaiNavigation { get; set; }
}
