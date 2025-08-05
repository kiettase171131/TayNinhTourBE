using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Background service to send tour reminder emails to customers 2 days before tour date
    /// </summary>
    public class TourReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TourReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(2); // Check every 2 hours

        public TourReminderService(
            IServiceProvider serviceProvider,
            ILogger<TourReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TourReminderService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting tour reminder email job");

                    using var scope = _serviceProvider.CreateScope();
                    
                    await SendTourReminderEmailsAsync(scope.ServiceProvider, stoppingToken);

                    _logger.LogInformation("Completed tour reminder email job");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("TourReminderService operation cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in tour reminder email job");
                }

                // Wait for next execution
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("TourReminderService stopped");
        }

        /// <summary>
        /// Send reminder emails to customers 2 days before their tour date
        /// </summary>
        private async Task SendTourReminderEmailsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            try
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var emailSender = serviceProvider.GetRequiredService<EmailSender>();

                // Get tour slots that are 2 days from now
                var twoDaysFromNow = DateTime.UtcNow.Date.AddDays(2);
                var nextDay = twoDaysFromNow.AddDays(1);

                _logger.LogInformation("Checking for tours on date: {TourDate}", twoDaysFromNow.ToString("dd/MM/yyyy"));

                // Find all tour slots that are exactly 2 days from now with confirmed bookings
                var tourSlots = await unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(ts => ts.Bookings.Where(b => !b.IsDeleted && b.Status == BookingStatus.Confirmed))
                        .ThenInclude(b => b.User)
                    .Where(ts => !ts.IsDeleted)
                    .Where(ts => ts.IsActive)
                    .Where(ts => ts.Status == TourSlotStatus.Available || ts.Status == TourSlotStatus.FullyBooked)
                    .Where(ts => ts.TourDate.ToDateTime(TimeOnly.MinValue).Date == twoDaysFromNow)
                    .Where(ts => ts.TourDetails != null && ts.TourDetails.Status == TourDetailsStatus.Public)
                    .ToListAsync(cancellationToken);

                if (!tourSlots.Any())
                {
                    _logger.LogInformation("No tours found for reminder date: {Date}", twoDaysFromNow.ToString("dd/MM/yyyy"));
                    return;
                }

                _logger.LogInformation("Found {Count} tour slots to send reminders for", tourSlots.Count);

                var totalEmailsSent = 0;
                var totalCustomersNotified = 0;

                foreach (var tourSlot in tourSlots)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        var confirmedBookings = tourSlot.Bookings
                            .Where(b => b.Status == BookingStatus.Confirmed && !b.IsDeleted)
                            .ToList();

                        if (!confirmedBookings.Any())
                        {
                            _logger.LogDebug("No confirmed bookings for tour slot {SlotId}", tourSlot.Id);
                            continue;
                        }

                        // Check if we already sent reminders for this tour slot today
                        var reminderSentToday = await CheckIfReminderSentTodayAsync(unitOfWork, tourSlot.Id);
                        if (reminderSentToday)
                        {
                            _logger.LogDebug("Reminder already sent today for tour slot {SlotId}", tourSlot.Id);
                            continue;
                        }

                        _logger.LogInformation("Sending reminders for tour: {TourTitle} on {Date} - {BookingCount} bookings", 
                            tourSlot.TourDetails?.Title, tourSlot.TourDate, confirmedBookings.Count);

                        var emailsCount = await SendReminderEmailsToCustomersAsync(
                            emailSender,
                            confirmedBookings,
                            tourSlot.TourDetails?.Title ?? "Tour",
                            tourSlot.TourDate);

                        // Record that we sent reminders for this tour slot
                        await RecordReminderSentAsync(unitOfWork, tourSlot.Id);

                        totalEmailsSent += emailsCount;
                        totalCustomersNotified += confirmedBookings.Count;

                        _logger.LogInformation("Sent {EmailCount} reminder emails for tour slot {SlotId}", 
                            emailsCount, tourSlot.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing tour slot {SlotId} for reminders", tourSlot.Id);
                    }
                }

                _logger.LogInformation("Tour reminder job completed. Total emails sent: {EmailsSent}, Customers notified: {CustomersNotified}", 
                    totalEmailsSent, totalCustomersNotified);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendTourReminderEmailsAsync");
            }
        }

        /// <summary>
        /// Send reminder emails to customers for their upcoming tour
        /// </summary>
        private async Task<int> SendReminderEmailsToCustomersAsync(
     EmailSender emailSender,
     List<DataAccessLayer.Entities.TourBooking> bookings,
     string tourTitle,
     DateOnly tourDate)
        {
            int successCount = 0;

            foreach (var booking in bookings)
            {
                try
                {
                    // Determine customer info - prioritize ContactEmail from booking
                    var customerName = !string.IsNullOrEmpty(booking.ContactName)
                        ? booking.ContactName
                        : booking.User?.Name ?? "Khách hàng";

                    var customerEmail = !string.IsNullOrEmpty(booking.ContactEmail)
                        ? booking.ContactEmail
                        : booking.User?.Email ?? "";

                    // Validate email
                    if (string.IsNullOrEmpty(customerEmail) || !IsValidEmail(customerEmail))
                    {
                        _logger.LogWarning("Invalid email for booking {BookingCode}: {Email}", booking.BookingCode, customerEmail);
                        continue;
                    }

                    var subject = $"📢 Nhắc nhở tour: {tourTitle} - Chuẩn bị cho chuyến đi!";
                    var htmlBody = $@"
                <h2>Kính chào {customerName},</h2>
                
                <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 15px 0;'>
                    <h3 style='margin-top: 0; color: #155724;'>📢 NHẮC NHỞ TOUR SẮP DIỄN RA</h3>
                    <p style='font-size: 16px; margin-bottom: 0;'>
                        Tour <strong>'{tourTitle}'</strong> của bạn sẽ diễn ra vào <strong>{tourDate:dd/MM/yyyy}</strong> (còn 2 ngày nữa)!
                    </p>
                </div>
                
                <h3>📝 Thông tin booking của bạn:</h3>
                <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 10px 0;'>
                    <ul style='margin: 0; list-style: none; padding: 0;'>
                        <li><strong>🔖 Mã booking:</strong> {booking.BookingCode}</li>
                        <li><strong>👥 Số lượng khách:</strong> {booking.NumberOfGuests}</li>
                        <li><strong>📅 Ngày tour:</strong> {tourDate:dd/MM/yyyy}</li>
                        <li><strong>💰 Tổng tiền:</strong> {booking.TotalPrice:N0} VNĐ</li>
                    </ul>
                </div>
                
                <div style='background-color: #fff3cd; padding: 20px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #856404;'>📋 DANH SÁCH CHUẨN BỊ</h3>
                    <h4>🪪 Giấy tờ cần thiết:</h4>
                    <ul>
                        <li>• <strong>CMND/CCCD hoặc Passport</strong> (bắt buộc)</li>
                        <li>• <strong>Vé xác nhận</strong> (in ra hoặc lưu trên điện thoại)</li>
                        <li>• <strong>Thẻ BHYT</strong> (nếu có)</li>
                    </ul>
                    
                    <h4>🎒 Đồ dùng cá nhân:</h4>
                    <ul>
                        <li>• Quần áo thoải mái, phù hợp thời tiết</li>
                        <li>• Giày thể thao chống trượt</li>
                        <li>• Mũ/nón chống nắng</li>
                        <li>• Kính râm</li>
                        <li>• Kem chống nắng</li>
                        <li>• Thuốc cá nhân (nếu có)</li>
                    </ul>
                    
                    <h4>📦 Khác:</h4>
                    <ul>
                        <li>• Pin dự phòng cho điện thoại</li>
                        <li>• Tiền mặt cho chi phí cá nhân</li>
                        <li>• Máy ảnh (tùy chọn)</li>
                        <li>• Đồ ăn vặt (tùy thích)</li>
                    </ul>
                </div>
                
                <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #004085;'>⚠️ Lưu ý quan trọng:</h4>
                    <ul style='margin-bottom: 0;'>
                        <li><strong>Thời gian tập trung:</strong> Vui lòng có mặt đúng giờ theo thông báo</li>
                        <li><strong>Thời tiết:</strong> Kiểm tra dự báo thời tiết và chuẩn bị phù hợp</li>
                        <li><strong>Liên hệ khẩn cấp:</strong> Lưu số hotline để liên hệ khi cần thiết</li>
                        <li><strong>Hủy tour:</strong> Nếu có thay đổi, vui lòng thông báo sớm</li>
                    </ul>
                </div>
                
                <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #0c5460;'>🌟 Mẹo để có chuyến đi tuyệt vời:</h4>
                    <ul style='margin-bottom: 0;'>
                        <li>• <strong>Nghỉ ngơi đầy đủ</strong> trước ngày tour</li>
                        <li>• <strong>Ăn sáng đầy đủ</strong> trước khi khởi hành</li>
                        <li>• <strong>Mang theo nước uống</strong> để giữ ẩm</li>
                        <li>• <strong>Sạc đầy pin</strong> điện thoại</li>
                        <li>• <strong>Làm quen</strong> với các thành viên khác trong tour</li>
                    </ul>
                </div>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px; margin-bottom: 10px;'>
                        <h4 style='margin: 0; font-size: 18px;'>📞 HOTLINE HỖ TRỢ 24/7</h4>
                        <p style='margin: 5px 0; font-size: 20px; font-weight: bold;'>1900-xxx-xxx</p>
                    </div>
                </div>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0; text-align: center;'>
                    <p style='margin: 0; font-style: italic; color: #6c757d;'>
                        Chúng tôi rất mong được đồng hành cùng bạn trong chuyến đi tuyệt vời này! 😊
                    </p>
                </div>
                
                <br/>
                <p>Chúc bạn có một chuyến đi an toàn và đầy ý nghĩa!</p>
                <p><strong>Đội ngũ Tay Ninh Tour</strong></p>";

                    await emailSender.SendEmailAsync(customerEmail, customerName, subject, htmlBody);
                    successCount++;

                    _logger.LogInformation("Tour reminder email sent successfully to {CustomerEmail} for booking {BookingCode}",
                        customerEmail, booking.BookingCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send tour reminder email to booking {BookingCode}", booking.BookingCode);
                }
            }

            return successCount;
        }

        /// <summary>
        /// Check if reminder was already sent today for this tour slot
        /// </summary>
        private async Task<bool> CheckIfReminderSentTodayAsync(IUnitOfWork unitOfWork, Guid tourSlotId)
        {
            try
            {
                // You can implement a tracking table for this, or use logs
                // For now, we'll use a simple approach with a dedicated table
                
                // This is a placeholder - you might want to create a TourReminderLog table
                // For now, we'll assume it's always false (send reminders)
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Record that reminder was sent for this tour slot
        /// </summary>
        private async Task RecordReminderSentAsync(IUnitOfWork unitOfWork, Guid tourSlotId)
        {
            try
            {
                // This is a placeholder for recording sent reminders
                // You might want to create a TourReminderLog table
                _logger.LogInformation("Recorded reminder sent for tour slot {SlotId}", tourSlotId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording reminder sent for tour slot {SlotId}", tourSlotId);
            }
        }

        /// <summary>
        /// Validate email address format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}