using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho các chức năng admin
    /// Quản lý approval workflow và admin operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ITourDetailsService _tourDetailsService;
        private readonly ITourGuideInvitationService _invitationService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ITourDetailsService tourDetailsService,
            ITourGuideInvitationService invitationService,
            ILogger<AdminController> logger)
        {
            _tourDetailsService = tourDetailsService;
            _invitationService = invitationService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách TourDetails đang chờ admin duyệt
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (0-based, default: 0)</param>
        /// <param name="pageSize">Kích thước trang (default: 10)</param>
        /// <returns>Danh sách TourDetails chờ duyệt</returns>
        [HttpGet("tourdetails/pending-approval")]
        public async Task<IActionResult> GetTourDetailsPendingApproval(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Admin getting TourDetails pending approval - Page: {PageIndex}, Size: {PageSize}",
                    pageIndex, pageSize);

                // Get TourDetails with AwaitingAdminApproval status
                var response = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    pageIndex,
                    pageSize,
                    includeInactive: false);

                if (response.IsSuccess && response.Data != null)
                {
                    // Filter only AwaitingAdminApproval status
                    var pendingApprovalDetails = response.Data
                        .Where(td => td.Status.ToString() == TourDetailsStatus.AwaitingAdminApproval.ToString())
                        .ToList();

                    var filteredResponse = new
                    {
                        StatusCode = 200,
                        Message = "Lấy danh sách TourDetails chờ duyệt thành công",
                        Data = pendingApprovalDetails,
                        TotalCount = pendingApprovalDetails.Count,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        TotalPages = (int)Math.Ceiling((double)pendingApprovalDetails.Count / pageSize)
                    };

                    return Ok(filteredResponse);
                }

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TourDetails pending approval");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách TourDetails chờ duyệt"
                });
            }
        }

        /// <summary>
        /// Admin duyệt TourDetails
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="request">Thông tin duyệt</param>
        /// <returns>Kết quả duyệt</returns>
        [HttpPost("tourdetails/{tourDetailsId:guid}/approve")]
        public async Task<IActionResult> ApproveTourDetails(
            [FromRoute] Guid tourDetailsId,
            [FromBody] RequestApprovalTourDetailDto request)
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

                var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("Admin {AdminId} approving TourDetails {TourDetailsId}",
                    adminId, tourDetailsId);

                // Set approval to true
                request.IsApproved = true;

                var response = await _tourDetailsService.ApproveRejectTourDetailAsync(
                    tourDetailsId, request, adminId);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving TourDetails {TourDetailsId}", tourDetailsId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi duyệt TourDetails"
                });
            }
        }

        /// <summary>
        /// Admin từ chối TourDetails
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="request">Thông tin từ chối</param>
        /// <returns>Kết quả từ chối</returns>
        [HttpPost("tourdetails/{tourDetailsId:guid}/reject")]
        public async Task<IActionResult> RejectTourDetails(
            [FromRoute] Guid tourDetailsId,
            [FromBody] RequestApprovalTourDetailDto request)
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

                // Validate rejection reason is provided
                if (string.IsNullOrWhiteSpace(request.Comment))
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Lý do từ chối là bắt buộc"
                    });
                }

                var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                _logger.LogInformation("Admin {AdminId} rejecting TourDetails {TourDetailsId}",
                    adminId, tourDetailsId);

                // Set approval to false
                request.IsApproved = false;

                var response = await _tourDetailsService.ApproveRejectTourDetailAsync(
                    tourDetailsId, request, adminId);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting TourDetails {TourDetailsId}", tourDetailsId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi từ chối TourDetails"
                });
            }
        }

        /// <summary>
        /// Lấy thống kê tổng quan cho admin dashboard
        /// </summary>
        /// <returns>Thống kê admin dashboard</returns>
        [HttpGet("dashboard/statistics")]
        public async Task<IActionResult> GetAdminDashboardStatistics()
        {
            try
            {
                _logger.LogInformation("Getting admin dashboard statistics");

                // Get all TourDetails for statistics
                var allTourDetailsResponse = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    0, 1000, includeInactive: true); // Get large page to get all

                if (!allTourDetailsResponse.IsSuccess || allTourDetailsResponse.Data == null)
                {
                    return StatusCode(500, new
                    {
                        StatusCode = 500,
                        Message = "Không thể lấy dữ liệu thống kê"
                    });
                }

                var tourDetails = allTourDetailsResponse.Data;

                var statistics = new
                {
                    TourDetails = new
                    {
                        Total = tourDetails.Count,
                        Pending = tourDetails.Count(td => td.Status.ToString() == TourDetailsStatus.Pending.ToString()),
                        AwaitingGuideAssignment = tourDetails.Count(td => td.Status.ToString() == TourDetailsStatus.AwaitingGuideAssignment.ToString()),
                        AwaitingAdminApproval = tourDetails.Count(td => td.Status.ToString() == TourDetailsStatus.AwaitingAdminApproval.ToString()),
                        Approved = tourDetails.Count(td => td.Status.ToString() == TourDetailsStatus.Approved.ToString()),
                        Rejected = tourDetails.Count(td => td.Status.ToString() == TourDetailsStatus.Rejected.ToString()),
                        Cancelled = tourDetails.Count(td => td.Status.ToString() == TourDetailsStatus.Cancelled.ToString())
                    },
                    RecentActivity = new
                    {
                        TodayCreated = tourDetails.Count(td => td.CreatedAt.Date == DateTime.UtcNow.Date),
                        ThisWeekCreated = tourDetails.Count(td => td.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
                        ThisMonthCreated = tourDetails.Count(td => td.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                    },
                    PendingActions = new
                    {
                        RequireAdminApproval = tourDetails.Count(td => td.Status.ToString() == TourDetailsStatus.AwaitingAdminApproval.ToString()),
                        AwaitingGuideAssignment = tourDetails.Count(td => td.Status.ToString() == TourDetailsStatus.AwaitingGuideAssignment.ToString())
                    }
                };

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Lấy thống kê admin thành công",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard statistics");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thống kê admin"
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết TourDetails cho admin review
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Thông tin chi tiết để admin review</returns>
        [HttpGet("tourdetails/{tourDetailsId:guid}/review")]
        public async Task<IActionResult> GetTourDetailsForReview(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Admin getting TourDetails {TourDetailsId} for review", tourDetailsId);

                // Get TourDetails details
                var tourDetailsResponse = await _tourDetailsService.GetTourDetailByIdAsync(tourDetailsId);
                if (!tourDetailsResponse.IsSuccess)
                {
                    return StatusCode(tourDetailsResponse.StatusCode, tourDetailsResponse);
                }

                // Get guide assignment status
                var assignmentStatusResponse = await _tourDetailsService.GetGuideAssignmentStatusAsync(tourDetailsId);

                // Get invitations for this TourDetails
                var invitationsResponse = await _invitationService.GetInvitationsForTourDetailsAsync(tourDetailsId);

                var reviewData = new
                {
                    TourDetails = tourDetailsResponse.Data,
                    GuideAssignmentStatus = assignmentStatusResponse.IsSuccess ? "Assignment status available" : null,
                    Invitations = invitationsResponse.IsSuccess ? invitationsResponse.Invitations : null
                };

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin TourDetails để review thành công",
                    Data = reviewData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TourDetails {TourDetailsId} for review", tourDetailsId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thông tin TourDetails để review"
                });
            }
        }
    }
}
