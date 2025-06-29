using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Org.BouncyCastle.Asn1.Ocsp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
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
            string orderCode2 = DateTimeOffset.Now.ToString("ffffff");
            PaymentData paymentData = new PaymentData(
             orderCode: int.Parse(orderCode2),
             amount: (int)amount*1000,
             description: $"don hang",
             items: items,
             cancelUrl: "https://tayninhtour.card-diversevercel.io.vn",
             returnUrl: "https://tayninhtour.card-diversevercel.io.vn",
             buyerName: "kiet");
            CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);
            return createPayment.checkoutUrl;
        }
        //public async Task<string> VerifyPaymentStatusAsync(PayOsStatusResponseDto dto)
        //{
        //    if (dto.RawQueryCollection == null || dto.Code == "01")
        //        return "Duong dan tra ve khong hop ly";
        //    var orderCode = dto.OrderCode.ToString();

        //}

    }
}
