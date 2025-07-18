using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Notification
{
    /// <summary>
    /// DTO cho response th�ng b�o
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// ID c?a th�ng b�o
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Ti�u ?? th�ng b�o
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// N?i dung th�ng b�o
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Lo?i th�ng b�o
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// ?? ?u ti�n th�ng b�o
        /// </summary>
        public string Priority { get; set; } = string.Empty;

        /// <summary>
        /// ?� ??c ch?a
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
        /// Th?i gian t? khi t?o (v� d?: "2 ph�t tr??c")
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
    /// DTO cho response danh s�ch th�ng b�o
    /// </summary>
    public class NotificationsResponseDto : BaseResposeDto
    {
        /// <summary>
        /// Danh s�ch th�ng b�o
        /// </summary>
        public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();

        /// <summary>
        /// T?ng s? th�ng b�o
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Trang hi?n t?i
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// K�ch th??c trang
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// T?ng s? trang
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// S? th�ng b�o ch?a ??c
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// C� trang ti?p theo kh�ng
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// C� trang tr??c kh�ng
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO cho response s? l??ng th�ng b�o ch?a ??c
    /// </summary>
    public class UnreadCountResponseDto : BaseResposeDto
    {
        /// <summary>
        /// S? th�ng b�o ch?a ??c
        /// </summary>
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// DTO cho response th?ng k� th�ng b�o
    /// </summary>
    public class NotificationStatsDto
    {
        /// <summary>
        /// T?ng s? th�ng b�o
        /// </summary>
        public int TotalNotifications { get; set; }

        /// <summary>
        /// S? th�ng b�o ch?a ??c
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// S? th�ng b�o ?� ??c
        /// </summary>
        public int ReadCount { get; set; }

        /// <summary>
        /// S? th�ng b�o ?u ti�n cao
        /// </summary>
        public int HighPriorityCount { get; set; }

        /// <summary>
        /// S? th�ng b�o kh?n c?p
        /// </summary>
        public int UrgentCount { get; set; }

        /// <summary>
        /// Th�ng b�o m?i nh?t
        /// </summary>
        public NotificationDto? LatestNotification { get; set; }
    }
}