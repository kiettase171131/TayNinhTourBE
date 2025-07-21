using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Notification;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho qu?n lý in-app notifications
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// T?o thông báo m?i
        /// </summary>
        /// <param name="createDto">Thông tin thông báo</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateNotificationAsync(CreateNotificationDto createDto);

        /// <summary>
        /// L?y danh sách thông báo c?a user
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="request">Tham s? phân trang và filter</param>
        /// <returns>Danh sách thông báo</returns>
        Task<NotificationsResponseDto> GetUserNotificationsAsync(Guid userId, GetNotificationsRequestDto request);

        /// <summary>
        /// L?y s? l??ng notifications ch?a ??c c?a user
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>S? l??ng notifications ch?a ??c</returns>
        Task<UnreadCountResponseDto> GetUnreadCountAsync(Guid userId);

        /// <summary>
        /// ?ánh d?u thông báo ?ã ??c
        /// </summary>
        /// <param name="notificationId">ID c?a thông báo</param>
        /// <param name="userId">ID c?a user</param>
        /// <returns>K?t qu? thao tác</returns>
        Task<BaseResposeDto> MarkAsReadAsync(Guid notificationId, Guid userId);

        /// <summary>
        /// ?ánh d?u t?t c? thông báo ?ã ??c
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>K?t qu? thao tác</returns>
        Task<BaseResposeDto> MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        /// <param name="notificationId">ID c?a thông báo</param>
        /// <param name="userId">ID c?a user</param>
        /// <returns>K?t qu? thao tác</returns>
        Task<BaseResposeDto> DeleteNotificationAsync(Guid notificationId, Guid userId);

        /// <summary>
        /// L?y th?ng kê thông báo c?a user
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>Th?ng kê thông báo</returns>
        Task<NotificationStatsDto> GetNotificationStatsAsync(Guid userId);

        /// <summary>
        /// T?o thông báo booking m?i
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="bookingCode">Mã booking</param>
        /// <param name="tourTitle">Tên tour</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateBookingNotificationAsync(Guid userId, string bookingCode, string tourTitle);

        /// <summary>
        /// T?o thông báo TourGuide t? ch?i
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="tourTitle">Tên tour</param>
        /// <param name="guideName">Tên h??ng d?n viên</param>
        /// <param name="rejectionReason">Lý do t? ch?i</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateGuideRejectionNotificationAsync(Guid userId, string tourTitle, string guideName, string? rejectionReason);

        /// <summary>
        /// T?o thông báo c?n tìm guide th? công
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="tourTitle">Tên tour</param>
        /// <param name="expiredCount">S? l??ng l?i m?i h?t h?n</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateManualGuideSelectionNotificationAsync(Guid userId, string tourTitle, int expiredCount);

        /// <summary>
        /// T?o thông báo c?nh báo tour s?p b? h?y
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="tourTitle">Tên tour</param>
        /// <param name="daysUntilCancellation">S? ngày còn l?i</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateTourRiskCancellationNotificationAsync(Guid userId, string tourTitle, int daysUntilCancellation);

        /// <summary>
        /// Cleanup thông báo c? (background job)
        /// </summary>
        /// <param name="olderThanDays">Xóa thông báo c? h?n X ngày</param>
        /// <returns>S? l??ng thông báo ?ã xóa</returns>
        Task<int> CleanupOldNotificationsAsync(int olderThanDays = 30);

        // Additional helper methods for booking notifications

        /// <summary>
        /// T?o thông báo booking m?i (generic method)
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="booking">Thông tin booking</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateNewBookingNotificationAsync(Guid userId, object booking);

        /// <summary>
        /// T?o thông báo h?y tour v?i danh sách bookings
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="affectedBookings">Danh sách bookings b? ?nh h??ng</param>
        /// <param name="tourTitle">Tên tour</param>
        /// <param name="tourStartDate">Ngày kh?i hành</param>
        /// <param name="reason">Lý do h?y</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateTourCancellationNotificationAsync(
            Guid userId, 
            object affectedBookings, 
            string tourTitle, 
            DateTime tourStartDate, 
            string reason);

        /// <summary>
        /// T?o thông báo h?y booking c?a khách hàng
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="booking">Thông tin booking</param>
        /// <param name="reason">Lý do h?y</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateBookingCancellationNotificationAsync(Guid userId, object booking, string? reason);

        /// <summary>
        /// T?o thông báo khi TourGuide ???c m?i tham gia tour
        /// </summary>
        /// <param name="guideUserId">ID c?a User có role TourGuide</param>
        /// <param name="tourTitle">Tên tour</param>
        /// <param name="tourCompanyName">Tên công ty tour</param>
        /// <param name="skillsRequired">K? n?ng yêu c?u</param>
        /// <param name="invitationType">Lo?i l?i m?i (Automatic/Manual)</param>
        /// <param name="expiresAt">Th?i gian h?t h?n</param>
        /// <param name="invitationId">ID c?a invitation ?? action</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateTourGuideInvitationNotificationAsync(
            Guid guideUserId,
            string tourTitle,
            string tourCompanyName,
            string? skillsRequired,
            string invitationType,
            DateTime expiresAt,
            Guid invitationId);

        /// <summary>
        /// T?o thông báo khi invitation s?p h?t h?n (reminder)
        /// </summary>
        /// <param name="guideUserId">ID c?a User có role TourGuide</param>
        /// <param name="tourTitle">Tên tour</param>
        /// <param name="hoursUntilExpiry">S? gi? còn l?i tr??c khi h?t h?n</param>
        /// <param name="invitationId">ID c?a invitation ?? action</param>
        /// <returns>K?t qu? t?o thông báo</returns>
        Task<BaseResposeDto> CreateInvitationExpiryReminderNotificationAsync(
            Guid guideUserId,
            string tourTitle,
            int hoursUntilExpiry,
            Guid invitationId);
    }
}