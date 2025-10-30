using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models.Payment
{
    public class PaymentRequest
    {
        public int IdLopHoc { get; set; }
        public int? HocVienId { get; set; }
        public int? KhoaHocId { get; set; }
        public int PhuHuynhId { get; set; }
        public int Amount { get; set; }
        public string PaymentMethod { get; set; } // "MoMo" hoặc "VNPay"
    }
}