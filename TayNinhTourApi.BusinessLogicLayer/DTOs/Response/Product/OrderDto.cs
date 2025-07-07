using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAfterDiscount { get; set; }
        public OrderStatus Status { get; set; }
        public string? VoucherCode { get; set; }
        public string? PayOsOrderCode { get; set; } // Thay đổi từ long? thành string? để phù hợp với Order entity

        public DateTime CreatedAt { get; set; }

        public List<OrderDetailDto> OrderDetails { get; set; } = new();
    }
}
