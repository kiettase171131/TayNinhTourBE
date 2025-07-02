using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product
{
    public class CreateProductReviewDto
    {
        public Guid ProductId { get; set; }
        public string Content { get; set; } = null!;
    }

}
