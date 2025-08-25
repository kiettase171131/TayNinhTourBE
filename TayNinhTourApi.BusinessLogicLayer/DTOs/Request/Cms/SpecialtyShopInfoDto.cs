using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms
{
    public class SpecialtyShopInfoDto
    {
        [Required]
        [StringLength(200)]
        public string ShopName { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Location { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string RepresentativeName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string ContactEmail { get; set; } = null!;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        

        [StringLength(50)]
        public string? ShopType { get; set; }

        [StringLength(10)]
        public string? OpeningHours { get; set; }

        [StringLength(10)]
        public string? ClosingHours { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

}
