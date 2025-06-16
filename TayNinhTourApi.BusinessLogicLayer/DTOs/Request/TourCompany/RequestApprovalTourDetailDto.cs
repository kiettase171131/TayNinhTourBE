using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO for admin approval/rejection of tour details
    /// </summary>
    public class RequestApprovalTourDetailDto
    {
        /// <summary>
        /// Có duyệt hay không (true = duyệt, false = từ chối)
        /// </summary>
        [Required]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Bình luận của admin (bắt buộc khi từ chối)
        /// </summary>
        [StringLength(500)]
        public string? Comment { get; set; }
    }
}
