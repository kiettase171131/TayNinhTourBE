using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IPayOsService
    {
        Task<string?> CreatePaymentUrlAsync(decimal amount, string orderCode, string baseUrl);
        Task<string?> CreateTourBookingPaymentUrlAsync(decimal amount, string orderCode, string baseUrl);
        Task<OrderStatus> GetOrderPaymentStatusAsync(string orderCode);
        //Task<string> VerifyPaymentStatusAsync(string orderCode);
    }
}
