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
        public string? PayOsOrderCode { get; set; }

        /// <summary>
        /// Indicates if the order has been checked and delivered by the shop
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Timestamp when the order was checked/delivered by shop
        /// </summary>
        public DateTime? CheckedAt { get; set; }

        /// <summary>
        /// ID of the specialty shop that checked/delivered the order
        /// </summary>
        public Guid? CheckedByShopId { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<OrderDetailDto> OrderDetails { get; set; } = new();
    }
}
