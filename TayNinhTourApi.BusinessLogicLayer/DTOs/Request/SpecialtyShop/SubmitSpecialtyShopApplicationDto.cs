using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.BusinessLogicLayer.Attributes;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop
{
    public class SubmitSpecialtyShopApplicationDto
    {
        [Required(ErrorMessage = "Shop name is required")]
        [StringLength(200, ErrorMessage = "Shop name cannot exceed 200 characters")]
        public string ShopName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Shop description cannot exceed 1000 characters")]
        public string? ShopDescription { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Website cannot exceed 200 characters")]
        [Url(ErrorMessage = "Invalid website URL format")]
        public string? Website { get; set; }

        [StringLength(50, ErrorMessage = "Shop type cannot exceed 50 characters")]
        public string? ShopType { get; set; }

        [StringLength(10, ErrorMessage = "Opening hours cannot exceed 10 characters")]
        [TimeFormatValidation]
        public string? OpeningHours { get; set; }

        [StringLength(10, ErrorMessage = "Closing hours cannot exceed 10 characters")]
        [TimeFormatValidation]
        public string? ClosingHours { get; set; }

        [Required(ErrorMessage = "Representative name is required")]
        [StringLength(100, ErrorMessage = "Representative name cannot exceed 100 characters")]
        public string RepresentativeName { get; set; } = string.Empty;

        /// <summary>
        /// Business license document file upload
        /// </summary>
        [Required(ErrorMessage = "Giấy phép kinh doanh là bắt buộc")]
        [BusinessLicenseFileValidation]
        public IFormFile BusinessLicenseFile { get; set; } = null!;

        /// <summary>
        /// Shop logo file upload
        /// </summary>
        [Required(ErrorMessage = "Logo cửa hàng là bắt buộc")]
        [LogoFileValidation]
        public IFormFile Logo { get; set; } = null!;
    }
}
