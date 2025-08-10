using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourFeedback
{
    public class CreateTourFeedbackRequest
    {
        [Required] public Guid BookingId { get; set; }
        [Range(1, 5)] public int TourRating { get; set; }
        [StringLength(2000)] public string? TourComment { get; set; }
        [Range(1, 5)] public int? GuideRating { get; set; }
        [StringLength(2000)] public string? GuideComment { get; set; }
    }
}
