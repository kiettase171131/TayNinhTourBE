using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product
{
    public class ProductReviewDto 
    {
        public string UserName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Rating { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
    }

}
