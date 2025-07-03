using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product
{
    public class RequestUpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? QuantityInStock { get; set; }
        public string? Category { get; set; }
        public bool? IsSale { get; set; }
        public int? SalePercent { get; set; }
        public List<IFormFile>? Files { get; set; }
    }

}
