using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller xử lý tour booking cho user
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserTourBookingController : ControllerBase
    {
        private readonly IUserTourBookingService _userTourBookingService;
        private readonly ICurrentUserService _currentUserService;

        public UserTourBookingController(
            IUserTourBookingService userTourBookingService,
            ICurrentUserService currentUserService)
        {
            _userTourBookingService = userTourBookingService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Lấy danh sách tours có thể booking
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (mặc định: 1)</param>
        /// <param name="pageSize">Số lượng items per page (mặc định: 10)</param>
        /// <param name="fromDate">Lọc từ ngày (optional)</param>
        /// <param name="toDate">Lọc đến ngày (optional)</param>
        /// <param name="searchKeyword">Từ khóa tìm kiếm (optional)</param>
        /// <returns>Danh sách tours có thể booking</returns>
        [HttpGet("available-tours")]
        public async Task<IActionResult> GetAvailableTours(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? searchKeyword = null)
        {
            try
            {
                var result = await _userTourBookingService.GetAvailableToursAsync(
                    pageIndex, pageSize, fromDate, toDate, searchKeyword);

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách tours thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi lấy danh sách tours: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết tour để booking
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Chi tiết tour để booking</returns>
        [HttpGet("tour-details/{tourDetailsId}")]
        public async Task<IActionResult> GetTourDetailsForBooking(Guid tourDetailsId)
        {
            try
            {
                var result = await _userTourBookingService.GetTourDetailsForBookingAsync(tourDetailsId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tour hoặc tour không khả dụng"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy chi tiết tour thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi lấy chi tiết tour: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tính giá tour trước khi booking
        /// </summary>
        /// <param name="request">Thông tin tính giá</param>
        /// <returns>Kết quả tính giá</returns>
        [HttpPost("calculate-price")]
        public async Task<IActionResult> CalculateBookingPrice([FromBody] CalculatePriceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState
                    });
                }

                var result = await _userTourBookingService.CalculateBookingPriceAsync(request);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tour operation"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Tính giá thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi tính giá: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo booking tour mới
        /// </summary>
        /// <param name="request">Thông tin booking</param>
        /// <returns>Kết quả tạo booking</returns>
        [HttpPost("create-booking")]
        [Authorize]
        public async Task<IActionResult> CreateBooking([FromBody] CreateTourBookingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { 
                            Field = x.Key, 
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage) 
                        })
                        .ToList();

                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = errors
                    });
                }

                var userId = _currentUserService.GetCurrentUserId();
                var result = await _userTourBookingService.CreateBookingAsync(request, userId);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi tạo booking: {ex.Message}",
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bookings của user hiện tại
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (mặc định: 1)</param>
        /// <param name="pageSize">Số lượng items per page (mặc định: 10)</param>
        /// <returns>Danh sách bookings của user</returns>
        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                var result = await _userTourBookingService.GetUserBookingsAsync(userId, pageIndex, pageSize);

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách bookings thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi lấy danh sách bookings: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết booking theo ID
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <returns>Chi tiết booking</returns>
        [HttpGet("booking-details/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> GetBookingDetails(Guid bookingId)
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                var result = await _userTourBookingService.GetBookingDetailsAsync(bookingId, userId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy booking"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy chi tiết booking thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi lấy chi tiết booking: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Hủy booking
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="reason">Lý do hủy (optional)</param>
        /// <returns>Kết quả hủy booking</returns>
        [HttpPost("cancel-booking/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(Guid bookingId, [FromBody] string? reason = null)
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                var result = await _userTourBookingService.CancelBookingAsync(bookingId, userId, reason);

                if (result.StatusCode != 200)
                {
                    return StatusCode(result.StatusCode, new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi hủy booking: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Test dependencies
        /// </summary>
        [HttpGet("debug-dependencies")]
        [Authorize]
        public async Task<IActionResult> DebugDependencies()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                var result = new
                {
                    userId = userId,
                    currentUserService = _currentUserService?.GetType().Name ?? "NULL",
                    userTourBookingService = _userTourBookingService?.GetType().Name ?? "NULL",
                    timestamp = DateTime.UtcNow
                };

                return Ok(new
                {
                    success = true,
                    message = "Dependencies debug info",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error in dependencies: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Test create booking step by step
        /// </summary>
        [HttpPost("debug-create-booking")]
        [Authorize]
        public async Task<IActionResult> DebugCreateBooking([FromBody] CreateTourBookingRequest request)
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                var debugInfo = new List<string>();

                debugInfo.Add($"Step 1: UserId = {userId}");
                debugInfo.Add($"Step 2: Request validation - TourSlotId: {request.TourSlotId}, TourOperationId: {request.TourOperationId}, Guests: {request.NumberOfGuests}");

                // Step 3: Test calling the service directly
                try
                {
                    var result = await _userTourBookingService.CreateBookingAsync(request, userId);
                    
                    debugInfo.Add($"Step 3: Service call result - Success: {result.Success}");
                    debugInfo.Add($"Step 4: Service call message - {result.Message}");

                    if (result.Success)
                    {
                        debugInfo.Add($"Step 5: Booking created - ID: {result.BookingId}, Code: {result.BookingCode}");
                    }

                    return Ok(new
                    {
                        success = result.Success,
                        message = "Debug create booking completed",
                        debugInfo = debugInfo,
                        serviceResult = result
                    });
                }
                catch (Exception serviceEx)
                {
                    debugInfo.Add($"Step 3 ERROR: Service exception - {serviceEx.Message}");
                    debugInfo.Add($"Step 3 STACK: {serviceEx.StackTrace}");
                    
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Service call failed",
                        debugInfo = debugInfo,
                        error = serviceEx.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Controller error: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Test payment URL generation
        /// </summary>
        [HttpPost("debug-payment-url")]
        [Authorize]
        public async Task<IActionResult> DebugPaymentUrl([FromBody] CreateTourBookingRequest request)
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                var debugInfo = new List<string>();

                debugInfo.Add($"Step 1: Generate booking code and PayOS order code");
                
                // Test generating codes
                var bookingCode = $"TB{DateTime.UtcNow:yyyyMMdd}{new Random().Next(100000, 999999)}";
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                var timestampLast7 = timestamp.Substring(Math.Max(0, timestamp.Length - 7));
                var random = new Random().Next(100, 999);
                var payOsOrderCode = $"TNDT{timestampLast7}{random}";
                
                debugInfo.Add($"Step 2: Booking Code = {bookingCode}");
                debugInfo.Add($"Step 3: PayOS Order Code = {payOsOrderCode}");

                // Test amount calculation
                var testAmount = 500000m; // Test with 500,000 VND
                debugInfo.Add($"Step 4: Test Amount = {testAmount:N0} VND");

                // Test PayOS service
                try
                {
                    var payOsService = HttpContext.RequestServices.GetRequiredService<IPayOsService>();
                    if (payOsService != null)
                    {
                        debugInfo.Add($"Step 5: PayOsService found - {payOsService.GetType().Name}");
                        
                        // FIXED: Use correct method for tour booking debug
                        var paymentUrl = await payOsService.CreateTourBookingPaymentUrlAsync(
                            testAmount,
                            payOsOrderCode,
                            "https://tndt.netlify.app");
                        
                        debugInfo.Add($"Step 6: Payment URL created successfully");
                        debugInfo.Add($"Step 7: Payment URL = {paymentUrl}");

                        return Ok(new
                        {
                            success = true,
                            message = "PayOS payment URL generation test completed",
                            debugInfo = debugInfo,
                            paymentData = new
                            {
                                bookingCode,
                                payOsOrderCode,
                                testAmount,
                                paymentUrl
                            }
                        });
                    }
                    else
                    {
                        debugInfo.Add($"Step 5: PayOsService is NULL");
                        return BadRequest(new
                        {
                            success = false,
                            message = "PayOsService not available",
                            debugInfo = debugInfo
                        });
                    }
                }
                catch (Exception payOsEx)
                {
                    debugInfo.Add($"Step 5 ERROR: PayOS service error - {payOsEx.Message}");
                    debugInfo.Add($"Step 5 STACK: {payOsEx.StackTrace}");
                    
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "PayOS service failed",
                        debugInfo = debugInfo,
                        error = payOsEx.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Debug payment URL error: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
