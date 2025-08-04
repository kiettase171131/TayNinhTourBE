using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Enhanced Payment Controller tương tự như PaymentController trong Java Spring Boot
    /// Cung cấp các API để quản lý thanh toán qua PayOS với transaction tracking
    /// </summary>
    [ApiController]
    [Route("api/enhanced-payments")]
    [Tags("Enhanced Payment API")]
    public class EnhancedPaymentController : ControllerBase
    {
        private readonly IPayOsService _payOsService;
        private readonly ILogger<EnhancedPaymentController> _logger;

        public EnhancedPaymentController(IPayOsService payOsService, ILogger<EnhancedPaymentController> logger)
        {
            _payOsService = payOsService;
            _logger = logger;
        }

        /// <summary>
        /// Xác nhận webhook PayOS
        /// Tương tự như confirmWebhook trong Java code
        /// </summary>
        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook()
        {
            try
            {
                var webhookUrl = HttpContext.Request.Headers["X-Webhook-Url"].FirstOrDefault() 
                    ?? "https://your-domain.com/api/enhanced-payments/webhook/payos";
                
                var confirmedUrl = await _payOsService.ConfirmWebhookAsync(webhookUrl);
                
                return Ok(new
                {
                    success = true,
                    message = "✅ Webhook đã được xác nhận",
                    confirmedUrl = confirmedUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming webhook");
                return BadRequest(new
                {
                    success = false,
                    message = "❌ Lỗi xác nhận webhook PayOS: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Webhook từ PayOS
        /// Endpoint nhận dữ liệu webhook từ PayOS khi trạng thái thanh toán thay đổi
        /// </summary>
        [HttpPost("webhook/payos")]
        public async Task<IActionResult> HandleWebhook([FromBody] object body)
        {
            try
            {
                _logger.LogInformation("Received PayOS webhook");
                
                var transaction = await _payOsService.HandlePayOsWebhookAsync(body);
                
                return Ok(new
                {
                    success = true,
                    message = "Webhook processed successfully",
                    transactionId = transaction.Id,
                    status = transaction.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Tạo link thanh toán PayOS
        /// Tương tự như createPaymentLink trong Java code
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value!.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new 
                    { 
                        success = false,
                        message = "Dữ liệu không hợp lệ", 
                        errors = errors 
                    });
                }

                var transaction = await _payOsService.CreatePaymentLinkAsync(request);
                
                var response = new PaymentLinkResponseDto
                {
                    TransactionId = transaction.Id,
                    PayOsOrderCode = transaction.PayOsOrderCode ?? 0,
                    CheckoutUrl = transaction.CheckoutUrl ?? "",
                    QrCode = transaction.QrCode,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    Status = transaction.Status,
                    ExpiredAt = transaction.ExpiredAt,
                    CreatedAt = transaction.CreatedAt
                };

                return Ok(new
                {
                    success = true,
                    message = "Tạo link thanh toán thành công!",
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment link");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Thử lại thanh toán
        /// Tương tự như retryPayment trong Java code
        /// </summary>
        [HttpPost("retry")]
        [Authorize]
        public async Task<IActionResult> RetryPayment([FromBody] RetryPaymentRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ"
                    });
                }

                var transaction = await _payOsService.RetryPaymentAsync(request);
                
                var response = new PaymentLinkResponseDto
                {
                    TransactionId = transaction.Id,
                    PayOsOrderCode = transaction.PayOsOrderCode ?? 0,
                    CheckoutUrl = transaction.CheckoutUrl ?? "",
                    QrCode = transaction.QrCode,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    Status = transaction.Status,
                    ExpiredAt = transaction.ExpiredAt,
                    CreatedAt = transaction.CreatedAt
                };

                return Ok(new
                {
                    success = true,
                    message = "Tạo link thanh toán thử lại thành công!",
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying payment");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách giao dịch theo orderId
        /// </summary>
        [HttpGet("orders/{orderId}/transactions")]
        [Authorize]
        public async Task<IActionResult> GetTransactionsByOrderId(Guid orderId)
        {
            try
            {
                var transactions = await _payOsService.GetTransactionsByOrderIdAsync(orderId);
                
                return Ok(new
                {
                    success = true,
                    message = "Danh sách giao dịch theo orderId",
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions by order ID");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách giao dịch theo tourBookingId
        /// </summary>
        [HttpGet("tour-bookings/{tourBookingId}/transactions")]
        [Authorize]
        public async Task<IActionResult> GetTransactionsByTourBookingId(Guid tourBookingId)
        {
            try
            {
                var transactions = await _payOsService.GetTransactionsByTourBookingIdAsync(tourBookingId);
                
                return Ok(new
                {
                    success = true,
                    message = "Danh sách giao dịch theo tourBookingId",
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions by tour booking ID");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Huỷ tất cả giao dịch đang chờ thanh toán
        /// Tương tự như cancelAllPendingTransactions trong Java code
        /// </summary>
        [HttpPost("cancel")]
        [Authorize]
        public async Task<IActionResult> CancelAllPendingTransactions([FromBody] CancelPaymentRequestDto request)
        {
            try
            {
                var result = await _payOsService.CancelAllPendingTransactionsAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Cancel payment success",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling pending transactions");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả Product Payment transactions
        /// </summary>
        [HttpGet("product-payments")]
        [Authorize]
        public async Task<IActionResult> GetProductPayments([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
        {
            try
            {
                var transactions = await _payOsService.GetProductPaymentTransactionsAsync(pageIndex, pageSize);

                return Ok(new
                {
                    success = true,
                    message = "Danh sách Product Payment transactions",
                    data = transactions,
                    pagination = new { pageIndex, pageSize, total = transactions.Count }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product payment transactions");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả Tour Booking Payment transactions
        /// </summary>
        [HttpGet("tour-booking-payments")]
        [Authorize]
        public async Task<IActionResult> GetTourBookingPayments([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
        {
            try
            {
                var transactions = await _payOsService.GetTourBookingPaymentTransactionsAsync(pageIndex, pageSize);

                return Ok(new
                {
                    success = true,
                    message = "Danh sách Tour Booking Payment transactions",
                    data = transactions,
                    pagination = new { pageIndex, pageSize, total = transactions.Count }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour booking payment transactions");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Kiểm tra loại thanh toán của một transaction
        /// </summary>
        [HttpGet("transactions/{transactionId}/type")]
        [Authorize]
        public async Task<IActionResult> GetPaymentType(Guid transactionId)
        {
            try
            {
                var paymentType = await _payOsService.GetPaymentTypeAsync(transactionId);

                return Ok(new
                {
                    success = true,
                    message = "Payment type retrieved successfully",
                    data = new
                    {
                        transactionId = transactionId,
                        paymentType = paymentType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment type for transaction {TransactionId}", transactionId);
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy transaction theo PayOS order code
        /// </summary>
        [HttpGet("transaction/order-code/{orderCode}")]
        public async Task<IActionResult> GetTransactionByOrderCode(string orderCode)
        {
            try
            {
                var transaction = await _payOsService.GetTransactionByOrderCodeAsync(orderCode);
                if (transaction == null)
                {
                    return NotFound(new { success = false, message = "Transaction không tồn tại" });
                }

                return Ok(new { success = true, data = transaction });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction by order code {OrderCode}", orderCode);
                return StatusCode(500, new { success = false, message = "Lỗi server khi lấy transaction" });
            }
        }

        /// <summary>
        /// Xử lý webhook callback
        /// </summary>
        [HttpPost("webhook/callback")]
        public async Task<IActionResult> ProcessWebhookCallback([FromBody] JsonElement request)
        {
            try
            {
                string orderCode = request.GetProperty("orderCode").GetString() ?? "";
                string status = request.GetProperty("status").GetString() ?? "";

                var result = await _payOsService.ProcessWebhookCallbackAsync(orderCode, status);

                return Ok(new { success = true, message = "Webhook callback processed successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook callback");
                return StatusCode(500, new { success = false, message = "Lỗi server khi xử lý webhook" });
            }
        }
    }
}
