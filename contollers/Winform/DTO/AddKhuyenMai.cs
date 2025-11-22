using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Winform.DTO
{
    public class AddKhuyenMai
    {
        public string tenKhuyenMai { get; set; }
        public double phanTramKhuyenMai { get; set; }
    }

    public class ApdungKhuyenMai
    {
        public int idKhuyenMai { get; set; }
        public List<int>? lstIdHoaCu { get; set; }
        public List<int>? lstIdKhoaHoc { get; set; }
        public DateTime ngayBatDau { get; set; }
        public DateTime ngayKetThuc { get; set; }
        public int soLuong { get; set; }
    }
    public class UpdateApDung
    {
        public int id { get; set; }
        public string type { get; set; }
        public DateTime ngayKetThuc { get; set; }
        public int soLuong { get; set; }
    }
}