namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái duyệt của TourDetails với workflow phân công hướng dẫn viên
    /// </summary>
    public enum TourDetailsStatus
    {
        /// <summary>
        /// Chờ duyệt - Trạng thái mặc định khi tạo mới, chưa có hướng dẫn viên
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đã duyệt - Admin đã phê duyệt tour details
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Từ chối - Admin đã từ chối tour details
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Chờ phân công hướng dẫn viên - Đang trong giai đoạn tìm và mời hướng dẫn viên
        /// </summary>
        AwaitingGuideAssignment = 3,

        /// <summary>
        /// Chờ admin duyệt - Đã có hướng dẫn viên được phân công, chờ admin phê duyệt
        /// </summary>
        AwaitingAdminApproval = 4,

        /// <summary>
        /// Đã hủy - Không tìm được hướng dẫn viên trong thời gian quy định (5 ngày)
        /// </summary>
        Cancelled = 5
    }
}
