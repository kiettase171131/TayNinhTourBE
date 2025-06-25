using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho quản lý SpecialtyShop
    /// Cung cấp APIs cho shop owners và public users
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SpecialtyShopController : ControllerBase
    {
        private readonly ISpecialtyShopService _specialtyShopService;
        private readonly ILogger<SpecialtyShopController> _logger;

        public SpecialtyShopController(ISpecialtyShopService specialtyShopService, ILogger<SpecialtyShopController> logger)
        {
            _specialtyShopService = specialtyShopService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả SpecialtyShops với phân trang mặc định
        /// Endpoint tương thích với frontend calls
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1, default: 1)</param>
        /// <param name="pageSize">Số lượng items per page (default: 10)</param>
        /// <param name="includeInactive">Có bao gồm shops không active không (default: false)</param>
        /// <returns>Danh sách SpecialtyShops với thông tin phân trang</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllShops(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting all SpecialtyShops - Page: {PageIndex}, Size: {PageSize}, IncludeInactive: {IncludeInactive}",
                    pageIndex, pageSize, includeInactive);

                // Nếu includeInactive = false, sử dụng active shops
                if (!includeInactive)
                {
                    var activeResult = await _specialtyShopService.GetAllActiveShopsAsync();
                    return StatusCode(activeResult.StatusCode, activeResult);
                }
                else
                {
                    // Sử dụng paged endpoint cho trường hợp cần includeInactive
                    var pagedResult = await _specialtyShopService.GetPagedShopsAsync(pageIndex, pageSize);
                    return StatusCode(pagedResult.StatusCode, pagedResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all SpecialtyShops");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách cửa hàng"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin shop của user hiện tại
        /// Chỉ user có role "Specialty Shop" mới có thể gọi
        /// </summary>
        /// <returns>Thông tin SpecialtyShop của user</returns>
        [HttpGet("my-shop")]
        [Authorize(Roles = "Specialty Shop")]
        public async Task<IActionResult> GetMyShop()
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _specialtyShopService.GetMyShopAsync(currentUser);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Cập nhật thông tin shop của user hiện tại
        /// Chỉ user có role "Specialty Shop" mới có thể gọi
        /// </summary>
        /// <param name="updateDto">Dữ liệu cập nhật</param>
        /// <returns>Thông tin SpecialtyShop sau khi cập nhật</returns>
        [HttpPut("my-shop")]
        [Authorize(Roles = "Specialty Shop")]
        public async Task<IActionResult> UpdateMyShop([FromBody] UpdateSpecialtyShopDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _specialtyShopService.UpdateMyShopAsync(updateDto, currentUser);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách tất cả shops đang hoạt động
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <returns>Danh sách SpecialtyShops đang hoạt động</returns>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveShops()
        {
            var result = await _specialtyShopService.GetAllActiveShopsAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách shops theo loại
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="shopType">Loại shop (Souvenir, Food, Craft, etc.)</param>
        /// <returns>Danh sách SpecialtyShops theo loại</returns>
        [HttpGet("by-type/{shopType}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetShopsByType(string shopType)
        {
            var result = await _specialtyShopService.GetShopsByTypeAsync(shopType);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một shop theo ID
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="shopId">ID của SpecialtyShop</param>
        /// <returns>Thông tin chi tiết SpecialtyShop</returns>
        [HttpGet("{shopId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetShopById(Guid shopId)
        {
            var result = await _specialtyShopService.GetShopByIdAsync(shopId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Tìm kiếm shops theo từ khóa
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách SpecialtyShops phù hợp</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchShops([FromQuery] string? searchTerm = null)
        {
            var result = await _specialtyShopService.SearchShopsAsync(searchTerm ?? string.Empty);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách shops với phân trang
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng items per page</param>
        /// <returns>Danh sách SpecialtyShops với thông tin phân trang</returns>
        [HttpGet("paged")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPagedShops([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _specialtyShopService.GetPagedShopsAsync(pageIndex, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách shops theo rating tối thiểu
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="minRating">Rating tối thiểu (1-5)</param>
        /// <returns>Danh sách SpecialtyShops có rating >= minRating</returns>
        [HttpGet("by-rating/{minRating:decimal}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetShopsByMinRating(decimal minRating)
        {
            var result = await _specialtyShopService.GetShopsByMinRatingAsync(minRating);
            return StatusCode(result.StatusCode, result);
        }

        // ========== MERGED SHOP TIMELINE INTEGRATION ENDPOINTS ==========

        /// <summary>
        /// Lấy danh sách SpecialtyShops với pagination và filtering cho timeline integration
        /// Dành cho timeline integration và admin management
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="textSearch">Từ khóa tìm kiếm (tên hoặc mô tả shop)</param>
        /// <param name="location">Filter theo location (optional)</param>
        /// <param name="shopType">Filter theo shop type (optional)</param>
        /// <param name="status">Filter theo status (true: active, false: inactive, null: all)</param>
        /// <returns>Danh sách SpecialtyShops với pagination</returns>
        [HttpGet("timeline")]
        [Authorize(Roles = $"{Constants.RoleAdminName},{Constants.RoleTourCompanyName}")]
        public async Task<IActionResult> GetShopsForTimeline(
            [FromQuery] int? pageIndex = 1,
            [FromQuery] int? pageSize = 10,
            [FromQuery] string? textSearch = null,
            [FromQuery] string? location = null,
            [FromQuery] string? shopType = null,
            [FromQuery] bool? status = null)
        {
            try
            {
                _logger.LogInformation("Getting SpecialtyShops for timeline with filters: pageIndex={PageIndex}, pageSize={PageSize}, textSearch={TextSearch}, location={Location}, shopType={ShopType}, status={Status}",
                    pageIndex, pageSize, textSearch, location, shopType, status);

                var response = await _specialtyShopService.GetShopsForTimelineAsync(pageIndex, pageSize, textSearch, location, shopType, status);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SpecialtyShops for timeline");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách SpecialtyShops"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết SpecialtyShop theo ID cho timeline integration
        /// </summary>
        /// <param name="id">ID của SpecialtyShop</param>
        /// <returns>Thông tin chi tiết SpecialtyShop</returns>
        [HttpGet("timeline/{id:guid}")]
        [Authorize(Roles = $"{Constants.RoleAdminName},{Constants.RoleTourCompanyName}")]
        public async Task<IActionResult> GetShopByIdForTimeline([FromRoute] Guid id)
        {
            try
            {
                _logger.LogInformation("Getting SpecialtyShop by ID for timeline: {ShopId}", id);

                var response = await _specialtyShopService.GetShopByIdForTimelineAsync(id);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SpecialtyShop by ID for timeline: {ShopId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thông tin SpecialtyShop"
                });
            }
        }

        // CreateShopForTimeline endpoint removed
        // SpecialtyShops are created through the shop application approval process
        // Timeline integration only needs to read existing SpecialtyShops
    }
}
