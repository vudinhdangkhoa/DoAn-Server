using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Winform.DTO
{
    public class AddLichHoc
    {
        public int idLopHoc { get; set; }
        public int idLichHoc { get; set; }

        public DateTime ngayHoc { get; set; }
        public TimeSpan thoiGianBatDau { get; set; }
        public TimeSpan thoiGianKetThuc { get; set; }
        public int? idPhong { get; set; }
    }
}