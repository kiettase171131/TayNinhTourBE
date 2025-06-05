using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog
{
    public class ResponseBlogReactionDto : BaseResposeDto
    {
        // Thông tin phản hồi: tổng số like, tổng số dislike sau khi toggle
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public BlogStatusEnum? CurrentUserReaction { get; set; }
    }
}
