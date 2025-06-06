using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms
{
    public class RequestUpdateBlogCmsDto
    {
        public byte? Status { get; set; }
        public string CommentOfAdmin { get; set; } = null!;
    }
}
