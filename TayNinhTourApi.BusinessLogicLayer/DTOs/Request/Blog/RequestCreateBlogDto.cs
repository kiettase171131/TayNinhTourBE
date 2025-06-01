using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog
{
    public class RequestCreateBlogDto
    {
        [Required(ErrorMessage = "Please enter title")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Please enter content")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "Please select images")]
        public List<IFormFile>? Files { get; set; } 
    }
}
