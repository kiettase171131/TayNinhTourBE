using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms
{
    public class RequestUpdateUserCmsDto
    {
        public string? Name { get; set; }

        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Phone number must be 10 digits and start with 0")]
        public string? PhoneNumber { get; set; }

        public string? Avatar { get; set; }
        
        public bool? IsActive { get; set; }
    }
}
