using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Entity ?? l?u tr? tin nh?n trong phiên chat v?i AI
    /// </summary>
    public class AIChatMessage : BaseEntity
    {
        /// <summary>
        /// Foreign Key ??n AIChatSession
        /// </summary>
        [Required]
        public Guid SessionId { get; set; }

        /// <summary>
        /// N?i dung tin nh?n
        /// </summary>
        [Required]
        [StringLength(4000)]
        public string Content { get; set; } = null!;

        /// <summary>
        /// Lo?i tin nh?n: User ho?c AI
        /// </summary>
        [Required]
        [StringLength(10)]
        public string MessageType { get; set; } = null!; // "User" or "AI"

        /// <summary>
        /// Token ???c s? d?ng b?i Gemini API (n?u có)
        /// </summary>
        public int? TokensUsed { get; set; }

        /// <summary>
        /// Th?i gian ph?n h?i c?a AI (milliseconds)
        /// </summary>
        public int? ResponseTimeMs { get; set; }

        /// <summary>
        /// Metadata b? sung (JSON string)
        /// </summary>
        [StringLength(1000)]
        public string? Metadata { get; set; }

        /// <summary>
        /// Navigation property ??n AIChatSession
        /// </summary>
        public virtual AIChatSession Session { get; set; } = null!;
    }
}