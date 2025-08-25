using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller xử lý payment callbacks từ frontend cho product orders
    /// Enhanced với PaymentTransaction tracking và specialty shop wallet management
    /// </summary>
    [ApiController]
    [Route("api/product-payment")]
    public class ProductPaymentController : ControllerBase
    {
        private readonly ILogger<ProductPaymentController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductService _productService;
        private readonly IProductRepository _productRepository;
        private readonly ISpecialtyShopRepository _specialtyShopRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        // Commission rate for specialty shops (10% commission only, no VAT)
        private const decimal SHOP_COMMISSION_RATE = 0.10m;

        public ProductPaymentController(
            ILogger<ProductPaymentController> logger,
            IOrderRepository orderRepository,
            IProductService productService,
            IProductRepository productRepository,
            ISpecialtyShopRepository specialtyShopRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _productService = productService;
            _productRepository = productRepository;
            _specialtyShopRepository = specialtyShopRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Enhanced payment success callback
        /// URL: /api/product-payment/payment-success
        /// Logic: Order status = 1 (Paid), PaymentTransaction status = 1 (Paid), trừ stock, xóa cart, cộng tiền vào ví shop (trừ 10% hoa hồng)
        /// </summary>
        /// <param name="request">Thông tin callback từ frontend</param>
        /// <returns>Kết quả xử lý payment success</returns>
        [HttpPost("payment-success")]
        public async Task<IActionResult> HandlePaymentSuccess([FromBody] ProductPaymentCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Enhanced payment success callback for order: {OrderCode}, Code: {Code}, Status: {Status}", 
                    request.OrderCode, request.Code, request.Status);

                if (string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    _logger.LogWarning("Payment success callback received with empty order code");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order code is required"
                    });
                }

                // Optional: Validate PayOS success code
                if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != "00")
                {
                    _logger.LogWarning("Payment success callback received with non-success code: {Code}", request.Code);
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Invalid success code: {request.Code}. Expected '00' for successful payment."
                    });
                }

                // Tìm order bằng PayOsOrderCode hoặc Order.Id với OrderDetails
                Order? order = null;
                order = await _orderRepository.GetFirstOrDefaultAsync(
                    x => x.PayOsOrderCode == request.OrderCode, 
                    includes: new[] { "OrderDetails" });

                if (order == null && Guid.TryParse(request.OrderCode, out var orderId))
                {
                    order = await _orderRepository.GetByIdAsync(orderId, new[] { "OrderDetails" });
                }

                if (order == null)
                {
                    _logger.LogWarning("Order not found for orderCode: {OrderCode}", request.OrderCode);
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng"
                    });
                }

                _logger.LogInformation("Found order: {OrderId}, Current Status: {Status}", order.Id, order.Status);

                // Kiểm tra xem order đã được thanh toán chưa
                if (order.Status == OrderStatus.Paid)
                {
                    _logger.LogInformation("Order {OrderId} already paid, returning success", order.Id);
                    return Ok(new
                    {
                        success = true,
                        message = "Đơn hàng đã được thanh toán trước đó",
                        orderId = order.Id,
                        status = order.Status,
                        statusValue = (int)order.Status,
                        isAlreadyProcessed = true,
                        stockUpdated = true,
                        cartCleared = true,
                        walletUpdated = true,
                        paymentTransactionUpdated = true
                    });
                }

                // ENHANCED PAYMENT LOGIC
                _logger.LogInformation("Processing enhanced payment success for order: {OrderId}", order.Id);

                // 1. Update Order status to Paid (1)
                order.Status = OrderStatus.Paid;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();
                _logger.LogInformation("✅ Order status updated to PAID (status = 1)");

                // 2. Update PaymentTransaction status to Paid (1)
                var paymentTransaction = await _paymentTransactionRepository.GetByOrderIdAsync(order.Id);
                if (paymentTransaction != null)
                {
                    paymentTransaction.Status = PaymentStatus.Paid;
                    await _paymentTransactionRepository.UpdateAsync(paymentTransaction);
                    await _paymentTransactionRepository.SaveChangesAsync();
                    _logger.LogInformation("✅ PaymentTransaction status updated to PAID (status = 1)");
                }
                else
                {
                    _logger.LogWarning("⚠️ PaymentTransaction not found for order: {OrderId}", order.Id);
                }

                // 3. Clear cart and update inventory (reduce stock)
                await _productService.ClearCartAndUpdateInventoryAsync(order.Id);
                _logger.LogInformation("✅ Stock reduced and cart cleared");

                // 4. Add money to specialty shop wallets (minus 10% commission only)
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
                        var commissionAmount = itemTotalAmount * SHOP_COMMISSION_RATE; // 10% commission
                        var shopWalletAmount = itemTotalAmount - commissionAmount; // Shop gets 90%

                        var specialtyShop = await _specialtyShopRepository.GetByUserIdAsync(product.ShopId);
                        if (specialtyShop != null)
                        {
                            // Add 90% to shop wallet (after 10% commission deduction)
                            specialtyShop.Wallet += shopWalletAmount;
                            await _specialtyShopRepository.UpdateAsync(specialtyShop);

                            totalWalletAdded += shopWalletAmount;
                            totalCommissionDeducted += commissionAmount;

                            _logger.LogInformation("💰 Added {Amount:N0} VNĐ to shop '{ShopName}' wallet (commission: -{Commission:N0} VNĐ)", 
                                shopWalletAmount, specialtyShop.ShopName, commissionAmount);
                        }
                    }
                }

                await _productRepository.SaveChangesAsync();
                await _specialtyShopRepository.SaveChangesAsync();

                _logger.LogInformation("✅ Enhanced payment processing completed:");
                _logger.LogInformation("   💰 Total wallet amount added: {TotalAdded:N0} VNĐ", totalWalletAdded);
                _logger.LogInformation("   💸 Total commission deducted: {TotalCommission:N0} VNĐ", totalCommissionDeducted);

                return Ok(new
                {
                    success = true,
                    message = "Thanh toán sản phẩm thành công - Enhanced processing completed",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status,
                    stockUpdated = true,
                    cartCleared = true,
                    walletUpdated = true,
                    paymentTransactionUpdated = true,
                    walletSummary = new
                    {
                        totalWalletAdded = totalWalletAdded,
                        totalCommissionDeducted = totalCommissionDeducted,
                        commissionRate = "10%"
                    },
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
                _logger.LogError(ex, "❌ Error processing enhanced payment success for order: {OrderCode}", request.OrderCode);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xử lý thanh toán: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Enhanced payment cancel callback
        /// URL: /api/product-payment/payment-cancel
        /// Logic: Order status = 2 (Cancelled), PaymentTransaction status = 2 (Cancelled), KHÔNG trừ stock, KHÔNG xóa cart, KHÔNG cộng tiền vào ví
        /// </summary>
        /// <param name="request">Thông tin callback từ frontend</param>
        /// <returns>Kết quả xử lý payment cancel</returns>
        [HttpPost("payment-cancel")]
        public async Task<IActionResult> HandlePaymentCancel([FromBody] ProductPaymentCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Enhanced payment cancel callback for order: {OrderCode}", request.OrderCode);

                if (string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    _logger.LogWarning("Payment cancel callback received with empty order code");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order code is required"
                    });
                }

                // Tìm order bằng PayOsOrderCode hoặc Order.Id
                Order? order = null;
                order = await _orderRepository.GetByPayOsOrderCodeAsync(request.OrderCode);

                if (order == null && Guid.TryParse(request.OrderCode, out var orderId))
                {
                    order = await _orderRepository.GetByIdAsync(orderId);
                }

                if (order == null)
                {
                    _logger.LogWarning("Order not found for orderCode: {OrderCode}", request.OrderCode);
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng"
                    });
                }

                _logger.LogInformation("Found order: {OrderId}, Current Status: {Status}", order.Id, order.Status);

                // Sử dụng execution strategy để tránh conflict với MySQL retry strategy
                var strategy = _unitOfWork.GetExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // 1. Update Order status to Cancelled (2)
                        order.Status = OrderStatus.Cancelled;
                        await _orderRepository.UpdateAsync(order);
                        await _unitOfWork.SaveChangesAsync();
                        _logger.LogInformation("✅ Order status updated to CANCELLED (status = 2)");

                        // 2. Update PaymentTransaction status to Cancelled (2)
                        var paymentTransaction = await _paymentTransactionRepository.GetByOrderIdAsync(order.Id);
                        if (paymentTransaction != null)
                        {
                            paymentTransaction.Status = PaymentStatus.Cancelled;
                            await _paymentTransactionRepository.UpdateAsync(paymentTransaction);
                            await _paymentTransactionRepository.SaveChangesAsync();
                            _logger.LogInformation("✅ PaymentTransaction status updated to CANCELLED (status = 2)");
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ PaymentTransaction not found for order: {OrderId}", order.Id);
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                _logger.LogInformation("✅ Enhanced payment cancellation completed - NO stock reduction, NO cart clearing, NO wallet updates");

                return Ok(new
                {
                    success = true,
                    message = "Đã hủy thanh toán sản phẩm - Enhanced processing completed",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status,
                    stockUpdated = false,
                    cartCleared = false,
                    walletUpdated = false,
                    paymentTransactionUpdated = true,
                    note = "Stock và cart được giữ nguyên, không cộng tiền vào ví shop",
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
                _logger.LogError(ex, "❌ Error processing enhanced payment cancel for order: {OrderCode}", request.OrderCode);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xử lý hủy thanh toán: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lookup thông tin đơn hàng sản phẩm bằng PayOS order code
        /// URL: /api/product-payment/lookup/{payOsOrderCode}
        /// </summary>
        /// <param name="payOsOrderCode">PayOS order code</param>
        /// <returns>Thông tin đơn hàng</returns>
        [HttpGet("lookup/{payOsOrderCode}")]
        public async Task<IActionResult> LookupOrderByPayOsOrderCode(string payOsOrderCode)
        {
            try
            {
                _logger.LogInformation("Looking up order by PayOS order code: {PayOsOrderCode}", payOsOrderCode);

                if (string.IsNullOrWhiteSpace(payOsOrderCode))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "PayOS order code is required"
                    });
                }

                var order = await _orderRepository.GetByPayOsOrderCodeAsync(payOsOrderCode);

                if (order == null)
                {
                    _logger.LogWarning("Order not found for PayOS order code: {PayOsOrderCode}", payOsOrderCode);
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = order.Id,
                        payOsOrderCode = order.PayOsOrderCode,
                        totalAmount = order.TotalAmount,
                        totalAfterDiscount = order.TotalAfterDiscount,
                        discountAmount = order.DiscountAmount,
                        status = order.Status,
                        statusValue = (int)order.Status,
                        createdAt = order.CreatedAt,
                        userId = order.UserId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up order by PayOS order code: {PayOsOrderCode}", payOsOrderCode);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tra cứu đơn hàng: " + ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Request model cho product payment callbacks
    /// </summary>
    public class ProductPaymentCallbackRequest
    {
        public string OrderCode { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? Code { get; set; }
        public string? Id { get; set; }
        public bool? Cancel { get; set; }
    }
}
