using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.TrangChu.DTO
{
    public class AddTaiKhoanHocVien
    {
        public string? TenHv { get; set; }

        public string? NgaySinh { get; set; }

        public string? GioiTinh { get; set; }
        public IFormFile? Avatar { get; set; }
        
    }

    
}