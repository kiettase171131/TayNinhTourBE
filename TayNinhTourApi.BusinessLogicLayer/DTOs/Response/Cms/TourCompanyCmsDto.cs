using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms
{
    namespace TayNinhTourApi.Business.DTOs.TourCompany
    {
        public class TourCompanyCmsDto
        {
            public Guid Id { get; set; }              // ID của công ty tour
            public Guid UserId { get; set; }          // ID của User có role "Tour Company"
            public string CompanyName { get; set; } = string.Empty;
            public decimal Wallet { get; set; }
            public decimal RevenueHold { get; set; }
            public string? Description { get; set; }
            public string? Address { get; set; }
            public string? Website { get; set; }
            public string? BusinessLicense { get; set; }
            public bool IsActive { get; set; }

            // Thông tin từ User (navigation đơn giản)
            public string? Email { get; set; }
            public string? FullName { get; set; }
            public string? PhoneNumber { get; set; }
        }
    }

}
