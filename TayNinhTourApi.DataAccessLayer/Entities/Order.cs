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
        /// PayOS numeric order code for payment tracking
        /// </summary>
        public long? PayOsOrderCode { get; set; }
       
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
