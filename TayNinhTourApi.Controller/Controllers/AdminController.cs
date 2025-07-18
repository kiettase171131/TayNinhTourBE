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

                // Get TourDetails with Pending status (chờ admin duyệt)
                var response = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    pageIndex,
                    pageSize,
                    tourTemplateId: null,
                    titleFilter: null,
                    includeInactive: false,
                    statusFilter: TourDetailsStatus.Pending);

                if (response.success && response.Data != null)
                {
                    var filteredResponse = new
                    {
                        StatusCode = 200,
                        Message = "Lấy danh sách TourDetails chờ duyệt thành công",
                        Data = response.Data,
                        TotalCount = response.TotalCount,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        TotalPages = response.TotalPages
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
        /// Lấy tất cả danh sách TourDetails với mọi status (cho admin quản lý)
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (0-based, default: 0)</param>
        /// <param name="pageSize">Kích thước trang (default: 10)</param>
        /// <param name="statusFilter">Lọc theo status cụ thể (optional)</param>
        /// <param name="titleFilter">Lọc theo title (optional)</param>
        /// <param name="includeInactive">Bao gồm các records không active (default: false)</param>
        /// <returns>Danh sách tất cả TourDetails</returns>
        [HttpGet("tourdetails/all")]
        public async Task<IActionResult> GetAllTourDetails(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 20,
            [FromQuery] TourDetailsStatus? statusFilter = null,
            [FromQuery] string? titleFilter = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Admin getting all TourDetails - Page: {PageIndex}, Size: {PageSize}, StatusFilter: {StatusFilter}",
                    pageIndex, pageSize, statusFilter);

                // Get all TourDetails với filter tùy chọn
                var response = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    pageIndex,
                    pageSize,
                    tourTemplateId: null,
                    titleFilter: titleFilter,
                    includeInactive: includeInactive,
                    statusFilter: statusFilter);

                if (response.success && response.Data != null)
                {
                    var filteredResponse = new
                    {
                        StatusCode = 200,
                        Message = "Lấy danh sách tất cả TourDetails thành công",
                        Data = response.Data,
                        TotalCount = response.TotalCount,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        TotalPages = response.TotalPages,
                        Filters = new
                        {
                            StatusFilter = statusFilter?.ToString(),
                            TitleFilter = titleFilter,
                            IncludeInactive = includeInactive
                        }
                    };

                    return Ok(filteredResponse);
                }

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all TourDetails");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách TourDetails"
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

                if (!allTourDetailsResponse.success || allTourDetailsResponse.Data == null)
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
                        RequireAdminApproval = tourDetails.Count(td => td.Status.ToString() == TourDetailsStatus.Pending.ToString()),
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
                if (!tourDetailsResponse.success)
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
                    GuideAssignmentStatus = assignmentStatusResponse.success ? "Assignment status available" : null,
                    Invitations = invitationsResponse.success ? invitationsResponse.Invitations : null
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

        /// <summary>
        /// Fix TourDetails status manually - TEMPORARY for debugging
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Kết quả fix</returns>
        [HttpPost("tourdetails/{tourDetailsId:guid}/fix-status")]
        public async Task<IActionResult> FixTourDetailsStatus([FromRoute] Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Admin manually fixing TourDetails {TourDetailsId} status", tourDetailsId);

                // Get TourDetails
                var tourDetailsResponse = await _tourDetailsService.GetTourDetailByIdAsync(tourDetailsId);
                if (!tourDetailsResponse.success)
                {
                    return StatusCode(tourDetailsResponse.StatusCode, new
                    {
                        StatusCode = tourDetailsResponse.StatusCode,
                        Message = "TourDetails không tồn tại"
                    });
                }

                // Check if has accepted invitation
                var invitationsResponse = await _invitationService.GetInvitationsForTourDetailsAsync(tourDetailsId);
                if (!invitationsResponse.success)
                {
                    return StatusCode(500, new
                    {
                        StatusCode = 500,
                        Message = "Không thể kiểm tra invitations"
                    });
                }

                var hasAcceptedInvitation = invitationsResponse.Invitations?.Any(i => i.Status == "Accepted") ?? false;
                if (!hasAcceptedInvitation)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "TourDetails này chưa có guide nào accept invitation"
                    });
                }

                // Use invitation service to fix status
                var result = await _invitationService.FixTourDetailsStatusAsync(tourDetailsId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing TourDetails {TourDetailsId} status", tourDetailsId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi fix TourDetails status"
                });
            }
        }

        /// <summary>
        /// Lấy thống kê chi tiết theo từng status của TourDetails
        /// </summary>
        /// <returns>Thống kê chi tiết và danh sách samples theo status</returns>
        [HttpGet("tourdetails/status-breakdown")]
        public async Task<IActionResult> GetTourDetailsStatusBreakdown()
        {
            try
            {
                _logger.LogInformation("Admin getting TourDetails status breakdown");

                // Get all TourDetails without pagination for complete statistics
                var allResponse = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    0, 10000, // Large page size to get all
                    includeInactive: true);

                if (!allResponse.success || allResponse.Data == null)
                {
                    return StatusCode(500, new
                    {
                        StatusCode = 500,
                        Message = "Không thể lấy dữ liệu TourDetails"
                    });
                }

                var allTourDetails = allResponse.Data;

                // Group by status and create breakdown
                var statusBreakdown = new
                {
                    Total = allTourDetails.Count,
                    StatusCounts = new
                    {
                        Pending = allTourDetails.Count(td => td.Status == TourDetailsStatus.Pending),
                        Approved = allTourDetails.Count(td => td.Status == TourDetailsStatus.Approved),
                        Rejected = allTourDetails.Count(td => td.Status == TourDetailsStatus.Rejected),
                        AwaitingGuideAssignment = allTourDetails.Count(td => td.Status == TourDetailsStatus.AwaitingGuideAssignment),
                        WaitToPublic = allTourDetails.Count(td => td.Status == TourDetailsStatus.WaitToPublic),
                        Public = allTourDetails.Count(td => td.Status == TourDetailsStatus.Public),
                        Cancelled = allTourDetails.Count(td => td.Status == TourDetailsStatus.Cancelled),
                        Suspended = allTourDetails.Count(td => td.Status == TourDetailsStatus.Suspended)
                    },
                    StatusSamples = new
                    {
                        Pending = allTourDetails
                            .Where(td => td.Status == TourDetailsStatus.Pending)
                            .Take(5)
                            .Select(td => new { td.Id, td.Title, td.CreatedAt, td.Status }),
                        Approved = allTourDetails
                            .Where(td => td.Status == TourDetailsStatus.Approved)
                            .Take(5)
                            .Select(td => new { td.Id, td.Title, td.CreatedAt, td.Status }),
                        Rejected = allTourDetails
                            .Where(td => td.Status == TourDetailsStatus.Rejected)
                            .Take(5)
                            .Select(td => new { td.Id, td.Title, td.CreatedAt, td.Status, td.CommentApproved }),
                        AwaitingGuideAssignment = allTourDetails
                            .Where(td => td.Status == TourDetailsStatus.AwaitingGuideAssignment)
                            .Take(5)
                            .Select(td => new { td.Id, td.Title, td.CreatedAt, td.Status }),
                        WaitToPublic = allTourDetails
                            .Where(td => td.Status == TourDetailsStatus.WaitToPublic)
                            .Take(5)
                            .Select(td => new { td.Id, td.Title, td.CreatedAt, td.Status }),
                        Public = allTourDetails
                            .Where(td => td.Status == TourDetailsStatus.Public)
                            .Take(5)
                            .Select(td => new { td.Id, td.Title, td.CreatedAt, td.Status })
                    },
                    RecentActivity = new
                    {
                        RecentlyCreated = allTourDetails
                            .OrderByDescending(td => td.CreatedAt)
                            .Take(10)
                            .Select(td => new { td.Id, td.Title, td.CreatedAt, td.Status }),
                        RecentlyApproved = allTourDetails
                            .Where(td => td.Status == TourDetailsStatus.Approved)
                            .OrderByDescending(td => td.UpdatedAt ?? td.CreatedAt)
                            .Take(5)
                            .Select(td => new { td.Id, td.Title, td.UpdatedAt, td.Status }),
                        RecentlyRejected = allTourDetails
                            .Where(td => td.Status == TourDetailsStatus.Rejected)
                            .OrderByDescending(td => td.UpdatedAt ?? td.CreatedAt)
                            .Take(5)
                            .Select(td => new { td.Id, td.Title, td.UpdatedAt, td.Status, td.CommentApproved })
                    }
                };

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Lấy thống kê chi tiết TourDetails thành công",
                    Data = statusBreakdown,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TourDetails status breakdown");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thống kê TourDetails"
                });
            }
        }
    }
}
