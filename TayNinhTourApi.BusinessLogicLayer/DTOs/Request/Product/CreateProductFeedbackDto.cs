using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product
{
    public class CreateProductFeedbackDto
    {
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        [Range(1, 5, ErrorMessage = "Rating phải nằm trong khoảng từ 1 đến 5.")]
        public int Rating { get; set; }  // 1–5
        public string Review { get; set; } = null!;
    }

}
