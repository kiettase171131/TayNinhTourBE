using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product
{
    public class ProductReviewSummaryDto: BaseResposeDto
    {
        public double AverageRating { get; set; }
        public List<ProductReviewDto> Reviews { get; set; } = new();
    }
}
