using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification;
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
        private readonly ILogger<TourGuideController> _logger;

        public TourGuideController(
            TayNinhTouApiDbContext context,
            INotificationService notificationService,
            IQRCodeService qrCodeService,
            ILogger<TourGuideController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _qrCodeService = qrCodeService;
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

                // Lấy tours đang active của HDV này
                var activeTours = await _context.TourOperations
                    .Include(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                    .Include(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                    .Include(to => to.TourBookings)
                    .Where(to => to.TourGuideId == tourGuide.Id && 
                                to.IsActive && 
                                to.TourDetails.Status == DataAccessLayer.Enums.TourDetailsStatus.Approved)
                    .Select(to => new
                    {
                        to.Id,
                        to.TourDetails.Title,
                        to.TourDetails.Description,
                        TourDate = to.TourDetails.AssignedSlots.FirstOrDefault() != null ?
                                  to.TourDetails.AssignedSlots.FirstOrDefault()!.TourDate :
                                  DateOnly.FromDateTime(DateTime.Today),
                        to.Price,
                        to.MaxGuests,
                        to.CurrentBookings,
                        to.Status,
                        TourTemplate = new
                        {
                            to.TourDetails.TourTemplate.Title,
                            to.TourDetails.TourTemplate.StartLocation,
                            to.TourDetails.TourTemplate.EndLocation
                        },
                        BookingsCount = to.TourBookings.Count(tb => tb.Status == DataAccessLayer.Enums.BookingStatus.Confirmed),
                        CheckedInCount = to.TourBookings.Count(tb => tb.Status == DataAccessLayer.Enums.BookingStatus.Confirmed && tb.IsCheckedIn)
                    })
                    .OrderBy(to => to.TourDate)
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
        /// Lấy timeline items cho tour cụ thể
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
        /// Hoàn thành timeline item và gửi notification cho guests
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

                // Use transaction to ensure data consistency
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
    /// Request model cho complete timeline
    /// </summary>
    public class CompleteTimelineRequest
    {
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
