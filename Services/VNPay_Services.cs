using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace server.Services
{
    public class VNPay_Services
    {

        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VNPay_Services(IConfiguration config)
        {
            _config = config;
            _client = new HttpClient();
        }

        public string CreatePaymentUrl(string orderId, long amount, string orderInfo)
        {
            var tmnCode = _config["VNPay:TmnCode"];
            var hashSecret = _config["VNPay:HashSecret"];
            var baseUrl = _config["VNPay:BaseUrl"];
            var returnUrl = _config["VNPay:ReturnUrl"];

            var vnp_Params = new Dictionary<string, string>
        {
            {"vnp_Version", "2.1.0"},
            {"vnp_Command", "pay"},
            {"vnp_TmnCode", tmnCode},
            {"vnp_Amount", (amount * 100).ToString()},
            {"vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")},
            {"vnp_CurrCode", "VND"},
            {"vnp_IpAddr", "127.0.0.1"},
            {"vnp_Locale", "vn"},
            {"vnp_OrderInfo", orderInfo},
            {"vnp_OrderType", "other"},
            {"vnp_ReturnUrl", returnUrl},
            {"vnp_TxnRef", orderId},
            //{"vnp_BankCode", "VNBANK"} //VNBANK	Thẻ nội địa (ATM) INTCARD	Thẻ quốc tế (Visa/Master/JCB) QRCode	Thanh toán qua mã QR VNPay
        };

            // Sắp xếp theo key alphabet
            var sortedParams = vnp_Params.OrderBy(p => p.Key)
                                         .ToDictionary(p => p.Key, p => p.Value);

            var queryString = new StringBuilder();
            var rawData = new StringBuilder();

            foreach (var kv in sortedParams)
            {
                string encodedKey = WebUtility.UrlEncode(kv.Key);
                string encodedValue = WebUtility.UrlEncode(kv.Value)
                    .Replace("%20", "+"); // VNPAY quy định khoảng trắng là '+'

                queryString.Append($"{encodedKey}={encodedValue}&");
                rawData.Append($"{encodedKey}={encodedValue}&");
            }

            queryString.Length -= 1; // Bỏ ký tự '&' cuối
            rawData.Length -= 1;

            var secureHash = HmacSHA512(hashSecret, rawData.ToString());

            var paymentUrl = $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
            return paymentUrl;
        }

        private static string HmacSHA512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public bool ValidateSignature(string rawQueryWithQuestionMark, string inputHash)
        {
            var hashSecret = _config["VNPay:HashSecret"];

            // 1. Bỏ dấu '?' ở đầu chuỗi query
            if (string.IsNullOrEmpty(rawQueryWithQuestionMark) || rawQueryWithQuestionMark[0] != '?')
            {
                return false;
            }
            string rawData = rawQueryWithQuestionMark.Substring(1);

            // 2. Tách các cặp key-value. 
            // Các value ở đây VẪN CÒN NGUYÊN DẠNG URL ENCODE (ví dụ: "Thanh%20toan")
            var parts = rawData.Split('&')
                .Select(p =>
                {
                    var pair = p.Split('=');
                    return new KeyValuePair<string, string>(pair[0], pair.Length > 1 ? pair[1] : string.Empty);
                })
                .ToList();

            // 3. Lọc bỏ vnp_SecureHash và vnp_SecureHashType
            var sortedParams = new SortedDictionary<string, string>();
            foreach (var kvp in parts)
            {
                if (!string.IsNullOrEmpty(kvp.Value) && kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType")
                {
                    sortedParams.Add(kvp.Key, kvp.Value);
                }
            }

            // 4. Xây dựng lại chuỗi để băm TỪ CÁC GIÁ TRỊ CÒN MÃ HÓA
            var stringToHashBuilder = new StringBuilder();
            foreach (var kvp in sortedParams)
            {
                // kvp.Value ở đây vẫn là "Thanh%20toan", không phải "Thanh toan"
                stringToHashBuilder.Append($"{kvp.Key}={kvp.Value}&");
            }

            // Bỏ ký tự '&' cuối cùng
            string stringToHash = stringToHashBuilder.ToString();
            if (stringToHash.EndsWith("&"))
            {
                stringToHash = stringToHash.Substring(0, stringToHash.Length - 1);
            }

            // 5. Băm và so sánh
            string checkSum = HmacSHA512(hashSecret, stringToHash); // Giả sử hàm HmacSHA512 của bạn trả về Hex string

            // Debug (rất quan trọng)
            Console.WriteLine("--- VNPay Signature Validation ---");
            Console.WriteLine("String to Hash (My Side): " + stringToHash);
            Console.WriteLine("My Hash: " + checkSum);
            Console.WriteLine("VNPay's Hash: " + inputHash);
            Console.WriteLine("---------------------------------");

            return checkSum.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
        }


    }
}