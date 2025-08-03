using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    public class BloggerDashboardDto
    {
        public int TotalPosts { get; set; }
        public int ApprovedPosts { get; set; }
        public int RejectedPosts { get; set; }
        public int PendingPosts { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
    }
}
