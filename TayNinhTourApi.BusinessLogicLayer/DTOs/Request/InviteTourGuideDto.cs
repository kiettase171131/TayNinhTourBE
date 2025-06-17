using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request
{
    /// <summary>
    /// DTO cho request mời thủ công TourGuide cho một TourDetails
    /// Sử dụng bởi TourCompany để mời guide cụ thể
    /// </summary>
    public class InviteTourGuideDto
    {
        /// <summary>
        /// ID của TourDetails cần mời guide
        /// </summary>
        [Required(ErrorMessage = "TourDetailsId là bắt buộc")]
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// ID của TourGuide được mời
        /// </summary>
        [Required(ErrorMessage = "GuideId là bắt buộc")]
        public Guid GuideId { get; set; }

        /// <summary>
        /// Ghi chú thêm từ TourCompany (tùy chọn)
        /// Ví dụ: "Chúng tôi rất mong được hợp tác với bạn cho tour này"
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? AdditionalMessage { get; set; }

        /// <summary>
        /// Số ngày hết hạn lời mời (mặc định 3 ngày)
        /// </summary>
        [Range(1, 7, ErrorMessage = "Thời hạn lời mời phải từ 1 đến 7 ngày")]
        public int ExpirationDays { get; set; } = 3;
    }
}
