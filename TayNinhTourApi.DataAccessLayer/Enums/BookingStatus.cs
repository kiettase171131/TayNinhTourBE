namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái của booking tour
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>
        /// Booking đang chờ xác nhận
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Booking đã được xác nhận
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// Booking đã bị hủy bởi khách hàng
        /// </summary>
        CancelledByCustomer = 2,

        /// <summary>
        /// Booking đã bị hủy bởi tour company
        /// </summary>
        CancelledByCompany = 3,

        /// <summary>
        /// Tour đã hoàn thành
        /// </summary>
        Completed = 4,

        /// <summary>
        /// Khách hàng không xuất hiện
        /// </summary>
        NoShow = 5,

        /// <summary>
        /// Đã hoàn tiền
        /// </summary>
        Refunded = 6
    }
}
