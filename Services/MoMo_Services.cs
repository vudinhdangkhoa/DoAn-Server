using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using server.Models.Payment;
using System.Net.Http.Headers;


using System.Text.Json;
using Microsoft.Extensions.Configuration;
namespace server.Services
{
    public class MoMo_Services
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;

        public MoMo_Services(IConfiguration config)
        {
            _config = config;
            _client = new HttpClient();
        }

        public static string HmacSHA256(string text, string key)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(key);
            byte[] textBytes = encoding.GetBytes(text);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(textBytes);
                return Convert.ToHexString(hashmessage).ToLower();
            }
        }

        public async Task<string> CreateMoMoPaymentAsync(MoMoRequestModel model)
        {
            string endpoint = _config["MoMo:MoMoApiUrl"];
            string partnerCode = _config["MoMo:PartnerCode"];
            string accessKey = _config["MoMo:AccessKey"];
            string secretKey = _config["MoMo:SecretKey"];
            string returnUrl = _config["MoMo:ReturnUrl"]; 
            string notifyUrl = _config["MoMo:NotifyUrl"]; 
            string requestType = _config["MoMo:RequestType"]; // "captureWallet"

            string orderId = model.OrderId;
            string requestId = Guid.NewGuid().ToString();
            string orderInfo = model.OrderInfo;
            long amount = model.Amount;

            // ✅ RawSignature chính xác thứ tự
            string rawSignature =
                "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" +
                "&ipnUrl=" + notifyUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + returnUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType;

            string signature = HmacSHA256(rawSignature, secretKey);

            var body = new
            {
                partnerCode = partnerCode,
                partnerName = "MoMo Payment",
                storeId = "Test Store",
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                lang = "vi",
                autoCapture = true,
                requestType = requestType,
                extraData = "",
                signature = signature
            };

            var jsonBody = JsonSerializer.Serialize(body);
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync(endpoint, httpContent);
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine("RawSignature: " + rawSignature);
            Console.WriteLine("Signature: " + signature);
            Console.WriteLine("Response: " + result);

            return result;
        }


    }
}