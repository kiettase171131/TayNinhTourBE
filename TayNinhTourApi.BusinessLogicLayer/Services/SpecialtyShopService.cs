using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Voucher;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho SpecialtyShop business logic
    /// Merged with Shop functionality for timeline integration
    /// </summary>
    public class SpecialtyShopService : BaseService, ISpecialtyShopService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IVoucherRepository _voucherRepository;
        private readonly ITourSlotTimelineProgressRepository _progressRepo;
        private readonly ITimelineItemRepository _timelineRepo;
        private readonly ITourSlotRepository _slotRepo;
        private readonly ITourBookingRepository _bookingRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITourDetailsRepository _detailsRepo;
        private readonly ITourTemplateRepository _templateRepo;
        private readonly ISpecialtyShopRepository _shopRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderDetailRepository _orderDetailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IShopCustomerStatusRepository _shopCustomerStatusRepo;




        public SpecialtyShopService(IMapper mapper, IUnitOfWork unitOfWork, 
            ICurrentUserService currentUserService, IVoucherRepository voucherRepository, 
            ITourSlotTimelineProgressRepository progressRepo, ITimelineItemRepository timelineRepo, ITourSlotRepository slotRepo, 
            ITourBookingRepository bookingRepo, IUserRepository userRepo,ISpecialtyShopRepository specialtyShop,
            ITourDetailsRepository tourDetailsRepository,ITourTemplateRepository tourTemplateRepository,
            IOrderDetailRepository orderDetailRepository,IOrderRepository orderRepository,IProductRepository productRepository
            ,IShopCustomerStatusRepository shopCustomerStatusRepository) : base(mapper, unitOfWork)
        {
            _currentUserService = currentUserService;
            _voucherRepository = voucherRepository;
            _progressRepo = progressRepo;
            _timelineRepo = timelineRepo;
            _slotRepo = slotRepo;
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;
            _shopRepo = specialtyShop;
            _detailsRepo = tourDetailsRepository;
            _templateRepo = tourTemplateRepository;
            _orderRepo = orderRepository;
            _orderDetailRepo = orderDetailRepository;
            _productRepo = productRepository;
            _shopCustomerStatusRepo = shopCustomerStatusRepository;



        }

        /// <summary>
        /// Lấy thông tin shop của user hiện tại
        /// </summary>
        public async Task<ApiResponse<SpecialtyShopResponseDto>> GetMyShopAsync(CurrentUserObject currentUser)
        {
            try
            {
                var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(currentUser.Id);

                if (specialtyShop == null)
                {
                    return ApiResponse<SpecialtyShopResponseDto>.NotFound("You don't have a specialty shop yet. Please apply for shop registration first.");
                }

                var responseDto = _mapper.Map<SpecialtyShopResponseDto>(specialtyShop);
                return ApiResponse<SpecialtyShopResponseDto>.Success(responseDto, "Shop information retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SpecialtyShopResponseDto>.Error(500, $"An error occurred while retrieving shop information: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin shop của user hiện tại
        /// </summary>
        public async Task<ApiResponse<SpecialtyShopResponseDto>> UpdateMyShopAsync(UpdateSpecialtyShopDto updateDto, CurrentUserObject currentUser)
        {
            try
            {
                var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(currentUser.Id);

                if (specialtyShop == null)
                {
                    return ApiResponse<SpecialtyShopResponseDto>.NotFound("You don't have a specialty shop yet. Please apply for shop registration first.");
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateDto.ShopName))
                    specialtyShop.ShopName = updateDto.ShopName;

                if (updateDto.Description != null)
                    specialtyShop.Description = updateDto.Description;

                if (!string.IsNullOrWhiteSpace(updateDto.Location))
                    specialtyShop.Location = updateDto.Location;

                if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber))
                    specialtyShop.PhoneNumber = updateDto.PhoneNumber;

                if (!string.IsNullOrWhiteSpace(updateDto.Website))
                    specialtyShop.Website = updateDto.Website;

                if (!string.IsNullOrWhiteSpace(updateDto.ShopType))
                    specialtyShop.ShopType = updateDto.ShopType;

                if (!string.IsNullOrWhiteSpace(updateDto.OpeningHours))
                    specialtyShop.OpeningHours = updateDto.OpeningHours;

                if (!string.IsNullOrWhiteSpace(updateDto.ClosingHours))
                    specialtyShop.ClosingHours = updateDto.ClosingHours;

                if (updateDto.IsShopActive.HasValue)
                    specialtyShop.IsShopActive = updateDto.IsShopActive.Value;

                specialtyShop.UpdatedAt = DateTime.UtcNow;
                specialtyShop.UpdatedById = currentUser.Id;

                await _unitOfWork.SpecialtyShopRepository.UpdateAsync(specialtyShop);
                await _unitOfWork.SaveChangesAsync();

                var responseDto = _mapper.Map<SpecialtyShopResponseDto>(specialtyShop);
                return ApiResponse<SpecialtyShopResponseDto>.Success(responseDto, "Shop information updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SpecialtyShopResponseDto>.Error(500, $"An error occurred while updating shop information: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả shops đang hoạt động
        /// </summary>
        public async Task<ApiResponse<List<SpecialtyShopResponseDto>>> GetAllActiveShopsAsync(string? name = null)
        {
            try
            {       
                var shops = await _unitOfWork.SpecialtyShopRepository.GetActiveShopsAsync(name);
                var responseDtos = _mapper.Map<List<SpecialtyShopResponseDto>>(shops);

                return ApiResponse<List<SpecialtyShopResponseDto>>.Success(responseDtos, $"Retrieved {responseDtos.Count} active shops successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SpecialtyShopResponseDto>>.Error(500, $"An error occurred while retrieving shops: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách shops theo loại
        /// </summary>
        public async Task<ApiResponse<List<SpecialtyShopResponseDto>>> GetShopsByTypeAsync(string shopType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(shopType))
                {
                    return ApiResponse<List<SpecialtyShopResponseDto>>.BadRequest("Shop type cannot be empty");
                }

                var shops = await _unitOfWork.SpecialtyShopRepository.GetShopsByTypeAsync(shopType);
                var responseDtos = _mapper.Map<List<SpecialtyShopResponseDto>>(shops);

                return ApiResponse<List<SpecialtyShopResponseDto>>.Success(responseDtos, $"Retrieved {responseDtos.Count} shops of type '{shopType}' successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SpecialtyShopResponseDto>>.Error(500, $"An error occurred while retrieving shops by type: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một shop theo ID
        /// </summary>
        public async Task<ApiResponse<SpecialtyShopResponseDto>> GetShopByIdAsync(Guid shopId)
        {
            try
            {
                var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByIdAsync(shopId);

                if (specialtyShop == null || !specialtyShop.IsActive)
                {
                    return ApiResponse<SpecialtyShopResponseDto>.NotFound("Shop not found or inactive");
                }

                if (!specialtyShop.IsShopActive)
                {
                    return ApiResponse<SpecialtyShopResponseDto>.BadRequest("Shop is temporarily closed");
                }

                var responseDto = _mapper.Map<SpecialtyShopResponseDto>(specialtyShop);
                return ApiResponse<SpecialtyShopResponseDto>.Success(responseDto, "Shop information retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SpecialtyShopResponseDto>.Error(500, $"An error occurred while retrieving shop information: {ex.Message}");
            }
        }

        /// <summary>
        /// Tìm kiếm shops theo từ khóa
        /// </summary>
        public async Task<ApiResponse<List<SpecialtyShopResponseDto>>> SearchShopsAsync(string searchTerm)
        {
            try
            {
                var shops = await _unitOfWork.SpecialtyShopRepository.SearchAsync(searchTerm, true);
                var responseDtos = _mapper.Map<List<SpecialtyShopResponseDto>>(shops);

                string message = string.IsNullOrWhiteSpace(searchTerm)
                    ? $"Retrieved all {responseDtos.Count} active shops"
                    : $"Found {responseDtos.Count} shops matching '{searchTerm}'";

                return ApiResponse<List<SpecialtyShopResponseDto>>.Success(responseDtos, message);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SpecialtyShopResponseDto>>.Error(500, $"An error occurred while searching shops: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách shops với phân trang
        /// </summary>
        public async Task<ApiResponse<Common.PagedResult<SpecialtyShopResponseDto>>> GetPagedShopsAsync(int pageIndex, int pageSize, string? name = null)
        {
            try
            {
                if (pageIndex < 0) pageIndex = 0;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (shops, totalCount) = await _unitOfWork.SpecialtyShopRepository.GetPagedAsync(pageIndex, pageSize, true, name);
                var responseDtos = _mapper.Map<List<SpecialtyShopResponseDto>>(shops);

                var pagedResult = new Common.PagedResult<SpecialtyShopResponseDto>
                {
                    Items = responseDtos,
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                return ApiResponse<Common.PagedResult<SpecialtyShopResponseDto>>.Success(pagedResult, $"Retrieved page {pageIndex} of shops successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<Common.PagedResult<SpecialtyShopResponseDto>>.Error(500, $"An error occurred while retrieving paged shops: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách shops theo rating tối thiểu
        /// </summary>
        public async Task<ApiResponse<List<SpecialtyShopResponseDto>>> GetShopsByMinRatingAsync(decimal minRating)
        {
            try
            {
                if (minRating < 0 || minRating > 5)
                {
                    return ApiResponse<List<SpecialtyShopResponseDto>>.BadRequest("Rating must be between 0 and 5");
                }

                var shops = await _unitOfWork.SpecialtyShopRepository.GetShopsByMinRatingAsync(minRating, true);
                var responseDtos = _mapper.Map<List<SpecialtyShopResponseDto>>(shops);

                return ApiResponse<List<SpecialtyShopResponseDto>>.Success(responseDtos, $"Retrieved {responseDtos.Count} shops with rating >= {minRating} successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SpecialtyShopResponseDto>>.Error(500, $"An error occurred while retrieving shops by rating: {ex.Message}");
            }
        }

        // ========== TIMELINE INTEGRATION METHODS ==========

        /// <summary>
        /// Lấy danh sách SpecialtyShops với pagination và filters cho timeline integration
        /// </summary>
        public async Task<ApiResponse<Common.PagedResult<SpecialtyShopResponseDto>>> GetShopsForTimelineAsync(
            int? pageIndex,
            int? pageSize,
            string? textSearch = null,
            string? location = null,
            string? shopType = null,
            bool? status = null)
        {
            try
            {
                // Set default pagination values
                var currentPageIndex = pageIndex ?? 1;
                var currentPageSize = pageSize ?? 10;

                // Validate pagination parameters
                if (currentPageIndex < 1) currentPageIndex = 1;
                if (currentPageSize < 1) currentPageSize = 10;
                if (currentPageSize > 100) currentPageSize = 100; // Limit max page size

                // Build predicate for filtering
                var predicate = PredicateBuilder.New<SpecialtyShop>(x => !x.IsDeleted);

                // Apply status filter
                if (status.HasValue)
                {
                    predicate = predicate.And(x => x.IsActive == status.Value);
                }

                // Apply text search filter
                if (!string.IsNullOrEmpty(textSearch))
                {
                    var searchTerm = textSearch.Trim().ToLower();
                    predicate = predicate.And(x =>
                        x.ShopName.ToLower().Contains(searchTerm) ||
                        (x.Description != null && x.Description.ToLower().Contains(searchTerm)));
                }

                // Apply location filter
                if (!string.IsNullOrEmpty(location))
                {
                    predicate = predicate.And(x => x.Location.Contains(location));
                }

                // Apply shop type filter
                if (!string.IsNullOrEmpty(shopType))
                {
                    predicate = predicate.And(x => x.ShopType == shopType);
                }

                // Get paginated data using repository
                var (shops, totalCount) = await _unitOfWork.SpecialtyShopRepository.GetPagedAsync(
                    currentPageIndex,
                    currentPageSize,
                    status != false // includeActive
                );

                // Apply additional filters if needed
                var filteredShops = shops.AsEnumerable();

                if (!string.IsNullOrEmpty(textSearch))
                {
                    var searchTerm = textSearch.Trim().ToLower();
                    filteredShops = filteredShops.Where(x =>
                        x.ShopName.ToLower().Contains(searchTerm) ||
                        (x.Description != null && x.Description.ToLower().Contains(searchTerm)));
                }

                if (!string.IsNullOrEmpty(location))
                {
                    filteredShops = filteredShops.Where(x => x.Location.Contains(location));
                }

                if (!string.IsNullOrEmpty(shopType))
                {
                    filteredShops = filteredShops.Where(x => x.ShopType == shopType);
                }

                var finalShops = filteredShops.ToList();
                var finalTotalCount = finalShops.Count;

                // Map to SpecialtyShopResponseDto
                var shopDtos = _mapper.Map<List<SpecialtyShopResponseDto>>(finalShops);

                var pagedResult = new Common.PagedResult<SpecialtyShopResponseDto>
                {
                    Items = shopDtos,
                    TotalCount = finalTotalCount,
                    PageIndex = currentPageIndex,
                    PageSize = currentPageSize
                };

                return ApiResponse<Common.PagedResult<SpecialtyShopResponseDto>>.Success(pagedResult, "Lấy danh sách shops thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<Common.PagedResult<SpecialtyShopResponseDto>>.Error(500, $"Lỗi khi lấy danh sách shops: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy SpecialtyShop theo ID cho timeline integration
        /// </summary>
        public async Task<ApiResponse<SpecialtyShopResponseDto>> GetShopByIdForTimelineAsync(Guid id)
        {
            try
            {
                // Get shop with details using repository
                var shop = await _unitOfWork.SpecialtyShopRepository.GetByIdAsync(id);

                if (shop == null)
                {
                    return ApiResponse<SpecialtyShopResponseDto>.NotFound("Không tìm thấy shop này");
                }

                // Map to SpecialtyShopResponseDto
                var shopDto = _mapper.Map<SpecialtyShopResponseDto>(shop);

                return ApiResponse<SpecialtyShopResponseDto>.Success(shopDto, "Lấy thông tin shop thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<SpecialtyShopResponseDto>.Error(500, $"Lỗi khi lấy thông tin shop: {ex.Message}");
            }
        }


        public async Task<ShopVisitorsResponse> GetVisitorsAsync(
     CurrentUserObject currentUserObject,
     int? pageIndex, int? pageSize,
     DateOnly? fromDate, DateOnly? toDate
 )
        {
            var pageIdx = Math.Max(1, pageIndex ?? Constants.PageIndexDefault);
            var pageSz = Math.Max(1, pageSize ?? Constants.PageSizeDefault);

            // 1) Shop của user hiện tại
            var shop = await _shopRepo.GetFirstOrDefaultAsync(s => s.UserId == currentUserObject.UserId);
            if (shop == null)
                return new ShopVisitorsResponse
                {
                    StatusCode = 403,
                    success = false,
                    Message = "Tài khoản hiện tại không sở hữu Specialty Shop."
                };
            var shopId = shop.Id;

            // 2) Repo queryables
            var tlQ = _timelineRepo.GetQueryable().AsNoTracking();
            var slQ = _slotRepo.GetQueryable().AsNoTracking();
            var bkQ = _bookingRepo.GetQueryable().AsNoTracking();
            var usQ = _userRepo.GetQueryable().AsNoTracking();
            var dtQ = _detailsRepo.GetQueryable().AsNoTracking();

            var ordQ = _orderRepo.GetQueryable().AsNoTracking();
            var odQ = _orderDetailRepo.GetQueryable().AsNoTracking();
            var prQ = _productRepo.GetQueryable().AsNoTracking();

            // 3) Base query: Khách đi tour có shop này trong timeline + có ít nhất 1 đơn Paid tại shop
            var baseQ =
                from tl in tlQ.Where(t => t.SpecialtyShopId == shopId)
                join sl in slQ on tl.TourDetailsId equals sl.TourDetailsId
                join bk in bkQ on sl.Id equals bk.TourSlotId
                join us in usQ on bk.UserId equals us.Id
                join dt in dtQ on sl.TourDetailsId equals dt.Id
                where bk.Status == BookingStatus.Confirmed
                      && (from o in ordQ
                          join od in odQ on o.Id equals od.OrderId
                          join p in prQ on od.ProductId equals p.Id
                          where p.SpecialtyShopId == shopId
                                && o.UserId == us.Id
                                && o.Status == OrderStatus.Paid
                          select 1).Any()
                select new { tl, sl, bk, us, dt };

            if (fromDate.HasValue) baseQ = baseQ.Where(x => x.sl.TourDate >= fromDate.Value);
            if (toDate.HasValue) baseQ = baseQ.Where(x => x.sl.TourDate <= toDate.Value);

            // 4) Chọn mốc shop đầu tiên per booking
            var picks =
                from row in baseQ
                group row by row.bk.Id into g
                select new { BookingId = g.Key, MinSortOrder = g.Min(r => r.tl.SortOrder) };

            var chosenQ =
                from row in baseQ
                join p in picks
                  on new { row.bk.Id, row.tl.SortOrder }
                  equals new { Id = p.BookingId, SortOrder = p.MinSortOrder }
                select new ShopVisitQuery
                {
                    ShopId = shopId,
                    BookingId = row.bk.Id,
                    UserId = row.us.Id,
                    CustomerName = row.bk.ContactName ?? row.us.Name,
                    CustomerPhone = row.bk.ContactPhone ?? row.us.PhoneNumber,
                    CustomerEmail = row.bk.ContactEmail ?? row.us.Email,

                    TourSlotId = row.sl.Id,
                    TourDate = row.sl.TourDate,
                    TourName = row.dt.Title,

                    TimelineItemId = row.tl.Id,
                    Activity = row.tl.Activity,
                    SortOrder = row.tl.SortOrder,
                    PlannedCheckInTime = row.tl.CheckInTime,

                    IsCompleted = false,
                    ActualCompletedAt = null
                };

            // --- FILTER NGÀY (tùy chọn) ---
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                // nếu client gửi ngược thì hoán đổi cho thân thiện
                var tmp = fromDate; fromDate = toDate; toDate = tmp;
            }
            if (fromDate.HasValue) chosenQ = chosenQ.Where(x => x.TourDate >= fromDate.Value);
            if (toDate.HasValue) chosenQ = chosenQ.Where(x => x.TourDate <= toDate.Value);

            // --- ĐẾM SAU KHI FILTER, PHÂN TRANG ---
            var total = await chosenQ.CountAsync();

            if (total == 0)
            {
                // ✅ Không có dữ liệu vẫn trả 200 với mảng rỗng
                return new ShopVisitorsResponse
                {
                    StatusCode = 200,
                    success = true,
                    Message = "OK (không có dữ liệu theo điều kiện lọc).",
                    Data = new List<ShopVisitDto>(),
                    TotalRecord = 0,
                    TotalCount = 0,
                    TotalPages = 0,
                    PageIndex = Math.Max(1, pageIndex ?? Constants.PageIndexDefault),
                    PageSize = Math.Max(1, pageSize ?? Constants.PageSizeDefault)
                };
            }
            var page = await chosenQ
                .OrderByDescending(x => x.TourDate)
                .ThenBy(x => x.SortOrder)
                .Skip((pageIdx - 1) * pageSz)
                .Take(pageSz)
                .ProjectTo<ShopVisitDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            // 6) Lấy Products đã mua Paid
            var userIds = page.Select(x => x.UserId).Distinct().ToList();
            var purchasedFlat =
                from o in ordQ.Where(o => userIds.Contains(o.UserId) && o.Status == OrderStatus.Paid)
                join od in odQ on o.Id equals od.OrderId
                join p in prQ on od.ProductId equals p.Id
                where p.SpecialtyShopId == shopId
                select new
                {
                    o.UserId,
                    ProductId = p.Id,
                    ProductName = p.Name,
                    od.Quantity,
                    od.UnitPrice
                };

            var purchasedByUser = await purchasedFlat
                .GroupBy(x => new { x.UserId, x.ProductId, x.ProductName, x.UnitPrice })
                .Select(g => new
                {
                    g.Key.UserId,
                    Item = new PurchasedProductDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        UnitPrice = g.Key.UnitPrice,
                        Quantity = g.Sum(s => s.Quantity)
                    }
                })
                .ToListAsync();

            var dictProducts = purchasedByUser
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(z => z.Item).ToList()
                );

            // 7) Lấy tất cả đơn Paid của shop này + PayOsOrderCode + IsChecked
            var paidOrdersFlat =
                from o in ordQ.Where(o => userIds.Contains(o.UserId) && o.Status == OrderStatus.Paid)
                join od in odQ on o.Id equals od.OrderId
                join p in prQ on od.ProductId equals p.Id
                where p.SpecialtyShopId == shopId
                select new
                {
                    o.UserId,
                    o.Id,
                    o.PayOsOrderCode,
                    o.IsChecked,
                    o.CreatedAt
                };

            var paidOrdersDistinct = await paidOrdersFlat
                .GroupBy(x => new { x.UserId, x.Id, x.PayOsOrderCode, x.IsChecked, x.CreatedAt })
                .Select(g => new
                {
                    g.Key.UserId,
                    Order = new OrderBriefDto
                    {
                        OrderId = g.Key.Id,
                        PayOsOrderCode = g.Key.PayOsOrderCode,
                        IsChecked = g.Key.IsChecked,
                        CreatedAt = g.Key.CreatedAt
                    }
                })
                .ToListAsync();

            var paidOrdersByUser = paidOrdersDistinct
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Order)
                          .OrderByDescending(o => o.CreatedAt)
                          .ToList()
                );

            // 8) Gán PlannedCheckInAtUtc + Products + PaidOrders
            foreach (var item in page)
            {
                item.PlannedCheckInAtUtc = new DateTime(
                    item.TourDate.Year, item.TourDate.Month, item.TourDate.Day,
                    item.PlannedCheckInTime.Hours, item.PlannedCheckInTime.Minutes, item.PlannedCheckInTime.Seconds,
                    DateTimeKind.Utc);

                if (dictProducts.TryGetValue(item.UserId, out var products))
                    item.Products = products;

                if (paidOrdersByUser.TryGetValue(item.UserId, out var orders))
                    item.PaidOrders = orders;
            }

            return new ShopVisitorsResponse
            {
                StatusCode = 200,
                success = true,
                Message = "Get Successfully",
                Data = page,
                TotalRecord = total,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling((double)total / pageSz),
                PageIndex = pageIdx,
                PageSize = pageSz
            };
        }


        // Public để tái dùng ở nhiều nơi
        public async Task<(bool eligible, DateOnly? date, TimeSpan? time, Guid? tlId, string? activity, string? tourName)>
            CheckUpcomingVisitForShopAsync(Guid shopId, Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // ✅ Chỉ check: có tour tương lai (Confirmed) ghé shop hay không
            var visit = await (
                from b in _bookingRepo.GetQueryable().AsNoTracking()
                join s in _slotRepo.GetQueryable().AsNoTracking() on b.TourSlotId equals s.Id
                join tl in _timelineRepo.GetQueryable().AsNoTracking() on s.TourDetailsId equals tl.TourDetailsId
                join dt in _detailsRepo.GetQueryable().AsNoTracking() on s.TourDetailsId equals dt.Id
                where b.UserId == userId
                      && b.Status == BookingStatus.Confirmed
                      && s.TourDate >= today
                      && tl.SpecialtyShopId == shopId
                orderby s.TourDate, tl.SortOrder
                select new
                {
                    s.TourDate,
                    tl.CheckInTime,
                    tl.Id,
                    tl.Activity,
                    TourName = dt.Title
                }
            ).FirstOrDefaultAsync();

            if (visit == null)
                return (false, null, null, null, null, null);

            return (true, visit.TourDate, visit.CheckInTime, visit.Id, visit.Activity, visit.TourName);
        }


      








        // CreateShopAsync removed - timeline integration only needs to read existing SpecialtyShops
        // New SpecialtyShops are created through the shop application approval process
    }
}
