using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Entity ?? l?u tr? phi�n chat c?a user v?i AI chatbot
    /// </summary>
    public class AIChatSession : BaseEntity
    {
        /// <summary>
        /// Foreign Key ??n User
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Ti�u ?? c?a phi�n chat (t? ??ng t?o t? tin nh?n ??u ti�n)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Tr?ng th�i phi�n chat (Active, Archived, Deleted)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Th?i gian tin nh?n cu?i c�ng trong phi�n
        /// </summary>
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property ??n User
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Navigation property ??n danh s�ch tin nh?n
        /// </summary>
        public virtual ICollection<AIChatMessage> Messages { get; set; } = new List<AIChatMessage>();
    }
}