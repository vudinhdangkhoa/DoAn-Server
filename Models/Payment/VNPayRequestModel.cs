using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models.Payment
{
    public class VNPayRequestModel
    {
        public string OrderId { get; set; }
    public long Amount { get; set; }
    public string OrderInfo { get; set; }
    }
}