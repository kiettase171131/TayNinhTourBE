using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class ShopCustomerStatus : BaseEntity
    {
        [Required] public Guid SpecialtyShopId { get; set; }
        [Required] public Guid CustomerUserId { get; set; }

        // Cờ: user đã từng mua SP của shop, và trong TƯƠNG LAI có tour ghé shop
        public bool IsUpcomingVisitor { get; set; }

        // Thông tin chuyến ghé sắp tới
        public DateOnly? NextTourDate { get; set; }
        public TimeSpan? PlannedCheckInTime { get; set; }
        public Guid? TimelineItemId { get; set; }
        public string? Activity { get; set; }

        // Nav (optional)
        public virtual SpecialtyShop SpecialtyShop { get; set; } = null!;
        public virtual User Customer { get; set; } = null!;
    }
}
