using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response
{
    /// <summary>
    /// DTO cho response thông tin TourGuide invitation
    /// Sử dụng để trả về thông tin invitation cho API calls
    /// </summary>
    public class TourGuideInvitationDto
    {
        /// <summary>
        /// ID của invitation
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Thông tin TourDetails
        /// </summary>
        public TourDetailsBasicDto TourDetails { get; set; } = null!;

        /// <summary>
        /// Thông tin TourGuide được mời
        /// </summary>
        public UserBasicDto Guide { get; set; } = null!;

        /// <summary>
        /// Thông tin TourCompany đã tạo lời mời
        /// </summary>
        public UserBasicDto CreatedBy { get; set; } = null!;

        /// <summary>
        /// Loại lời mời (Automatic/Manual)
        /// </summary>
        public string InvitationType { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái lời mời (Pending/Accepted/Rejected/Expired)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian gửi lời mời
        /// </summary>
        public DateTime InvitedAt { get; set; }

        /// <summary>
        /// Thời gian hết hạn lời mời
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Thời gian TourGuide phản hồi (nếu có)
        /// </summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Lý do từ chối (nếu status = Rejected)
        /// </summary>
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Tin nhắn từ TourCompany khi gửi lời mời
        /// </summary>
        public string? InvitationMessage { get; set; }

        /// <summary>
        /// Gợi ý cải thiện từ TourGuide (nếu có)
        /// </summary>
        public string? ImprovementSuggestion { get; set; }

        /// <summary>
        /// Số giờ còn lại trước khi hết hạn (tính toán)
        /// </summary>
        public double? HoursUntilExpiry =>
            Status == "Pending" && ExpiresAt > VietnamTimeZoneUtility.GetVietnamNow()
                ? (ExpiresAt - VietnamTimeZoneUtility.GetVietnamNow()).TotalHours
                : null;

        /// <summary>
        /// Có thể chấp nhận lời mời không (tính toán)
        /// </summary>
        public bool CanAccept =>
            Status == "Pending" && ExpiresAt > VietnamTimeZoneUtility.GetVietnamNow();

        /// <summary>
        /// Có thể từ chối lời mời không (tính toán)
        /// </summary>
        public bool CanReject =>
            Status == "Pending" && ExpiresAt > VietnamTimeZoneUtility.GetVietnamNow();
    }

    /// <summary>
    /// DTO cơ bản cho TourDetails trong invitation context
    /// </summary>
    public class TourDetailsBasicDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SkillsRequired { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO cơ bản cho User trong invitation context
    /// </summary>
    public class UserBasicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
