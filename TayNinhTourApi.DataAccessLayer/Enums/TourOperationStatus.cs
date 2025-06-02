namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Định nghĩa trạng thái của tour operation
    /// </summary>
    public enum TourOperationStatus
    {
        /// <summary>
        /// Operation đã được lên lịch và sẵn sàng
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// Operation đang được thực hiện
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Operation đã hoàn thành thành công
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Operation bị hủy
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Operation bị hoãn
        /// </summary>
        Postponed = 5,

        /// <summary>
        /// Operation đang chờ xác nhận từ guide
        /// </summary>
        PendingConfirmation = 6
    }
}
