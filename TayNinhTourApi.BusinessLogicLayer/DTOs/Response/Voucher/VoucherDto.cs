using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher
{
    public class VoucherDto 

    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public int ClaimedCount { get; set; } // Số mã đã được claim
        public int UsedCount { get; set; } // Số mã đã được sử dụng
        public int RemainingCount { get; set; } // Số mã còn lại chưa claim
        public decimal DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<VoucherCodeDto> VoucherCodes { get; set; } = new List<VoucherCodeDto>();
    }

    public class VoucherCodeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public bool IsClaimed { get; set; }
        public string? ClaimedByUserName { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public bool IsUsed { get; set; }
        public string? UsedByUserName { get; set; }
        public DateTime? UsedAt { get; set; }
    }

    // DTO cho user xem voucher của mình
    public class MyVoucherDto
    {
        public Guid VoucherCodeId { get; set; }
        public string Code { get; set; } = null!;
        public string VoucherName { get; set; } = null!;
        public decimal DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsUsed { get; set; }
        public DateTime ClaimedAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public bool IsActive => !IsExpired && !IsUsed;
        public string Status => IsUsed ? "Đã sử dụng" : IsExpired ? "Đã hết hạn" : "Có thể sử dụng";
    }

}
