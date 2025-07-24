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
        public int Quantity { get; set; } // Số lượng mã voucher được tạo

        [Required]
        public decimal DiscountAmount { get; set; }  // số tiền giảm cố định

        [Range(0, 100)]
        public int? DiscountPercent { get; set; }    // hoặc % giảm giá

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Navigation property
        public virtual ICollection<VoucherCode> VoucherCodes { get; set; } = new List<VoucherCode>();
    }

}
