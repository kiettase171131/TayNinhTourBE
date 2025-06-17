namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Trạng thái đơn đăng ký Specialty Shop
    /// Thay thế cho ShopStatus với naming rõ ràng hơn
    /// </summary>
    public enum SpecialtyShopApplicationStatus
    {
        /// <summary>
        /// Chờ xử lý - Đơn vừa được nộp
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đã duyệt - User được cấp role "Specialty Shop"
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Từ chối - Đơn không đạt yêu cầu
        /// </summary>
        Rejected = 2
    }
}
