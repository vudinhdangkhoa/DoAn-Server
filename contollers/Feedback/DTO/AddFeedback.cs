using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.contollers.Feedback.DTO
{
    public class AddFeedback
    {
        public int? IdHocVien { get; set; }

        public int? IdLopHoc { get; set; }

        public int? SoSao { get; set; }
        
        public string? NoiDung { get; set; }
    }
}