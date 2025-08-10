using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourFeedback
{
    public class TourFeedbackDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid SlotId { get; set; }
        public Guid UserId { get; set; }
        public int TourRating { get; set; }
        public string? TourComment { get; set; }    
        public int? GuideRating { get; set; }
        public string? GuideComment { get; set; }
        public Guid? TourGuideId { get; set; }
        public string? TourGuideName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
