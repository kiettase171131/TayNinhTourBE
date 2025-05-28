using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication
{
    public class RegisterVerifyOtpRequestDto
    {
        [Required(ErrorMessage = "Vui lòng điền OTP")]
        public required string Otp { get; set; }

        [Required(ErrorMessage = "Vui lòng điền Email")]
        public required string Email { get; set; }

        [JsonIgnore]
        public string? ClientIp { get; set; }
    }
}
