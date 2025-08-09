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

            var tourDetailsData = await query
                .OrderByDescending(td => td.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var tours = new List<AvailableTourDto>();
            var currentDate = VietnamTimeZoneUtility.GetVietnamNow();

            foreach (var td in tourDetailsData)
            {
                var tourStartDate = td.AssignedSlots.Any() ? 
                    td.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : 
                    DateTime.MaxValue;

                // NEW LOGIC: Sử dụng UpdatedAt như ngày công khai tour (khi status chuyển thành Public)
                // Nếu không có UpdatedAt, fallback về CreatedAt
                var tourPublicDate = td.UpdatedAt ?? td.CreatedAt;

                // Get pricing information với logic mới
                var pricingInfo = _pricingService.GetPricingInfo(
                    td.TourOperation!.Price,
                    tourStartDate,
                    tourPublicDate, // Sử dụng ngày công khai thay vì CreatedAt
                    currentDate);

                // Calculate early bird end date với logic mới
                var earlyBirdEndDate = pricingInfo.IsEarlyBird 
                    ? _pricingService.CalculateEarlyBirdEndDate(tourPublicDate, tourStartDate)
                    : (DateTime?)null;
                
                var daysRemainingForEarlyBird = pricingInfo.IsEarlyBird 
                    ? _pricingService.CalculateDaysRemainingForEarlyBird(tourPublicDate, tourStartDate, currentDate)
                    : 0;

                tours.Add(new AvailableTourDto
                {
                    TourDetailsId = td.Id,
                    TourOperationId = td.TourOperation.Id,
                    Title = td.Title,
                    Description = td.Description,
                    ImageUrls = td.ImageUrls,
                    Price = td.TourOperation.Price,
                    MaxGuests = td.TourOperation.MaxGuests,
                    CurrentBookings = td.TourOperation.CurrentBookings,
                    TourStartDate = tourStartDate != DateTime.MaxValue ? tourStartDate : null,
                    StartLocation = td.TourTemplate.StartLocation,
                    EndLocation = td.TourTemplate.EndLocation,
                    CreatedAt = td.CreatedAt,

                    // Enhanced Early Bird Information với logic mới
                    IsEarlyBirdActive = pricingInfo.IsEarlyBird,
                    EarlyBirdPrice = pricingInfo.FinalPrice,
                    EarlyBirdDiscountPercent = pricingInfo.DiscountPercent,
                    EarlyBirdDiscountAmount = pricingInfo.DiscountAmount,
                    EarlyBirdEndDate = earlyBirdEndDate,
                    DaysRemainingForEarlyBird = daysRemainingForEarlyBird,
                    PricingType = pricingInfo.PricingType
                });
            }

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

            var currentDate = VietnamTimeZoneUtility.GetVietnamNow();
            var tourStartDate = slotsData.Any() ? 
                slotsData.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : 
                DateTime.MaxValue;

            // NEW LOGIC: Sử dụng UpdatedAt như ngày công khai tour
            var tourPublicDate = tourDetails.UpdatedAt ?? tourDetails.CreatedAt;

            // Get early bird information với logic mới
            var pricingInfo = _pricingService.GetPricingInfo(
                tourDetails.TourOperation.Price,
                tourStartDate,
                tourPublicDate, // Ngày công khai thay vì CreatedAt
                currentDate);

            var earlyBirdEndDate = pricingInfo.IsEarlyBird 
                ? _pricingService.CalculateEarlyBirdEndDate(tourPublicDate, tourStartDate)
                : (DateTime?)null;
            
            var daysRemainingForEarlyBird = pricingInfo.IsEarlyBird 
                ? _pricingService.CalculateDaysRemainingForEarlyBird(tourPublicDate, tourStartDate, currentDate)
                : 0;

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
                    TourStartDate = tourStartDate != DateTime.MaxValue ? tourStartDate : null,
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
                TourDates = slotsData.Select(slot => 
                {
                    // Calculate pricing for each specific slot date
                    var slotTourDate = slot.TourDate.ToDateTime(TimeOnly.MinValue);
                    var slotPricingInfo = _pricingService.GetPricingInfo(
                        tourDetails.TourOperation.Price,
                        slotTourDate,
                        tourPublicDate, // Sử dụng ngày công khai
                        currentDate);

                    return new TourDateDto
                    {
                        TourSlotId = slot.Id,
                        TourDate = slotTourDate,
                        ScheduleDay = slot.ScheduleDayName,
                        MaxGuests = slot.MaxGuests,
                        CurrentBookings = slot.CurrentBookings,
                        AvailableSpots = slot.AvailableSpots,
                        IsAvailable = slot.IsActive && slot.Status == TourSlotStatus.Available,
                        IsBookable = slot.IsBookable,
                        StatusName = slot.StatusName,
                        
                        // Pricing information for this specific slot
                        OriginalPrice = slotPricingInfo.OriginalPrice,
                        FinalPrice = slotPricingInfo.FinalPrice,
                        IsEarlyBirdApplicable = slotPricingInfo.IsEarlyBird,
                        EarlyBirdDiscountPercent = slotPricingInfo.DiscountPercent
                    };
                }).OrderBy(d => d.TourDate).ToList(),

                // Enhanced Early Bird Information với logic mới
                EarlyBirdInfo = new EarlyBirdInfoDto
                {
                    IsActive = pricingInfo.IsEarlyBird,
                    DiscountPercent = pricingInfo.DiscountPercent,
                    EndDate = earlyBirdEndDate,
                    DaysRemaining = daysRemainingForEarlyBird,
                    Description = pricingInfo.IsEarlyBird 
                        ? $"Đặt sớm tiết kiệm {pricingInfo.DiscountPercent}% trong {daysRemainingForEarlyBird} ngày còn lại! " +
                          $"(Early Bird từ {tourPublicDate:dd/MM/yyyy} {pricingInfo.EarlyBirdDescription})"
                        : "Không có ưu đãi Early Bird",
                    OriginalPrice = pricingInfo.OriginalPrice,
                    DiscountedPrice = pricingInfo.FinalPrice,
                    SavingsAmount = pricingInfo.DiscountAmount
                }
            };
        }

        /// <summary>
        /// Tính giá tour trước khi booking với logic Early Bird mới
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

            // NEW LOGIC: Sử dụng UpdatedAt như ngày công khai tour
            var tourPublicDate = tourOperation.TourDetails.UpdatedAt ?? tourOperation.TourDetails.CreatedAt;

            var availableSpots = tourOperation.MaxGuests - tourOperation.CurrentBookings;
            var isAvailable = availableSpots >= request.NumberOfGuests && tourStartDate > VietnamTimeZoneUtility.GetVietnamNow();

            var pricingInfo = _pricingService.GetPricingInfo(
                tourOperation.Price,
                tourStartDate,
                tourPublicDate, // Sử dụng ngày công khai thay vì CreatedAt
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
                DaysSinceCreated = pricingInfo.DaysSinceCreated, // Bây giờ là days since public
                DaysUntilTour = pricingInfo.DaysUntilTour,
                BookingDate = bookingDate,
                TourStartDate = tourStartDate != DateTime.MaxValue ? tourStartDate : null,
                TourDetailsCreatedAt = tourOperation.TourDetails.CreatedAt, // Giữ để reference
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
            // Sử dụng execution strategy để tránh conflict với retry strategy
            var strategy = _unitOfWork.GetExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
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
                    if (request.TourOperationId.HasValue && request.TourOperationId != tourOperation.Id)
                    {
                        _logger.LogWarning("Tour operation mismatch. Expected: {Expected}, Provided: {Provided}",
                            tourOperation.Id, request.TourOperationId);
                        return new CreateBookingResultDto
                        {
                            Success = false,
                            Message = "Tour slot không thuộc về tour operation được chọn"
                        };
                    }

                    // Auto-assign TourOperationId from TourSlot if not provided
                    if (!request.TourOperationId.HasValue)
                    {
                        _logger.LogInformation("Auto-assigning TourOperationId {TourOperationId} from TourSlot {TourSlotId}",
                            tourOperation.Id, request.TourSlotId);
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

                    // Calculate available spots correctly using TourOperation.MaxGuests
                    var actualMaxGuests = tourSlot.TourDetails?.TourOperation?.MaxGuests ?? tourSlot.MaxGuests;
                    var actualAvailableSpots = actualMaxGuests - tourSlot.CurrentBookings;

                    if (actualAvailableSpots < request.NumberOfGuests)
                    {
                        _logger.LogWarning("Insufficient capacity in slot. TourSlotId: {TourSlotId}, Available: {Available}, Requested: {Requested}, ActualMaxGuests: {ActualMaxGuests}, SlotMaxGuests: {SlotMaxGuests}",
                            request.TourSlotId, actualAvailableSpots, request.NumberOfGuests, actualMaxGuests, tourSlot.MaxGuests);
                        return new CreateBookingResultDto
                        {
                            Success = false,
                            Message = $"Slot này chỉ còn {actualAvailableSpots} chỗ trống, không đủ cho {request.NumberOfGuests} khách"
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
                    decimal totalPrice;
                    decimal originalTotalPrice = tourOperation.Price * request.NumberOfGuests; // Store original total for comparison
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

                            _logger.LogInformation("Pricing calculated for booking - Original: {OriginalPrice}, Final: {FinalPrice}, Discount: {DiscountPercent}%, Type: {PricingType}", 
                                originalTotalPrice, totalPrice, discountPercent, pricingType);
                        }
                        else
                        {
                            _logger.LogWarning("PricingService is null, using standard pricing");
                            totalPrice = originalTotalPrice;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error calculating pricing, falling back to standard price");
                        totalPrice = originalTotalPrice;
                    }

                    // Validate pricing logic - ensure discounted customers pay less
                    if (discountPercent > 0 && totalPrice >= originalTotalPrice)
                    {
                        _logger.LogError("PRICING ERROR: Discounted price ({DiscountedPrice}) is not less than original price ({OriginalPrice}) for discount {DiscountPercent}%", 
                            totalPrice, originalTotalPrice, discountPercent);
                        
                        // Force recalculation with manual discount application as fallback
                        totalPrice = originalTotalPrice * (1 - discountPercent / 100);
                        _logger.LogWarning("Applied manual discount calculation - Final price: {FinalPrice}", totalPrice);
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
                            Message = "Không thể đặt chỗ cho slot này. Slot có thể đã được đặt bởi khách hàng hoặc không đủ chỗ trống."
                        };
                    }

                    // 7. Generate booking code and PayOS order code
                    var bookingCode = GenerateBookingCode();
                    var payOsOrderCode = GeneratePayOsOrderCode();

                    // 8. Create booking entity
                    var booking = new TourBooking
                    {
                        Id = Guid.NewGuid(),
                        TourOperationId = tourOperation.Id,
                        TourSlotId = request.TourSlotId,
                        UserId = userId,
                        NumberOfGuests = request.NumberOfGuests,
                        OriginalPrice = originalTotalPrice, // Store original price before discount
                        DiscountPercent = discountPercent,
                        TotalPrice = totalPrice, // Store final price after discount
                        RevenueHold = 0, // ✅ Will be set when payment is confirmed
                        Status = BookingStatus.Pending,
                        BookingCode = bookingCode,
                        PayOsOrderCode = payOsOrderCode,
                        BookingDate = bookingDate,
                        ContactName = request.ContactName,
                        ContactPhone = request.ContactPhone,
                        ContactEmail = request.ContactEmail,
                        CustomerNotes = request.SpecialRequests,
                        IsCheckedIn = false, // ✅ Default check-in status
                        ReservedUntil = VietnamTimeZoneUtility.GetVietnamNow().AddMinutes(30), // Reserve slot for 30 minutes
                        IsActive = true,
                        IsDeleted = false, // ✅ Explicitly set BaseEntity fields
                        CreatedAt = bookingDate,
                        CreatedById = userId
                        // ✅ RowVersion will be auto-generated by database
                    };

                    // 9. Save booking to database FIRST to ensure FK exists for PaymentTransaction
                    await _unitOfWork.TourBookingRepository.AddAsync(booking);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Tour booking saved to database. BookingCode: {BookingCode}, BookingId: {BookingId}", 
                        bookingCode, booking.Id);

                    // 10. Create PayOS payment link (now that booking exists in DB)
                    if (_payOsService == null)
                    {
                        _logger.LogError("PayOsService is null - service not injected properly");
                        return new CreateBookingResultDto
                        {
                            Success = false,
                            Message = "PayOS service không khả dụng. Vui lòng thử lại sau."
                        };
                    }

                    // Use Enhanced PayOS system with PaymentTransaction tracking
                    var paymentRequest = new TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment.CreatePaymentRequestDto
                    {
                        OrderId = null, // NULL for tour booking payments
                        TourBookingId = booking.Id, // ✅ Now booking.Id exists in database
                        Amount = totalPrice, // ✅ Use discounted price - customers pay what they should pay
                        Description = $"Tour {payOsOrderCode}" // Shortened to fit 25 char limit
                    };

                    _logger.LogInformation("Creating PayOS payment for booking {BookingCode} - Amount: {Amount} (Original: {OriginalPrice}, Discount: {DiscountPercent}%)", 
                        bookingCode, totalPrice, originalTotalPrice, discountPercent);

                    PaymentTransaction paymentTransaction;
                    try
                    {
                        _logger.LogInformation("Creating PayOS payment link for booking {BookingCode}, Amount: {Amount}", 
                            bookingCode, totalPrice);
                        
                        paymentTransaction = await _payOsService.CreatePaymentLinkAsync(paymentRequest);
                        
                        if (paymentTransaction == null)
                        {
                            _logger.LogError("PayOS service returned null payment transaction for booking {BookingCode}", bookingCode);
                            return new CreateBookingResultDto
                            {
                                Success = false,
                                Message = "Không thể tạo link thanh toán. Vui lòng thử lại."
                            };
                        }
                        
                        if (string.IsNullOrEmpty(paymentTransaction.CheckoutUrl))
                        {
                            _logger.LogError("PayOS service returned empty checkout URL for booking {BookingCode}", bookingCode);
                            return new CreateBookingResultDto
                            {
                                Success = false,
                                Message = "Link thanh toán không hợp lệ. Vui lòng thử lại."
                            };
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogError(ex, "PayOS operation error for booking {BookingCode}: {Error}", bookingCode, ex.Message);
                        return new CreateBookingResultDto
                        {
                            Success = false,
                            Message = $"Lỗi thanh toán: {ex.Message}"
                        };
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(ex, "PayOS argument error for booking {BookingCode}: {Error}", bookingCode, ex.Message);
                        return new CreateBookingResultDto
                        {
                            Success = false,
                            Message = $"Thông tin thanh toán không hợp lệ: {ex.Message}"
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected PayOS error for booking {BookingCode}: {Error}", bookingCode, ex.Message);
                        return new CreateBookingResultDto
                        {
                            Success = false,
                            Message = $"Lỗi khi tạo link thanh toán: {ex.Message}. Vui lòng kiểm tra thông tin và thử lại."
                        };
                    }
                    
                    string paymentUrl = paymentTransaction.CheckoutUrl;

                    _logger.LogInformation("Enhanced PayOS tour booking payment created successfully for booking {BookingCode}: TransactionId={TransactionId}, PaymentUrl={PaymentUrl}",
                        bookingCode, paymentTransaction.Id, paymentUrl);

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
            });
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
        /// Generate PayOS order code using utility
        /// Format: TNDT + 10 digits (7 from timestamp + 3 random)
        /// </summary>
        private string GeneratePayOsOrderCode()
        {
            return PayOsOrderCodeUtility.GeneratePayOsOrderCode();
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
            // ✅ Sử dụng execution strategy để handle retry logic với transactions
            var strategy = _unitOfWork.GetExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
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
            });
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
                    TourStartDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue),
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
        /// Enhanced logic: Xử lý slot, revenue hold, QR code generation và gửi email
        /// </summary>
        public async Task<BaseResposeDto> HandlePaymentSuccessAsync(string payOsOrderCode)
        {
            // Sử dụng execution strategy thay vì manual transaction để tránh conflict với retry strategy
            var strategy = _unitOfWork.GetExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                        .Where(b => b.PayOsOrderCode == payOsOrderCode && !b.IsDeleted)
                        .Include(b => b.TourOperation)
                            .ThenInclude(to => to.TourDetails)
                                .ThenInclude(td => td.CreatedBy) // Include TourCompany user
                        .Include(b => b.TourSlot)
                        .Include(b => b.User) // Include customer info
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

                    _logger.LogInformation("Processing payment success for booking {BookingCode}, TotalPrice: {TotalPrice}, OriginalPrice: {OriginalPrice}, DiscountPercent: {DiscountPercent}%", 
                        booking.BookingCode, booking.TotalPrice, booking.OriginalPrice, booking.DiscountPercent);

                    // 1. Update booking status và generate QR code
                    booking.Status = BookingStatus.Confirmed;
                    booking.ConfirmedDate = DateTime.UtcNow;
                    booking.UpdatedAt = DateTime.UtcNow;
                    booking.ReservedUntil = null; // Clear reservation timeout since payment is confirmed

                    // Generate QR code for customer using QRCodeService (not the local duplicate method)
                    booking.QRCodeData = _qrCodeService.GenerateQRCodeData(booking);

                    _logger.LogInformation("Generated QR code for booking {BookingCode} with pricing data - OriginalPrice: {OriginalPrice}, TotalPrice: {TotalPrice}, DiscountPercent: {DiscountPercent}%", 
                        booking.BookingCode, booking.OriginalPrice, booking.TotalPrice, booking.DiscountPercent);

                    // 2. Calculate và set revenue hold (100% của total price, không trừ commission)
                    // UPDATED: Giữ đủ 100% số tiền khách thanh toán, chỉ trừ commission khi chuyển tiền cho TourCompany
                    booking.RevenueHold = booking.TotalPrice; // Giữ đủ 100% số tiền

                    _logger.LogInformation("Setting revenue hold in booking {BookingCode}: {RevenueHold} (100% of {TotalPrice})", 
                        booking.BookingCode, booking.RevenueHold, booking.TotalPrice);

                    await _unitOfWork.TourBookingRepository.UpdateAsync(booking);

                    // 3. ✅ Cộng TourOperation.CurrentBookings KHI THANH TOÁN THÀNH CÔNG
                    if (booking.TourOperation != null)
                    {
                        booking.TourOperation.CurrentBookings += booking.NumberOfGuests;
                        await _unitOfWork.TourOperationRepository.UpdateAsync(booking.TourOperation);
                        
                        _logger.LogInformation("Updated TourOperation.CurrentBookings (+{Guests}) for booking {BookingCode}", 
                            booking.NumberOfGuests, booking.BookingCode);
                    }

                    // 4. ✅ CẬP NHẬT TourSlot capacity - Xóa slots available dựa theo số khách mua
                    if (booking.TourSlotId.HasValue)
                    {
                        var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                        var confirmSuccess = await tourSlotService.ConfirmSlotCapacityAsync(booking.TourSlotId.Value, booking.NumberOfGuests);
                        
                        if (confirmSuccess)
                        {
                            _logger.LogInformation("Updated TourSlot capacity: -{Guests} available slots for booking {BookingCode}", 
                                booking.NumberOfGuests, booking.BookingCode);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to update TourSlot capacity for booking {BookingCode}. Slot may be unavailable.", 
                                booking.BookingCode);
                        }
                    }

                    // 5. Commit transaction trước khi gửi email (để đảm bảo data integrity)
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Payment success processed successfully for booking {BookingCode}. Revenue hold: {RevenueHold}", 
                        booking.BookingCode, booking.RevenueHold);

                    // 6. ✅ Send confirmation email with QR code - FIXED: Better error handling and validation
                    string emailStatus = "Email xác nhận đã được gửi";
                    bool emailSent = false;
                    
                    try
                    {
                        // Determine email address (priority: ContactEmail -> User.Email)
                        var customerEmail = !string.IsNullOrWhiteSpace(booking.ContactEmail) 
                            ? booking.ContactEmail 
                            : booking.User?.Email;

                        if (string.IsNullOrWhiteSpace(customerEmail))
                        {
                            _logger.LogWarning("No email address available for booking {BookingId} - ContactEmail: {ContactEmail}, UserEmail: {UserEmail}", 
                                booking.Id, booking.ContactEmail, booking.User?.Email);
                            emailStatus = "Không có địa chỉ email để gửi vé QR";
                        }
                        else if (!IsValidEmail(customerEmail))
                        {
                            _logger.LogWarning("Invalid email address for booking {BookingId}: {Email}", 
                                booking.Id, customerEmail);
                            emailStatus = "Địa chỉ email không hợp lệ";
                        }
                        else
                        {
                            // Send email synchronously to ensure it completes
                            await SendBookingConfirmationEmailAsync(booking);
                            emailSent = true;
                            _logger.LogInformation("Booking confirmation email with QR code sent successfully for booking {BookingCode} to {Email}", 
                                booking.BookingCode, customerEmail);
                        }
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send booking confirmation email for booking {BookingId}", booking.Id);
                        emailStatus = $"Gửi email thất bại: {emailEx.Message}";
                        
                        // Schedule retry in background
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(30000); // Wait 30 seconds before retry
                            try
                            {
                                _logger.LogInformation("Retrying email send for booking {BookingCode}", booking.BookingCode);
                                await SendBookingConfirmationEmailAsync(booking);
                                _logger.LogInformation("Email retry successful for booking {BookingCode}", booking.BookingCode);
                            }
                            catch (Exception retryEx)
                            {
                                _logger.LogError(retryEx, "Email retry also failed for booking {BookingId}", booking.Id);
                            }
                        });
                    }

                    // 7. ✅ Send notification to TourCompany về booking mới (in background)
                    if (booking.TourOperation?.TourDetails?.CreatedById != null)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var tourCompanyNotificationService = _serviceProvider.GetRequiredService<ITourCompanyNotificationService>();
                                var bookingDto = await MapToBookingDto(booking);
                                await tourCompanyNotificationService.NotifyNewBookingAsync(booking.TourOperation.TourDetails.CreatedById, bookingDto);
                                _logger.LogInformation("TourCompany notification sent for new booking {BookingCode}", booking.BookingCode);
                            }
                            catch (Exception notifEx)
                            {
                                _logger.LogError(notifEx, "Failed to send TourCompany notification for booking {BookingId}", booking.Id);
                            }
                        });
                    }

                    return new BaseResposeDto
                    {
                        StatusCode = 200,
                        Message = emailSent 
                            ? "Thanh toán thành công - Booking đã được xác nhận, QR code đã được tạo và email xác nhận đã được gửi"
                            : $"Thanh toán thành công - Booking đã được xác nhận, QR code đã được tạo. {emailStatus}",
                        success = true
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error processing payment success for booking with PayOS code: {PayOsOrderCode}", payOsOrderCode);
                    return new BaseResposeDto
                    {
                        StatusCode = 500,
                        Message = $"Lỗi khi xử lý thanh toán thành công: {ex.Message}"
                    };
                }
            });
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

        /// <summary>
        /// Manually resend QR ticket email for confirmed booking
        /// This can be called if the original email failed to send
        /// </summary>
        public async Task<BaseResposeDto> ResendQRTicketEmailAsync(Guid bookingId, Guid userId)
        {
            try
            {
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.Id == bookingId && b.UserId == userId && !b.IsDeleted)
                    .Include(b => b.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                    .Include(b => b.TourSlot)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking"
                    };
                }

                if (booking.Status != BookingStatus.Confirmed)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Chỉ có thể gửi lại email cho booking đã được xác nhận"
                    };
                }

                // Determine email address (priority: ContactEmail -> User.Email)
                var customerEmail = !string.IsNullOrWhiteSpace(booking.ContactEmail) 
                    ? booking.ContactEmail 
                    : booking.User?.Email;

                if (string.IsNullOrWhiteSpace(customerEmail))
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Không có địa chỉ email để gửi vé QR"
                    };
                }

                if (!IsValidEmail(customerEmail))
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Địa chỉ email không hợp lệ"
                    };
                }

                // Send the confirmation email
                await SendBookingConfirmationEmailAsync(booking);

                _logger.LogInformation("QR ticket email resent successfully for booking {BookingCode} to {Email}", 
                    booking.BookingCode, customerEmail);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Email vé QR đã được gửi lại thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending QR ticket email for booking {BookingId}", bookingId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi gửi lại email: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Xử lý callback thanh toán hủy
        /// </summary>
        public async Task<BaseResposeDto> HandlePaymentCancelAsync(string payOsOrderCode)
        {
            // Sử dụng execution strategy thay vì manual transaction để tránh conflict với retry strategy
            var strategy = _unitOfWork.GetExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
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
            });
        }

        /// <summary>
        /// Send booking confirmation email with QR code
        /// Enhanced: Includes tour details and QR code for customer
        /// FIXED: Better fallback handling when QR code generation fails
        /// </summary>
        private async Task SendBookingConfirmationEmailAsync(TourBooking booking)
        {
            try
            {
                // Determine email address (priority: ContactEmail -> User.Email)
                var customerEmail = !string.IsNullOrWhiteSpace(booking.ContactEmail) 
                    ? booking.ContactEmail 
                    : booking.User?.Email;

                if (string.IsNullOrWhiteSpace(customerEmail))
                {
                    _logger.LogWarning("No email address available for booking {BookingId} - ContactEmail: {ContactEmail}, UserEmail: {UserEmail}", 
                        booking.Id, booking.ContactEmail, booking.User?.Email);
                    return;
                }

                // Prepare email data
                var customerName = booking.ContactName ?? booking.User?.Name ?? "Valued Customer";
                var tourTitle = booking.TourOperation?.TourDetails?.Title ?? "Tour Experience";
                var tourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? VietnamTimeZoneUtility.GetVietnamNow();

                byte[] qrCodeImage;
                try
                {
                    // Try to generate QR code image using the QR service
                    _logger.LogInformation("Generating QR code for booking {BookingCode}", booking.BookingCode);
                    qrCodeImage = await _qrCodeService.GenerateQRCodeImageAsync(booking, 300);
                    _logger.LogInformation("QR code generated successfully for booking {BookingCode} - {ByteCount} bytes", 
                        booking.BookingCode, qrCodeImage.Length);
                }
                catch (Exception qrEx)
                {
                    _logger.LogError(qrEx, "Failed to generate QR code for booking {BookingId}, proceeding with email without QR", booking.Id);
                    
                    // Create a simple placeholder image or send email without QR
                    try
                    {
                        // Generate a minimal QR code with just booking code as fallback
                        var fallbackData = $"Booking: {booking.BookingCode}";
                        qrCodeImage = await _qrCodeService.GenerateQRCodeImageFromDataAsync(fallbackData, 300);
                        _logger.LogInformation("Generated fallback QR code for booking {BookingCode}", booking.BookingCode);
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Fallback QR code generation also failed for booking {BookingId}", booking.Id);
                        
                        // If all QR generation methods fail, send email without QR code
                        await SendBookingConfirmationEmailWithoutQRAsync(booking, customerEmail, customerName, tourTitle, tourDate);
                        return;
                    }
                }

                // Send confirmation email with QR code
                await _emailSender.SendTourBookingConfirmationAsync(
                    toEmail: customerEmail,
                    customerName: customerName,
                    bookingCode: booking.BookingCode,
                    tourTitle: tourTitle,
                    tourDate: tourDate,
                    numberOfGuests: booking.NumberOfGuests,
                    totalPrice: booking.TotalPrice,
                    contactPhone: booking.ContactPhone ?? booking.User?.PhoneNumber ?? "N/A",
                    qrCodeImage: qrCodeImage
                );

                _logger.LogInformation("Booking confirmation email with QR code sent successfully to {Email} for booking {BookingId}", 
                    customerEmail, booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send booking confirmation email for booking {BookingId}", booking.Id);
                
                // Try to send email without QR as last resort
                try
                {
                    var customerEmail = !string.IsNullOrWhiteSpace(booking.ContactEmail) 
                        ? booking.ContactEmail 
                        : booking.User?.Email;
                    
                    if (!string.IsNullOrWhiteSpace(customerEmail))
                    {
                        var customerName = booking.ContactName ?? booking.User?.Name ?? "Valued Customer";
                        var tourTitle = booking.TourOperation?.TourDetails?.Title ?? "Tour Experience";
                        var tourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? VietnamTimeZoneUtility.GetVietnamNow();
                        
                        await SendBookingConfirmationEmailWithoutQRAsync(booking, customerEmail, customerName, tourTitle, tourDate);
                        _logger.LogInformation("Sent fallback booking confirmation email without QR for booking {BookingId}", booking.Id);
                    }
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Even fallback email sending failed for booking {BookingId}", booking.Id);
                }
                
                throw; // Re-throw original exception for caller to handle
            }
        }

        /// <summary>
        /// Send booking confirmation email without QR code as fallback
        /// Used when QR code generation fails
        /// </summary>
        private async Task SendBookingConfirmationEmailWithoutQRAsync(
            TourBooking booking, 
            string customerEmail, 
            string customerName, 
            string tourTitle, 
            DateTime tourDate)
        {
            try
            {
                var subject = "Tour Booking Confirmed - Important Booking Information";
                var htmlBody = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
                    <div style=""text-align: center; margin-bottom: 30px;"">
                        <h1 style=""color: #2c3e50; margin-bottom: 10px;"">🎉 Booking Confirmed!</h1>
                        <p style=""color: #7f8c8d; font-size: 16px;"">Thank you for choosing Tay Ninh Tour</p>
                    </div>

                    <div style=""background-color: #d4edda; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #28a745;"">
                        <h2 style=""color: #155724; margin-top: 0; text-align: center;"">Your Tour Booking Details</h2>

                        <table style=""width: 100%; border-collapse: collapse; margin-top: 15px;"">
                            <tr>
                                <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Booking Code:</td>
                                <td style=""padding: 8px 0; color: #155724;"">{booking.BookingCode}</td>
                            </tr>
                            <tr>
                                <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Tour:</td>
                                <td style=""padding: 8px 0; color: #155724;"">{tourTitle}</td>
                            </tr>
                            <tr>
                                <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Date:</td>
                                <td style=""padding: 8px 0; color: #155724;"">{tourDate:dd/MM/yyyy}</td>
                            </tr>
                            <tr>
                                <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Number of Guests:</td>
                                <td style=""padding: 8px 0; color: #155724;"">{booking.NumberOfGuests}</td>
                            </tr>
                            <tr>
                                <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Total Price:</td>
                                <td style=""padding: 8px 0; color: #155724; font-weight: bold;"">{booking.TotalPrice:N0} VND</td>
                            </tr>
                            <tr>
                                <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Contact Phone:</td>
                                <td style=""padding: 8px 0; color: #155724;"">{booking.ContactPhone ?? booking.User?.PhoneNumber ?? "N/A"}</td>
                            </tr>
                        </table>
                    </div>

                    <div style=""background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;"">
                        <h4 style=""color: #856404; margin-top: 0;"">📋 Important Information:</h4>
                        <ul style=""color: #856404; margin: 10px 0; padding-left: 20px;"">
                            <li>Please arrive 15 minutes before the tour start time</li>
                            <li>Bring a valid ID for verification</li>
                            <li>Present your booking code <strong>{booking.BookingCode}</strong> to the tour guide</li>
                            <li>Contact us if you need to make any changes to your booking</li>
                        </ul>
                    </div>

                    <div style=""background-color: #f8d7da; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #dc3545;"">
                        <h4 style=""color: #721c24; margin-top: 0;"">⚠️ Note about QR Code:</h4>
                        <p style=""color: #721c24; margin: 0;"">
                            Due to technical issues, your QR code ticket could not be generated at this time. 
                            Please save this email and present your <strong>Booking Code: {booking.BookingCode}</strong> to the tour guide instead.
                            We apologize for any inconvenience.
                        </p>
                    </div>

                    <div style=""text-align: center; margin: 30px 0; padding: 20px; background-color: #f8f9fa; border-radius: 8px;"">
                        <h4 style=""color: #2c3e50; margin-bottom: 15px;"">Need Help?</h4>
                        <p style=""color: #6c757d; margin: 5px 0;"">📞 Phone: +84 123 456 789</p>
                        <p style=""color: #6c757d; margin: 5px 0;"">📧 Email: support@tayninhtravel.com</p>
                        <p style=""color: #6c757d; margin: 5px 0;"">🌐 Website: www.tayninhtravel.com</p>
                    </div>

                    <div style=""text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6;"">
                        <p style=""color: #6c757d; font-size: 14px;"">We look forward to providing you with an amazing tour experience!</p>
                        <br/>
                        <p style=""color: #2c3e50; font-weight: bold;"">Best regards,</p>
                        <p style=""color: #2c3e50; font-weight: bold;"">The Tay Ninh Tour Team</p>
                    </div>
                </div>";

                await _emailSender.SendEmailAsync(customerEmail, customerName, subject, htmlBody);
                
                _logger.LogInformation("Fallback booking confirmation email (without QR) sent to {Email} for booking {BookingId}", 
                    customerEmail, booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send fallback booking confirmation email for booking {BookingId}", booking.Id);
                throw;
            }
        }

        /// <summary>
        /// Map TourBooking entity to TourBookingDto for notifications
        /// </summary>
        private async Task<TourBookingDto> MapToBookingDto(TourBooking booking)
        {
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
                    TourStartDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue),
                    GuideId = booking.TourOperation.TourGuide?.Id.ToString(),
                    GuideName = booking.TourOperation.TourGuide?.FullName,
                    GuidePhone = booking.TourOperation.TourGuide?.PhoneNumber
                } : null,
                User = booking.User != null ? new DTOs.Response.TourBooking.UserSummaryDto
                {
                    Id = booking.User.Id,
                    Name = booking.User.Name,
                    Email = booking.User.Email,
                    PhoneNumber = booking.User.PhoneNumber
                } : null!
            };
        }
    }
}
