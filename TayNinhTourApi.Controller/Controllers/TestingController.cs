using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.Utilities;

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
        private readonly ILogger<TestingController> _logger;

        public TestingController(
            IUnitOfWork unitOfWork,
            ILogger<TestingController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                _logger.LogInformation("? TESTING: Skipping time to tour start for slot {SlotId}", tourSlotId);

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
                        Message = "Không tìm th?y tour slot"
                    });
                }

                if (tourSlot.TourDetails?.TourOperation == null)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot ch?a có tour operation"
                    });
                }

                var tourStartDate = tourSlot.TourDate.ToDateTime(TimeOnly.MinValue);
                var currentTime = DateTime.UtcNow;

                if (tourStartDate <= currentTime)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Tour ?ã b?t ??u ho?c ?ã qua. Tour date: {tourStartDate:dd/MM/yyyy}, Hi?n t?i: {currentTime:dd/MM/yyyy}"
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
                            pendingBooking.CancellationReason = "Tour b?t ??u, booking ch?a thanh toán b? t? ??ng h?y";
                            pendingBooking.UpdatedAt = DateTime.UtcNow;
                            await _unitOfWork.TourBookingRepository.UpdateAsync(pendingBooking);
                        }

                        // Update tour slot to InProgress so testing features can be used
                        tourSlot.Status = TourSlotStatus.InProgress;
                        tourSlot.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.TourSlotRepository.UpdateAsync(tourSlot);

                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("? Time skip completed for slot {SlotId} - Tour is now InProgress", tourSlotId);
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
                    message = "? Time skip thành công! Tour gi? ?ã ? tr?ng thái '?ang th?c hi?n'",
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
                            description = "HDV gi? có th? quét QR code c?a khách ?? check-in",
                            endpoint = "/api/TourGuide/scan-qr",
                            bookingsWithQR = confirmedBookings.Where(b => !string.IsNullOrEmpty(b.QRCodeData)).Count()
                        },
                        tourProgressUpdate = new
                        {
                            available = true,
                            description = "Có th? c?p nh?t ti?n ?? tour và hoàn thành tour",
                            completeEndpoint = "/api/Testing/complete-tour/" + tourSlotId
                        },
                        autoCancelCheck = new
                        {
                            wasEligible = totalConfirmedGuests < 2, // Assuming minimum 2 guests
                            description = totalConfirmedGuests < 2 
                                ? "Tour này có th? ?ã b? auto-cancel do không ?? khách (< 50% capacity)"
                                : "Tour có ?? khách ?? ti?n hành"
                        }
                    },
                    nextSteps = new[]
                    {
                        "?? HDV có th? scan QR code c?a khách ?? check-in",
                        "?? Có th? c?p nh?t ti?n ?? tour",
                        "?? Sau khi hoàn thành tour, revenue s? ???c transfer",
                        $"?? D? ki?n revenue: {confirmedBookings.Sum(b => b.TotalPrice):N0} VN?"
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
                    Message = $"L?i khi th?c hi?n time skip: {ex.Message}"
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
                _logger.LogInformation("?? TESTING: Completing tour for slot {SlotId}", tourSlotId);

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
                        Message = "Không tìm th?y tour slot"
                    });
                }

                if (tourSlot.Status != TourSlotStatus.InProgress)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Tour ph?i ? tr?ng thái '?ang th?c hi?n' ?? có th? hoàn thành. Tr?ng thái hi?n t?i: {tourSlot.Status}"
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

                        _logger.LogInformation("?? Tour completed for slot {SlotId}", tourSlotId);
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
                    message = "?? Tour ?ã hoàn thành thành công!",
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
                        description = "Revenue s? ???c t? ??ng transfer cho tour company sau 3 ngày"
                    },
                    testingFeatures = new
                    {
                        revenueTransfer = new
                        {
                            available = true,
                            description = "Background service s? t? ??ng transfer revenue sau 3 ngày, ho?c có th? test manually",
                            manualTestNote = "Có th? t?o API test ?? trigger revenue transfer ngay l?p t?c"
                        }
                    },
                    nextSteps = new[]
                    {
                        "?? Revenue ?ang ???c hold, s? transfer sau 3 ngày",
                        "?? Tour company s? nh?n thông báo khi revenue ???c transfer",
                        "?? Có th? check revenue status trong dashboard"
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
                    Message = $"L?i khi hoàn thành tour: {ex.Message}"
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
                        Message = "Không tìm th?y tour slot"
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
                    Message = $"L?i khi l?y thông tin tour: {ex.Message}"
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
                _logger.LogInformation("?? TESTING: Manual trigger auto-cancel for specific slot {SlotId}", tourSlotId);

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
                        Message = "Không tìm th?y tour slot"
                    });
                }

                if (tourSlot.TourDetails?.TourOperation == null)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot ch?a có tour operation"
                    });
                }

                if (tourSlot.TourDetails.Status != TourDetailsStatus.Public)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Ch? có th? auto-cancel tour ? tr?ng thái Public"
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
                        Message = $"Tour slot có ?? khách ({guestBookingRate:P} >= 50% capacity). Không th? auto-cancel."
                    });
                }

                if (!confirmedBookings.Any())
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot không có booking confirmed nào ?? cancel"
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
                            booking.CancellationReason = $"Tour b? h?y t? ??ng do không ?? khách ({guestBookingRate:P} < 50% capacity) - MANUAL TRIGGER";
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
                    $"Tour b? h?y t? ??ng do không ?? khách ({guestBookingRate:P} < 50% capacity)"
                );

                var result = new
                {
                    success = true,
                    message = "?? Auto-cancel thành công cho tour slot c? th?!",
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
                        reason = $"Không ?? khách ({guestBookingRate:P} < 50% capacity)"
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
                        "?? Khách hàng ?ã nh?n email thông báo h?y tour",
                        "?? Ti?n s? ???c hoàn tr? t? ??ng trong 3-5 ngày",
                        "?? Tour slot ?ã chuy?n sang tr?ng thái Cancelled",
                        "?? Capacity ?ã ???c gi?i phóng kh?i tour operation"
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
                    Message = $"L?i khi th?c hi?n auto-cancel: {ex.Message}"
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
                        Message = "Không tìm th?y tour slot"
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
                        ? "? Tour slot này có th? ???c auto-cancel vì có < 50% capacity" 
                        : "? Tour slot này không th? auto-cancel"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking auto-cancel eligibility for slot {SlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"L?i khi ki?m tra ?i?u ki?n auto-cancel: {ex.Message}"
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

                    var subject = $"?? Thông báo h?y tour: {tourTitle}";
                    var htmlBody = $@"
                        <h2>Kính chào {customerName},</h2>
                        
                        <div style='background-color: #f8d7da; padding: 20px; border-left: 4px solid #dc3545; margin: 15px 0;'>
                            <h3 style='margin-top: 0; color: #721c24;'>?? THÔNG BÁO H?Y TOUR</h3>
                            <p style='font-size: 16px; margin-bottom: 0;'>
                                Chúng tôi r?t ti?c ph?i thông báo r?ng tour <strong>'{tourTitle}'</strong> ?ã b? h?y.
                            </p>
                        </div>
                        
                        <h3>?? Thông tin booking c?a b?n:</h3>
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #6c757d; margin: 10px 0;'>
                            <ul style='margin: 0; list-style: none; padding: 0;'>
                                <li><strong>?? Mã booking:</strong> {booking.BookingCode}</li>
                                <li><strong>?? S? l??ng khách:</strong> {booking.NumberOfGuests}</li>
                                <li><strong>?? S? ti?n:</strong> {booking.TotalPrice:N0} VN?</li>
                            </ul>
                        </div>
                        
                        <h3>?? Lý do h?y tour:</h3>
                        <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 10px 0;'>
                            <p style='font-style: italic; margin: 0;'>{reason}</p>
                        </div>
                        
                        <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0;'>
                            <h3 style='margin-top: 0; color: #155724;'>?? HOÀN TI?N T? ??NG</h3>
                            <p style='font-size: 16px; margin-bottom: 10px;'>
                                <strong>S? ti?n {booking.TotalPrice:N0} VN? s? ???c hoàn tr? ??y ??</strong>
                            </p>
                            <ul style='margin-bottom: 0;'>
                                <li>? <strong>Th?i gian:</strong> 3-5 ngày làm vi?c</li>
                                <li>?? <strong>Ph??ng th?c:</strong> Hoàn v? tài kho?n thanh toán g?c</li>
                                <li>?? <strong>Xác nh?n:</strong> B?n s? nh?n email xác nh?n khi ti?n ???c hoàn</li>
                                <li>?? <strong>H? tr?:</strong> Nhân viên s? liên h? ?? h? tr? th? t?c hoàn ti?n</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #004085;'>?? G?i ý cho b?n:</h4>
                            <ul style='margin-bottom: 0;'>
                                <li><strong>Khám phá tour khác:</strong> Xem danh sách tour t??ng t? trên website</li>
                                <li><strong>??t l?i sau:</strong> Tour có th? ???c m? l?i v?i l?ch trình m?i</li>
                                <li><strong>Nhân ?u ?ãi:</strong> Theo dõi ?? nh?n thông báo khuy?n mãi ??c bi?t</li>
                                <li><strong>Voucher bù ??p:</strong> Chúng tôi s? g?i voucher gi?m giá cho l?n ??t tour ti?p theo</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #721c24;'>?? L?i xin l?i chân thành</h4>
                            <p style='margin-bottom: 0;'>
                                Chúng tôi thành th?t xin l?i vì s? b?t ti?n này. ?ây là quy?t ??nh khó kh?n nh?ng c?n thi?t ?? ??m b?o ch?t l??ng d?ch v? cho quý khách. 
                                <strong>Nhân viên c?a chúng tôi s? liên h? tr?c ti?p ?? h? tr? quá trình hoàn ti?n trong th?i gian s?m nh?t.</strong>
                            </p>
                        </div>
                        
                        <p><strong>?? C?n h? tr? kh?n c?p?</strong> Liên h? hotline: <a href='tel:1900-xxx-xxx'>1900-xxx-xxx</a> ho?c email: support@tayninhour.com</p>
                        
                        <br/>
                        <p>C?m ?n s? thông c?m c?a quý khách,</p>
                        <p><strong>??i ng? Tay Ninh Tour</strong></p>";

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
                _logger.LogInformation("?? TESTING: Skipping to reminder time (2 days before) for slot {SlotId}", tourSlotId);

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
                        Message = "Không tìm th?y tour slot"
                    });
                }

                if (tourSlot.TourDetails?.TourOperation == null)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot ch?a có tour operation"
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
                        Message = $"Tour ?ã qua th?i ?i?m nh?c nh? (2 ngày tr??c). Tour date: {tourDate:dd/MM/yyyy}, Reminder time: {twoDaysBeforeTour:dd/MM/yyyy}"
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
                        Message = "Tour slot không có booking confirmed nào ?? g?i reminder"
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
                    message = "?? Test reminder emails thành công!",
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
                        "?? Khách hàng ?ã nh?n email nh?c nh? chu?n b? tour",
                        "?? Email ch?a danh sách ?? c?n chu?n b? chi ti?t",
                        "?? Khách hàng có th? liên h? hotline n?u c?n h? tr?",
                        "? Tour s? di?n ra trong 2 ngày n?a"
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
                    Message = $"L?i khi test reminder emails: {ex.Message}"
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

                    var subject = $"?? Nh?c nh? tour: {tourTitle} - Chu?n b? cho chuy?n ?i!";
                    var htmlBody = $@"
                        <h2>Kính chào {customerName},</h2>
                        
                        <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 15px 0;'>
                            <h3 style='margin-top: 0; color: #155724;'>?? NH?C NH? TOUR S?P DI?N RA</h3>
                            <p style='font-size: 16px; margin-bottom: 0;'>
                                Tour <strong>'{tourTitle}'</strong> c?a b?n s? di?n ra vào <strong>{tourDate:dd/MM/yyyy}</strong> (còn 2 ngày n?a)!
                            </p>
                        </div>
                        
                        <h3>?? Thông tin booking c?a b?n:</h3>
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 10px 0;'>
                            <ul style='margin: 0; list-style: none; padding: 0;'>
                                <li><strong>?? Mã booking:</strong> {booking.BookingCode}</li>
                                <li><strong>?? S? l??ng khách:</strong> {booking.NumberOfGuests}</li>
                                <li><strong>?? Ngày tour:</strong> {tourDate:dd/MM/yyyy}</li>
                                <li><strong>?? T?ng ti?n:</strong> {booking.TotalPrice:N0} VN?</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 20px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                            <h3 style='margin-top: 0; color: #856404;'>?? DANH SÁCH CHU?N B?</h3>
                            <h4>?? Gi?y t? c?n thi?t:</h4>
                            <ul>
                                <li>? <strong>CMND/CCCD ho?c Passport</strong> (b?t bu?c)</li>
                                <li>? <strong>Vé xác nh?n</strong> (in ra ho?c l?u trên ?i?n tho?i)</li>
                                <li>? <strong>Th? BHYT</strong> (n?u có)</li>
                            </ul>
                            
                            <h4>?? ?? dùng cá nhân:</h4>
                            <ul>
                                <li>?? Qu?n áo tho?i mái, phù h?p th?i ti?t</li>
                                <li>?? Giày th? thao ch?ng tr??t</li>
                                <li>?? M?/nón ch?ng n?ng</li>
                                <li>??? Kính râm</li>
                                <li>?? Kem ch?ng n?ng</li>
                                <li>?? Thu?c cá nhân (n?u có)</li>
                            </ul>
                            
                            <h4>?? Khác:</h4>
                            <ul>
                                <li>?? Pin d? phòng cho ?i?n tho?i</li>
                                <li>?? Ti?n m?t cho chi phí cá nhân</li>
                                <li>?? Máy ?nh (tùy ch?n)</li>
                                <li>?? ?? ?n v?t (tùy thích)</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #004085;'>? L?u ý quan tr?ng:</h4>
                            <ul style='margin-bottom: 0;'>
                                <li><strong>Th?i gian t?p trung:</strong> Vui lòng có m?t ?úng gi? theo thông báo</li>
                                <li><strong>Th?i ti?t:</strong> Ki?m tra d? báo th?i ti?t và chu?n b? phù h?p</li>
                                <li><strong>Liên h? kh?n c?p:</strong> L?u s? hotline ?? liên h? khi c?n thi?t</li>
                                <li><strong>H?y tour:</strong> N?u có thay ??i, vui lòng thông báo s?m</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #0c5460;'>?? M?o ?? có chuy?n ?i tuy?t v?i:</h4>
                            <ul style='margin-bottom: 0;'>
                                <li>?? <strong>Ngh? ng?i ??y ??</strong> tr??c ngày tour</li>
                                <li>??? <strong>?n sáng ??y ??</strong> tr??c khi kh?i hành</li>
                                <li>?? <strong>Mang theo n??c u?ng</strong> ?? gi? ?m</li>
                                <li>?? <strong>S?c ??y pin</strong> ?i?n tho?i</li>
                                <li>?? <strong>Làm quen</strong> v?i các thành viên khác trong tour</li>
                            </ul>
                        </div>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px; margin-bottom: 10px;'>
                                <h4 style='margin: 0; font-size: 18px;'>?? HOTLINE H? TR? 24/7</h4>
                                <p style='margin: 5px 0; font-size: 20px; font-weight: bold;'>1900-xxx-xxx</p>
                            </div>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0; text-align: center;'>
                            <p style='margin: 0; font-style: italic; color: #6c757d;'>
                                Chúng tôi r?t mong ???c ??ng hành cùng b?n trong chuy?n ?i tuy?t v?i này! ??
                            </p>
                        </div>
                        
                        <br/>
                        <p>Chúc b?n có m?t chuy?n ?i an toàn và ??y ý ngh?a!</p>
                        <p><strong>??i ng? Tay Ninh Tour</strong></p>";

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