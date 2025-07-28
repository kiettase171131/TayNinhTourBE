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
    /// Service implementation cho qu?n l� in-app notifications
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
        /// T?o th�ng b�o m?i
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
                    Message = "Tạo thông báo thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", createDto.UserId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi tạo thông báo: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// L?y danh s�ch th�ng b�o c?a user
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
                    Message = "Lấy danh sách thông báo thành công",
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
                    Message = $"Có lỗi xảy ra khi tạo thông báo: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// L?y s? l??ng th�ng b�o ch?a ??c
        /// </summary>
        public async Task<UnreadCountResponseDto> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var unreadCount = await _unitOfWork.NotificationRepository.GetUnreadCountAsync(userId);

                return new UnreadCountResponseDto
                {
                    StatusCode = 200,
                    Message = "Lấy số thông báo chưa đọc thành công",
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
                    Message = $"Có lỗi xảy ra khi: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// ?�nh d?u th�ng b�o ?� ??c
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
                        Message = "Không tìm thấy thông báo hoặc thông báo đã được đọc",
                        success = false
                    };
                }

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã đánh dấu thông báo đã đọc",
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
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// ?�nh d?u t?t c? th�ng b�o ?� ??c
        /// </summary>
        public async Task<BaseResposeDto> MarkAllAsReadAsync(Guid userId)
        {
            try
            {
                var updatedCount = await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(userId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Đã đánh dấu {updatedCount} thông báo đã đọc",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// X�a th�ng b�o
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
                        Message = "Không tìm thấy thông báo",
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
                    Message = "Đã xóa thông báo",
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
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// L?y th?ng k� th�ng b�o c?a user
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
        /// Cleanup th�ng b�o c? (background job)
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
        /// T?o th�ng b�o booking m?i
        /// </summary>
        public async Task<BaseResposeDto> CreateBookingNotificationAsync(Guid userId, string bookingCode, string tourTitle)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "📩 Booking mới",
                Message = $"Bạn có booking mới #{bookingCode} cho tour '{tourTitle}'",
                Type = NotificationType.Booking,
                Priority = NotificationPriority.Normal,
                Icon = "📩",
                ActionUrl = $"/bookings/{bookingCode}"

            };

            return await CreateNotificationAsync(createDto);
        }

        /// <summary>
        /// T?o th�ng b�o TourGuide t? ch?i
        /// </summary>
        public async Task<BaseResposeDto> CreateGuideRejectionNotificationAsync(
            Guid userId, 
            string tourTitle, 
            string guideName, 
            string? rejectionReason)
        {
            return await CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = "🚫 Hướng dẫn viên từ chối",
                Message = $"{guideName} đã từ chối tour '{tourTitle}'. {(rejectionReason != null ? $"Lý do: {rejectionReason}" : "")}",
                Type = NotificationType.Warning,
                Priority = NotificationPriority.High,
                Icon = "🚫",
                ActionUrl = "/tours/pending-guide"

            });
        }

        /// <summary>
        /// T?o th�ng b�o c?n t�m guide th? c�ng
        /// </summary>
        public async Task<BaseResposeDto> CreateManualGuideSelectionNotificationAsync(
            Guid userId, 
            string tourTitle, 
            int expiredCount)
        {
            return await CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = "⚠️ Cần tìm hướng dẫn viên thủ công",
                Message = $"Tour '{tourTitle}' có {expiredCount} lời mời đã hết hạn. Cần tìm hướng dẫn viên thủ công.",
                Type = NotificationType.Warning,
                Priority = NotificationPriority.High,
                Icon = "⚠️",
                ActionUrl = "/tours/manual-guide-selection"

            });
        }

        /// <summary>
        /// T?o th�ng b�o c?nh b�o tour s?p b? h?y
        /// </summary>
        public async Task<BaseResposeDto> CreateTourRiskCancellationNotificationAsync(
            Guid userId, 
            string tourTitle, 
            int daysUntilCancellation)
        {
            return await CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = "🚨 Tour sắp bị hủy!",
                Message = $"Tour '{tourTitle}' sẽ bị hủy trong {daysUntilCancellation} ngày nếu không tìm được hướng dẫn viên!",
                Type = NotificationType.Critical,
                Priority = NotificationPriority.Critical,
                Icon = "🚨",
                ActionUrl = "/tours/urgent-guide-needed"

            });
        }

        /// <summary>
        /// T?o th�ng b�o khi TourGuide ???c m?i tham gia tour
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
                var message = $"Bạn được mời tham gia tour '{tourTitle}' bởi {tourCompanyName}.";
                if (!string.IsNullOrEmpty(skillsRequired))
                {
                    message += $"Kỹ năng yêu cầu: {skillsRequired}.";
                }

                var hoursUntilExpiry = (int)(expiresAt - DateTime.UtcNow).TotalHours;
                message += $"Hạn phản hồi: {hoursUntilExpiry} giờ.";

                var icon = invitationType.ToLower() == "manual" ? "??" : "??"; // Manual = personal invite, Auto = skill-matched

                return await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = guideUserId,
                    Title = "Lời mời tour mới!",
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
                    Message = $"Có lỗi xảy ra khi tạo thông báo: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// T?o th�ng b�o khi invitation s?p h?t h?n (reminder)
        /// </summary>
        public async Task<BaseResposeDto> CreateInvitationExpiryReminderNotificationAsync(
            Guid guideUserId,
            string tourTitle,
            int hoursUntilExpiry,
            Guid invitationId)
        {
            try
            {
                var urgencyIcon = hoursUntilExpiry <= 2 ? "⏰" : hoursUntilExpiry <= 6 ? "⚠️" : "🔔";
                var urgencyText = hoursUntilExpiry <= 2 ? "GẤP!" : hoursUntilExpiry <= 6 ? "Sắp hết hạn" : "Nhắc nhở";


                return await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = guideUserId,
                    Title = $"{urgencyIcon} {urgencyText} - Lời mời tour",
                    Message = $"Lời mời tour '{tourTitle}' sẽ hết hạn trong {hoursUntilExpiry} giờ. Vui lòng phản hồi sớm!",
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
                    Message = $"Có lỗi xảy ra khi tạo thông báo nhắc nhở: {ex.Message}",
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
        /// Helper method ?? t�nh th?i gian "ago"
        /// </summary>
        private static string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;

            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";


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

        // Additional helper methods for booking notifications

        /// <summary>
        /// T?o th�ng b�o booking m?i (generic method)
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="booking">Th�ng tin booking</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        public async Task<BaseResposeDto> CreateNewBookingNotificationAsync(Guid userId, object booking)
        {
            // Implementation depends on booking object structure
            // For now, create a generic notification
            return await CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = "Booking mới",
                Message = "Bạn có một booking tour mới",
                Type = NotificationType.Booking,
                Priority = NotificationPriority.High,
                Icon = "📥" // Hoặc "🆕", "📩", tùy phong cách hệ thống

            });
        }

        /// <summary>
        /// T?o th�ng b�o h?y tour v?i danh s�ch bookings
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="affectedBookings">Danh s�ch bookings b? ?nh h??ng</param>
        /// <param name="tourTitle">T�n tour</param>
        /// <param name="tourStartDate">Ng�y kh?i h�nh</param>
        /// <param name="reason">L� do h?y</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        public async Task<BaseResposeDto> CreateTourCancellationNotificationAsync(
            Guid userId, 
            object affectedBookings, 
            string tourTitle, 
            DateTime tourStartDate, 
            string reason)
        {
            return await CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = "Tour bị hủy",
                Message = $"Tour '{tourTitle}' đã bị hủy: {reason}",
                Type = NotificationType.Warning,
                Priority = NotificationPriority.High,
                Icon = "❌" // Hoặc "⚠️", "🚫", "📛" tùy mức độ cảnh báo

            });
        }

        /// <summary>
        /// T?o th�ng b�o h?y booking c?a kh�ch h�ng
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="booking">Th�ng tin booking</param>
        /// <param name="reason">L� do h?y</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        public async Task<BaseResposeDto> CreateBookingCancellationNotificationAsync(Guid userId, object booking, string? reason)
        {
            return await CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = "Booking bị hủy",
                Message = $"Khách hàng đã hủy booking. Lý do: {reason ?? "Không có lý do"}",
                Type = NotificationType.Warning,
                Priority = NotificationPriority.Medium,
                Icon = "🚫" // Hoặc "❌", "⚠️", "📭" tùy mức cảnh báo bạn muốn thể hiện

            });
        }

        /// <summary>
        /// L?y notifications m?i nh?t (?? polling/real-time updates)
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="lastCheckTime">Th?i gian check cu?i c�ng</param>
        /// <returns>Notifications m?i t? lastCheckTime</returns>
        public async Task<NotificationsResponseDto> GetLatestNotificationsAsync(Guid userId, DateTime lastCheckTime)
        {
            try
            {
                var (notifications, totalCount) = await _unitOfWork.NotificationRepository
                    .GetLatestNotificationsAsync(userId, lastCheckTime);

                var notificationDtos = notifications.Select(n => MapToNotificationDto(n)).ToList();

                return new NotificationsResponseDto
                {
                    StatusCode = 200,
                    Message = "Lấy thông báo mới thành công",
                    success = true,
                    Notifications = notificationDtos,
                    TotalCount = totalCount,
                    PageIndex = 0,
                    PageSize = totalCount,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest notifications for user {UserId}", userId);
                return new NotificationsResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        #region Withdrawal System Notifications

        /// <summary>
        /// Tạo thông báo cho admin khi có yêu cầu rút tiền mới
        /// </summary>
        public async Task<BaseResposeDto> CreateNewWithdrawalRequestNotificationAsync(
            Guid withdrawalRequestId,
            string shopName,
            decimal amount)
        {
            try
            {
                // Lấy danh sách admin users
                var adminUsers = await _unitOfWork.UserRepository.GetUsersByRoleAsync("Admin");

                foreach (var admin in adminUsers)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = admin.Id,
                        Title = "Yêu cầu rút tiền mới",
                        Message = $"Shop {shopName} đã tạo yêu cầu rút tiền {amount:N0} VNĐ",
                        Type = NotificationType.System,
                        Priority = NotificationPriority.High,
                        AdditionalData = $"{{\"withdrawalRequestId\":\"{withdrawalRequestId}\",\"shopName\":\"{shopName}\",\"amount\":{amount}}}",
                        ActionUrl = $"/admin/withdrawals/{withdrawalRequestId}",
                        Icon = "💰",
                        ExpiresAt = DateTime.UtcNow.AddDays(7),
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.NotificationRepository.AddAsync(notification);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created new withdrawal request notifications for {AdminCount} admins. Request: {WithdrawalRequestId}",
                    adminUsers.Count(), withdrawalRequestId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã tạo thông báo cho admin thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new withdrawal request notification for request {WithdrawalRequestId}", withdrawalRequestId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Tạo thông báo cho user khi yêu cầu rút tiền được duyệt
        /// </summary>
        public async Task<BaseResposeDto> CreateWithdrawalApprovedNotificationAsync(
            Guid userId,
            Guid withdrawalRequestId,
            decimal amount,
            string bankAccount,
            string? transactionReference)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "Yêu cầu rút tiền đã được duyệt",
                    Message = $"Yêu cầu rút tiền {amount:N0} VNĐ đã được duyệt. Tiền sẽ được chuyển vào tài khoản {bankAccount}",
                    Type = NotificationType.System,
                    Priority = NotificationPriority.High,
                    AdditionalData = $"{{\"withdrawalRequestId\":\"{withdrawalRequestId}\",\"amount\":{amount},\"bankAccount\":\"{bankAccount}\",\"transactionReference\":\"{transactionReference}\"}}",
                    ActionUrl = $"/shop/withdrawals/{withdrawalRequestId}",
                    Icon = "✅",
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.NotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created withdrawal approved notification for user {UserId}. Request: {WithdrawalRequestId}",
                    userId, withdrawalRequestId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã tạo thông báo duyệt rút tiền thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating withdrawal approved notification for user {UserId}", userId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Tạo thông báo cho user khi yêu cầu rút tiền bị từ chối
        /// </summary>
        public async Task<BaseResposeDto> CreateWithdrawalRejectedNotificationAsync(
            Guid userId,
            Guid withdrawalRequestId,
            decimal amount,
            string reason)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "Yêu cầu rút tiền bị từ chối",
                    Message = $"Yêu cầu rút tiền {amount:N0} VNĐ đã bị từ chối. Lý do: {reason}",
                    Type = NotificationType.Warning,
                    Priority = NotificationPriority.High,
                    AdditionalData = $"{{\"withdrawalRequestId\":\"{withdrawalRequestId}\",\"amount\":{amount},\"reason\":\"{reason}\"}}",
                    ActionUrl = $"/shop/withdrawals/{withdrawalRequestId}",
                    Icon = "❌",
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.NotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created withdrawal rejected notification for user {UserId}. Request: {WithdrawalRequestId}",
                    userId, withdrawalRequestId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã tạo thông báo từ chối rút tiền thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating withdrawal rejected notification for user {UserId}", userId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Tạo thông báo nhắc nhở admin về yêu cầu rút tiền đã chờ lâu
        /// </summary>
        public async Task<BaseResposeDto> CreateWithdrawalReminderNotificationAsync(
            Guid withdrawalRequestId,
            string shopName,
            decimal amount,
            int daysPending)
        {
            try
            {
                // Lấy danh sách admin users
                var adminUsers = await _unitOfWork.UserRepository.GetUsersByRoleAsync("Admin");

                foreach (var admin in adminUsers)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = admin.Id,
                        Title = "Yêu cầu rút tiền cần xử lý gấp",
                        Message = $"Yêu cầu rút tiền {amount:N0} VNĐ của shop {shopName} đã chờ {daysPending} ngày",
                        Type = NotificationType.Warning,
                        Priority = NotificationPriority.Urgent,
                        AdditionalData = $"{{\"withdrawalRequestId\":\"{withdrawalRequestId}\",\"shopName\":\"{shopName}\",\"amount\":{amount},\"daysPending\":{daysPending}}}",
                        ActionUrl = $"/admin/withdrawals/{withdrawalRequestId}",
                        Icon = "⏰",
                        ExpiresAt = DateTime.UtcNow.AddDays(3),
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.NotificationRepository.AddAsync(notification);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created withdrawal reminder notifications for {AdminCount} admins. Request: {WithdrawalRequestId}, Days pending: {DaysPending}",
                    adminUsers.Count(), withdrawalRequestId, daysPending);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã tạo thông báo nhắc nhở admin thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating withdrawal reminder notification for request {WithdrawalRequestId}", withdrawalRequestId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        #endregion
    }
}