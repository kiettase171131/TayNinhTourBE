using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller quản lý TourGuide invitation workflow
    /// Cung cấp endpoints cho việc mời, chấp nhận, từ chối invitations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TourGuideInvitationController : ControllerBase
    {
        private readonly ITourGuideInvitationService _invitationService;
        private readonly ILogger<TourGuideInvitationController> _logger;

        public TourGuideInvitationController(
            ITourGuideInvitationService invitationService,
            ILogger<TourGuideInvitationController> logger)
        {
            _invitationService = invitationService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách invitations của TourGuide hiện tại
        /// </summary>
        /// <param name="status">Lọc theo status (optional)</param>
        /// <returns>Danh sách invitations</returns>
        [HttpGet("my-invitations")]
        [Authorize(Roles = "TourGuide")]
        public async Task<ActionResult<MyInvitationsResponseDto>> GetMyInvitations([FromQuery] string? status = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không thể xác thực người dùng",
                        IsSuccess = false
                    });
                }

                // Parse status if provided
                InvitationStatus? invitationStatus = null;
                if (!string.IsNullOrEmpty(status))
                {
                    if (Enum.TryParse<InvitationStatus>(status, true, out var parsedStatus))
                    {
                        invitationStatus = parsedStatus;
                    }
                    else
                    {
                        return BadRequest(new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = $"Status không hợp lệ: {status}",
                            IsSuccess = false
                        });
                    }
                }

                var result = await _invitationService.GetMyInvitationsAsync(currentUserId, invitationStatus);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitations for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lời mời",
                    IsSuccess = false
                });
            }
        }

        /// <summary>
        /// TourGuide chấp nhận invitation
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <param name="request">Thông tin chấp nhận</param>
        /// <returns>Kết quả chấp nhận</returns>
        [HttpPost("{invitationId}/accept")]
        [Authorize(Roles = "TourGuide")]
        public async Task<ActionResult<BaseResposeDto>> AcceptInvitation(
            Guid invitationId,
            [FromBody] AcceptInvitationDto request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không thể xác thực người dùng",
                        IsSuccess = false
                    });
                }

                // Validate request
                if (request.InvitationId != invitationId)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "InvitationId trong URL và body không khớp",
                        IsSuccess = false
                    });
                }

                if (!request.ConfirmUnderstanding)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Cần xác nhận đã hiểu yêu cầu tour",
                        IsSuccess = false
                    });
                }

                var result = await _invitationService.AcceptInvitationAsync(invitationId, currentUserId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting invitation {InvitationId} by user {UserId}",
                    invitationId, GetCurrentUserId());
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi chấp nhận lời mời",
                    IsSuccess = false
                });
            }
        }

        /// <summary>
        /// TourGuide từ chối invitation
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <param name="request">Thông tin từ chối</param>
        /// <returns>Kết quả từ chối</returns>
        [HttpPost("{invitationId}/reject")]
        [Authorize(Roles = "TourGuide")]
        public async Task<ActionResult<BaseResposeDto>> RejectInvitation(
            Guid invitationId,
            [FromBody] RejectInvitationDto request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không thể xác thực người dùng",
                        IsSuccess = false
                    });
                }

                // Validate request
                if (request.InvitationId != invitationId)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "InvitationId trong URL và body không khớp",
                        IsSuccess = false
                    });
                }

                var result = await _invitationService.RejectInvitationAsync(
                    invitationId,
                    currentUserId,
                    request.RejectionReason);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting invitation {InvitationId} by user {UserId}",
                    invitationId, GetCurrentUserId());
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi từ chối lời mời",
                    IsSuccess = false
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết một invitation
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <returns>Thông tin chi tiết invitation</returns>
        [HttpGet("{invitationId}")]
        [Authorize(Roles = "TourGuide,Admin,TourCompany")]
        public async Task<ActionResult<InvitationDetailsResponseDto>> GetInvitationDetails(Guid invitationId)
        {
            try
            {
                var result = await _invitationService.GetInvitationDetailsAsync(invitationId);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitation details {InvitationId}", invitationId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thông tin lời mời",
                    IsSuccess = false
                });
            }
        }

        /// <summary>
        /// Lấy danh sách invitations cho một TourDetails (admin/company view)
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Danh sách invitations</returns>
        [HttpGet("tourdetails/{tourDetailsId}")]
        [Authorize(Roles = "Admin,TourCompany")]
        public async Task<ActionResult<TourDetailsInvitationsResponseDto>> GetInvitationsForTourDetails(Guid tourDetailsId)
        {
            try
            {
                var result = await _invitationService.GetInvitationsForTourDetailsAsync(tourDetailsId);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitations for TourDetails {TourDetailsId}", tourDetailsId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lời mời",
                    IsSuccess = false
                });
            }
        }

        /// <summary>
        /// Validate xem có thể chấp nhận invitation không
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <returns>Kết quả validation</returns>
        [HttpGet("{invitationId}/validate-acceptance")]
        [Authorize(Roles = "TourGuide")]
        public async Task<ActionResult<BaseResposeDto>> ValidateInvitationAcceptance(Guid invitationId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không thể xác thực người dùng",
                        IsSuccess = false
                    });
                }

                var result = await _invitationService.ValidateInvitationAcceptanceAsync(invitationId, currentUserId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invitation acceptance {InvitationId}", invitationId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi validate lời mời",
                    IsSuccess = false
                });
            }
        }

        /// <summary>
        /// Helper method để lấy current user ID từ JWT token
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
