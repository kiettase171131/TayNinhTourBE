using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    public class UpdateTourCompanyDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? BusinessLicense { get; set; }
        
    }

}
