namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái phản hồi của SpecialtyShop đối với invitation
    /// </summary>
    public enum ShopInvitationStatus
    {
        /// <summary>
        /// Chờ phản hồi
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đã chấp nhận tham gia
        /// </summary>
        Accepted = 1,

        /// <summary>
        /// Đã từ chối tham gia
        /// </summary>
        Declined = 2,

        /// <summary>
        /// Hết hạn (không phản hồi)
        /// </summary>
        Expired = 3,

        /// <summary>
        /// Đã hủy bởi Tour Company
        /// </summary>
        Cancelled = 4
    }
}
