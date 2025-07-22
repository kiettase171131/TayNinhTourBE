using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest
{
    /// <summary>
    /// DTO cho thông tin chi tiết yêu cầu rút tiền
    /// </summary>
    public class WithdrawalRequestDetailDto : WithdrawalRequestResponseDto
    {
        /// <summary>
        /// Thông tin user tạo yêu cầu
        /// </summary>
        public UserInfo User { get; set; } = null!;

        /// <summary>
        /// Thông tin admin xử lý (nếu có)
        /// </summary>
        public AdminInfo? ProcessedBy { get; set; }

        /// <summary>
        /// Lịch sử thay đổi trạng thái (nếu có)
        /// </summary>
        public List<StatusHistoryItem> StatusHistory { get; set; } = new();
    }

    /// <summary>
    /// Thông tin user trong withdrawal request detail
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// ID của user
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Thông tin admin xử lý
    /// </summary>
    public class AdminInfo
    {
        /// <summary>
        /// ID của admin
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Item lịch sử thay đổi trạng thái
    /// </summary>
    public class StatusHistoryItem
    {
        /// <summary>
        /// Trạng thái
        /// </summary>
        public WithdrawalStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái
        /// </summary>
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian thay đổi
        /// </summary>
        public DateTime ChangedAt { get; set; }

        /// <summary>
        /// Người thay đổi
        /// </summary>
        public string? ChangedBy { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Notes { get; set; }
    }
}
