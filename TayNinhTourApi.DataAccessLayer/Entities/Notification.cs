using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Entity ??i di?n cho thông báo trong h? th?ng
    /// L?u tr? t?t c? các lo?i thông báo in-app
    /// </summary>
    public class Notification : BaseEntity
    {
        /// <summary>
        /// ID c?a user nh?n thông báo
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Tiêu ?? thông báo
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// N?i dung thông báo
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Lo?i thông báo
        /// </summary>
        [Required]
        public NotificationType Type { get; set; }

        /// <summary>
        /// ?? ?u tiên thông báo
        /// </summary>
        [Required]
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// ?ã ??c ch?a
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Th?i gian ??c
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// D? li?u b? sung (JSON)
        /// Ví d?: {"tourId": "123", "bookingId": "456"}
        /// </summary>
        [StringLength(2000)]
        public string? AdditionalData { get; set; }

        /// <summary>
        /// URL ?? chuy?n h??ng khi click vào notification
        /// </summary>
        [StringLength(500)]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Icon cho notification
        /// </summary>
        [StringLength(50)]
        public string? Icon { get; set; }

        /// <summary>
        /// Th?i gian h?t h?n (null = không h?t h?n)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        // Navigation Properties

        /// <summary>
        /// User nh?n thông báo
        /// </summary>
        public virtual User User { get; set; } = null!;
    }
}