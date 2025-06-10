using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Common;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller quản lý TourOperation - thông tin vận hành cho TourDetails
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Constants.RoleTourCompanyName)]
    public class TourOperationController : ControllerBase
    {
        private readonly ITourOperationService _tourOperationService;
        private readonly ILogger<TourOperationController> _logger;

        public TourOperationController(
            ITourOperationService tourOperationService,
            ILogger<TourOperationController> logger)
        {
            _tourOperationService = tourOperationService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo operation mới cho TourDetails
        /// </summary>
        /// <param name="request">Thông tin operation</param>
        /// <returns>Operation được tạo</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseCreateOperationDto), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 400)]
        public async Task<ActionResult<ResponseCreateOperationDto>> CreateOperation(
            [FromBody] RequestCreateOperationDto request)
        {
            try
            {
                _logger.LogInformation("Creating operation for TourDetails {TourDetailsId}", request.TourDetailsId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        IsSuccess = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var result = await _tourOperationService.CreateOperationAsync(request);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                _logger.LogInformation("Operation created successfully for TourDetails {TourDetailsId}", request.TourDetailsId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating operation for TourDetails {TourDetailsId}", request.TourDetailsId);
                return StatusCode(500, new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống khi tạo operation"
                });
            }
        }

        /// <summary>
        /// Lấy operation theo TourDetails ID
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Operation của TourDetails</returns>
        [HttpGet("details/{tourDetailsId:guid}")]
        [ProducesResponseType(typeof(TourOperationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TourOperationDto>> GetOperationByTourDetails(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Getting operation for TourDetails {TourDetailsId}", tourDetailsId);

                var operation = await _tourOperationService.GetOperationByTourDetailsAsync(tourDetailsId);

                if (operation == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        IsSuccess = false,
                        Message = "TourDetails chưa có operation"
                    });
                }

                return Ok(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting operation for TourDetails {TourDetailsId}", tourDetailsId);
                return StatusCode(500, new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống khi lấy operation"
                });
            }
        }

        /// <summary>
        /// Cập nhật operation
        /// </summary>
        /// <param name="id">ID của operation</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(ResponseUpdateOperationDto), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ResponseUpdateOperationDto>> UpdateOperation(
            Guid id,
            [FromBody] RequestUpdateOperationDto request)
        {
            try
            {
                _logger.LogInformation("Updating operation {OperationId}", id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        IsSuccess = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var result = await _tourOperationService.UpdateOperationAsync(id, request);

                if (!result.IsSuccess)
                {
                    if (result.Message.Contains("không tồn tại"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                _logger.LogInformation("Operation {OperationId} updated successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating operation {OperationId}", id);
                return StatusCode(500, new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống khi cập nhật operation"
                });
            }
        }

        /// <summary>
        /// Xóa operation (soft delete)
        /// </summary>
        /// <param name="id">ID của operation</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(BaseResposeDto), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BaseResposeDto>> DeleteOperation(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting operation {OperationId}", id);

                var result = await _tourOperationService.DeleteOperationAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.Message.Contains("không tồn tại"))
                        return NotFound(result);
                    return BadRequest(result);
                }

                _logger.LogInformation("Operation {OperationId} deleted successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting operation {OperationId}", id);
                return StatusCode(500, new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống khi xóa operation"
                });
            }
        }
    }
}
