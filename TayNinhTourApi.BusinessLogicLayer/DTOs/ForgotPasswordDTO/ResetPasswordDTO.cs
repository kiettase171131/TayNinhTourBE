using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.ForgotPasswordDTO
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Vui lòng điền OTP")]
        public required string Otp { get; set; }

        [Required(ErrorMessage = "Vui lòng điền Email")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        public required string NewPassword { get; set; }


        [JsonIgnore]
        public string? ClientIp { get; set; }
    }
}
