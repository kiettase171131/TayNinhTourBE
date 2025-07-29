using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service xử lý tour booking cho user với early bird pricing
    /// </summary>
    public class UserTourBookingService : IUserTourBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITourPricingService _pricingService;
        private readonly ITourRevenueService _revenueService;
        private readonly IPayOsService _payOsService;
        private readonly IQRCodeService _qrCodeService;
        private readonly EmailSender _emailSender;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserTourBookingService> _logger;

        public UserTourBookingService(
            IUnitOfWork unitOfWork,
            ITourPricingService pricingService,
            ITourRevenueService revenueService,
            IPayOsService payOsService,
            IQRCodeService qrCodeService,
            EmailSender emailSender,
            IServiceProvider serviceProvider,
            ILogger<UserTourBookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _pricingService = pricingService;
            _revenueService = revenueService;
            _payOsService = payOsService;
            _qrCodeService = qrCodeService;
            _emailSender = emailSender;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tours có thể booking
        /// </summary>
        public async Task<Common.PagedResult<AvailableTourDto>> GetAvailableToursAsync(
            int pageIndex = 1, 
            int pageSize = 10, 
            DateTime? fromDate = null, 
            DateTime? toDate = null, 
            string? searchKeyword = null)
        {
            var query = _unitOfWork.TourDetailsRepository.GetQueryable()
                .Where(td => td.Status == TourDetailsStatus.Public && !td.IsDeleted)
                .Include(td => td.TourOperation)
                .Include(td => td.TourTemplate)
                .Include(td => td.AssignedSlots)
                .Where(td => td.TourOperation != null && td.TourOperation.IsActive)
                .Where(td => td.TourOperation!.CurrentBookings < td.TourOperation.MaxGuests);

            // Filter by date range
            if (fromDate.HasValue || toDate.HasValue)
            {
                query = query.Where(td => td.AssignedSlots.Any(slot => 
                    (!fromDate.HasValue || slot.TourDate >= DateOnly.FromDateTime(fromDate.Value)) &&
                    (!toDate.HasValue || slot.TourDate <= DateOnly.FromDateTime(toDate.Value))));
            }

            // Filter by search keyword
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var keyword = searchKeyword.ToLower();
                query = query.Where(td => 
                    td.Title.ToLower().Contains(keyword) ||
                    (td.Description != null && td.Description.ToLower().Contains(keyword)) ||
                    td.TourTemplate.StartLocation.ToLower().Contains(keyword) ||
                    td.TourTemplate.EndLocation.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var tours = await query
                .OrderByDescending(td => td.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(td => new AvailableTourDto
                {
                    TourDetailsId = td.Id,
                    TourOperationId = td.TourOperation!.Id,
                    Title = td.Title,
                    Description = td.Description,
                    ImageUrls = td.ImageUrls,
                    Price = td.TourOperation.Price,
                    MaxGuests = td.TourOperation.MaxGuests,
                    CurrentBookings = td.TourOperation.CurrentBookings,
                    TourStartDate = td.AssignedSlots.Any() ? 
                        td.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : null,
                    StartLocation = td.TourTemplate.StartLocation,
                    EndLocation = td.TourTemplate.EndLocation,
                    IsEarlyBirdEligible = _pricingService.IsEarlyBirdEligible(
                        td.AssignedSlots.Any() ? td.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : DateTime.MaxValue,
                        td.CreatedAt,
                        VietnamTimeZoneUtility.GetVietnamNow()),
                    EarlyBirdPrice = _pricingService.CalculatePrice(
                        td.TourOperation.Price,
                        td.AssignedSlots.Any() ? td.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : DateTime.MaxValue,
                        td.CreatedAt,
                        VietnamTimeZoneUtility.GetVietnamNow()).finalPrice,
                    CreatedAt = td.CreatedAt
                })
                .ToListAsync();

            return new Common.PagedResult<AvailableTourDto>
            {
                Items = tours,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Lấy chi tiết tour để booking
        /// </summary>
        public async Task<TourDetailsForBookingDto?> GetTourDetailsForBookingAsync(Guid tourDetailsId)
        {
            var tourDetails = await _unitOfWork.TourDetailsRepository.GetQueryable()
                .Where(td => td.Id == tourDetailsId && td.Status == TourDetailsStatus.Public && !td.IsDeleted)
                .Include(td => td.TourOperation)
                    .ThenInclude(to => to!.TourGuide)
                .Include(td => td.TourTemplate)
                .Include(td => td.Timeline.OrderBy(t => t.SortOrder))
                    .ThenInclude(t => t.SpecialtyShop)
                .Include(td => td.AssignedSlots)
                .FirstOrDefaultAsync();

            if (tourDetails?.TourOperation == null || !tourDetails.TourOperation.IsActive)
                return null;

            // Get slots with capacity information
            var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
            var slotsData = await tourSlotService.GetSlotsByTourDetailsAsync(tourDetailsId);

            return new TourDetailsForBookingDto
            {
                Id = tourDetails.Id,
                Title = tourDetails.Title,
                Description = tourDetails.Description,
                ImageUrls = tourDetails.ImageUrls,
                SkillsRequired = tourDetails.SkillsRequired,
                CreatedAt = tourDetails.CreatedAt,
                StartLocation = tourDetails.TourTemplate.StartLocation,
                EndLocation = tourDetails.TourTemplate.EndLocation,
                TourOperation = new TourOperationSummaryDto
                {
                    Id = tourDetails.TourOperation.Id,
                    TourDetailsId = tourDetails.Id,
                    TourTitle = tourDetails.Title,
                    Price = tourDetails.TourOperation.Price,
                    MaxGuests = tourDetails.TourOperation.MaxGuests,
                    CurrentBookings = tourDetails.TourOperation.CurrentBookings,
                    TourStartDate = slotsData.Any() ? 
                        slotsData.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : null,
                    GuideId = tourDetails.TourOperation.TourGuide?.Id.ToString(),
                    GuideName = tourDetails.TourOperation.TourGuide?.FullName,
                    GuidePhone = tourDetails.TourOperation.TourGuide?.PhoneNumber
                },
                Timeline = tourDetails.Timeline.Select(t => new TimelineItemDto
                {
                    Id = t.Id,
                    TourDetailsId = t.TourDetailsId,
                    CheckInTime = t.CheckInTime.ToString(@"hh\:mm"),
                    Activity = t.Activity,
                    SpecialtyShopId = t.SpecialtyShopId,
                    SortOrder = t.SortOrder,
                    SpecialtyShop = t.SpecialtyShop != null ? new SpecialtyShopResponseDto
                    {
                        Id = t.SpecialtyShop.Id,
                        ShopName = t.SpecialtyShop.ShopName,
                        ShopType = t.SpecialtyShop.ShopType,
                        Location = t.SpecialtyShop.Location,
                        Description = t.SpecialtyShop.Description,
                        IsShopActive = t.SpecialtyShop.IsActive
                    } : null,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList(),
                TourDates = slotsData.Select(slot => new TourDateDto
                {
                    TourSlotId = slot.Id,
                    TourDate = slot.TourDate.ToDateTime(TimeOnly.MinValue),
                    ScheduleDay = slot.ScheduleDayName,
                    MaxGuests = slot.MaxGuests,
                    CurrentBookings = slot.CurrentBookings,
                    AvailableSpots = slot.AvailableSpots,
                    IsAvailable = slot.IsActive && slot.Status == TourSlotStatus.Available,
                    IsBookable = slot.IsBookable,
                    StatusName = slot.StatusName
                }).OrderBy(d => d.TourDate).ToList()
            };
        }

        /// <summary>
        /// Tính giá tour trước khi booking
        /// </summary>
        public async Task<PriceCalculationDto?> CalculateBookingPriceAsync(CalculatePriceRequest request)
        {
            var tourOperation = await _unitOfWork.TourOperationRepository.GetQueryable()
                .Where(to => to.Id == request.TourOperationId && to.IsActive && !to.IsDeleted)
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.AssignedSlots)
                .FirstOrDefaultAsync();

            if (tourOperation?.TourDetails == null || 
                tourOperation.TourDetails.Status != TourDetailsStatus.Public)
                return null;

            var bookingDate = request.BookingDate ?? VietnamTimeZoneUtility.GetVietnamNow();
            var tourStartDate = tourOperation.TourDetails.AssignedSlots.Any() ? 
                tourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : 
                DateTime.MaxValue;

            var availableSpots = tourOperation.MaxGuests - tourOperation.CurrentBookings;
            var isAvailable = availableSpots >= request.NumberOfGuests && tourStartDate > VietnamTimeZoneUtility.GetVietnamNow();

            var pricingInfo = _pricingService.GetPricingInfo(
                tourOperation.Price,
                tourStartDate,
                tourOperation.TourDetails.CreatedAt,
                bookingDate);

            var totalOriginalPrice = pricingInfo.OriginalPrice * request.NumberOfGuests;
            var totalFinalPrice = pricingInfo.FinalPrice * request.NumberOfGuests;

            return new PriceCalculationDto
            {
                TourOperationId = tourOperation.Id,
                TourTitle = tourOperation.TourDetails.Title,
                NumberOfGuests = request.NumberOfGuests,
                OriginalPricePerGuest = pricingInfo.OriginalPrice,
                TotalOriginalPrice = totalOriginalPrice,
                DiscountPercent = pricingInfo.DiscountPercent,
                DiscountAmount = totalOriginalPrice - totalFinalPrice,
                FinalPrice = totalFinalPrice,
                IsEarlyBird = pricingInfo.IsEarlyBird,
                PricingType = pricingInfo.PricingType,
                DaysSinceCreated = pricingInfo.DaysSinceCreated,
                DaysUntilTour = pricingInfo.DaysUntilTour,
                BookingDate = bookingDate,
                TourStartDate = tourStartDate != DateTime.MaxValue ? tourStartDate : null,
                TourDetailsCreatedAt = tourOperation.TourDetails.CreatedAt,
                IsAvailable = isAvailable,
                AvailableSpots = availableSpots,
                UnavailableReason = !isAvailable ? 
                    (availableSpots < request.NumberOfGuests ? "Không đủ chỗ trống" : "Tour đã khởi hành") : null
            };
        }

        /// <summary>
        /// Tạo booking tour mới
        /// </summary>
        public async Task<CreateBookingResultDto> CreateBookingAsync(CreateTourBookingRequest request, Guid userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Starting tour booking process. UserId: {UserId}, TourSlotId: {TourSlotId}, Guests: {Guests}", 
                    userId, request.TourSlotId, request.NumberOfGuests);

                // 1. Get and validate TourSlot first
                var tourSlot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Where(ts => ts.Id == request.TourSlotId && ts.IsActive && !ts.IsDeleted)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                            .ThenInclude(to => to.CreatedBy)
                    .FirstOrDefaultAsync();

                if (tourSlot?.TourDetails?.TourOperation == null)
                {
                    _logger.LogWarning("Tour slot not found or invalid. TourSlotId: {TourSlotId}", request.TourSlotId);
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Tour slot không tồn tại hoặc không khả dụng"
                    };
                }

                var tourOperation = tourSlot.TourDetails.TourOperation;
                _logger.LogDebug("Found tour operation. Id: {TourOperationId}, MaxGuests: {MaxGuests}, CurrentBookings: {CurrentBookings}", 
                    tourOperation.Id, tourOperation.MaxGuests, tourOperation.CurrentBookings);

                // 2. Validate TourOperation and TourDetails
                if (!tourOperation.IsActive || tourOperation.IsDeleted)
                {
                    _logger.LogWarning("Tour operation not active. TourOperationId: {TourOperationId}, IsActive: {IsActive}, IsDeleted: {IsDeleted}", 
                        tourOperation.Id, tourOperation.IsActive, tourOperation.IsDeleted);
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Tour operation không khả dụng"
                    };
                }

                if (tourSlot.TourDetails.Status != TourDetailsStatus.Public)
                {
                    _logger.LogWarning("Tour not public. TourDetailsId: {TourDetailsId}, Status: {Status}", 
                        tourSlot.TourDetails.Id, tourSlot.TourDetails.Status);
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Tour chưa được công khai"
                    };
                }

                // 3. Check if the tour operation matches the request (if provided)
                if (request.TourOperationId != tourOperation.Id)
                {
                    _logger.LogWarning("Tour operation mismatch. Expected: {Expected}, Provided: {Provided}", 
                        tourOperation.Id, request.TourOperationId);
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Tour slot không thuộc về tour operation được chọn"
                    };
                }

                // 4. Check slot-specific availability
                if (tourSlot.Status != TourSlotStatus.Available)
                {
                    _logger.LogWarning("Tour slot not available. TourSlotId: {TourSlotId}, Status: {Status}", 
                        request.TourSlotId, tourSlot.Status);
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Tour slot không khả dụng để booking"
                    };
                }

                if (tourSlot.AvailableSpots < request.NumberOfGuests)
                {
                    _logger.LogWarning("Insufficient capacity in slot. TourSlotId: {TourSlotId}, Available: {Available}, Requested: {Requested}", 
                        request.TourSlotId, tourSlot.AvailableSpots, request.NumberOfGuests);
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = $"Slot này chỉ còn {tourSlot.AvailableSpots} chỗ trống, không đủ cho {request.NumberOfGuests} khách"
                    };
                }

                var tourStartDate = tourSlot.TourDate.ToDateTime(TimeOnly.MinValue);
                if (tourStartDate <= VietnamTimeZoneUtility.GetVietnamNow())
                {
                    _logger.LogWarning("Tour already started. TourSlotId: {TourSlotId}, TourDate: {TourDate}", 
                        request.TourSlotId, tourStartDate);
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Tour đã khởi hành"
                    };
                }

                // 5. Calculate pricing based on TourOperation (with null check for pricing service)
                var bookingDate = DateTime.UtcNow;
                var baseUrl = "https://tndt.netlify.app"; // Base URL for payment callbacks
                decimal totalPrice;
                decimal discountPercent = 0;
                string pricingType = "Standard";

                try
                {
                    if (_pricingService != null)
                    {
                        var pricingInfo = _pricingService.GetPricingInfo(
                            tourOperation.Price,
                            tourStartDate,
                            tourSlot.TourDetails.CreatedAt,
                            bookingDate);

                        totalPrice = pricingInfo.FinalPrice * request.NumberOfGuests;
                        discountPercent = pricingInfo.DiscountPercent;
                        pricingType = pricingInfo.PricingType;
                    }
                    else
                    {
                        _logger.LogWarning("PricingService is null, using standard pricing");
                        totalPrice = tourOperation.Price * request.NumberOfGuests;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating pricing, falling back to standard price");
                    totalPrice = tourOperation.Price * request.NumberOfGuests;
                }

                // 5.5. Pre-check capacity before attempting to reserve
                var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                var canBook = await tourSlotService.CanBookSlotAsync(request.TourSlotId, request.NumberOfGuests);
                
                if (!canBook)
                {
                    _logger.LogWarning("Pre-check failed for booking. TourSlotId: {TourSlotId}, Guests: {Guests}", 
                        request.TourSlotId, request.NumberOfGuests);
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Slot này hiện không có đủ chỗ trống hoặc không khả dụng. Vui lòng chọn slot khác."
                    };
                }

                // 6. Reserve slot capacity first (optimistic concurrency control)
                var reserveSuccess = await tourSlotService.ReserveSlotCapacityAsync(request.TourSlotId, request.NumberOfGuests);
                
                if (!reserveSuccess)
                {
                    _logger.LogWarning("Failed to reserve slot capacity. TourSlotId: {TourSlotId}, Guests: {Guests}", 
                        request.TourSlotId, request.NumberOfGuests);
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Không thể đặt chỗ cho slot này. Slot có thể đã được đặt bởi khách hàng khác hoặc không đủ chỗ trống."
                    };
                }

                // 7. Generate booking code and PayOS order code
                var bookingCode = GenerateBookingCode();
                var payOsOrderCode = GeneratePayOsOrderCode();

                // 8. Create booking
                var booking = new TourBooking
                {
                    Id = Guid.NewGuid(),
                    TourOperationId = tourOperation.Id,
                    TourSlotId = request.TourSlotId,
                    UserId = userId,
                    NumberOfGuests = request.NumberOfGuests,
                    OriginalPrice = tourOperation.Price * request.NumberOfGuests,
                    DiscountPercent = discountPercent,
                    TotalPrice = totalPrice,
                    Status = BookingStatus.Pending,
                    BookingCode = bookingCode,
                    PayOsOrderCode = payOsOrderCode,
                    BookingDate = bookingDate,
                    ContactName = request.ContactName,
                    ContactPhone = request.ContactPhone,
                    ContactEmail = request.ContactEmail,
                    CustomerNotes = request.SpecialRequests,
                    CreatedAt = bookingDate,
                    CreatedById = userId,
                    ReservedUntil = VietnamTimeZoneUtility.GetVietnamNow().AddMinutes(30) // Reserve slot for 30 minutes
                };

                await _unitOfWork.TourBookingRepository.AddAsync(booking);

                // 9. KHÔNG cộng TourOperation.CurrentBookings ngay lập tức 
                // Chỉ cộng khi thanh toán thành công trong HandlePaymentSuccessAsync
                // tourOperation.CurrentBookings += request.NumberOfGuests; // ❌ Remove this line
                // await _unitOfWork.TourOperationRepository.UpdateAsync(tourOperation); // ❌ Remove this line

                _logger.LogInformation("Tour booking created (PENDING payment). BookingCode: {BookingCode}, TourSlot reserved: {Guests} guests", 
                    bookingCode, request.NumberOfGuests);

                // 10. Create PayOS payment URL for tour booking (with proper error handling)
                string paymentUrl = $"{baseUrl}/tour-payment-cancel?orderId={booking.Id}&orderCode={payOsOrderCode}"; // Default fallback URL
                try
                {
                    if (_payOsService != null)
                    {
                        // FIXED: Use correct method for tour booking payment URLs
                        paymentUrl = await _payOsService.CreateTourBookingPaymentUrlAsync(
                            totalPrice,
                            payOsOrderCode,
                            "https://tndt.netlify.app") ?? paymentUrl;

                        _logger.LogInformation("PayOS tour booking payment URL created successfully for booking {BookingCode}: {PaymentUrl}",
                            bookingCode, paymentUrl);
                    }
                    else
                    {
                        _logger.LogWarning("PayOsService is null, using fallback URL");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating PayOS tour booking URL for booking {BookingCode}, using fallback", bookingCode);
                    // Keep the fallback URL
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Tour booking created successfully. BookingId: {BookingId}, BookingCode: {BookingCode}, UserId: {UserId}", 
                    booking.Id, bookingCode, userId);

                return new CreateBookingResultDto
                {
                    Success = true,
                    Message = "Tạo booking thành công",
                    BookingId = booking.Id,
                    BookingCode = bookingCode,
                    PaymentUrl = paymentUrl,
                    OriginalPrice = tourOperation.Price * request.NumberOfGuests,
                    DiscountPercent = discountPercent,
                    FinalPrice = totalPrice,
                    PricingType = pricingType,
                    BookingDate = bookingDate,
                    TourStartDate = tourStartDate
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Concurrency conflict during booking creation. UserId: {UserId}, TourSlotId: {TourSlotId}", 
                    userId, request.TourSlotId);
                return new CreateBookingResultDto
                {
                    Success = false,
                    Message = "Tour slot đã được booking bởi người khác, vui lòng thử lại"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating tour booking. UserId: {UserId}, TourSlotId: {TourSlotId}", 
                    userId, request.TourSlotId);
                return new CreateBookingResultDto
                {
                    Success = false,
                    Message = $"Lỗi khi tạo booking: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Generate unique booking code
        /// Format: TB + YYYYMMDD + 6 random digits
        /// </summary>
        private string GenerateBookingCode()
        {
            var dateStr = VietnamTimeZoneUtility.GetVietnamNow().ToString("yyyyMMdd");
            var random = new Random().Next(100000, 999999);
            return $"TB{dateStr}{random}";
        }

        /// <summary>
        /// Generate PayOS order code
        /// Format: TNDT + 10 digits (7 from timestamp + 3 random)
        /// </summary>
        private string GeneratePayOsOrderCode()
        {
            // Tạo timestamp với milliseconds để tăng tính unique
            var timestamp = VietnamTimeZoneUtility.GetVietnamNow().Ticks.ToString();
            var timestampLast7 = timestamp.Substring(Math.Max(0, timestamp.Length - 7));
            var random = new Random().Next(100, 999);
            return $"TNDT{timestampLast7}{random}";
        }

        /// <summary>
        /// Lấy danh sách bookings của user
        /// </summary>
        public async Task<Common.PagedResult<TourBookingDto>> GetUserBookingsAsync(Guid userId, int pageIndex = 1, int pageSize = 10)
        {
            var query = _unitOfWork.TourBookingRepository.GetQueryable()
                .Where(b => b.UserId == userId && !b.IsDeleted)
                .Include(b => b.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                .Include(b => b.TourOperation)
                    .ThenInclude(to => to.TourGuide)
                .Include(b => b.User);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new TourBookingDto
                {
                    Id = b.Id,
                    TourOperationId = b.TourOperationId,
                    UserId = b.UserId,
                    NumberOfGuests = b.NumberOfGuests,
                    OriginalPrice = b.OriginalPrice,
                    DiscountPercent = b.DiscountPercent,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    StatusName = GetBookingStatusName(b.Status),
                    BookingCode = b.BookingCode,
                    PayOsOrderCode = b.PayOsOrderCode,
                    QRCodeData = b.QRCodeData,
                    BookingDate = b.BookingDate,
                    ConfirmedDate = b.ConfirmedDate,
                    CancelledDate = b.CancelledDate,
                    CancellationReason = b.CancellationReason,
                    CustomerNotes = b.CustomerNotes,
                    ContactName = b.ContactName,
                    ContactPhone = b.ContactPhone,
                    ContactEmail = b.ContactEmail,
                    SpecialRequests = b.CustomerNotes,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    TourOperation = b.TourOperation != null ? new TourOperationSummaryDto
                    {
                        Id = b.TourOperation.Id,
                        TourDetailsId = b.TourOperation.TourDetailsId,
                        TourTitle = b.TourOperation.TourDetails != null ? b.TourOperation.TourDetails.Title : "",
                        Price = b.TourOperation.Price,
                        MaxGuests = b.TourOperation.MaxGuests,
                        CurrentBookings = b.TourOperation.CurrentBookings,
                        TourStartDate = b.TourOperation.TourDetails != null && b.TourOperation.TourDetails.AssignedSlots.Any() ?
                            b.TourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : null,
                        GuideId = b.TourOperation.TourGuide != null ? b.TourOperation.TourGuide.Id.ToString() : null,
                        GuideName = b.TourOperation.TourGuide != null ? b.TourOperation.TourGuide.FullName : null,
                        GuidePhone = b.TourOperation.TourGuide != null ? b.TourOperation.TourGuide.PhoneNumber : null
                    } : null,
                    User = new DTOs.Response.TourBooking.UserSummaryDto
                    {
                        Id = b.User.Id,
                        Name = b.User.Name,
                        Email = b.User.Email,
                        PhoneNumber = b.User.PhoneNumber
                    }
                })
                .ToListAsync();

            return new Common.PagedResult<TourBookingDto>
            {
                Items = bookings,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Hủy booking
        /// </summary>
        public async Task<BaseResposeDto> CancelBookingAsync(Guid bookingId, Guid userId, string? reason = null)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.Id == bookingId && b.UserId == userId && !b.IsDeleted)
                    .Include(b => b.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                    .Include(b => b.TourSlot)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking"
                    };
                }

                if (booking.Status == BookingStatus.CancelledByCustomer ||
                    booking.Status == BookingStatus.CancelledByCompany)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Booking đã được hủy trước đó"
                    };
                }

                if (booking.Status == BookingStatus.Completed)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Không thể hủy booking đã hoàn thành"
                    };
                }

                // ✅ Check if booking was confirmed (to know if we need to release TourOperation capacity)
                var wasConfirmed = booking.Status == BookingStatus.Confirmed;

                // Update booking status
                booking.Status = BookingStatus.CancelledByCustomer;
                booking.CancelledDate = DateTime.UtcNow;
                booking.CancellationReason = reason ?? "Hủy bởi khách hàng";
                booking.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);

                // ✅ Chỉ trừ TourOperation.CurrentBookings nếu booking đã được CONFIRMED (đã thanh toán)
                // Release capacity from TourOperation
                if (booking.TourOperation != null && wasConfirmed)
                {
                    booking.TourOperation.CurrentBookings -= booking.NumberOfGuests;
                    await _unitOfWork.TourOperationRepository.UpdateAsync(booking.TourOperation);
                    
                    _logger.LogInformation("Released TourOperation capacity (-{Guests}) for confirmed booking {BookingCode}", 
                        booking.NumberOfGuests, booking.BookingCode);
                }

                // ✅ Luôn release TourSlot capacity (đã được reserve từ khi tạo booking)
                // Release capacity from TourSlot
                if (booking.TourSlotId.HasValue)
                {
                    var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                    await tourSlotService.ReleaseSlotCapacityAsync(booking.TourSlotId.Value, booking.NumberOfGuests);
                    
                    _logger.LogInformation("Released TourSlot capacity (-{Guests}) for cancelled booking {BookingCode}", 
                        booking.NumberOfGuests, booking.BookingCode);
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Hủy booking thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi hủy booking: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Lấy chi tiết booking theo ID
        /// </summary>
        public async Task<TourBookingDto?> GetBookingDetailsAsync(Guid bookingId, Guid userId)
        {
            var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                .Where(b => b.Id == bookingId && b.UserId == userId && !b.IsDeleted)
                .Include(b => b.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                .Include(b => b.TourOperation)
                    .ThenInclude(to => to.TourGuide)
                .Include(b => b.User)
                .FirstOrDefaultAsync();

            if (booking == null)
                return null;

            return new TourBookingDto
            {
                Id = booking.Id,
                TourOperationId = booking.TourOperationId,
                UserId = booking.UserId,
                NumberOfGuests = booking.NumberOfGuests,
                OriginalPrice = booking.OriginalPrice,
                DiscountPercent = booking.DiscountPercent,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                StatusName = GetBookingStatusName(booking.Status),
                BookingCode = booking.BookingCode,
                PayOsOrderCode = booking.PayOsOrderCode,
                QRCodeData = booking.QRCodeData,
                BookingDate = booking.BookingDate,
                ConfirmedDate = booking.ConfirmedDate,
                CancelledDate = booking.CancelledDate,
                CancellationReason = booking.CancellationReason,
                CustomerNotes = booking.CustomerNotes,
                ContactName = booking.ContactName,
                ContactPhone = booking.ContactPhone,
                ContactEmail = booking.ContactEmail,
                SpecialRequests = booking.CustomerNotes,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                TourOperation = booking.TourOperation != null ? new TourOperationSummaryDto
                {
                    Id = booking.TourOperation.Id,
                    TourDetailsId = booking.TourOperation.TourDetailsId,
                    TourTitle = booking.TourOperation.TourDetails?.Title ?? "",
                    Price = booking.TourOperation.Price,
                    MaxGuests = booking.TourOperation.MaxGuests,
                    CurrentBookings = booking.TourOperation.CurrentBookings,
                    TourStartDate = booking.TourOperation.TourDetails?.AssignedSlots.Any() == true ?
                        booking.TourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : null,
                    GuideId = booking.TourOperation.TourGuide?.Id.ToString(),
                    GuideName = booking.TourOperation.TourGuide?.FullName,
                    GuidePhone = booking.TourOperation.TourGuide?.PhoneNumber
                } : null,
                User = new DTOs.Response.TourBooking.UserSummaryDto
                {
                    Id = booking.User.Id,
                    Name = booking.User.Name,
                    Email = booking.User.Email,
                    PhoneNumber = booking.User.PhoneNumber
                }
            };
        }

        /// <summary>
        /// Xử lý callback thanh toán thành công
        /// </summary>
        public async Task<BaseResposeDto> HandlePaymentSuccessAsync(string payOsOrderCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.PayOsOrderCode == payOsOrderCode && !b.IsDeleted)
                    .Include(b => b.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking với mã thanh toán này"
                    };
                }

                if (booking.Status == BookingStatus.Confirmed)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 200,
                        Message = "Booking đã được xác nhận trước đó",
                        success = true
                    };
                }

                // Update booking status
                booking.Status = BookingStatus.Confirmed;
                booking.ConfirmedDate = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.ReservedUntil = null; // Clear reservation timeout since payment is confirmed

                // Generate QR code for customer
                booking.QRCodeData = GenerateQRCodeData(booking);

                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);

                // ✅ Cộng TourOperation.CurrentBookings KHI THANH TOÁN THÀNH CÔNG
                if (booking.TourOperation != null)
                {
                    booking.TourOperation.CurrentBookings += booking.NumberOfGuests;
                    await _unitOfWork.TourOperationRepository.UpdateAsync(booking.TourOperation);
                    
                    _logger.LogInformation("Updated TourOperation.CurrentBookings (+{Guests}) for booking {BookingCode}", 
                        booking.NumberOfGuests, booking.BookingCode);
                }

                // ✅ CẬP NHẬT TourSlot.CurrentBookings KHI THANH TOÁN THÀNH CÔNG
                if (booking.TourSlotId.HasValue)
                {
                    var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                    var confirmSuccess = await tourSlotService.ConfirmSlotCapacityAsync(booking.TourSlotId.Value, booking.NumberOfGuests);
                    
                    if (confirmSuccess)
                    {
                        _logger.LogInformation("Updated TourSlot.CurrentBookings (+{Guests}) for booking {BookingCode}", 
                            booking.NumberOfGuests, booking.BookingCode);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to update TourSlot.CurrentBookings for booking {BookingCode}. Slot may be unavailable.", 
                            booking.BookingCode);
                    }
                }

                // Add money to TourCompany revenue hold
                if (booking.TourOperation?.TourDetails != null)
                {
                    var tourCompanyUserId = booking.TourOperation.CreatedById;
                    await _revenueService.AddToRevenueHoldAsync(tourCompanyUserId, booking.TotalPrice, booking.Id);
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send confirmation email with QR code (async, don't wait for completion)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendBookingConfirmationEmailAsync(booking);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send booking confirmation email for booking {BookingId}", booking.Id);
                    }
                });

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Thanh toán thành công - Booking đã được xác nhận",
                    success = true
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi xử lý thanh toán thành công: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Xử lý callback thanh toán hủy
        /// </summary>
        public async Task<BaseResposeDto> HandlePaymentCancelAsync(string payOsOrderCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.PayOsOrderCode == payOsOrderCode && !b.IsDeleted)
                    .Include(b => b.TourOperation)
                    .Include(b => b.TourSlot)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking với mã thanh toán này"
                    };
                }

                if (booking.Status != BookingStatus.Pending)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Booking không ở trạng thái chờ thanh toán"
                    };
                }

                // Cancel booking and release capacity
                booking.Status = BookingStatus.CancelledByCustomer;
                booking.CancelledDate = DateTime.UtcNow;
                booking.CancellationReason = "Hủy thanh toán";
                booking.UpdatedAt = DateTime.UtcNow;
                booking.ReservedUntil = null; // Clear reservation timeout since booking is cancelled

                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);

                // Release capacity from TourOperation
                // if (booking.TourOperation != null)
                // {
                //     booking.TourOperation.CurrentBookings -= booking.NumberOfGuests;
                //     await _unitOfWork.TourOperationRepository.UpdateAsync(booking.TourOperation);
                // }

                // Release capacity from TourSlot
                if (booking.TourSlotId.HasValue)
                {
                    var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                    await tourSlotService.ReleaseSlotCapacityAsync(booking.TourSlotId.Value, booking.NumberOfGuests);
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã hủy booking do không thanh toán",
                    success = true
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi xử lý hủy thanh toán: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Send booking confirmation email with QR code
        /// </summary>
        private async Task SendBookingConfirmationEmailAsync(TourBooking booking)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(booking.ContactEmail))
                {
                    _logger.LogWarning("No email address provided for booking {BookingId}", booking.Id);
                    return;
                }

                // Generate QR code image
                var qrCodeImage = await _qrCodeService.GenerateQRCodeImageAsync(booking, 300);

                // Prepare email data
                var customerName = booking.ContactName ?? "Valued Customer";
                var tourTitle = booking.TourOperation?.TourDetails?.Title ?? "Tour Experience";
                var tourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? VietnamTimeZoneUtility.GetVietnamNow();

                // Send email
                await _emailSender.SendTourBookingConfirmationAsync(
                    toEmail: booking.ContactEmail,
                    customerName: customerName,
                    bookingCode: booking.BookingCode,
                    tourTitle: tourTitle,
                    tourDate: tourDate,
                    numberOfGuests: booking.NumberOfGuests,
                    totalPrice: booking.TotalPrice,
                    contactPhone: booking.ContactPhone ?? "N/A",
                    qrCodeImage: qrCodeImage
                );

                _logger.LogInformation("Booking confirmation email sent successfully for booking {BookingId}", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send booking confirmation email for booking {BookingId}", booking.Id);
                throw;
            }
        }

        /// <summary>
        /// Generate QR code data for customer
        /// </summary>
        private string GenerateQRCodeData(TourBooking booking)
        {
            var qrData = new
            {
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                UserId = booking.UserId,
                TourOperationId = booking.TourOperationId,
                NumberOfGuests = booking.NumberOfGuests,
                TotalPrice = booking.TotalPrice,
                BookingDate = booking.BookingDate,
                Status = booking.Status.ToString()
            };

            return System.Text.Json.JsonSerializer.Serialize(qrData);
        }

        /// <summary>
        /// Get booking status name in Vietnamese
        /// </summary>
        private string GetBookingStatusName(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "Chờ thanh toán",
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
