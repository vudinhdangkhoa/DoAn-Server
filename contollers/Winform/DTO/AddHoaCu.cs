using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Winform.DTO
{
    public class AddHoaCu
    {
        public int? IdLoaiHocCu { get; set; }

        public string? TenHocCu { get; set; }

        public int? SoLuong { get; set; }

        public string? DonViTinh { get; set; }

        public double? GiaBan { get; set; }
    }

    public class DSNhapKhoHocCu
    {
        public List<NhapKhoHocCu> danhSachNhapKhoHocCu { get; set; }
        public int idNhaCungCap { get; set; }
        public int userId { get; set; }

    }

    public class NhapKhoHocCu
    {
        public int idHocCu { get; set; }
        public int soLuong { get; set; }
        public double giaNhap { get; set; }
    }

    public class AddLoaiHocCu
    {
        public string? tenLoai { get; set; }
    }
    public class AddNhaCungCap
    {
        public string? tenNhaCungCap { get; set; }

        public string? soDienThoai { get; set; }
    }

    public class BanHoaCu
    {
        public string sdt { get; set; }
        public double? TongTien { get; set; }

        public double? GiamGia { get; set; }

        public int? IdNhanVien { get; set; }
        public List<Dictionary<int,int>> dsHoaCu { get; set; }// key: idHocCu, value: soLuong
    }
}