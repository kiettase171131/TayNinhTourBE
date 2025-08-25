using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using TayNinhTourApi.BusinessLogicLayer.Common;  // Để dùng Constants
    using TayNinhTourApi.BusinessLogicLayer.Common.Enums;


    public class RequestCreateUserDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [RegularExpression(Constants.EmailRegexPattern, ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên tối đa 200 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(Constants.PhoneNumberRegexPattern, ErrorMessage = "Số điện thoại phải đúng 10 số và không chứa ký tự đặc biệt")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Role là bắt buộc")]
        
        public RoleNameEnum RoleName { get; set; }
        // Optional - các field chỉ dùng nếu role là TourCompany hoặc SpecialtyShop
        public TourCompanyInfoDto? TourCompanyInfo { get; set; }
        public SpecialtyShopInfoDto? SpecialtyShopInfo { get; set; }
    }


}
