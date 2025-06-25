using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller quản lý TourOperation - thông tin vận hành cho TourDetails
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
 
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
        [ProducesResponseType(typeof(BaseResposeDto), 404)]
        [ProducesResponseType(typeof(BaseResposeDto), 409)]
        [ProducesResponseType(typeof(BaseResposeDto), 422)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
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
                    // Handle different types of validation errors with appropriate HTTP status codes
                    if (result.Message.Contains("không tồn tại"))
                    {
                        return NotFound(result);
                    }
                    else if (result.Message.Contains("đã có operation"))
                    {
                        return Conflict(result);
                    }
                    else if (result.Message.Contains("Không thể tạo tour operation"))
                    {
                        // This is our new readiness validation error - return 422 Unprocessable Entity
                        return UnprocessableEntity(result);
                    }
                    else
                    {
                        return BadRequest(result);
                    }
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
        /// Kiểm tra tính sẵn sàng của TourDetails để tạo TourOperation
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails cần kiểm tra</param>
        /// <returns>Thông tin chi tiết về tính sẵn sàng</returns>
        [HttpGet("readiness/{tourDetailsId:guid}")]
        [ProducesResponseType(typeof(TourDetailsReadinessDto), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 404)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company,Admin")]
        public async Task<ActionResult<TourDetailsReadinessDto>> GetTourDetailsReadiness(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Checking readiness for TourDetails {TourDetailsId}", tourDetailsId);

                // Check if TourDetails exists
                var tourDetails = await _tourOperationService.GetOperationByTourDetailsAsync(tourDetailsId);
                // Note: We'll use a different method to check existence, but for now this works

                var readinessInfo = await _tourOperationService.GetTourDetailsReadinessAsync(tourDetailsId);

                _logger.LogInformation("Readiness check completed for TourDetails {TourDetailsId}: IsReady={IsReady}",
                    tourDetailsId, readinessInfo.IsReady);

                return Ok(readinessInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking readiness for TourDetails {TourDetailsId}", tourDetailsId);
                return StatusCode(500, new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống khi kiểm tra tính sẵn sàng"
                });
            }
        }

        /// <summary>
        /// Kiểm tra nhanh xem TourDetails có thể tạo TourOperation không
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails cần kiểm tra</param>
        /// <returns>True nếu có thể tạo operation, false nếu không</returns>
        [HttpGet("can-create/{tourDetailsId:guid}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 404)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company,Admin")]
        public async Task<ActionResult> CanCreateOperation(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Checking if can create operation for TourDetails {TourDetailsId}", tourDetailsId);

                var (isReady, errorMessage) = await _tourOperationService.ValidateTourDetailsReadinessAsync(tourDetailsId);

                var response = new
                {
                    TourDetailsId = tourDetailsId,
                    CanCreate = isReady,
                    Message = isReady ? "Tour có thể được public" : errorMessage,
                    IsSuccess = true
                };

                _logger.LogInformation("Can create operation check for TourDetails {TourDetailsId}: {CanCreate}",
                    tourDetailsId, isReady);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if can create operation for TourDetails {TourDetailsId}", tourDetailsId);
                return StatusCode(500, new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống khi kiểm tra khả năng tạo operation"
                });
            }
        }

        /// <summary>
        /// Lấy operation theo TourDetails ID
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Operation của TourDetails</returns>
        [HttpGet("details/{tourDetailsId:guid}")]
        [ProducesResponseType(typeof(TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>> GetOperationByTourDetails(Guid tourDetailsId)
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
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
