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

        public Task<(string? checkoutUrl, long payOsOrderCode)> CreatePaymentUrlAsync(decimal amount, string orderCode, string returnUrl)
        {
            throw new NotImplementedException();
        }

        public Task<OrderStatus> GetOrderPaymentStatusAsync(string orderCode)
        {
            throw new NotImplementedException();
        }
    }
}
