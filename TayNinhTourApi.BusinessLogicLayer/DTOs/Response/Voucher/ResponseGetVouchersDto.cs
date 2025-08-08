using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher
{
    public class ResponseGetVouchersDto : BaseResposeDto
    {
        public List<VoucherDto> Data { get; set; } = new List<VoucherDto>();
        public int TotalRecord { get; set; }
        public int TotalPages { get; set; }
    }

    public class ResponseGetVoucherDto : BaseResposeDto
    {
        public VoucherDto? Data { get; set; }
    }

    public class ResponseGetAvailableVouchersDto : BaseResposeDto
    {
        public List<AvailableVoucherDto> Data { get; set; } = new List<AvailableVoucherDto>();
        public int TotalRecord { get; set; }
        public int TotalPages { get; set; }
    }

    public class AvailableVoucherDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public int RemainingCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        // Sử dụng Vietnam timezone
        public bool IsExpiringSoon => EndDate.Subtract(VietnamTimeZoneUtility.GetVietnamNow()).TotalDays <= 7;
    }

    public class ResponseCreateVoucher : BaseResposeDto
    {
        public Guid VoucherId { get; set; }
        public string VoucherName { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
