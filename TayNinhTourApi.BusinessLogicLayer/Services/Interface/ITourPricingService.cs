namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service xử lý logic tính giá tour với early bird discount
    /// Cập nhật: Early Bird tính từ ngày công khai tour thay vì ngày tạo
    /// </summary>
    public interface ITourPricingService
    {
        /// <summary>
        /// Tính giá tour với logic early bird discount mới
        /// </summary>
        /// <param name="originalPrice">Giá gốc của tour</param>
        /// <param name="tourStartDate">Ngày khởi hành tour (slot đầu tiên)</param>
        /// <param name="tourPublicDate">Ngày tour được công khai (thay vì ngày tạo)</param>
        /// <param name="bookingDate">Ngày đặt tour</param>
        /// <returns>Tuple chứa giá cuối cùng và phần trăm giảm giá</returns>
        (decimal finalPrice, decimal discountPercent) CalculatePrice(
            decimal originalPrice,
            DateTime tourStartDate,
            DateTime tourPublicDate,
            DateTime bookingDate);

        /// <summary>
        /// Kiểm tra xem booking có đủ điều kiện early bird không
        /// Logic mới: Early Bird tối đa 14 ngày từ ngày công khai, hoặc đến ngày slot đầu tiên nếu < 14 ngày
        /// </summary>
        /// <param name="tourStartDate">Ngày khởi hành tour</param>
        /// <param name="tourPublicDate">Ngày tour được công khai</param>
        /// <param name="bookingDate">Ngày đặt tour</param>
        /// <returns>True nếu đủ điều kiện early bird</returns>
        bool IsEarlyBirdEligible(
            DateTime tourStartDate,
            DateTime tourPublicDate,
            DateTime bookingDate);

        /// <summary>
        /// Lấy thông tin chi tiết về pricing
        /// </summary>
        /// <param name="originalPrice">Giá gốc</param>
        /// <param name="tourStartDate">Ngày khởi hành</param>
        /// <param name="tourPublicDate">Ngày công khai tour</param>
        /// <param name="bookingDate">Ngày đặt</param>
        /// <returns>Thông tin chi tiết pricing</returns>
        TourPricingInfo GetPricingInfo(
            decimal originalPrice,
            DateTime tourStartDate,
            DateTime tourPublicDate,
            DateTime bookingDate);

        /// <summary>
        /// Tính ngày kết thúc Early Bird
        /// </summary>
        /// <param name="tourPublicDate">Ngày tour được công khai</param>
        /// <param name="tourStartDate">Ngày khởi hành tour</param>
        /// <returns>Ngày kết thúc Early Bird</returns>
        DateTime CalculateEarlyBirdEndDate(DateTime tourPublicDate, DateTime tourStartDate);

        /// <summary>
        /// Tính số ngày còn lại của Early Bird
        /// </summary>
        /// <param name="tourPublicDate">Ngày tour được công khai</param>
        /// <param name="tourStartDate">Ngày khởi hành tour</param>
        /// <param name="currentDate">Ngày hiện tại</param>
        /// <returns>Số ngày còn lại của Early Bird</returns>
        int CalculateDaysRemainingForEarlyBird(DateTime tourPublicDate, DateTime tourStartDate, DateTime currentDate);
    }

    /// <summary>
    /// Thông tin chi tiết về pricing của tour - cập nhật với Early Bird logic mới
    /// </summary>
    public class TourPricingInfo
    {
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool IsEarlyBird { get; set; }
        public string PricingType { get; set; } = string.Empty;
        
        /// <summary>
        /// Số ngày từ khi công khai tour đến ngày booking
        /// (Trước đây là DaysSinceCreated - giữ tên để backward compatibility)
        /// </summary>
        public int DaysSinceCreated { get; set; }
        
        public int DaysUntilTour { get; set; }

        #region New Fields for Enhanced Early Bird Logic

        /// <summary>
        /// Số ngày từ khi công khai tour đến ngày slot đầu tiên
        /// </summary>
        public int DaysFromPublicToTourStart { get; set; }

        /// <summary>
        /// Số ngày Early Bird window thực tế (14 hoặc ít hơn nếu tour bắt đầu sớm)
        /// </summary>
        public int EarlyBirdWindowDays { get; set; }

        /// <summary>
        /// Ngày kết thúc Early Bird
        /// </summary>
        public DateTime EarlyBirdEndDate { get; set; }

        /// <summary>
        /// Mô tả logic Early Bird áp dụng
        /// </summary>
        public string EarlyBirdDescription => IsEarlyBird
            ? DaysFromPublicToTourStart >= 14
                ? $"Early Bird trong 14 ngày đầu sau công khai (kết thúc {EarlyBirdEndDate:dd/MM/yyyy})"
                : $"Early Bird kéo dài đến ngày tour vì chỉ còn {DaysFromPublicToTourStart} ngày (kết thúc {EarlyBirdEndDate:dd/MM/yyyy})"
            : "Không áp dụng Early Bird";

        /// <summary>
        /// Early Bird có sắp hết hạn không (còn <= 3 ngày)
        /// </summary>
        public bool IsEarlyBirdExpiringSoon => IsEarlyBird && 
            (EarlyBirdEndDate.Date - DateTime.UtcNow.Date).Days <= 3 &&
            (EarlyBirdEndDate.Date - DateTime.UtcNow.Date).Days > 0;

        #endregion
    }
}
