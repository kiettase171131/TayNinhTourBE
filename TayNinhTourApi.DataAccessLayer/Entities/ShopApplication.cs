using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class ShopApplication : BaseEntity
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = null!;

        public string RepresentativeName { get; set; } = null!;

        public string? Description { get; set; }

        public string Location { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }

        public string? BusinessLicenseUrl { get; set; }
        public string? ShopType { get; set; }
        public ShopStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
