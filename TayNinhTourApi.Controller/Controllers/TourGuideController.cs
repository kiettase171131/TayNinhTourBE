using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourGuide;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourGuide;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho HDV (Hướng dẫn viên) quản lý tour operations
    /// Cung cấp các chức năng: check-in khách, track timeline, báo cáo sự cố, thông báo khách
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Guide")]
    public class TourGuideController : ControllerBase
    {
        private readonly TayNinhTouApiDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IQRCodeService _qrCodeService;
        private readonly ITourGuideTimelineService _timelineService;
        private readonly ILogger<TourGuideController> _logger;

        public TourGuideController(
            TayNinhTouApiDbContext context,
            INotificationService notificationService,
            IQRCodeService qrCodeService,
            ITourGuideTimelineService timelineService,
            ILogger<TourGuideController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _qrCodeService = qrCodeService;
            _timelineService = timelineService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tours đang active của HDV hiện tại
        /// </summary>
        /// <returns>Danh sách TourOperation đang active</returns>
        [HttpGet("my-active-tours")]
        public async Task<IActionResult> GetMyActiveTours()
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                // Tìm TourGuide record từ UserId
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserObject.UserId);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                // Lấy tourDetails đang active của HDV này (có TourSlot status InProgress)
                var activeTours = await _context.TourOperations
                    .Include(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                    .Include(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                    .Include(to => to.TourBookings)
                    .Where(to => to.TourGuideId == tourGuide.Id &&
                                to.IsActive &&
                                to.TourDetails.Status == DataAccessLayer.Enums.TourDetailsStatus.Public &&
                                to.TourDetails.AssignedSlots.Any(slot =>
                                    slot.Status == DataAccessLayer.Enums.TourSlotStatus.InProgress))
                    .Select(to => new
                    {
                        to.Id,
                        TourDetailsId = to.TourDetails.Id,
                        to.TourDetails.Title,
                        to.TourDetails.Description,
                        StartDate = to.TourDetails.AssignedSlots.FirstOrDefault() != null ?
                                   to.TourDetails.AssignedSlots.FirstOrDefault()!.TourDate.ToDateTime(TimeOnly.MinValue) :
                                   DateTime.Today,
                        EndDate = to.TourDetails.AssignedSlots.FirstOrDefault() != null ?
                                 to.TourDetails.AssignedSlots.FirstOrDefault()!.TourDate.ToDateTime(TimeOnly.MaxValue) :
                                 DateTime.Today.AddHours(23).AddMinutes(59),
                        to.Price,
                        to.MaxGuests,
                        to.CurrentBookings,
                        to.Status,
                        TourTemplate = new
                        {
                            Id = to.TourDetails.TourTemplate.Id,
                            to.TourDetails.TourTemplate.Title,
                            to.TourDetails.TourTemplate.StartLocation,
                            to.TourDetails.TourTemplate.EndLocation,
                            Description = to.TourDetails.Description ?? ""
                        },
                        BookingsCount = to.TourBookings.Count(tb => tb.Status == DataAccessLayer.Enums.BookingStatus.Confirmed),
                        CheckedInCount = to.TourBookings.Count(tb => tb.Status == DataAccessLayer.Enums.BookingStatus.Confirmed && tb.IsCheckedIn)
                    })
                    .OrderBy(to => to.StartDate)
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách tours thành công",
                    Data = activeTours,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active tours for tour guide");
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi lấy danh sách tours"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bookings cho tour cụ thể để HDV check-in khách
        /// </summary>
        /// <param name="operationId">ID của TourOperation</param>
        /// <returns>Danh sách TourBooking</returns>
        [HttpGet("tour/{operationId:guid}/bookings")]
        public async Task<IActionResult> GetTourBookings(Guid operationId)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                // Verify HDV có quyền access tour này không
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserObject.UserId);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var tourOperation = await _context.TourOperations
                    .FirstOrDefaultAsync(to => to.Id == operationId && to.TourGuideId == tourGuide.Id);

                if (tourOperation == null)
                {
                    return Forbid("HDV không có quyền truy cập tour này");
                }

                // Lấy danh sách bookings
                var bookings = await _context.TourBookings
                    .Include(tb => tb.User)
                    .Where(tb => tb.TourOperationId == operationId &&
                                tb.Status == DataAccessLayer.Enums.BookingStatus.Confirmed)
                    .Select(tb => new
                    {
                        tb.Id,
                        tb.BookingCode,
                        tb.ContactName,
                        tb.ContactPhone,
                        tb.ContactEmail,
                        tb.NumberOfGuests,
                        tb.TotalPrice,
                        tb.IsCheckedIn,
                        tb.CheckInTime,
                        tb.CheckInNotes,
                        tb.QRCodeData,
                        CustomerName = tb.User.Name
                    })
                    .OrderBy(tb => tb.ContactName)
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách bookings thành công",
                    Data = bookings,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour bookings for operation {OperationId}", operationId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi lấy danh sách bookings"
                });
            }
        }

        /// <summary>
        /// [LEGACY] Lấy timeline items cho tour cụ thể - Backward compatibility
        /// Sử dụng API mới: GET /tour-slot/{tourSlotId}/timeline
        /// </summary>
        /// <param name="operationId">ID của TourOperation</param>
        /// <returns>Danh sách TimelineItem</returns>
        [HttpGet("tour/{operationId:guid}/timeline")]
        public async Task<IActionResult> GetTourTimeline(Guid operationId)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                // Verify HDV có quyền access tour này không
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserObject.UserId);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var tourOperation = await _context.TourOperations
                    .Include(to => to.TourDetails)
                    .FirstOrDefaultAsync(to => to.Id == operationId && to.TourGuideId == tourGuide.Id);

                if (tourOperation == null)
                {
                    return Forbid("HDV không có quyền truy cập tour này");
                }

                // Lấy timeline items
                var timelineItems = await _context.TimelineItems
                    .Include(ti => ti.SpecialtyShop)
                    .Where(ti => ti.TourDetailsId == tourOperation.TourDetailsId)
                    .OrderBy(ti => ti.SortOrder)
                    .Select(ti => new
                    {
                        ti.Id,
                        ti.CheckInTime,
                        ti.Activity,
                        ti.SortOrder,
                        ti.IsCompleted,
                        ti.CompletedAt,
                        ti.CompletionNotes,
                        SpecialtyShop = ti.SpecialtyShop != null ? new
                        {
                            ti.SpecialtyShop.Id,
                            ti.SpecialtyShop.ShopName,
                            ti.SpecialtyShop.Address
                        } : null
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Lấy timeline thành công",
                    Data = timelineItems,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline for tour {OperationId}", operationId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi lấy timeline"
                });
            }
        }

        /// <summary>
        /// Check-in khách hàng qua QR code
        /// </summary>
        /// <param name="bookingId">ID của TourBooking</param>
        /// <param name="request">Thông tin check-in</param>
        /// <returns>Kết quả check-in</returns>
        [HttpPost("checkin/{bookingId:guid}")]
        public async Task<IActionResult> CheckInGuest(Guid bookingId, [FromBody] CheckInGuestRequest request)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                // Verify HDV có quyền check-in booking này không
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserObject.UserId);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var booking = await _context.TourBookings
                    .Include(tb => tb.TourOperation)
                    .FirstOrDefaultAsync(tb => tb.Id == bookingId &&
                                              tb.TourOperation.TourGuideId == tourGuide.Id &&
                                              tb.Status == DataAccessLayer.Enums.BookingStatus.Confirmed);

                if (booking == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking hoặc HDV không có quyền truy cập"
                    });
                }

                // Kiểm tra đã check-in chưa
                if (booking.IsCheckedIn)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Khách hàng đã check-in trước đó"
                    });
                }

                // Validate QR code nếu có
                if (!string.IsNullOrEmpty(request.QRCodeData) && !string.IsNullOrEmpty(booking.QRCodeData))
                {
                    if (request.QRCodeData != booking.QRCodeData)
                    {
                        return BadRequest(new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = "QR code không hợp lệ"
                        });
                    }
                }

                // Update check-in status
                booking.IsCheckedIn = true;
                booking.CheckInTime = DateTime.UtcNow;
                booking.CheckInNotes = request.Notes;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.UpdatedById = currentUserObject.UserId;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Check-in thành công",
                    Data = new
                    {
                        booking.Id,
                        booking.BookingCode,
                        booking.ContactName,
                        booking.IsCheckedIn,
                        booking.CheckInTime,
                        booking.CheckInNotes
                    },
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in guest for booking {BookingId}", bookingId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi check-in khách"
                });
            }
        }

        /// <summary>
        /// [LEGACY] Hoàn thành timeline item và gửi notification cho guests - Backward compatibility
        /// Sử dụng API mới: POST /tour-slot/{tourSlotId}/timeline/{timelineItemId}/complete
        /// </summary>
        /// <param name="timelineId">ID của TimelineItem</param>
        /// <param name="request">Thông tin hoàn thành</param>
        /// <returns>Kết quả hoàn thành</returns>
        [HttpPost("timeline/{timelineId:guid}/complete")]
        public async Task<IActionResult> CompleteTimelineItem(Guid timelineId, [FromBody] CompleteTimelineRequest request)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                // Verify HDV có quyền complete timeline này không
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserObject.UserId);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var timelineItem = await _context.TimelineItems
                    .Include(ti => ti.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .FirstOrDefaultAsync(ti => ti.Id == timelineId &&
                                              ti.TourDetails.TourOperation.TourGuideId == tourGuide.Id);

                if (timelineItem == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy timeline item hoặc HDV không có quyền truy cập"
                    });
                }

                // Kiểm tra đã complete chưa
                if (timelineItem.IsCompleted)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Timeline item đã được hoàn thành trước đó"
                    });
                }

                // Kiểm tra timeline order - phải complete theo thứ tự
                var previousIncompleteItems = await _context.TimelineItems
                    .Where(ti => ti.TourDetailsId == timelineItem.TourDetailsId &&
                                ti.SortOrder < timelineItem.SortOrder &&
                                !ti.IsCompleted)
                    .CountAsync();

                if (previousIncompleteItems > 0)
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Phải hoàn thành các mục timeline trước đó theo thứ tự"
                    });
                }

                // Use execution strategy to handle transactions properly with MySQL
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Update completion status
                        timelineItem.IsCompleted = true;
                        timelineItem.CompletedAt = DateTime.UtcNow;
                        timelineItem.CompletionNotes = request.Notes;
                        timelineItem.UpdatedAt = DateTime.UtcNow;
                        timelineItem.UpdatedById = currentUserObject.UserId;

                        await _context.SaveChangesAsync();

                        // Gửi notification cho tất cả guests trong tour
                        await NotifyGuestsAboutTimelineProgress(timelineItem.TourDetails.TourOperation.Id, timelineItem.Activity);

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Hoàn thành timeline item thành công",
                    Data = new
                    {
                        timelineItem.Id,
                        timelineItem.Activity,
                        timelineItem.IsCompleted,
                        timelineItem.CompletedAt,
                        timelineItem.CompletionNotes
                    },
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing timeline item {TimelineId}", timelineId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi hoàn thành timeline item"
                });
            }
        }

        /// <summary>
        /// Helper method để gửi notification cho guests về timeline progress
        /// </summary>
        private async Task NotifyGuestsAboutTimelineProgress(Guid tourOperationId, string activity)
        {
            try
            {
                var guestUserIds = await _context.TourBookings
                    .Where(tb => tb.TourOperationId == tourOperationId &&
                                tb.Status == DataAccessLayer.Enums.BookingStatus.Confirmed &&
                                tb.IsCheckedIn)
                    .Select(tb => tb.UserId)
                    .ToListAsync();

                foreach (var userId in guestUserIds)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = userId,
                        Title = "🎯 Cập nhật lịch trình tour",
                        Message = $"Đoàn đã hoàn thành: {activity}",
                        Type = DataAccessLayer.Enums.NotificationType.Tour,
                        Priority = DataAccessLayer.Enums.NotificationPriority.Normal,
                        Icon = "🎯"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending timeline progress notifications for tour {TourOperationId}", tourOperationId);
            }
        }

        // ===== NEW TIMELINE PROGRESS APIs =====

        /// <summary>
        /// [NEW] Lấy timeline với progress cho tour slot cụ thể
        /// </summary>
        /// <param name="tourSlotId">ID của TourSlot</param>
        /// <param name="includeInactive">Bao gồm timeline items không active</param>
        /// <param name="includeShopInfo">Bao gồm thông tin specialty shop</param>
        /// <returns>Timeline với progress information</returns>
        [HttpGet("tour-slot/{tourSlotId:guid}/timeline")]
        public async Task<IActionResult> GetTourSlotTimeline(
            Guid tourSlotId,
            [FromQuery] bool includeInactive = false,
            [FromQuery] bool includeShopInfo = true)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var response = await _timelineService.GetTimelineWithProgressAsync(
                    tourSlotId,
                    currentUserObject.UserId,
                    includeInactive,
                    includeShopInfo);

                return Ok(new ApiResponse<TimelineProgressResponse>
                {
                    StatusCode = 200,
                    Message = "Lấy timeline với progress thành công",
                    Data = response,
                    success = true
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline with progress for tour slot {TourSlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi lấy timeline với progress"
                });
            }
        }

        /// <summary>
        /// [NEW] Hoàn thành timeline item cho tour slot cụ thể
        /// </summary>
        /// <param name="tourSlotId">ID của TourSlot</param>
        /// <param name="timelineItemId">ID của TimelineItem</param>
        /// <param name="request">Thông tin hoàn thành</param>
        /// <returns>Kết quả hoàn thành</returns>
        [HttpPost("tour-slot/{tourSlotId:guid}/timeline/{timelineItemId:guid}/complete")]
        public async Task<IActionResult> CompleteTimelineItemForSlot(
            Guid tourSlotId,
            Guid timelineItemId,
            [FromBody] CompleteTimelineRequest request)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var response = await _timelineService.CompleteTimelineItemAsync(
                    tourSlotId,
                    timelineItemId,
                    request,
                    currentUserObject.UserId);

                return Ok(new ApiResponse<CompleteTimelineResponse>
                {
                    StatusCode = 200,
                    Message = response.Message,
                    Data = response,
                    success = response.Success
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing timeline item {TimelineItemId} for tour slot {TourSlotId}",
                    timelineItemId, tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi hoàn thành timeline item"
                });
            }
        }

        /// <summary>
        /// [NEW] Hoàn thành nhiều timeline items cùng lúc
        /// </summary>
        /// <param name="request">Bulk completion request</param>
        /// <returns>Kết quả bulk operation</returns>
        [HttpPost("timeline/bulk-complete")]
        public async Task<IActionResult> BulkCompleteTimelineItems([FromBody] BulkCompleteTimelineRequest request)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var response = await _timelineService.BulkCompleteTimelineItemsAsync(request, currentUserObject.UserId);

                return Ok(new ApiResponse<BulkTimelineResponse>
                {
                    StatusCode = 200,
                    Message = response.Message,
                    Data = response,
                    success = response.IsFullySuccessful
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk completing timeline items for tour slot {TourSlotId}", request.TourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi bulk complete timeline items"
                });
            }
        }

        /// <summary>
        /// [NEW] Reset timeline item completion
        /// </summary>
        /// <param name="tourSlotId">ID của TourSlot</param>
        /// <param name="timelineItemId">ID của TimelineItem</param>
        /// <param name="request">Reset request</param>
        /// <returns>Kết quả reset</returns>
        [HttpPost("tour-slot/{tourSlotId:guid}/timeline/{timelineItemId:guid}/reset")]
        public async Task<IActionResult> ResetTimelineItem(
            Guid tourSlotId,
            Guid timelineItemId,
            [FromBody] ResetTimelineRequest request)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var response = await _timelineService.ResetTimelineItemAsync(
                    tourSlotId,
                    timelineItemId,
                    request,
                    currentUserObject.UserId);

                return Ok(new ApiResponse<CompleteTimelineResponse>
                {
                    StatusCode = 200,
                    Message = response.Message,
                    Data = response,
                    success = response.Success
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting timeline item {TimelineItemId} for tour slot {TourSlotId}",
                    timelineItemId, tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi reset timeline item"
                });
            }
        }

        /// <summary>
        /// [NEW] Lấy progress summary cho tour slot
        /// </summary>
        /// <param name="tourSlotId">ID của TourSlot</param>
        /// <returns>Progress summary</returns>
        [HttpGet("tour-slot/{tourSlotId:guid}/progress-summary")]
        public async Task<IActionResult> GetProgressSummary(Guid tourSlotId)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var summary = await _timelineService.GetProgressSummaryAsync(tourSlotId, currentUserObject.UserId);

                return Ok(new ApiResponse<TimelineProgressSummaryDto>
                {
                    StatusCode = 200,
                    Message = "Lấy progress summary thành công",
                    Data = summary,
                    success = true
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress summary for tour slot {TourSlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi lấy progress summary"
                });
            }
        }

        /// <summary>
        /// [NEW] Lấy timeline statistics cho analytics
        /// </summary>
        /// <param name="tourSlotId">ID của TourSlot</param>
        /// <returns>Timeline statistics</returns>
        [HttpGet("tour-slot/{tourSlotId:guid}/statistics")]
        public async Task<IActionResult> GetTimelineStatistics(Guid tourSlotId)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var statistics = await _timelineService.GetTimelineStatisticsAsync(tourSlotId, currentUserObject.UserId);

                return Ok(new ApiResponse<TimelineStatisticsResponse>
                {
                    StatusCode = 200,
                    Message = "Lấy timeline statistics thành công",
                    Data = statistics,
                    success = true
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline statistics for tour slot {TourSlotId}", tourSlotId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi lấy timeline statistics"
                });
            }
        }

        /// <summary>
        /// Báo cáo sự cố trong tour
        /// </summary>
        /// <param name="request">Thông tin sự cố</param>
        /// <returns>Kết quả báo cáo</returns>
        [HttpPost("incident/report")]
        public async Task<IActionResult> ReportIncident([FromBody] ReportIncidentRequest request)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                // Verify HDV có quyền báo cáo sự cố cho tour này không
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserObject.UserId);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var tourOperation = await _context.TourOperations
                    .FirstOrDefaultAsync(to => to.Id == request.TourOperationId && to.TourGuideId == tourGuide.Id);

                if (tourOperation == null)
                {
                    return Forbid("HDV không có quyền báo cáo sự cố cho tour này");
                }

                // Tạo TourIncident record
                var incident = new TourIncident
                {
                    Id = Guid.NewGuid(),
                    TourOperationId = request.TourOperationId,
                    ReportedByGuideId = tourGuide.Id,
                    Title = request.Title,
                    Description = request.Description,
                    Severity = request.Severity,
                    Status = "Reported",
                    ImageUrls = request.ImageUrls != null && request.ImageUrls.Any()
                        ? System.Text.Json.JsonSerializer.Serialize(request.ImageUrls)
                        : null,
                    ReportedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUserObject.UserId,
                    IsActive = true
                };

                _context.TourIncidents.Add(incident);
                await _context.SaveChangesAsync();

                // Gửi notification cho admin nếu severity cao
                if (request.Severity == "High" || request.Severity == "Critical")
                {
                    await NotifyAdminAboutIncident(incident);
                }

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Báo cáo sự cố thành công",
                    Data = new
                    {
                        incident.Id,
                        incident.Title,
                        incident.Severity,
                        incident.Status,
                        incident.ReportedAt
                    },
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting incident for tour {TourOperationId}", request.TourOperationId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi báo cáo sự cố"
                });
            }
        }

        /// <summary>
        /// Helper method để gửi notification cho admin về incident
        /// </summary>
        private async Task NotifyAdminAboutIncident(TourIncident incident)
        {
            try
            {
                // Lấy danh sách admin users
                var adminUserIds = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.Name == "Admin")
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var adminId in adminUserIds)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = adminId,
                        Title = $"🚨 Sự cố {incident.Severity} trong tour",
                        Message = $"{incident.Title}: {incident.Description}",
                        Type = DataAccessLayer.Enums.NotificationType.System,
                        Priority = incident.Severity == "Critical" ? DataAccessLayer.Enums.NotificationPriority.Critical : DataAccessLayer.Enums.NotificationPriority.High,
                        Icon = "🚨"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending incident notification to admins for incident {IncidentId}", incident.Id);
            }
        }

        /// <summary>
        /// Hoàn thành tour và cập nhật trạng thái
        /// </summary>
        /// <param name="operationId">ID của TourOperation</param>
        /// <returns>Kết quả hoàn thành tour</returns>
        [HttpPost("tour/{operationId:guid}/complete")]
        public async Task<IActionResult> CompleteTour(Guid operationId)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                // Verify HDV có quyền complete tour này không
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserObject.UserId);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var tourOperation = await _context.TourOperations
                    .Include(to => to.TourDetails)
                        .ThenInclude(td => td.Timeline)
                    .Include(to => to.TourBookings)
                    .FirstOrDefaultAsync(to => to.Id == operationId && to.TourGuideId == tourGuide.Id);

                if (tourOperation == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour hoặc HDV không có quyền truy cập"
                    });
                }

                // Kiểm tra tất cả timeline items đã hoàn thành chưa
                var incompleteItems = tourOperation.TourDetails.Timeline
                    .Where(ti => !ti.IsCompleted)
                    .ToList();

                if (incompleteItems.Any())
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Còn {incompleteItems.Count} mục timeline chưa hoàn thành. Vui lòng hoàn thành tất cả trước khi kết thúc tour."
                    });
                }

                // Use execution strategy to handle transactions properly with MySQL
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Update tour operation status
                        tourOperation.Status = DataAccessLayer.Enums.TourOperationStatus.Completed;
                        tourOperation.UpdatedAt = DateTime.UtcNow;
                        tourOperation.UpdatedById = currentUserObject.UserId;

                        // Update all confirmed bookings to completed
                        var confirmedBookings = tourOperation.TourBookings
                            .Where(tb => tb.Status == DataAccessLayer.Enums.BookingStatus.Confirmed)
                            .ToList();

                        foreach (var booking in confirmedBookings)
                        {
                            booking.Status = DataAccessLayer.Enums.BookingStatus.Completed;
                            booking.UpdatedAt = DateTime.UtcNow;
                            booking.UpdatedById = currentUserObject.UserId;
                        }

                        await _context.SaveChangesAsync();

                        // Gửi notification cho tất cả guests
                        await NotifyGuestsAboutTourCompletion(operationId);

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Tour đã hoàn thành thành công",
                    Data = new
                    {
                        tourOperation.Id,
                        tourOperation.Status,
                        CompletedBookings = tourOperation.TourBookings.Count(tb => tb.Status == DataAccessLayer.Enums.BookingStatus.Completed),
                        CompletedAt = DateTime.UtcNow
                    },
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing tour {OperationId}", operationId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi hoàn thành tour"
                });
            }
        }

        /// <summary>
        /// Helper method để gửi notification cho guests về tour completion
        /// </summary>
        private async Task NotifyGuestsAboutTourCompletion(Guid tourOperationId)
        {
            try
            {
                var guestUserIds = await _context.TourBookings
                    .Where(tb => tb.TourOperationId == tourOperationId &&
                                tb.Status == DataAccessLayer.Enums.BookingStatus.Completed)
                    .Select(tb => tb.UserId)
                    .ToListAsync();

                foreach (var userId in guestUserIds)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = userId,
                        Title = "🎉 Tour hoàn thành",
                        Message = "Tour đã kết thúc thành công! Cảm ơn bạn đã tham gia. Chúc bạn về nhà an toàn!",
                        Type = DataAccessLayer.Enums.NotificationType.Tour,
                        Priority = DataAccessLayer.Enums.NotificationPriority.High,
                        Icon = "🎉"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending tour completion notifications for tour {TourOperationId}", tourOperationId);
            }
        }

        /// <summary>
        /// Gửi thông báo cho tất cả guests trong tour
        /// </summary>
        /// <param name="operationId">ID của TourOperation</param>
        /// <param name="request">Nội dung thông báo</param>
        /// <returns>Kết quả gửi thông báo</returns>
        [HttpPost("tour/{operationId:guid}/notify-guests")]
        public async Task<IActionResult> NotifyGuests(Guid operationId, [FromBody] NotifyGuestsRequest request)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                // Verify HDV có quyền gửi thông báo cho tour này không
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == currentUserObject.UserId);

                if (tourGuide == null)
                {
                    return NotFound(new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                var tourOperation = await _context.TourOperations
                    .FirstOrDefaultAsync(to => to.Id == operationId && to.TourGuideId == tourGuide.Id);

                if (tourOperation == null)
                {
                    return Forbid("HDV không có quyền gửi thông báo cho tour này");
                }

                // Lấy danh sách guests đã check-in
                var guestUserIds = await _context.TourBookings
                    .Where(tb => tb.TourOperationId == operationId &&
                                tb.Status == DataAccessLayer.Enums.BookingStatus.Confirmed &&
                                tb.IsCheckedIn)
                    .Select(tb => tb.UserId)
                    .ToListAsync();

                if (!guestUserIds.Any())
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Không có khách nào đã check-in để gửi thông báo"
                    });
                }

                // Gửi notification cho tất cả guests
                int successCount = 0;
                foreach (var userId in guestUserIds)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = userId,
                            Title = "📢 Thông báo từ HDV",
                            Message = request.Message,
                            Type = DataAccessLayer.Enums.NotificationType.Tour,
                            Priority = request.IsUrgent ? DataAccessLayer.Enums.NotificationPriority.High : DataAccessLayer.Enums.NotificationPriority.Normal,
                            Icon = "📢"
                        });
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                    }
                }

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = $"Đã gửi thông báo cho {successCount}/{guestUserIds.Count} khách",
                    Data = new
                    {
                        TotalGuests = guestUserIds.Count,
                        SuccessCount = successCount,
                        Message = request.Message
                    },
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying guests for tour {OperationId}", operationId);
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi gửi thông báo"
                });
            }
        }

        /// <summary>
        /// Upload hình ảnh cho incident reporting
        /// </summary>
        /// <param name="files">Danh sách file ảnh</param>
        /// <returns>Danh sách URL của ảnh đã upload</returns>
        [HttpPost("incident/upload-images")]
        public async Task<IActionResult> UploadIncidentImages([FromForm] List<IFormFile> files)
        {
            try
            {
                var currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUserObject?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin HDV"
                    });
                }

                if (files == null || !files.Any())
                {
                    return BadRequest(new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Không có file nào được upload"
                    });
                }

                // Validate files
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var maxFileSize = 5 * 1024 * 1024; // 5MB
                var uploadedUrls = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length == 0)
                        continue;

                    if (file.Length > maxFileSize)
                    {
                        return BadRequest(new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = $"File {file.FileName} quá lớn. Kích thước tối đa là 5MB."
                        });
                    }

                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = $"File {file.FileName} không đúng định dạng. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}"
                        });
                    }

                    // Generate unique filename
                    var fileName = $"incident_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}";
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "incidents");

                    // Create directory if not exists
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Generate URL (adjust based on your domain configuration)
                    var fileUrl = $"/uploads/incidents/{fileName}";
                    uploadedUrls.Add(fileUrl);
                }

                return Ok(new ApiResponse<List<string>>
                {
                    StatusCode = 200,
                    Message = "Upload ảnh thành công",
                    Data = uploadedUrls,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading incident images");
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi upload ảnh"
                });
            }
        }
    }

    /// <summary>
    /// Request model cho check-in guest
    /// </summary>
    public class CheckInGuestRequest
    {
        public string? QRCodeData { get; set; }
        public string? Notes { get; set; }
    }



    /// <summary>
    /// Request model cho báo cáo sự cố
    /// </summary>
    public class ReportIncidentRequest
    {
        public Guid TourOperationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
        public List<string>? ImageUrls { get; set; }
    }

    /// <summary>
    /// Request model cho gửi thông báo guests
    /// </summary>
    public class NotifyGuestsRequest
    {
        public string Message { get; set; } = string.Empty;
        public bool IsUrgent { get; set; } = false;
    }
}
