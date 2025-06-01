using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog
{
    public class ResponseCreateBlogDto : BaseResposeDto
    {
        public Guid BlogId { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
