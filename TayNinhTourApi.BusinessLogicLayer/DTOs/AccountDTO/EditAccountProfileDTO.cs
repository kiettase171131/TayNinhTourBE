using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO
{
    public class EditAccountProfileDTO
    {
        [Required(ErrorMessage = "Account name is required.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Account name must contain only letters and spaces.")]
        public string? Name { get; set; }
       
        

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits long and contain only numbers.")]
        public string? PhoneNumber { get; set; }
    }
}
