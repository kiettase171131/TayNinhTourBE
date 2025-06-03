using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho quản lý TourSlot với tự động scheduling
    /// Cung cấp API endpoints cho tạo, quản lý và scheduling tour slots
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Constants.RoleTourCompanyName)]
    public class TourSlotController : ControllerBase
    {
        private readonly ITourSlotService _tourSlotService;

        public TourSlotController(ITourSlotService tourSlotService)
        {
            _tourSlotService = tourSlotService;
        }

        /// <summary>
        /// Tự động tạo tour slots cho một tháng dựa trên tour template
        /// </summary>
        /// <param name="request">Thông tin để generate slots</param>
        /// <returns>Kết quả tạo slots với thông tin chi tiết</returns>
        [HttpPost("generate")]
        public async Task<ActionResult<ResponseGenerateSlotsDto>> GenerateSlots([FromBody] RequestGenerateSlotsDto request)
        {
            var response = await _tourSlotService.GenerateSlotsAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Preview các slots sẽ được tạo trước khi commit vào database
        /// </summary>
        /// <param name="request">Thông tin để preview slots</param>
        /// <returns>Danh sách slots sẽ được tạo</returns>
        [HttpPost("preview")]
        public async Task<ActionResult<ResponsePreviewSlotsDto>> PreviewSlots([FromBody] RequestPreviewSlotsDto request)
        {
            var response = await _tourSlotService.PreviewSlotsAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Lấy danh sách tour slots với filtering và pagination
        /// </summary>
        /// <param name="request">Criteria để filter slots</param>
        /// <returns>Danh sách slots với pagination</returns>
        [HttpGet]
        public async Task<ActionResult<ResponseGetSlotsDto>> GetSlots([FromQuery] RequestGetSlotsDto request)
        {
            var response = await _tourSlotService.GetSlotsAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một tour slot
        /// </summary>
        /// <param name="id">ID của tour slot</param>
        /// <returns>Thông tin chi tiết tour slot</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseGetSlotDetailDto>> GetSlotById(Guid id)
        {
            var response = await _tourSlotService.GetSlotDetailAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Cập nhật thông tin tour slot
        /// </summary>
        /// <param name="id">ID của tour slot</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResposeDto>> UpdateSlot(Guid id, [FromBody] RequestUpdateSlotDto request)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _tourSlotService.UpdateSlotAsync(id, request);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Xóa tour slot
        /// </summary>
        /// <param name="id">ID của tour slot</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResposeDto>> DeleteSlot(Guid id)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _tourSlotService.DeleteSlotAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Kiểm tra conflicts khi tạo slots mới
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="dates">Danh sách dates cần kiểm tra</param>
        /// <returns>Danh sách dates bị conflict</returns>
        [HttpPost("check-conflicts")]
        public async Task<ActionResult<ResponseCheckSlotConflictsDto>> CheckSlotConflicts([FromBody] RequestCheckSlotConflictsDto request)
        {
            var response = await _tourSlotService.CheckSlotConflictsAsync(request.TourTemplateId, request.Dates);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Bulk update status cho nhiều slots cùng lúc
        /// </summary>
        /// <param name="request">Thông tin bulk update</param>
        /// <returns>Kết quả bulk update</returns>
        [HttpPut("bulk-status")]
        public async Task<ActionResult<BaseResposeDto>> BulkUpdateSlotStatus([FromBody] RequestBulkUpdateSlotStatusDto request)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            // Set updatedBy in request if needed
            var response = await _tourSlotService.BulkUpdateSlotStatusAsync(request);
            return StatusCode(response.StatusCode, response);
        }
    }
}
