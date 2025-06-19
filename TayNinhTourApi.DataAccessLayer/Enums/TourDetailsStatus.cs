namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái duyệt của tour details
    /// </summary>
    public enum TourDetailsStatus
    {
        /// <summary>
        /// Chờ duyệt
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đã được duyệt
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Bị từ chối
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Tạm ngưng
        /// </summary>
        Suspended = 3,

        /// <summary>
        /// Chờ phân công hướng dẫn viên
        /// </summary>
        AwaitingGuideAssignment = 4,

        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// Chờ admin duyệt
        /// </summary>
        AwaitingAdminApproval = 6
    }
}
