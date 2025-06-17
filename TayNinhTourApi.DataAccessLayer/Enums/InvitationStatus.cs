namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái lời mời hướng dẫn viên
    /// </summary>
    public enum InvitationStatus
    {
        /// <summary>
        /// Đang chờ phản hồi - Lời mời đã được gửi nhưng chưa có phản hồi
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Đã chấp nhận - TourGuide đã chấp nhận lời mời
        /// </summary>
        Accepted = 2,

        /// <summary>
        /// Đã từ chối - TourGuide đã từ chối lời mời
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Đã hết hạn - Lời mời đã hết hạn mà không có phản hồi
        /// </summary>
        Expired = 4
    }
}
