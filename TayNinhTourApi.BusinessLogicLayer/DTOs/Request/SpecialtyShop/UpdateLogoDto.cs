using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop
{
    public class UpdateLogoDto
    {
        [Required]
        public IFormFile Logo { get; set; } = null!;
    }

}
