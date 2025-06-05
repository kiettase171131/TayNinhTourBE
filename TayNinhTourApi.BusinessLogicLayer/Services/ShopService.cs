using AutoMapper;
using LinqKit;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho quản lý Shop operations
    /// Kế thừa từ BaseService và implement IShopService
    /// </summary>
    public class ShopService : BaseService, IShopService
    {
        private readonly ICurrentUserService _currentUserService;

        public ShopService(IMapper mapper, IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : base(mapper, unitOfWork)
        {
            _currentUserService = currentUserService;
        }

        public async Task<ResponseGetShopsDto> GetShopsAsync(
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
                var predicate = PredicateBuilder.New<Shop>(x => !x.IsDeleted);

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
                        x.Name.ToLower().Contains(searchTerm) ||
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
                var (shops, totalCount) = await _unitOfWork.ShopRepository.GetPaginatedAsync(
                    currentPageIndex,
                    currentPageSize,
                    location,
                    shopType,
                    null, // minRating - not used in this method
                    status == false || status == null // includeInactive
                );

                // Apply additional text search if needed (repository method doesn't support text search)
                if (!string.IsNullOrEmpty(textSearch))
                {
                    var searchTerm = textSearch.Trim().ToLower();
                    shops = shops.Where(x =>
                        x.Name.ToLower().Contains(searchTerm) ||
                        (x.Description != null && x.Description.ToLower().Contains(searchTerm)));
                    totalCount = shops.Count();
                }

                // Map to DTOs
                var shopDtos = _mapper.Map<List<ShopDto>>(shops);

                return new ResponseGetShopsDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách shops thành công",
                    Data = shopDtos,
                    TotalCount = totalCount,
                    PageIndex = currentPageIndex,
                    PageSize = currentPageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize)
                };
            }
            catch (Exception ex)
            {
                return new ResponseGetShopsDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi lấy danh sách shops: {ex.Message}"
                };
            }
        }

        public async Task<ResponseGetShopByIdDto> GetShopByIdAsync(Guid id)
        {
            try
            {
                // Get shop with details using repository
                var shop = await _unitOfWork.ShopRepository.GetWithDetailsAsync(id);

                if (shop == null)
                {
                    return new ResponseGetShopByIdDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy shop này"
                    };
                }

                // Map to DTO
                var shopDto = _mapper.Map<ShopDto>(shop);

                return new ResponseGetShopByIdDto
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin shop thành công",
                    Data = shopDto
                };
            }
            catch (Exception ex)
            {
                return new ResponseGetShopByIdDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi lấy thông tin shop: {ex.Message}"
                };
            }
        }

        public async Task<ResponseCreateShopDto> CreateShopAsync(RequestCreateShopDto request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(request.Name?.Trim()))
                {
                    return new ResponseCreateShopDto
                    {
                        StatusCode = 400,
                        Message = "Tên shop là bắt buộc"
                    };
                }

                if (string.IsNullOrEmpty(request.Location?.Trim()))
                {
                    return new ResponseCreateShopDto
                    {
                        StatusCode = 400,
                        Message = "Địa điểm shop là bắt buộc"
                    };
                }

                // Check if shop name already exists in the same location
                var existingShops = await _unitOfWork.ShopRepository.GetByLocationAsync(request.Location, true);
                if (existingShops.Any(x => x.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase) && !x.IsDeleted))
                {
                    return new ResponseCreateShopDto
                    {
                        StatusCode = 400,
                        Message = "Đã tồn tại shop với tên này tại địa điểm này"
                    };
                }

                // Map request to entity
                var shop = _mapper.Map<Shop>(request);

                // Get current user ID
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return new ResponseCreateShopDto
                    {
                        StatusCode = 401,
                        Message = "Không thể xác định user hiện tại"
                    };
                }

                // Set audit fields
                shop.Id = Guid.NewGuid();
                shop.CreatedAt = DateTime.Now;
                shop.CreatedById = currentUserId;
                shop.IsActive = true;
                shop.IsDeleted = false;

                // Add to repository
                await _unitOfWork.ShopRepository.AddAsync(shop);
                await _unitOfWork.SaveChangesAsync();

                // Get the created shop with details
                var createdShop = await _unitOfWork.ShopRepository.GetWithDetailsAsync(shop.Id);
                var shopDto = _mapper.Map<ShopDto>(createdShop);

                return new ResponseCreateShopDto
                {
                    StatusCode = 201,
                    Message = "Tạo shop thành công",
                    Data = shopDto
                };
            }
            catch (Exception ex)
            {
                return new ResponseCreateShopDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi tạo shop: {ex.Message}"
                };
            }
        }

        public async Task<BaseResposeDto> UpdateShopAsync(RequestUpdateShopDto request, Guid id)
        {
            try
            {
                // Get existing shop
                var existingShop = await _unitOfWork.ShopRepository.GetByIdAsync(id);

                if (existingShop == null || existingShop.IsDeleted)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy shop này"
                    };
                }

                // Check if name is being updated and already exists in the same location
                if (!string.IsNullOrEmpty(request.Name?.Trim()) &&
                    !existingShop.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    var location = request.Location?.Trim() ?? existingShop.Location;
                    var existingShops = await _unitOfWork.ShopRepository.GetByLocationAsync(location, true);
                    if (existingShops.Any(x => x.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                              x.Id != id && !x.IsDeleted))
                    {
                        return new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = "Đã tồn tại shop với tên này tại địa điểm này"
                        };
                    }
                }

                // Map updates to existing entity (only non-null values)
                _mapper.Map(request, existingShop);

                // Set audit fields
                existingShop.UpdatedAt = DateTime.Now;
                existingShop.UpdatedById = _currentUserService.GetCurrentUserId();

                // Update in repository
                _unitOfWork.ShopRepository.Update(existingShop);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Cập nhật shop thành công"
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi cập nhật shop: {ex.Message}"
                };
            }
        }

        public async Task<BaseResposeDto> DeleteShopAsync(Guid id)
        {
            try
            {
                // Get existing shop
                var existingShop = await _unitOfWork.ShopRepository.GetByIdAsync(id);

                if (existingShop == null || existingShop.IsDeleted)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy shop này"
                    };
                }

                // Check if shop is being used in any tour details
                var isInUse = await _unitOfWork.ShopRepository.IsShopInUseAsync(id);
                if (isInUse)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Không thể xóa shop này vì đang được sử dụng trong tour"
                    };
                }

                // Soft delete
                existingShop.IsDeleted = true;
                existingShop.UpdatedAt = DateTime.Now;

                _unitOfWork.ShopRepository.Update(existingShop);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Xóa shop thành công"
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi xóa shop: {ex.Message}"
                };
            }
        }

        public async Task<ResponseGetActiveShopsDto> GetActiveShopsAsync(string? location = null, string? shopType = null)
        {
            try
            {
                IEnumerable<Shop> shops;

                // Get shops based on filters
                if (!string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(shopType))
                {
                    // Filter by both location and shop type
                    var locationShops = await _unitOfWork.ShopRepository.GetByLocationAsync(location, false);
                    shops = locationShops.Where(x => x.ShopType == shopType);
                }
                else if (!string.IsNullOrEmpty(location))
                {
                    // Filter by location only
                    shops = await _unitOfWork.ShopRepository.GetByLocationAsync(location, false);
                }
                else if (!string.IsNullOrEmpty(shopType))
                {
                    // Filter by shop type only
                    shops = await _unitOfWork.ShopRepository.GetByShopTypeAsync(shopType, false);
                }
                else
                {
                    // Get all active shops
                    shops = await _unitOfWork.ShopRepository.GetAllAsync();
                    shops = shops.Where(x => x.IsActive && !x.IsDeleted);
                }

                // Map to summary DTOs
                var shopSummaryDtos = _mapper.Map<List<ShopSummaryDto>>(shops.OrderBy(x => x.Name));

                return new ResponseGetActiveShopsDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách shops active thành công",
                    Data = shopSummaryDtos,
                    TotalCount = shopSummaryDtos.Count
                };
            }
            catch (Exception ex)
            {
                return new ResponseGetActiveShopsDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi lấy danh sách shops active: {ex.Message}"
                };
            }
        }
    }
}
