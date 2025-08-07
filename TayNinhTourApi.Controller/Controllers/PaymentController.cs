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
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;

        // Commission rate for specialty shops (10%)
        private const decimal SHOP_COMMISSION_RATE = 0.10m;

        public PaymentController(
            IOrderRepository orderRepository, 
            IProductService productService, 
            IProductRepository productRepository, 
            ISpecialtyShopRepository specialtyShopRepository,
            IPaymentTransactionRepository paymentTransactionRepository)
        {
            _orderRepository = orderRepository;
            _productService = productService;
            _productRepository = productRepository;
            _specialtyShopRepository = specialtyShopRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
        }

        /// <summary>
        /// PayOS webhook callback khi thanh toán thành công cho product orders
        /// URL: /api/payment-callback/paid/{orderCode}
        /// Supports both string PayOsOrderCode (TNDT format) and GUID Order.Id
        /// Status = 1 (Paid) + Trừ stock + Xóa cart + Cộng tiền vào ví shop (sau khi trừ 10% commission) + Update PaymentTransaction status
        /// Follows PayOS best practices for webhook handling
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

                    // Update Order status to Paid
                    order.Status = OrderStatus.Paid;
                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();
                    Console.WriteLine("Order status updated to PAID (status = 1)");

                    // Update PaymentTransaction status to Paid
                    var paymentTransaction = await _paymentTransactionRepository.GetByOrderIdAsync(order.Id);
                    if (paymentTransaction != null)
                    {
                        paymentTransaction.Status = PaymentStatus.Paid;
                        await _paymentTransactionRepository.UpdateAsync(paymentTransaction);
                        await _paymentTransactionRepository.SaveChangesAsync();
                        Console.WriteLine("PaymentTransaction status updated to PAID (status = 1)");
                    }
                    else
                    {
                        Console.WriteLine("PaymentTransaction not found for this order");
                    }

                    Console.WriteLine("Calling ClearCartAndUpdateInventoryAsync...");
                    await _productService.ClearCartAndUpdateInventoryAsync(order.Id);
                    Console.WriteLine("Stock updated and cart cleared");
                    
                    var orderDetails = order.OrderDetails.ToList();
                    decimal totalWalletAdded = 0;
                    decimal totalCommissionDeducted = 0;
                    
                    foreach (var item in orderDetails)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            // Update sold count
                            product.SoldCount += item.Quantity;
                            await _productRepository.UpdateAsync(product);

                            // Calculate commission and shop amount
                            var itemTotalAmount = item.UnitPrice * item.Quantity;
                            var commissionAmount = itemTotalAmount * SHOP_COMMISSION_RATE;
                            var shopWalletAmount = itemTotalAmount - commissionAmount; // Shop gets 90%
                            
                            var specialtyShop = await _specialtyShopRepository.GetByUserIdAsync(product.ShopId);
                            if (specialtyShop != null)
                            {
                                // Add only 90% to shop wallet (after 10% commission deduction)
                                specialtyShop.Wallet += shopWalletAmount;
                                await _specialtyShopRepository.UpdateAsync(specialtyShop);
                                
                                totalWalletAdded += shopWalletAmount;
                                totalCommissionDeducted += commissionAmount;
                                
                                Console.WriteLine($"Added {shopWalletAmount:N0} VNĐ to shop {specialtyShop.ShopName} wallet (after {commissionAmount:N0} VNĐ commission)");
                            }
                        }
                    }
                    
                    await _productRepository.SaveChangesAsync();
                    await _specialtyShopRepository.SaveChangesAsync();
                    
                    Console.WriteLine($"Total wallet amount added to shops: {totalWalletAdded:N0} VNĐ");
                    Console.WriteLine($"Total commission deducted: {totalCommissionDeducted:N0} VNĐ");
                }
                else
                {
                    Console.WriteLine("Order already paid, skipping processing");
                }

                return Ok(new
                {
                    message = "Thanh toán thành công - Đã cập nhật trạng thái order và payment transaction, trừ stock và cộng tiền vào ví shop (sau khi trừ 10% hoa hồng)",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status, // = 1
                    stockUpdated = true,
                    cartCleared = true,
                    walletUpdated = true,
                    paymentTransactionUpdated = true,
                    commissionApplied = "10% commission deducted from shop revenue"
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
        /// Status = 2 (Cancelled) + KHÔNG trừ stock + KHÔNG xóa cart + Update PaymentTransaction status
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

                // Update PaymentTransaction status to Cancelled
                var paymentTransaction = await _paymentTransactionRepository.GetByOrderIdAsync(order.Id);
                if (paymentTransaction != null)
                {
                    paymentTransaction.Status = PaymentStatus.Cancelled;
                    await _paymentTransactionRepository.UpdateAsync(paymentTransaction);
                    await _paymentTransactionRepository.SaveChangesAsync();
                    Console.WriteLine("PaymentTransaction status updated to CANCELLED (status = 2)");
                }
                else
                {
                    Console.WriteLine("PaymentTransaction not found for this order");
                }

                return Ok(new
                {
                    message = "Thanh toán đã bị hủy - Chỉ cập nhật trạng thái order và payment transaction, KHÔNG trừ stock",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status, // = 2
                    stockUpdated = false,
                    cartCleared = false,
                    paymentTransactionUpdated = true,
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

        /// <summary>
        /// Frontend callback API cho product payment success (tương tự ProductPaymentController)
        /// URL: /api/payment-callback/payment-success/{orderCode}
        /// </summary>
        [HttpPost("payment-success/{orderCode}")]
        public async Task<IActionResult> HandleProductPaymentSuccess(string orderCode)
        {
            try
            {
                Console.WriteLine($"Frontend Product Payment Success callback received for orderCode: {orderCode}");

                if (string.IsNullOrEmpty(orderCode))
                {
                    Console.WriteLine("Invalid orderCode: null or empty");
                    return BadRequest(new
                    {
                        success = false,
                        message = "OrderCode is required"
                    });
                }

                // Tìm order theo PayOsOrderCode hoặc Order.Id
                var order = await _orderRepository.GetByPayOsOrderCodeAsync(orderCode);
                if (order == null)
                {
                    // Thử tìm theo GUID nếu không tìm thấy theo PayOsOrderCode
                    if (Guid.TryParse(orderCode, out var orderId))
                    {
                        order = await _orderRepository.GetByIdAsync(orderId);
                    }
                }

                if (order == null)
                {
                    Console.WriteLine($"Order not found with orderCode: {orderCode}");
                    return NotFound(new
                    {
                        success = false,
                        message = "Order not found"
                    });
                }

                Console.WriteLine($"Found order: {order.Id}, Current Status: {order.Status}");

                // Xử lý thanh toán nếu chưa được xử lý
                if (order.Status != OrderStatus.Paid)
                {
                    Console.WriteLine("Processing PAID status via frontend callback...");

                    // Update Order status to Paid
                    order.Status = OrderStatus.Paid;
                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();
                    Console.WriteLine("Order status updated to PAID (status = 1)");

                    // Update PaymentTransaction status to Paid
                    var paymentTransaction = await _paymentTransactionRepository.GetByOrderIdAsync(order.Id);
                    if (paymentTransaction != null)
                    {
                        paymentTransaction.Status = PaymentStatus.Paid;
                        await _paymentTransactionRepository.UpdateAsync(paymentTransaction);
                        await _paymentTransactionRepository.SaveChangesAsync();
                        Console.WriteLine("PaymentTransaction status updated to PAID (status = 1)");
                    }

                    Console.WriteLine("Calling ClearCartAndUpdateInventoryAsync...");
                    await _productService.ClearCartAndUpdateInventoryAsync(order.Id);
                    Console.WriteLine("Stock updated and cart cleared");

                    // Cộng tiền vào ví shop (sau khi trừ 10% commission)
                    var orderDetails = order.OrderDetails.ToList();
                    decimal totalWalletAdded = 0;
                    decimal totalCommissionDeducted = 0;

                    foreach (var orderDetail in orderDetails)
                    {
                        if (orderDetail.Product?.Shop?.SpecialtyShop != null)
                        {
                            var shop = orderDetail.Product.Shop.SpecialtyShop;
                            var orderDetailTotal = orderDetail.Quantity * orderDetail.UnitPrice;
                            var commission = orderDetailTotal * SHOP_COMMISSION_RATE;
                            var shopRevenue = orderDetailTotal - commission;

                            shop.Wallet += shopRevenue;
                            await _specialtyShopRepository.UpdateAsync(shop);

                            totalWalletAdded += shopRevenue;
                            totalCommissionDeducted += commission;

                            Console.WriteLine($"Shop {shop.ShopName}: +{shopRevenue:N0} VNĐ (commission: -{commission:N0} VNĐ)");
                        }
                    }

                    await _productRepository.SaveChangesAsync();
                    await _specialtyShopRepository.SaveChangesAsync();

                    Console.WriteLine($"Total wallet amount added to shops: {totalWalletAdded:N0} VNĐ");
                    Console.WriteLine($"Total commission deducted: {totalCommissionDeducted:N0} VNĐ");
                }
                else
                {
                    Console.WriteLine("Order already paid, skipping processing");
                }

                return Ok(new
                {
                    success = true,
                    message = "Thanh toán sản phẩm thành công",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status,
                    stockUpdated = true,
                    cartCleared = true,
                    walletUpdated = true,
                    paymentTransactionUpdated = true,
                    orderData = new
                    {
                        id = order.Id,
                        payOsOrderCode = order.PayOsOrderCode,
                        totalAmount = order.TotalAmount,
                        totalAfterDiscount = order.TotalAfterDiscount,
                        discountAmount = order.DiscountAmount,
                        createdAt = order.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Frontend Product Payment Success callback error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xử lý thanh toán thành công: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Frontend callback API cho product payment cancel
        /// URL: /api/payment-callback/payment-cancel/{orderCode}
        /// </summary>
        [HttpPost("payment-cancel/{orderCode}")]
        public async Task<IActionResult> HandleProductPaymentCancel(string orderCode)
        {
            try
            {
                Console.WriteLine($"Frontend Product Payment Cancel callback received for orderCode: {orderCode}");

                if (string.IsNullOrEmpty(orderCode))
                {
                    Console.WriteLine("Invalid orderCode: null or empty");
                    return BadRequest(new
                    {
                        success = false,
                        message = "OrderCode is required"
                    });
                }

                // Tìm order theo PayOsOrderCode hoặc Order.Id
                var order = await _orderRepository.GetByPayOsOrderCodeAsync(orderCode);
                if (order == null)
                {
                    // Thử tìm theo GUID nếu không tìm thấy theo PayOsOrderCode
                    if (Guid.TryParse(orderCode, out var orderId))
                    {
                        order = await _orderRepository.GetByIdAsync(orderId);
                    }
                }

                if (order == null)
                {
                    Console.WriteLine($"Order not found with orderCode: {orderCode}");
                    return NotFound(new
                    {
                        success = false,
                        message = "Order not found"
                    });
                }

                Console.WriteLine($"Found order: {order.Id}, Current Status: {order.Status}");

                // Cập nhật status = Cancelled nếu chưa được xử lý
                if (order.Status == OrderStatus.Pending)
                {
                    Console.WriteLine("Processing CANCELLED status via frontend callback...");

                    order.Status = OrderStatus.Cancelled;
                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();
                    Console.WriteLine("Order status updated to CANCELLED (status = 2)");

                    // Update PaymentTransaction status to Cancelled
                    var paymentTransaction = await _paymentTransactionRepository.GetByOrderIdAsync(order.Id);
                    if (paymentTransaction != null)
                    {
                        paymentTransaction.Status = PaymentStatus.Cancelled;
                        await _paymentTransactionRepository.UpdateAsync(paymentTransaction);
                        await _paymentTransactionRepository.SaveChangesAsync();
                        Console.WriteLine("PaymentTransaction status updated to CANCELLED (status = 2)");
                    }

                    // KHÔNG trừ stock, KHÔNG xóa cart khi cancel
                }
                else
                {
                    Console.WriteLine($"Order already processed with status: {order.Status}");
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã hủy thanh toán sản phẩm",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status,
                    stockUpdated = false,
                    cartCleared = false,
                    walletUpdated = false,
                    paymentTransactionUpdated = true,
                    orderData = new
                    {
                        id = order.Id,
                        payOsOrderCode = order.PayOsOrderCode,
                        totalAmount = order.TotalAmount,
                        totalAfterDiscount = order.TotalAfterDiscount,
                        discountAmount = order.DiscountAmount,
                        createdAt = order.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Frontend Product Payment Cancel callback error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xử lý hủy thanh toán: " + ex.Message
                });
            }
        }
    }
}
