using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class Order :BaseEntity
    {
        
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAfterDiscount { get; set; } 
        public decimal DiscountAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string? VoucherCode { get; set; }

        /// <summary>
        /// PayOS order code with TNDT prefix for display and tracking
        /// Format: TNDT + 10 digits (7 from timestamp + 3 random)
        /// </summary>
        public string? PayOsOrderCode { get; set; }

        /// <summary>
        /// QR code data string containing order information for specialty shop verification
        /// Contains: OrderId, PayOsOrderCode, UserId, TotalAmount, CreatedAt
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// URL to the generated QR code image stored on server
        /// Specialty shops will scan this QR code to verify customer purchases
        /// </summary>
        public string? QRCodeImageUrl { get; set; }

        /// <summary>
        /// Indicates if the QR code has been scanned/used by specialty shop
        /// False = Not scanned, True = Already scanned/redeemed
        /// </summary>
        public bool IsQRCodeUsed { get; set; } = false;

        /// <summary>
        /// Timestamp when the QR code was scanned/used by specialty shop
        /// </summary>
        public DateTime? QRCodeUsedAt { get; set; }

        /// <summary>
        /// ID of the specialty shop that scanned/redeemed the QR code
        /// </summary>
        public Guid? QRCodeUsedByShopId { get; set; }
       
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
