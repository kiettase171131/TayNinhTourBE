using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher
{
    public class ResponseGetVouchersDto : BaseResposeDto
    {
        public List<VoucherDto> Data { get; set; } = new List<VoucherDto>();
        public int TotalRecord { get; set; }
        public int TotalPages { get; set; }
    }

    public class ResponseGetAvailableVoucherCodesDto : BaseResposeDto
    {
        public List<AvailableVoucherCodeDto> Data { get; set; } = new List<AvailableVoucherCodeDto>();
        public int TotalRecord { get; set; }
        public int TotalPages { get; set; }
    }

    public class AvailableVoucherCodeDto
    {
        public Guid VoucherCodeId { get; set; }
        public string VoucherName { get; set; } = null!;
        public string Code { get; set; } = null!;
        public decimal DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsExpiringSoon => EndDate.Subtract(DateTime.UtcNow).TotalDays <= 7;
        public bool CanClaim => !IsExpiringSoon && DateTime.UtcNow >= StartDate;
    }

    public class ResponseClaimVoucherDto : BaseResposeDto
    {
        public MyVoucherDto? VoucherCode { get; set; }
    }

    public class ResponseGetMyVouchersDto : BaseResposeDto
    {
        public List<MyVoucherDto> Data { get; set; } = new List<MyVoucherDto>();
        public int TotalRecord { get; set; }
        public int TotalPages { get; set; }
        public int ActiveCount { get; set; } // Số voucher chưa sử dụng
        public int UsedCount { get; set; } // Số voucher đã sử dụng
        public int ExpiredCount { get; set; } // Số voucher đã hết hạn
    }
}
