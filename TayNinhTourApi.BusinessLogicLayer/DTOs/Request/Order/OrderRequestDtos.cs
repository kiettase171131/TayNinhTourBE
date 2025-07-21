using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Order
{
    public class CheckOrderRequestDto
    {
        [Required(ErrorMessage = "PayOS Order Code là b?t bu?c")]
        public string PayOsOrderCode { get; set; } = null!;
    }

    public class GetOrdersRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsChecked { get; set; } // Filter by checked status
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}