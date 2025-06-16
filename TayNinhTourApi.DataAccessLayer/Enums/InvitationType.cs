namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Loại lời mời hướng dẫn viên
    /// </summary>
    public enum InvitationType
    {
        /// <summary>
        /// Lời mời tự động - Hệ thống tự động gửi cho các guide có skills phù hợp
        /// </summary>
        Automatic = 1,

        /// <summary>
        /// Lời mời thủ công - TourCompany chọn và mời guide cụ thể
        /// </summary>
        Manual = 2
    }
}
