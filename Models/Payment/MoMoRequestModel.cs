using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models.Payment
{
    public class MoMoRequestModel
    {
        public string OrderId { get; set; }
        public int Amount { get; set; }
        public string OrderInfo { get; set; } = "Thanh toán với MoMo";
    }
}