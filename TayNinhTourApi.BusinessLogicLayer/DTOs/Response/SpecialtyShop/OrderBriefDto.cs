using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop
{
    public class OrderBriefDto
    {
        public Guid OrderId { get; set; }
        public string? PayOsOrderCode { get; set; }
        public bool IsChecked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
