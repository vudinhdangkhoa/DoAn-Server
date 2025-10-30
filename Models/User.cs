using System;
using System.Collections.Generic;

namespace server.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Mail { get; set; }

    public string? MatKhau { get; set; }

    public bool? IsHocVien { get; set; }

    public string? Token { get; set; }

    public string? RefeshToken { get; set; }

    public DateTime? RefeshTokenCreatedAt { get; set; }

    public DateTime? RefreshTokenExpires { get; set; }

    public virtual NhanVien? NhanVien { get; set; }

    public virtual PhuHuynh? PhuHuynh { get; set; }
}
