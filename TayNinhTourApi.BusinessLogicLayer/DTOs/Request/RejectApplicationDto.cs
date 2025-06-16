using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request
{
    public class RejectApplicationDto
    {
        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
