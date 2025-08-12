using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop
{
    public class ShopVisitDto
    {
        public Guid ShopId { get; set; }

        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }

        public Guid TourSlotId { get; set; }
        public DateOnly TourDate { get; set; }
        public string? TourName { get; set; }               // 👈 thêm

        public Guid TimelineItemId { get; set; }
        public string Activity { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public TimeSpan PlannedCheckInTime { get; set; }
        public DateTime PlannedCheckInAtUtc { get; set; }   // sẽ tính sau khi ProjectTo
        //public bool IsCompleted { get; set; }
        //public DateTime? ActualCompletedAt { get; set; }
        public List<PurchasedProductDto> Products { get; set; } = new();

    }
}
