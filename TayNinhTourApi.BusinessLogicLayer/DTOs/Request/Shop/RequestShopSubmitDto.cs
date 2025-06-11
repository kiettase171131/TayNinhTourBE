using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Shop
{
    public class RequestShopSubmitDto
    {
        [Required]
        public string ShopName { get; set; } = null!;
        [Required]
        public string RepresentativeName { get; set; } = null!;
        public string? Website { get; set; }

        public string? Description { get; set; }
        [Required]
        public string? ShopType { get; set; }
        [Required]
        public string Location { get; set; } = null!;
        [Required]
        public IFormFile? Logo { get; set; }
        [Required]
        public IFormFile? BusinessLicense { get; set; }
        [Required]
        public string Email { get; set; } = null!;
    }
}
