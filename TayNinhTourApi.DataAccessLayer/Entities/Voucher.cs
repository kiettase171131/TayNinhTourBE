using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class Voucher : BaseEntity   
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!; // Tên voucher: VD "Khuyến mãi Black Friday"

        [Required]
        [StringLength(500)]
        public string? Description { get; set; } // Mô tả voucher

        [Required]
        public int Quantity { get; set; } // Số lượng voucher có thể được sử dụng

        [Required]
        public int UsedCount { get; set; } = 0; // Số lượng đã được sử dụng

        [Required]
        public decimal DiscountAmount { get; set; } = 0; // Số tiền giảm cố định

        [Range(0, 100)]
        public int? DiscountPercent { get; set; } // Hoặc % giảm giá

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Computed properties
        public int RemainingCount => Math.Max(0, Quantity - UsedCount);
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public bool IsAvailable => IsActive && !IsExpired && RemainingCount > 0 && DateTime.UtcNow >= StartDate;

        // Navigation properties for orders that used this voucher
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
