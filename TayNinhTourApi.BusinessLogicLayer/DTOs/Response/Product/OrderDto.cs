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

        /// <summary>
        /// QR code data for customer pickup verification
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// URL to the QR code image
        /// </summary>
        public string? QRCodeImageUrl { get; set; }

        /// <summary>
        /// Whether the QR code has been used for pickup
        /// </summary>
        public bool IsQRCodeUsed { get; set; }

        /// <summary>
        /// When the QR code was used
        /// </summary>
        public DateTime? QRCodeUsedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<OrderDetailDto> OrderDetails { get; set; } = new();
    }
}
