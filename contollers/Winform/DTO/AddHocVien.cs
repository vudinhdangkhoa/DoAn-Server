using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Winform.DTO
{
    public class AddHocVien
    {
        public string tenPH;
        public string sdt;
        public string email;
        public DateOnly? ngaySinhPH;
        public List<hocVienDTO> DSHocVien;
    }

    public class hocVienDTO
    {
        public string tenHv;
        public DateOnly? ngaySinh;
        public string gioiTinh;
       
    }
}