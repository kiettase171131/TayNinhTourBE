using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/payment-callback")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductService _productService;
        private readonly IProductRepository _productRepository;
        public PaymentController(IOrderRepository orderRepository, IProductService productService, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productService = productService;
            _productRepository = productRepository;
        }

        /// <summary>
        /// PayOS callback khi thanh toán thành công
        /// URL: /api/payment-callback/paid/{orderCode}
        /// Supports both numeric PayOsOrderCode and GUID Order.Id
        /// Status = 1 (Paid) + Tr? stock + Xóa cart
        /// </summary>
        [HttpPost("paid/{orderCode}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PaymentPaidCallback(string orderCode)
        {
            try
            {
                Console.WriteLine($"PayOS PAID Callback received for orderCode: {orderCode}");

                if (string.IsNullOrEmpty(orderCode))
                {
                    Console.WriteLine("Invalid orderCode: null or empty");
                    return BadRequest("OrderCode is required");
                }

                Order? order = null;

                // Try parse as numeric PayOsOrderCode first
                if (long.TryParse(orderCode, out long numericOrderCode))
                {
                    Console.WriteLine($"Looking for order with PayOsOrderCode: {numericOrderCode}");
                    order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == numericOrderCode, includes: new[] { "OrderDetails" });
                }

                // If not found, try parse as GUID Order.Id
                if (order == null && Guid.TryParse(orderCode, out Guid orderGuid))
                {   
                    Console.WriteLine($"Looking for order with ID: {orderGuid}");
                    var includes = new[] { "OrderDetails" };
                    order = await _orderRepository.GetByIdAsync(orderGuid,includes);
                }

                if (order == null)
                {
                    Console.WriteLine($"Order not found with orderCode: {orderCode}");
                    return NotFound($"Không tìm th?y don hàng v?i orderCode: {orderCode}");
                }

                Console.WriteLine($"Found order: {order.Id}, Current Status: {order.Status}");

                // Process payment only if not already paid
                if (order.Status != OrderStatus.Paid)
                {
                    Console.WriteLine("Processing PAID status...");

                    order.Status = OrderStatus.Paid;
                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();
                    Console.WriteLine("Order status updated to PAID (status = 1)");

                    Console.WriteLine("Calling ClearCartAndUpdateInventoryAsync...");
                    await _productService.ClearCartAndUpdateInventoryAsync(order.Id);
                    Console.WriteLine("Stock updated and cart cleared");
                    var orderDetails = order.OrderDetails.ToList();
                    foreach (var item in orderDetails)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.SoldCount += item.Quantity;
                            await _productRepository.UpdateAsync(product);
                        }
                    }
                    await _productRepository.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine("Order already paid, skipping processing");
                }

                return Ok(new
                {
                    message = "Thanh toán thành công - Ðã c?p nh?t tr?ng thái và tr? stock",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status, // = 1
                    stockUpdated = true,
                    cartCleared = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PAID callback error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Có l?i x?y ra khi x? lý thanh toán thành công.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// PayOS callback khi thanh toán b? h?y
        /// URL: /api/payment-callback/cancelled/{orderCode}
        /// Supports both numeric PayOsOrderCode and GUID Order.Id
        /// Status = 2 (Cancelled) + KHÔNG tr? stock + KHÔNG xóa cart
        /// </summary>
        [HttpPost("cancelled/{orderCode}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PaymentCancelledCallback(string orderCode)
        {
            try
            {
                Console.WriteLine($"PayOS CANCELLED Callback received for orderCode: {orderCode}");

                if (string.IsNullOrEmpty(orderCode))
                {
                    Console.WriteLine("Invalid orderCode: null or empty");
                    return BadRequest("OrderCode is required");
                }

                Order? order = null;

                // Try parse as numeric PayOsOrderCode first
                if (long.TryParse(orderCode, out long numericOrderCode))
                {
                    Console.WriteLine($"Looking for order with PayOsOrderCode: {numericOrderCode}");
                    order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == numericOrderCode);
                }

                // If not found, try parse as GUID Order.Id
                if (order == null && Guid.TryParse(orderCode, out Guid orderGuid))
                {
                    Console.WriteLine($"Looking for order with ID: {orderGuid}");
                    order = await _orderRepository.GetByIdAsync(orderGuid);
                }

                if (order == null)
                {
                    Console.WriteLine($"Order not found with orderCode: {orderCode}");
                    return NotFound($"Không tìm th?y don hàng v?i orderCode: {orderCode}");
                }

                Console.WriteLine($"Found order: {order.Id}, Current Status: {order.Status}");

                // ? Ch? d?i status thành CANCELLED - KHÔNG tr? stock, KHÔNG xóa cart
                Console.WriteLine("Processing CANCELLED status...");
                order.Status = OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();
                Console.WriteLine("Order status updated to CANCELLED (status = 2) - Stock UNCHANGED");

                return Ok(new
                {
                    message = "Thanh toán dã b? h?y - Ch? c?p nh?t tr?ng thái, KHÔNG tr? stock",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status, // = 2
                    stockUpdated = false,
                    cartCleared = false,
                    note = "Stock và cart du?c gi? nguyên"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CANCELLED callback error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Có l?i x?y ra khi x? lý h?y thanh toán.",
                    error = ex.Message
                });
            }
        }
    }
}
