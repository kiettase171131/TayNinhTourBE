namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái của yêu cầu rút tiền
    /// Workflow: Pending -> Approved/Rejected/Cancelled
    /// </summary>
    public enum WithdrawalStatus
    {
        /// <summary>
        /// Yêu cầu rút tiền đang chờ admin xử lý
        /// Trạng thái mặc định khi user tạo yêu cầu
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Yêu cầu đã được admin duyệt và tiền đã được chuyển
        /// Tiền sẽ được trừ khỏi ví của shop
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Yêu cầu bị admin từ chối
        /// Tiền vẫn còn trong ví của shop
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Yêu cầu bị hủy bởi user trước khi admin xử lý
        /// Chỉ có thể hủy khi status = Pending
        /// </summary>
        Cancelled = 3
    }
}
