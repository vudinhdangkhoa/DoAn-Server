using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.TrangChu.DTO
{
    public class DangKyLopHoc
    {
        public int? HocVienId { get; set; }
        public int? IdLopHoc { get; set; }

        public int? IdKhoaHoc { get; set; }

        public double? TongTien { get; set; }

        public double? GiamGia { get; set; }
    }
}