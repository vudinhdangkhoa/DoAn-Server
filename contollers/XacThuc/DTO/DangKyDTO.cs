using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.XacThuc
{
    public class DangKyDTO : LoginDTO
    {

        public string tenHV { get; set; }
        public DateOnly ngaySinh { get; set; }
        public string sdt { get; set; }
        public string otp { get; set; }
       
    }
}