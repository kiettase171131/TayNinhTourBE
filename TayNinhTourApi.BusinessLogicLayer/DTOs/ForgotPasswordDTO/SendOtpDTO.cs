using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.ForgotPasswordDTO
{
    public class SendOtpDTO
    {
        [Required(ErrorMessage = "Vui lòng điền Email")]
        public string Email { get; set; }

        [JsonIgnore]
        public string? ClientIp { get; set; }
    }
}
