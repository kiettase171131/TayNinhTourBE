namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service xử lý logic tính giá tour với early bird discount
    /// </summary>
    public interface ITourPricingService
    {
        /// <summary>
        /// Tính giá tour với logic early bird discount
        /// </summary>
        /// <param name="originalPrice">Giá gốc của tour</param>
        /// <param name="tourStartDate">Ngày khởi hành tour</param>
        /// <param name="tourDetailsCreatedAt">Ngày tạo tour details (ngày mở bán)</param>
        /// <param name="bookingDate">Ngày đặt tour</param>
        /// <returns>Tuple chứa giá cuối cùng và phần trăm giảm giá</returns>
        (decimal finalPrice, decimal discountPercent) CalculatePrice(
            decimal originalPrice,
            DateTime tourStartDate,
            DateTime tourDetailsCreatedAt,
            DateTime bookingDate);

        /// <summary>
        /// Kiểm tra xem booking có đủ điều kiện early bird không
        /// </summary>
        /// <param name="tourStartDate">Ngày khởi hành tour</param>
        /// <param name="tourDetailsCreatedAt">Ngày tạo tour details</param>
        /// <param name="bookingDate">Ngày đặt tour</param>
        /// <returns>True nếu đủ điều kiện early bird</returns>
        bool IsEarlyBirdEligible(
            DateTime tourStartDate,
            DateTime tourDetailsCreatedAt,
            DateTime bookingDate);

        /// <summary>
        /// Lấy thông tin chi tiết về pricing
        /// </summary>
        /// <param name="originalPrice">Giá gốc</param>
        /// <param name="tourStartDate">Ngày khởi hành</param>
        /// <param name="tourDetailsCreatedAt">Ngày mở bán</param>
        /// <param name="bookingDate">Ngày đặt</param>
        /// <returns>Thông tin chi tiết pricing</returns>
        TourPricingInfo GetPricingInfo(
            decimal originalPrice,
            DateTime tourStartDate,
            DateTime tourDetailsCreatedAt,
            DateTime bookingDate);
    }

    /// <summary>
    /// Thông tin chi tiết về pricing của tour
    /// </summary>
    public class TourPricingInfo
    {
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool IsEarlyBird { get; set; }
        public string PricingType { get; set; } = string.Empty;
        public int DaysSinceCreated { get; set; }
        public int DaysUntilTour { get; set; }
    }
}
