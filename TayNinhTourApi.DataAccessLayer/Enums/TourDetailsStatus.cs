namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái duyệt của TourDetails
    /// </summary>
    public enum TourDetailsStatus
    {
        /// <summary>
        /// Chờ duyệt - Trạng thái mặc định khi tạo mới
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đã duyệt - Admin đã phê duyệt tour details
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Từ chối - Admin đã từ chối tour details
        /// </summary>
        Rejected = 2
    }
}
