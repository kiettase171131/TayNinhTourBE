using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog
{
    public class RequestBlogReactionDto
    {
        public Guid BlogId { get; set; }
        public BlogStatusEnum Reaction { get; set; }
    }
}
