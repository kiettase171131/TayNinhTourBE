using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/payment-callback")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductService _productService;

        public PaymentController(IOrderRepository orderRepository, IProductService productService)
        {
            _orderRepository = orderRepository;
            _productService = productService;
        }

        [HttpPost] 
        public async Task<IActionResult> Callback([FromBody] PayOSCallbackDto payload)
        {
            try
            {
                // ✅ Add comprehensive logging for debugging
                Console.WriteLine($"PayOS Callback received:");
                Console.WriteLine($"OrderCode: {payload.orderCode}");
                Console.WriteLine($"Status: {payload.status}");
                Console.WriteLine($"Full payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");

                // ✅ Validate payload
                if (string.IsNullOrEmpty(payload.orderCode) || string.IsNullOrEmpty(payload.status))
                {
                    Console.WriteLine("Invalid payload: orderCode or status is null/empty");
                    return BadRequest("Invalid payload: orderCode and status are required");
                }

                // ✅ Find order by PayOsOrderCode
                if (!long.TryParse(payload.orderCode, out long numericOrderCode))
                {
                    Console.WriteLine($"Failed to parse orderCode: {payload.orderCode}");
                    return BadRequest("Invalid orderCode format");
                }

                Console.WriteLine($"Looking for order with PayOsOrderCode: {numericOrderCode}");
                var order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == numericOrderCode);

                if (order == null)
                {
                    Console.WriteLine($"Order not found with PayOsOrderCode: {numericOrderCode}");
                    return NotFound($"Không tìm thấy đơn hàng với PayOsOrderCode: {numericOrderCode}");
                }

                Console.WriteLine($"Found order: {order.Id}, Current Status: {order.Status}");

                // Cập nhật trạng thái theo payload.status
                if (payload.status == "PAID")
                {
                    Console.WriteLine("Processing PAID status...");
                    order.Status = OrderStatus.Paid;
                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();
                    Console.WriteLine("Order status updated to PAID");
                    
                    // Process post-payment actions
                    Console.WriteLine("Calling ClearCartAndUpdateInventoryAsync...");
                    await _productService.ClearCartAndUpdateInventoryAsync(order.Id);
                    Console.WriteLine("ClearCartAndUpdateInventoryAsync completed");
                }
                else if (payload.status == "CANCELLED")
                {
                    Console.WriteLine("Processing CANCELLED status...");
                    order.Status = OrderStatus.Cancelled;
                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();
                    Console.WriteLine("Order status updated to CANCELLED");
                }
                else
                {
                    Console.WriteLine($"Processing other status: {payload.status}");
                    order.Status = OrderStatus.Pending;
                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();
                    Console.WriteLine("Order status updated to PENDING");
                }

                Console.WriteLine("Callback processing completed successfully");
                return Ok(new { message = "Cập nhật trạng thái thành công.", orderId = order.Id, newStatus = order.Status });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Callback error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Có lỗi xảy ra khi xử lý callback.", error = ex.Message });
            }
        }

        /// <summary>
        /// Health check endpoint để PayOS test callback
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                status = "OK", 
                timestamp = DateTime.UtcNow,
                message = "Payment callback endpoint is ready" 
            });
        }

        /// <summary>
        /// Test endpoint để manually trigger callback processing
        /// </summary>
        [HttpPost("test")]
        public async Task<IActionResult> TestCallback([FromBody] PayOSCallbackDto payload)
        {
            Console.WriteLine("TEST CALLBACK ENDPOINT CALLED");
            return await Callback(payload);
        }

        /// <summary>
        /// Get callback URL for PayOS configuration
        /// </summary>
        [HttpGet("url")]
        public IActionResult GetCallbackUrl()
        {
            var request = HttpContext.Request;
            var callbackUrl = $"{request.Scheme}://{request.Host}/api/payment-callback";
            return Ok(new { 
                callbackUrl = callbackUrl,
                testUrl = $"{callbackUrl}/test",
                healthUrl = $"{callbackUrl}/health",
                message = "Configure this URL in PayOS dashboard as webhook URL" 
            });
        }

        /// <summary>
        /// Manual payment confirmation - trigger callback processing manually
        /// </summary>
        [HttpPost("confirm/{orderId}")]
        public async Task<IActionResult> ConfirmPayment(Guid orderId, [FromBody] PayOSCallbackDto payload)
        {
            try
            {
                Console.WriteLine($"Manual payment confirmation for order: {orderId}");
                
                // Override orderCode with the PayOsOrderCode from database
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order?.PayOsOrderCode == null)
                {
                    return NotFound("Order not found or PayOsOrderCode missing");
                }

                // Use the stored PayOsOrderCode
                var modifiedPayload = new PayOSCallbackDto
                {
                    orderCode = order.PayOsOrderCode.ToString(),
                    status = payload.status ?? "PAID" // Default to PAID if not specified
                };

                Console.WriteLine($"Using PayOsOrderCode from database: {modifiedPayload.orderCode}");
                return await Callback(modifiedPayload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Manual confirmation error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Có lỗi xảy ra khi xác nhận thanh toán.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get payment status from PayOS and update if needed
        /// </summary>
        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetPaymentStatus(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound("Order not found");
                }

                Console.WriteLine($"Getting payment status for order: {orderId}, PayOsOrderCode: {order.PayOsOrderCode}");

                // Check status from PayOS if PayOsOrderCode exists
                if (order.PayOsOrderCode.HasValue)
                {
                    try
                    {
                        var payosStatus = await _productService.GetOrderPaymentStatusAsync(orderId);
                        Console.WriteLine($"PayOS Status: {payosStatus}, DB Status: {order.Status}");
                        
                        // ✅ Trigger callback processing if PayOS status differs from DB status
                        if (payosStatus != order.Status)
                        {
                            Console.WriteLine($"PayOS shows {payosStatus} but DB shows {order.Status}. Triggering callback processing...");
                            
                            string callbackStatus = payosStatus switch
                            {
                                OrderStatus.Paid => "PAID",
                                OrderStatus.Cancelled => "CANCELLED",
                                _ => "PENDING"
                            };
                            
                            var callbackPayload = new PayOSCallbackDto
                            {
                                orderCode = order.PayOsOrderCode.ToString(),
                                status = callbackStatus
                            };
                            
                            Console.WriteLine($"Triggering callback with status: {callbackStatus}");
                            await Callback(callbackPayload);
                            
                            // Refresh order status after callback
                            order = await _orderRepository.GetByIdAsync(orderId);
                            Console.WriteLine($"After callback - DB Status: {order?.Status}");
                        }
                        
                        return Ok(new { 
                            orderId = order.Id,
                            status = order.Status.ToString(),
                            payosStatus = payosStatus.ToString(),
                            message = payosStatus != order.Status ? "Status synced from PayOS" : "Status is already up to date"
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking PayOS status: {ex.Message}");
                        return Ok(new { 
                            orderId = order.Id,
                            status = order.Status.ToString(),
                            message = "Could not verify with PayOS, showing current status",
                            error = ex.Message
                        });
                    }
                }

                return Ok(new { 
                    orderId = order.Id,
                    status = order.Status.ToString(),
                    message = "No PayOsOrderCode found - showing current status"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get payment status error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Có lỗi xảy ra khi kiểm tra trạng thái.", error = ex.Message });
            }
        }

        /// <summary>
        /// Auto-check pending payments and update status
        /// Useful for development when callback URL is not accessible
        /// </summary>
        [HttpPost("auto-check")]
        public async Task<IActionResult> AutoCheckPendingPayments()
        {
            try
            {
                Console.WriteLine("Auto-checking pending payments...");
                
                // ✅ Get all orders that might need status update (not just Pending)
                var ordersToCheck = await _orderRepository.GetAllAsync(x => 
                    x.PayOsOrderCode.HasValue &&
                    x.CreatedAt > DateTime.UtcNow.AddHours(-24)); // Check orders from last 24 hours

                var checkedCount = 0;
                var updatedCount = 0;

                foreach (var order in ordersToCheck)
                {
                    try
                    {
                        checkedCount++;
                        Console.WriteLine($"Checking order {order.Id} with PayOsOrderCode {order.PayOsOrderCode}, Current Status: {order.Status}");
                        
                        var payosStatus = await _productService.GetOrderPaymentStatusAsync(order.Id);
                        Console.WriteLine($"Order {order.Id}: PayOS={payosStatus}, DB={order.Status}");
                        
                        // ✅ Trigger callback if status differs
                        if (payosStatus != order.Status)
                        {
                            Console.WriteLine($"Order {order.Id} status mismatch. Triggering callback...");
                            
                            string callbackStatus = payosStatus switch
                            {
                                OrderStatus.Paid => "PAID",
                                OrderStatus.Cancelled => "CANCELLED", 
                                _ => "PENDING"
                            };
                            
                            var callbackPayload = new PayOSCallbackDto
                            {
                                orderCode = order.PayOsOrderCode.ToString(),
                                status = callbackStatus
                            };
                            
                            await Callback(callbackPayload);
                            updatedCount++;
                            
                            // Small delay to avoid overwhelming the system
                            await Task.Delay(500);
                        }
                        else
                        {
                            Console.WriteLine($"Order {order.Id} status is already in sync");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking order {order.Id}: {ex.Message}");
                    }
                }

                return Ok(new { 
                    message = "Auto-check completed",
                    checkedOrders = checkedCount,
                    updatedOrders = updatedCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auto-check error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Có lỗi xảy ra khi auto-check.", error = ex.Message });
            }
        }

        /// <summary>
        /// Debug endpoint to check PayOsOrderCode for an order
        /// </summary>
        [HttpGet("debug/{orderId}")]
        public async Task<IActionResult> DebugOrder(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound("Order not found");
                }

                return Ok(new { 
                    orderId = order.Id,
                    payOsOrderCode = order.PayOsOrderCode,
                    status = order.Status.ToString(),
                    totalAmount = order.TotalAmount,
                    createdAt = order.CreatedAt,
                    message = $"Use PayOsOrderCode '{order.PayOsOrderCode}' for callback testing"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Enhanced debug endpoint with step-by-step callback testing
        /// </summary>
        [HttpPost("debug-callback")]
        public async Task<IActionResult> DebugCallback([FromBody] PayOSCallbackDto payload)
        {
            var debugSteps = new List<string>();
            
            try
            {
                debugSteps.Add("Step 1: Received callback request");
                debugSteps.Add($"Payload: orderCode={payload?.orderCode}, status={payload?.status}");

                if (payload == null)
                {
                    debugSteps.Add("ERROR: Payload is null");
                    return BadRequest(new { debugSteps, error = "Payload is null" });
                }

                if (string.IsNullOrEmpty(payload.orderCode) || string.IsNullOrEmpty(payload.status))
                {
                    debugSteps.Add("ERROR: OrderCode or Status is null/empty");
                    return BadRequest(new { debugSteps, error = "OrderCode or Status is required" });
                }

                debugSteps.Add("Step 2: Parsing orderCode to long");
                if (!long.TryParse(payload.orderCode, out long numericOrderCode))
                {
                    debugSteps.Add($"ERROR: Cannot parse orderCode '{payload.orderCode}' to long");
                    return BadRequest(new { debugSteps, error = "Invalid orderCode format" });
                }

                debugSteps.Add($"Step 3: Looking for order with PayOsOrderCode: {numericOrderCode}");
                var order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == numericOrderCode);

                if (order == null)
                {
                    debugSteps.Add("ERROR: Order not found in database");
                    
                    // Also check all orders with PayOsOrderCode to see what's in database
                    var allOrdersWithPayOs = await _orderRepository.GetAllAsync(x => x.PayOsOrderCode.HasValue);
                    debugSteps.Add($"Found {allOrdersWithPayOs.Count()} orders with PayOsOrderCode in database:");
                    foreach (var o in allOrdersWithPayOs.Take(5)) // Limit to 5 for debugging
                    {
                        debugSteps.Add($"  Order {o.Id}: PayOsOrderCode={o.PayOsOrderCode}, Status={o.Status}");
                    }
                    
                    return NotFound(new { debugSteps, error = "Order not found" });
                }

                debugSteps.Add($"Step 4: Found order {order.Id} with current status: {order.Status}");

                var oldStatus = order.Status;
                debugSteps.Add($"Step 5: Processing status change from {oldStatus} to {payload.status}");

                // Update status based on payload
                if (payload.status == "PAID")
                {
                    debugSteps.Add("Step 6: Setting status to Paid");
                    order.Status = OrderStatus.Paid;
                }
                else if (payload.status == "CANCELLED")
                {
                    debugSteps.Add("Step 6: Setting status to Cancelled");
                    order.Status = OrderStatus.Cancelled;
                }
                else
                {
                    debugSteps.Add($"Step 6: Setting status to Pending for unknown status: {payload.status}");
                    order.Status = OrderStatus.Pending;
                }

                debugSteps.Add("Step 7: Updating order in database");
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();
                debugSteps.Add("Step 8: Database update completed");

                // Verify the update
                var updatedOrder = await _orderRepository.GetByIdAsync(order.Id);
                debugSteps.Add($"Step 9: Verification - Order status is now: {updatedOrder?.Status}");

                if (payload.status == "PAID" && updatedOrder?.Status == OrderStatus.Paid)
                {
                    debugSteps.Add("Step 10: Calling ClearCartAndUpdateInventoryAsync");
                    try
                    {
                        await _productService.ClearCartAndUpdateInventoryAsync(order.Id);
                        debugSteps.Add("Step 11: ClearCartAndUpdateInventoryAsync completed successfully");
                    }
                    catch (Exception ex)
                    {
                        debugSteps.Add($"Step 11: ERROR in ClearCartAndUpdateInventoryAsync: {ex.Message}");
                    }
                }

                return Ok(new { 
                    message = "Debug callback completed successfully",
                    oldStatus = oldStatus.ToString(),
                    newStatus = updatedOrder?.Status.ToString(),
                    debugSteps = debugSteps
                });
            }
            catch (Exception ex)
            {
                debugSteps.Add($"FATAL ERROR: {ex.Message}");
                debugSteps.Add($"Stack trace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { 
                    message = "Debug callback failed", 
                    error = ex.Message,
                    debugSteps = debugSteps
                });
            }
        }

        /// <summary>
        /// Payment success endpoint - được gọi từ frontend khi user redirect về từ PayOS
        /// </summary>
        [HttpPost("payment-success/{orderId}")]
        public async Task<IActionResult> PaymentSuccess(Guid orderId)
        {
            try
            {
                Console.WriteLine($"Payment success called for order: {orderId}");
                
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found", orderId });
                }

                Console.WriteLine($"Order found: {orderId}, Current Status: {order.Status}, PayOsOrderCode: {order.PayOsOrderCode}");

                // Nếu order đã được xử lý rồi, return luôn
                if (order.Status == OrderStatus.Paid)
                {
                    return Ok(new { 
                        message = "Payment already processed", 
                        orderId = order.Id,
                        status = order.Status.ToString(),
                        isAlreadyProcessed = true
                    });
                }

                // Check status từ PayOS
                if (order.PayOsOrderCode.HasValue)
                {
                    try
                    {
                        var payosStatus = await _productService.GetOrderPaymentStatusAsync(orderId);
                        Console.WriteLine($"PayOS Status: {payosStatus}, DB Status: {order.Status}");

                        if (payosStatus == OrderStatus.Paid)
                        {
                            Console.WriteLine("PayOS shows PAID. Triggering callback processing...");
                            
                            var callbackPayload = new PayOSCallbackDto
                            {
                                orderCode = order.PayOsOrderCode.ToString(),
                                status = "PAID"
                            };
                            
                            await Callback(callbackPayload);
                            
                            // Refresh order để lấy status mới
                            order = await _orderRepository.GetByIdAsync(orderId);
                            
                            return Ok(new { 
                                message = "Payment processed successfully", 
                                orderId = order.Id,
                                status = order.Status.ToString(),
                                payosStatus = payosStatus.ToString(),
                                inventoryUpdated = true,
                                cartCleared = true
                            });
                        }
                        else
                        {
                            return Ok(new { 
                                message = "Payment verification pending", 
                                orderId = order.Id,
                                status = order.Status.ToString(),
                                payosStatus = payosStatus.ToString(),
                                shouldRetry = true
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking PayOS status: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError, new { 
                            message = "Error verifying payment status", 
                            orderId = order.Id,
                            error = ex.Message
                        });
                    }
                }

                return BadRequest(new { 
                    message = "Order has no PayOS code", 
                    orderId = order.Id 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Payment success error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { 
                    message = "Error processing payment success", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Payment cancel endpoint - được gọi từ frontend khi user cancel payment
        /// </summary>
        [HttpPost("payment-cancel/{orderId}")]
        public async Task<IActionResult> PaymentCancel(Guid orderId)
        {
            try
            {
                Console.WriteLine($"Payment cancel called for order: {orderId}");
                
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found", orderId });
                }

                // Check status từ PayOS để confirm
                if (order.PayOsOrderCode.HasValue)
                {
                    try
                    {
                        var payosStatus = await _productService.GetOrderPaymentStatusAsync(orderId);
                        
                        if (payosStatus == OrderStatus.Cancelled)
                        {
                            Console.WriteLine("PayOS shows CANCELLED. Triggering callback processing...");
                            
                            var callbackPayload = new PayOSCallbackDto
                            {
                                orderCode = order.PayOsOrderCode.ToString(),
                                status = "CANCELLED"
                            };
                            
                            await Callback(callbackPayload);
                            
                            // Refresh order
                            order = await _orderRepository.GetByIdAsync(orderId);
                        }
                        
                        return Ok(new { 
                            message = "Payment cancellation processed", 
                            orderId = order.Id,
                            status = order.Status.ToString(),
                            payosStatus = payosStatus.ToString()
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking PayOS cancel status: {ex.Message}");
                        return Ok(new { 
                            message = "Payment cancellation noted", 
                            orderId = order.Id,
                            status = order.Status.ToString(),
                            error = ex.Message
                        });
                    }
                }

                return Ok(new { 
                    message = "Payment cancellation noted", 
                    orderId = order.Id,
                    status = order.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Payment cancel error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { 
                    message = "Error processing payment cancellation", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Simple payment status check for frontend polling
        /// </summary>
        [HttpGet("check-status/{orderId}")]
        public async Task<IActionResult> CheckPaymentStatus(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found", orderId });
                }

                // Chỉ return status hiện tại, không trigger callback
                return Ok(new { 
                    orderId = order.Id,
                    status = order.Status.ToString(),
                    totalAmount = order.TotalAmount,
                    isPaid = order.Status == OrderStatus.Paid,
                    isCancelled = order.Status == OrderStatus.Cancelled,
                    isPending = order.Status == OrderStatus.Pending
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { 
                    message = "Error checking status", 
                    error = ex.Message 
                });
            }
        }
    }
}
