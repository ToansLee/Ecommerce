using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceMVC.Services
{
    public class VNPayService
    {
        private readonly IConfiguration _configuration;

        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string ipAddress)
        {
            var vnpayUrl = _configuration["VNPay:Url"];
            var vnpayTmnCode = _configuration["VNPay:TmnCode"];
            var vnpayHashSecret = _configuration["VNPay:HashSecret"];
            var returnUrl = _configuration["VNPay:ReturnUrl"];

            var vnpay = new VNPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnpayTmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());

            var paymentUrl = vnpay.CreateRequestUrl(vnpayUrl, vnpayHashSecret);
            return paymentUrl;
        }

        public VNPayResponseModel ProcessPaymentResponse(IQueryCollection queryCollection)
        {
            var vnpayHashSecret = _configuration["VNPay:HashSecret"];
            var vnpay = new VNPayLibrary();

            foreach (var (key, value) in queryCollection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            var orderId = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
            var vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnpayResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnpaySecureHash = queryCollection["vnp_SecureHash"];

            bool checkSignature = vnpay.ValidateSignature(vnpaySecureHash, vnpayHashSecret);

            return new VNPayResponseModel
            {
                Success = checkSignature && vnpayResponseCode == "00",
                OrderId = orderId,
                TransactionId = vnpayTranId,
                ResponseCode = vnpayResponseCode,
                SecureHash = vnpaySecureHash
            };
        }
    }

    public class VNPayResponseModel
    {
        public bool Success { get; set; }
        public int OrderId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string SecureHash { get; set; } = string.Empty;
    }

    public class VNPayLibrary
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>(new VNPayComparer());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new VNPayComparer());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();
            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            var queryString = data.ToString();
            baseUrl += "?" + queryString;
            var signData = queryString;
            if (signData.Length > 0)
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }

            var vnpSecureHash = HmacSHA512(vnpHashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnpSecureHash;

            return baseUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }

            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }

            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }
    }

    public class VNPayComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
