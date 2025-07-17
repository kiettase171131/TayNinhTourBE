using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Order;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface for order management
    /// Used for order checking and delivery confirmation by specialty shops
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Mark an order as checked/delivered by a specialty shop using PayOsOrderCode
        /// Only the shop that owns products in this order can mark it as checked
        /// </summary>
        /// <param name="payOsOrderCode">PayOS Order Code to identify the order</param>
        /// <param name="shopId">ID of the specialty shop checking the order</param>
        /// <returns>Response containing order details and processing result</returns>
        Task<CheckOrderResponseDto> CheckOrderAsync(string payOsOrderCode, Guid shopId);

        /// <summary>
        /// Get order details for checking purposes using PayOsOrderCode
        /// </summary>
        /// <param name="payOsOrderCode">PayOS Order Code</param>
        /// <returns>Order details</returns>
        Task<OrderDetailsResponseDto> GetOrderDetailsAsync(string payOsOrderCode);

        /// <summary>
        /// Get all orders that can be checked by a specific shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>List of orders that can be checked by the shop</returns>
        Task<OrderListResponseDto> GetCheckableOrdersAsync(Guid shopId);
    }
}