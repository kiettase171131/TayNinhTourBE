using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Notification;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller qu?n l� in-app notifications
    /// Cung c?p API ?? xem, qu?n l� th�ng b�o trong ?ng d?ng
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
        /// L?y danh s�ch th�ng b�o c?a user hi?n t?i
        /// </summary>
        /// <param name="pageIndex">Trang hi?n t?i (0-based, default: 0)</param>
        /// <param name="pageSize">K�ch th??c trang (default: 20, max: 100)</param>
        /// <param name="isRead">L?c theo tr?ng th�i ??c (null = t?t c?)</param>
        /// <param name="type">L?c theo lo?i th�ng b�o (null = t?t c?)</param>
        /// <returns>Danh s�ch th�ng b�o</returns>
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
                    Message = "C� l?i x?y ra khi l?y danh s�ch th�ng b�o",
                    success = false
                });
            }
        }

        /// <summary>
        /// L?y s? l??ng th�ng b�o ch?a ??c
        /// </summary>
        /// <returns>S? l??ng th�ng b�o ch?a ??c</returns>
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
                    Message = "C� l?i x?y ra khi l?y s? th�ng b�o ch?a ??c",
                    success = false
                });
            }
        }

        /// <summary>
        /// ?�nh d?u th�ng b�o ?� ??c
        /// </summary>
        /// <param name="notificationId">ID c?a th�ng b�o</param>
        /// <returns>K?t qu? thao t�c</returns>
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
                    Message = "C� l?i x?y ra khi ?�nh d?u th�ng b�o ?� ??c",
                    success = false
                });
            }
        }

        /// <summary>
        /// ?�nh d?u t?t c? th�ng b�o ?� ??c
        /// </summary>
        /// <returns>K?t qu? thao t�c</returns>
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
                    Message = "C� l?i x?y ra khi ?�nh d?u t?t c? th�ng b�o ?� ??c",
                    success = false
                });
            }
        }

        /// <summary>
        /// X�a th�ng b�o
        /// </summary>
        /// <param name="notificationId">ID c?a th�ng b�o</param>
        /// <returns>K?t qu? thao t�c</returns>
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
                    Message = "C� l?i x?y ra khi x�a th�ng b�o",
                    success = false
                });
            }
        }

        /// <summary>
        /// L?y th?ng k� th�ng b�o
        /// </summary>
        /// <returns>Th?ng k� th�ng b�o</returns>
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
                    Message = "C� l?i x?y ra khi l?y th?ng k� th�ng b�o",
                    success = false
                });
            }
        }

        /// <summary>
        /// L?y th�ng b�o m?i nh?t (realtime polling endpoint)
        /// </summary>
        /// <param name="lastCheckTime">Th?i gian check l?n cu?i (ISO format)</param>
        /// <returns>Th�ng b�o m?i t? th?i ?i?m lastCheckTime</returns>
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
                    Message = "C� l?i x?y ra khi l?y th�ng b�o m?i nh?t",
                    success = false
                });
            }
        }

        /// <summary>
        /// Admin endpoint: T?o th�ng b�o cho user c? th?
        /// </summary>
        /// <param name="createDto">Th�ng tin th�ng b�o</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
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
                    Message = "C� l?i x?y ra khi t?o th�ng b�o",
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