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
        private readonly ISpecialtyShopRepository _specialtyShopRepository;

        public PaymentController(IOrderRepository orderRepository, IProductService productService, IProductRepository productRepository, ISpecialtyShopRepository specialtyShopRepository)
        {
            _orderRepository = orderRepository;
            _productService = productService;
            _productRepository = productRepository;
            _specialtyShopRepository = specialtyShopRepository;
        }

        /// <summary>
        /// PayOS callback khi thanh toán thành công
        /// URL: /api/payment-callback/paid/{orderCode}
        /// Supports both string PayOsOrderCode (TNDT format) and GUID Order.Id
        /// Status = 1 (Paid) + Trừ stock + Xóa cart + Cộng tiền vào ví shop
        /// </summary>
        [HttpPost("paid/{orderCode}")]
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

                // Try to find by PayOsOrderCode first (now string with TNDT format)
                Console.WriteLine($"Looking for order with PayOsOrderCode: {orderCode}");
                order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == orderCode, includes: new[] { "OrderDetails" });

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
                    return NotFound($"Không tìm thấy đơn hàng với orderCode: {orderCode}");
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
                    decimal totalWalletAdded = 0;
                    
                    foreach (var item in orderDetails)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            // Update sold count
                            product.SoldCount += item.Quantity;
                            await _productRepository.UpdateAsync(product);

                            // Add money to specialty shop wallet
                            // Use TotalAmount (original price) instead of discounted price
                            // System bears the discount loss, shop gets full amount
                            var shopWalletAmount = item.UnitPrice * item.Quantity;
                            
                            var specialtyShop = await _specialtyShopRepository.GetByUserIdAsync(product.ShopId);
                            if (specialtyShop != null)
                            {
                                specialtyShop.Wallet += shopWalletAmount;
                                await _specialtyShopRepository.UpdateAsync(specialtyShop);
                                totalWalletAdded += shopWalletAmount;
                                Console.WriteLine($"Added {shopWalletAmount:N0} VNĐ to shop {specialtyShop.ShopName} wallet");
                            }
                        }
                    }
                    
                    await _productRepository.SaveChangesAsync();
                    await _specialtyShopRepository.SaveChangesAsync();
                    
                    Console.WriteLine($"Total wallet amount added to shops: {totalWalletAdded:N0} VNĐ");
                }
                else
                {
                    Console.WriteLine("Order already paid, skipping processing");
                }

                return Ok(new
                {
                    message = "Thanh toán thành công - Đã cập nhật trạng thái, trừ stock và cộng tiền vào ví shop",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status, // = 1
                    stockUpdated = true,
                    cartCleared = true,
                    walletUpdated = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PAID callback error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Có lỗi xảy ra khi xử lý thanh toán thành công.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// PayOS callback khi thanh toán bị hủy
        /// URL: /api/payment-callback/cancelled/{orderCode}
        /// Supports both string PayOsOrderCode (TNDT format) and GUID Order.Id
        /// Status = 2 (Cancelled) + KHÔNG trừ stock + KHÔNG xóa cart
        /// </summary>
        [HttpPost("cancelled/{orderCode}")]
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

                // Try to find by PayOsOrderCode first (now string with TNDT format)
                Console.WriteLine($"Looking for order with PayOsOrderCode: {orderCode}");
                order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == orderCode);

                // If not found, try parse as GUID Order.Id
                if (order == null && Guid.TryParse(orderCode, out Guid orderGuid))
                {
                    Console.WriteLine($"Looking for order with ID: {orderGuid}");
                    order = await _orderRepository.GetByIdAsync(orderGuid);
                }

                if (order == null)
                {
                    Console.WriteLine($"Order not found with orderCode: {orderCode}");
                    return NotFound($"Không tìm thấy đơn hàng với orderCode: {orderCode}");
                }

                Console.WriteLine($"Found order: {order.Id}, Current Status: {order.Status}");

                // Chỉ đổi status thành CANCELLED - KHÔNG trừ stock, KHÔNG xóa cart
                Console.WriteLine("Processing CANCELLED status...");
                order.Status = OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();
                Console.WriteLine("Order status updated to CANCELLED (status = 2) - Stock UNCHANGED");

                return Ok(new
                {
                    message = "Thanh toán đã bị hủy - Chỉ cập nhật trạng thái, KHÔNG trừ stock",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status, // = 2
                    stockUpdated = false,
                    cartCleared = false,
                    note = "Stock và cart được giữ nguyên"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CANCELLED callback error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Có lỗi xảy ra khi xử lý hủy thanh toán.",
                    error = ex.Message
                });
            }
        }
    }
}
