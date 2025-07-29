using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service xử lý logic tính giá tour với early bird discount
    /// </summary>
    public class TourPricingService : ITourPricingService
    {
        // Constants cho pricing logic
        private const int EARLY_BIRD_WINDOW_DAYS = 15; // 15 ngày đầu sau khi mở bán
        private const int MINIMUM_DAYS_BEFORE_TOUR = 30; // Tour phải khởi hành sau ít nhất 30 ngày
        private const decimal EARLY_BIRD_DISCOUNT_PERCENT = 25m; // Giảm 25%

        /// <summary>
        /// Tính giá tour với logic early bird discount
        /// </summary>
        public (decimal finalPrice, decimal discountPercent) CalculatePrice(
            decimal originalPrice,
            DateTime tourStartDate,
            DateTime tourDetailsCreatedAt,
            DateTime bookingDate)
        {
            if (originalPrice <= 0)
                throw new ArgumentException("Giá gốc phải lớn hơn 0", nameof(originalPrice));

            if (bookingDate < tourDetailsCreatedAt)
                throw new ArgumentException("Ngày đặt không thể trước ngày mở bán", nameof(bookingDate));

            if (tourStartDate <= bookingDate)
                throw new ArgumentException("Ngày khởi hành phải sau ngày đặt", nameof(tourStartDate));

            // Kiểm tra điều kiện Early Bird
            if (IsEarlyBirdEligible(tourStartDate, tourDetailsCreatedAt, bookingDate))
            {
                var discountAmount = originalPrice * (EARLY_BIRD_DISCOUNT_PERCENT / 100m);
                var finalPrice = originalPrice - discountAmount;
                return (finalPrice, EARLY_BIRD_DISCOUNT_PERCENT);
            }

            // Last Minute: Không giảm giá
            return (originalPrice, 0m);
        }

        /// <summary>
        /// Kiểm tra xem booking có đủ điều kiện early bird không
        /// </summary>
        public bool IsEarlyBirdEligible(
            DateTime tourStartDate,
            DateTime tourDetailsCreatedAt,
            DateTime bookingDate)
        {
            var daysSinceCreated = (bookingDate.Date - tourDetailsCreatedAt.Date).Days;
            var daysUntilTour = (tourStartDate.Date - bookingDate.Date).Days;

            // Điều kiện Early Bird:
            // 1. Đặt trong 15 ngày đầu sau khi mở bán
            // 2. Tour khởi hành sau ít nhất 30 ngày kể từ ngày đặt
            return daysSinceCreated <= EARLY_BIRD_WINDOW_DAYS && daysUntilTour >= MINIMUM_DAYS_BEFORE_TOUR;
        }

        /// <summary>
        /// Lấy thông tin chi tiết về pricing
        /// </summary>
        public TourPricingInfo GetPricingInfo(
            decimal originalPrice,
            DateTime tourStartDate,
            DateTime tourDetailsCreatedAt,
            DateTime bookingDate)
        {
            var daysSinceCreated = (bookingDate.Date - tourDetailsCreatedAt.Date).Days;
            var daysUntilTour = (tourStartDate.Date - bookingDate.Date).Days;
            var isEarlyBird = IsEarlyBirdEligible(tourStartDate, tourDetailsCreatedAt, bookingDate);

            var (finalPrice, discountPercent) = CalculatePrice(originalPrice, tourStartDate, tourDetailsCreatedAt, bookingDate);
            var discountAmount = originalPrice - finalPrice;

            return new TourPricingInfo
            {
                OriginalPrice = originalPrice,
                FinalPrice = finalPrice,
                DiscountPercent = discountPercent,
                DiscountAmount = discountAmount,
                IsEarlyBird = isEarlyBird,
                PricingType = isEarlyBird ? "Early Bird" : "Last Minute",
                DaysSinceCreated = daysSinceCreated,
                DaysUntilTour = daysUntilTour
            };
        }
    }
}
