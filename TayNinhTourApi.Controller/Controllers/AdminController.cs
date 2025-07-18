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

                // Get ALL TourDetails to filter properly
                var response = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    pageIndex,
                    pageSize * 10, // Get more data to ensure we can filter properly
                    includeInactive: false);

                if (response.success && response.Data != null)
                {
                    // Filter TourDetails that need admin approval
                    // Include both Pending (0) and AwaitingAdminApproval (6) statuses
                    var pendingApprovalDetails = response.Data
                        .Where(td => 
                            td.Status.ToString() == TourDetailsStatus.Pending.ToString() ||
                            td.Status.ToString() == TourDetailsStatus.AwaitingAdminApproval.ToString())
                        .ToList();

                    _logger.LogInformation("Found {Count} TourDetails pending approval out of {Total} total", 
                        pendingApprovalDetails.Count, response.Data.Count);

                    // Log the statuses for debugging
                    foreach (var td in response.Data.Take(5)) // Log first 5 for debugging
                    {
                        _logger.LogInformation("TourDetails {Id}: Status = {Status}", td.Id, td.Status);
                    }

                    // Apply pagination to filtered results
                    var paginatedResults = pendingApprovalDetails
                        .Skip(pageIndex * pageSize)
                        .Take(pageSize)
                        .ToList();

                    var filteredResponse = new
                    {
                        StatusCode = 200,
                        Message = "Lấy danh sách TourDetails chờ duyệt thành công",
                        Data = paginatedResults,
                        TotalCount = pendingApprovalDetails.Count,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        TotalPages = (int)Math.Ceiling((double)pendingApprovalDetails.Count / pageSize),
                        FilteredFrom = response.Data.Count,
                        Debug = new
                        {
                            AllStatuses = response.Data.GroupBy(td => td.Status.ToString())
                                .ToDictionary(g => g.Key, g => g.Count()),
                            PendingCount = response.Data.Count(td => td.Status.ToString() == TourDetailsStatus.Pending.ToString()),
                            AwaitingAdminApprovalCount = response.Data.Count(td => td.Status.ToString() == TourDetailsStatus.AwaitingAdminApproval.ToString())
                        }
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
        /// Lấy tất cả TourDetails cho admin (không phân trang)
        /// </summary>
        /// <param name="includeInactive">Có bao gồm TourDetails không active không (default: false)</param>
        /// <param name="status">Lọc theo status cụ thể (optional)</param>
        /// <param name="keyword">Tìm kiếm theo title hoặc description (optional)</param>
        /// <returns>Danh sách tất cả TourDetails</returns>
        [HttpGet("tourdetails/all")]
        public async Task<IActionResult> GetAllTourDetails(
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? status = null,
            [FromQuery] string? keyword = null)
        {
            try
            {
                _logger.LogInformation("Admin getting all TourDetails - IncludeInactive: {IncludeInactive}, Status: {Status}, Keyword: {Keyword}",
                    includeInactive, status, keyword);

                // Get ALL TourDetails without pagination
                var response = await _tourDetailsService.GetTourDetailsPaginatedAsync(
                    0, // pageIndex
                    10000, // Large pageSize to get all
                    includeInactive: includeInactive);

                if (response.success && response.Data != null)
                {
                    var allTourDetails = response.Data;

                    // Apply status filter if provided
                    if (!string.IsNullOrEmpty(status))
                    {
                        if (Enum.TryParse<TourDetailsStatus>(status, true, out var statusEnum))
                        {
                            allTourDetails = allTourDetails
                                .Where(td => td.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            _logger.LogInformation("Applied status filter '{Status}': {Count} TourDetails found", 
                                status, allTourDetails.Count);
                        }
                        else
                        {
                            return BadRequest(new
                            {
                                StatusCode = 400,
                                Message = $"Status '{status}' không hợp lệ. Các giá trị hợp lệ: {string.Join(", ", Enum.GetNames<TourDetailsStatus>())}"
                            });
                        }
                    }

                    // Apply keyword search if provided
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        var searchKeyword = keyword.ToLower();
                        allTourDetails = allTourDetails
                            .Where(td => 
                                td.Title.ToLower().Contains(searchKeyword) ||
                                (!string.IsNullOrEmpty(td.Description) && td.Description.ToLower().Contains(searchKeyword)) ||
                                (!string.IsNullOrEmpty(td.SkillsRequired) && td.SkillsRequired.ToLower().Contains(searchKeyword)))
                            .ToList();

                        _logger.LogInformation("Applied keyword search '{Keyword}': {Count} TourDetails found", 
                            keyword, allTourDetails.Count);
                    }

                    // Group by status for statistics
                    var statusStatistics = allTourDetails
                        .GroupBy(td => td.Status.ToString())
                        .ToDictionary(g => g.Key, g => g.Count());

                    // Group by created date for recent activity
                    var today = DateTime.UtcNow.Date;
                    var thisWeek = DateTime.UtcNow.AddDays(-7);
                    var thisMonth = DateTime.UtcNow.AddDays(-30);

                    var recentActivity = new
                    {
                        TodayCreated = allTourDetails.Count(td => td.CreatedAt.Date == today),
                        ThisWeekCreated = allTourDetails.Count(td => td.CreatedAt >= thisWeek),
                        ThisMonthCreated = allTourDetails.Count(td => td.CreatedAt >= thisMonth)
                    };

                    var adminResponse = new
                    {
                        StatusCode = 200,
                        Message = "Lấy tất cả TourDetails thành công",
                        Data = allTourDetails,
                        TotalCount = allTourDetails.Count,
                        Statistics = new
                        {
                            StatusBreakdown = statusStatistics,
                            RecentActivity = recentActivity,
                            FilterApplied = new
                            {
                                IncludeInactive = includeInactive,
                                Status = status,
                                Keyword = keyword
                            }
                        },
                        AvailableStatuses = Enum.GetNames<TourDetailsStatus>(),
                        QueryInfo = new
                        {
                            ExecutedAt = DateTime.UtcNow,
                            TotalRecordsBeforeFilter = response.Data.Count,
                            TotalRecordsAfterFilter = allTourDetails.Count,
                            FiltersApplied = new List<string>
                            {
                                includeInactive ? "Include Inactive" : "Active Only",
                                !string.IsNullOrEmpty(status) ? $"Status: {status}" : null,
                                !string.IsNullOrEmpty(keyword) ? $"Keyword: {keyword}" : null
                            }.Where(f => f != null).ToList()
                        }
                    };

                    return Ok(adminResponse);
                }

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all TourDetails for admin");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách TourDetails",
                    Error = ex.Message
                });
            }
        }
    }
}
