using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product
{
    public class CreateProductFeedbackDto
    {
        public Guid ProductId { get; set; }
        public int Rating { get; set; }  // 1–5
        public string Review { get; set; } = null!;
    }

}
