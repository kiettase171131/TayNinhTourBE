using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class PayOsService : IPayOsService
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PayOsService> _logger;

        public PayOsService(IConfiguration config, IUnitOfWork unitOfWork, ILogger<PayOsService> logger)
        {
            _config = config;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        /// <summary>
        /// Tạo PayOS payment URL cho product orders với webhook URLs
        /// Follows PayOS best practices with proper error handling and logging
        /// DEPRECATED: Use CreatePaymentLinkAsync instead
        /// </summary>
        // public async Task<string?> CreatePaymentUrlAsync(decimal amount, string orderCode, string baseUrl)
        /*
        {
            try
            {
                var clientId = _config["PayOS:ClientId"];
                var apiKey = _config["PayOS:ApiKey"];
                var checksumKey = _config["PayOS:ChecksumKey"];

                // Validate PayOS configuration
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
                {
                    throw new InvalidOperationException("PayOS configuration is incomplete. Please check ClientId, ApiKey, and ChecksumKey.");
                }

                List<ItemData> items = new List<ItemData>();
                PayOS payOS = new PayOS(clientId, apiKey, checksumKey);

                // orderCode đã được truyền vào với format TNDT + 10 số
                var payOsOrderCodeString = orderCode;

                // PayOS yêu cầu orderCode phải là số, nên chỉ lấy phần số từ orderCode (loại bỏ "TNDT")
                var numericPart = orderCode.StartsWith("TNDT") ? orderCode.Substring(4) : orderCode;
                if (!long.TryParse(numericPart, out var orderCode2))
                {
                    throw new ArgumentException($"Invalid order code format: {orderCode}. Expected TNDT followed by numeric value.");
                }

                var orderCodeDisplay = payOsOrderCodeString;
                // Fix: Rút ngắn description để phù hợp với giới hạn 25 ký tự của PayOS
                PaymentData paymentData = new PaymentData(
                 orderCode: orderCode2,
                 amount: (int)amount,
                 description: $"{orderCodeDisplay}",
                 items: items,
                 cancelUrl: $"https://tndt.netlify.app/payment-cancel?orderId={orderCode}&orderCode={payOsOrderCodeString}",
                 returnUrl: $"https://tndt.netlify.app/payment-success?orderId={orderCode}&orderCode={payOsOrderCodeString}",
                 buyerName: "Product Customer");

                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                // Log successful payment URL creation
                Console.WriteLine($"PayOS product payment URL created successfully for order {orderCode}: {createPayment.checkoutUrl}");

                return createPayment.checkoutUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating PayOS product payment URL for order {orderCode}: {ex.Message}");
                throw; // Re-throw to let caller handle the error appropriately
            }
        }
        */

        /// <summary>
        /// Tạo PayOS payment URL cho tour booking với webhook URLs
        /// Follows PayOS best practices with proper error handling and logging
        /// DEPRECATED: Use CreatePaymentLinkAsync instead
        /// </summary>
        // public async Task<string?> CreateTourBookingPaymentUrlAsync(decimal amount, string orderCode, string baseUrl)
        /*
        {
            try
            {
                var clientId = _config["PayOS:ClientId"];
                var apiKey = _config["PayOS:ApiKey"];
                var checksumKey = _config["PayOS:ChecksumKey"];

                // Validate PayOS configuration
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
                {
                    throw new InvalidOperationException("PayOS configuration is incomplete. Please check ClientId, ApiKey, and ChecksumKey.");
                }

                List<ItemData> items = new List<ItemData>();
                PayOS payOS = new PayOS(clientId, apiKey, checksumKey);

                // orderCode đã được truyền vào với format TNDT + 10 số
                var payOsOrderCodeString = orderCode;

                // PayOS yêu cầu orderCode phải là số, nên chỉ lấy phần số từ orderCode (loại bỏ "TNDT")
                var numericPart = orderCode.StartsWith("TNDT") ? orderCode.Substring(4) : orderCode;
                if (!long.TryParse(numericPart, out var orderCode2))
                {
                    throw new ArgumentException($"Invalid order code format: {orderCode}. Expected TNDT followed by numeric value.");
                }

                var orderCodeDisplay = payOsOrderCodeString;
                // Fix: Rút ngắn description để phù hợp với giới hạn 25 ký tự của PayOS
                PaymentData paymentData = new PaymentData(
                 orderCode: orderCode2,
                 amount: (int)amount,
                 description: $"Tour {orderCodeDisplay}",
                 items: items,
                 cancelUrl: $"https://tndt.netlify.app/payment-cancel?orderId={orderCode}&orderCode={payOsOrderCodeString}",
                 returnUrl: $"https://tndt.netlify.app/payment-success?orderId={orderCode}&orderCode={payOsOrderCodeString}",
                 buyerName: "Tour Customer");

                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                // Log successful payment URL creation
                Console.WriteLine($"PayOS tour booking payment URL created successfully for order {orderCode}: {createPayment.checkoutUrl}");

                return createPayment.checkoutUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating PayOS tour booking payment URL for order {orderCode}: {ex.Message}");
                throw; // Re-throw to let caller handle the error appropriately
            }
        }
        */
        public async Task<OrderStatus> GetOrderPaymentStatusAsync(string orderCode)
        {
            var clientId = _config["PayOS:ClientId"];
            var apiKey = _config["PayOS:ApiKey"];
            var url = $"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-client-id", clientId);
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Không lấy được trạng thái thanh toán từ PayOS");

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);
                var statusStr = json["data"]?["status"]?.ToString();

                return statusStr switch
                {
                    "PAID" => OrderStatus.Paid,
                    "CANCELLED" => OrderStatus.Cancelled,
                    _ => OrderStatus.Pending
                };
            }
        }



        //public async Task<string> VerifyPaymentStatusAsync(PayOsStatusResponseDto dto)
        //{
        //    if (dto.RawQueryCollection == null || dto.Code == "01")
        //        return "Duong dan tra ve khong hop ly";
        //    var orderCode = dto.OrderCode.ToString();

        //}

        #region Enhanced PayOS Methods (Similar to Java Spring Boot)

        /// <summary>
        /// Tạo payment link với PaymentTransaction tracking
        /// Tương tự như createPaymentLink trong Java code
        /// </summary>
        public async Task<PaymentTransaction> CreatePaymentLinkAsync(CreatePaymentRequestDto request)
        {
            try
            {
                var clientId = _config["PayOS:ClientId"];
                var apiKey = _config["PayOS:ApiKey"];
                var checksumKey = _config["PayOS:ChecksumKey"];

                // Validate PayOS configuration
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
                {
                    throw new InvalidOperationException("PayOS configuration is incomplete.");
                }

                // Generate TNDT format order code
                var tndtOrderCode = PayOsOrderCodeUtility.GeneratePayOsOrderCode();
                var numericOrderCode = PayOsOrderCodeUtility.ExtractNumericPart(tndtOrderCode);

                var item = new ItemData("Thanh toán đơn hàng", 1, (int)request.Amount);
                var items = new List<ItemData> { item };

                // PayOS requires description to be max 25 characters
                var description = request.Description ?? "Thanh toán";
                if (description.Length > 25)
                {
                    description = description.Substring(0, 25);
                }

                var paymentData = new PaymentData(
                    orderCode: numericOrderCode, // PayOS API requires numeric
                    amount: (int)request.Amount,
                    description: description,
                    items: items,
                    cancelUrl: $"{_config["PayOS:CancelUrl"]}?orderCode={tndtOrderCode}",
                    returnUrl: $"{_config["PayOS:ReturnUrl"]}?orderCode={tndtOrderCode}"
                );

                var payOS = new PayOS(clientId, apiKey, checksumKey);
                var response = await payOS.createPaymentLink(paymentData);

                // Create PaymentTransaction record
                var transaction = new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    OrderId = request.OrderId,
                    TourBookingId = request.TourBookingId,
                    Amount = request.Amount,
                    Status = PaymentStatus.Pending,
                    Description = request.Description,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(15),
                    Gateway = PaymentGateway.PayOS,
                    PayOsOrderCode = tndtOrderCode, // Store TNDT format
                    PayOsTransactionId = response.paymentLinkId,
                    CheckoutUrl = response.checkoutUrl,
                    QrCode = response.qrCode
                };

                await _unitOfWork.PaymentTransactionRepository.AddAsync(transaction);

                // Save PaymentTransaction first to ensure it's persisted
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("PaymentTransaction saved with PayOS order code {OrderCode}", tndtOrderCode);

                // === SYNC PAYOS ORDER CODE TO ORDER/TOURBOOKING FOR LOOKUP COMPATIBILITY ===
                // Use separate try-catch to prevent rollback of PaymentTransaction if sync fails
                try
                {
                    if (request.OrderId.HasValue)
                    {
                        // Update Order with PayOS order code for lookup compatibility
                        var order = await _unitOfWork.OrderRepository.GetByIdAsync(request.OrderId.Value);
                        if (order != null)
                        {
                            order.PayOsOrderCode = tndtOrderCode; // Store TNDT format
                            _unitOfWork.OrderRepository.Update(order);
                            _logger.LogInformation("Updated Order {OrderId} with PayOS order code {OrderCode}", order.Id, tndtOrderCode);
                        }
                        else
                        {
                            _logger.LogWarning("Order {OrderId} not found for PayOS sync", request.OrderId.Value);
                        }
                    }

                    if (request.TourBookingId.HasValue)
                    {
                        // Update TourBooking with PayOS order code for lookup compatibility
                        var tourBooking = await _unitOfWork.TourBookingRepository.GetByIdAsync(request.TourBookingId.Value);
                        if (tourBooking != null)
                        {
                            tourBooking.PayOsOrderCode = tndtOrderCode; // Store TNDT format
                            _unitOfWork.TourBookingRepository.Update(tourBooking);
                            _logger.LogInformation("Updated TourBooking {BookingId} with PayOS order code {OrderCode}", tourBooking.Id, tndtOrderCode);
                        }
                        else
                        {
                            _logger.LogWarning("TourBooking {BookingId} not found for PayOS sync", request.TourBookingId.Value);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("PayOS order code sync completed successfully");
                }
                catch (Exception syncEx)
                {
                    _logger.LogError(syncEx, "Failed to sync PayOS order code {OrderCode} to Order/TourBooking, but PaymentTransaction was saved", tndtOrderCode);
                    // Don't throw - PaymentTransaction is already saved and payment can still work
                }

                _logger.LogInformation("Created PayOS payment link for transaction {TransactionId}", transaction.Id);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment link");
                throw new InvalidOperationException("Không thể tạo link thanh toán PayOS", ex);
            }
        }

        /// <summary>
        /// Retry payment - tạo payment link mới cho transaction đã thất bại
        /// Tương tự như retryPayment trong Java code
        /// </summary>
        public async Task<PaymentTransaction> RetryPaymentAsync(RetryPaymentRequestDto request)
        {
            try
            {
                // 1. Tìm transaction gốc
                PaymentTransaction? rootTransaction = null;

                if (request.OrderId.HasValue)
                {
                    rootTransaction = await _unitOfWork.PaymentTransactionRepository
                        .FindRootTransactionByOrderIdAsync(request.OrderId.Value);
                }
                else if (request.TourBookingId.HasValue)
                {
                    rootTransaction = await _unitOfWork.PaymentTransactionRepository
                        .FindRootTransactionByTourBookingIdAsync(request.TourBookingId.Value);
                }

                if (rootTransaction == null)
                {
                    throw new InvalidOperationException("Không tìm thấy transaction gốc");
                }

                // 2. Lấy transaction cuối cùng trong chain
                var latestTransaction = await GetLatestInRetryChainAsync(rootTransaction);

                // 3. Kiểm tra trạng thái
                if (latestTransaction.Status == PaymentStatus.Paid ||
                    latestTransaction.Status == PaymentStatus.Pending ||
                    latestTransaction.Status == PaymentStatus.Retry)
                {
                    throw new InvalidOperationException("Không thể retry khi giao dịch cuối cùng đang PENDING hoặc PAID hoặc đã RETRY.");
                }

                // 4. Tạo payment link mới
                var newRequest = new CreatePaymentRequestDto
                {
                    OrderId = request.OrderId,
                    TourBookingId = request.TourBookingId,
                    Amount = rootTransaction.Amount,
                    Description = "Thanh toán thử lại"
                };

                var newTransaction = await CreatePaymentLinkAsync(newRequest);

                // 5. Cập nhật status và parent relationship
                newTransaction.Status = PaymentStatus.Retry;
                newTransaction.ParentTransactionId = latestTransaction.Id;

                await _unitOfWork.PaymentTransactionRepository.UpdateAsync(newTransaction);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created retry payment for transaction {ParentId} -> {NewId}",
                    latestTransaction.Id, newTransaction.Id);

                return newTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying payment");
                throw;
            }
        }

        /// <summary>
        /// Xử lý webhook từ PayOS với khả năng phân biệt Product Payment vs Tour Booking Payment
        /// Tương tự như handlePayosWebhook trong Java code
        /// </summary>
        public async Task<PaymentTransaction> HandlePayOsWebhookAsync(object webhookBody)
        {
            try
            {
                var webhookJson = JsonConvert.SerializeObject(webhookBody);
                _logger.LogInformation("Received PayOS webhook: {Webhook}", webhookJson);

                // Parse webhook data (simplified - in real implementation, verify signature)
                dynamic webhook = JsonConvert.DeserializeObject(webhookJson)!;
                string payOsTransactionId = webhook.data?.paymentLinkId?.ToString() ?? "";
                string orderCodeStr = webhook.data?.orderCode?.ToString() ?? "";
                string status = webhook.data?.status?.ToString()?.ToUpper() ?? "";

                if (string.IsNullOrEmpty(payOsTransactionId))
                {
                    throw new InvalidOperationException("PayOS Transaction ID not found in webhook");
                }

                // Tìm transaction theo PayOS Transaction ID
                var transaction = await _unitOfWork.PaymentTransactionRepository
                    .FindByPayOsTransactionIdAsync(payOsTransactionId);

                if (transaction == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy giao dịch với PayOS ID: {payOsTransactionId}");
                }

                // === PHÂN BIỆT PAYMENT TYPE ===
                string paymentType = "Unknown";
                if (transaction.OrderId.HasValue)
                {
                    paymentType = "ProductPayment";
                    _logger.LogInformation("Processing Product Payment webhook for Order {OrderId}", transaction.OrderId.Value);
                }
                else if (transaction.TourBookingId.HasValue)
                {
                    paymentType = "TourBookingPayment";
                    _logger.LogInformation("Processing Tour Booking Payment webhook for TourBooking {TourBookingId}", transaction.TourBookingId.Value);
                }

                // Chỉ xử lý nếu transaction đang PENDING hoặc RETRY
                if (transaction.Status != PaymentStatus.Pending && transaction.Status != PaymentStatus.Retry)
                {
                    _logger.LogInformation("Transaction {TransactionId} already processed with status {Status}",
                        transaction.Id, transaction.Status);
                    return transaction;
                }

                // Cập nhật webhook payload
                transaction.WebhookPayload = webhookJson;

                // Cập nhật status dựa trên PayOS status
                PaymentStatus newStatus;
                string? failureReason = null;

                switch (status)
                {
                    case "PAID":
                        // Kiểm tra duplicate payment theo payment type
                        bool hasPaidTransaction = false;
                        if (transaction.OrderId.HasValue)
                        {
                            hasPaidTransaction = await _unitOfWork.PaymentTransactionRepository
                                .ExistsByOrderAndGatewayAndStatusAsync(transaction.OrderId.Value, PaymentGateway.PayOS, PaymentStatus.Paid);
                        }
                        else if (transaction.TourBookingId.HasValue)
                        {
                            hasPaidTransaction = await _unitOfWork.PaymentTransactionRepository
                                .ExistsByTourBookingAndGatewayAndStatusAsync(transaction.TourBookingId.Value, PaymentGateway.PayOS, PaymentStatus.Paid);
                        }

                        if (hasPaidTransaction)
                        {
                            _logger.LogWarning("Duplicate payment detected for {PaymentType} transaction {TransactionId}",
                                paymentType, transaction.Id);
                            return transaction;
                        }

                        newStatus = PaymentStatus.Paid;

                        // === XỬ LÝ BUSINESS LOGIC THEO PAYMENT TYPE ===
                        await ProcessPaymentSuccessByTypeAsync(transaction, paymentType);
                        break;
                    case "PENDING":
                        _logger.LogInformation("Payment still pending for {PaymentType} transaction {TransactionId}",
                            paymentType, transaction.Id);
                        return transaction; // Không thay đổi gì
                    case "CANCELLED":
                        newStatus = PaymentStatus.Cancelled;
                        failureReason = "Giao dịch đã bị huỷ.";
                        break;
                    case "EXPIRED":
                        newStatus = PaymentStatus.Expired;
                        failureReason = "Giao dịch đã hết hạn.";
                        break;
                    default:
                        newStatus = PaymentStatus.Failed;
                        failureReason = $"Trạng thái không xác định từ PayOS: {status}";
                        break;
                }

                transaction.Status = newStatus;
                transaction.FailureReason = failureReason;

                await _unitOfWork.PaymentTransactionRepository.UpdateAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated {PaymentType} transaction {TransactionId} status to {Status}",
                    paymentType, transaction.Id, newStatus);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling PayOS webhook");
                throw;
            }
        }

        /// <summary>
        /// Xử lý business logic khi thanh toán thành công theo loại payment
        /// </summary>
        private async Task ProcessPaymentSuccessByTypeAsync(PaymentTransaction transaction, string paymentType)
        {
            try
            {
                if (paymentType == "ProductPayment" && transaction.OrderId.HasValue)
                {
                    _logger.LogInformation("Processing Product Payment success for Order {OrderId}", transaction.OrderId.Value);

                    // TODO: Gọi ProductService để xử lý:
                    // - Update Order status to Paid
                    // - Clear cart
                    // - Update inventory
                    // - Add money to shop wallet

                    // Tạm thời log để biết flow hoạt động
                    _logger.LogInformation("Product Payment success processing completed for Order {OrderId}", transaction.OrderId.Value);
                }
                else if (paymentType == "TourBookingPayment" && transaction.TourBookingId.HasValue)
                {
                    _logger.LogInformation("Processing Tour Booking Payment success for TourBooking {TourBookingId}", transaction.TourBookingId.Value);

                    // TODO: Gọi UserTourBookingService để xử lý:
                    // - Update TourBooking status to Confirmed
                    // - Send confirmation email
                    // - Add money to tour company revenue

                    // Tạm thời log để biết flow hoạt động
                    _logger.LogInformation("Tour Booking Payment success processing completed for TourBooking {TourBookingId}", transaction.TourBookingId.Value);
                }
                else
                {
                    _logger.LogWarning("Unknown payment type {PaymentType} for transaction {TransactionId}",
                        paymentType, transaction.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment success for {PaymentType} transaction {TransactionId}",
                    paymentType, transaction.Id);
                // Don't throw - webhook should still be marked as processed
            }
        }

        /// <summary>
        /// Lấy danh sách transactions theo OrderId
        /// </summary>
        public async Task<List<PaymentTransactionResponseDto>> GetTransactionsByOrderIdAsync(Guid orderId)
        {
            var transactions = await _unitOfWork.PaymentTransactionRepository.FindAllByOrderIdAsync(orderId);
            return transactions.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Lấy danh sách transactions theo TourBookingId
        /// </summary>
        public async Task<List<PaymentTransactionResponseDto>> GetTransactionsByTourBookingIdAsync(Guid tourBookingId)
        {
            var transactions = await _unitOfWork.PaymentTransactionRepository.FindAllByTourBookingIdAsync(tourBookingId);
            return transactions.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Hủy tất cả transactions đang pending
        /// </summary>
        public async Task<CancelPaymentResponseDto> CancelAllPendingTransactionsAsync(CancelPaymentRequestDto request)
        {
            try
            {
                List<PaymentTransaction> transactions = new List<PaymentTransaction>();

                if (request.OrderId.HasValue)
                {
                    transactions = await _unitOfWork.PaymentTransactionRepository.FindAllByOrderIdAsync(request.OrderId.Value);
                }
                else if (request.TourBookingId.HasValue)
                {
                    transactions = await _unitOfWork.PaymentTransactionRepository.FindAllByTourBookingIdAsync(request.TourBookingId.Value);
                }

                if (!transactions.Any())
                {
                    throw new InvalidOperationException("Không tìm thấy bất kỳ giao dịch nào");
                }

                int cancelledCount = 0;
                var cancelledIds = new List<Guid>();

                foreach (var transaction in transactions.Where(t => t.Status == PaymentStatus.Pending))
                {
                    try
                    {
                        // Cancel trên PayOS nếu có PayOS Order Code
                        if (!string.IsNullOrWhiteSpace(transaction.PayOsOrderCode))
                        {
                            var clientId = _config["PayOS:ClientId"];
                            var apiKey = _config["PayOS:ApiKey"];
                            var checksumKey = _config["PayOS:ChecksumKey"];

                            var payOS = new PayOS(clientId!, apiKey!, checksumKey!);
                            var numericOrderCode = PayOsOrderCodeUtility.ExtractNumericPart(transaction.PayOsOrderCode);
                            await payOS.cancelPaymentLink(
                                numericOrderCode,
                                request.CancellationReason ?? "Huỷ đơn hàng"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to cancel PayOS payment link for transaction {TransactionId}", transaction.Id);
                        // Continue với việc cancel local transaction
                    }

                    transaction.Status = PaymentStatus.Cancelled;
                    transaction.FailureReason = request.CancellationReason ?? "Đã hủy bởi người dùng";

                    await _unitOfWork.PaymentTransactionRepository.UpdateAsync(transaction);
                    cancelledCount++;
                    cancelledIds.Add(transaction.Id);
                }

                await _unitOfWork.SaveChangesAsync();

                return new CancelPaymentResponseDto
                {
                    CancelledCount = cancelledCount,
                    Message = cancelledCount > 0
                        ? "Đã huỷ toàn bộ các giao dịch PENDING"
                        : "Không có giao dịch PENDING để huỷ",
                    CancelledTransactionIds = cancelledIds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling pending transactions");
                throw;
            }
        }

        /// <summary>
        /// Xác nhận webhook URL với PayOS
        /// </summary>
        public async Task<string> ConfirmWebhookAsync(string webhookUrl)
        {
            try
            {
                var clientId = _config["PayOS:ClientId"];
                var apiKey = _config["PayOS:ApiKey"];
                var checksumKey = _config["PayOS:ChecksumKey"];

                var payOS = new PayOS(clientId!, apiKey!, checksumKey!);
                var confirmedUrl = await payOS.confirmWebhook(webhookUrl);

                _logger.LogInformation("Webhook confirmed: {ConfirmedUrl}", confirmedUrl);
                return confirmedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming webhook: {WebhookUrl}", webhookUrl);
                throw;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Lấy transaction cuối cùng trong retry chain
        /// </summary>
        private async Task<PaymentTransaction> GetLatestInRetryChainAsync(PaymentTransaction current)
        {
            var child = await _unitOfWork.PaymentTransactionRepository.FindByParentTransactionIdAsync(current.Id);
            return child != null ? await GetLatestInRetryChainAsync(child) : current;
        }

        /// <summary>
        /// Map PaymentTransaction entity to response DTO với Payment Type detection
        /// </summary>
        private static PaymentTransactionResponseDto MapToResponseDto(PaymentTransaction transaction)
        {
            // Determine payment type
            string paymentType = "Unknown";
            object? orderInfo = null;
            object? tourBookingInfo = null;

            if (transaction.OrderId.HasValue)
            {
                paymentType = "ProductPayment";
                if (transaction.Order != null)
                {
                    orderInfo = new
                    {
                        transaction.Order.Id,
                        transaction.Order.PayOsOrderCode,
                        transaction.Order.TotalAmount,
                        transaction.Order.TotalAfterDiscount,
                        transaction.Order.Status,
                        transaction.Order.CreatedAt
                    };
                }
            }
            else if (transaction.TourBookingId.HasValue)
            {
                paymentType = "TourBookingPayment";
                if (transaction.TourBooking != null)
                {
                    tourBookingInfo = new
                    {
                        transaction.TourBooking.Id,
                        transaction.TourBooking.BookingCode,
                        transaction.TourBooking.PayOsOrderCode,
                        transaction.TourBooking.TotalPrice,
                        transaction.TourBooking.Status,
                        transaction.TourBooking.CreatedAt
                    };
                }
            }

            return new PaymentTransactionResponseDto
            {
                Id = transaction.Id,
                OrderId = transaction.OrderId,
                TourBookingId = transaction.TourBookingId,
                Amount = transaction.Amount,
                Status = transaction.Status,
                Description = transaction.Description,
                Gateway = transaction.Gateway,
                PayOsOrderCode = transaction.PayOsOrderCode,
                PayOsTransactionId = transaction.PayOsTransactionId,
                CheckoutUrl = transaction.CheckoutUrl,
                QrCode = transaction.QrCode,
                FailureReason = transaction.FailureReason,
                ParentTransactionId = transaction.ParentTransactionId,
                ExpiredAt = transaction.ExpiredAt,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                PaymentType = paymentType,
                OrderInfo = orderInfo,
                TourBookingInfo = tourBookingInfo
            };
        }

        /// <summary>
        /// Kiểm tra loại thanh toán của transaction
        /// </summary>
        public async Task<string> GetPaymentTypeAsync(Guid transactionId)
        {
            return await _unitOfWork.PaymentTransactionRepository.GetPaymentTypeAsync(transactionId);
        }

        /// <summary>
        /// Lấy danh sách Product Payment transactions
        /// </summary>
        public async Task<List<PaymentTransactionResponseDto>> GetProductPaymentTransactionsAsync(int pageIndex = 0, int pageSize = 10)
        {
            var transactions = await _unitOfWork.PaymentTransactionRepository.GetProductPaymentTransactionsAsync(pageIndex, pageSize);
            return transactions.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Lấy danh sách Tour Booking Payment transactions
        /// </summary>
        public async Task<List<PaymentTransactionResponseDto>> GetTourBookingPaymentTransactionsAsync(int pageIndex = 0, int pageSize = 10)
        {
            var transactions = await _unitOfWork.PaymentTransactionRepository.GetTourBookingPaymentTransactionsAsync(pageIndex, pageSize);
            return transactions.Select(MapToResponseDto).ToList();
        }



        /// <summary>
        /// Lấy transaction theo PayOS order code
        /// </summary>
        public async Task<object?> GetTransactionByOrderCodeAsync(string orderCode)
        {
            var transaction = await _unitOfWork.PaymentTransactionRepository.GetByPayOsOrderCodeAsync(orderCode);
            if (transaction == null)
            {
                return null;
            }

            // Map to response object with order/booking info
            object? orderInfo = null;
            object? tourBookingInfo = null;

            if (transaction.OrderId.HasValue)
            {
                // Get order info
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(transaction.OrderId.Value);
                if (order != null)
                {
                    orderInfo = new
                    {
                        id = order.Id,
                        payOsOrderCode = order.PayOsOrderCode,
                        totalAmount = order.TotalAmount,
                        status = order.Status.ToString(),
                        createdAt = order.CreatedAt
                    };
                }
            }
            else if (transaction.TourBookingId.HasValue)
            {
                // Get tour booking info
                var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(transaction.TourBookingId.Value);
                if (booking != null)
                {
                    tourBookingInfo = new
                    {
                        id = booking.Id,
                        bookingCode = booking.BookingCode,
                        totalPrice = booking.TotalPrice,
                        status = booking.Status.ToString(),
                        createdAt = booking.CreatedAt
                    };
                }
            }

            return new
            {
                id = transaction.Id,
                orderId = transaction.OrderId,
                tourBookingId = transaction.TourBookingId,
                amount = transaction.Amount,
                status = transaction.Status.ToString(),
                payOsOrderCode = transaction.PayOsOrderCode,
                checkoutUrl = transaction.CheckoutUrl,
                createdAt = transaction.CreatedAt,
                paymentType = transaction.OrderId.HasValue ? "ProductPayment" : "TourBookingPayment",
                orderInfo = orderInfo,
                tourBookingInfo = tourBookingInfo
            };
        }

        /// <summary>
        /// Xử lý webhook callback
        /// </summary>
        public async Task<object> ProcessWebhookCallbackAsync(string orderCode, string status)
        {
            var transaction = await _unitOfWork.PaymentTransactionRepository.GetByPayOsOrderCodeAsync(orderCode);
            if (transaction == null)
            {
                throw new ArgumentException("Transaction không tồn tại");
            }

            // Update transaction status
            transaction.Status = status.ToUpper() == "PAID" ? PaymentStatus.Paid : PaymentStatus.Failed;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.PaymentTransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            // Process business logic based on payment type
            if (transaction.OrderId.HasValue)
            {
                // Product payment - delegate to existing service
                _logger.LogInformation("Processing enhanced product payment success for order {OrderId}", transaction.OrderId);
                // You can call existing product payment success logic here
            }
            else if (transaction.TourBookingId.HasValue)
            {
                // Tour booking payment - delegate to existing service
                _logger.LogInformation("Processing enhanced tour booking payment success for booking {BookingId}", transaction.TourBookingId);
                // You can call existing tour booking payment success logic here
            }

            return new
            {
                transactionId = transaction.Id,
                status = transaction.Status.ToString(),
                processedAt = DateTime.UtcNow
            };
        }

        #endregion

        #endregion

    }
}
