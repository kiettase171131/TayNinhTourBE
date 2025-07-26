using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using static System.Net.WebRequestMethods;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class PayOsService : IPayOsService
    {
        private readonly IConfiguration _config;
        public PayOsService(IConfiguration config)
        {
            _config = config;
        }
        /// <summary>
        /// Tạo PayOS payment URL cho product orders với webhook URLs
        /// Follows PayOS best practices with proper error handling and logging
        /// </summary>
        public async Task<string?> CreatePaymentUrlAsync(decimal amount, string orderCode, string baseUrl)
        {
            try
            {
                var clientId = _config["PayOS:ClientId"];
                var apiKey = _config["PayOS:ApiKey"];
                var checksumKey = _config["PayOS:ChecksumKey"];

                // Validate PayOS configuration
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
                {
                    throw new InvalidOperationException("PayOS configuration is incomplete. Please check ClientId, ApiKey, and ChecksumKey.");
                }

                List<ItemData> items = new List<ItemData>();
                PayOS payOS = new PayOS(clientId, apiKey, checksumKey);

                // orderCode đã được truyền vào với format TNDT + 10 số
                var payOsOrderCodeString = orderCode;

                // PayOS yêu cầu orderCode phải là số, nên chỉ lấy phần số từ orderCode (loại bỏ "TNDT")
                var numericPart = orderCode.StartsWith("TNDT") ? orderCode.Substring(4) : orderCode;
                if (!long.TryParse(numericPart, out var orderCode2))
                {
                    throw new ArgumentException($"Invalid order code format: {orderCode}. Expected TNDT followed by numeric value.");
                }

                var orderCodeDisplay = payOsOrderCodeString;
                // Fix: Rút ngắn description để phù hợp với giới hạn 25 ký tự của PayOS
                PaymentData paymentData = new PaymentData(
                 orderCode: orderCode2,
                 amount: (int)amount,
                 description: $"{orderCodeDisplay}",
                 items: items,
                 cancelUrl: $"{baseUrl}/product-payment-cancel?orderId={orderCode}&orderCode={payOsOrderCodeString}",
                 returnUrl: $"{baseUrl}/product-payment-success?orderId={orderCode}&orderCode={payOsOrderCodeString}",
                 buyerName: "Product Customer");

                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                // Log successful payment URL creation
                Console.WriteLine($"PayOS product payment URL created successfully for order {orderCode}: {createPayment.checkoutUrl}");

                return createPayment.checkoutUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating PayOS product payment URL for order {orderCode}: {ex.Message}");
                throw; // Re-throw to let caller handle the error appropriately
            }
        }

        /// <summary>
        /// Tạo PayOS payment URL cho tour booking với webhook URLs
        /// Follows PayOS best practices with proper error handling and logging
        /// </summary>
        public async Task<string?> CreateTourBookingPaymentUrlAsync(decimal amount, string orderCode, string baseUrl)
        {
            try
            {
                var clientId = _config["PayOS:ClientId"];
                var apiKey = _config["PayOS:ApiKey"];
                var checksumKey = _config["PayOS:ChecksumKey"];

                // Validate PayOS configuration
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
                {
                    throw new InvalidOperationException("PayOS configuration is incomplete. Please check ClientId, ApiKey, and ChecksumKey.");
                }

                List<ItemData> items = new List<ItemData>();
                PayOS payOS = new PayOS(clientId, apiKey, checksumKey);

                // orderCode đã được truyền vào với format TNDT + 10 số
                var payOsOrderCodeString = orderCode;

                // PayOS yêu cầu orderCode phải là số, nên chỉ lấy phần số từ orderCode (loại bỏ "TNDT")
                var numericPart = orderCode.StartsWith("TNDT") ? orderCode.Substring(4) : orderCode;
                if (!long.TryParse(numericPart, out var orderCode2))
                {
                    throw new ArgumentException($"Invalid order code format: {orderCode}. Expected TNDT followed by numeric value.");
                }

                var orderCodeDisplay = payOsOrderCodeString;
                // Fix: Rút ngắn description để phù hợp với giới hạn 25 ký tự của PayOS
                PaymentData paymentData = new PaymentData(
                 orderCode: orderCode2,
                 amount: (int)amount,
                 description: $"Tour {orderCodeDisplay}",
                 items: items,
                 cancelUrl: $"{baseUrl}/tour-payment-cancel?orderId={orderCode}&orderCode={payOsOrderCodeString}",
                 returnUrl: $"{baseUrl}/tour-payment-success?orderId={orderCode}&orderCode={payOsOrderCodeString}",
                 buyerName: "Tour Customer");

                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                // Log successful payment URL creation
                Console.WriteLine($"PayOS tour booking payment URL created successfully for order {orderCode}: {createPayment.checkoutUrl}");

                return createPayment.checkoutUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating PayOS tour booking payment URL for order {orderCode}: {ex.Message}");
                throw; // Re-throw to let caller handle the error appropriately
            }
        }
        public async Task<OrderStatus> GetOrderPaymentStatusAsync(string orderCode)
        {
            var clientId = _config["PayOS:ClientId"];
            var apiKey = _config["PayOS:ApiKey"];
            var url = $"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-client-id", clientId);
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Không lấy được trạng thái thanh toán từ PayOS");

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);
                var statusStr = json["data"]?["status"]?.ToString();

                return statusStr switch
                {
                    "PAID" => OrderStatus.Paid,
                    "CANCELLED" => OrderStatus.Cancelled,
                    _ => OrderStatus.Pending
                };
            }
        }



        //public async Task<string> VerifyPaymentStatusAsync(PayOsStatusResponseDto dto)
        //{
        //    if (dto.RawQueryCollection == null || dto.Code == "01")
        //        return "Duong dan tra ve khong hop ly";
        //    var orderCode = dto.OrderCode.ToString();

        //}

    }
}
