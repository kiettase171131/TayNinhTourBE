using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho lời mời hướng dẫn viên cho một TourDetails
    /// Quản lý workflow invitation/acceptance cho việc phân công guide
    /// </summary>
    public class TourGuideInvitation : BaseEntity
    {
        /// <summary>
        /// ID của TourDetails mà lời mời này thuộc về
        /// </summary>
        [Required]
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// ID của TourGuide được mời
        /// </summary>
        [Required]
        public Guid GuideId { get; set; }

        /// <summary>
        /// Loại lời mời (Automatic hoặc Manual)
        /// </summary>
        [Required]
        public InvitationType InvitationType { get; set; }

        /// <summary>
        /// Trạng thái lời mời
        /// </summary>
        [Required]
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        /// <summary>
        /// Thời gian gửi lời mời
        /// </summary>
        [Required]
        public DateTime InvitedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian TourGuide phản hồi (accept/reject)
        /// </summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Thời gian hết hạn lời mời
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Ghi chú từ TourGuide khi từ chối lời mời
        /// </summary>
        [StringLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Tin nhắn từ TourCompany khi gửi lời mời
        /// </summary>
        [StringLength(500)]
        public string? InvitationMessage { get; set; }

        // Navigation Properties

        /// <summary>
        /// TourDetails mà lời mời này thuộc về
        /// </summary>
        public virtual TourDetails TourDetails { get; set; } = null!;

        /// <summary>
        /// TourGuide được mời
        /// </summary>
        public virtual TourGuide TourGuide { get; set; } = null!;

        /// <summary>
        /// User (TourCompany) đã tạo lời mời này
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật lời mời này lần cuối
        /// </summary>
        public virtual User? UpdatedBy { get; set; }
    }
}
