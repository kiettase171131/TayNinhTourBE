using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Entity ?? l?u tr? phiên chat c?a user v?i AI chatbot
    /// </summary>
    public class AIChatSession : BaseEntity
    {
        /// <summary>
        /// Foreign Key ??n User
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Tiêu ?? c?a phiên chat (t? ??ng t?o t? tin nh?n ??u tiên)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Tr?ng thái phiên chat (Active, Archived, Deleted)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Th?i gian tin nh?n cu?i cùng trong phiên
        /// </summary>
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property ??n User
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Navigation property ??n danh sách tin nh?n
        /// </summary>
        public virtual ICollection<AIChatMessage> Messages { get; set; } = new List<AIChatMessage>();
    }
}