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
        /// Indicates if the order has been checked and delivered by the shop
        /// Only the shop that owns products in this order can mark it as checked
        /// </summary>
        public bool IsChecked { get; set; } = false;

        /// <summary>
        /// Timestamp when the order was checked/delivered by shop
        /// </summary>
        public DateTime? CheckedAt { get; set; }

        /// <summary>
        /// ID of the specialty shop that checked/delivered the order
        /// </summary>
        public Guid? CheckedByShopId { get; set; }
       
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
