using System;
using System.Collections.Generic;

namespace server.Models;

public partial class GiaoVienDdayLop
{
    public int Id { get; set; }

    public int? IdLopHoc { get; set; }

    public int? IdGiaoVien { get; set; }

    public virtual GiaoVien? IdGiaoVienNavigation { get; set; }

    public virtual LopHoc? IdLopHocNavigation { get; set; }
}
