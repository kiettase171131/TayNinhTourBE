using AutoMapper;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Booking;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Booking;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho TourBooking management
    /// </summary>
    public class TourBookingService : ITourBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TourBookingService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public TourBookingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TourBookingService> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Tạo booking mới với capacity validation và concurrency control
        /// </summary>
        public async Task<ResponseCreateBookingDto> CreateBookingAsync(RequestCreateBookingDto request, CurrentUserObject currentUser)
        {
            try
            {
                _logger.LogInformation("Creating booking for TourOperation {TourOperationId} by User {UserId}", 
                    request.TourOperationId, currentUser.Id);

                // 1. Validate business rules
                var validation = await ValidateBookingAsync(request, currentUser);
                if (!validation.IsValid)
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = validation.ErrorMessage,
                        StatusCode = 400
                    };
                }

                // 2. Get TourOperation with lock để tránh race condition
                var tourOperation = await _unitOfWork.TourOperationRepository.GetByIdAsync(request.TourOperationId, 
                    new[] { "TourDetails", "TourDetails.TourTemplate", "Guide" });

                if (tourOperation == null)
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Tour operation không tồn tại",
                        StatusCode = 404
                    };
                }

                // 3. Check và reserve capacity với concurrency control
                using var transaction = _unitOfWork.BeginTransaction();
                try
                {
                    // Sử dụng optimistic concurrency control để reserve capacity
                    var reserveSuccess = await _unitOfWork.TourBookingRepository.TryReserveCapacityAsync(
                        request.TourOperationId, request.TotalGuests);

                    if (!reserveSuccess)
                    {
                        return new ResponseCreateBookingDto
                        {
                            success = false,
                            Message = "Tour đã hết chỗ hoặc có người khác đang đặt cùng lúc. Vui lòng thử lại.",
                            StatusCode = 409
                        };
                    }

                    // 4. Calculate price
                    var totalPrice = await CalculateBookingPriceAsync(
                        request.TourOperationId, request.AdultCount, request.ChildCount);

                    // 5. Generate unique booking code
                    var bookingCode = await GenerateBookingCodeAsync();

                    // 6. Create booking entity
                    var booking = new TourBooking
                    {
                        Id = Guid.NewGuid(),
                        TourOperationId = request.TourOperationId,
                        UserId = currentUser.Id,
                        AdultCount = request.AdultCount,
                        ChildCount = request.ChildCount,
                        NumberOfGuests = request.TotalGuests,
                        TotalPrice = totalPrice,
                        Status = BookingStatus.Pending,
                        BookingDate = DateTime.UtcNow,
                        BookingCode = bookingCode,
                        CustomerNotes = request.CustomerNotes,
                        ContactName = request.ContactName,
                        ContactPhone = request.ContactPhone,
                        ContactEmail = request.ContactEmail,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = currentUser.Id,
                        IsActive = true
                    };

                    // 7. Save booking
                    await _unitOfWork.TourBookingRepository.AddAsync(booking);
                    await _unitOfWork.SaveChangesAsync();

                    // 8. Commit transaction
                    await transaction.CommitAsync();

                    // 9. Get updated capacity info
                    var capacityInfo = await GetCapacityInfo(request.TourOperationId, tourOperation.MaxGuests);

                    // 10. Map to response
                    var bookingResponse = await MapToBookingResponse(booking);

                    _logger.LogInformation("Booking created successfully: {BookingCode}", bookingCode);

                    return new ResponseCreateBookingDto
                    {
                        success = true,
                        Message = "Đặt tour thành công",
                        StatusCode = 201,
                        BookingData = bookingResponse,
                        BookingCode = bookingCode,
                        CapacityInfo = capacityInfo
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for TourOperation {TourOperationId}", request.TourOperationId);
                return new ResponseCreateBookingDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi đặt tour. Vui lòng thử lại.",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Hủy booking
        /// </summary>
        public async Task<ResponseCreateBookingDto> CancelBookingAsync(Guid bookingId, RequestUpdateBookingStatusDto request, CurrentUserObject currentUser)
        {
            try
            {
                _logger.LogInformation("Cancelling booking {BookingId} by User {UserId}", bookingId, currentUser.Id);

                var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(bookingId, 
                    new[] { "TourOperation", "User" });

                if (booking == null)
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Booking không tồn tại",
                        StatusCode = 404
                    };
                }

                // Check permission
                if (booking.UserId != currentUser.Id && !await IsAdminOrTourCompanyAsync(currentUser))
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Bạn không có quyền hủy booking này",
                        StatusCode = 403
                    };
                }

                // Check if booking can be cancelled
                if (booking.Status == BookingStatus.CancelledByCustomer || booking.Status == BookingStatus.CancelledByCompany)
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Booking đã được hủy trước đó",
                        StatusCode = 400
                    };
                }

                // Update booking status và release capacity
                using var transaction = _unitOfWork.BeginTransaction();
                try
                {
                    var newStatus = booking.UserId == currentUser.Id ? BookingStatus.CancelledByCustomer : BookingStatus.CancelledByCompany;
                    var success = await _unitOfWork.TourBookingRepository.UpdateBookingStatusAsync(bookingId, newStatus, request.Reason);

                    if (!success)
                    {
                        return new ResponseCreateBookingDto
                        {
                            success = false,
                            Message = "Không thể hủy booking",
                            StatusCode = 500
                        };
                    }

                    // Release capacity cho tất cả active bookings (Pending hoặc Confirmed)
                    // vì capacity đã được reserve ngay khi tạo booking
                    if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
                    {
                        await _unitOfWork.TourBookingRepository.ReleaseCapacityAsync(
                            booking.TourOperationId, booking.NumberOfGuests);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                _logger.LogInformation("Booking {BookingId} cancelled successfully", bookingId);

                return new ResponseCreateBookingDto
                {
                    success = true,
                    Message = "Hủy booking thành công",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                return new ResponseCreateBookingDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi hủy booking",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Confirm booking (chỉ admin/tour company)
        /// </summary>
        public async Task<ResponseCreateBookingDto> ConfirmBookingAsync(Guid bookingId, CurrentUserObject currentUser)
        {
            try
            {
                _logger.LogInformation("Confirming booking {BookingId} by User {UserId}", bookingId, currentUser.Id);

                // Check permission
                if (!await IsAdminOrTourCompanyAsync(currentUser))
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Bạn không có quyền confirm booking",
                        StatusCode = 403
                    };
                }

                var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(bookingId,
                    new[] { "TourOperation", "User" });

                if (booking == null)
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Booking không tồn tại",
                        StatusCode = 404
                    };
                }

                // Check if booking can be confirmed
                if (booking.Status != BookingStatus.Pending)
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Chỉ có thể confirm booking đang ở trạng thái Pending",
                        StatusCode = 400
                    };
                }

                // Update booking status
                var success = await _unitOfWork.TourBookingRepository.UpdateBookingStatusAsync(
                    bookingId, BookingStatus.Confirmed);

                if (!success)
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Không thể confirm booking",
                        StatusCode = 500
                    };
                }

                _logger.LogInformation("Booking {BookingId} confirmed successfully", bookingId);

                return new ResponseCreateBookingDto
                {
                    success = true,
                    Message = "Confirm booking thành công",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
                return new ResponseCreateBookingDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi confirm booking",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Lấy danh sách bookings của user hiện tại
        /// </summary>
        public async Task<ResponseGetBookingsDto> GetMyBookingsAsync(CurrentUserObject currentUser, bool includeInactive = false)
        {
            try
            {
                var bookings = await _unitOfWork.TourBookingRepository.GetBookingsByUserIdAsync(currentUser.Id, includeInactive);
                var bookingResponses = new List<ResponseBookingDto>();

                foreach (var booking in bookings)
                {
                    var response = await MapToBookingResponse(booking);
                    bookingResponses.Add(response);
                }

                return new ResponseGetBookingsDto
                {
                    success = true,
                    Message = "Lấy danh sách booking thành công",
                    StatusCode = 200,
                    Bookings = bookingResponses,
                    Pagination = new PaginationInfoDto
                    {
                        CurrentPage = 0,
                        PageSize = bookingResponses.Count,
                        TotalItems = bookingResponses.Count,
                        TotalPages = 1,
                        HasPrevious = false,
                        HasNext = false
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user {UserId}", currentUser.Id);
                return new ResponseGetBookingsDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách booking",
                    StatusCode = 500
                };
            }
        }

        // Helper methods sẽ được implement trong phần tiếp theo...
        private async Task<CapacityInfoDto> GetCapacityInfo(Guid tourOperationId, int maxCapacity)
        {
            // Sử dụng CurrentBookings từ TourOperation thay vì tính toán từ TourBookings
            // để có real-time capacity với concurrency control
            var tourOperation = await _unitOfWork.TourOperationRepository.GetByIdAsync(tourOperationId);
            var currentBookings = tourOperation?.CurrentBookings ?? 0;
            var actualMaxCapacity = tourOperation?.MaxGuests ?? maxCapacity;

            var availableCapacity = Math.Max(0, actualMaxCapacity - currentBookings);
            var bookingPercentage = actualMaxCapacity > 0 ? (decimal)currentBookings / actualMaxCapacity * 100 : 0;

            return new CapacityInfoDto
            {
                MaxCapacity = actualMaxCapacity,
                BookedCapacity = currentBookings,
                AvailableCapacity = availableCapacity,
                BookingPercentage = Math.Round(bookingPercentage, 2)
            };
        }

        private async Task<bool> IsAdminOrTourCompanyAsync(CurrentUserObject user)
        {
            try
            {
                var userEntity = await _unitOfWork.UserRepository.GetByIdAsync(user.UserId, new[] { "Role" });
                if (userEntity?.Role == null) return false;

                return userEntity.Role.Name == "Admin" || userEntity.Role.Name == "Tour Company";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách bookings với filter
        /// </summary>
        public async Task<ResponseGetBookingsDto> GetBookingsAsync(RequestGetBookingsDto request, CurrentUserObject currentUser)
        {
            try
            {
                var bookings = await _unitOfWork.TourBookingRepository.GetBookingsWithPaginationAsync(
                    request.PageIndex, request.PageSize, request.TourOperationId, request.UserId, request.Status);

                var totalCount = await _unitOfWork.TourBookingRepository.CountBookingsAsync(
                    request.TourOperationId, request.UserId, request.Status);

                var bookingResponses = new List<ResponseBookingDto>();
                foreach (var booking in bookings)
                {
                    var response = await MapToBookingResponse(booking);
                    bookingResponses.Add(response);
                }

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                return new ResponseGetBookingsDto
                {
                    success = true,
                    Message = "Lấy danh sách booking thành công",
                    StatusCode = 200,
                    Bookings = bookingResponses,
                    Pagination = new PaginationInfoDto
                    {
                        CurrentPage = request.PageIndex,
                        PageSize = request.PageSize,
                        TotalItems = totalCount,
                        TotalPages = totalPages,
                        HasPrevious = request.PageIndex > 0,
                        HasNext = request.PageIndex < totalPages - 1
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings with filter");
                return new ResponseGetBookingsDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách booking",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Lấy chi tiết booking theo ID
        /// </summary>
        public async Task<ResponseBookingDto?> GetBookingByIdAsync(Guid bookingId, CurrentUserObject currentUser)
        {
            try
            {
                var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(bookingId,
                    new[] { "TourOperation", "TourOperation.TourDetails", "TourOperation.TourDetails.TourTemplate", "TourOperation.TourGuide", "User" });

                if (booking == null) return null;

                // Check permission
                if (booking.UserId != currentUser.Id && !await IsAdminOrTourCompanyAsync(currentUser))
                {
                    return null;
                }

                return await MapToBookingResponse(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", bookingId);
                return null;
            }
        }

        /// <summary>
        /// Lấy booking theo booking code
        /// </summary>
        public async Task<ResponseBookingDto?> GetBookingByCodeAsync(string bookingCode, CurrentUserObject? currentUser = null)
        {
            try
            {
                var booking = await _unitOfWork.TourBookingRepository.GetBookingByCodeAsync(bookingCode);
                if (booking == null) return null;

                return await MapToBookingResponse(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking by code {BookingCode}", bookingCode);
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra capacity
        /// </summary>
        public async Task<ResponseCapacityCheckDto> CheckCapacityAsync(Guid tourOperationId, int? requestedGuests = null)
        {
            try
            {
                var tourOperation = await _unitOfWork.TourOperationRepository.GetByIdAsync(tourOperationId);
                if (tourOperation == null)
                {
                    return new ResponseCapacityCheckDto
                    {
                        success = false,
                        Message = "Tour operation không tồn tại",
                        StatusCode = 404,
                        TourOperationId = tourOperationId
                    };
                }

                var capacityInfo = await GetCapacityInfo(tourOperationId, tourOperation.MaxGuests);
                var canBook = !requestedGuests.HasValue || capacityInfo.AvailableCapacity >= requestedGuests.Value;

                string userMessage;
                if (capacityInfo.IsFull)
                {
                    userMessage = "Tour đã hết chỗ";
                }
                else if (requestedGuests.HasValue && !canBook)
                {
                    userMessage = $"Chỉ còn {capacityInfo.AvailableCapacity} chỗ trống, không đủ cho {requestedGuests} khách";
                }
                else
                {
                    userMessage = $"Còn {capacityInfo.AvailableCapacity} chỗ trống";
                }

                return new ResponseCapacityCheckDto
                {
                    success = true,
                    Message = "Kiểm tra capacity thành công",
                    StatusCode = 200,
                    TourOperationId = tourOperationId,
                    CapacityInfo = capacityInfo,
                    CanBook = canBook,
                    MaxAdditionalGuests = capacityInfo.AvailableCapacity,
                    UserMessage = userMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking capacity for TourOperation {TourOperationId}", tourOperationId);
                return new ResponseCapacityCheckDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi kiểm tra capacity",
                    StatusCode = 500,
                    TourOperationId = tourOperationId
                };
            }
        }

        /// <summary>
        /// Cập nhật trạng thái booking
        /// </summary>
        public async Task<ResponseCreateBookingDto> UpdateBookingStatusAsync(Guid bookingId, RequestUpdateBookingStatusDto request, CurrentUserObject currentUser)
        {
            try
            {
                if (!await IsAdminOrTourCompanyAsync(currentUser))
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Bạn không có quyền cập nhật trạng thái booking",
                        StatusCode = 403
                    };
                }

                var success = await _unitOfWork.TourBookingRepository.UpdateBookingStatusAsync(bookingId, request.NewStatus, request.Reason);

                if (!success)
                {
                    return new ResponseCreateBookingDto
                    {
                        success = false,
                        Message = "Không thể cập nhật trạng thái booking",
                        StatusCode = 404
                    };
                }

                return new ResponseCreateBookingDto
                {
                    success = true,
                    Message = "Cập nhật trạng thái booking thành công",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status {BookingId}", bookingId);
                return new ResponseCreateBookingDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi cập nhật trạng thái",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Lấy thống kê bookings
        /// </summary>
        public async Task<BookingSummaryDto> GetBookingStatisticsAsync(Guid tourOperationId, CurrentUserObject currentUser)
        {
            try
            {
                var statistics = await _unitOfWork.TourBookingRepository.GetBookingStatisticsAsync(tourOperationId);
                var bookings = await _unitOfWork.TourBookingRepository.GetBookingsByOperationIdAsync(tourOperationId, true);

                var confirmedBookings = bookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();
                var totalRevenue = confirmedBookings.Sum(b => b.TotalPrice);
                var totalGuests = confirmedBookings.Sum(b => b.NumberOfGuests);

                return new BookingSummaryDto
                {
                    TotalBookings = statistics.Values.Sum(),
                    PendingBookings = statistics.GetValueOrDefault(BookingStatus.Pending, 0),
                    ConfirmedBookings = statistics.GetValueOrDefault(BookingStatus.Confirmed, 0),
                    CancelledBookings = statistics.GetValueOrDefault(BookingStatus.CancelledByCustomer, 0) +
                                      statistics.GetValueOrDefault(BookingStatus.CancelledByCompany, 0),
                    TotalRevenue = totalRevenue,
                    TotalGuests = totalGuests
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking statistics for TourOperation {TourOperationId}", tourOperationId);
                return new BookingSummaryDto();
            }
        }

        /// <summary>
        /// Validate business rules cho booking
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateBookingAsync(RequestCreateBookingDto request, CurrentUserObject currentUser)
        {
            try
            {
                // Check TourOperation exists and is active
                var tourOperation = await _unitOfWork.TourOperationRepository.GetByIdAsync(request.TourOperationId);
                if (tourOperation == null)
                    return (false, "Tour operation không tồn tại");

                if (!tourOperation.IsActive)
                    return (false, "Tour operation không còn hoạt động");

                // Check guest count is valid
                if (request.TotalGuests <= 0)
                    return (false, "Số lượng khách phải lớn hơn 0");

                if (request.AdultCount <= 0)
                    return (false, "Phải có ít nhất 1 người lớn");

                // Check capacity
                var canBook = await _unitOfWork.TourBookingRepository.CanBookAsync(
                    request.TourOperationId, request.TotalGuests, tourOperation.MaxGuests);

                if (!canBook)
                    return (false, "Tour đã hết chỗ hoặc không đủ chỗ cho số lượng khách yêu cầu");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating booking");
                return (false, "Lỗi validation");
            }
        }

        /// <summary>
        /// Generate unique booking code
        /// </summary>
        public async Task<string> GenerateBookingCodeAsync()
        {
            string bookingCode;
            bool isUnique;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                // Format: TN + YYYYMMDD + 4 random digits
                var datePart = DateTime.Now.ToString("yyyyMMdd");
                var randomPart = new Random().Next(1000, 9999).ToString();
                bookingCode = $"TN{datePart}{randomPart}";

                isUnique = !await _unitOfWork.TourBookingRepository.IsBookingCodeExistsAsync(bookingCode);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    // Fallback với timestamp để đảm bảo unique
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    bookingCode = $"TN{timestamp.Substring(timestamp.Length - 8)}";
                    break;
                }
            }
            while (!isUnique);

            return bookingCode;
        }

        /// <summary>
        /// Tính toán giá tiền cho booking
        /// </summary>
        public async Task<decimal> CalculateBookingPriceAsync(Guid tourOperationId, int adultCount, int childCount)
        {
            try
            {
                var tourOperation = await _unitOfWork.TourOperationRepository.GetByIdAsync(tourOperationId,
                    new[] { "TourDetails", "TourDetails.TourTemplate" });

                if (tourOperation == null) return 0;

                var adultPrice = tourOperation.Price;
                // For now, child price is same as adult price since TourTemplate doesn't have ChildPrice
                var childPrice = adultPrice;

                return (adultCount * adultPrice) + (childCount * childPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating booking price for TourOperation {TourOperationId}", tourOperationId);
                return 0;
            }
        }

        /// <summary>
        /// Map TourBooking entity to ResponseBookingDto
        /// </summary>
        private async Task<ResponseBookingDto> MapToBookingResponse(TourBooking booking)
        {
            var statusName = GetBookingStatusName(booking.Status);

            var response = new ResponseBookingDto
            {
                Id = booking.Id,
                TourOperationId = booking.TourOperationId,
                UserId = booking.UserId,
                UserName = booking.User?.Name ?? "N/A",
                UserEmail = booking.User?.Email,
                AdultCount = booking.AdultCount,
                ChildCount = booking.ChildCount,
                TotalGuests = booking.NumberOfGuests,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                StatusName = statusName,
                BookingCode = booking.BookingCode,
                BookingDate = booking.BookingDate,
                ConfirmedDate = booking.ConfirmedDate,
                CancelledDate = booking.CancelledDate,
                CancellationReason = booking.CancellationReason,
                CustomerNotes = booking.CustomerNotes,
                ContactName = booking.ContactName,
                ContactPhone = booking.ContactPhone,
                ContactEmail = booking.ContactEmail,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };

            // Map TourOperation info if available
            if (booking.TourOperation != null)
            {
                response.TourOperation = new TourOperationSummaryDto
                {
                    Id = booking.TourOperation.Id,
                    Price = booking.TourOperation.Price,
                    MaxGuests = booking.TourOperation.MaxGuests,
                    TourTitle = booking.TourOperation.TourDetails?.TourTemplate?.Title ?? "N/A",
                    TourDescription = null, // TourTemplate doesn't have Description field
                    GuideName = booking.TourOperation.TourGuide?.FullName,
                    GuidePhone = booking.TourOperation.TourGuide?.PhoneNumber
                };

                // Get tour date from TourSlot if available
                var tourSlots = await _unitOfWork.TourSlotRepository.GetAllAsync(
                    ts => ts.TourDetailsId == booking.TourOperation.TourDetailsId);
                var tourSlot = tourSlots.FirstOrDefault();
                if (tourSlot != null)
                {
                    response.TourOperation.TourDate = tourSlot.TourDate.ToDateTime(TimeOnly.MinValue);
                }
            }

            return response;
        }

        /// <summary>
        /// Get booking status name in Vietnamese
        /// </summary>
        private string GetBookingStatusName(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "Chờ xác nhận",
                BookingStatus.Confirmed => "Đã xác nhận",
                BookingStatus.CancelledByCustomer => "Đã hủy bởi khách hàng",
                BookingStatus.CancelledByCompany => "Đã hủy bởi công ty",
                BookingStatus.Completed => "Đã hoàn thành",
                BookingStatus.NoShow => "Không xuất hiện",
                BookingStatus.Refunded => "Đã hoàn tiền",
                _ => "Không xác định"
            };
        }
    }
}
