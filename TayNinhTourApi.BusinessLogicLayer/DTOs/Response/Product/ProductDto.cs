using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public Guid SpecialtyShopId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public List<string> ImageUrl { get; set; } = new();
        public string? Category { get; set; }
        public bool IsSale { get; set; }
        public int? SalePercent { get; set; }
        public int SoldCount { get; set; }
        public double? AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
