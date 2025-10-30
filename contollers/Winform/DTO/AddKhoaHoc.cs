using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Winform.DTO
{
    public class AddKhoaHoc
    {
        public int? IdChuyenMon { get; set; }


        public string? MoTa { get; set; }

        public string? MucTieu { get; set; }

        public double? HocPhi { get; set; }

        public int? SoLuongBuoi { get; set; }

        public IFormFile? HinhAnh { get; set; }

        public string? TenKhoaHoc { get; set; }
        

    }
}