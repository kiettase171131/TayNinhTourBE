using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller quản lý TourDetails và timeline chi tiết của tour templates
    /// Cung cấp các endpoints để CRUD TourDetails và quản lý timeline items
    /// Read operations accessible to all authenticated users, write operations restricted to Tour Company role
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Base authentication required, specific role restrictions applied at method level
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
        /// Lấy danh sách tất cả TourDetails với phân trang mặc định
        /// Endpoint tương thích với frontend calls
        /// </summary>
        /// <param name="pageIndex">Chỉ số trang (0-based, default: 0)</param>
        /// <param name="pageSize">Kích thước trang (default: 10)</param>
        /// <param name="includeInactive">Bao gồm inactive records (default: false)</param>
        /// <returns>Danh sách TourDetails có phân trang</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllTourDetails(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting all TourDetails - Page: {PageIndex}, Size: {PageSize}, IncludeInactive: {IncludeInactive}",
                    pageIndex, pageSize, includeInactive);

                var response = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    pageIndex, pageSize, null, null, null, null, null, null, null, includeInactive);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all TourDetails");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lịch trình"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách TourDetails của một tour template
        /// </summary>
        /// <param name="templateId">ID của tour template</param>
        /// <param name="includeInactive">Có bao gồm TourDetails không active không (default: false)</param>
        /// <returns>Danh sách TourDetails của template</returns>
        [HttpGet("template/{templateId:guid}")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [Authorize(Roles = Constants.RoleTourCompanyName)]
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
        [Authorize(Roles = Constants.RoleTourCompanyName)]
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
        [Authorize(Roles = Constants.RoleTourCompanyName)]
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
        /// <param name="searchTerm">Tìm kiếm theo title và description của tour (optional)</param>
        /// <param name="minPrice">Giá tối thiểu của tour operation (optional)</param>
        /// <param name="maxPrice">Giá tối đa của tour operation (optional)</param>
        /// <param name="scheduleDay">Thứ trong tuần (Saturday/Sunday) từ tour template (optional)</param>
        /// <param name="startLocation">Điểm bắt đầu từ tour template (optional)</param>
        /// <param name="endLocation">Điểm kết thúc từ tour template (optional)</param>
        /// <param name="hasEarlyBird">Lọc tour có early bird discount (optional)</param>
        /// <param name="includeInactive">Bao gồm inactive records (default: false)</param>
        /// <returns>Danh sách TourDetails có phân trang</returns>
        [HttpGet("paginated")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTourDetailsPaginated(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? scheduleDay = null,
            [FromQuery] string? startLocation = null,
            [FromQuery] string? endLocation = null,
            [FromQuery] bool? hasEarlyBird = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting paginated TourDetails - Page: {PageIndex}, Size: {PageSize}, SearchTerm: {SearchTerm}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, ScheduleDay: {ScheduleDay}, StartLocation: {StartLocation}, EndLocation: {EndLocation}, HasEarlyBird: {HasEarlyBird}",
                    pageIndex, pageSize, searchTerm, minPrice, maxPrice, scheduleDay, startLocation, endLocation, hasEarlyBird);

                var response = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    pageIndex, pageSize, searchTerm, minPrice, maxPrice, scheduleDay, startLocation, endLocation, hasEarlyBird, includeInactive);
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
        /// Tạo mới timeline items (single hoặc bulk)
        /// </summary>
        /// <param name="request">Thông tin timeline items cần tạo</param>
        /// <returns>Timeline items vừa được tạo</returns>
        [HttpPost("timeline")]
        [Authorize(Roles = Constants.RoleTourCompanyName)]
        public async Task<IActionResult> CreateTimelineItems([FromBody] RequestCreateTimelineItemsDto request)
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
                _logger.LogInformation("Creating {Count} timeline items for TourDetails {TourDetailsId} by user {UserId}",
                    request.TimelineItems.Count, request.TourDetailsId, userId);

                var response = await _tourDetailsService.CreateTimelineItemsAsync(request, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timeline items for TourDetails {TourDetailsId}", request.TourDetailsId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo timeline items"
                });
            }
        }

        /// <summary>
        /// Tạo mới một timeline item (backward compatibility)
        /// </summary>
        /// <param name="request">Thông tin timeline item cần tạo</param>
        /// <returns>Timeline item vừa được tạo</returns>
        [HttpPost("timeline/single")]
        [Authorize(Roles = Constants.RoleTourCompanyName)]
        public async Task<IActionResult> CreateSingleTimelineItem([FromBody] RequestCreateTimelineItemDto request)
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
                _logger.LogInformation("Creating single timeline item for TourDetails {TourDetailsId} by user {UserId}",
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
        [Authorize(Roles = Constants.RoleTourCompanyName)]
        public async Task<IActionResult> UpdateTimelineItem(
            [FromRoute] Guid id,
            [FromBody] RequestUpdateTimelineItemDto request)
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
        [Authorize(Roles = Constants.RoleTourCompanyName)]
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
        [Authorize(Roles = Constants.RoleTourCompanyName)]
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

        // ===== TOUR GUIDE ASSIGNMENT WORKFLOW ENDPOINTS =====

        /// <summary>
        /// Lấy trạng thái phân công hướng dẫn viên cho TourDetails
        /// </summary>
        /// <param name="id">ID của TourDetails</param>
        /// <returns>Thông tin trạng thái assignment</returns>
        [HttpGet("{id:guid}/guide-assignment-status")]
        [Authorize(Roles = Constants.RoleTourCompanyName + "," + Constants.RoleAdminName)]
        public async Task<IActionResult> GetGuideAssignmentStatus([FromRoute] Guid id)
        {
            try
            {
                _logger.LogInformation("Getting guide assignment status for TourDetails {TourDetailsId}", id);

                var response = await _tourDetailsService.GetGuideAssignmentStatusAsync(id);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guide assignment status for TourDetails {TourDetailsId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy trạng thái phân công hướng dẫn viên"
                });
            }
        }

        /// <summary>
        /// TourCompany mời thủ công một TourGuide cụ thể
        /// </summary>
        /// <param name="id">ID của TourDetails</param>
        /// <param name="request">Thông tin mời guide</param>
        /// <returns>Kết quả gửi lời mời</returns>
        [HttpPost("{id:guid}/manual-invite-guide")]
        [Authorize(Roles = Constants.RoleTourCompanyName)]
        public async Task<IActionResult> ManualInviteGuide(
            [FromRoute] Guid id,
            [FromBody] InviteTourGuideDto request)
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

                // Validate TourDetailsId matches URL parameter
                if (request.TourDetailsId != id)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "TourDetailsId trong URL và body không khớp"
                    });
                }

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("TourCompany {CompanyId} manually inviting Guide {GuideId} for TourDetails {TourDetailsId}",
                    userId, request.GuideId, id);

                var response = await _tourDetailsService.ManualInviteGuideAsync(id, request.GuideId, userId, request.AdditionalMessage);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual invitation for TourDetails {TourDetailsId}", id);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi mời hướng dẫn viên"
                });
            }
        }
    }
}
