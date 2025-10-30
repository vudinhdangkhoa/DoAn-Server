using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using server.Models.Payment;
using server.Services;

namespace server.contollers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentVNPayController : ControllerBase
    {
        private readonly VNPay_Services _vnPayService;

        public PaymentVNPayController(VNPay_Services vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpPost("CreatePayment")]
        public IActionResult CreatePayment([FromBody] VNPayRequestModel model)
        {
            var paymentUrl = _vnPayService.CreatePaymentUrl(model.OrderId, model.Amount, model.OrderInfo);

            return Ok(new { payUrl = paymentUrl });
        }

        [HttpGet("ReceiveVNPay")]
        public IActionResult ReceiveVNPay([FromQuery] Dictionary<string, string> query)
        {
            // Log tất cả query parameters nhận được
            Console.WriteLine("===== VNPay Callback Received =====");
            Console.WriteLine($"Total parameters: {query.Count}");

            foreach (var param in query)
            {
                Console.WriteLine($"Parameter: {param.Key} = {param.Value}");
            }
            Console.WriteLine("===================================");

            // Kiểm tra xem có nhận được các thông số quan trọng không
            bool hasResponseCode = query.ContainsKey("vnp_ResponseCode");
            bool hasTxnRef = query.ContainsKey("vnp_TxnRef");

            Console.WriteLine($"Has ResponseCode: {hasResponseCode}, Has TxnRef: {hasTxnRef}");

            // Lấy giá trị
            string resultCode = hasResponseCode ? query["vnp_ResponseCode"] : "unknown";
            string orderId = hasTxnRef ? query["vnp_TxnRef"] : "N/A";

            if (resultCode == "00")
            {
                // Thành công
                Console.WriteLine("Redirecting to success page");
                return Redirect($"http://localhost:3000/payment-result?status=success&orderId={orderId}");
            }
            else
            {
                // Thất bại
                Console.WriteLine("Redirecting to failure page");
                return Redirect($"http://localhost:3000/payment-result?status=fail&orderId={orderId}");
            }
        }
    }
}