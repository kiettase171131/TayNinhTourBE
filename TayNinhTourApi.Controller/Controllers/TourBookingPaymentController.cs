using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller xử lý payment callbacks cho tour booking
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TourBookingPaymentController : ControllerBase
    {
        private readonly IUserTourBookingService _userTourBookingService;
        private readonly ILogger<TourBookingPaymentController> _logger;

        public TourBookingPaymentController(
            IUserTourBookingService userTourBookingService,
            ILogger<TourBookingPaymentController> logger)
        {
            _userTourBookingService = userTourBookingService;
            _logger = logger;
        }

        /// <summary>
        /// Webhook callback khi thanh toán thành công từ PayOS
        /// </summary>
        /// <param name="request">Thông tin callback từ PayOS</param>
        /// <returns>Kết quả xử lý</returns>
        [HttpPost("payment-success")]
        public async Task<IActionResult> HandlePaymentSuccess([FromBody] PaymentCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received payment success callback for order: {OrderCode}", request.OrderCode);

                if (string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    _logger.LogWarning("Payment success callback received with empty order code");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order code is required"
                    });
                }

                var result = await _userTourBookingService.HandlePaymentSuccessAsync(request.OrderCode);

                if (result.StatusCode != 200)
                {
                    _logger.LogError("Failed to process payment success for order {OrderCode}: {Message}", 
                        request.OrderCode, result.Message);
                    
                    return StatusCode(result.StatusCode, new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                _logger.LogInformation("Successfully processed payment success for order: {OrderCode}", request.OrderCode);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    orderCode = request.OrderCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment success callback for order: {OrderCode}", request.OrderCode);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while processing payment success",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Webhook callback khi thanh toán bị hủy từ PayOS
        /// </summary>
        /// <param name="request">Thông tin callback từ PayOS</param>
        /// <returns>Kết quả xử lý</returns>
        [HttpPost("payment-cancel")]
        public async Task<IActionResult> HandlePaymentCancel([FromBody] PaymentCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received payment cancel callback for order: {OrderCode}", request.OrderCode);

                if (string.IsNullOrWhiteSpace(request.OrderCode))
                {
                    _logger.LogWarning("Payment cancel callback received with empty order code");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order code is required"
                    });
                }

                var result = await _userTourBookingService.HandlePaymentCancelAsync(request.OrderCode);

                if (result.StatusCode != 200)
                {
                    _logger.LogError("Failed to process payment cancel for order {OrderCode}: {Message}", 
                        request.OrderCode, result.Message);
                    
                    return StatusCode(result.StatusCode, new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                _logger.LogInformation("Successfully processed payment cancel for order: {OrderCode}", request.OrderCode);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    orderCode = request.OrderCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment cancel callback for order: {OrderCode}", request.OrderCode);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while processing payment cancel",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// PayOS webhook callback khi thanh toán thành công (server-to-server)
        /// URL: /api/tour-booking-payment/webhook/paid/{orderCode}
        /// Tương tự như product payment - đơn giản, không cần signature verification
        /// </summary>
        /// <param name="orderCode">PayOS order code</param>
        /// <returns>Kết quả xử lý</returns>
        [HttpPost("webhook/paid/{orderCode}")]
        public async Task<IActionResult> PayOsWebhookPaid(string orderCode)
        {
            try
            {
                _logger.LogInformation("PayOS webhook PAID callback received for tour booking order: {OrderCode}", orderCode);

                if (string.IsNullOrWhiteSpace(orderCode))
                {
                    _logger.LogWarning("PayOS webhook PAID callback received with empty order code");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order code is required"
                    });
                }

                var result = await _userTourBookingService.HandlePaymentSuccessAsync(orderCode);

                if (result.StatusCode != 200)
                {
                    _logger.LogError("Failed to process PayOS webhook PAID for tour booking {OrderCode}: {Message}",
                        orderCode, result.Message);

                    return StatusCode(result.StatusCode, new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                _logger.LogInformation("Successfully processed PayOS webhook PAID for tour booking: {OrderCode}", orderCode);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    orderCode = orderCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook PAID callback for tour booking: {OrderCode}", orderCode);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while processing PayOS webhook PAID",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// PayOS webhook callback khi thanh toán bị hủy (server-to-server)
        /// URL: /api/tour-booking-payment/webhook/cancelled/{orderCode}
        /// Tương tự như product payment - đơn giản, không cần signature verification
        /// </summary>
        /// <param name="orderCode">PayOS order code</param>
        /// <returns>Kết quả xử lý</returns>
        [HttpPost("webhook/cancelled/{orderCode}")]
        public async Task<IActionResult> PayOsWebhookCancelled(string orderCode)
        {
            try
            {
                _logger.LogInformation("PayOS webhook CANCELLED callback received for tour booking order: {OrderCode}", orderCode);

                if (string.IsNullOrWhiteSpace(orderCode))
                {
                    _logger.LogWarning("PayOS webhook CANCELLED callback received with empty order code");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Order code is required"
                    });
                }

                var result = await _userTourBookingService.HandlePaymentCancelAsync(orderCode);

                if (result.StatusCode != 200)
                {
                    _logger.LogError("Failed to process PayOS webhook CANCELLED for tour booking {OrderCode}: {Message}",
                        orderCode, result.Message);

                    return StatusCode(result.StatusCode, new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                _logger.LogInformation("Successfully processed PayOS webhook CANCELLED for tour booking: {OrderCode}", orderCode);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    orderCode = orderCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook CANCELLED callback for tour booking: {OrderCode}", orderCode);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while processing PayOS webhook CANCELLED",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lookup booking information by PayOS order code (for frontend)
        /// </summary>
        /// <param name="payOsOrderCode">PayOS order code</param>
        /// <returns>Booking information</returns>
        [HttpGet("lookup/{payOsOrderCode}")]
        public async Task<IActionResult> LookupBookingByPayOsOrderCode(string payOsOrderCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(payOsOrderCode))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "PayOS order code is required"
                    });
                }

                // This would need to be implemented in the service
                // For now, return a placeholder response
                return Ok(new
                {
                    success = true,
                    message = "Lookup functionality to be implemented",
                    payOsOrderCode = payOsOrderCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up booking by PayOS order code: {PayOsOrderCode}", payOsOrderCode);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while looking up booking",
                    error = ex.Message
                });
            }
        }


    }

    /// <summary>
    /// DTO cho payment callback request từ PayOS
    /// </summary>
    public class PaymentCallbackRequest
    {
        public string OrderCode { get; set; } = string.Empty;
        public string? Status { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public DateTime? TransactionDateTime { get; set; }
        public string? Reference { get; set; }
    }


}
