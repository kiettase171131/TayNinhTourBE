namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái đơn đăng ký TourGuide
    /// Enhanced version thay thế cho ApplicationStatus cũ
    /// </summary>
    public enum TourGuideApplicationStatus
    {
        /// <summary>
        /// Chờ xử lý - Đơn vừa được nộp
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đã duyệt - User được cấp role "TourGuide"
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Từ chối - Đơn không đạt yêu cầu
        /// </summary>
        Rejected = 2
    }
}
