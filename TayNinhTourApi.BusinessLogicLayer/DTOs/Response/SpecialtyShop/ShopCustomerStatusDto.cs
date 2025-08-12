using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop
{
    public class ShopCustomerStatusDto : BaseResposeDto
    {
        public Guid SpecialtyShopId { get; set; }
        public Guid CustomerUserId { get; set; }
        public bool IsShop { get; set; }                 // 👈 theo yêu cầu
        public DateOnly? NextTourDate { get; set; }
        public TimeSpan? PlannedCheckInTime { get; set; }
        public Guid? TimelineItemId { get; set; }
        public string? Activity { get; set; }
    }
}
