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
        [StringLength(50)]
        public string Code { get; set; } = null!; // Mã voucher: VD SALE50

        [Required]
        public decimal DiscountAmount { get; set; }  // số tiền giảm cố định

        [Range(0, 100)]
        public int? DiscountPercent { get; set; }    // hoặc % giảm giá

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        
    }

}
