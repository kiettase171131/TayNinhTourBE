using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service gửi thông báo cho TourCompany về các sự kiện booking
    /// </summary>
    public interface ITourCompanyNotificationService
    {
        /// <summary>
        /// Gửi thông báo khi có booking mới
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <param name="booking">Thông tin booking</param>
        /// <returns>Kết quả gửi thông báo</returns>
        Task<bool> NotifyNewBookingAsync(Guid tourCompanyUserId, TourBookingDto booking);

        /// <summary>
        /// Gửi thông báo khi tour bị hủy tự động do không đủ khách
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <param name="affectedBookings">Danh sách bookings bị ảnh hưởng</param>
        /// <param name="tourTitle">Tên tour bị hủy</param>
        /// <param name="tourStartDate">Ngày khởi hành tour</param>
        /// <param name="reason">Lý do hủy</param>
        /// <returns>Kết quả gửi thông báo</returns>
        Task<bool> NotifyTourCancellationAsync(
            Guid tourCompanyUserId, 
            List<TourBookingDto> affectedBookings, 
            string tourTitle, 
            DateTime tourStartDate, 
            string reason);

        /// <summary>
        /// Gửi thông báo khi khách hàng hủy booking
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <param name="booking">Thông tin booking bị hủy</param>
        /// <param name="reason">Lý do hủy</param>
        /// <returns>Kết quả gửi thông báo</returns>
        Task<bool> NotifyBookingCancellationAsync(Guid tourCompanyUserId, TourBookingDto booking, string? reason);

        /// <summary>
        /// Gửi thông báo khi tiền được chuyển từ revenue hold sang wallet
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <param name="amount">Số tiền được chuyển</param>
        /// <param name="tourTitle">Tên tour</param>
        /// <param name="tourCompletedDate">Ngày hoàn thành tour</param>
        /// <returns>Kết quả gửi thông báo</returns>
        Task<bool> NotifyRevenueTransferAsync(
            Guid tourCompanyUserId, 
            decimal amount, 
            string tourTitle, 
            DateTime tourCompletedDate);

        /// <summary>
        /// Gửi email thông báo cho TourCompany
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="htmlBody">Nội dung email HTML</param>
        /// <returns>Kết quả gửi email</returns>
        Task<bool> SendEmailNotificationAsync(Guid tourCompanyUserId, string subject, string htmlBody);
    }
}
