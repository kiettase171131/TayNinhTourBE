namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái của yêu cầu hoàn tiền tour booking
    /// Workflow: Pending -> Approved/Rejected -> Completed
    /// </summary>
    public enum TourRefundStatus
    {
        /// <summary>
        /// Yêu cầu hoàn tiền đang chờ admin xử lý
        /// Trạng thái mặc định khi tạo yêu cầu
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Yêu cầu đã được admin duyệt, chuẩn bị chuyển tiền
        /// Admin sẽ chuyển tiền thủ công bên ngoài hệ thống
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Yêu cầu bị admin từ chối
        /// Không có hoàn tiền
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Hoàn tiền đã được thực hiện thành công
        /// Admin đã confirm việc chuyển tiền thủ công
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Yêu cầu bị hủy bởi customer trước khi admin xử lý
        /// Chỉ có thể hủy khi status = Pending
        /// </summary>
        Cancelled = 4
    }
}
