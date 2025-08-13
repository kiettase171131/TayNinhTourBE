using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop
{
    public class OrderFullDto
    {
        public Guid OrderId { get; set; }
        public string? PayOsOrderCode { get; set; }
        public bool IsChecked { get; set; }
        public DateTime CreatedAt { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAfterDiscount { get; set; }
        public OrderStatus Status { get; set; }
        public Guid? VoucherId { get; set; }
        public DateTime? CheckedAt { get; set; }
        public Guid? CheckedByShopId { get; set; }

        public List<OrderDetailDto1> Details { get; set; } = new();
    }
}
