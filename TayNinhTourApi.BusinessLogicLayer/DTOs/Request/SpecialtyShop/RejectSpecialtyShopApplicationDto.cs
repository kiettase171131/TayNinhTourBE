using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop
{
    /// <summary>
    /// DTO cho việc từ chối đơn đăng ký Specialty Shop
    /// </summary>
    public class RejectSpecialtyShopApplicationDto
    {
        /// <summary>
        /// Lý do từ chối đơn đăng ký
        /// </summary>
        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        [MinLength(10, ErrorMessage = "Rejection reason must be at least 10 characters")]
        public string Reason { get; set; } = null!;
    }
}
