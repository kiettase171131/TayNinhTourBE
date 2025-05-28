using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? UpdatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
