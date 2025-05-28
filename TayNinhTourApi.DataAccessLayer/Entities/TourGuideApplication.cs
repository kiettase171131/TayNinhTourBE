using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class TourGuideApplication : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? CurriculumVitae { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public string? RejectionReason { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
