using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Winform.DTO
{
    public class AddHocVien
    {
        public string? tenPH { get; set; }
        public string? sdt { get; set; }
        public string? email { get; set; }
        public string? gioiTinh { get; set; }
        public DateOnly? ngaySinhPH { get; set; }
        public List<hocVienDTO>? DSHocVien { get; set; }
    }

    public class hocVienDTO
    {
        public string? tenHv { get; set; }
        public DateOnly? ngaySinh { get; set; }
        public string? gioiTinh { get; set; }

        public List<int>? dsLopId { get; set; }

    }
    
    
}