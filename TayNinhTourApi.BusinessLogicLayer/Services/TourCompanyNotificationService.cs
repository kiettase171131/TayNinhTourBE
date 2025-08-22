using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service gửi thông báo cho TourCompany về các sự kiện booking
    /// OPTIMIZED: Chỉ gửi in-app notifications, bỏ email để tối ưu hiệu năng
    /// </summary>
    public class TourCompanyNotificationService : ITourCompanyNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public TourCompanyNotificationService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gửi thông báo khi có booking mới
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email
        /// </summary>
        public async Task<bool> NotifyNewBookingAsync(Guid tourCompanyUserId, TourBookingDto booking)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateNewBookingNotificationAsync(tourCompanyUserId, booking);

                Console.WriteLine($"New booking notification sent (in-app only) for user {tourCompanyUserId}, booking: {booking.BookingCode}");
                return true;
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking the main flow
                Console.WriteLine($"Error sending new booking notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi tour bị hủy tự động
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email
        /// </summary>
        public async Task<bool> NotifyTourCancellationAsync(
            Guid tourCompanyUserId, 
            List<TourBookingDto> affectedBookings, 
            string tourTitle, 
            DateTime tourStartDate, 
            string reason)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateTourCancellationNotificationAsync(
                    tourCompanyUserId, affectedBookings, tourTitle, tourStartDate, reason);

                Console.WriteLine($"Tour cancellation notification sent (in-app only) for user {tourCompanyUserId}, tour: {tourTitle}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending tour cancellation notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi khách hàng hủy booking
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email
        /// </summary>
        public async Task<bool> NotifyBookingCancellationAsync(Guid tourCompanyUserId, TourBookingDto booking, string? reason)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateBookingCancellationNotificationAsync(tourCompanyUserId, booking, reason);

                Console.WriteLine($"Booking cancellation notification sent (in-app only) for user {tourCompanyUserId}, booking: {booking.BookingCode}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending booking cancellation notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi tiền được chuyển từ revenue hold sang wallet
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email
        /// </summary>
        public async Task<bool> NotifyRevenueTransferAsync(
            Guid tourCompanyUserId, 
            decimal amount, 
            string tourTitle, 
            DateTime tourCompletedDate)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateNotificationAsync(new DTOs.Request.Notification.CreateNotificationDto
                {
                    UserId = tourCompanyUserId,
                    Title = "💰 Tiền tour đã chuyển vào ví",
                    Message = $"Tiền từ tour '{tourTitle}' ({amount:N0} VNĐ) đã được chuyển vào ví. Bạn có thể rút tiền ngay!",
                    Type = DataAccessLayer.Enums.NotificationType.Wallet,
                    Priority = DataAccessLayer.Enums.NotificationPriority.Medium,
                    Icon = "💰",
                    ActionUrl = "/tour-company/wallet"
                });

                Console.WriteLine($"Revenue transfer notification sent (in-app only) for user {tourCompanyUserId}, amount: {amount:N0} VNĐ");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending revenue transfer notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi email thông báo cho TourCompany
        /// DEPRECATED: Method deprecated as we only use in-app notifications now
        /// </summary>
        public async Task<bool> SendEmailNotificationAsync(Guid tourCompanyUserId, string subject, string htmlBody)
        {
            // ⚡ EMAIL FUNCTIONALITY REMOVED FOR PERFORMANCE OPTIMIZATION
            Console.WriteLine($"Email notification skipped (performance optimization) for user {tourCompanyUserId}, subject: {subject}");
            return true; // Return true to maintain compatibility
        }

        /// <summary>
        /// Gửi thông báo khi TourGuide từ chối lời mời
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email
        /// </summary>
        public async Task<bool> NotifyGuideRejectionAsync(
            Guid tourCompanyUserId,
            string tourDetailsTitle,
            string guideFullName,
            string? rejectionReason)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateGuideRejectionNotificationAsync(
                    tourCompanyUserId, tourDetailsTitle, guideFullName, rejectionReason);

                Console.WriteLine($"Guide rejection notification sent (in-app only) for user {tourCompanyUserId}, tour: {tourDetailsTitle}, guide: {guideFullName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending guide rejection notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi lời mời hết hạn sau 24h và cần tìm guide thủ công
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email
        /// </summary>
        public async Task<bool> NotifyManualGuideSelectionNeededAsync(
            Guid tourCompanyUserId,
            string tourDetailsTitle,
            int expiredInvitationsCount)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateManualGuideSelectionNotificationAsync(
                    tourCompanyUserId, tourDetailsTitle, expiredInvitationsCount);

                Console.WriteLine($"Manual guide selection notification sent (in-app only) for user {tourCompanyUserId}, tour: {tourDetailsTitle}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending manual guide selection notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi tất cả guides không phản hồi và tour sắp bị hủy
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email
        /// </summary>
        public async Task<bool> NotifyTourRiskCancellationAsync(
            Guid tourCompanyUserId,
            string tourDetailsTitle,
            int daysUntilCancellation)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateTourRiskCancellationNotificationAsync(
                    tourCompanyUserId, tourDetailsTitle, daysUntilCancellation);

                Console.WriteLine($"Tour risk cancellation notification sent (in-app only) for user {tourCompanyUserId}, tour: {tourDetailsTitle}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending tour risk cancellation notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi admin duyệt tour details
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email để giảm thời gian phản hồi
        /// </summary>
        public async Task<bool> NotifyTourApprovalAsync(
            Guid tourCompanyUserId,
            string tourDetailsTitle,
            string? adminComment = null)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateNotificationAsync(new DTOs.Request.Notification.CreateNotificationDto
                {
                    UserId = tourCompanyUserId,
                    Title = "✅ Tour được duyệt",
                    Message = $"Tour '{tourDetailsTitle}' đã được admin duyệt và có thể bắt đầu mời hướng dẫn viên!",
                    Type = DataAccessLayer.Enums.NotificationType.Tour,
                    Priority = DataAccessLayer.Enums.NotificationPriority.High,
                    Icon = "✅",
                    ActionUrl = "/tour-company/tours"
                });

                Console.WriteLine($"Tour approval notification sent (in-app only) for user {tourCompanyUserId}, tour: {tourDetailsTitle}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending tour approval notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi admin từ chối tour details
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email để giảm thời gian phản hồi
        /// </summary>
        public async Task<bool> NotifyTourRejectionAsync(
            Guid tourCompanyUserId,
            string tourDetailsTitle,
            string rejectionReason)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateNotificationAsync(new DTOs.Request.Notification.CreateNotificationDto
                {
                    UserId = tourCompanyUserId,
                    Title = "❌ Tour bị từ chối",
                    Message = $"Tour '{tourDetailsTitle}' đã bị admin từ chối. Lý do: {rejectionReason}. Vui lòng chỉnh sửa và gửi lại.",
                    Type = DataAccessLayer.Enums.NotificationType.Warning,
                    Priority = DataAccessLayer.Enums.NotificationPriority.High,
                    Icon = "❌",
                    ActionUrl = "/tour-company/tours"
                });

                Console.WriteLine($"Tour rejection notification sent (in-app only) for user {tourCompanyUserId}, tour: {tourDetailsTitle}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending tour rejection notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi TourGuide chấp nhận lời mời tour
        /// OPTIMIZED: Chỉ gửi in-app notification, không gửi email
        /// </summary>
        public async Task<bool> NotifyGuideAcceptanceAsync(
            Guid tourCompanyUserId,
            string tourDetailsTitle,
            string guideFullName,
            string guideEmail,
            DateTime acceptedAt)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                // 🔔 Tạo in-app notification ONLY
                await _notificationService.CreateNotificationAsync(new DTOs.Request.Notification.CreateNotificationDto
                {
                    UserId = tourCompanyUserId,
                    Title = "🎉 Hướng dẫn viên chấp nhận!",
                    Message = $"{guideFullName} đã chấp nhận lời mời cho tour '{tourDetailsTitle}'. Tour sẵn sàng để public!",
                    Type = DataAccessLayer.Enums.NotificationType.TourGuide,
                    Priority = DataAccessLayer.Enums.NotificationPriority.High,
                    Icon = "🎉",
                    ActionUrl = "/tour-company/tours"
                });

                Console.WriteLine($"Guide acceptance notification sent (in-app only) for user {tourCompanyUserId}, tour: {tourDetailsTitle}, guide: {guideFullName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending guide acceptance notification: {ex.Message}");
                return false;
            }
        }
    }
}
