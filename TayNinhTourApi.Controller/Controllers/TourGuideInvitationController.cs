using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Contexts;

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
        private readonly TayNinhTouApiDbContext _context;

        public TourGuideInvitationController(
            ITourGuideInvitationService invitationService,
            ILogger<TourGuideInvitationController> logger,
            TayNinhTouApiDbContext context)
        {
            _invitationService = invitationService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách invitations của TourGuide hiện tại
        /// </summary>
        /// <param name="status">Lọc theo status (optional)</param>
        /// <returns>Danh sách invitations</returns>
        [HttpGet("my-invitations")]
        [Authorize(Roles = "Tour Guide")]
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
                        success = false
                    });
                }

                // Tìm TourGuide record từ User ID
                _logger.LogInformation("Looking for TourGuide with UserId: {UserId}", currentUserId);

                // Debug: Kiểm tra tất cả TourGuides
                var allTourGuides = await _context.TourGuides.ToListAsync();
                _logger.LogInformation("Total TourGuides in database: {Count}", allTourGuides.Count);
                foreach (var tg in allTourGuides)
                {
                    _logger.LogInformation("TourGuide: Id={Id}, UserId={UserId}, IsActive={IsActive}, FullName={FullName}",
                        tg.Id, tg.UserId, tg.IsActive, tg.FullName);
                }

                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserId && tg.IsActive);

                if (tourGuide == null)
                {
                    _logger.LogWarning("TourGuide not found for UserId: {UserId}", currentUserId);
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy thông tin TourGuide cho UserId: {currentUserId}",
                        success = false
                    });
                }

                _logger.LogInformation("Found TourGuide: Id={GuideId}, UserId={UserId}, FullName={FullName}",
                    tourGuide.Id, tourGuide.UserId, tourGuide.FullName);

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
                            success = false
                        });
                    }
                }

                // Sử dụng TourGuide ID thay vì User ID
                var result = await _invitationService.GetMyInvitationsAsync(tourGuide.Id, invitationStatus);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitations for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lời mời",
                    success = false
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
        [Authorize(Roles = "Tour Guide")]
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
                        success = false
                    });
                }

                // Validate request
                if (request.InvitationId != invitationId)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "InvitationId trong URL và body không khớp",
                        success = false
                    });
                }

                if (!request.ConfirmUnderstanding)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Cần xác nhận đã hiểu yêu cầu tour",
                        success = false
                    });
                }

                // Tìm TourGuide record từ User ID
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserId && tg.IsActive);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin TourGuide",
                        success = false
                    });
                }

                // Sử dụng TourGuide ID thay vì User ID
                var result = await _invitationService.AcceptInvitationAsync(invitationId, tourGuide.Id);
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
                    success = false
                });
            }
        }
        /// <summary>
        /// Debug method để manually update TourOperation với guide info
        /// </summary>
        /// <param name="invitationId">ID của invitation đã được accept</param>
        /// <returns>Kết quả debug</returns>
        [HttpPost("{invitationId}/debug-update-tour-operation")]
        [Authorize(Roles = "Tour Guide,Admin")]
        public async Task<ActionResult<BaseResposeDto>> DebugUpdateTourOperation(Guid invitationId)
        {
            try
            {
                var result = await _invitationService.DebugUpdateTourOperationAsync(invitationId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug update tour operation {InvitationId}", invitationId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Đã xảy ra lỗi khi debug update tour operation",
                    success = false
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
        [Authorize(Roles = "Tour Guide")]
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
                        success = false
                    });
                }

                // Validate request
                if (request.InvitationId != invitationId)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "InvitationId trong URL và body không khớp",
                        success = false
                    });
                }

                // Tìm TourGuide record từ User ID
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserId && tg.IsActive);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin TourGuide",
                        success = false
                    });
                }

                // Sử dụng TourGuide ID thay vì User ID
                var result = await _invitationService.RejectInvitationAsync(
                    invitationId,
                    tourGuide.Id,
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
                    success = false
                });
            }
        }

        /// <summary>
        /// DEBUG: Kiểm tra TourGuide records cho User ID
        /// </summary>
        [HttpGet("debug/tourguide/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DebugTourGuideMapping(Guid userId)
        {
            try
            {
                var tourGuides = await _context.TourGuides
                    .Where(tg => tg.UserId == userId)
                    .Select(tg => new
                    {
                        tg.Id,
                        tg.UserId,
                        tg.FullName,
                        tg.Email,
                        tg.Skills,
                        tg.IsActive,
                        tg.CreatedAt
                    })
                    .ToListAsync();

                var invitations = await _context.TourGuideInvitations
                    .Where(i => tourGuides.Select(tg => tg.Id).Contains(i.GuideId))
                    .Select(i => new
                    {
                        i.Id,
                        i.GuideId,
                        i.TourDetailsId,
                        i.Status,
                        i.InvitedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    UserId = userId,
                    TourGuides = tourGuides,
                    Invitations = invitations,
                    Message = $"Found {tourGuides.Count} TourGuide records and {invitations.Count} invitations"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debugging TourGuide mapping for {UserId}", userId);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết một invitation
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <returns>Thông tin chi tiết invitation</returns>
        [HttpGet("{invitationId}")]
        [Authorize(Roles = "Tour Guide,Admin,Tour Company")]
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
                    success = false
                });
            }
        }

        /// <summary>
        /// Lấy danh sách invitations cho một TourDetails (admin/company view)
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Danh sách invitations</returns>
        [HttpGet("tourdetails/{tourDetailsId}")]
        [Authorize(Roles = "Admin,Tour Company")]
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
                    success = false
                });
            }
        }

        /// <summary>
        /// Validate xem có thể chấp nhận invitation không
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <returns>Kết quả validation</returns>
        [HttpGet("{invitationId}/validate-acceptance")]
        [Authorize(Roles = "Tour Guide")]
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
                        success = false
                    });
                }

                // Tìm TourGuide record từ User ID
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserId && tg.IsActive);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin TourGuide",
                        success = false
                    });
                }

                // Sử dụng TourGuide ID thay vì User ID
                var result = await _invitationService.ValidateInvitationAcceptanceAsync(invitationId, tourGuide.Id);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invitation acceptance {InvitationId}", invitationId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi validate lời mời",
                    success = false
                });
            }
        }

        /// <summary>
        /// Fix TourDetails status cho các case đã có guide accept nhưng status vẫn AwaitingGuideAssignment
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails cần fix</param>
        /// <returns>Kết quả fix status</returns>
        [HttpPost("fix-tourdetails-status/{tourDetailsId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResposeDto>> FixTourDetailsStatus(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Admin fixing TourDetails {TourDetailsId} status", tourDetailsId);
                var result = await _invitationService.FixTourDetailsStatusAsync(tourDetailsId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing TourDetails {TourDetailsId} status", tourDetailsId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi fix TourDetails status",
                    success = false
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
