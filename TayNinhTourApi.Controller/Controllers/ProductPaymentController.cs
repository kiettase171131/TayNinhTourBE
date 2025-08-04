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
    /// Tách riêng từ PaymentController để handle product payment logic riêng biệt
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductPaymentController : ControllerBase
    {
        private readonly ILogger<ProductPaymentController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;

        public ProductPaymentController(
            ILogger<ProductPaymentController> logger,
            IOrderRepository orderRepository,
            IProductService productService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _productService = productService;
        }

        /// <summary>
        /// Frontend callback khi user được redirect về từ PayOS sau khi thanh toán thành công
        /// URL: /api/product-payment/payment-success
        /// </summary>
        /// <param name="request">Thông tin callback từ frontend</param>
        /// <returns>Kết quả xử lý payment success</returns>
        [HttpPost("payment-success")]
        public async Task<IActionResult> HandlePaymentSuccess([FromBody] ProductPaymentCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received product payment success callback for order: {OrderCode}", request.OrderCode);

                if (string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    _logger.LogWarning("Product payment success callback received with empty order code");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order code is required"
                    });
                }

                // Tìm order bằng PayOsOrderCode hoặc Order.Id
                Order? order = null;

                // Thử tìm bằng PayOsOrderCode trước
                order = await _orderRepository.GetByPayOsOrderCodeAsync(request.OrderCode);

                // Nếu không tìm thấy, thử parse làm GUID và tìm bằng Order.Id
                if (order == null && Guid.TryParse(request.OrderCode, out var orderId))
                {
                    order = await _orderRepository.GetByIdAsync(orderId);
                }

                if (order == null)
                {
                    _logger.LogWarning("Product order not found for orderCode: {OrderCode}", request.OrderCode);
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng"
                    });
                }

                _logger.LogInformation("Found product order: {OrderId}, Current Status: {Status}", order.Id, order.Status);

                // Kiểm tra xem order đã được thanh toán chưa
                if (order.Status == OrderStatus.Paid)
                {
                    _logger.LogInformation("Product order {OrderId} already paid, returning success", order.Id);
                    return Ok(new
                    {
                        success = true,
                        message = "Đơn hàng đã được thanh toán trước đó",
                        orderId = order.Id,
                        status = order.Status,
                        statusValue = (int)order.Status,
                        isAlreadyProcessed = true,
                        stockUpdated = true,
                        cartCleared = true
                    });
                }

                // Xử lý thanh toán thành công
                order.Status = OrderStatus.Paid;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();

                _logger.LogInformation("Updated product order {OrderId} status to PAID", order.Id);

                // Gọi service để clear cart và update inventory
                await _productService.ClearCartAndUpdateInventoryAsync(order.Id);
                _logger.LogInformation("Cleared cart and updated inventory for order {OrderId}", order.Id);

                return Ok(new
                {
                    success = true,
                    message = "Thanh toán sản phẩm thành công",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status,
                    stockUpdated = true,
                    cartCleared = true,
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
                _logger.LogError(ex, "Error processing product payment success for order: {OrderCode}", request.OrderCode);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xử lý thanh toán: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Frontend callback khi user được redirect về từ PayOS sau khi hủy thanh toán
        /// URL: /api/product-payment/payment-cancel
        /// </summary>
        /// <param name="request">Thông tin callback từ frontend</param>
        /// <returns>Kết quả xử lý payment cancel</returns>
        [HttpPost("payment-cancel")]
        public async Task<IActionResult> HandlePaymentCancel([FromBody] ProductPaymentCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received product payment cancel callback for order: {OrderCode}", request.OrderCode);

                if (string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    _logger.LogWarning("Product payment cancel callback received with empty order code");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order code is required"
                    });
                }

                // Tìm order bằng PayOsOrderCode hoặc Order.Id
                Order? order = null;

                // Thử tìm bằng PayOsOrderCode trước
                order = await _orderRepository.GetByPayOsOrderCodeAsync(request.OrderCode);

                // Nếu không tìm thấy, thử parse làm GUID và tìm bằng Order.Id
                if (order == null && Guid.TryParse(request.OrderCode, out var orderId))
                {
                    order = await _orderRepository.GetByIdAsync(orderId);
                }

                if (order == null)
                {
                    _logger.LogWarning("Product order not found for orderCode: {OrderCode}", request.OrderCode);
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng"
                    });
                }

                _logger.LogInformation("Found product order: {OrderId}, Current Status: {Status}", order.Id, order.Status);

                // Sử dụng execution strategy để tránh conflict với MySQL retry strategy
                var strategy = _unitOfWork.GetExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Cập nhật status thành Cancelled (không trừ stock, không xóa cart)
                        order.Status = OrderStatus.Cancelled;
                        await _orderRepository.UpdateAsync(order);
                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                _logger.LogInformation("Updated product order {OrderId} status to CANCELLED", order.Id);

                return Ok(new
                {
                    success = true,
                    message = "Đã hủy thanh toán sản phẩm",
                    orderId = order.Id,
                    status = order.Status,
                    statusValue = (int)order.Status,
                    stockUpdated = false,
                    cartCleared = false,
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
                _logger.LogError(ex, "Error processing product payment cancel for order: {OrderCode}", request.OrderCode);
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
                _logger.LogInformation("Looking up product order by PayOS order code: {PayOsOrderCode}", payOsOrderCode);

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
                    _logger.LogWarning("Product order not found for PayOS order code: {PayOsOrderCode}", payOsOrderCode);
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
                _logger.LogError(ex, "Error looking up product order by PayOS order code: {PayOsOrderCode}", payOsOrderCode);
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
