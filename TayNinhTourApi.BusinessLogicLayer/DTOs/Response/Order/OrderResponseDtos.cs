using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Order
{
    public class CheckOrderResponseDto : BaseResposeDto
    {
        public bool IsProcessed { get; set; }
        public bool WasAlreadyChecked { get; set; }
        public DateTime? CheckedAt { get; set; }
        public string? CheckedByShopName { get; set; }
        public OrderDto? Order { get; set; }
        public CustomerInfoDto? Customer { get; set; }
        public List<OrderDetailDto>? Products { get; set; }
    }

    public class OrderDetailsResponseDto : BaseResposeDto
    {
        public Guid OrderId { get; set; }
        public string? PayOsOrderCode { get; set; }
        public bool IsChecked { get; set; }
        public DateTime? CheckedAt { get; set; }
        public string? CheckedByShopName { get; set; }
        public OrderDto? Order { get; set; }
    }

    public class OrderListResponseDto : BaseResposeDto
    {
        public List<OrderDto>? Orders { get; set; }
        public int TotalCount { get; set; }
    }

    public class CustomerInfoDto
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}