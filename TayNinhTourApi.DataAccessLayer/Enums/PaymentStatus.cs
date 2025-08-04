namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái của giao dịch thanh toán PayOS
    /// Tương tự như StatusEnum trong code mẫu Java
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Giao dịch đang chờ thanh toán
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Giao dịch đã thanh toán thành công
        /// </summary>
        Paid = 1,

        /// <summary>
        /// Giao dịch đã bị hủy
        /// </summary>
        Cancelled = 2,

        /// <summary>
        /// Giao dịch thất bại
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Giao dịch đã hết hạn
        /// </summary>
        Expired = 4,

        /// <summary>
        /// Giao dịch retry (thử lại)
        /// </summary>
        Retry = 5
    }
}
