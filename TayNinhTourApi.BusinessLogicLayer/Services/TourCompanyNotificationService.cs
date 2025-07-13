using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service gửi thông báo cho TourCompany về các sự kiện booking
    /// </summary>
    public class TourCompanyNotificationService : ITourCompanyNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;

        public TourCompanyNotificationService(
            IUnitOfWork unitOfWork,
            EmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
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
    }
}
