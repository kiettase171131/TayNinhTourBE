namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Loại hoàn tiền tour booking
    /// Xác định nguyên nhân và quy trình hoàn tiền
    /// </summary>
    public enum TourRefundType
    {
        /// <summary>
        /// Tour company chủ động hủy tour
        /// Khách hàng được hoàn 100% tiền
        /// </summary>
        CompanyCancellation = 0,

        /// <summary>
        /// Hệ thống tự động hủy tour do không đủ khách (< 50% capacity)
        /// Khách hàng được hoàn 100% tiền
        /// </summary>
        AutoCancellation = 1,

        /// <summary>
        /// Khách hàng chủ động hủy booking
        /// Áp dụng chính sách hoàn tiền theo thời gian
        /// </summary>
        UserCancellation = 2
    }
}
