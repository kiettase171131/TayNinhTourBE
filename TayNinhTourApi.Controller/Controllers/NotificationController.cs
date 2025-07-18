using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Notification;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller qu?n lý in-app notifications
    /// Cung c?p API ?? xem, qu?n lý thông báo trong ?ng d?ng
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// L?y danh sách thông báo c?a user hi?n t?i
        /// </summary>
        /// <param name="pageIndex">Trang hi?n t?i (0-based, default: 0)</param>
        /// <param name="pageSize">Kích th??c trang (default: 20, max: 100)</param>
        /// <param name="isRead">L?c theo tr?ng thái ??c (null = t?t c?)</param>
        /// <param name="type">L?c theo lo?i thông báo (null = t?t c?)</param>
        /// <returns>Danh sách thông báo</returns>
        [HttpGet]
        public async Task<ActionResult<NotificationsResponseDto>> GetMyNotifications(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isRead = null,
            [FromQuery] string? type = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} requesting notifications - Page: {PageIndex}, Size: {PageSize}", 
                    userId, pageIndex, pageSize);

                var request = new GetNotificationsRequestDto
                {
                    PageIndex = pageIndex,
                    PageSize = Math.Min(pageSize, 100), // Limit max page size
                    IsRead = isRead
                };

                // Parse type if provided
                if (!string.IsNullOrEmpty(type) && Enum.TryParse<DataAccessLayer.Enums.NotificationType>(type, true, out var notificationType))
                {
                    request.Type = notificationType;
                }

                var result = await _notificationService.GetUserNotificationsAsync(userId, request);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user");
                return StatusCode(500, new NotificationsResponseDto
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi l?y danh sách thông báo",
                    success = false
                });
            }
        }

        /// <summary>
        /// L?y s? l??ng thông báo ch?a ??c
        /// </summary>
        /// <returns>S? l??ng thông báo ch?a ??c</returns>
        [HttpGet("unread-count")]
        public async Task<ActionResult<UnreadCountResponseDto>> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.GetUnreadCountAsync(userId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user");
                return StatusCode(500, new UnreadCountResponseDto
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi l?y s? thông báo ch?a ??c",
                    success = false
                });
            }
        }

        /// <summary>
        /// ?ánh d?u thông báo ?ã ??c
        /// </summary>
        /// <param name="notificationId">ID c?a thông báo</param>
        /// <returns>K?t qu? thao tác</returns>
        [HttpPut("{notificationId}/read")]
        public async Task<ActionResult> MarkAsRead(Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} marking notification {NotificationId} as read", 
                    userId, notificationId);

                var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi ?ánh d?u thông báo ?ã ??c",
                    success = false
                });
            }
        }

        /// <summary>
        /// ?ánh d?u t?t c? thông báo ?ã ??c
        /// </summary>
        /// <returns>K?t qu? thao tác</returns>
        [HttpPut("mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} marking all notifications as read", userId);

                var result = await _notificationService.MarkAllAsReadAsync(userId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi ?ánh d?u t?t c? thông báo ?ã ??c",
                    success = false
                });
            }
        }

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        /// <param name="notificationId">ID c?a thông báo</param>
        /// <returns>K?t qu? thao tác</returns>
        [HttpDelete("{notificationId}")]
        public async Task<ActionResult> DeleteNotification(Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} deleting notification {NotificationId}", 
                    userId, notificationId);

                var result = await _notificationService.DeleteNotificationAsync(notificationId, userId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi xóa thông báo",
                    success = false
                });
            }
        }

        /// <summary>
        /// L?y th?ng kê thông báo
        /// </summary>
        /// <returns>Th?ng kê thông báo</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<NotificationStatsDto>> GetNotificationStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var stats = await _notificationService.GetNotificationStatsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats for user");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi l?y th?ng kê thông báo",
                    success = false
                });
            }
        }

        /// <summary>
        /// L?y thông báo m?i nh?t (realtime polling endpoint)
        /// </summary>
        /// <param name="lastCheckTime">Th?i gian check l?n cu?i (ISO format)</param>
        /// <returns>Thông báo m?i t? th?i ?i?m lastCheckTime</returns>
        [HttpGet("latest")]
        public async Task<ActionResult<NotificationsResponseDto>> GetLatestNotifications(
            [FromQuery] DateTime? lastCheckTime = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var checkTime = lastCheckTime ?? DateTime.UtcNow.AddMinutes(-5); // Default to 5 minutes ago

                _logger.LogDebug("User {UserId} checking for notifications since {CheckTime}", 
                    userId, checkTime);

                // Get recent notifications (this would need a new repository method)
                var request = new GetNotificationsRequestDto
                {
                    PageIndex = 0,
                    PageSize = 50, // Limit for latest notifications
                    IsRead = false // Only unread notifications for real-time
                };

                var result = await _notificationService.GetUserNotificationsAsync(userId, request);
                
                // Filter to only notifications created after lastCheckTime
                if (result.success && result.Notifications.Any())
                {
                    result.Notifications = result.Notifications
                        .Where(n => n.CreatedAt > checkTime)
                        .ToList();
                    result.TotalCount = result.Notifications.Count;
                }

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest notifications for user");
                return StatusCode(500, new NotificationsResponseDto
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi l?y thông báo m?i nh?t",
                    success = false
                });
            }
        }

        /// <summary>
        /// Admin endpoint: T?o thông báo cho user c? th?
        /// </summary>
        /// <param name="createDto">Thông tin thông báo</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateNotification([FromBody] CreateNotificationDto createDto)
        {
            try
            {
                _logger.LogInformation("Admin creating notification for user {UserId}", createDto.UserId);

                var result = await _notificationService.CreateNotificationAsync(createDto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", createDto.UserId);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi t?o thông báo",
                    success = false
                });
            }
        }

        /// <summary>
        /// Helper method ?? l?y current user ID t? JWT token
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}