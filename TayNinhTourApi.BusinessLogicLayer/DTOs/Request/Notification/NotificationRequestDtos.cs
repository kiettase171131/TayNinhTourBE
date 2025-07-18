using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification
{
    /// <summary>
    /// DTO cho request t?o th�ng b�o m?i
    /// </summary>
    public class CreateNotificationDto
    {
        /// <summary>
        /// ID c?a user nh?n th�ng b�o
        /// </summary>
        public Guid UserId { get; set; }

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
        public NotificationType Type { get; set; }

        /// <summary>
        /// ?? ?u ti�n th�ng b�o
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// D? li?u b? sung (JSON)
        /// </summary>
        public string? AdditionalData { get; set; }

        /// <summary>
        /// URL ?? chuy?n h??ng khi click v�o notification
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Icon cho notification
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Th?i gian h?t h?n (null = kh�ng h?t h?n)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO cho request l?y danh s�ch th�ng b�o
    /// </summary>
    public class GetNotificationsRequestDto
    {
        /// <summary>
        /// Trang hi?n t?i (0-based, default: 0)
        /// </summary>
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// K�ch th??c trang (default: 20)
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// L?c theo tr?ng th�i ??c (null = t?t c?)
        /// </summary>
        public bool? IsRead { get; set; }

        /// <summary>
        /// L?c theo lo?i th�ng b�o (null = t?t c?)
        /// </summary>
        public NotificationType? Type { get; set; }
    }

    /// <summary>
    /// DTO cho request ?�nh d?u th�ng b�o ?� ??c
    /// </summary>
    public class MarkNotificationReadDto
    {
        /// <summary>
        /// ID c?a th�ng b�o
        /// </summary>
        public Guid NotificationId { get; set; }
    }
}