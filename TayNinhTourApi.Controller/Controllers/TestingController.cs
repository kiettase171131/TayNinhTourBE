using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.BusinessLogicLayer.Tests;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller for testing utilities - Skip time to enable testing of tour features
    /// FOR TESTING PURPOSES ONLY - Should be disabled in production
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITourPricingService _pricingService;
        private readonly ILogger<TestingController> _logger;

        public TestingController(
            IUnitOfWork unitOfWork,
            ITourPricingService pricingService,
            ILogger<TestingController> logger)
        {
            _unitOfWork = unitOfWork;
            _pricingService = pricingService;
            _logger = logger;
        }

        /// <summary>
        /// Run Early Bird Pricing Tests - Test tính giá early bird discount
        /// </summary>
        /// <returns>Kết quả của tất cả early bird tests</returns>
        [HttpGet("early-bird-pricing-tests")]
        [ProducesResponseType(typeof(object), 200)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> RunEarlyBirdPricingTests()
        {
            try
            {
                _logger.LogInformation("🧪 Running Early Bird Pricing Tests via API");

                var testResults = TestRunner.RunAllTests();

                var result = new
                {
                    success = testResults.FailedTests == 0,
                    message = testResults.FailedTests == 0 
                        ? "🎉 Tất cả Early Bird tests đều PASSED!" 
                        : $"⚠️ {testResults.FailedTests} test(s) FAILED",
                    
                    summary = new
                    {
                        totalTests = testResults.TotalTests,
                        passedTests = testResults.PassedTests,
                        failedTests = testResults.FailedTests,
                        successRate = $"{testResults.SuccessRate:P2}",
                        executionTime = $"{testResults.TotalExecutionTime.TotalMilliseconds:F2}ms"
                    },
                    
                    testDetails = testResults.AllResults.Select(test => new
                    {
                        testName = test.TestName,
                        status = test.Passed ? "✅ PASSED" : "❌ FAILED",
                        executionTime = $"{test.ExecutionTime.TotalMilliseconds:F2}ms",
                        errorMessage = test.ErrorMessage
                    }).ToList(),
                    
                    earlyBirdRules = new
                    {
                        discountPercent = "25%",
                        timeWindow = "15 ngày đầu sau khi tạo tour",
                        minimumNotice = "Tour phải khởi hành sau ít nhất 30 ngày từ ngày đặt",
                        logic = "daysSinceCreated <= 15 AND daysUntilTour >= 30"
                    },
                    
                    passedTests = testResults.PassedTestResults.Select(test => new
                    {
                        name = test.TestName,
                        executionTime = $"{test.ExecutionTime.TotalMilliseconds:F2}ms"
                    }).ToList(),
                    
                    failedTests = testResults.FailedTestResults.Select(test => new
                    {
                        name = test.TestName,
                        error = test.ErrorMessage,
                        executionTime = $"{test.ExecutionTime.TotalMilliseconds:F2}ms"
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Early Bird pricing tests");
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi chạy Early Bird tests: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Test Early Bird pricing với thông số tùy chỉnh
        /// </summary>
        /// <param name="originalPrice">Giá gốc tour</param>
        /// <param name="daysSinceCreated">Số ngày kể từ khi tạo tour</param>
        /// <param name="daysUntilTour">Số ngày từ bây giờ đến khi tour khởi hành</param>
        /// <returns>Kết quả tính giá với early bird</returns>
        [HttpGet("test-early-bird-pricing")]
        [ProducesResponseType(typeof(object), 200)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> TestEarlyBirdPricing(
            [FromQuery] decimal originalPrice = 1000000,
            [FromQuery] int daysSinceCreated = 10,
            [FromQuery] int daysUntilTour = 45)
        {
            try
            {
                _logger.LogInformation("🧮 Testing Early Bird pricing with custom parameters: Price={Price}, DaysSinceCreated={DaysSinceCreated}, DaysUntilTour={DaysUntilTour}",
                    originalPrice, daysSinceCreated, daysUntilTour);

                var currentDate = DateTime.UtcNow;
                var tourCreatedDate = currentDate.AddDays(-daysSinceCreated);
                var tourStartDate = currentDate.AddDays(daysUntilTour);

                // Get pricing info
                var pricingInfo = _pricingService.GetPricingInfo(
                    originalPrice,
                    tourStartDate,
                    tourCreatedDate,
                    currentDate);

                // Calculate for multiple guests
                var guestCounts = new[] { 1, 2, 4, 6 };
                var pricingForGuests = guestCounts.Select(guests => new
                {
                    numberOfGuests = guests,
                    originalTotal = originalPrice * guests,
                    finalTotal = pricingInfo.FinalPrice * guests,
                    savings = (originalPrice - pricingInfo.FinalPrice) * guests
                }).ToList();

                var result = new
                {
                    success = true,
                    message = "Early Bird pricing calculation completed",
                    
                    inputParameters = new
                    {
                        originalPrice = $"{originalPrice:N0} VND",
                        daysSinceCreated,
                        daysUntilTour,
                        tourCreatedDate = tourCreatedDate.ToString("dd/MM/yyyy"),
                        tourStartDate = tourStartDate.ToString("dd/MM/yyyy"),
                        currentDate = currentDate.ToString("dd/MM/yyyy")
                    },
                    
                    pricingResults = new
                    {
                        isEarlyBird = pricingInfo.IsEarlyBird,
                        pricingType = pricingInfo.PricingType,
                        discountPercent = pricingInfo.DiscountPercent,
                        originalPrice = $"{pricingInfo.OriginalPrice:N0} VND",
                        finalPrice = $"{pricingInfo.FinalPrice:N0} VND",
                        discountAmount = $"{pricingInfo.DiscountAmount:N0} VND",
                        daysSinceCreated = pricingInfo.DaysSinceCreated,
                        daysUntilTour = pricingInfo.DaysUntilTour
                    },
                    
                    eligibilityCheck = new
                    {
                        withinEarlyBirdWindow = daysSinceCreated <= 15,
                        sufficientNotice = daysUntilTour >= 30,
                        meetsAllConditions = pricingInfo.IsEarlyBird,
                        explanation = pricingInfo.IsEarlyBird
                            ? "✅ Đủ điều kiện Early Bird - Đặt trong 15 ngày đầu và tour sau ít nhất 30 ngày"
                            : daysSinceCreated > 15
                                ? "❌ Không đủ điều kiện - Đã quá 15 ngày kể từ khi mở bán"
                                : "❌ Không đủ điều kiện - Tour khởi hành quá gần (< 30 ngày)"
                    },
                    
                    pricingForDifferentGuests = pricingForGuests,
                    
                    earlyBirdRules = new
                    {
                        windowDays = 15,
                        minimumNoticeDays = 30,
                        discountPercent = 25,
                        calculation = "finalPrice = originalPrice * (1 - 0.25) if eligible"
                    },
                    
                    recommendation = pricingInfo.IsEarlyBird
                        ? $"🎉 Khuyến nghị: Quảng bá Early Bird discount {pricingInfo.DiscountPercent}%!"
                        : "ℹ️ Tour này không áp dụng Early Bird discount"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Early Bird pricing");
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi test Early Bird pricing: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Performance test cho Early Bird pricing service
        /// </summary>
        /// <param name="iterations">Số lần chạy test (default: 1000)</param>
        /// <returns>Kết quả performance test</returns>
        [HttpGet("early-bird-performance-test")]
        [ProducesResponseType(typeof(object), 200)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> RunEarlyBirdPerformanceTest([FromQuery] int iterations = 1000)
        {
            try
            {
                _logger.LogInformation("⚡ Running Early Bird Performance Test with {Iterations} iterations", iterations);

                var startTime = DateTime.UtcNow;
                var successCount = 0;
                var failCount = 0;
                var executionTimes = new List<double>();

                // Test data
                var testCases = new[]
                {
                    new { Price = 500000m, DaysCreated = 5, DaysUntil = 45 },
                    new { Price = 1000000m, DaysCreated = 10, DaysUntil = 35 },
                    new { Price = 1500000m, DaysCreated = 20, DaysUntil = 25 },
                    new { Price = 2000000m, DaysCreated = 8, DaysUntil = 60 }
                };

                for (int i = 0; i < iterations; i++)
                {
                    var testCase = testCases[i % testCases.Length];
                    var iterationStart = DateTime.UtcNow;

                    try
                    {
                        var currentDate = DateTime.UtcNow;
                        var createdDate = currentDate.AddDays(-testCase.DaysCreated);
                        var startDate = currentDate.AddDays(testCase.DaysUntil);

                        var pricingInfo = _pricingService.GetPricingInfo(
                            testCase.Price,
                            startDate,
                            createdDate,
                            currentDate);

                        // Verify result is reasonable
                        if (pricingInfo.OriginalPrice == testCase.Price &&
                            pricingInfo.FinalPrice > 0 &&
                            pricingInfo.FinalPrice <= testCase.Price)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                    catch
                    {
                        failCount++;
                    }
                    finally
                    {
                        var iterationTime = (DateTime.UtcNow - iterationStart).TotalMilliseconds;
                        executionTimes.Add(iterationTime);
                    }
                }

                var totalTime = DateTime.UtcNow - startTime;
                var avgTime = executionTimes.Average();
                var minTime = executionTimes.Min();
                var maxTime = executionTimes.Max();

                var result = new
                {
                    success = true,
                    message = "Early Bird Performance Test completed",
                    
                    testParameters = new
                    {
                        iterations,
                        testCasesUsed = testCases.Length,
                        testDuration = $"{totalTime.TotalSeconds:F2} seconds"
                    },
                    
                    performanceMetrics = new
                    {
                        totalIterations = iterations,
                        successfulIterations = successCount,
                        failedIterations = failCount,
                        successRate = $"{(double)successCount / iterations:P2}",
                        
                        timing = new
                        {
                            totalTime = $"{totalTime.TotalMilliseconds:F2}ms",
                            averageTimePerIteration = $"{avgTime:F4}ms",
                            minTimePerIteration = $"{minTime:F4}ms",
                            maxTimePerIteration = $"{maxTime:F4}ms",
                            operationsPerSecond = $"{iterations / totalTime.TotalSeconds:F0}"
                        }
                    },
                    
                    benchmarkResults = new
                    {
                        isPerformant = avgTime < 1.0, // Should be under 1ms per operation
                        performance = avgTime switch
                        {
                            < 0.1 => "🚀 Excellent (< 0.1ms)",
                            < 0.5 => "✅ Very Good (< 0.5ms)",
                            < 1.0 => "👍 Good (< 1.0ms)",  
                            < 5.0 => "⚠️ Acceptable (< 5.0ms)",
                            _ => "❌ Needs Optimization (> 5.0ms)"
                        },
                        recommendation = avgTime > 1.0 
                            ? "Consider caching or optimization for pricing calculations"
                            : "Performance is excellent for production use"
                    },
                    
                    testCases = testCases.Select((tc, idx) => new
                    {
                        caseIndex = idx + 1,
                        price = $"{tc.Price:N0} VND",
                        daysSinceCreated = tc.DaysCreated,
                        daysUntilTour = tc.DaysUntil,
                        expectedEarlyBird = tc.DaysCreated <= 15 && tc.DaysUntil >= 30
                    })
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Early Bird performance test");
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi chạy performance test: {ex.Message}"
                });
            }
        }


        /// <summary>
        /// Skip time to tour start date to enable testing of tour features
        /// This simulates time passing so you can test:
        /// - Auto-cancel tours with insufficient bookings
        /// - Tour guide QR code scanning
        /// - Tour progress updates
        /// - Revenue transfer after tour completion
        /// </summary>
        /// <param name="tourSlotId">ID c?a tour slot c?n skip time t?i</param>
        /// <returns>Information about the tour state after time skip</returns>
        [HttpPost("skip-to-tour-start/{tourSlotId}")]
        [ProducesResponseType(typeof(object), 200)]  
        [ProducesResponseType(typeof(BaseResposeDto), 400)]
        [ProducesResponseType(typeof(BaseResposeDto), 404)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SkipToTourStart([FromRoute] Guid tourSlotId)
        {
            try
            {
                _logger.LogInformation("🔎 TESTING: Skipping time to tour start for slot {SlotId}", tourSlotId);

                // Get tour slot with full details
                var tourSlot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(ts => ts.Bookings.Where(b => !b.IsDeleted))
                        .ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId && !ts.IsDeleted);

                if (tourSlot == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour slot"
                    });
                }

                if (tourSlot.TourDetails?.TourOperation == null)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot chưa có tour operation"
                    });
                }

                var tourStartDate = tourSlot.TourDate.ToDateTime(TimeOnly.MinValue);
                var currentTime = DateTime.UtcNow;

                if (tourStartDate <= currentTime)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Tour đã bắt đầu hoặc đã qua. Ngày tour: {tourStartDate:dd/MM/yyyy}, Hiện tại: {currentTime:dd/MM/yyyy}"
                    });
                }

                var allBookings = tourSlot.Bookings.Where(b => !b.IsDeleted).ToList();
                var confirmedBookings = allBookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();
                var pendingBookings = allBookings.Where(b => b.Status == BookingStatus.Pending).ToList();

                var totalConfirmedGuests = confirmedBookings.Sum(b => b.NumberOfGuests);
                var totalPendingGuests = pendingBookings.Sum(b => b.NumberOfGuests);

                // Use execution strategy to handle MySQL retry policy with transactions
                var executionStrategy = _unitOfWork.GetExecutionStrategy();

                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Cancel pending bookings (they hadn't paid in time)
                        foreach (var pendingBooking in pendingBookings)
                        {
                            pendingBooking.Status = BookingStatus.CancelledByCompany;
                            pendingBooking.CancelledDate = DateTime.UtcNow;
                            pendingBooking.CancellationReason = "Tour bắt đầu, booking chưa thanh toán bị tự động huỷ";
                            pendingBooking.UpdatedAt = DateTime.UtcNow;
                            await _unitOfWork.TourBookingRepository.UpdateAsync(pendingBooking);
                        }

                        // Update tour slot to InProgress so testing features can be used
                        tourSlot.Status = TourSlotStatus.InProgress;
                        tourSlot.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.TourSlotRepository.UpdateAsync(tourSlot);

                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("⏩ Time skip completed for slot {SlotId} - Tour is now InProgress", tourSlotId);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error during time skip for slot {SlotId}", tourSlotId);
                        throw;
                    }
                });

                // Return current state for testing
                var result = new
                {
                    success = true,
                    message = "⏩ Time skip thành công! Tour giờ đã ở trạng thái 'Đang thực hiện'",
                    tourInfo = new
                    {
                        slotId = tourSlot.Id,
                        tourTitle = tourSlot.TourDetails.Title,
                        tourDate = tourStartDate,
                        status = tourSlot.Status.ToString(),
                        maxGuests = tourSlot.TourDetails.TourOperation.MaxGuests
                    },
                    bookingInfo = new
                    {
                        totalBookings = allBookings.Count,
                        confirmedBookings = confirmedBookings.Count,
                        cancelledPendingBookings = pendingBookings.Count,
                        totalConfirmedGuests = totalConfirmedGuests,
                        totalCancelledGuests = pendingBookings.Sum(b => b.NumberOfGuests),
                        confirmedBookingIds = confirmedBookings.Select(b => b.Id).ToList()
                    },
                    testingFeatures = new
                    {
                        qrCodeScanning = new
                        {
                            available = true,
                            description = "HDV giờ có thể quét QR code của khách để check-in",
                            endpoint = "/api/TourGuide/scan-qr",
                            bookingsWithQR = confirmedBookings.Where(b => !string.IsNullOrEmpty(b.QRCodeData)).Count()
                        },
                        tourProgressUpdate = new
                        {
                            available = true,
                            description = "Có thể cập nhật tiến độ tour và hoàn thành tour",
                            completeEndpoint = "/api/Testing/complete-tour/" + tourSlotId
                        },
                        autoCancelCheck = new
                        {
                            wasEligible = totalConfirmedGuests < 2, // Assuming minimum 2 guests
                            description = totalConfirmedGuests < 2
                                ? "Tour này có thể đã bị auto-cancel do không đủ khách (< 50% sức chứa)"
                                : "Tour có đủ khách để tiến hành"
                        }
                    },
                    nextSteps = new[]
                    {
                "🔎 HDV có thể scan QR code của khách để check-in",
                "📝 Có thể cập nhật tiến độ tour",
                "💵 Sau khi hoàn thành tour, revenue sẽ được transfer",
                $"📊 Dự kiến revenue: {confirmedBookings.Sum(b => b.TotalPrice):N0} VNĐ"
            }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during time skip simulation for slot {SlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi thực hiện time skip: {ex.Message}"
                });
            }
        }


        /// <summary>
        /// Complete a tour for testing revenue transfer
        /// </summary>
        /// <param name="tourSlotId">ID c?a tour slot c?n hoàn thành</param>
        /// <returns>Information about completed tour</returns>
        [HttpPost("complete-tour/{tourSlotId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 400)]
        [ProducesResponseType(typeof(BaseResposeDto), 404)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> CompleteTour([FromRoute] Guid tourSlotId)
        {
            try
            {
                _logger.LogInformation("✅ TESTING: Completing tour for slot {SlotId}", tourSlotId);

                var tourSlot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(ts => ts.Bookings.Where(b => !b.IsDeleted && b.Status == BookingStatus.Confirmed))
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId && !ts.IsDeleted);

                if (tourSlot == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour slot"
                    });
                }

                if (tourSlot.Status != TourSlotStatus.InProgress)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Tour phải ở trạng thái 'Đang thực hiện' để có thể hoàn thành. Trạng thái hiện tại: {tourSlot.Status}"
                    });
                }

                var confirmedBookings = tourSlot.Bookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();

                // Use execution strategy to handle MySQL retry policy with transactions
                var executionStrategy = _unitOfWork.GetExecutionStrategy();

                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        tourSlot.Status = TourSlotStatus.Completed;
                        tourSlot.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.TourSlotRepository.UpdateAsync(tourSlot);

                        foreach (var booking in confirmedBookings)
                        {
                            booking.Status = BookingStatus.Completed;
                            booking.UpdatedAt = DateTime.UtcNow;
                            await _unitOfWork.TourBookingRepository.UpdateAsync(booking);
                        }

                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("✅ Tour completed for slot {SlotId}", tourSlotId);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                var totalRevenue = confirmedBookings.Sum(b => b.TotalPrice);
                var totalRevenueHold = confirmedBookings.Sum(b => b.RevenueHold);

                var result = new
                {
                    success = true,
                    message = "✅ Tour đã hoàn thành thành công!",
                    tourInfo = new
                    {
                        slotId = tourSlot.Id,
                        tourTitle = tourSlot.TourDetails?.Title,
                        tourDate = tourSlot.TourDate,
                        status = tourSlot.Status.ToString(),
                        completedBookings = confirmedBookings.Count
                    },
                    revenueInfo = new
                    {
                        totalRevenue = totalRevenue,
                        totalRevenueHold = totalRevenueHold,
                        revenueTransferEligibleIn = "3 ngày",
                        description = "Revenue sẽ được tự động chuyển cho tour company sau 3 ngày"
                    },
                    testingFeatures = new
                    {
                        revenueTransfer = new
                        {
                            available = true,
                            description = "Background service sẽ tự động chuyển revenue sau 3 ngày, hoặc có thể test manually",
                            manualTestNote = "Có thể tạo API test để trigger chuyển revenue ngay lập tức"
                        }
                    },
                    nextSteps = new[]
                    {
                "💵 Revenue đang được giữ, sẽ chuyển sau 3 ngày",
                "🏢 Tour company sẽ nhận thông báo khi revenue được chuyển",
                "📊 Có thể kiểm tra trạng thái revenue trong dashboard"
            }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing tour for slot {SlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi hoàn thành tour: {ex.Message}"
                });
            }
        }


        /// <summary>
        /// Get tour slot information for testing
        /// </summary>
        /// <param name="tourSlotId">ID c?a tour slot</param>
        /// <returns>Current tour slot state and available testing features</returns>
        [HttpGet("tour-info/{tourSlotId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 404)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetTourInfo([FromRoute] Guid tourSlotId)
        {
            try
            {
                var tourSlot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(ts => ts.Bookings.Where(b => !b.IsDeleted))
                        .ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId && !ts.IsDeleted);

                if (tourSlot == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour slot"
                    });
                }

                var currentTime = DateTime.UtcNow;
                var tourDate = tourSlot.TourDate.ToDateTime(TimeOnly.MinValue);
                var bookings = tourSlot.Bookings.Where(b => !b.IsDeleted).ToList();

                var result = new
                {
                    tourSlot = new
                    {
                        id = tourSlot.Id,
                        tourTitle = tourSlot.TourDetails?.Title,
                        tourDate = tourDate,
                        status = tourSlot.Status.ToString(),
                        maxGuests = tourSlot.TourDetails?.TourOperation?.MaxGuests ?? tourSlot.MaxGuests,
                        currentBookings = tourSlot.CurrentBookings,
                        isActive = tourSlot.IsActive
                    },
                    timeInfo = new
                    {
                        currentTime = currentTime,
                        tourStartTime = tourDate,
                        daysUntilTour = (tourDate.Date - currentTime.Date).Days,
                        hoursUntilTour = Math.Round((tourDate - currentTime).TotalHours, 1),
                        isTourStarted = tourDate <= currentTime,
                        isPastTour = tourDate.Date < currentTime.Date
                    },
                    bookings = new
                    {
                        total = bookings.Count,
                        confirmed = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                        pending = bookings.Count(b => b.Status == BookingStatus.Pending),
                        cancelled = bookings.Count(b => 
                            b.Status == BookingStatus.CancelledByCustomer || 
                            b.Status == BookingStatus.CancelledByCompany),
                        completed = bookings.Count(b => b.Status == BookingStatus.Completed),
                        totalGuests = bookings.Where(b => 
                            b.Status == BookingStatus.Confirmed || 
                            b.Status == BookingStatus.Pending).Sum(b => b.NumberOfGuests),
                        totalRevenue = bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.TotalPrice),
                        bookingsWithQR = bookings.Count(b => !string.IsNullOrEmpty(b.QRCodeData))
                    },
                    testingActions = new
                    {
                        canSkipToTourStart = tourDate > currentTime && tourSlot.Status == TourSlotStatus.Available,
                        canCompleteTour = tourSlot.Status == TourSlotStatus.InProgress,
                        canUseQRScanning = tourSlot.Status == TourSlotStatus.InProgress,
                        canTriggerRevenueTransfer = tourSlot.Status == TourSlotStatus.Completed &&
                            bookings.Any(b => b.Status == BookingStatus.Completed && b.RevenueHold > 0)
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour info for slot {SlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi lấy thông tin tour: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Manually trigger auto-cancel for a specific tour slot (TESTING ONLY)
        /// This will cancel a specific tour slot if it meets the under-booked criteria
        /// </summary>
        /// <param name="tourSlotId">ID c?a tour slot c?n cancel</param>
        /// <returns>Result of auto-cancel process for the specific slot</returns>
        [HttpPost("trigger-auto-cancel/{tourSlotId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 400)]
        [ProducesResponseType(typeof(BaseResposeDto), 404)]
        [ProducesResponseType(typeof(BaseResposeDto), 500)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> TriggerAutoCancelForSlot([FromRoute] Guid tourSlotId)
        {
            try
            {
                _logger.LogInformation("🚨 TESTING: Manual trigger auto-cancel for specific slot {SlotId}", tourSlotId);

                // Get tour slot with full details
                var tourSlot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(ts => ts.Bookings.Where(b => !b.IsDeleted))
                        .ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId && !ts.IsDeleted);

                if (tourSlot == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour slot"
                    });
                }

                if (tourSlot.TourDetails?.TourOperation == null)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot chưa có tour operation"
                    });
                }

                if (tourSlot.TourDetails.Status != TourDetailsStatus.Public)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Chỉ có thể auto-cancel tour ở trạng thái Public"
                    });
                }

                var allBookings = tourSlot.Bookings.Where(b => !b.IsDeleted).ToList();
                var confirmedBookings = allBookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();
                var totalConfirmedGuests = confirmedBookings.Sum(b => b.NumberOfGuests);
                var maxGuests = tourSlot.TourDetails.TourOperation.MaxGuests;

                // Calculate booking rate
                var guestBookingRate = maxGuests > 0 ? (double)totalConfirmedGuests / maxGuests : 0;

                _logger.LogInformation("Tour slot {SlotId} analysis: {ConfirmedGuests}/{MaxGuests} guests ({BookingRate:P})",
                    tourSlotId, totalConfirmedGuests, maxGuests, guestBookingRate);

                // Check if eligible for auto-cancel (< 50% capacity)
                if (guestBookingRate >= 0.5)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Tour slot có đủ khách ({guestBookingRate:P} >= 50% sức chứa). Không thể auto-cancel."
                    });
                }

                if (!confirmedBookings.Any())
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot không có booking confirmed nào để cancel"
                    });
                }

                // Perform auto-cancel
                var executionStrategy = _unitOfWork.GetExecutionStrategy();
                var emailsCount = 0;

                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Cancel bookings
                        foreach (var booking in confirmedBookings)
                        {
                            booking.Status = BookingStatus.CancelledByCompany;
                            booking.CancelledDate = DateTime.UtcNow;
                            booking.CancellationReason = $"Tour bị huỷ tự động do không đủ khách ({guestBookingRate:P} < 50% sức chứa) - MANUAL TRIGGER";
                            booking.UpdatedAt = DateTime.UtcNow;
                            await _unitOfWork.TourBookingRepository.UpdateAsync(booking);
                        }

                        // Cancel slot
                        tourSlot.Status = TourSlotStatus.Cancelled;
                        tourSlot.IsActive = false;
                        tourSlot.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.TourSlotRepository.UpdateAsync(tourSlot);

                        // Update tour operation current bookings
                        var tourOperation = tourSlot.TourDetails.TourOperation;
                        var totalGuestsToRelease = confirmedBookings.Sum(b => b.NumberOfGuests);
                        tourOperation.CurrentBookings = Math.Max(0, tourOperation.CurrentBookings - totalGuestsToRelease);
                        tourOperation.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.TourOperationRepository.UpdateAsync(tourOperation);

                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Successfully cancelled tour slot {SlotId}", tourSlotId);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error in transaction for tour slot {SlotId}", tourSlotId);
                        throw;
                    }
                });

                // Send cancellation emails to customers (after transaction commit)
                emailsCount = await SendCancellationEmailsToCustomersAsync(
                    confirmedBookings,
                    tourSlot.TourDetails.Title,
                    $"Tour bị huỷ tự động do không đủ khách ({guestBookingRate:P} < 50% sức chứa)"
                );

                var result = new
                {
                    success = true,
                    message = "🚨 Auto-cancel thành công cho tour slot cụ thể!",
                    tourSlotInfo = new
                    {
                        slotId = tourSlot.Id,
                        tourTitle = tourSlot.TourDetails.Title,
                        tourDate = tourSlot.TourDate,
                        previousStatus = TourSlotStatus.Available.ToString(),
                        newStatus = TourSlotStatus.Cancelled.ToString(),
                        bookingRate = $"{guestBookingRate:P}"
                    },
                    cancellationResults = new
                    {
                        totalBookingsCancelled = confirmedBookings.Count,
                        totalGuestsCancelled = totalConfirmedGuests,
                        totalEmailsSent = emailsCount,
                        reason = $"Không đủ khách ({guestBookingRate:P} < 50% sức chứa)"
                    },
                    affectedBookings = confirmedBookings.Select(b => new
                    {
                        bookingId = b.Id,
                        bookingCode = b.BookingCode,
                        customerName = !string.IsNullOrEmpty(b.ContactName) ? b.ContactName : b.User?.Name ?? "Unknown",
                        customerEmail = !string.IsNullOrEmpty(b.ContactEmail) ? b.ContactEmail : b.User?.Email ?? "",
                        numberOfGuests = b.NumberOfGuests,
                        totalPrice = b.TotalPrice,
                        refundAmount = b.TotalPrice
                    }).ToList(),
                    nextSteps = new[]
                    {
                "📧 Khách hàng đã nhận email thông báo huỷ tour",
                "💵 Tiền sẽ được hoàn trả tự động trong 3-5 ngày",
                "❌ Tour slot đã chuyển sang trạng thái Cancelled",
                "🔄 Sức chứa đã được giải phóng khỏi tour operation"
            }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual auto-cancel for slot {SlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi thực hiện auto-cancel: {ex.Message}"
                });
            }
        }


        /// <summary>
        /// Check if a tour slot is eligible for auto-cancel
        /// </summary>
        /// <param name="tourSlotId">ID c?a tour slot c?n ki?m tra</param>
        /// <returns>Information about auto-cancel eligibility</returns>
        [HttpGet("check-auto-cancel-eligibility/{tourSlotId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 404)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> CheckAutoCancelEligibility([FromRoute] Guid tourSlotId)
        {
            try
            {
                var tourSlot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(ts => ts.Bookings.Where(b => !b.IsDeleted))
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId && !ts.IsDeleted);

                if (tourSlot == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour slot"
                    });
                }

                var confirmedBookings = tourSlot.Bookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();
                var totalConfirmedGuests = confirmedBookings.Sum(b => b.NumberOfGuests);
                var maxGuests = tourSlot.TourDetails?.TourOperation?.MaxGuests ?? tourSlot.MaxGuests;
                var guestBookingRate = maxGuests > 0 ? (double)totalConfirmedGuests / maxGuests : 0;

                var isEligible = guestBookingRate < 0.5 &&
                                confirmedBookings.Any() &&
                                tourSlot.TourDetails?.Status == TourDetailsStatus.Public &&
                                tourSlot.IsActive;

                var result = new
                {
                    tourSlotInfo = new
                    {
                        slotId = tourSlot.Id,
                        tourTitle = tourSlot.TourDetails?.Title,
                        tourDate = tourSlot.TourDate,
                        status = tourSlot.Status.ToString(),
                        isActive = tourSlot.IsActive
                    },
                    bookingAnalysis = new
                    {
                        maxGuests = maxGuests,
                        confirmedGuests = totalConfirmedGuests,
                        confirmedBookings = confirmedBookings.Count,
                        bookingRate = $"{guestBookingRate:P}",
                        hasConfirmedBookings = confirmedBookings.Any()
                    },
                    eligibilityCheck = new
                    {
                        isEligibleForAutoCancel = isEligible,
                        reasons = new
                        {
                            bookingRateUnder50Percent = guestBookingRate < 0.5,
                            hasConfirmedBookings = confirmedBookings.Any(),
                            tourIsPublic = tourSlot.TourDetails?.Status == TourDetailsStatus.Public,
                            slotIsActive = tourSlot.IsActive
                        }
                    },
                    recommendation = isEligible
                        ? "✅ Tour slot này có thể được auto-cancel vì có < 50% sức chứa"
                        : "ℹ️ Tour slot này không thể auto-cancel"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking auto-cancel eligibility for slot {SlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi kiểm tra điều kiện auto-cancel: {ex.Message}"
                });
            }
        }


        /// <summary>
        /// Send cancellation emails to customers (similar to TourSlotService implementation)
        /// </summary>
        private async Task<int> SendCancellationEmailsToCustomersAsync(
    List<DataAccessLayer.Entities.TourBooking> bookings,
    string tourTitle,
    string reason)
        {
            var emailSender = HttpContext.RequestServices.GetRequiredService<EmailSender>();
            int successCount = 0;

            foreach (var booking in bookings)
            {
                try
                {
                    // Determine customer info - prioritize ContactEmail from booking
                    var customerName = !string.IsNullOrEmpty(booking.ContactName)
                        ? booking.ContactName
                        : booking.User?.Name ?? "Khách hàng";

                    var customerEmail = !string.IsNullOrEmpty(booking.ContactEmail)
                        ? booking.ContactEmail
                        : booking.User?.Email ?? "";

                    // Validate email
                    if (string.IsNullOrEmpty(customerEmail) || !IsValidEmail(customerEmail))
                    {
                        _logger.LogWarning("Invalid email for booking {BookingCode}: {Email}", booking.BookingCode, customerEmail);
                        continue;
                    }

                    var subject = $"⛔ Thông báo hủy tour: {tourTitle}";
                    var htmlBody = $@"
                <h2>Kính chào {customerName},</h2>
                
                <div style='background-color: #f8d7da; padding: 20px; border-left: 4px solid #dc3545; margin: 15px 0;'>
                    <h3 style='margin-top: 0; color: #721c24;'>⛔ THÔNG BÁO HỦY TOUR</h3>
                    <p style='font-size: 16px; margin-bottom: 0;'>
                        Chúng tôi rất tiếc phải thông báo rằng tour <strong>'{tourTitle}'</strong> đã bị hủy.
                    </p>
                </div>
                
                <h3>📝 Thông tin booking của bạn:</h3>
                <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #6c757d; margin: 10px 0;'>
                    <ul style='margin: 0; list-style: none; padding: 0;'>
                        <li><strong>🔖 Mã booking:</strong> {booking.BookingCode}</li>
                        <li><strong>👥 Số lượng khách:</strong> {booking.NumberOfGuests}</li>
                        <li><strong>💰 Số tiền:</strong> {booking.TotalPrice:N0} VNĐ</li>
                    </ul>
                </div>
                
                <h3>❗ Lý do hủy tour:</h3>
                <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 10px 0;'>
                    <p style='font-style: italic; margin: 0;'>{reason}</p>
                </div>
                
                <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #155724;'>💵 HOÀN TIỀN TỰ ĐỘNG</h3>
                    <p style='font-size: 16px; margin-bottom: 10px;'>
                        <strong>Số tiền {booking.TotalPrice:N0} VNĐ sẽ được hoàn trả đầy đủ</strong>
                    </p>
                    <ul style='margin-bottom: 0;'>
                        <li>• <strong>Thời gian:</strong> 3-5 ngày làm việc</li>
                        <li>• <strong>Phương thức:</strong> Hoàn về tài khoản thanh toán gốc</li>
                        <li>• <strong>Xác nhận:</strong> Bạn sẽ nhận email xác nhận khi tiền được hoàn</li>
                        <li>• <strong>Hỗ trợ:</strong> Nhân viên sẽ liên hệ để hỗ trợ thủ tục hoàn tiền</li>
                    </ul>
                </div>
                
                <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #004085;'>✨ Gợi ý cho bạn:</h4>
                    <ul style='margin-bottom: 0;'>
                        <li><strong>Khám phá tour khác:</strong> Xem danh sách tour tương tự trên website</li>
                        <li><strong>Đặt lại sau:</strong> Tour có thể được mở lại với lịch trình mới</li>
                        <li><strong>Nhận ưu đãi:</strong> Theo dõi để nhận thông báo khuyến mãi đặc biệt</li>
                        <li><strong>Voucher bù đắp:</strong> Chúng tôi sẽ gửi voucher giảm giá cho lần đặt tour tiếp theo</li>
                    </ul>
                </div>
                
                <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #721c24;'>🙏 Lời xin lỗi chân thành</h4>
                    <p style='margin-bottom: 0;'>
                        Chúng tôi thành thật xin lỗi vì sự bất tiện này. Đây là quyết định khó khăn nhưng cần thiết để đảm bảo chất lượng dịch vụ cho quý khách. 
                        <strong>Nhân viên của chúng tôi sẽ liên hệ trực tiếp để hỗ trợ quá trình hoàn tiền trong thời gian sớm nhất.</strong>
                    </p>
                </div>
                
                <p><strong>❓ Cần hỗ trợ khẩn cấp?</strong> Liên hệ hotline: <a href='tel:1900-xxx-xxx'>1900-xxx-xxx</a> hoặc email: support@tayninhour.com</p>
                
                <br/>
                <p>Cảm ơn sự thông cảm của quý khách,</p>
                <p><strong>Đội ngũ Tay Ninh Tour</strong></p>";

                    await emailSender.SendEmailAsync(customerEmail, customerName, subject, htmlBody);
                    successCount++;

                    _logger.LogInformation("Cancellation email sent successfully to {CustomerEmail} for booking {BookingCode}",
                        customerEmail, booking.BookingCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send cancellation email to booking {BookingCode}", booking.BookingCode);
                }
            }

            return successCount;
        }


        /// <summary>
        /// Validate email address format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Skip time to 2 days before tour to trigger reminder emails (TESTING ONLY)
        /// This will simulate time passing so reminder emails are sent
        /// </summary>
        /// <param name="tourSlotId">ID c?a tour slot c?n test reminder</param>
        /// <returns>Information about the reminder email test</returns>
        [HttpPost("skip-to-reminder/{tourSlotId}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 400)]
        [ProducesResponseType(typeof(BaseResposeDto), 404)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SkipToReminderTime([FromRoute] Guid tourSlotId)
        {
            try
            {
                _logger.LogInformation("⏰ TESTING: Skipping to reminder time (2 days before) for slot {SlotId}", tourSlotId);

                // Get tour slot with full details
                var tourSlot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(ts => ts.Bookings.Where(b => !b.IsDeleted && b.Status == BookingStatus.Confirmed))
                        .ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId && !ts.IsDeleted);

                if (tourSlot == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour slot"
                    });
                }

                if (tourSlot.TourDetails?.TourOperation == null)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot chưa có tour operation"
                    });
                }

                var tourDate = tourSlot.TourDate.ToDateTime(TimeOnly.MinValue);
                var twoDaysBeforeTour = tourDate.AddDays(-2);
                var currentTime = DateTime.UtcNow;

                if (twoDaysBeforeTour <= currentTime)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Tour đã qua thời điểm nhắc nhở (2 ngày trước). Ngày tour: {tourDate:dd/MM/yyyy}, Thời điểm nhắc: {twoDaysBeforeTour:dd/MM/yyyy}"
                    });
                }

                var confirmedBookings = tourSlot.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed && !b.IsDeleted)
                    .ToList();

                if (!confirmedBookings.Any())
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot không có booking confirmed nào để gửi reminder"
                    });
                }

                // Send reminder emails manually (simulating the background service)
                var emailSender = HttpContext.RequestServices.GetRequiredService<EmailSender>();
                var emailsCount = await SendReminderEmailsToCustomersAsync(
                    emailSender,
                    confirmedBookings,
                    tourSlot.TourDetails.Title,
                    tourSlot.TourDate);

                var result = new
                {
                    success = true,
                    message = "✅ Test reminder emails thành công!",
                    tourSlotInfo = new
                    {
                        slotId = tourSlot.Id,
                        tourTitle = tourSlot.TourDetails.Title,
                        tourDate = tourSlot.TourDate,
                        status = tourSlot.Status.ToString(),
                        maxGuests = tourSlot.TourDetails.TourOperation.MaxGuests
                    },
                    timeInfo = new
                    {
                        currentTime = currentTime,
                        tourDate = tourDate,
                        reminderTime = twoDaysBeforeTour,
                        daysUntilTour = (tourDate.Date - currentTime.Date).Days,
                        daysUntilReminder = (twoDaysBeforeTour.Date - currentTime.Date).Days
                    },
                    reminderResults = new
                    {
                        totalBookingsWithReminders = confirmedBookings.Count,
                        totalEmailsSent = emailsCount,
                        emailSuccessRate = confirmedBookings.Count > 0 ? (double)emailsCount / confirmedBookings.Count * 100 : 0
                    },
                    affectedBookings = confirmedBookings.Select(b => new
                    {
                        bookingId = b.Id,
                        bookingCode = b.BookingCode,
                        customerName = !string.IsNullOrEmpty(b.ContactName) ? b.ContactName : b.User?.Name ?? "Unknown",
                        customerEmail = !string.IsNullOrEmpty(b.ContactEmail) ? b.ContactEmail : b.User?.Email ?? "",
                        numberOfGuests = b.NumberOfGuests,
                        totalPrice = b.TotalPrice
                    }).ToList(),
                    nextSteps = new[]
                    {
                "📧 Khách hàng đã nhận email nhắc nhở chuẩn bị tour",
                "📋 Email chứa danh sách đồ cần chuẩn bị chi tiết",
                "📞 Khách hàng có thể liên hệ hotline nếu cần hỗ trợ",
                "🗓️ Tour sẽ diễn ra trong 2 ngày nữa"
            }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reminder email test for slot {SlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi test reminder emails: {ex.Message}"
                });
            }
        }


        /// <summary>
        /// Send reminder emails to customers for their upcoming tour (Testing method)
        /// </summary>
        private async Task<int> SendReminderEmailsToCustomersAsync(
     EmailSender emailSender,
     List<DataAccessLayer.Entities.TourBooking> bookings,
     string tourTitle,
     DateOnly tourDate)
        {
            int successCount = 0;

            foreach (var booking in bookings)
            {
                try
                {
                    // Determine customer info - prioritize ContactEmail from booking
                    var customerName = !string.IsNullOrEmpty(booking.ContactName)
                        ? booking.ContactName
                        : booking.User?.Name ?? "Khách hàng";

                    var customerEmail = !string.IsNullOrEmpty(booking.ContactEmail)
                        ? booking.ContactEmail
                        : booking.User?.Email ?? "";

                    // Validate email
                    if (string.IsNullOrEmpty(customerEmail) || !IsValidEmail(customerEmail))
                    {
                        _logger.LogWarning("Invalid email for booking {BookingCode}: {Email}", booking.BookingCode, customerEmail);
                        continue;
                    }

                    var subject = $"📢 Nhắc nhở tour: {tourTitle} - Chuẩn bị cho chuyến đi!";
                    var htmlBody = $@"
                <h2>Kính chào {customerName},</h2>
                
                <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 15px 0;'>
                    <h3 style='margin-top: 0; color: #155724;'>📢 NHẮC NHỞ TOUR SẮP DIỄN RA</h3>
                    <p style='font-size: 16px; margin-bottom: 0;'>
                        Tour <strong>'{tourTitle}'</strong> của bạn sẽ diễn ra vào <strong>{tourDate:dd/MM/yyyy}</strong> (còn 2 ngày nữa)!
                    </p>
                </div>
                
                <h3>📝 Thông tin booking của bạn:</h3>
                <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 10px 0;'>
                    <ul style='margin: 0; list-style: none; padding: 0;'>
                        <li><strong>🔖 Mã booking:</strong> {booking.BookingCode}</li>
                        <li><strong>👥 Số lượng khách:</strong> {booking.NumberOfGuests}</li>
                        <li><strong>📅 Ngày tour:</strong> {tourDate:dd/MM/yyyy}</li>
                        <li><strong>💰 Tổng tiền:</strong> {booking.TotalPrice:N0} VNĐ</li>
                    </ul>
                </div>
                
                <div style='background-color: #fff3cd; padding: 20px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #856404;'>📋 DANH SÁCH CHUẨN BỊ</h3>
                    <h4>🪪 Giấy tờ cần thiết:</h4>
                    <ul>
                        <li>• <strong>CMND/CCCD hoặc Passport</strong> (bắt buộc)</li>
                        <li>• <strong>Vé xác nhận</strong> (in ra hoặc lưu trên điện thoại)</li>
                        <li>• <strong>Thẻ BHYT</strong> (nếu có)</li>
                    </ul>
                    
                    <h4>🎒 Đồ dùng cá nhân:</h4>
                    <ul>
                        <li>• Quần áo thoải mái, phù hợp thời tiết</li>
                        <li>• Giày thể thao chống trượt</li>
                        <li>• Mũ/nón chống nắng</li>
                        <li>• Kính râm</li>
                        <li>• Kem chống nắng</li>
                        <li>• Thuốc cá nhân (nếu có)</li>
                    </ul>
                    
                    <h4>📦 Khác:</h4>
                    <ul>
                        <li>• Pin dự phòng cho điện thoại</li>
                        <li>• Tiền mặt cho chi phí cá nhân</li>
                        <li>• Máy ảnh (tùy chọn)</li>
                        <li>• Đồ ăn vặt (tùy thích)</li>
                    </ul>
                </div>
                
                <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #004085;'>⚠️ Lưu ý quan trọng:</h4>
                    <ul style='margin-bottom: 0;'>
                        <li><strong>Thời gian tập trung:</strong> Vui lòng có mặt đúng giờ theo thông báo</li>
                        <li><strong>Thời tiết:</strong> Kiểm tra dự báo thời tiết và chuẩn bị phù hợp</li>
                        <li><strong>Liên hệ khẩn cấp:</strong> Lưu số hotline để liên hệ khi cần thiết</li>
                        <li><strong>Hủy tour:</strong> Nếu có thay đổi, vui lòng thông báo sớm</li>
                    </ul>
                </div>
                
                <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #0c5460;'>🌟 Mẹo để có chuyến đi tuyệt vời:</h4>
                    <ul style='margin-bottom: 0;'>
                        <li>• <strong>Nghỉ ngơi đầy đủ</strong> trước ngày tour</li>
                        <li>• <strong>Ăn sáng đầy đủ</strong> trước khi khởi hành</li>
                        <li>• <strong>Mang theo nước uống</strong> để giữ ẩm</li>
                        <li>• <strong>Sạc đầy pin</strong> điện thoại</li>
                        <li>• <strong>Làm quen</strong> với các thành viên khác trong tour</li>
                    </ul>
                </div>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px; margin-bottom: 10px;'>
                        <h4 style='margin: 0; font-size: 18px;'>📞 HOTLINE HỖ TRỢ 24/7</h4>
                        <p style='margin: 5px 0; font-size: 20px; font-weight: bold;'>1900-xxx-xxx</p>
                    </div>
                </div>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0; text-align: center;'>
                    <p style='margin: 0; font-style: italic; color: #6c757d;'>
                        Chúng tôi rất mong được đồng hành cùng bạn trong chuyến đi tuyệt vời này! 😊
                    </p>
                </div>
                
                <br/>
                <p>Chúc bạn có một chuyến đi an toàn và đầy ý nghĩa!</p>
                <p><strong>Đội ngũ Tay Ninh Tour</strong></p>";

                    await emailSender.SendEmailAsync(customerEmail, customerName, subject, htmlBody);
                    successCount++;

                    _logger.LogInformation("Tour reminder email sent successfully to {CustomerEmail} for booking {BookingCode}",
                        customerEmail, booking.BookingCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send tour reminder email to booking {BookingCode}", booking.BookingCode);
                }
            }

            return successCount;
        }

    }
}