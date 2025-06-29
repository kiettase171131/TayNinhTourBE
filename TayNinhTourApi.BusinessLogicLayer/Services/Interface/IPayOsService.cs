using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IPayOsService
    {
        Task<string?> CreatePaymentUrlAsync(decimal amount, string orderCode, string returnUrl);
        //Task<string> VerifyPaymentStatusAsync(string orderCode);
    }
}
