using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms
{
    public class TourCompanyInfoDto
    {
        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

       

    }

}
