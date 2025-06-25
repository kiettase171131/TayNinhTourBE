using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms
{
    using System.ComponentModel.DataAnnotations;
    using TayNinhTourApi.BusinessLogicLayer.Common;  // Để dùng Constants

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

        [Required(ErrorMessage = "RoleId là bắt buộc")]
        public Guid RoleId { get; set; }
    }


}
