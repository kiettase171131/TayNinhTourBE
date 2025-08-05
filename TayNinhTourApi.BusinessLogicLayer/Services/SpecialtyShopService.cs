using AutoMapper;
using LinqKit;
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

        public SpecialtyShopService(IMapper mapper, IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IVoucherRepository voucherRepository) : base(mapper, unitOfWork)
        {
            _currentUserService = currentUserService;
            _voucherRepository = voucherRepository;
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

        

        
        


       

        // CreateShopAsync removed - timeline integration only needs to read existing SpecialtyShops
        // New SpecialtyShops are created through the shop application approval process
    }
}
