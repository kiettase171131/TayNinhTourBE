using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class VoucherCode : BaseEntity
    {
        [Required]
        public Guid VoucherId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = null!; // M� voucher ng?u nhi�n: VD SALE50-AB12CD

        public bool IsClaimed { get; set; } = false; // ?� ???c user claim/nh?n hay ch?a

        public Guid? ClaimedByUserId { get; set; } // User n�o ?� claim/nh?n voucher n�y

        public DateTime? ClaimedAt { get; set; } // Th?i gian claim

        public bool IsUsed { get; set; } = false; // ?� ???c s? d?ng trong order hay ch?a

        public Guid? UsedByUserId { get; set; } // User n�o ?� s? d?ng (th??ng gi?ng ClaimedByUserId)

        public DateTime? UsedAt { get; set; } // Th?i gian s? d?ng

        // Navigation properties
        [ForeignKey(nameof(VoucherId))]
        public virtual Voucher Voucher { get; set; } = null!;

        [ForeignKey(nameof(ClaimedByUserId))]
        public virtual User? ClaimedByUser { get; set; }

        [ForeignKey(nameof(UsedByUserId))]
        public virtual User? UsedByUser { get; set; }
    }
}