using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.XacThuc.DTO
{
    public class UpdatePhuHuynh
    {
        public string? TenPh { get; set; }

        public string? NgaySinh { get; set; }

        public string? Sdt { get; set; }
        public string? GioiTinh { get; set; }
        public IFormFile? avatar { get; set; }

    }
}