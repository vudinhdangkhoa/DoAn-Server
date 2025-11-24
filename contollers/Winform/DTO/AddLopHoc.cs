using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Winform.DTO
{
    public class AddLopHoc
    {
        public string? TenLopHoc { get; set; }

        public int? IdKhoaHoc { get; set; }

        public int? IdPhong { get; set; }

        public List<int>? GiaoVienId { get; set; }

        public DateTime? NgayKhaiGiang { get; set; }

       

        public string? SoBuoiTrenTuan { get; set; }

        public int? SoLuongToiThieu { get; set; }

       

        public int? SoLuongToiDa { get; set; }

        public TimeOnly? ThoiGianBatDau { get; set; }

        public TimeOnly? ThoiGianKetThuc { get; set; }

        public Dictionary<int,int>? DShocCu { get; set; }// Key: IdHocCu, Value: SoLuong
    }
}