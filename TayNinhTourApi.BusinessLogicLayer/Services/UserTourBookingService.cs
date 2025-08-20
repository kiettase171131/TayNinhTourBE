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
                .Where(td => td.TourOperation != null && td.TourOperation.IsActive);
                // ✅ REMOVED: .Where(td => td.TourOperation!.CurrentBookings < td.TourOperation.MaxGuests)
                // Vì mỗi TourSlot có capacity riêng, không cần check tổng capacity của TourOperation

            // Filter by date range - check if any slots are available in the date range
            if (fromDate.HasValue || toDate.HasValue)
            {
                query = query.Where(td => td.AssignedSlots.Any(slot =>
                    slot.IsActive && 
                    slot.Status == TourSlotStatus.Available &&
                    slot.CurrentBookings < slot.MaxGuests && // ✅ Check slot-specific capacity
                    (!fromDate.HasValue || slot.TourDate >= DateOnly.FromDateTime(fromDate.Value)) &&
                    (!toDate.HasValue || slot.TourDate <= DateOnly.FromDateTime(toDate.Value))));
            }
            else
            {
                // ✅ NEW: Only show tours that have at least one available slot
                query = query.Where(td => td.AssignedSlots.Any(slot =>
                    slot.IsActive && 
                    slot.Status == TourSlotStatus.Available &&
                    slot.CurrentBookings < slot.MaxGuests)); // Check slot-specific capacity
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

                // ✅ FIXED: Calculate available slots capacity correctly
                var availableSlots = td.AssignedSlots.Where(s => 
                    s.IsActive && 
                    s.Status == TourSlotStatus.Available &&
                    s.CurrentBookings < s.MaxGuests).ToList();
                
                var totalAvailableSpots = availableSlots.Sum(s => s.MaxGuests - s.CurrentBookings);
                var totalMaxGuests = availableSlots.Sum(s => s.MaxGuests);
                var totalCurrentBookings = availableSlots.Sum(s => s.CurrentBookings);

                tours.Add(new AvailableTourDto
                {
                    TourDetailsId = td.Id,
                    TourOperationId = td.TourOperation.Id,
                    Title = td.Title,
                    Description = td.Description,
                    ImageUrls = td.ImageUrls,
                    Price = td.TourOperation.Price,
                    MaxGuests = td.TourOperation.MaxGuests, // Keep for reference, but use slot data for availability
                    CurrentBookings = td.TourOperation.CurrentBookings, // Keep for reference
                    TourStartDate = tourStartDate != DateTime.MaxValue ? tourStartDate : null,
                    StartLocation = td.TourTemplate.StartLocation,
                    EndLocation = td.TourTemplate.EndLocation,
                    CreatedAt = td.CreatedAt,

                    // ✅ NEW: Add slot-specific availability info
                    AvailableSlots = availableSlots.Count,
                    TotalSlotsCapacity = totalMaxGuests,
                    TotalAvailableSpots = totalAvailableSpots,

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

            // ✅ FIXED: Check availability across all slots, not just TourOperation capacity
            var availableSlots = tourOperation.TourDetails.AssignedSlots.Where(s => 
                s.IsActive && 
                s.Status == TourSlotStatus.Available &&
                s.CurrentBookings < s.MaxGuests).ToList();
            
            var totalAvailableSpots = availableSlots.Sum(s => s.MaxGuests - s.CurrentBookings);
            var isAvailable = totalAvailableSpots >= request.NumberOfGuests && 
                             tourStartDate > VietnamTimeZoneUtility.GetVietnamNow();

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
                AvailableSpots = totalAvailableSpots, // ✅ FIXED: Use slot-based availability
                UnavailableReason = !isAvailable ?
                    (totalAvailableSpots < request.NumberOfGuests ? "Không đủ chỗ trống trong các slot" : "Tour đã khởi hành") : null
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

                    // ✅ FIXED: Use TourSlot capacity correctly - each slot has independent capacity
                    var slotAvailableSpots = tourSlot.MaxGuests - tourSlot.CurrentBookings;

                    if (slotAvailableSpots < request.NumberOfGuests)
                    {
                        _logger.LogWarning("Insufficient capacity in slot. TourSlotId: {TourSlotId}, Available: {Available}, Requested: {Requested}, SlotMaxGuests: {SlotMaxGuests}, SlotCurrentBookings: {SlotCurrentBookings}",
                            request.TourSlotId, slotAvailableSpots, request.NumberOfGuests, tourSlot.MaxGuests, tourSlot.CurrentBookings);
                        return new CreateBookingResultDto
                        {
                            Success = false,
                            Message = $"Slot này chỉ còn {slotAvailableSpots} chỗ trống, không đủ cho {request.NumberOfGuests} khách"
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
                        BookingType = request.BookingType ?? "Individual", // New field for booking type
                        GroupName = request.GroupName, // New field for group name
                        GroupDescription = request.GroupDescription, // New field for group description
                        ContactName = request.BookingType == "GroupRepresentative" && request.GroupRepresentative != null
                            ? request.GroupRepresentative.GuestName
                            : request.Guests?.FirstOrDefault()?.GuestName ?? "Guest",
                        ContactPhone = request.ContactPhone,
                        ContactEmail = request.BookingType == "GroupRepresentative" && request.GroupRepresentative != null
                            ? request.GroupRepresentative.GuestEmail
                            : request.Guests?.FirstOrDefault()?.GuestEmail ?? "",
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

                    // 10. Create TourBookingGuest records based on booking type
                    if (request.BookingType == "GroupRepresentative")
                    {
                        // For group bookings, create one record for the representative
                        if (request.GroupRepresentative != null)
                        {
                            var representativeGuest = new TourBookingGuest
                            {
                                Id = Guid.NewGuid(),
                                TourBookingId = booking.Id,
                                GuestName = request.GroupRepresentative.GuestName.Trim(),
                                GuestEmail = request.GroupRepresentative.GuestEmail.Trim().ToLowerInvariant(),
                                GuestPhone = string.IsNullOrWhiteSpace(request.GroupRepresentative.GuestPhone)
                                    ? null : request.GroupRepresentative.GuestPhone.Trim(),
                                IsGroupRepresentative = true, // Mark as group representative
                                IsCheckedIn = false,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedAt = bookingDate,
                                CreatedById = userId
                            };

                            await _unitOfWork.TourBookingGuestRepository.AddAsync(representativeGuest);

                            // Create placeholder records for other guests in the group
                            for (int i = 1; i < request.NumberOfGuests; i++)
                            {
                                var placeholderGuest = new TourBookingGuest
                                {
                                    Id = Guid.NewGuid(),
                                    TourBookingId = booking.Id,
                                    GuestName = $"Khách {i + 1} - {request.GroupName ?? "Nhóm"}",
                                    GuestEmail = $"guest{i + 1}_{booking.BookingCode}@placeholder.com",
                                    GuestPhone = null,
                                    IsGroupRepresentative = false,
                                    IsCheckedIn = false,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedAt = bookingDate,
                                    CreatedById = userId
                                };

                                await _unitOfWork.TourBookingGuestRepository.AddAsync(placeholderGuest);
                            }
                        }

                        _logger.LogInformation("Created group booking with representative for {GuestCount} guests, booking {BookingCode}",
                            request.NumberOfGuests, bookingCode);
                    }
                    else
                    {
                        // For individual bookings, create records for each guest
                        if (request.Guests != null && request.Guests.Any())
                        {
                            foreach (var guestInfo in request.Guests)
                            {
                                var guest = new TourBookingGuest
                                {
                                    Id = Guid.NewGuid(),
                                    TourBookingId = booking.Id,
                                    GuestName = guestInfo.GuestName.Trim(),
                                    GuestEmail = guestInfo.GuestEmail.Trim().ToLowerInvariant(),
                                    GuestPhone = string.IsNullOrWhiteSpace(guestInfo.GuestPhone) ? null : guestInfo.GuestPhone.Trim(),
                                    IsGroupRepresentative = false,
                                    IsCheckedIn = false,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedAt = bookingDate,
                                    CreatedById = userId
                                };

                                await _unitOfWork.TourBookingGuestRepository.AddAsync(guest);
                            }

                            _logger.LogInformation("Created {GuestCount} individual guest records for booking {BookingCode}",
                                request.Guests.Count, bookingCode);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();

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
                                Message = "Não foi possível criar o link de pagamento. Por favor, tente novamente."
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
                        PricingType = pricingType, // Fix: Use pricingType variable instead of pricingInfo.PricingType
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
        /// Lấy danh sách bookings của user với filter
        /// </summary>
        public async Task<Common.PagedResult<TourBookingDto>> GetUserBookingsAsync(
            Guid userId,
            int pageIndex = 1,
            int pageSize = 10,
            BookingStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            string? bookingCode = null)
        {
            // Convert 1-based pageIndex to 0-based for repository
            var repoPageIndex = Math.Max(0, pageIndex - 1);

            var (bookingEntities, totalCount) = await _unitOfWork.TourBookingRepository.GetUserBookingsWithFilterAsync(
                userId, repoPageIndex, pageSize, status, startDate, endDate, searchTerm, bookingCode);

            // Map to DTOs including guests using dedicated method
            var bookings = bookingEntities.Select(MapToBookingDtoWithGuests).ToList();

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
                    _logger.LogError(ex, "Error canceling tour booking. UserId: {UserId}, BookingId: {BookingId}",
                        userId, bookingId);
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
                .Include(b => b.Guests.Where(g => !g.IsDeleted)) // ✅ NEW: Include guests
                .Include(b => b.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                .Include(b => b.TourOperation)
                    .ThenInclude(to => to.TourGuide)
                .Include(b => b.User)
                .FirstOrDefaultAsync();

            if (booking == null)
                return null;

            // ✅ NEW: Use method with guests mapping
            return MapToBookingDtoWithGuests(booking);
        }

        /// <summary>
        /// Xử lý callback thanh toán thành công
        /// Enhanced logic: Xử lý slot, revenue hold, QR code generation và gửi email
        /// FIXED: Proper MySQL execution strategy usage
        /// </summary>
        public async Task<BaseResposeDto> HandlePaymentSuccessAsync(string payOsOrderCode)
        {
            try
            {
                _logger.LogInformation("Processing payment success for PayOS order code: {PayOsOrderCode}", payOsOrderCode);

                // Step 1: Find booking with minimal query to avoid tracking issues
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.PayOsOrderCode == payOsOrderCode && !b.IsDeleted)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for PayOS order code: {PayOsOrderCode}", payOsOrderCode);
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking với mã thanh toán này"
                    };
                }

                if (booking.Status == BookingStatus.Confirmed)
                {
                    _logger.LogInformation("Booking {BookingCode} already confirmed", booking.BookingCode);
                    return new BaseResposeDto
                    {
                        StatusCode = 200,
                        Message = "Booking đã được xác nhận trước đó",
                        success = true
                    };
                }

                _logger.LogInformation("Processing payment success for booking {BookingCode}, TotalPrice: {TotalPrice}, OriginalPrice: {OriginalPrice}, Discount: {DiscountPercent}%",
                    booking.BookingCode, booking.TotalPrice, booking.OriginalPrice, booking.DiscountPercent);

                // Step 2: Use execution strategy to handle the transaction properly with MySQL
                var strategy = _unitOfWork.GetExecutionStrategy();
                string emailStatus = "";

                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Update booking fields
                        booking.Status = BookingStatus.Confirmed;
                        booking.ConfirmedDate = DateTime.UtcNow;
                        booking.UpdatedAt = DateTime.UtcNow;
                        booking.RevenueHold = booking.TotalPrice;

                        // Generate main booking QR code
                        booking.QRCodeData = (_qrCodeService as QRCodeService)?.GenerateGroupQRCodeData(booking)
                            ?? _qrCodeService.GenerateQRCodeData(booking);

                        // Save booking first
                        await _unitOfWork.TourBookingRepository.UpdateAsync(booking);

                        // Handle individual guest QR codes if needed
                        if (booking.BookingType != "GroupRepresentative")
                        {
                            var guests = await _unitOfWork.TourBookingGuestRepository.GetGuestsByBookingIdAsync(booking.Id);
                            foreach (var guest in guests)
                            {
                                guest.QRCodeData = _qrCodeService.GenerateGuestQRCodeData(guest, booking);
                                await _unitOfWork.TourBookingGuestRepository.UpdateAsync(guest);
                            }
                            _logger.LogInformation("Generated INDIVIDUAL QR codes for {GuestCount} guests in booking {BookingCode}",
                                guests.Count, booking.BookingCode);
                        }
                        else
                        {
                            _logger.LogInformation("Generated GROUP QR code for booking {BookingCode} with {GuestCount} guests",
                                booking.BookingCode, booking.NumberOfGuests);
                        }

                        // Save all changes
                        await _unitOfWork.SaveChangesAsync();

                        // ✅ FIXED: Check TourOperation capacity constraint before updating
                        if (booking.TourOperationId != Guid.Empty)
                        {
                            var tourOperation = await _unitOfWork.TourOperationRepository.GetByIdAsync(booking.TourOperationId);
                            if (tourOperation != null)
                            {
                                var newCurrentBookings = tourOperation.CurrentBookings + booking.NumberOfGuests;

                                // ✅ Check constraint before update to avoid database constraint violation
                                if (newCurrentBookings <= tourOperation.MaxGuests)
                                {
                                    tourOperation.CurrentBookings = newCurrentBookings;
                                    await _unitOfWork.TourOperationRepository.UpdateAsync(tourOperation);
                                    await _unitOfWork.SaveChangesAsync();
                                    _logger.LogInformation("Updated TourOperation.CurrentBookings (+{Guests}) for booking {BookingCode} - New total: {NewTotal}/{MaxGuests}",
                                        booking.NumberOfGuests, booking.BookingCode, newCurrentBookings, tourOperation.MaxGuests);
                                }
                                else
                                {
                                    _logger.LogWarning("Skipping TourOperation.CurrentBookings update for booking {BookingCode} - Would exceed MaxGuests ({NewTotal} > {MaxGuests}). Using slot-specific capacity only.",
                                        booking.BookingCode, newCurrentBookings, tourOperation.MaxGuests);

                                    // ✅ Note: TourSlot capacity is still updated correctly above
                                    // This is just to avoid breaking the constraint while maintaining slot independence
                                }
                            }
                        }

                        // Update TourSlot capacity if applicable
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
                                _logger.LogWarning("Failed to update TourSlot capacity for booking {BookingCode}. Continuing...",
                                    booking.BookingCode);
                            }
                        }

                        await transaction.CommitAsync();
                        _logger.LogInformation("Payment success transaction completed for booking {BookingCode}. Revenue hold: {RevenueHold}",
                            booking.BookingCode, booking.RevenueHold);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Transaction failed during payment success processing for booking {BookingCode}", booking.BookingCode);
                        throw;
                    }
                });

                // Step 3: Send confirmation emails (outside transaction to avoid holding locks)
                try
                {
                    emailStatus = await SendConfirmationEmailsAsync(booking);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send confirmation emails for booking {BookingCode}", booking.BookingCode);
                    emailStatus = "Email sending failed but payment was processed successfully";
                }

                // Step 4: Send notification to TourCompany (background task)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Get tour company user ID with fresh query
                        var tourOperation = await _unitOfWork.TourOperationRepository.GetQueryable()
                            .Where(to => to.Id == booking.TourOperationId)
                            .Include(to => to.TourDetails)
                                .ThenInclude(td => td.CreatedBy)
                            .FirstOrDefaultAsync();

                        if (tourOperation?.TourDetails?.CreatedById != null)
                        {
                            var tourCompanyNotificationService = _serviceProvider.GetRequiredService<ITourCompanyNotificationService>();
                            var bookingDto = await MapToBookingDto(booking);
                            await tourCompanyNotificationService.NotifyNewBookingAsync(tourOperation.TourDetails.CreatedById, bookingDto);
                            _logger.LogInformation("TourCompany notification sent for new booking {BookingCode}", booking.BookingCode);
                        }
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "Failed to send TourCompany notification for booking {BookingId}", booking.Id);
                    }
                });

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Thanh toán thành công - Booking đã được xác nhận, QR code đã được tạo. {emailStatus}",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment success for PayOS order code: {PayOsOrderCode}. Error: {ErrorMessage}", payOsOrderCode, ex.Message);

                // Try simple fallback method if main method fails due to execution strategy issues
                if (ex.Message.Contains("execution strategy") || ex.Message.Contains("MySqlRetryingExecutionStrategy"))
                {
                    _logger.LogInformation("Attempting simple payment success method for {PayOsOrderCode} due to execution strategy conflict", payOsOrderCode);
                    return await HandlePaymentSuccessSimpleAsync(payOsOrderCode);
                }

                // Try standard fallback method for other issues
                try
                {
                    _logger.LogInformation("Attempting standard fallback payment success method for {PayOsOrderCode}", payOsOrderCode);
                    return await HandlePaymentSuccessFallback(payOsOrderCode);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "All fallback methods failed for {PayOsOrderCode}", payOsOrderCode);
                    return new BaseResposeDto
                    {
                        StatusCode = 500,
                        Message = $"Lỗi khi xử lý thanh toán thành công: {ex.Message}"
                    };
                }
            }
        }

        /// <summary>
        /// Fallback payment success handler using simpler operations
        /// Used when the main method encounters database update exceptions
        /// FIXED: Avoid execution strategy conflicts
        /// </summary>
        private async Task<BaseResposeDto> HandlePaymentSuccessFallback(string payOsOrderCode)
        {
            try
            {
                _logger.LogInformation("Using fallback payment success handler for PayOS order code: {PayOsOrderCode}", payOsOrderCode);

                // Get fresh booking entity
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.PayOsOrderCode == payOsOrderCode && !b.IsDeleted)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Booking not found in fallback handler"
                    };
                }

                // Simple status update without complex entity operations and without execution strategy
                booking.Status = BookingStatus.Confirmed;
                booking.ConfirmedDate = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.RevenueHold = booking.TotalPrice;
                booking.QRCodeData = _qrCodeService.GenerateQRCodeData(booking);

                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Fallback payment success completed for booking {BookingCode}", booking.BookingCode);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Thanh toán thành công (fallback method) - Booking đã được xác nhận",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback payment success handler also failed for PayOS order code: {PayOsOrderCode}", payOsOrderCode);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi nghiêm trọng khi xử lý thanh toán: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Xử lý callback thanh toán hủy
        /// FIXED: Proper MySQL execution strategy usage
        /// </summary>
        public async Task<BaseResposeDto> HandlePaymentCancelAsync(string payOsOrderCode)
        {
            try
            {
                _logger.LogInformation("Processing payment cancel for PayOS order code: {PayOsOrderCode}", payOsOrderCode);

                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.PayOsOrderCode == payOsOrderCode && !b.IsDeleted)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for PayOS order code: {PayOsOrderCode}", payOsOrderCode);
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking với mã thanh toán này"
                    };
                }

                if (booking.Status != BookingStatus.Pending)
                {
                    _logger.LogInformation("Booking {BookingCode} is not in pending status: {Status}",
                        booking.BookingCode, booking.Status);
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Booking không ở trạng thái chờ thanh toán"
                    };
                }

                // Use execution strategy for MySQL transaction handling
                var strategy = _unitOfWork.GetExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Cancel booking and release capacity
                        booking.Status = BookingStatus.CancelledByCustomer;
                        booking.CancelledDate = DateTime.UtcNow;
                        booking.CancellationReason = "Hủy thanh toán";
                        booking.UpdatedAt = DateTime.UtcNow;
                        booking.ReservedUntil = null;

                        await _unitOfWork.TourBookingRepository.UpdateAsync(booking);

                        // Release capacity from TourSlot
                        if (booking.TourSlotId.HasValue)
                        {
                            var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                            await tourSlotService.ReleaseSlotCapacityAsync(booking.TourSlotId.Value, booking.NumberOfGuests);
                        }

                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Payment cancel processed successfully for booking {BookingCode}", booking.BookingCode);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Transaction failed during payment cancel processing for booking {BookingCode}", booking.BookingCode);
                        throw;
                    }
                });

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã hủy booking do không thanh toán",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment cancel for PayOS order code: {PayOsOrderCode}", payOsOrderCode);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi xử lý hủy thanh toán: {ex.Message}"
                };
            }
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
                var emailStatus = await SendConfirmationEmailsAsync(booking);

                _logger.LogInformation("QR ticket email resent successfully for booking {BookingCode} to {Email}",
                    booking.BookingCode, customerEmail);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Email vé QR đã được gửi lại thành công. {emailStatus}",
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
        /// Generate QR codes for booking based on booking type
        /// Separated into its own method for better maintainability
        /// FIXED: Remove entity updates to avoid tracking conflicts
        /// </summary>
        private async Task GenerateQRCodesForBooking(TourBooking booking)
        {
            try
            {
                var guests = await _unitOfWork.TourBookingGuestRepository.GetGuestsByBookingIdAsync(booking.Id);

                if (booking.BookingType == "GroupRepresentative")
                {
                    // For GroupRepresentative: Generate ONE group QR code for the entire booking
                    booking.QRCodeData = (_qrCodeService as QRCodeService)?.GenerateGroupQRCodeData(booking)
                        ?? _qrCodeService.GenerateQRCodeData(booking);

                    _logger.LogInformation("Generated GROUP QR code for booking {BookingCode} with {GuestCount} guests",
                        booking.BookingCode, booking.NumberOfGuests);
                }
                else
                {
                    // For Individual: Generate personal QR codes for each guest
                    foreach (var guest in guests)
                    {
                        guest.QRCodeData = _qrCodeService.GenerateGuestQRCodeData(guest, booking);
                        // REMOVED: await _unitOfWork.TourBookingGuestRepository.UpdateAsync(guest);
                        // These will be saved when the main transaction commits
                    }

                    // Also keep booking QR for backward compatibility
                    booking.QRCodeData = _qrCodeService.GenerateQRCodeData(booking);

                    _logger.LogInformation("Generated INDIVIDUAL QR codes for {GuestCount} guests in booking {BookingCode}",
                        guests.Count, booking.BookingCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR codes for booking {BookingId}", booking.Id);
                // Don't throw - QR code generation failure shouldn't fail the payment processing
                booking.QRCodeData = $"FALLBACK-{booking.BookingCode}"; // Fallback QR data
            }
        }

        /// <summary>
        /// Send confirmation emails based on booking type
        /// Separated into its own method for better maintainability
        /// </summary>
        private async Task<string> SendConfirmationEmailsAsync(TourBooking booking)
        {
            try
            {
                // Get tour details for email
                var tourOperation = await _unitOfWork.TourOperationRepository.GetQueryable()
                    .Where(to => to.Id == booking.TourOperationId)
                    .Include(to => to.TourDetails)
                    .FirstOrDefaultAsync();

                var tourSlot = booking.TourSlotId.HasValue
                    ? await _unitOfWork.TourSlotRepository.GetByIdAsync(booking.TourSlotId.Value)
                    : null;

                var tourTitle = tourOperation?.TourDetails?.Title ?? "Tour Experience";
                var tourDate = tourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? VietnamTimeZoneUtility.GetVietnamNow();

                if (booking.BookingType == "GroupRepresentative")
                {
                    // For group booking: Send ONE email to representative with GROUP QR
                    var representativeEmail = booking.ContactEmail;
                    var representativeName = booking.ContactName ?? "Customer";

                    if (!string.IsNullOrWhiteSpace(representativeEmail) && IsValidEmail(representativeEmail))
                    {
                        // Generate group QR code image
                        var qrCodeImage = (_qrCodeService as QRCodeService)?.GenerateGroupQRCodeImageAsync(booking, 300).Result
                            ?? await _qrCodeService.GenerateQRCodeImageAsync(booking, 300);

                        // Send group booking confirmation email
                        await _emailSender.SendGroupBookingConfirmationAsync(
                            booking, representativeName, representativeEmail, tourTitle, tourDate, qrCodeImage);

                        _logger.LogInformation("GROUP booking confirmation email sent to {Email} for booking {BookingCode} with {GuestCount} guests",
                            representativeEmail, booking.BookingCode, booking.NumberOfGuests);
                        return "Email xác nhận nhóm đã được gửi cho người đại diện";
                    }
                    else
                    {
                        _logger.LogWarning("Invalid email for group representative in booking {BookingId}", booking.Id);
                        return "Không thể gửi email - địa chỉ email người đại diện không hợp lệ";
                    }
                }
                else
                {
                    // For individual booking: Send personal emails to each guest
                    var guestsWithQR = await _unitOfWork.TourBookingGuestRepository.GetGuestsByBookingIdAsync(booking.Id);

                    if (!guestsWithQR.Any())
                    {
                        _logger.LogWarning("No guests found for booking {BookingId} - cannot send individual emails", booking.Id);
                        return "Không tìm thấy thông tin khách hàng để gửi email";
                    }

                    await SendIndividualGuestEmailsAsync(booking, guestsWithQR, tourTitle, tourDate);
                    _logger.LogInformation("INDIVIDUAL confirmation emails sent successfully for {GuestCount} guests in booking {BookingCode}",
                        guestsWithQR.Count, booking.BookingCode);
                    return "Email xác nhận cá nhân đã được gửi cho từng khách";
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send booking confirmation email for booking {BookingId}", booking.Id);
                return $"Gửi email thất bại: {emailEx.Message}";
            }
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
        /// Send individual emails to each guest with their personal QR codes
        /// NEW: For individual guest QR system
        /// </summary>
        private async Task SendIndividualGuestEmailsAsync(TourBooking booking, List<TourBookingGuest> guests, string tourTitle, DateTime tourDate)
        {
            var emailTasks = guests.Select(async guest =>
            {
                try
                {
                    // Validate guest email
                    if (string.IsNullOrWhiteSpace(guest.GuestEmail) || !IsValidEmail(guest.GuestEmail))
                    {
                        _logger.LogWarning("Invalid email for guest {GuestId} ({GuestName}): {Email}",
                            guest.Id, guest.GuestName, guest.GuestEmail);
                        return;
                    }

                    // Generate individual QR code image
                    var qrCodeImage = await _qrCodeService.GenerateGuestQRCodeImageAsync(guest, booking, 300);

                    // Send individual email
                    await _emailSender.SendIndividualGuestBookingConfirmationAsync(
                        guest, booking, tourTitle, tourDate, qrCodeImage);

                    _logger.LogInformation("Sent individual email to guest {GuestName} ({GuestEmail}) for booking {BookingCode}",
                        guest.GuestName, guest.GuestEmail, booking.BookingCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to guest {GuestName} ({GuestEmail}) for booking {BookingCode}",
                        guest.GuestName, guest.GuestEmail, booking.BookingCode);
                }
            });

            // Wait for all emails to complete
            await Task.WhenAll(emailTasks);
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
                    // ✅ FIXED: Get tour date from TourSlot if booking has TourSlot assigned
                    TourStartDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ??
                        (booking.TourOperation.TourDetails?.AssignedSlots?.Any() == true ?
                            booking.TourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : null),
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

        /// <summary>
        /// Map TourBooking entity to TourBookingDto including guests
        /// NEW: For individual guest QR system
        /// </summary>
        private TourBookingDto MapToBookingDtoWithGuests(TourBooking booking)
        {
            // Get company name from tour details
            var companyName = string.Empty;
            if (booking.TourOperation?.TourDetails?.CreatedBy?.TourCompany?.CompanyName != null)
            {
                companyName = booking.TourOperation.TourDetails.CreatedBy.TourCompany.CompanyName;
            }
            else if (booking.TourOperation?.TourDetails?.CreatedBy?.Name != null)
            {
                // Fallback to user name if company name is not available
                companyName = booking.TourOperation.TourDetails.CreatedBy.Name;
            }

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
                QRCodeData = booking.QRCodeData, // ✅ LEGACY: Keep for backward compatibility
                BookingDate = booking.BookingDate,
                ConfirmedDate = booking.ConfirmedDate,
                CancelledDate = booking.CancelledDate,
                CancellationReason = booking.CancellationReason,
                CustomerNotes = booking.CustomerNotes,
                ContactName = booking.ContactName,
                ContactPhone = booking.ContactPhone,
                ContactEmail = booking.ContactEmail,
                SpecialRequests = booking.CustomerNotes,
                BookingType = booking.BookingType,
                GroupName = booking.GroupName,
                GroupDescription = booking.GroupDescription,
                GroupQRCodeData = booking.GroupQRCodeData,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,

                // ✅ NEW: Set tour title, tour date, and company name
                TourTitle = booking.TourOperation?.TourDetails?.Title ?? string.Empty,
                TourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ??
                          (booking.TourOperation?.TourDetails?.AssignedSlots?.Any() == true ?
                              booking.TourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : null),
                CompanyName = companyName,

                // ✅ NEW: Include guests information
                Guests = booking.Guests?.Where(g => !g.IsDeleted).Select(g => new TourBookingGuestDto
                {
                    Id = g.Id,
                    TourBookingId = g.TourBookingId,
                    GuestName = g.GuestName,
                    GuestEmail = g.GuestEmail,
                    GuestPhone = g.GuestPhone,
                    IsCheckedIn = g.IsCheckedIn,
                    CheckInTime = g.CheckInTime,
                    CheckInNotes = g.CheckInNotes,
                    QRCodeData = g.QRCodeData,
                    CreatedAt = g.CreatedAt
                }).ToList() ?? new List<TourBookingGuestDto>(),

                TourOperation = booking.TourOperation != null ? new TourOperationSummaryDto
                {
                    Id = booking.TourOperation.Id,
                    TourDetailsId = booking.TourOperation.TourDetailsId,
                    TourTitle = booking.TourOperation.TourDetails?.Title ?? "",
                    Price = booking.TourOperation.Price,
                    MaxGuests = booking.TourOperation.MaxGuests,
                    CurrentBookings = booking.TourOperation.CurrentBookings,
                    // ✅ FIXED: Get tour date from TourSlot if booking has TourSlot assigned
                    TourStartDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ??
                        (booking.TourOperation.TourDetails?.AssignedSlots?.Any() == true ?
                            booking.TourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : null),
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

        /// <summary>
        /// Alternative payment success handler that avoids execution strategy completely
        /// Uses simple database operations without complex transactions
        /// </summary>
        public async Task<BaseResposeDto> HandlePaymentSuccessSimpleAsync(string payOsOrderCode)
        {
            try
            {
                _logger.LogInformation("Processing payment success (simple method) for PayOS order code: {PayOsOrderCode}", payOsOrderCode);

                // Step 1: Find and validate booking
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.PayOsOrderCode == payOsOrderCode && !b.IsDeleted)
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

                // Step 2: Update booking status (simple update, no transaction)
                booking.Status = BookingStatus.Confirmed;
                booking.ConfirmedDate = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.RevenueHold = booking.TotalPrice;
                booking.QRCodeData = _qrCodeService.GenerateQRCodeData(booking);

                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                // Step 3: Update TourOperation capacity (separate operation)
                try
                {
                    if (booking.TourOperationId != Guid.Empty)
                    {
                        var tourOperation = await _unitOfWork.TourOperationRepository.GetByIdAsync(booking.TourOperationId);
                        if (tourOperation != null)
                        {
                            tourOperation.CurrentBookings += booking.NumberOfGuests;
                            await _unitOfWork.TourOperationRepository.UpdateAsync(tourOperation);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update TourOperation capacity for booking {BookingCode}, but booking was confirmed", booking.BookingCode);
                }

                // Step 4: Update TourSlot capacity (separate operation)
                try
                {
                    if (booking.TourSlotId.HasValue)
                    {
                        var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                        await tourSlotService.ConfirmSlotCapacityAsync(booking.TourSlotId.Value, booking.NumberOfGuests);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update TourSlot capacity for booking {BookingCode}, but booking was confirmed", booking.BookingCode);
                }

                // Step 5: Handle guest QR codes (separate operation)
                try
                {
                    if (booking.BookingType != "GroupRepresentative")
                    {
                        var guests = await _unitOfWork.TourBookingGuestRepository.GetGuestsByBookingIdAsync(booking.Id);
                        foreach (var guest in guests)
                        {
                            guest.QRCodeData = _qrCodeService.GenerateGuestQRCodeData(guest, booking);
                            await _unitOfWork.TourBookingGuestRepository.UpdateAsync(guest);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate guest QR codes for booking {BookingCode}, but booking was confirmed", booking.BookingCode);
                }

                // Step 6: Send emails (background task)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendConfirmationEmailsAsync(booking);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send confirmation emails for booking {BookingCode}", booking.BookingCode);
                    }
                });

                _logger.LogInformation("Payment success processed successfully (simple method) for booking {BookingCode}", booking.BookingCode);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Thanh toán thành công - Booking đã được xác nhận",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in simple payment success handler for PayOS order code: {PayOsOrderCode}", payOsOrderCode);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi xử lý thanh toán: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Lấy tiến độ tour đang diễn ra cho user
        /// </summary>
        public async Task<UserTourProgressDto?> GetTourProgressAsync(Guid tourOperationId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting tour progress for operation {TourOperationId} and user {UserId}", tourOperationId, userId);

                // Lấy thông tin tour operation với timeline
                var tourOperation = await _unitOfWork.TourOperationRepository.GetQueryable()
                    .Where(to => to.Id == tourOperationId && to.IsActive && !to.IsDeleted)
                    .Include(to => to.TourDetails)
                        .ThenInclude(td => td.Timeline.Where(ti => ti.IsActive))
                    .Include(to => to.TourGuide)
                    .FirstOrDefaultAsync();

                if (tourOperation?.TourDetails == null)
                {
                    _logger.LogWarning("Tour operation {TourOperationId} not found", tourOperationId);
                    return null;
                }

                // Lấy TourSlots thông qua TourDetails
                var tourSlots = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Where(ts => ts.TourDetailsId == tourOperation.TourDetailsId && ts.IsActive)
                    .Include(ts => ts.TimelineProgress)
                    .ToListAsync();

                // Lấy thông tin timeline progress từ TourSlot
                var activeSlot = tourSlots.FirstOrDefault(ts => ts.IsActive);
                var timelineProgresses = activeSlot?.TimelineProgress?.ToList() ?? new List<TourSlotTimelineProgress>();

                // Tạo timeline items với progress status
                var timelineItems = tourOperation.TourDetails.Timeline
                    .OrderBy(ti => ti.SortOrder)
                    .Select(ti =>
                    {
                        var progress = timelineProgresses.FirstOrDefault(tp => tp.TimelineItemId == ti.Id);
                        return new TourTimelineProgressItemDto
                        {
                            Id = ti.Id,
                            CheckInTime = ti.CheckInTime.ToString(@"hh\:mm"),
                            Activity = ti.Activity,
                            SpecialtyShopId = ti.SpecialtyShopId,
                            SpecialtyShopName = ti.SpecialtyShop?.ShopName,
                            SortOrder = ti.SortOrder,
                            IsCompleted = progress?.IsCompleted ?? false,
                            CompletedAt = progress?.CompletedAt,
                            IsActive = progress?.IsCompleted == false &&
                                      (timelineProgresses.Where(tp => tp.TimelineItem.SortOrder < ti.SortOrder).All(tp => tp.IsCompleted) ||
                                       ti.SortOrder == 1)
                        };
                    }).ToList();

                // Tính toán thống kê
                var totalItems = timelineItems.Count;
                var completedItems = timelineItems.Count(ti => ti.IsCompleted);
                var progressPercentage = totalItems > 0 ? (double)completedItems / totalItems * 100 : 0;

                // Lấy thông tin guests và check-in status
                var bookings = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(tb => tb.TourOperationId == tourOperationId &&
                                tb.Status == BookingStatus.Confirmed)
                    .Include(tb => tb.Guests)
                    .ToListAsync();

                var totalGuests = bookings.Sum(b => b.NumberOfGuests);
                var checkedInGuests = bookings.SelectMany(b => b.Guests).Count(g => g.IsCheckedIn);
                var checkInPercentage = totalGuests > 0 ? (double)checkedInGuests / totalGuests * 100 : 0;

                // Xác định current status
                string currentStatus;
                if (completedItems == 0)
                    currentStatus = "Chưa bắt đầu";
                else if (completedItems == totalItems)
                    currentStatus = "Đã hoàn thành";
                else
                    currentStatus = "Đang diễn ra";

                // Ước tính thời gian hoàn thành
                DateTime? estimatedCompletion = null;
                if (completedItems > 0 && completedItems < totalItems && activeSlot != null)
                {
                    var averageTimePerItem = TimeSpan.FromHours(8.0 / totalItems); // Giả sử tour 8 tiếng
                    var remainingItems = totalItems - completedItems;
                    estimatedCompletion = DateTime.UtcNow.Add(averageTimePerItem * remainingItems);
                }

                return new UserTourProgressDto
                {
                    TourOperationId = tourOperationId,
                    TourTitle = tourOperation.TourDetails.Title,
                    TourStartDate = activeSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow,
                    GuideName = tourOperation.TourGuide?.FullName,
                    GuidePhone = tourOperation.TourGuide?.PhoneNumber,
                    Timeline = timelineItems,
                    Stats = new TourProgressStatsDto
                    {
                        TotalItems = totalItems,
                        CompletedItems = completedItems,
                        ProgressPercentage = progressPercentage,
                        TotalGuests = totalGuests,
                        CheckedInGuests = checkedInGuests,
                        CheckInPercentage = checkInPercentage
                    },
                    CurrentStatus = currentStatus,
                    CurrentLocation = timelineItems.FirstOrDefault(ti => ti.IsActive)?.Activity,
                    EstimatedCompletion = estimatedCompletion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour progress for operation {TourOperationId}", tourOperationId);
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra user có booking cho tour này không
        /// </summary>
        public async Task<bool> UserHasBookingForTourAsync(Guid userId, Guid tourOperationId)
        {
            try
            {
                return await _unitOfWork.TourBookingRepository.GetQueryable()
                    .AnyAsync(tb => tb.UserId == userId &&
                                   tb.TourOperationId == tourOperationId &&
                                   tb.Status == BookingStatus.Confirmed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user booking for tour {TourOperationId} and user {UserId}", tourOperationId, userId);
                return false;
            }
        }

        /// <summary>
        /// Lấy tổng quan dashboard cho user
        /// </summary>
        public async Task<UserDashboardSummaryDto> GetUserDashboardSummaryAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting dashboard summary for user {UserId}", userId);

                // Lấy tất cả bookings của user
                var bookings = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(tb => tb.UserId == userId)
                    .Include(tb => tb.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                    .Include(tb => tb.TourSlot)
                    .Include(tb => tb.Guests)
                    .OrderByDescending(tb => tb.CreatedAt)
                    .ToListAsync();

                var currentDate = VietnamTimeZoneUtility.GetVietnamNow().Date;

                // Phân loại bookings theo status
                var confirmedBookings = bookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();
                var upcomingBookings = confirmedBookings.Where(b =>
                    b.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue).Date >= currentDate).ToList();
                var completedBookings = confirmedBookings.Where(b =>
                    b.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue).Date < currentDate).ToList();
                var cancelledBookings = bookings.Where(b =>
                    b.Status == BookingStatus.CancelledByCustomer ||
                    b.Status == BookingStatus.CancelledByCompany).ToList();

                // Tính ongoing tours (tours đang diễn ra trong ngày hôm nay)
                var ongoingBookings = confirmedBookings.Where(b =>
                    b.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue).Date == currentDate).ToList();

                // Tính pending feedbacks (tours đã hoàn thành nhưng chưa có feedback)
                var completedBookingIds = completedBookings.Select(b => b.Id).ToList();
                var existingFeedbacks = await _unitOfWork.TourFeedbackRepository.GetQueryable()
                    .Where(tf => completedBookingIds.Contains(tf.TourBookingId))
                    .Select(tf => tf.TourBookingId)
                    .ToListAsync();

                var pendingFeedbacksCount = completedBookings.Count(b => !existingFeedbacks.Contains(b.Id));

                // Lấy recent bookings (5 bookings gần nhất)
                var recentBookingDtos = bookings.Take(5).Select(MapToTourBookingDto).ToList();

                // Lấy upcoming bookings (5 tours sắp tới)
                var upcomingBookingDtos = upcomingBookings.Take(5).Select(MapToTourBookingDto).ToList();

                return new UserDashboardSummaryDto
                {
                    TotalBookings = bookings.Count,
                    UpcomingTours = upcomingBookings.Count,
                    OngoingTours = ongoingBookings.Count,
                    CompletedTours = completedBookings.Count,
                    CancelledTours = cancelledBookings.Count,
                    PendingFeedbacks = pendingFeedbacksCount,
                    RecentBookings = recentBookingDtos,
                    UpcomingBookings = upcomingBookingDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary for user {UserId}", userId);
                return new UserDashboardSummaryDto();
            }
        }

        /// <summary>
        /// Gửi lại QR ticket cho booking
        /// </summary>
        public async Task<ResendQRTicketResultDto> ResendQRTicketAsync(Guid bookingId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Resending QR ticket for booking {BookingId} and user {UserId}", bookingId, userId);

                // Kiểm tra booking thuộc về user và đã confirmed
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(tb => tb.Id == bookingId && tb.UserId == userId && tb.Status == BookingStatus.Confirmed)
                    .Include(tb => tb.User)
                    .Include(tb => tb.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                    .Include(tb => tb.TourSlot)
                    .Include(tb => tb.Guests)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return new ResendQRTicketResultDto
                    {
                        Success = false,
                        Message = "Không tìm thấy booking hoặc booking chưa được xác nhận"
                    };
                }

                // Kiểm tra tour chưa bắt đầu
                var tourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.MaxValue;
                if (tourDate <= VietnamTimeZoneUtility.GetVietnamNow())
                {
                    return new ResendQRTicketResultDto
                    {
                        Success = false,
                        Message = "Không thể gửi lại QR ticket cho tour đã bắt đầu"
                    };
                }

                // Gửi email với QR ticket
                await SendConfirmationEmailsAsync(booking);

                return new ResendQRTicketResultDto
                {
                    Success = true,
                    Message = "QR ticket đã được gửi lại thành công",
                    SentAt = DateTime.UtcNow,
                    Email = booking.ContactEmail
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending QR ticket for booking {BookingId}", bookingId);
                return new ResendQRTicketResultDto
                {
                    Success = false,
                    Message = $"Lỗi khi gửi lại QR ticket: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Helper method để map TourBooking entity sang DTO
        /// </summary>
        private TourBookingDto MapToTourBookingDto(TourBooking booking)
        {
            // Get company name from tour details
            var companyName = string.Empty;
            if (booking.TourOperation?.TourDetails?.CreatedBy?.TourCompany?.CompanyName != null)
            {
                companyName = booking.TourOperation.TourDetails.CreatedBy.TourCompany.CompanyName;
            }
            else if (booking.TourOperation?.TourDetails?.CreatedBy?.Name != null)
            {
                // Fallback to user name if company name is not available
                companyName = booking.TourOperation.TourDetails.CreatedBy.Name;
            }

            return new TourBookingDto
            {
                Id = booking.Id,
                BookingCode = booking.BookingCode,
                TourOperationId = booking.TourOperationId,
                UserId = booking.UserId,
                NumberOfGuests = booking.NumberOfGuests,
                OriginalPrice = booking.OriginalPrice,
                DiscountPercent = booking.DiscountPercent,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                StatusName = booking.Status.ToString(),
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
                BookingType = booking.BookingType,
                GroupName = booking.GroupName,
                GroupDescription = booking.GroupDescription,
                GroupQRCodeData = booking.GroupQRCodeData,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                TourTitle = booking.TourOperation?.TourDetails?.Title ?? "N/A",
                TourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue),
                CompanyName = companyName, // ✅ NEW: Add company name
                Guests = booking.Guests?.Select(g => new TourBookingGuestDto
                {
                    Id = g.Id,
                    GuestName = g.GuestName,
                    GuestEmail = g.GuestEmail,
                    GuestPhone = g.GuestPhone,
                    IsGroupRepresentative = g.IsGroupRepresentative,
                    QRCodeData = g.QRCodeData,
                    IsCheckedIn = g.IsCheckedIn,
                    CheckInTime = g.CheckInTime,
                    CheckInNotes = g.CheckInNotes
                }).ToList() ?? new List<TourBookingGuestDto>()
            };
        }
    }
}
