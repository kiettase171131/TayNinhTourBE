using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification
{
    /// <summary>
    /// DTO cho request t?o thông báo m?i
    /// </summary>
    public class CreateNotificationDto
    {
        /// <summary>
        /// ID c?a user nh?n thông báo
        /// </summary>
        public Guid UserId { get; set; }

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
        public NotificationType Type { get; set; }

        /// <summary>
        /// ?? ?u tiên thông báo
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// D? li?u b? sung (JSON)
        /// </summary>
        public string? AdditionalData { get; set; }

        /// <summary>
        /// URL ?? chuy?n h??ng khi click vào notification
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Icon cho notification
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Th?i gian h?t h?n (null = không h?t h?n)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO cho request l?y danh sách thông báo
    /// </summary>
    public class GetNotificationsRequestDto
    {
        /// <summary>
        /// Trang hi?n t?i (0-based, default: 0)
        /// </summary>
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// Kích th??c trang (default: 20)
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// L?c theo tr?ng thái ??c (null = t?t c?)
        /// </summary>
        public bool? IsRead { get; set; }

        /// <summary>
        /// L?c theo lo?i thông báo (null = t?t c?)
        /// </summary>
        public NotificationType? Type { get; set; }
    }

    /// <summary>
    /// DTO cho request ?ánh d?u thông báo ?ã ??c
    /// </summary>
    public class MarkNotificationReadDto
    {
        /// <summary>
        /// ID c?a thông báo
        /// </summary>
        public Guid NotificationId { get; set; }
    }
}