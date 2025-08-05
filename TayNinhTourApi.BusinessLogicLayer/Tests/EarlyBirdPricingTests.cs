using TayNinhTourApi.BusinessLogicLayer.Services;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Tests
{
    /// <summary>
    /// Test cases for Early Bird pricing logic
    /// </summary>
    public class EarlyBirdPricingTests
    {
        private readonly ITourPricingService _pricingService;

        public EarlyBirdPricingTests()
        {
            _pricingService = new TourPricingService();
        }

        /// <summary>
        /// Test case: Early Bird active - booking trong 15 ngày ??u, tour sau 30 ngày
        /// </summary>
        public void TestEarlyBirdActive()
        {
            // Arrange
            var originalPrice = 1000000m;
            var tourCreatedDate = new DateTime(2024, 1, 1);
            var bookingDate = new DateTime(2024, 1, 10); // 9 ngày sau khi t?o
            var tourStartDate = new DateTime(2024, 2, 15); // 45 ngày sau booking

            // Act
            var pricingInfo = _pricingService.GetPricingInfo(
                originalPrice, tourStartDate, tourCreatedDate, bookingDate);

            // Assert
            var expected = new
            {
                IsEarlyBird = true,
                DiscountPercent = 25m,
                FinalPrice = 750000m,
                DiscountAmount = 250000m,
                PricingType = "Early Bird"
            };

            Console.WriteLine($"? Early Bird Active Test:");
            Console.WriteLine($"   Original Price: {pricingInfo.OriginalPrice:N0} VND");
            Console.WriteLine($"   Final Price: {pricingInfo.FinalPrice:N0} VND");
            Console.WriteLine($"   Discount: {pricingInfo.DiscountPercent}%");
            Console.WriteLine($"   Is Early Bird: {pricingInfo.IsEarlyBird}");
            Console.WriteLine($"   Type: {pricingInfo.PricingType}");
            Console.WriteLine($"   Days Since Created: {pricingInfo.DaysSinceCreated}");
            Console.WriteLine($"   Days Until Tour: {pricingInfo.DaysUntilTour}");
            Console.WriteLine();

            // Verify results
            var success = pricingInfo.IsEarlyBird == expected.IsEarlyBird &&
                         pricingInfo.DiscountPercent == expected.DiscountPercent &&
                         pricingInfo.FinalPrice == expected.FinalPrice &&
                         pricingInfo.DiscountAmount == expected.DiscountAmount &&
                         pricingInfo.PricingType == expected.PricingType;

            Console.WriteLine(success ? "? PASSED" : "? FAILED");
        }

        /// <summary>
        /// Test case: Early Bird expired - booking sau 15 ngày
        /// </summary>
        public void TestEarlyBirdExpired()
        {
            // Arrange
            var originalPrice = 1000000m;
            var tourCreatedDate = new DateTime(2024, 1, 1);
            var bookingDate = new DateTime(2024, 1, 20); // 19 ngày sau khi t?o (> 15 ngày)
            var tourStartDate = new DateTime(2024, 3, 1); // 45 ngày sau booking

            // Act
            var pricingInfo = _pricingService.GetPricingInfo(
                originalPrice, tourStartDate, tourCreatedDate, bookingDate);

            // Assert
            var expected = new
            {
                IsEarlyBird = false,
                DiscountPercent = 0m,
                FinalPrice = 1000000m,
                DiscountAmount = 0m,
                PricingType = "Standard"
            };

            Console.WriteLine($"? Early Bird Expired Test:");
            Console.WriteLine($"   Original Price: {pricingInfo.OriginalPrice:N0} VND");
            Console.WriteLine($"   Final Price: {pricingInfo.FinalPrice:N0} VND");
            Console.WriteLine($"   Discount: {pricingInfo.DiscountPercent}%");
            Console.WriteLine($"   Is Early Bird: {pricingInfo.IsEarlyBird}");
            Console.WriteLine($"   Type: {pricingInfo.PricingType}");
            Console.WriteLine($"   Days Since Created: {pricingInfo.DaysSinceCreated}");
            Console.WriteLine($"   Days Until Tour: {pricingInfo.DaysUntilTour}");
            Console.WriteLine();

            var success = pricingInfo.IsEarlyBird == expected.IsEarlyBird &&
                         pricingInfo.DiscountPercent == expected.DiscountPercent &&
                         pricingInfo.FinalPrice == expected.FinalPrice &&
                         pricingInfo.DiscountAmount == expected.DiscountAmount &&
                         pricingInfo.PricingType == expected.PricingType;

            Console.WriteLine(success ? "? PASSED" : "? FAILED");
        }

        /// <summary>
        /// Test case: Tour too soon - không ?? 30 ngày
        /// </summary>
        public void TestTourTooSoon()
        {
            // Arrange
            var originalPrice = 1000000m;
            var tourCreatedDate = new DateTime(2024, 1, 1);
            var bookingDate = new DateTime(2024, 1, 5); // 4 ngày sau khi t?o
            var tourStartDate = new DateTime(2024, 1, 25); // Ch? 20 ngày sau booking (< 30 ngày)

            // Act
            var pricingInfo = _pricingService.GetPricingInfo(
                originalPrice, tourStartDate, tourCreatedDate, bookingDate);

            // Assert
            var expected = new
            {
                IsEarlyBird = false,
                DiscountPercent = 0m,
                FinalPrice = 1000000m,
                PricingType = "Standard"
            };

            Console.WriteLine($"? Tour Too Soon Test:");
            Console.WriteLine($"   Original Price: {pricingInfo.OriginalPrice:N0} VND");
            Console.WriteLine($"   Final Price: {pricingInfo.FinalPrice:N0} VND");
            Console.WriteLine($"   Discount: {pricingInfo.DiscountPercent}%");
            Console.WriteLine($"   Is Early Bird: {pricingInfo.IsEarlyBird}");
            Console.WriteLine($"   Type: {pricingInfo.PricingType}");
            Console.WriteLine($"   Days Since Created: {pricingInfo.DaysSinceCreated}");
            Console.WriteLine($"   Days Until Tour: {pricingInfo.DaysUntilTour}");
            Console.WriteLine();

            var success = pricingInfo.IsEarlyBird == expected.IsEarlyBird &&
                         pricingInfo.DiscountPercent == expected.DiscountPercent &&
                         pricingInfo.FinalPrice == expected.FinalPrice &&
                         pricingInfo.PricingType == expected.PricingType;

            Console.WriteLine(success ? "? PASSED" : "? FAILED");
        }

        /// <summary>
        /// Test case: Boundary - exactly 15 days and 30 days
        /// </summary>
        public void TestBoundaryConditions()
        {
            Console.WriteLine("?? Boundary Conditions Tests:");
            
            // Test 1: Exactly 15 days since creation
            var originalPrice = 1000000m;
            var tourCreatedDate = new DateTime(2024, 1, 1);
            var bookingDate = new DateTime(2024, 1, 16); // Exactly 15 days
            var tourStartDate = new DateTime(2024, 2, 15); // 30+ days later

            var pricingInfo1 = _pricingService.GetPricingInfo(
                originalPrice, tourStartDate, tourCreatedDate, bookingDate);

            Console.WriteLine($"?? Day 15 boundary: IsEarlyBird = {pricingInfo1.IsEarlyBird} (should be FALSE)");

            // Test 2: Exactly 30 days until tour
            bookingDate = new DateTime(2024, 1, 5); // Within 15 days
            tourStartDate = new DateTime(2024, 2, 4); // Exactly 30 days later

            var pricingInfo2 = _pricingService.GetPricingInfo(
                originalPrice, tourStartDate, tourCreatedDate, bookingDate);

            Console.WriteLine($"?? Day 30 boundary: IsEarlyBird = {pricingInfo2.IsEarlyBird} (should be TRUE)");

            // Test 3: 29 days until tour (should fail)
            tourStartDate = new DateTime(2024, 2, 3); // Only 29 days later

            var pricingInfo3 = _pricingService.GetPricingInfo(
                originalPrice, tourStartDate, tourCreatedDate, bookingDate);

            Console.WriteLine($"?? Day 29 boundary: IsEarlyBird = {pricingInfo3.IsEarlyBird} (should be FALSE)");
            Console.WriteLine();
        }

        /// <summary>
        /// Run all early bird tests
        /// </summary>
        public void RunAllTests()
        {
            Console.WriteLine("?? Running Early Bird Pricing Tests...");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();

            TestEarlyBirdActive();
            TestEarlyBirdExpired();
            TestTourTooSoon();
            TestBoundaryConditions();

            Console.WriteLine(new string('=', 60));
            Console.WriteLine("? All Early Bird tests completed!");
            Console.WriteLine();
            Console.WriteLine("?? Summary of Early Bird Rules:");
            Console.WriteLine("   - Discount: 25% off original price");
            Console.WriteLine("   - Time window: First 15 days after tour creation");
            Console.WriteLine("   - Minimum notice: Tour must start 30+ days after booking");
            Console.WriteLine("   - Logic: daysSinceCreated <= 15 AND daysUntilTour >= 30");
        }
    }
}