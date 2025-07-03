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
            var orderCode2 = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PaymentData paymentData = new PaymentData(
             orderCode: orderCode2,
             amount: (int)amount,
             description: $"don hang",
             items: items,
             cancelUrl: "https://tndt.netlify.app/about",
             returnUrl: "https://tndt.netlify.app/blog",
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
