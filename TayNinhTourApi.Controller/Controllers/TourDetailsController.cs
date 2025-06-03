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
    /// Controller quản lý timeline chi tiết của tour templates
    /// Cung cấp các endpoints để thêm, sửa, xóa và sắp xếp lại timeline items
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

        /// <summary>
        /// Lấy timeline đầy đủ của một tour template
        /// </summary>
        /// <param name="templateId">ID của tour template</param>
        /// <param name="includeInactive">Có bao gồm các items không active không (default: false)</param>
        /// <param name="includeShopInfo">Có bao gồm thông tin shop không (default: true)</param>
        /// <returns>Timeline với danh sách các tour details được sắp xếp theo thứ tự</returns>
        [HttpGet("timeline/{templateId:guid}")]
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
        public async Task<IActionResult> CreateTimelineItem([FromBody] RequestCreateTourDetailDto request)
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
                _logger.LogInformation("Creating timeline item for template {TemplateId} by user {UserId}", 
                    request.TourTemplateId, userId);

                var response = await _tourDetailsService.AddTimelineItemAsync(request, userId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timeline item for template {TemplateId}", request.TourTemplateId);
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
