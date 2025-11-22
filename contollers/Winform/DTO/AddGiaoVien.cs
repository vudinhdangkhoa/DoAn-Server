using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Winform.DTO
{
    public class AddGiaoVien
    {
        public string? TenGv { get; set; }

        public DateOnly? NgaySinh { get; set; }

        public string? Sdt { get; set; }

        public IFormFile? Avatar { get; set; }

        public int? SoNamKinhNghiem { get; set; }


    }
}