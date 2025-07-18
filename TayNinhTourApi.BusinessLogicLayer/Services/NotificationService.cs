using AutoMapper;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Notification;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho qu?n lý in-app notifications
    /// </summary>
    public class NotificationService : BaseService, INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<NotificationService> logger) : base(mapper, unitOfWork)
        {
            _logger = logger;
        }

        /// <summary>
        /// T?o thông báo m?i
        /// </summary>
        public async Task<BaseResposeDto> CreateNotificationAsync(CreateNotificationDto createDto)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = createDto.UserId,
                    Title = createDto.Title,
                    Message = createDto.Message,
                    Type = createDto.Type,
                    Priority = createDto.Priority,
                    AdditionalData = createDto.AdditionalData,
                    ActionUrl = createDto.ActionUrl,
                    Icon = createDto.Icon,
                    ExpiresAt = createDto.ExpiresAt,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.NotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created notification {NotificationId} for user {UserId}",
                    notification.Id, createDto.UserId);

                return new BaseResposeDto
                {
                    StatusCode = 201,
                    Message = "T?o thông báo thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", createDto.UserId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có l?i x?y ra khi t?o thông báo: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// L?y danh sách thông báo c?a user
        /// </summary>
        public async Task<NotificationsResponseDto> GetUserNotificationsAsync(Guid userId, GetNotificationsRequestDto request)
        {
            try
            {
                // Validate page size
                if (request.PageSize > 100)
                    request.PageSize = 100;

                var (notifications, totalCount) = await _unitOfWork.NotificationRepository
                    .GetUserNotificationsAsync(userId, request.PageIndex, request.PageSize, request.IsRead, request.Type);

                var unreadCount = await _unitOfWork.NotificationRepository.GetUnreadCountAsync(userId);

                var notificationDtos = notifications.Select(n => MapToNotificationDto(n)).ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                return new NotificationsResponseDto
                {
                    StatusCode = 200,
                    Message = "L?y danh sách thông báo thành công",
                    success = true,
                    Notifications = notificationDtos,
                    TotalCount = totalCount,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    UnreadCount = unreadCount,
                    HasNextPage = request.PageIndex < totalPages - 1,
                    HasPreviousPage = request.PageIndex > 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return new NotificationsResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có l?i x?y ra khi l?y thông báo: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// L?y s? l??ng thông báo ch?a ??c
        /// </summary>
        public async Task<UnreadCountResponseDto> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var unreadCount = await _unitOfWork.NotificationRepository.GetUnreadCountAsync(userId);

                return new UnreadCountResponseDto
                {
                    StatusCode = 200,
                    Message = "L?y s? thông báo ch?a ??c thành công",
                    success = true,
                    UnreadCount = unreadCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                return new UnreadCountResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có l?i x?y ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// ?ánh d?u thông báo ?ã ??c
        /// </summary>
        public async Task<BaseResposeDto> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            try
            {
                var success = await _unitOfWork.NotificationRepository.MarkAsReadAsync(notificationId, userId);

                if (!success)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm th?y thông báo ho?c thông báo ?ã ???c ??c",
                        success = false
                    };
                }

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "?ã ?ánh d?u thông báo ?ã ??c",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}",
                    notificationId, userId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có l?i x?y ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// ?ánh d?u t?t c? thông báo ?ã ??c
        /// </summary>
        public async Task<BaseResposeDto> MarkAllAsReadAsync(Guid userId)
        {
            try
            {
                var updatedCount = await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(userId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"?ã ?ánh d?u {updatedCount} thông báo ?ã ??c",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có l?i x?y ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        public async Task<BaseResposeDto> DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            try
            {
                var notification = await _unitOfWork.NotificationRepository.GetByIdAndUserAsync(notificationId, userId);

                if (notification == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm th?y thông báo",
                        success = false
                    };
                }

                // Soft delete
                notification.IsActive = false;
                notification.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.NotificationRepository.UpdateAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "?ã xóa thông báo",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}",
                    notificationId, userId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có l?i x?y ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// L?y th?ng kê thông báo c?a user
        /// </summary>
        public async Task<NotificationStatsDto> GetNotificationStatsAsync(Guid userId)
        {
            try
            {
                var (allNotifications, totalCount) = await _unitOfWork.NotificationRepository
                    .GetUserNotificationsAsync(userId, 0, 1000); // Get all notifications for stats

                var unreadCount = await _unitOfWork.NotificationRepository.GetUnreadCountAsync(userId);

                var stats = new NotificationStatsDto
                {
                    TotalNotifications = totalCount,
                    UnreadCount = unreadCount,
                    ReadCount = totalCount - unreadCount,
                    HighPriorityCount = allNotifications.Count(n => n.Priority == NotificationPriority.High),
                    UrgentCount = allNotifications.Count(n => n.Priority == NotificationPriority.Urgent),
                    LatestNotification = allNotifications.Any() ? MapToNotificationDto(allNotifications.First()) : null
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats for user {UserId}", userId);
                return new NotificationStatsDto();
            }
        }

        /// <summary>
        /// Cleanup thông báo c? (background job)
        /// </summary>
        public async Task<int> CleanupOldNotificationsAsync(int olderThanDays = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
                var oldNotifications = await _unitOfWork.NotificationRepository
                    .GetAllAsync(n => n.CreatedAt < cutoffDate);

                var count = 0;
                foreach (var notification in oldNotifications)
                {
                    notification.IsDeleted = true;
                    notification.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.NotificationRepository.UpdateAsync(notification);
                    count++;
                }

                if (count > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Cleaned up {Count} old notifications", count);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old notifications");
                return 0;
            }
        }

        // Helper methods for creating specific notification types

        /// <summary>
        /// T?o thông báo booking m?i
        /// </summary>
        public async Task<BaseResposeDto> CreateBookingNotificationAsync(Guid userId, string bookingCode, string tourTitle)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Booking m?i",
                Message = $"B?n có booking m?i #{bookingCode} cho tour '{tourTitle}'",
                Type = NotificationType.Booking,
                Priority = NotificationPriority.Normal,
                Icon = "??",
                ActionUrl = $"/bookings/{bookingCode}"
            };

            return await CreateNotificationAsync(createDto);
        }

        /// <summary>
        /// T?o thông báo TourGuide t? ch?i
        /// </summary>
        public async Task<BaseResposeDto> CreateGuideRejectionNotificationAsync(Guid userId, string tourTitle, string guideName, string? rejectionReason)
        {
            var message = $"H??ng d?n viên {guideName} ?ã t? ch?i tour '{tourTitle}'";
            if (!string.IsNullOrEmpty(rejectionReason))
            {
                message += $". Lý do: {rejectionReason}";
            }

            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "H??ng d?n viên t? ch?i",
                Message = message,
                Type = NotificationType.TourGuide,
                Priority = NotificationPriority.High,
                Icon = "?",
                ActionUrl = $"/tours/{tourTitle}"
            };

            return await CreateNotificationAsync(createDto);
        }

        /// <summary>
        /// T?o thông báo c?n tìm guide th? công
        /// </summary>
        public async Task<BaseResposeDto> CreateManualGuideSelectionNotificationAsync(Guid userId, string tourTitle, int expiredCount)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "C?n tìm h??ng d?n viên",
                Message = $"Tour '{tourTitle}' c?n tìm h??ng d?n viên th? công. {expiredCount} l?i m?i ?ã h?t h?n.",
                Type = NotificationType.TourGuide,
                Priority = NotificationPriority.High,
                Icon = "??",
                ActionUrl = $"/tours/manual-guide-selection"
            };

            return await CreateNotificationAsync(createDto);
        }

        /// <summary>
        /// T?o thông báo c?nh báo tour s?p b? h?y
        /// </summary>
        public async Task<BaseResposeDto> CreateTourRiskCancellationNotificationAsync(Guid userId, string tourTitle, int daysUntilCancellation)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "?? KH?N C?P: Tour s?p b? h?y",
                Message = $"Tour '{tourTitle}' s? b? h?y trong {daysUntilCancellation} ngày n?u không tìm ???c h??ng d?n viên!",
                Type = NotificationType.Warning,
                Priority = NotificationPriority.Urgent,
                Icon = "??",
                ActionUrl = $"/tours/emergency"
            };

            return await CreateNotificationAsync(createDto);
        }

        // NEW: TourGuide notification methods

        /// <summary>
        /// T?o thông báo khi TourGuide ???c m?i tham gia tour
        /// </summary>
        public async Task<BaseResposeDto> CreateTourGuideInvitationNotificationAsync(
            Guid guideUserId,
            string tourTitle,
            string tourCompanyName,
            string? skillsRequired,
            string invitationType,
            DateTime expiresAt,
            Guid invitationId)
        {
            try
            {
                var message = $"B?n ???c m?i tham gia tour '{tourTitle}' b?i {tourCompanyName}.";
                if (!string.IsNullOrEmpty(skillsRequired))
                {
                    message += $" K? n?ng yêu c?u: {skillsRequired}.";
                }

                var hoursUntilExpiry = (int)(expiresAt - DateTime.UtcNow).TotalHours;
                message += $" H?n ph?n h?i: {hoursUntilExpiry} gi?.";

                var icon = invitationType.ToLower() == "manual" ? "??" : "??"; // Manual = personal invite, Auto = skill-matched

                return await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = guideUserId,
                    Title = "?? L?i m?i tour m?i!",
                    Message = message,
                    Type = NotificationType.TourGuide,
                    Priority = NotificationPriority.High,
                    Icon = icon,
                    ActionUrl = $"/invitations/{invitationId}",
                    Data = new Dictionary<string, object>
                    {
                        ["invitationId"] = invitationId,
                        ["tourTitle"] = tourTitle,
                        ["tourCompanyName"] = tourCompanyName,
                        ["invitationType"] = invitationType,
                        ["expiresAt"] = expiresAt,
                        ["skillsRequired"] = skillsRequired ?? ""
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating TourGuide invitation notification for user {UserId}", guideUserId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có l?i x?y ra khi t?o thông báo: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// T?o thông báo khi invitation s?p h?t h?n (reminder)
        /// </summary>
        public async Task<BaseResposeDto> CreateInvitationExpiryReminderNotificationAsync(
            Guid guideUserId,
            string tourTitle,
            int hoursUntilExpiry,
            Guid invitationId)
        {
            try
            {
                var urgencyIcon = hoursUntilExpiry <= 2 ? "??" : hoursUntilExpiry <= 6 ? "?" : "?";
                var urgencyText = hoursUntilExpiry <= 2 ? "G?P!" : hoursUntilExpiry <= 6 ? "S?p h?t h?n" : "Nh?c nh?";

                return await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = guideUserId,
                    Title = $"{urgencyIcon} {urgencyText} - L?i m?i tour",
                    Message = $"L?i m?i tour '{tourTitle}' s? h?t h?n trong {hoursUntilExpiry} gi?. Vui lòng ph?n h?i s?m!",
                    Type = hoursUntilExpiry <= 2 ? NotificationType.Critical : NotificationType.Warning,
                    Priority = hoursUntilExpiry <= 2 ? NotificationPriority.Critical : NotificationPriority.High,
                    Icon = urgencyIcon,
                    ActionUrl = $"/invitations/{invitationId}",
                    Data = new Dictionary<string, object>
                    {
                        ["invitationId"] = invitationId,
                        ["tourTitle"] = tourTitle,
                        ["hoursUntilExpiry"] = hoursUntilExpiry,
                        ["isReminder"] = true
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invitation expiry reminder notification for user {UserId}", guideUserId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có l?i x?y ra khi t?o thông báo nh?c nh?: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Helper method ?? map Notification entity sang DTO
        /// </summary>
        private static NotificationDto MapToNotificationDto(Notification notification)
        {
            var timeAgo = GetTimeAgo(notification.CreatedAt);
            var priorityClass = GetPriorityClass(notification.Priority);
            var typeClass = GetTypeClass(notification.Type);

            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type.ToString(),
                Priority = notification.Priority.ToString(),
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                AdditionalData = notification.AdditionalData,
                ActionUrl = notification.ActionUrl,
                Icon = notification.Icon,
                ExpiresAt = notification.ExpiresAt,
                TimeAgo = timeAgo,
                PriorityClass = priorityClass,
                TypeClass = typeClass
            };
        }

        /// <summary>
        /// Helper method ?? tính th?i gian "ago"
        /// </summary>
        private static string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;

            if (timeSpan.TotalMinutes < 1)
                return "V?a xong";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút tr??c";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} gi? tr??c";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày tr??c";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} tu?n tr??c";

            return createdAt.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Helper method ?? l?y CSS class cho priority
        /// </summary>
        private static string GetPriorityClass(NotificationPriority priority)
        {
            return priority switch
            {
                NotificationPriority.Low => "priority-low",
                NotificationPriority.Normal => "priority-normal",
                NotificationPriority.High => "priority-high",
                NotificationPriority.Urgent => "priority-urgent",
                _ => "priority-normal"
            };
        }

        /// <summary>
        /// Helper method ?? l?y CSS class cho type
        /// </summary>
        private static string GetTypeClass(NotificationType type)
        {
            return type switch
            {
                NotificationType.Booking => "type-booking",
                NotificationType.Tour => "type-tour",
                NotificationType.TourGuide => "type-guide",
                NotificationType.Payment => "type-payment",
                NotificationType.Wallet => "type-wallet",
                NotificationType.System => "type-system",
                NotificationType.Warning => "type-warning",
                NotificationType.Error => "type-error",
                _ => "type-general"
            };
        }
    }
}