using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Notification
{
    /// <summary>
    /// DTO cho response thông báo
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// ID c?a thông báo
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tiêu ?? thông báo
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// N?i dung thông báo
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Lo?i thông báo
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// ?? ?u tiên thông báo
        /// </summary>
        public string Priority { get; set; } = string.Empty;

        /// <summary>
        /// ?ã ??c ch?a
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Th?i gian t?o
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Th?i gian ??c
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// D? li?u b? sung
        /// </summary>
        public string? AdditionalData { get; set; }

        /// <summary>
        /// URL ?? chuy?n h??ng
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Icon c?a notification
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Th?i gian h?t h?n
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Th?i gian t? khi t?o (ví d?: "2 phút tr??c")
        /// </summary>
        public string TimeAgo { get; set; } = string.Empty;

        /// <summary>
        /// CSS class cho priority styling
        /// </summary>
        public string PriorityClass { get; set; } = string.Empty;

        /// <summary>
        /// CSS class cho type styling
        /// </summary>
        public string TypeClass { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho response danh sách thông báo
    /// </summary>
    public class NotificationsResponseDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách thông báo
        /// </summary>
        public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();

        /// <summary>
        /// T?ng s? thông báo
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Trang hi?n t?i
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Kích th??c trang
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// T?ng s? trang
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// S? thông báo ch?a ??c
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// Có trang ti?p theo không
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Có trang tr??c không
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO cho response s? l??ng thông báo ch?a ??c
    /// </summary>
    public class UnreadCountResponseDto : BaseResposeDto
    {
        /// <summary>
        /// S? thông báo ch?a ??c
        /// </summary>
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// DTO cho response th?ng kê thông báo
    /// </summary>
    public class NotificationStatsDto
    {
        /// <summary>
        /// T?ng s? thông báo
        /// </summary>
        public int TotalNotifications { get; set; }

        /// <summary>
        /// S? thông báo ch?a ??c
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// S? thông báo ?ã ??c
        /// </summary>
        public int ReadCount { get; set; }

        /// <summary>
        /// S? thông báo ?u tiên cao
        /// </summary>
        public int HighPriorityCount { get; set; }

        /// <summary>
        /// S? thông báo kh?n c?p
        /// </summary>
        public int UrgentCount { get; set; }

        /// <summary>
        /// Thông báo m?i nh?t
        /// </summary>
        public NotificationDto? LatestNotification { get; set; }
    }
}