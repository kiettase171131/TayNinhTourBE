using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller quản lý TourDetails và timeline chi tiết của tour templates
    /// Cung cấp các endpoints để CRUD TourDetails và quản lý timeline items
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Constants.RoleTourCompanyName)]
    public class TourDetailsController : ControllerBase
    {
        private readonly ITourDetailsService _tourDetailsService;
        private readonly ILogger<TourDetailsController> _logger;

        public TourDetailsController(
            ITourDetailsService tourDetailsService,
            ILogger<TourDetailsController> logger)
        {
            _tourDetailsService = tourDetailsService;
            _logger = logger;
        }

        // ===== TOURDETAILS CRUD ENDPOINTS =====

        /// <summary>
        /// Lấy danh sách TourDetails của một tour template
        /// </summary>
        /// <param name="templateId">ID của tour template</param>
        /// <param name="includeInactive">Có bao gồm TourDetails không active không (default: false)</param>
        /// <returns>Danh sách TourDetails của template</returns>
        [HttpGet("template/{templateId:guid}")]
        public async Task<IActionResult> GetTourDetailsByTemplate(
            [FromRoute] Guid templateId,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting TourDetails for template {TemplateId}", templateId);

                var response = await _tourDetailsService.GetTourDetailsAsync(templateId, includeInactive);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TourDetails for template {TemplateId}", templateId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lịch trình"
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết TourDetails theo ID
        /// </summary>
        /// <param name="id">ID của TourDetails</param>
        /// <returns>Thông tin chi tiết TourDetails</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTourDetailById([FromRoute] Guid id)
        {
            try
            {
                _logger.LogInformation("Getting TourDetail {TourDetailId}", id);

                var response = await _tourDetailsService.GetTourDetailByIdAsync(id);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TourDetail {TourDetailId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thông tin lịch trình"
                });
            }
        }

        /// <summary>
        /// Tạo TourDetails mới (tự động clone TourSlots từ template)
        /// </summary>
        /// <param name="request">Thông tin TourDetails cần tạo</param>
        /// <returns>TourDetails vừa được tạo với cloned TourSlots</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTourDetail([FromBody] RequestCreateTourDetailDto request)
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

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("Creating TourDetail for template {TemplateId} by user {UserId}",
                    request.TourTemplateId, userId);

                var response = await _tourDetailsService.CreateTourDetailAsync(request, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating TourDetail for template {TemplateId}", request.TourTemplateId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo lịch trình"
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin TourDetails
        /// </summary>
        /// <param name="id">ID của TourDetails cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>TourDetails sau khi cập nhật</returns>
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateTourDetail(
            [FromRoute] Guid id,
            [FromBody] RequestUpdateTourDetailDto request)
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

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("Updating TourDetail {TourDetailId} by user {UserId}", id, userId);

                var response = await _tourDetailsService.UpdateTourDetailAsync(id, request, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating TourDetail {TourDetailId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi cập nhật lịch trình"
                });
            }
        }

        /// <summary>
        /// Xóa TourDetails và cleanup related data
        /// </summary>
        /// <param name="id">ID của TourDetails cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTourDetail([FromRoute] Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("Deleting TourDetail {TourDetailId} by user {UserId}", id, userId);

                var response = await _tourDetailsService.DeleteTourDetailAsync(id, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting TourDetail {TourDetailId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi xóa lịch trình"
                });
            }
        }

        /// <summary>
        /// Tìm kiếm TourDetails theo keyword
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="templateId">ID template để lọc (optional)</param>
        /// <param name="includeInactive">Bao gồm inactive records (default: false)</param>
        /// <returns>Kết quả tìm kiếm</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchTourDetails(
            [FromQuery, Required] string keyword,
            [FromQuery] Guid? templateId = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Searching TourDetails with keyword: {Keyword}", keyword);

                var response = await _tourDetailsService.SearchTourDetailsAsync(keyword, templateId, includeInactive);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching TourDetails with keyword: {Keyword}", keyword);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tìm kiếm lịch trình"
                });
            }
        }

        /// <summary>
        /// Lấy TourDetails với phân trang
        /// </summary>
        /// <param name="pageIndex">Chỉ số trang (0-based, default: 0)</param>
        /// <param name="pageSize">Kích thước trang (default: 10)</param>
        /// <param name="templateId">Filter theo template (optional)</param>
        /// <param name="titleFilter">Filter theo title (optional)</param>
        /// <param name="includeInactive">Bao gồm inactive records (default: false)</param>
        /// <returns>Danh sách TourDetails có phân trang</returns>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetTourDetailsPaginated(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? templateId = null,
            [FromQuery] string? titleFilter = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting paginated TourDetails - Page: {PageIndex}, Size: {PageSize}",
                    pageIndex, pageSize);

                var response = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    pageIndex, pageSize, templateId, titleFilter, includeInactive);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated TourDetails");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lịch trình"
                });
            }
        }

        // ===== TIMELINE ENDPOINTS (EXISTING & NEW) =====

        /// <summary>
        /// Lấy timeline của một TourDetails cụ thể (NEW - theo design mới)
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="includeInactive">Có bao gồm các items không active không (default: false)</param>
        /// <param name="includeShopInfo">Có bao gồm thông tin shop không (default: true)</param>
        /// <returns>Timeline với danh sách các timeline items của TourDetails</returns>
        [HttpGet("{tourDetailsId:guid}/timeline")]
        public async Task<IActionResult> GetTimelineByTourDetails(
            [FromRoute] Guid tourDetailsId,
            [FromQuery] bool includeInactive = false,
            [FromQuery] bool includeShopInfo = true)
        {
            try
            {
                _logger.LogInformation("Getting timeline for TourDetails {TourDetailsId}", tourDetailsId);

                var request = new RequestGetTimelineByTourDetailsDto
                {
                    TourDetailsId = tourDetailsId,
                    IncludeInactive = includeInactive,
                    IncludeShopInfo = includeShopInfo
                };

                var response = await _tourDetailsService.GetTimelineByTourDetailsAsync(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline for TourDetails {TourDetailsId}", tourDetailsId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy timeline"
                });
            }
        }

        /// <summary>
        /// Lấy timeline đầy đủ của một tour template (DEPRECATED - dùng cho backward compatibility)
        /// </summary>
        /// <param name="templateId">ID của tour template</param>
        /// <param name="includeInactive">Có bao gồm các items không active không (default: false)</param>
        /// <param name="includeShopInfo">Có bao gồm thông tin shop không (default: true)</param>
        /// <returns>Timeline với danh sách các tour details được sắp xếp theo thứ tự</returns>
        [HttpGet("timeline/{templateId:guid}")]
        [Obsolete("Use GetTimelineByTourDetails instead. This endpoint will be removed in future versions.")]
        public async Task<IActionResult> GetTimeline(
            [FromRoute] Guid templateId,
            [FromQuery] bool includeInactive = false,
            [FromQuery] bool includeShopInfo = true)
        {
            try
            {
                _logger.LogInformation("Getting timeline for template {TemplateId}", templateId);

                var request = new RequestGetTimelineDto
                {
                    TourTemplateId = templateId,
                    IncludeInactive = includeInactive,
                    IncludeShopInfo = includeShopInfo
                };

                var response = await _tourDetailsService.GetTimelineAsync(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline for template {TemplateId}", templateId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy timeline"
                });
            }
        }

        /// <summary>
        /// Tạo mới một timeline item
        /// </summary>
        /// <param name="request">Thông tin timeline item cần tạo</param>
        /// <returns>Timeline item vừa được tạo</returns>
        [HttpPost("timeline")]
        public async Task<IActionResult> CreateTimelineItem([FromBody] RequestCreateTimelineItemDto request)
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

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("Creating timeline item for TourDetails {TourDetailsId} by user {UserId}",
                    request.TourDetailsId, userId);

                var response = await _tourDetailsService.CreateTimelineItemAsync(request, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timeline item for TourDetails {TourDetailsId}", request.TourDetailsId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo timeline item"
                });
            }
        }

        /// <summary>
        /// Cập nhật một timeline item
        /// </summary>
        /// <param name="id">ID của timeline item cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Timeline item sau khi cập nhật</returns>
        [HttpPatch("timeline/{id:guid}")]
        public async Task<IActionResult> UpdateTimelineItem(
            [FromRoute] Guid id,
            [FromBody] RequestUpdateTourDetailDto request)
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

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("Updating timeline item {ItemId} by user {UserId}", id, userId);

                var response = await _tourDetailsService.UpdateTimelineItemAsync(id, request, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating timeline item {ItemId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi cập nhật timeline item"
                });
            }
        }

        /// <summary>
        /// Xóa một timeline item
        /// </summary>
        /// <param name="id">ID của timeline item cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("timeline/{id:guid}")]
        public async Task<IActionResult> DeleteTimelineItem([FromRoute] Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("Deleting timeline item {ItemId} by user {UserId}", id, userId);

                var response = await _tourDetailsService.DeleteTimelineItemAsync(id, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting timeline item {ItemId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi xóa timeline item"
                });
            }
        }

        /// <summary>
        /// Sắp xếp lại thứ tự các timeline items
        /// </summary>
        /// <param name="request">Danh sách timeline items với thứ tự mới</param>
        /// <returns>Timeline sau khi sắp xếp lại</returns>
        [HttpPost("timeline/reorder")]
        public async Task<IActionResult> ReorderTimeline([FromBody] RequestReorderTimelineDto request)
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

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("Reordering timeline items by user {UserId}", userId);

                var response = await _tourDetailsService.ReorderTimelineAsync(request, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering timeline items");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi sắp xếp lại timeline"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách shops có sẵn cho dropdown selection
        /// </summary>
        /// <param name="includeInactive">Có bao gồm shops không active không (default: false)</param>
        /// <param name="search">Từ khóa tìm kiếm (tùy chọn)</param>
        /// <returns>Danh sách shops có sẵn</returns>
        [HttpGet("shops")]
        public async Task<IActionResult> GetAvailableShops(
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? search = null)
        {
            try
            {
                _logger.LogInformation("Getting available shops with search: {Search}", search);

                var response = await _tourDetailsService.GetAvailableShopsAsync(includeInactive, search);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available shops");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách shops"
                });
            }
        }
    }
}
