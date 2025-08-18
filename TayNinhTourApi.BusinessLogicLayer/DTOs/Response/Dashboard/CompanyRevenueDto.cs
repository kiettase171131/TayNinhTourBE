using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    public class CompanyRevenueDto
    {
        
        public Guid UserId { get; set; }         // ID của user thuộc TourCompany
        public string CompanyName { get; set; } = string.Empty;
        public decimal RevenueBeforeTax { get; set; }    // 90% (trừ 10%)
        public decimal RevenueAfterTax { get; set; }     // 80% (trừ 20%)
    }

}
