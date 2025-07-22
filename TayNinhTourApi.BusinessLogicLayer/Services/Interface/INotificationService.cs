using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Notification;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho qu?n l� in-app notifications
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// T?o th�ng b�o m?i
        /// </summary>
        /// <param name="createDto">Th�ng tin th�ng b�o</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateNotificationAsync(CreateNotificationDto createDto);

        /// <summary>
        /// L?y danh s�ch th�ng b�o c?a user
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="request">Tham s? ph�n trang v� filter</param>
        /// <returns>Danh s�ch th�ng b�o</returns>
        Task<NotificationsResponseDto> GetUserNotificationsAsync(Guid userId, GetNotificationsRequestDto request);

        /// <summary>
        /// L?y s? l??ng notifications ch?a ??c c?a user
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>S? l??ng notifications ch?a ??c</returns>
        Task<UnreadCountResponseDto> GetUnreadCountAsync(Guid userId);

        /// <summary>
        /// ?�nh d?u th�ng b�o ?� ??c
        /// </summary>
        /// <param name="notificationId">ID c?a th�ng b�o</param>
        /// <param name="userId">ID c?a user</param>
        /// <returns>K?t qu? thao t�c</returns>
        Task<BaseResposeDto> MarkAsReadAsync(Guid notificationId, Guid userId);

        /// <summary>
        /// ?�nh d?u t?t c? th�ng b�o ?� ??c
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>K?t qu? thao t�c</returns>
        Task<BaseResposeDto> MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// X�a th�ng b�o
        /// </summary>
        /// <param name="notificationId">ID c?a th�ng b�o</param>
        /// <param name="userId">ID c?a user</param>
        /// <returns>K?t qu? thao t�c</returns>
        Task<BaseResposeDto> DeleteNotificationAsync(Guid notificationId, Guid userId);

        /// <summary>
        /// L?y th?ng k� th�ng b�o c?a user
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>Th?ng k� th�ng b�o</returns>
        Task<NotificationStatsDto> GetNotificationStatsAsync(Guid userId);

        /// <summary>
        /// T?o th�ng b�o booking m?i
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="bookingCode">M� booking</param>
        /// <param name="tourTitle">T�n tour</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateBookingNotificationAsync(Guid userId, string bookingCode, string tourTitle);

        /// <summary>
        /// T?o th�ng b�o TourGuide t? ch?i
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="tourTitle">T�n tour</param>
        /// <param name="guideName">T�n h??ng d?n vi�n</param>
        /// <param name="rejectionReason">L� do t? ch?i</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateGuideRejectionNotificationAsync(Guid userId, string tourTitle, string guideName, string? rejectionReason);

        /// <summary>
        /// T?o th�ng b�o c?n t�m guide th? c�ng
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="tourTitle">T�n tour</param>
        /// <param name="expiredCount">S? l??ng l?i m?i h?t h?n</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateManualGuideSelectionNotificationAsync(Guid userId, string tourTitle, int expiredCount);

        /// <summary>
        /// T?o th�ng b�o c?nh b�o tour s?p b? h?y
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="tourTitle">T�n tour</param>
        /// <param name="daysUntilCancellation">S? ng�y c�n l?i</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateTourRiskCancellationNotificationAsync(Guid userId, string tourTitle, int daysUntilCancellation);

        /// <summary>
        /// Cleanup th�ng b�o c? (background job)
        /// </summary>
        /// <param name="olderThanDays">X�a th�ng b�o c? h?n X ng�y</param>
        /// <returns>S? l??ng th�ng b�o ?� x�a</returns>
        Task<int> CleanupOldNotificationsAsync(int olderThanDays = 30);

        // Additional helper methods for booking notifications

        /// <summary>
        /// T?o th�ng b�o booking m?i (generic method)
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="booking">Th�ng tin booking</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateNewBookingNotificationAsync(Guid userId, object booking);

        /// <summary>
        /// T?o th�ng b�o h?y tour v?i danh s�ch bookings
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="affectedBookings">Danh s�ch bookings b? ?nh h??ng</param>
        /// <param name="tourTitle">T�n tour</param>
        /// <param name="tourStartDate">Ng�y kh?i h�nh</param>
        /// <param name="reason">L� do h?y</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateTourCancellationNotificationAsync(
            Guid userId, 
            object affectedBookings, 
            string tourTitle, 
            DateTime tourStartDate, 
            string reason);

        /// <summary>
        /// T?o th�ng b�o h?y booking c?a kh�ch h�ng
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="booking">Th�ng tin booking</param>
        /// <param name="reason">L� do h?y</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateBookingCancellationNotificationAsync(Guid userId, object booking, string? reason);

        /// <summary>
        /// T?o th�ng b�o khi TourGuide ???c m?i tham gia tour
        /// </summary>
        /// <param name="guideUserId">ID c?a User c� role TourGuide</param>
        /// <param name="tourTitle">T�n tour</param>
        /// <param name="tourCompanyName">T�n c�ng ty tour</param>
        /// <param name="skillsRequired">K? n?ng y�u c?u</param>
        /// <param name="invitationType">Lo?i l?i m?i (Automatic/Manual)</param>
        /// <param name="expiresAt">Th?i gian h?t h?n</param>
        /// <param name="invitationId">ID c?a invitation ?? action</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateTourGuideInvitationNotificationAsync(
            Guid guideUserId,
            string tourTitle,
            string tourCompanyName,
            string? skillsRequired,
            string invitationType,
            DateTime expiresAt,
            Guid invitationId);

        /// <summary>
        /// T?o th�ng b�o khi invitation s?p h?t h?n (reminder)
        /// </summary>
        /// <param name="guideUserId">ID c?a User c� role TourGuide</param>
        /// <param name="tourTitle">T�n tour</param>
        /// <param name="hoursUntilExpiry">S? gi? c�n l?i tr??c khi h?t h?n</param>
        /// <param name="invitationId">ID c?a invitation ?? action</param>
        /// <returns>K?t qu? t?o th�ng b�o</returns>
        Task<BaseResposeDto> CreateInvitationExpiryReminderNotificationAsync(
            Guid guideUserId,
            string tourTitle,
            int hoursUntilExpiry,
            Guid invitationId);

        // Withdrawal System Notifications

        /// <summary>
        /// Tạo thông báo cho admin khi có yêu cầu rút tiền mới
        /// </summary>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <param name="shopName">Tên shop</param>
        /// <param name="amount">Số tiền yêu cầu rút</param>
        /// <returns>Kết quả tạo thông báo</returns>
        Task<BaseResposeDto> CreateNewWithdrawalRequestNotificationAsync(
            Guid withdrawalRequestId,
            string shopName,
            decimal amount);

        /// <summary>
        /// Tạo thông báo cho user khi yêu cầu rút tiền được duyệt
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <param name="amount">Số tiền được duyệt</param>
        /// <param name="bankAccount">Thông tin tài khoản ngân hàng</param>
        /// <param name="transactionReference">Mã tham chiếu giao dịch</param>
        /// <returns>Kết quả tạo thông báo</returns>
        Task<BaseResposeDto> CreateWithdrawalApprovedNotificationAsync(
            Guid userId,
            Guid withdrawalRequestId,
            decimal amount,
            string bankAccount,
            string? transactionReference);

        /// <summary>
        /// Tạo thông báo cho user khi yêu cầu rút tiền bị từ chối
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <param name="amount">Số tiền bị từ chối</param>
        /// <param name="reason">Lý do từ chối</param>
        /// <returns>Kết quả tạo thông báo</returns>
        Task<BaseResposeDto> CreateWithdrawalRejectedNotificationAsync(
            Guid userId,
            Guid withdrawalRequestId,
            decimal amount,
            string reason);

        /// <summary>
        /// Tạo thông báo nhắc nhở admin về yêu cầu rút tiền đã chờ lâu
        /// </summary>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <param name="shopName">Tên shop</param>
        /// <param name="amount">Số tiền yêu cầu rút</param>
        /// <param name="daysPending">Số ngày đã chờ</param>
        /// <returns>Kết quả tạo thông báo</returns>
        Task<BaseResposeDto> CreateWithdrawalReminderNotificationAsync(
            Guid withdrawalRequestId,
            string shopName,
            decimal amount,
            int daysPending);
    }
}