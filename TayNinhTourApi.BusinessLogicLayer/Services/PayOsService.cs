using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class PayOsService : IPayOsService
    {
        private readonly IConfiguration _config;
        private readonly IOrderRepository _orderRepository;

        public PayOsService(IConfiguration config, IOrderRepository orderRepository)
        {
            _config = config;
            _orderRepository = orderRepository;
        }

        public async Task<string?> CreatePaymentUrlAsync(decimal amount, string orderCode, string returnUrl)
        {
            var maxRetries = 3;
            var retryCount = 0;
            
            while (retryCount < maxRetries)
            {
                try
                {
                    // ✅ Validation cấu hình PayOS
                    var clientId = _config["PayOS:ClientId"];
                    var apiKey = _config["PayOS:ApiKey"];
                    var checksumKey = _config["PayOS:CheckSum"];

                    Console.WriteLine($"PayOS Config - ClientId: {clientId}, ApiKey: {apiKey?.Substring(0, 5)}..., ChecksumKey: {checksumKey?.Substring(0, 5)}...");

                    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
                    {
                        throw new InvalidOperationException("PayOS configuration is missing or invalid");
                    }

                    // ✅ Validation input
                    if (amount <= 0)
                    {
                        throw new ArgumentException("Amount must be greater than 0");
                    }

                    // ✅ PayOS yêu cầu amount tối thiểu là 1000 VND
                    if (amount < 1000)
                    {
                        throw new ArgumentException("Amount must be at least 1000 VND for PayOS");
                    }

                    if (string.IsNullOrEmpty(orderCode))
                    {
                        throw new ArgumentException("OrderCode cannot be null or empty");
                    }

                    Console.WriteLine($"Creating payment for OrderCode: {orderCode}, Amount: {amount} (Attempt: {retryCount + 1})");

                    // ✅ Tạo ABSOLUTELY UNIQUE orderCode với session prefix để tránh cache
                    var sessionId = Environment.TickCount % 1000; // Session ID duy nhất cho mỗi lần chạy app
                    var guidHash = Math.Abs(Guid.NewGuid().GetHashCode()); // Random hash từ new Guid
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 10000; // 4 digits cuối của timestamp
                    var randomSuffix = new Random().Next(100, 999); // 3 digits random
                    
                    // Format: {sessionId_3_digits}{guidHash_7_digits}{timestamp_4_digits}{random_3_digits}
                    var sessionStr = sessionId.ToString().PadLeft(3, '0'); // 3 digits session
                    var guidHashStr = Math.Abs(guidHash).ToString().PadLeft(8, '0').Substring(0, 7); // 7 digits từ guid hash
                    var timestampStr = timestamp.ToString().PadLeft(4, '0'); // 4 digits từ timestamp
                    var randomStr = randomSuffix.ToString(); // 3 digits random
                    
                    // Total: 3 + 7 + 4 + 3 = 17 digits → Cắt xuống 15 để an toàn
                    var tempOrderCode = $"{sessionStr}{guidHashStr}{timestampStr}{randomStr}";
                    if (tempOrderCode.Length > 15)
                    {
                        tempOrderCode = tempOrderCode.Substring(0, 15);
                    }
                    
                    var numericOrderCode = long.Parse(tempOrderCode);
                    
                    Console.WriteLine($"Generated UNIQUE OrderCode: {numericOrderCode} (session: {sessionStr}, guidHash: {guidHashStr}, timestamp: {timestampStr}, random: {randomStr}, length: {tempOrderCode.Length})");

                    // ✅ Save PayOsOrderCode to database for callback tracking
                    var orderId = Guid.Parse(orderCode);
                    var order = await _orderRepository.GetByIdAsync(orderId);
                    if (order != null)
                    {
                        order.PayOsOrderCode = numericOrderCode;
                        await _orderRepository.UpdateAsync(order);
                        await _orderRepository.SaveChangesAsync();
                        Console.WriteLine($"Saved PayOsOrderCode {numericOrderCode} to order {orderId}");
                    }

                    // ✅ Tạo items data cho PayOS (bắt buộc) với amount chính xác
                    List<ItemData> items = new List<ItemData>
                    {
                        new ItemData("Order Payment", 1, (int)amount)
                    };

                    Console.WriteLine($"Items created: {items.Count} items, Total amount: {(int)amount}");

                    PayOS payOS = new PayOS(clientId, apiKey, checksumKey);

                    PaymentData paymentData = new PaymentData(
                     orderCode: numericOrderCode,
                     amount: (int)amount,
                     description: "Order Payment",
                     items: items,
                     cancelUrl: $"https://tndt.netlify.app/payment-cancel?orderId={orderCode}",
                     returnUrl: $"https://tndt.netlify.app/payment-success?orderId={orderCode}",
                     buyerName: "Customer");

                    Console.WriteLine($"PaymentData created: OrderCode={numericOrderCode}, Amount={(int)amount}, Description=Order Payment");
                    Console.WriteLine($"PayOS will use callback URL from dashboard configuration");
                    
                    CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);
                    Console.WriteLine($"Payment URL created successfully: {createPayment.checkoutUrl}");
                    
                    return createPayment.checkoutUrl;
                }
                catch (Net.payOS.Errors.PayOSError payOSError)
                {
                    Console.WriteLine($"PayOS Error on attempt {retryCount + 1}: {payOSError.Message}");
                    
                    // Nếu là lỗi orderCode đã tồn tại và còn lần retry, thử lại
                    if ((payOSError.Message.Contains("đã tồn tại") || payOSError.Message.Contains("duplicate") || payOSError.Message.Contains("exists")) 
                        && retryCount < maxRetries - 1)
                    {
                        retryCount++;
                        Console.WriteLine($"Retrying with new orderCode... Attempt {retryCount + 1}");
                        await Task.Delay(1000 * retryCount); // Delay tăng dần
                        continue;
                    }
                    
                    // Nếu không phải lỗi duplicate hoặc đã hết retry, throw exception
                    throw new InvalidOperationException($"Lỗi PayOS sau {retryCount + 1} lần thử: {payOSError.Message}", payOSError);
                }
                catch (Exception ex)
                {
                    // Đối với exception khác, không retry
                    Console.WriteLine($"CreatePaymentUrlAsync Error: {ex.Message}");
                    Console.WriteLine($"Error Type: {ex.GetType().Name}");
                    
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }

                    throw new InvalidOperationException($"Không thể tạo link thanh toán: {ex.Message}", ex);
                }
            }
            
            throw new InvalidOperationException($"Không thể tạo payment link sau {maxRetries} lần thử");
        }

        public async Task<OrderStatus> GetOrderPaymentStatusAsync(string orderCode)
        {
            var clientId = _config["PayOS:ClientId"];
            var apiKey = _config["PayOS:ApiKey"];

            // ✅ Lấy PayOsOrderCode từ database thay vì tính toán lại
            var orderId = Guid.Parse(orderCode);
            var order = await _orderRepository.GetByIdAsync(orderId);
            
            if (order?.PayOsOrderCode == null)
            {
                throw new Exception("Không tìm thấy PayOsOrderCode cho đơn hàng này");
            }
            
            var numericOrderCode = order.PayOsOrderCode.Value;
            var url = $"https://api-merchant.payos.vn/v2/payment-requests/{numericOrderCode}";

            Console.WriteLine($"Checking payment status for PayOsOrderCode: {numericOrderCode}");

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
    }
}
