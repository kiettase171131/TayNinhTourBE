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
                Console.WriteLine($"Looking for order with PayOsOrderCode: '{orderCode}', Length: {orderCode.Length}");
                
                // Trim any whitespace and ensure clean comparison
                var cleanOrderCode = orderCode.Trim();
                Console.WriteLine($"Clean order code: '{cleanOrderCode}', Length: {cleanOrderCode.Length}");
                
                order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == cleanOrderCode && !x.IsDeleted, includes: new[] { "OrderDetails" });

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
                Console.WriteLine($"Looking for order with PayOsOrderCode: '{orderCode}', Length: {orderCode.Length}");
                
                // Trim any whitespace and ensure clean comparison
                var cleanOrderCode = orderCode.Trim();
                Console.WriteLine($"Clean order code: '{cleanOrderCode}', Length: {cleanOrderCode.Length}");
                
                order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == cleanOrderCode && !x.IsDeleted);

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
                Console.WriteLine($"Frontend Product Payment Success callback received for orderCode: '{orderCode}', Length: {orderCode.Length}");

                if (string.IsNullOrEmpty(orderCode))
                {
                    Console.WriteLine("Invalid orderCode: null or empty");
                    return BadRequest(new
                    {
                        success = false,
                        message = "OrderCode is required"
                    });
                }

                // Trim any whitespace and ensure clean comparison
                var cleanOrderCode = orderCode.Trim();
                Console.WriteLine($"Clean order code: '{cleanOrderCode}', Length: {cleanOrderCode.Length}");

                // Tìm order theo PayOsOrderCode hoặc Order.Id
                Console.WriteLine($"[HandleProductPaymentSuccess/Cancel] Using raw SQL to search for order with cleanCode: '{cleanOrderCode}'");
                var order = await _orderRepository.GetByPayOsOrderCodeRawSqlAsync(cleanOrderCode);
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
                Console.WriteLine($"Frontend Product Payment Cancel callback received for orderCode: '{orderCode}', Length: {orderCode.Length}");

                if (string.IsNullOrEmpty(orderCode))
                {
                    Console.WriteLine("Invalid orderCode: null or empty");
                    return BadRequest(new
                    {
                        success = false,
                        message = "OrderCode is required"
                    });
                }

                // Trim any whitespace and ensure clean comparison
                var cleanOrderCode = orderCode.Trim();
                Console.WriteLine($"Clean order code: '{cleanOrderCode}', Length: {cleanOrderCode.Length}");

                // Tìm order theo PayOsOrderCode hoặc Order.Id
                Console.WriteLine($"[HandleProductPaymentSuccess/Cancel] Using raw SQL to search for order with cleanCode: '{cleanOrderCode}'");
                var order = await _orderRepository.GetByPayOsOrderCodeRawSqlAsync(cleanOrderCode);
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

        /// <summary>
        /// DEBUG: Test endpoint to list all orders with PayOsOrderCode
        /// </summary>
        [HttpGet("debug/list-orders")]
        public async Task<IActionResult> DebugListOrders()
        {
            try
            {
                var allOrders = await _orderRepository.GetAllAsync(
                    o => o.PayOsOrderCode != null,
                    include: new[] { "OrderDetails" }
                );

                var ordersList = allOrders.Select(o => new
                {
                    o.Id,
                    o.PayOsOrderCode,
                    PayOsOrderCodeLength = o.PayOsOrderCode?.Length,
                    o.Status,
                    o.IsDeleted,
                    o.CreatedAt,
                    o.TotalAmount,
                    OrderDetailsCount = o.OrderDetails.Count
                }).ToList();

                return Ok(new
                {
                    totalOrders = ordersList.Count,
                    orders = ordersList.Take(20), // Show first 20 orders
                    searchingFor = "TNDT2469671853",
                    exactMatch = ordersList.Any(o => o.PayOsOrderCode == "TNDT2469671853"),
                    trimmedMatch = ordersList.Any(o => o.PayOsOrderCode?.Trim() == "TNDT2469671853"),
                    containsMatch = ordersList.Any(o => o.PayOsOrderCode?.Contains("TNDT2469671853") == true),
                    message = "DEBUG: List of orders with PayOsOrderCode"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// DEBUG: Test raw SQL to check column names and values
        /// </summary>
        [HttpGet("debug/raw-sql")]
        public async Task<IActionResult> DebugRawSql()
        {
            try
            {
                using var command = _orderRepository.GetDbConnection().CreateCommand();
                command.CommandText = @"
                    SELECT Id, PayOsOrderCode, IsDeleted, Status, TotalAmount 
                    FROM Orders 
                    WHERE PayOsOrderCode IS NOT NULL 
                    LIMIT 10";
                
                await _orderRepository.GetDbConnection().OpenAsync();
                
                var orders = new List<object>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(new
                    {
                        Id = reader.GetGuid(0),
                        PayOsOrderCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                        PayOsOrderCodeHex = reader.IsDBNull(1) ? null : BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(reader.GetString(1))),
                        IsDeleted = reader.GetBoolean(2),
                        Status = reader.GetInt32(3),
                        TotalAmount = reader.GetDecimal(4)
                    });
                }
                
                await _orderRepository.GetDbConnection().CloseAsync();
                
                // Also check for the specific order
                command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM Orders 
                    WHERE PayOsOrderCode = 'TNDT2469671853'";
                
                await _orderRepository.GetDbConnection().OpenAsync();
                var count = await command.ExecuteScalarAsync();
                await _orderRepository.GetDbConnection().CloseAsync();
                
                // Check with LIKE
                command.CommandText = @"
                    SELECT Id, PayOsOrderCode 
                    FROM Orders 
                    WHERE PayOsOrderCode LIKE '%2469671853%'";
                
                await _orderRepository.GetDbConnection().OpenAsync();
                var similarOrders = new List<object>();
                using var reader2 = await command.ExecuteReaderAsync();
                while (await reader2.ReadAsync())
                {
                    similarOrders.Add(new
                    {
                        Id = reader2.GetGuid(0),
                        PayOsOrderCode = reader2.IsDBNull(1) ? null : reader2.GetString(1)
                    });
                }
                await _orderRepository.GetDbConnection().CloseAsync();
                
                return Ok(new
                {
                    databaseName = "tayninhtourdb_local",
                    ordersInDatabase = orders,
                    countWithExactCode = count,
                    ordersWithSimilarCode = similarOrders,
                    searchingFor = "TNDT2469671853"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// DEBUG: Test direct database query
        /// </summary>
        [HttpGet("debug/find-order/{orderCode}")]
        public async Task<IActionResult> DebugFindOrder(string orderCode)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Searching for orderCode: '{orderCode}'");
                
                // Method 1: Direct LINQ query
                var order1 = await _orderRepository.GetFirstOrDefaultAsync(
                    o => o.PayOsOrderCode == orderCode && !o.IsDeleted
                );
                
                // Method 2: GetByPayOsOrderCodeAsync
                var order2 = await _orderRepository.GetByPayOsOrderCodeAsync(orderCode);
                
                // Method 3: Raw SQL
                var order3 = await _orderRepository.GetByPayOsOrderCodeRawSqlAsync(orderCode);
                
                // Method 4: Get all and filter in memory
                var allOrders = await _orderRepository.GetAllAsync(o => true);
                var order4 = allOrders.FirstOrDefault(o => o.PayOsOrderCode == orderCode && !o.IsDeleted);
                
                return Ok(new
                {
                    searchCode = orderCode,
                    searchCodeLength = orderCode.Length,
                    method1_directLinq = order1 != null ? new { order1.Id, order1.PayOsOrderCode } : null,
                    method2_repository = order2 != null ? new { order2.Id, order2.PayOsOrderCode } : null,
                    method3_rawSql = order3 != null ? new { order3.Id, order3.PayOsOrderCode } : null,
                    method4_inMemory = order4 != null ? new { order4.Id, order4.PayOsOrderCode } : null,
                    allOrdersCount = allOrders.Count(),
                    similarCodes = allOrders
                        .Where(o => o.PayOsOrderCode != null && o.PayOsOrderCode.Contains("2469671853"))
                        .Select(o => new { o.Id, o.PayOsOrderCode, PayOsOrderCodeLength = o.PayOsOrderCode?.Length, o.IsDeleted })
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
