using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service xử lý logic tính giá tour với early bird discount
    /// Logic mới: Early Bird tính từ ngày công khai tour, kéo dài tối đa 14 ngày hoặc đến ngày slot đầu tiên
    /// </summary>
    public class TourPricingService : ITourPricingService
    {
        // Constants cho pricing logic - Updated
        private const int EARLY_BIRD_WINDOW_DAYS = 14; // 14 ngày (thay vì 15 ngày)
        private const int MINIMUM_DAYS_BEFORE_TOUR = 30; // Tour phải khởi hành sau ít nhất 30 ngày (giữ nguyên)
        private const decimal EARLY_BIRD_DISCOUNT_PERCENT = 25m; // Giảm 25%

        /// <summary>
        /// Tính giá tour với logic early bird discount mới
        /// </summary>
        public (decimal finalPrice, decimal discountPercent) CalculatePrice(
            decimal originalPrice,
            DateTime tourStartDate,
            DateTime tourPublicDate, // Đổi từ tourDetailsCreatedAt thành tourPublicDate
            DateTime bookingDate)
        {
            if (originalPrice <= 0)
                throw new ArgumentException("Giá gốc phải lớn hơn 0", nameof(originalPrice));

            if (tourStartDate <= bookingDate)
                throw new ArgumentException("Ngày khởi hành phải sau ngày đặt", nameof(tourStartDate));

            // Kiểm tra điều kiện Early Bird với logic mới
            if (IsEarlyBirdEligible(tourStartDate, tourPublicDate, bookingDate))
            {
                var discountAmount = originalPrice * (EARLY_BIRD_DISCOUNT_PERCENT / 100m);
                var finalPrice = originalPrice - discountAmount;
                return (finalPrice, EARLY_BIRD_DISCOUNT_PERCENT);
            }

            // Standard pricing: Không giảm giá
            return (originalPrice, 0m);
        }

        /// <summary>
        /// Kiểm tra xem booking có đủ điều kiện early bird không
        /// Logic mới: 
        /// - Tính từ ngày tour được công khai (không phải ngày tạo)
        /// - Early Bird kéo dài tối đa 14 ngày HOẶC đến ngày slot đầu tiên (nếu < 14 ngày)
        /// - Vẫn giữ điều kiện tour phải khởi hành sau ít nhất 30 ngày từ ngày đặt
        /// </summary>
        public bool IsEarlyBirdEligible(
            DateTime tourStartDate,
            DateTime tourPublicDate, // Ngày tour được công khai
            DateTime bookingDate)
        {
            // Tính số ngày từ khi tour được công khai đến ngày đặt
            var daysSincePublic = Math.Max(0, (bookingDate.Date - tourPublicDate.Date).Days);
            var daysUntilTour = (tourStartDate.Date - bookingDate.Date).Days;

            // Tính số ngày từ khi công khai đến ngày slot đầu tiên
            var daysFromPublicToTourStart = Math.Max(0, (tourStartDate.Date - tourPublicDate.Date).Days);

            // Điều kiện Early Bird mới:
            // 1. Tour phải khởi hành sau ít nhất 30 ngày kể từ ngày đặt (giữ nguyên)
            // 2a. Nếu từ ngày công khai đến slot đầu tiên >= 14 ngày: Early Bird trong 14 ngày đầu
            // 2b. Nếu từ ngày công khai đến slot đầu tiên < 14 ngày: Early Bird đến tận ngày slot
            
            if (daysUntilTour < MINIMUM_DAYS_BEFORE_TOUR)
            {
                return false; // Không đủ 30 ngày notice
            }

            // Xác định Early Bird window
            int earlyBirdWindow;
            if (daysFromPublicToTourStart >= EARLY_BIRD_WINDOW_DAYS)
            {
                // Trường hợp bình thường: có đủ 14 ngày, Early Bird trong 14 ngày đầu
                earlyBirdWindow = EARLY_BIRD_WINDOW_DAYS;
            }
            else
            {
                // Trường hợp đặc biệt: không đủ 14 ngày, Early Bird kéo dài đến ngày slot
                earlyBirdWindow = daysFromPublicToTourStart;
            }

            return daysSincePublic < earlyBirdWindow;
        }

        /// <summary>
        /// Lấy thông tin chi tiết về pricing với logic mới
        /// </summary>
        public TourPricingInfo GetPricingInfo(
            decimal originalPrice,
            DateTime tourStartDate,
            DateTime tourPublicDate, // Đổi từ tourDetailsCreatedAt
            DateTime bookingDate)
        {
            var daysSincePublic = Math.Max(0, (bookingDate.Date - tourPublicDate.Date).Days);
            var daysUntilTour = (tourStartDate.Date - bookingDate.Date).Days;
            var daysFromPublicToTourStart = Math.Max(0, (tourStartDate.Date - tourPublicDate.Date).Days);
            
            var isEarlyBird = IsEarlyBirdEligible(tourStartDate, tourPublicDate, bookingDate);

            var (finalPrice, discountPercent) = CalculatePrice(originalPrice, tourStartDate, tourPublicDate, bookingDate);
            var discountAmount = originalPrice - finalPrice;

            // Tính Early Bird window thực tế
            int earlyBirdWindow = daysFromPublicToTourStart >= EARLY_BIRD_WINDOW_DAYS 
                ? EARLY_BIRD_WINDOW_DAYS 
                : daysFromPublicToTourStart;

            return new TourPricingInfo
            {
                OriginalPrice = originalPrice,
                FinalPrice = finalPrice,
                DiscountPercent = discountPercent,
                DiscountAmount = discountAmount,
                IsEarlyBird = isEarlyBird,
                PricingType = isEarlyBird ? "Early Bird" : "Standard",
                DaysSinceCreated = daysSincePublic, // Đổi tên nhưng giữ field để backward compatibility
                DaysUntilTour = daysUntilTour,
                
                // Thêm thông tin mới để debug
                DaysFromPublicToTourStart = daysFromPublicToTourStart,
                EarlyBirdWindowDays = earlyBirdWindow,
                EarlyBirdEndDate = tourPublicDate.AddDays(earlyBirdWindow)
            };
        }

        /// <summary>
        /// Tính Early Bird end date dựa trên logic mới
        /// </summary>
        public DateTime CalculateEarlyBirdEndDate(DateTime tourPublicDate, DateTime tourStartDate)
        {
            var daysFromPublicToTourStart = Math.Max(0, (tourStartDate.Date - tourPublicDate.Date).Days);
            
            if (daysFromPublicToTourStart >= EARLY_BIRD_WINDOW_DAYS)
            {
                // Đủ 14 ngày: Early Bird kết thúc sau 14 ngày
                return tourPublicDate.AddDays(EARLY_BIRD_WINDOW_DAYS);
            }
            else
            {
                // Không đủ 14 ngày: Early Bird kéo dài đến ngày slot đầu tiên
                return tourStartDate;
            }
        }

        /// <summary>
        /// Tính số ngày còn lại của Early Bird
        /// </summary>
        public int CalculateDaysRemainingForEarlyBird(DateTime tourPublicDate, DateTime tourStartDate, DateTime currentDate)
        {
            var earlyBirdEndDate = CalculateEarlyBirdEndDate(tourPublicDate, tourStartDate);
            var daysRemaining = Math.Max(0, (earlyBirdEndDate.Date - currentDate.Date).Days);
            
            return daysRemaining;
        }
    }
}
