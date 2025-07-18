using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service gửi thông báo cho TourCompany về các sự kiện booking
    /// Bao gồm cả email và in-app notifications
    /// </summary>
    public class TourCompanyNotificationService : ITourCompanyNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;
        private readonly INotificationService _notificationService;

        public TourCompanyNotificationService(
            IUnitOfWork unitOfWork,
            EmailSender emailSender,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gửi thông báo khi có booking mới
        /// </summary>
        public async Task<bool> NotifyNewBookingAsync(Guid tourCompanyUserId, TourBookingDto booking)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                var subject = "Thông báo: Có booking tour mới";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    <p>Bạn có một booking tour mới với thông tin sau:</p>
                    <ul>
                        <li><strong>Mã booking:</strong> {booking.BookingCode}</li>
                        <li><strong>Tour:</strong> {booking.TourOperation?.TourTitle}</li>
                        <li><strong>Số khách:</strong> {booking.NumberOfGuests}</li>
                        <li><strong>Tổng tiền:</strong> {booking.TotalPrice:N0} VNĐ</li>
                        <li><strong>Ngày đặt:</strong> {booking.BookingDate:dd/MM/yyyy HH:mm}</li>
                        <li><strong>Ngày khởi hành:</strong> {booking.TourOperation?.TourStartDate:dd/MM/yyyy}</li>
                    </ul>
                    <p>Vui lòng kiểm tra và chuẩn bị cho tour.</p>
                    <br/>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                // 🔔 Tạo in-app notification
                await _notificationService.CreateNewBookingNotificationAsync(tourCompanyUserId, booking);

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
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

                var totalRefundAmount = affectedBookings.Sum(b => b.TotalPrice);
                var bookingsList = string.Join("", affectedBookings.Select(b => 
                    $"<li>{b.BookingCode} - {b.NumberOfGuests} khách - {b.TotalPrice:N0} VNĐ</li>"));

                var subject = $"Thông báo: Tour '{tourTitle}' đã bị hủy tự động";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    <p>Tour <strong>'{tourTitle}'</strong> dự kiến khởi hành ngày <strong>{tourStartDate:dd/MM/yyyy}</strong> đã bị hủy tự động.</p>
                    <p><strong>Lý do:</strong> {reason}</p>
                    
                    <h3>Các booking bị ảnh hưởng:</h3>
                    <ul>
                        {bookingsList}
                    </ul>
                    
                    <p><strong>Tổng số tiền cần hoàn:</strong> {totalRefundAmount:N0} VNĐ</p>
                    <p>Số tiền này đã được trừ khỏi revenue hold của bạn để chuẩn bị hoàn tiền cho khách hàng.</p>
                    
                    <p>Chúng tôi đã gửi email thông báo hủy tour cho tất cả khách hàng bị ảnh hưởng.</p>
                    
                    <br/>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                // 🔔 Tạo in-app notification
                await _notificationService.CreateTourCancellationNotificationAsync(
                    tourCompanyUserId, affectedBookings, tourTitle, tourStartDate, reason);

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending tour cancellation notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi khách hàng hủy booking
        /// </summary>
        public async Task<bool> NotifyBookingCancellationAsync(Guid tourCompanyUserId, TourBookingDto booking, string? reason)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                var subject = "Thông báo: Khách hàng đã hủy booking";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    <p>Khách hàng đã hủy booking với thông tin sau:</p>
                    <ul>
                        <li><strong>Mã booking:</strong> {booking.BookingCode}</li>
                        <li><strong>Tour:</strong> {booking.TourOperation?.TourTitle}</li>
                        <li><strong>Số khách:</strong> {booking.NumberOfGuests}</li>
                        <li><strong>Tổng tiền:</strong> {booking.TotalPrice:N0} VNĐ</li>
                        <li><strong>Ngày đặt:</strong> {booking.BookingDate:dd/MM/yyyy HH:mm}</li>
                        <li><strong>Ngày hủy:</strong> {booking.CancelledDate:dd/MM/yyyy HH:mm}</li>
                        <li><strong>Lý do hủy:</strong> {reason ?? "Không có lý do cụ thể"}</li>
                    </ul>
                    <p>Slot đã được giải phóng và có thể nhận booking mới.</p>
                    <br/>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                // 🔔 Tạo in-app notification
                await _notificationService.CreateBookingCancellationNotificationAsync(tourCompanyUserId, booking, reason);

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending booking cancellation notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi tiền được chuyển từ revenue hold sang wallet
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

                var subject = "Thông báo: Tiền tour đã được chuyển vào ví";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    <p>Tiền từ tour <strong>'{tourTitle}'</strong> đã được chuyển vào ví của bạn.</p>
                    <ul>
                        <li><strong>Số tiền:</strong> {amount:N0} VNĐ</li>
                        <li><strong>Tour:</strong> {tourTitle}</li>
                        <li><strong>Ngày hoàn thành tour:</strong> {tourCompletedDate:dd/MM/yyyy}</li>
                        <li><strong>Ngày chuyển tiền:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
                    </ul>
                    <p>Tiền đã được chuyển từ revenue hold sang wallet và bạn có thể rút tiền.</p>
                    <br/>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending revenue transfer notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi email thông báo cho TourCompany
        /// </summary>
        public async Task<bool> SendEmailNotificationAsync(Guid tourCompanyUserId, string subject, string htmlBody)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return false;

                await _emailSender.SendEmailAsync(user.Email, user.Name, subject, htmlBody);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi TourGuide từ chối lời mời
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

                // 🔔 Tạo in-app notification
                await _notificationService.CreateGuideRejectionNotificationAsync(
                    tourCompanyUserId, tourDetailsTitle, guideFullName, rejectionReason);

                // 📧 Gửi email notification
                var subject = $"Thông báo: Hướng dẫn viên từ chối tour '{tourDetailsTitle}'";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    <p>Hướng dẫn viên <strong>{guideFullName}</strong> đã từ chối lời mời cho tour <strong>'{tourDetailsTitle}'</strong>.</p>
                    
                    {(!string.IsNullOrEmpty(rejectionReason) ? $@"
                    <h3>Lý do từ chối:</h3>
                    <div style='background-color: #f5f5f5; padding: 15px; border-left: 4px solid #ff6b6b; margin: 10px 0;'>
                        <p><em>{rejectionReason}</em></p>
                    </div>" : "")}
                    
                    <h3>Hành động tiếp theo:</h3>
                    <ul>
                        <li>Mời hướng dẫn viên khác thủ công</li>
                        <li>Kiểm tra và điều chỉnh yêu cầu kỹ năng nếu cần</li>
                        <li>Xem xét tăng mức phí hoặc điều kiện tour</li>
                    </ul>
                    
                    <p><strong>Gợi ý:</strong> Đăng nhập vào hệ thống để xem danh sách hướng dẫn viên có sẵn và gửi lời mời thủ công.</p>
                    
                    <br/>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending guide rejection notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi lời mời hết hạn sau 24h và cần tìm guide thủ công
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

                // 🔔 Tạo in-app notification
                await _notificationService.CreateManualGuideSelectionNotificationAsync(
                    tourCompanyUserId, tourDetailsTitle, expiredInvitationsCount);

                // 📧 Gửi email notification
                var subject = $"Cần hành động: Tour '{tourDetailsTitle}' chưa có hướng dẫn viên";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    <p>Tour <strong>'{tourDetailsTitle}'</strong> của bạn đã được chuyển sang chế độ tìm kiếm hướng dẫn viên thủ công.</p>
                    
                    <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 15px 0;'>
                        <h3 style='margin-top: 0; color: #856404;'>⚠️ Tình trạng hiện tại:</h3>
                        <ul style='margin-bottom: 0;'>
                            <li><strong>{expiredInvitationsCount}</strong> lời mời đã hết hạn (24 giờ)</li>
                            <li>Chưa có hướng dẫn viên nào chấp nhận</li>
                            <li>Cần tìm hướng dẫn viên thủ công ngay</li>
                        </ul>
                    </div>
                    
                    <h3>🎯 Hành động cần thực hiện:</h3>
                    <ol>
                        <li><strong>Đăng nhập hệ thống</strong> để xem danh sách hướng dẫn viên</li>
                        <li><strong>Gửi lời mời thủ công</strong> cho các hướng dẫn viên phù hợp</li>
                        <li><strong>Xem xét điều chỉnh:</strong>
                            <ul>
                                <li>Yêu cầu kỹ năng</li>
                                <li>Mức phí tour</li>
                                <li>Thời gian tour</li>
                            </ul>
                        </li>
                    </ol>
                    
                    <div style='background-color: #d4edda; padding: 15px; border-left: 4px solid #28a745; margin: 15px 0;'>
                        <p style='margin: 0;'><strong>💡 Lưu ý:</strong> Nếu không tìm được hướng dẫn viên trong <strong>5 ngày</strong>, tour sẽ bị hủy tự động.</p>
                    </div>
                    
                    <br/>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending manual guide selection notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi tất cả guides không phản hồi và tour sắp bị hủy
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

                // 🔔 Tạo in-app notification
                await _notificationService.CreateTourRiskCancellationNotificationAsync(
                    tourCompanyUserId, tourDetailsTitle, daysUntilCancellation);

                // 📧 Gửi email notification
                var subject = $"🚨 KHẨN CẤP: Tour '{tourDetailsTitle}' sắp bị hủy";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    
                    <div style='background-color: #f8d7da; padding: 20px; border-left: 4px solid #dc3545; margin: 15px 0;'>
                        <h3 style='margin-top: 0; color: #721c24;'>🚨 CẢNH BÁO KHẨN CẤP</h3>
                        <p style='font-size: 16px; margin-bottom: 0;'>
                            Tour <strong>'{tourDetailsTitle}'</strong> sẽ bị <strong>HỦY TỰ ĐỘNG</strong> trong <strong>{daysUntilCancellation} ngày</strong> nếu không tìm được hướng dẫn viên!
                        </p>
                    </div>
                    
                    <h3>📊 Tình trạng hiện tại:</h3>
                    <ul>
                        <li>❌ Chưa có hướng dẫn viên chấp nhận</li>
                        <li>⏰ Đã hết thời gian chờ tự động</li>
                        <li>🕒 Còn <strong>{daysUntilCancellation} ngày</strong> trước khi hủy</li>
                    </ul>
                    
                    <h3>⚡ HÀNH ĐỘNG NGAY LẬP TỨC:</h3>
                    <ol style='background-color: #fff3cd; padding: 15px; border-radius: 5px;'>
                        <li><strong>Đăng nhập hệ thống ngay</strong></li>
                        <li><strong>Gửi lời mời thủ công</strong> cho nhiều hướng dẫn viên</li>
                        <li><strong>Liên hệ hotline:</strong> 1900-xxx-xxx để được hỗ trợ</li>
                        <li><strong>Xem xét giảm yêu cầu</strong> hoặc tăng phí để thu hút guide</li>
                    </ol>
                    
                    <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h4 style='margin-top: 0; color: #721c24;'>⚠️ Hậu quả nếu tour bị hủy:</h4>
                        <ul style='margin-bottom: 0;'>
                            <li>Tất cả booking sẽ bị hủy</li>
                            <li>Khách hàng sẽ được hoàn tiền</li>
                            <li>Ảnh hưởng đến uy tín công ty</li>
                            <li>Mất cơ hội kinh doanh</li>
                        </ul>
                    </div>
                    
                    <p style='font-size: 16px; font-weight: bold; color: #dc3545;'>
                        📞 Cần hỗ trợ khẩn cấp? Gọi ngay: <a href='tel:1900-xxx-xxx'>1900-xxx-xxx</a>
                    </p>
                    
                    <br/>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending tour risk cancellation notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi admin duyệt tour details
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

                // 🔔 Tạo in-app notification
                await _notificationService.CreateNotificationAsync(new DTOs.Request.Notification.CreateNotificationDto
                {
                    UserId = tourCompanyUserId,
                    Title = "✅ Tour được duyệt",
                    Message = $"Tour '{tourDetailsTitle}' đã được admin duyệt và có thể bắt đầu mời hướng dẫn viên!",
                    Type = DataAccessLayer.Enums.NotificationType.Tour,
                    Priority = DataAccessLayer.Enums.NotificationPriority.High,
                    Icon = "✅",
                    ActionUrl = "/tours/approved"
                });

                // 📧 Gửi email notification
                var subject = $"🎉 Chúc mừng! Tour '{tourDetailsTitle}' đã được duyệt";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    
                    <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 15px 0;'>
                        <h3 style='margin-top: 0; color: #155724;'>🎉 CHÚC MỪNG!</h3>
                        <p style='font-size: 16px; margin-bottom: 0;'>
                            Tour <strong>'{tourDetailsTitle}'</strong> đã được admin <strong>DUYỆT</strong> và sẵn sàng hoạt động!
                        </p>
                    </div>
                    
                    {(!string.IsNullOrEmpty(adminComment) ? $@"
                    <h3>💬 Nhận xét từ admin:</h3>
                    <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #6c757d; margin: 10px 0;'>
                        <p><em>{adminComment}</em></p>
                    </div>" : "")}
                    
                    <h3>🚀 Bước tiếp theo:</h3>
                    <ol>
                        <li><strong>Kiểm tra lời mời hướng dẫn viên:</strong> Hệ thống đã tự động gửi lời mời cho các hướng dẫn viên phù hợp</li>
                        <li><strong>Theo dõi phản hồi:</strong> Chờ hướng dẫn viên chấp nhận lời mời</li>
                        <li><strong>Chuẩn bị tour:</strong> Sau khi có hướng dẫn viên, tour sẽ sẵn sàng nhận booking</li>
                        <li><strong>Marketing:</strong> Bắt đầu quảng bá tour để thu hút khách hàng</li>
                    </ol>
                    
                    <div style='background-color: #cce5ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h4 style='margin-top: 0; color: #004085;'>📋 Thông tin quan trọng:</h4>
                        <ul style='margin-bottom: 0;'>
                            <li>Tour sẽ tự động chuyển sang trạng thái 'Public' sau khi có hướng dẫn viên</li>
                            <li>Khách hàng có thể đặt booking ngay khi tour ở trạng thái 'Public'</li>
                            <li>Bạn sẽ nhận thông báo khi có booking mới</li>
                        </ul>
                    </div>
                    
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='#' style='background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            🎯 Xem trạng thái tour
                        </a>
                    </p>
                    
                    <br/>
                    <p>Chúc bạn kinh doanh thành công!</p>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending tour approval notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi admin từ chối tour details
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

                // 🔔 Tạo in-app notification
                await _notificationService.CreateNotificationAsync(new DTOs.Request.Notification.CreateNotificationDto
                {
                    UserId = tourCompanyUserId,
                    Title = "❌ Tour bị từ chối",
                    Message = $"Tour '{tourDetailsTitle}' đã bị admin từ chối. Vui lòng kiểm tra lý do và chỉnh sửa lại.",
                    Type = DataAccessLayer.Enums.NotificationType.Warning,
                    Priority = DataAccessLayer.Enums.NotificationPriority.High,
                    Icon = "❌",
                    ActionUrl = "/tours/rejected"
                });

                // 📧 Gửi email notification
                var subject = $"❌ Tour '{tourDetailsTitle}' cần chỉnh sửa";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    
                    <div style='background-color: #f8d7da; padding: 20px; border-left: 4px solid #dc3545; margin: 15px 0;'>
                        <h3 style='margin-top: 0; color: #721c24;'>❌ TOUR CẦN CHỈNH SỬA</h3>
                        <p style='font-size: 16px; margin-bottom: 0;'>
                            Tour <strong>'{tourDetailsTitle}'</strong> chưa được duyệt và cần chỉnh sửa theo yêu cầu.
                        </p>
                    </div>
                    
                    <h3>📝 Lý do từ admin:</h3>
                    <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 10px 0;'>
                        <p style='font-weight: bold; color: #856404;'>{rejectionReason}</p>
                    </div>
                    
                    <h3>🔧 Hành động cần thực hiện:</h3>
                    <ol>
                        <li><strong>Đọc kỹ phản hồi:</strong> Hiểu rõ những điểm cần chỉnh sửa</li>
                        <li><strong>Chỉnh sửa tour:</strong> Cập nhật thông tin theo yêu cầu của admin</li>
                        <li><strong>Kiểm tra lại:</strong> Đảm bảo tour đáp ứng đầy đủ yêu cầu</li>
                        <li><strong>Gửi lại duyệt:</strong> Submit tour để admin xem xét lại</li>
                    </ol>
                    
                    <div style='background-color: #e2f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h4 style='margin-top: 0; color: #004085;'>💡 Gợi ý cải thiện:</h4>
                        <ul style='margin-bottom: 0;'>
                            <li>Cung cấp mô tả chi tiết và rõ ràng về tour</li>
                            <li>Đảm bảo hình ảnh chất lượng cao và phù hợp</li>
                            <li>Kiểm tra thông tin liên hệ và địa điểm chính xác</li>
                            <li>Tuân thủ các quy định và chính sách của platform</li>
                        </ul>
                    </div>
                    
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='#' style='background-color: #ffc107; color: #212529; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            ✏️ Chỉnh sửa tour ngay
                        </a>
                    </p>
                    
                    <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>🤝 Cần hỗ trợ?</strong> Liên hệ team support qua email: support@tayninhour.com hoặc hotline: 1900-xxx-xxx</p>
                    </div>
                    
                    <br/>
                    <p>Chúng tôi mong muốn hỗ trợ bạn tạo ra những tour tuyệt vời!</p>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending tour rejection notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo khi TourGuide chấp nhận lời mời tour
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

                // 🔔 Tạo in-app notification
                await _notificationService.CreateNotificationAsync(new DTOs.Request.Notification.CreateNotificationDto
                {
                    UserId = tourCompanyUserId,
                    Title = "🎉 Hướng dẫn viên chấp nhận!",
                    Message = $"{guideFullName} đã chấp nhận lời mời cho tour '{tourDetailsTitle}'. Tour sẵn sàng để public!",
                    Type = DataAccessLayer.Enums.NotificationType.TourGuide,
                    Priority = DataAccessLayer.Enums.NotificationPriority.High,
                    Icon = "🎉",
                    ActionUrl = "/tours/ready-to-public"
                });

                // 📧 Gửi email notification
                var subject = $"🎉 Tuyệt vời! Hướng dẫn viên đã chấp nhận tour '{tourDetailsTitle}'";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    
                    <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 15px 0;'>
                        <h3 style='margin-top: 0; color: #155724;'>🎉 TIN TUYỆT VỜI!</h3>
                        <p style='font-size: 16px; margin-bottom: 0;'>
                            Hướng dẫn viên <strong>{guideFullName}</strong> đã <strong>CHẤP NHẬN</strong> lời mời cho tour <strong>'{tourDetailsTitle}'</strong>!
                        </p>
                    </div>
                    
                    <h3>👨‍🏫 Thông tin hướng dẫn viên:</h3>
                    <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 10px 0;'>
                        <ul style='margin: 0; list-style: none; padding: 0;'>
                            <li><strong>🙋‍♂️ Tên:</strong> {guideFullName}</li>
                            <li><strong>📧 Email:</strong> {guideEmail}</li>
                            <li><strong>⏰ Chấp nhận lúc:</strong> {acceptedAt:dd/MM/yyyy HH:mm}</li>
                        </ul>
                    </div>
                    
                    <h3>🚀 Bước tiếp theo:</h3>
                    <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                        <ol style='margin: 0;'>
                            <li><strong>✅ Xác nhận thông tin:</strong> Liên hệ với hướng dẫn viên để xác nhận chi tiết tour</li>
                            <li><strong>📅 Lên lịch meeting:</strong> Thảo luận về kế hoạch cụ thể cho tour</li>
                            <li><strong>🌐 Kích hoạt Public:</strong> Tour sẵn sàng để nhận booking từ khách hàng</li>
                            <li><strong>📢 Marketing:</strong> Bắt đầu quảng bá tour để thu hút khách hàng</li>
                        </ol>
                    </div>
                    
                    <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h4 style='margin-top: 0; color: #004085;'>💡 Gợi ý thành công:</h4>
                        <ul style='margin-bottom: 0;'>
                            <li><strong>Giao tiếp sớm:</strong> Liên hệ ngay với hướng dẫn viên để tạo relationship tốt</li>
                            <li><strong>Chuẩn bị kỹ:</strong> Chia sẻ tài liệu chi tiết về tour và khách hàng</li>
                            <li><strong>Feedback loop:</strong> Thiết lập kênh communication thường xuyên</li>
                            <li><strong>Backup plan:</strong> Có kế hoạch dự phòng cho các tình huống bất ngờ</li>
                        </ul>
                    </div>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='mailto:{guideEmail}' style='background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-right: 10px;'>
                            📧 Liên hệ hướng dẫn viên
                        </a>
                        <a href='#' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            🌐 Kích hoạt Public Tour
                        </a>
                    </div>
                    
                    <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>📞 Cần hỗ trợ?</strong> Liên hệ team support để được tư vấn về việc quản lý tour và hướng dẫn viên.</p>
                    </div>
                    
                    <br/>
                    <p>Chúc bạn có một tour thành công và đáng nhớ!</p>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                return await SendEmailNotificationAsync(tourCompanyUserId, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending guide acceptance notification: {ex.Message}");
                return false;
            }
        }
    }
}
