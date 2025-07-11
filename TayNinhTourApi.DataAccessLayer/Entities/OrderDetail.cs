using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class OrderDetail : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
            
        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}
