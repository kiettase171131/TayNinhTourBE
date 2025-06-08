using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog
{
    public class BlogDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public string CommentOfAdmin { get; set; } = null!;
        public byte Status { get; set; }
        public List<string> ImageUrl { get; set; } =  new();
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public int TotalComments { get; set; }
    }
}
