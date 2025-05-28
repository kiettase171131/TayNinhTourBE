using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO
{
    public class SubmitApplicationDto
    {
        public IFormFile? CurriculumVitae { get; set; }
        [Required]  
        public string Email { get; set; } = null!;

    }
}
