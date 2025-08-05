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
                        Message = "Kh�ng t�m th?y tour slot"
                    });
                }

                if (tourSlot.TourDetails?.TourOperation == null)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot ch?a c� tour operation"
                    });
                }

                var tourStartDate = tourSlot.TourDate.ToDateTime(TimeOnly.MinValue);
                var currentTime = DateTime.UtcNow;

                if (tourStartDate <= currentTime)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Tour ?� b?t ??u ho?c ?� qua. Tour date: {tourStartDate:dd/MM/yyyy}, Hi?n t?i: {currentTime:dd/MM/yyyy}"
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
                            pendingBooking.CancellationReason = "Tour b?t ??u, booking ch?a thanh to�n b? t? ??ng h?y";
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
                    message = "? Time skip th�nh c�ng! Tour gi? ?� ? tr?ng th�i '?ang th?c hi?n'",
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
                            description = "HDV gi? c� th? qu�t QR code c?a kh�ch ?? check-in",
                            endpoint = "/api/TourGuide/scan-qr",
                            bookingsWithQR = confirmedBookings.Where(b => !string.IsNullOrEmpty(b.QRCodeData)).Count()
                        },
                        tourProgressUpdate = new
                        {
                            available = true,
                            description = "C� th? c?p nh?t ti?n ?? tour v� ho�n th�nh tour",
                            completeEndpoint = "/api/Testing/complete-tour/" + tourSlotId
                        },
                        autoCancelCheck = new
                        {
                            wasEligible = totalConfirmedGuests < 2, // Assuming minimum 2 guests
                            description = totalConfirmedGuests < 2 
                                ? "Tour n�y c� th? ?� b? auto-cancel do kh�ng ?? kh�ch (< 50% capacity)"
                                : "Tour c� ?? kh�ch ?? ti?n h�nh"
                        }
                    },
                    nextSteps = new[]
                    {
                        "?? HDV c� th? scan QR code c?a kh�ch ?? check-in",
                        "?? C� th? c?p nh?t ti?n ?? tour",
                        "?? Sau khi ho�n th�nh tour, revenue s? ???c transfer",
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
        /// <param name="tourSlotId">ID c?a tour slot c?n ho�n th�nh</param>
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
                        Message = "Kh�ng t�m th?y tour slot"
                    });
                }

                if (tourSlot.Status != TourSlotStatus.InProgress)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Tour ph?i ? tr?ng th�i '?ang th?c hi?n' ?? c� th? ho�n th�nh. Tr?ng th�i hi?n t?i: {tourSlot.Status}"
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
                    message = "?? Tour ?� ho�n th�nh th�nh c�ng!",
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
                        revenueTransferEligibleIn = "3 ng�y",
                        description = "Revenue s? ???c t? ??ng transfer cho tour company sau 3 ng�y"
                    },
                    testingFeatures = new
                    {
                        revenueTransfer = new
                        {
                            available = true,
                            description = "Background service s? t? ??ng transfer revenue sau 3 ng�y, ho?c c� th? test manually",
                            manualTestNote = "C� th? t?o API test ?? trigger revenue transfer ngay l?p t?c"
                        }
                    },
                    nextSteps = new[]
                    {
                        "?? Revenue ?ang ???c hold, s? transfer sau 3 ng�y",
                        "?? Tour company s? nh?n th�ng b�o khi revenue ???c transfer",
                        "?? C� th? check revenue status trong dashboard"
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
                    Message = $"L?i khi ho�n th�nh tour: {ex.Message}"
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
                        Message = "Kh�ng t�m th?y tour slot"
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
                    Message = $"L?i khi l?y th�ng tin tour: {ex.Message}"
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
                        Message = "Kh�ng t�m th?y tour slot"
                    });
                }

                if (tourSlot.TourDetails?.TourOperation == null)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot ch?a c� tour operation"
                    });
                }

                if (tourSlot.TourDetails.Status != TourDetailsStatus.Public)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Ch? c� th? auto-cancel tour ? tr?ng th�i Public"
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
                        Message = $"Tour slot c� ?? kh�ch ({guestBookingRate:P} >= 50% capacity). Kh�ng th? auto-cancel."
                    });
                }

                if (!confirmedBookings.Any())
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot kh�ng c� booking confirmed n�o ?? cancel"
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
                            booking.CancellationReason = $"Tour b? h?y t? ??ng do kh�ng ?? kh�ch ({guestBookingRate:P} < 50% capacity) - MANUAL TRIGGER";
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
                    $"Tour b? h?y t? ??ng do kh�ng ?? kh�ch ({guestBookingRate:P} < 50% capacity)"
                );

                var result = new
                {
                    success = true,
                    message = "?? Auto-cancel th�nh c�ng cho tour slot c? th?!",
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
                        reason = $"Kh�ng ?? kh�ch ({guestBookingRate:P} < 50% capacity)"
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
                        "?? Kh�ch h�ng ?� nh?n email th�ng b�o h?y tour",
                        "?? Ti?n s? ???c ho�n tr? t? ??ng trong 3-5 ng�y",
                        "?? Tour slot ?� chuy?n sang tr?ng th�i Cancelled",
                        "?? Capacity ?� ???c gi?i ph�ng kh?i tour operation"
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
                        Message = "Kh�ng t�m th?y tour slot"
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
                        ? "? Tour slot n�y c� th? ???c auto-cancel v� c� < 50% capacity" 
                        : "? Tour slot n�y kh�ng th? auto-cancel"
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
                        : booking.User?.Name ?? "Kh�ch h�ng";
                    
                    var customerEmail = !string.IsNullOrEmpty(booking.ContactEmail) 
                        ? booking.ContactEmail 
                        : booking.User?.Email ?? "";

                    // Validate email
                    if (string.IsNullOrEmpty(customerEmail) || !IsValidEmail(customerEmail))
                    {
                        _logger.LogWarning("Invalid email for booking {BookingCode}: {Email}", booking.BookingCode, customerEmail);
                        continue;
                    }

                    var subject = $"?? Th�ng b�o h?y tour: {tourTitle}";
                    var htmlBody = $@"
                        <h2>K�nh ch�o {customerName},</h2>
                        
                        <div style='background-color: #f8d7da; padding: 20px; border-left: 4px solid #dc3545; margin: 15px 0;'>
                            <h3 style='margin-top: 0; color: #721c24;'>?? TH�NG B�O H?Y TOUR</h3>
                            <p style='font-size: 16px; margin-bottom: 0;'>
                                Ch�ng t�i r?t ti?c ph?i th�ng b�o r?ng tour <strong>'{tourTitle}'</strong> ?� b? h?y.
                            </p>
                        </div>
                        
                        <h3>?? Th�ng tin booking c?a b?n:</h3>
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #6c757d; margin: 10px 0;'>
                            <ul style='margin: 0; list-style: none; padding: 0;'>
                                <li><strong>?? M� booking:</strong> {booking.BookingCode}</li>
                                <li><strong>?? S? l??ng kh�ch:</strong> {booking.NumberOfGuests}</li>
                                <li><strong>?? S? ti?n:</strong> {booking.TotalPrice:N0} VN?</li>
                            </ul>
                        </div>
                        
                        <h3>?? L� do h?y tour:</h3>
                        <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 10px 0;'>
                            <p style='font-style: italic; margin: 0;'>{reason}</p>
                        </div>
                        
                        <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0;'>
                            <h3 style='margin-top: 0; color: #155724;'>?? HO�N TI?N T? ??NG</h3>
                            <p style='font-size: 16px; margin-bottom: 10px;'>
                                <strong>S? ti?n {booking.TotalPrice:N0} VN? s? ???c ho�n tr? ??y ??</strong>
                            </p>
                            <ul style='margin-bottom: 0;'>
                                <li>? <strong>Th?i gian:</strong> 3-5 ng�y l�m vi?c</li>
                                <li>?? <strong>Ph??ng th?c:</strong> Ho�n v? t�i kho?n thanh to�n g?c</li>
                                <li>?? <strong>X�c nh?n:</strong> B?n s? nh?n email x�c nh?n khi ti?n ???c ho�n</li>
                                <li>?? <strong>H? tr?:</strong> Nh�n vi�n s? li�n h? ?? h? tr? th? t?c ho�n ti?n</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #004085;'>?? G?i � cho b?n:</h4>
                            <ul style='margin-bottom: 0;'>
                                <li><strong>Kh�m ph� tour kh�c:</strong> Xem danh s�ch tour t??ng t? tr�n website</li>
                                <li><strong>??t l?i sau:</strong> Tour c� th? ???c m? l?i v?i l?ch tr�nh m?i</li>
                                <li><strong>Nh�n ?u ?�i:</strong> Theo d�i ?? nh?n th�ng b�o khuy?n m�i ??c bi?t</li>
                                <li><strong>Voucher b� ??p:</strong> Ch�ng t�i s? g?i voucher gi?m gi� cho l?n ??t tour ti?p theo</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #721c24;'>?? L?i xin l?i ch�n th�nh</h4>
                            <p style='margin-bottom: 0;'>
                                Ch�ng t�i th�nh th?t xin l?i v� s? b?t ti?n n�y. ?�y l� quy?t ??nh kh� kh?n nh?ng c?n thi?t ?? ??m b?o ch?t l??ng d?ch v? cho qu� kh�ch. 
                                <strong>Nh�n vi�n c?a ch�ng t�i s? li�n h? tr?c ti?p ?? h? tr? qu� tr�nh ho�n ti?n trong th?i gian s?m nh?t.</strong>
                            </p>
                        </div>
                        
                        <p><strong>?? C?n h? tr? kh?n c?p?</strong> Li�n h? hotline: <a href='tel:1900-xxx-xxx'>1900-xxx-xxx</a> ho?c email: support@tayninhour.com</p>
                        
                        <br/>
                        <p>C?m ?n s? th�ng c?m c?a qu� kh�ch,</p>
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
                        Message = "Kh�ng t�m th?y tour slot"
                    });
                }

                if (tourSlot.TourDetails?.TourOperation == null)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour slot ch?a c� tour operation"
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
                        Message = $"Tour ?� qua th?i ?i?m nh?c nh? (2 ng�y tr??c). Tour date: {tourDate:dd/MM/yyyy}, Reminder time: {twoDaysBeforeTour:dd/MM/yyyy}"
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
                        Message = "Tour slot kh�ng c� booking confirmed n�o ?? g?i reminder"
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
                    message = "?? Test reminder emails th�nh c�ng!",
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
                        "?? Kh�ch h�ng ?� nh?n email nh?c nh? chu?n b? tour",
                        "?? Email ch?a danh s�ch ?? c?n chu?n b? chi ti?t",
                        "?? Kh�ch h�ng c� th? li�n h? hotline n?u c?n h? tr?",
                        "? Tour s? di?n ra trong 2 ng�y n?a"
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
                        : booking.User?.Name ?? "Kh�ch h�ng";
                    
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
                        <h2>K�nh ch�o {customerName},</h2>
                        
                        <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 15px 0;'>
                            <h3 style='margin-top: 0; color: #155724;'>?? NH?C NH? TOUR S?P DI?N RA</h3>
                            <p style='font-size: 16px; margin-bottom: 0;'>
                                Tour <strong>'{tourTitle}'</strong> c?a b?n s? di?n ra v�o <strong>{tourDate:dd/MM/yyyy}</strong> (c�n 2 ng�y n?a)!
                            </p>
                        </div>
                        
                        <h3>?? Th�ng tin booking c?a b?n:</h3>
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 10px 0;'>
                            <ul style='margin: 0; list-style: none; padding: 0;'>
                                <li><strong>?? M� booking:</strong> {booking.BookingCode}</li>
                                <li><strong>?? S? l??ng kh�ch:</strong> {booking.NumberOfGuests}</li>
                                <li><strong>?? Ng�y tour:</strong> {tourDate:dd/MM/yyyy}</li>
                                <li><strong>?? T?ng ti?n:</strong> {booking.TotalPrice:N0} VN?</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 20px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                            <h3 style='margin-top: 0; color: #856404;'>?? DANH S�CH CHU?N B?</h3>
                            <h4>?? Gi?y t? c?n thi?t:</h4>
                            <ul>
                                <li>? <strong>CMND/CCCD ho?c Passport</strong> (b?t bu?c)</li>
                                <li>? <strong>V� x�c nh?n</strong> (in ra ho?c l?u tr�n ?i?n tho?i)</li>
                                <li>? <strong>Th? BHYT</strong> (n?u c�)</li>
                            </ul>
                            
                            <h4>?? ?? d�ng c� nh�n:</h4>
                            <ul>
                                <li>?? Qu?n �o tho?i m�i, ph� h?p th?i ti?t</li>
                                <li>?? Gi�y th? thao ch?ng tr??t</li>
                                <li>?? M?/n�n ch?ng n?ng</li>
                                <li>??? K�nh r�m</li>
                                <li>?? Kem ch?ng n?ng</li>
                                <li>?? Thu?c c� nh�n (n?u c�)</li>
                            </ul>
                            
                            <h4>?? Kh�c:</h4>
                            <ul>
                                <li>?? Pin d? ph�ng cho ?i?n tho?i</li>
                                <li>?? Ti?n m?t cho chi ph� c� nh�n</li>
                                <li>?? M�y ?nh (t�y ch?n)</li>
                                <li>?? ?? ?n v?t (t�y th�ch)</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #004085;'>? L?u � quan tr?ng:</h4>
                            <ul style='margin-bottom: 0;'>
                                <li><strong>Th?i gian t?p trung:</strong> Vui l�ng c� m?t ?�ng gi? theo th�ng b�o</li>
                                <li><strong>Th?i ti?t:</strong> Ki?m tra d? b�o th?i ti?t v� chu?n b? ph� h?p</li>
                                <li><strong>Li�n h? kh?n c?p:</strong> L?u s? hotline ?? li�n h? khi c?n thi?t</li>
                                <li><strong>H?y tour:</strong> N?u c� thay ??i, vui l�ng th�ng b�o s?m</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #0c5460;'>?? M?o ?? c� chuy?n ?i tuy?t v?i:</h4>
                            <ul style='margin-bottom: 0;'>
                                <li>?? <strong>Ngh? ng?i ??y ??</strong> tr??c ng�y tour</li>
                                <li>??? <strong>?n s�ng ??y ??</strong> tr??c khi kh?i h�nh</li>
                                <li>?? <strong>Mang theo n??c u?ng</strong> ?? gi? ?m</li>
                                <li>?? <strong>S?c ??y pin</strong> ?i?n tho?i</li>
                                <li>?? <strong>L�m quen</strong> v?i c�c th�nh vi�n kh�c trong tour</li>
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
                                Ch�ng t�i r?t mong ???c ??ng h�nh c�ng b?n trong chuy?n ?i tuy?t v?i n�y! ??
                            </p>
                        </div>
                        
                        <br/>
                        <p>Ch�c b?n c� m?t chuy?n ?i an to�n v� ??y � ngh?a!</p>
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