using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms
{
    public class SpecialtyShopCmsDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string ShopName { get; set; } = null!;
        public string? Description { get; set; }
        public string Location { get; set; } = null!;
        public string RepresentativeName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? BusinessLicense { get; set; }
        public string? BusinessLicenseUrl { get; set; }
        public string? LogoUrl { get; set; }
        public string? ShopType { get; set; }
        public string? OpeningHours { get; set; }
        public string? ClosingHours { get; set; }
        public decimal? Rating { get; set; }
        public bool IsShopActive { get; set; }
        public string? Notes { get; set; }

        /// <summary>
        /// Tên tài khoản User của shop (nếu cần hiển thị)
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Email User gốc
        /// </summary>
        public string UserEmail { get; set; } = null!;
    }

}
