using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    public class SlotRevenueDto
    {
        public Guid TourSlotId { get; set; }
        public decimal TotalRevenueHold { get; set; }
        public DateTime? LatestRevenueTransferredDate { get; set; }
        public TourSlotStatus Status { get; set; }

        public string CompanyName { get; set; } = string.Empty; // ✅ Mới thêm
    }

}
