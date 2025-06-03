using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller quản lý Shop operations
    /// Cung cấp các endpoints để thêm, sửa, xóa và quản lý shops
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Constants.RoleAdminName},{Constants.RoleTourCompanyName}")]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;
        private readonly ILogger<ShopController> _logger;

        public ShopController(
            IShopService shopService,
            ILogger<ShopController> logger)
        {
            _shopService = shopService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách shops với pagination và filtering
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="textSearch">Từ khóa tìm kiếm (tên hoặc mô tả shop)</param>
        /// <param name="location">Filter theo location (optional)</param>
        /// <param name="shopType">Filter theo shop type (optional)</param>
        /// <param name="status">Filter theo status (true: active, false: inactive, null: all)</param>
        /// <returns>Danh sách shops với pagination</returns>
        [HttpGet]
        public async Task<IActionResult> GetShops(
            [FromQuery] int? pageIndex = 1,
            [FromQuery] int? pageSize = 10,
            [FromQuery] string? textSearch = null,
            [FromQuery] string? location = null,
            [FromQuery] string? shopType = null,
            [FromQuery] bool? status = null)
        {
            try
            {
                _logger.LogInformation("Getting shops with filters: pageIndex={PageIndex}, pageSize={PageSize}, textSearch={TextSearch}, location={Location}, shopType={ShopType}, status={Status}",
                    pageIndex, pageSize, textSearch, location, shopType, status);

                var response = await _shopService.GetShopsAsync(pageIndex, pageSize, textSearch, location, shopType, status);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shops");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách shops"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết shop theo ID
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <returns>Thông tin chi tiết shop</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetShopById([FromRoute] Guid id)
        {
            try
            {
                _logger.LogInformation("Getting shop by ID: {ShopId}", id);

                var response = await _shopService.GetShopByIdAsync(id);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop by ID: {ShopId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thông tin shop"
                });
            }
        }

        /// <summary>
        /// Tạo shop mới
        /// </summary>
        /// <param name="request">Thông tin shop cần tạo</param>
        /// <returns>Thông tin shop vừa được tạo</returns>
        [HttpPost]
        public async Task<IActionResult> CreateShop([FromBody] RequestCreateShopDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                _logger.LogInformation("Creating shop by user {UserId}", currentUserObject.Id);

                var response = await _shopService.CreateShopAsync(request, currentUserObject);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shop");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo shop"
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin shop
        /// </summary>
        /// <param name="id">ID của shop cần cập nhật</param>
        /// <param name="request">Thông tin cần cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateShop(
            [FromRoute] Guid id,
            [FromBody] RequestUpdateShopDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                _logger.LogInformation("Updating shop {ShopId} by user {UserId}", id, currentUserObject.Id);

                var response = await _shopService.UpdateShopAsync(request, id, currentUserObject);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shop {ShopId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi cập nhật shop"
                });
            }
        }

        /// <summary>
        /// Xóa shop (soft delete)
        /// </summary>
        /// <param name="id">ID của shop cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteShop([FromRoute] Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting shop {ShopId}", id);

                var response = await _shopService.DeleteShopAsync(id);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shop {ShopId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi xóa shop"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách shops active cho dropdown selection
        /// </summary>
        /// <param name="location">Filter theo location (optional)</param>
        /// <param name="search">Từ khóa tìm kiếm (optional)</param>
        /// <returns>Danh sách shops active</returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveShops(
            [FromQuery] string? location = null,
            [FromQuery] string? search = null)
        {
            try
            {
                _logger.LogInformation("Getting active shops for dropdown with location: {Location}, search: {Search}", location, search);

                var response = await _shopService.GetActiveShopsAsync(location, search);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active shops");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách shops active"
                });
            }
        }
    }
}
