using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
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

        public UserTourBookingService(
            IUnitOfWork unitOfWork,
            ITourPricingService pricingService,
            ITourRevenueService revenueService,
            IPayOsService payOsService)
        {
            _unitOfWork = unitOfWork;
            _pricingService = pricingService;
            _revenueService = revenueService;
            _payOsService = payOsService;
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
                        DateTime.UtcNow),
                    EarlyBirdPrice = _pricingService.CalculatePrice(
                        td.TourOperation.Price,
                        td.AssignedSlots.Any() ? td.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : DateTime.MaxValue,
                        td.CreatedAt,
                        DateTime.UtcNow).finalPrice,
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
                    TourStartDate = tourDetails.AssignedSlots.Any() ? 
                        tourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : null,
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
                TourDates = tourDetails.AssignedSlots.Select(s => new TourDateDto
                {
                    TourSlotId = s.Id,
                    TourDate = s.TourDate.ToDateTime(TimeOnly.MinValue),
                    ScheduleDay = s.ScheduleDay.ToString(),
                    IsAvailable = s.IsActive && s.Status == TourSlotStatus.Available
                }).ToList()
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

            var bookingDate = request.BookingDate ?? DateTime.UtcNow;
            var tourStartDate = tourOperation.TourDetails.AssignedSlots.Any() ? 
                tourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : 
                DateTime.MaxValue;

            var availableSpots = tourOperation.MaxGuests - tourOperation.CurrentBookings;
            var isAvailable = availableSpots >= request.NumberOfGuests && tourStartDate > DateTime.UtcNow;

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
            // Validation
            if (request.NumberOfGuests != request.AdultCount + request.ChildCount)
            {
                return new CreateBookingResultDto
                {
                    Success = false,
                    Message = "Tổng số khách không khớp với số người lớn + trẻ em"
                };
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Get and lock TourOperation with optimistic concurrency
                var tourOperation = await _unitOfWork.TourOperationRepository.GetQueryable()
                    .Where(to => to.Id == request.TourOperationId && to.IsActive && !to.IsDeleted)
                    .Include(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                    .Include(to => to.CreatedBy) // To get TourCompany info
                    .FirstOrDefaultAsync();

                if (tourOperation?.TourDetails == null)
                {
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Tour không tồn tại hoặc không khả dụng"
                    };
                }

                if (tourOperation.TourDetails.Status != TourDetailsStatus.Public)
                {
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Tour chưa được công khai"
                    };
                }

                // 2. Check availability
                var availableSpots = tourOperation.MaxGuests - tourOperation.CurrentBookings;
                if (availableSpots < request.NumberOfGuests)
                {
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = $"Chỉ còn {availableSpots} chỗ trống, không đủ cho {request.NumberOfGuests} khách"
                    };
                }

                var tourStartDate = tourOperation.TourDetails.AssignedSlots.Any() ? 
                    tourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : 
                    DateTime.MaxValue;

                if (tourStartDate <= DateTime.UtcNow)
                {
                    return new CreateBookingResultDto
                    {
                        Success = false,
                        Message = "Tour đã khởi hành"
                    };
                }

                // 3. Calculate pricing
                var bookingDate = DateTime.UtcNow;
                var pricingInfo = _pricingService.GetPricingInfo(
                    tourOperation.Price,
                    tourStartDate,
                    tourOperation.TourDetails.CreatedAt,
                    bookingDate);

                var totalPrice = pricingInfo.FinalPrice * request.NumberOfGuests;

                // 4. Generate booking code and PayOS order code
                var bookingCode = GenerateBookingCode();
                var payOsOrderCode = GeneratePayOsOrderCode();

                // 5. Create booking
                var booking = new TourBooking
                {
                    Id = Guid.NewGuid(),
                    TourOperationId = request.TourOperationId,
                    TourSlotId = request.TourSlotId, // Include selected slot
                    UserId = userId,
                    NumberOfGuests = request.NumberOfGuests,
                    AdultCount = request.AdultCount,
                    ChildCount = request.ChildCount,
                    OriginalPrice = tourOperation.Price * request.NumberOfGuests,
                    DiscountPercent = pricingInfo.DiscountPercent,
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
                    ReservedUntil = DateTime.UtcNow.AddMinutes(30) // Reserve slot for 30 minutes
                };

                await _unitOfWork.TourBookingRepository.AddAsync(booking);

                // 6. Update current bookings (optimistic concurrency)
                tourOperation.CurrentBookings += request.NumberOfGuests;
                await _unitOfWork.TourOperationRepository.UpdateAsync(tourOperation);

                // 7. Create PayOS payment URL
                var paymentUrl = await _payOsService.CreatePaymentUrlAsync(
                    totalPrice,
                    payOsOrderCode,
                    "https://tndt.netlify.app");

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateBookingResultDto
                {
                    Success = true,
                    Message = "Tạo booking thành công",
                    BookingId = booking.Id,
                    BookingCode = bookingCode,
                    PaymentUrl = paymentUrl,
                    OriginalPrice = pricingInfo.OriginalPrice * request.NumberOfGuests,
                    DiscountPercent = pricingInfo.DiscountPercent,
                    FinalPrice = totalPrice,
                    PricingType = pricingInfo.PricingType,
                    BookingDate = bookingDate,
                    TourStartDate = tourStartDate != DateTime.MaxValue ? tourStartDate : null
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return new CreateBookingResultDto
                {
                    Success = false,
                    Message = "Tour đã được booking bởi người khác, vui lòng thử lại"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(100000, 999999);
            return $"TB{dateStr}{random}";
        }

        /// <summary>
        /// Generate PayOS order code
        /// Format: TNDT + 10 digits (7 from timestamp + 3 random)
        /// </summary>
        private string GeneratePayOsOrderCode()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
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
                    AdultCount = b.AdultCount,
                    ChildCount = b.ChildCount,
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

                // Update booking status
                booking.Status = BookingStatus.CancelledByCustomer;
                booking.CancelledDate = DateTime.UtcNow;
                booking.CancellationReason = reason ?? "Hủy bởi khách hàng";
                booking.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);

                // Release capacity
                if (booking.TourOperation != null)
                {
                    booking.TourOperation.CurrentBookings -= booking.NumberOfGuests;
                    await _unitOfWork.TourOperationRepository.UpdateAsync(booking.TourOperation);
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
                AdultCount = booking.AdultCount,
                ChildCount = booking.ChildCount,
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

                // Add money to TourCompany revenue hold
                if (booking.TourOperation?.TourDetails != null)
                {
                    var tourCompanyUserId = booking.TourOperation.CreatedById;
                    await _revenueService.AddToRevenueHoldAsync(tourCompanyUserId, booking.TotalPrice, booking.Id);
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

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

                // Release capacity
                if (booking.TourOperation != null)
                {
                    booking.TourOperation.CurrentBookings -= booking.NumberOfGuests;
                    await _unitOfWork.TourOperationRepository.UpdateAsync(booking.TourOperation);
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
