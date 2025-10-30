using System;
using System.Collections.Generic;

namespace server.Models;

public partial class CacKhoaHocKhuyenMai
{
    public int? IdKhuyenMai { get; set; }

    public int? IdKhoaHoc { get; set; }

    public int Id { get; set; }

    public virtual KhoaHoc? IdKhoaHocNavigation { get; set; }

    public virtual KhuyenMai? IdKhuyenMaiNavigation { get; set; }
}
