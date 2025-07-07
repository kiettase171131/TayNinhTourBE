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
        public async Task<string?> CreatePaymentUrlAsync(decimal amount, string orderCode, string returnUrl)
        {
            var clientId = _config["PayOS:ClientId"];
            var apiKey = _config["PayOS:ApiKey"];
            var checksumKey = _config["PayOS:CheckSum"];
            List<ItemData> items = new List<ItemData>();

            PayOS payOS = new PayOS(clientId, apiKey, checksumKey);
            
            // orderCode đã được truyền vào với format TNDT + 10 số
            var payOsOrderCodeString = orderCode; // Sử dụng orderCode đã truyền vào
            
            // PayOS yêu cầu orderCode phải là số, nên chỉ lấy phần số từ orderCode (loại bỏ "TNDT")
            var numericPart = orderCode.StartsWith("TNDT") ? orderCode.Substring(4) : orderCode;
            var orderCode2 = long.Parse(numericPart);
            
            var orderCodeDisplay = payOsOrderCodeString;
            PaymentData paymentData = new PaymentData(
             orderCode: orderCode2,
             amount: (int)amount,
             description: $"{orderCodeDisplay}",
             items: items,
             cancelUrl: $"https://tndt.netlify.app/payment-cancel?orderId={orderCode}&orderCode={payOsOrderCodeString}",
             returnUrl: $"https://tndt.netlify.app/payment-success?orderId={orderCode}&orderCode={payOsOrderCodeString}",
             buyerName: "kiet");
            CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);
            return createPayment.checkoutUrl;
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
                if (!response.successStatusCode)
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
