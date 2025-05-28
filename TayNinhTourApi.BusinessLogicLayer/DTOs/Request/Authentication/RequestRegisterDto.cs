using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication
{
    public class RequestRegisterDto
    {
        [EmailAddress(ErrorMessage = "Vui lòng nhập đúng định dạng Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng điền số điện thoại")]
        public string PhoneNumber { get; set; } = null!;

        public string? Avatar { get; set; } = null!;

        [JsonIgnore]
        public string? ClientIp { get; set; }
    }
}
