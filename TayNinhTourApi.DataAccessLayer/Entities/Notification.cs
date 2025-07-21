using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Entity ??i di?n cho th�ng b�o trong h? th?ng
    /// L?u tr? t?t c? c�c lo?i th�ng b�o in-app
    /// </summary>
    public class Notification : BaseEntity
    {
        /// <summary>
        /// ID c?a user nh?n th�ng b�o
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Ti�u ?? th�ng b�o
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// N?i dung th�ng b�o
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Lo?i th�ng b�o
        /// </summary>
        [Required]
        public NotificationType Type { get; set; }

        /// <summary>
        /// ?? ?u ti�n th�ng b�o
        /// </summary>
        [Required]
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// ?� ??c ch?a
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Th?i gian ??c
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// D? li?u b? sung (JSON)
        /// V� d?: {"tourId": "123", "bookingId": "456"}
        /// </summary>
        [StringLength(2000)]
        public string? AdditionalData { get; set; }

        /// <summary>
        /// URL ?? chuy?n h??ng khi click v�o notification
        /// </summary>
        [StringLength(500)]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Icon cho notification
        /// </summary>
        [StringLength(50)]
        public string? Icon { get; set; }

        /// <summary>
        /// Th?i gian h?t h?n (null = kh�ng h?t h?n)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        // Navigation Properties

        /// <summary>
        /// User nh?n th�ng b�o
        /// </summary>
        public virtual User User { get; set; } = null!;
    }
}