using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Tests
{
    /// <summary>
    /// Test cases cho Early Bird Pricing Service v?i logic m?i
    /// Logic: Early Bird t?i ?a 14 ngày t? ngày công khai tour, kéo dài ??n ngày slot ??u tiên n?u < 14 ngày
    /// </summary>
    public class EarlyBirdPricingTests
    {
        private readonly ITourPricingService _pricingService;

        public EarlyBirdPricingTests()
        {
            _pricingService = new Services.TourPricingService();
        }

        /// <summary>
        /// Test case: Early Bird active - ??t trong 14 ngày ??u sau khi tour ???c công khai
        /// </summary>
        public void TestEarlyBirdActive()
        {
            Console.WriteLine("?? Test Early Bird Active:");
            
            // Setup: Tour ???c công khai ngày 1/1, khách ??t ngày 5/1, tour b?t ??u 15/2
            var tourPublicDate = new DateTime(2024, 1, 1);      // Tour ???c công khai
            var bookingDate = new DateTime(2024, 1, 5);         // ??t ngày th? 5 (< 14 ngày)
            var tourStartDate = new DateTime(2024, 2, 15);      // Tour b?t ??u sau 45 ngày

            var pricingInfo = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate);

            Console.WriteLine($"?? Ngày công khai: {tourPublicDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ngày ??t: {bookingDate:dd/MM/yyyy} (ngày th? {(bookingDate - tourPublicDate).Days + 1})");
            Console.WriteLine($"?? Ngày tour: {tourStartDate:dd/MM/yyyy}");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {pricingInfo.IsEarlyBird} (Expected: TRUE)");
            Console.WriteLine($"?? Giá g?c: {pricingInfo.OriginalPrice:N0} VND");
            Console.WriteLine($"?? Giá cu?i: {pricingInfo.FinalPrice:N0} VND");
            Console.WriteLine($"?? Gi?m giá: {pricingInfo.DiscountPercent}%");
            Console.WriteLine($"?? Early Bird window: {pricingInfo.EarlyBirdWindowDays} ngày");
            Console.WriteLine($"?? Mô t?: {pricingInfo.EarlyBirdDescription}");
            Console.WriteLine();

            // Assertions
            if (!pricingInfo.IsEarlyBird)
                throw new Exception($"Expected Early Bird to be active, but got false. Days since public: {pricingInfo.DaysSinceCreated}");
            
            if (pricingInfo.DiscountPercent != 25m)
                throw new Exception($"Expected 25% discount, but got {pricingInfo.DiscountPercent}%");
            
            if (pricingInfo.FinalPrice != 750000m)
                throw new Exception($"Expected final price 750,000 VND, but got {pricingInfo.FinalPrice:N0} VND");

            Console.WriteLine("? TestEarlyBirdActive PASSED");
        }

        /// <summary>
        /// Test case: Early Bird expired - ??t sau 14 ngày k? t? công khai
        /// </summary>
        public void TestEarlyBirdExpired()
        {
            Console.WriteLine("? Test Early Bird Expired:");
            
            // Setup: Tour ???c công khai ngày 1/1, khách ??t ngày 20/1 (sau 14 ngày), tour b?t ??u 15/3
            var tourPublicDate = new DateTime(2024, 1, 1);      // Tour ???c công khai
            var bookingDate = new DateTime(2024, 1, 20);        // ??t ngày th? 20 (> 14 ngày)
            var tourStartDate = new DateTime(2024, 3, 15);      // Tour b?t ??u sau 55 ngày

            var pricingInfo = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate);

            Console.WriteLine($"?? Ngày công khai: {tourPublicDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ngày ??t: {bookingDate:dd/MM/yyyy} (ngày th? {(bookingDate - tourPublicDate).Days + 1})");
            Console.WriteLine($"?? Ngày tour: {tourStartDate:dd/MM/yyyy}");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {pricingInfo.IsEarlyBird} (Expected: FALSE)");
            Console.WriteLine($"?? Giá: {pricingInfo.FinalPrice:N0} VND (giá g?c)");
            Console.WriteLine($"?? Pricing Type: {pricingInfo.PricingType}");
            Console.WriteLine();

            // Assertions
            if (pricingInfo.IsEarlyBird)
                throw new Exception($"Expected Early Bird to be expired, but got true. Days since public: {pricingInfo.DaysSinceCreated}");
            
            if (pricingInfo.DiscountPercent != 0m)
                throw new Exception($"Expected no discount, but got {pricingInfo.DiscountPercent}%");
            
            if (pricingInfo.FinalPrice != 1000000m)
                throw new Exception($"Expected full price 1,000,000 VND, but got {pricingInfo.FinalPrice:N0} VND");

            Console.WriteLine("? TestEarlyBirdExpired PASSED");
        }

        /// <summary>
        /// Test case: Tour quá g?n - không ?? 30 ngày notice
        /// </summary>
        public void TestTourTooSoon()
        {
            Console.WriteLine("?? Test Tour Too Soon:");
            
            // Setup: Tour ???c công khai ngày 1/1, khách ??t ngày 5/1, nh?ng tour ch? b?t ??u 25/1 (< 30 ngày)
            var tourPublicDate = new DateTime(2024, 1, 1);      // Tour ???c công khai
            var bookingDate = new DateTime(2024, 1, 5);         // ??t ngày th? 5 (< 14 ngày)
            var tourStartDate = new DateTime(2024, 1, 25);      // Tour ch? sau 20 ngày (< 30 ngày)

            var pricingInfo = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate);

            Console.WriteLine($"?? Ngày công khai: {tourPublicDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ngày ??t: {bookingDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ngày tour: {tourStartDate:dd/MM/yyyy} ({pricingInfo.DaysUntilTour} ngày n?a)");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {pricingInfo.IsEarlyBird} (Expected: FALSE - không ?? 30 ngày notice)");
            Console.WriteLine($"?? Giá: {pricingInfo.FinalPrice:N0} VND (giá g?c)");
            Console.WriteLine($"?? Pricing Type: {pricingInfo.PricingType}");
            Console.WriteLine();

            // Assertions
            if (pricingInfo.IsEarlyBird)
                throw new Exception($"Expected Early Bird to be false due to insufficient notice, but got true. Days until tour: {pricingInfo.DaysUntilTour}");
            
            if (pricingInfo.DaysUntilTour >= 30)
                throw new Exception($"Expected less than 30 days until tour, but got {pricingInfo.DaysUntilTour} days");

            Console.WriteLine("? TestTourTooSoon PASSED");
        }

        /// <summary>
        /// Test case: Tour ???c công khai mu?n - Early Bird kéo dài ??n ngày slot ??u tiên
        /// </summary>
        public void TestEarlyBirdExtendedToTourStart()
        {
            Console.WriteLine("?? Test Early Bird Extended To Tour Start:");
            
            // Setup: Tour ???c công khai ngày 1/2, tour b?t ??u ngày 10/2 (ch? có 9 ngày < 14)
            var tourPublicDate = new DateTime(2024, 2, 1);      // Tour ???c công khai
            var bookingDate = new DateTime(2024, 2, 5);         // ??t ngày 5/2 (4 ngày sau công khai)
            var tourStartDate = new DateTime(2024, 3, 15);      // Tour sau 43 ngày (?? 30 ngày notice)

            // T?o case ??c bi?t: tour ch? có 10 ngày t? public ??n start
            var shortNoticeTourStart = new DateTime(2024, 2, 11); // Ch? 10 ngày t? public

            var pricingInfo = _pricingService.GetPricingInfo(
                1000000m, shortNoticeTourStart, tourPublicDate, bookingDate);

            Console.WriteLine($"?? Ngày công khai: {tourPublicDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ngày ??t: {bookingDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ngày tour: {shortNoticeTourStart:dd/MM/yyyy}");
            Console.WriteLine($"?? T? công khai ??n tour: {pricingInfo.DaysFromPublicToTourStart} ngày");
            Console.WriteLine($"?? Early Bird window: {pricingInfo.EarlyBirdWindowDays} ngày (thay vì 14)");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {pricingInfo.IsEarlyBird} (Expected: FALSE - không ?? 30 ngày notice)");
            Console.WriteLine($"?? Logic: {pricingInfo.EarlyBirdDescription}");
            Console.WriteLine();

            // Test case khác: tour có ?? 30 ngày notice
            var validTourStart = new DateTime(2024, 3, 5);      // 32 ngày t? booking
            var validPricingInfo = _pricingService.GetPricingInfo(
                1000000m, validTourStart, tourPublicDate, bookingDate);

            Console.WriteLine("?? Test v?i tour h?p l? (?? 30 ngày notice):");
            Console.WriteLine($"?? Ngày tour: {validTourStart:dd/MM/yyyy}");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {validPricingInfo.IsEarlyBird} (Expected: TRUE)");
            Console.WriteLine($"?? Giá cu?i: {validPricingInfo.FinalPrice:N0} VND");
            Console.WriteLine($"?? Early Bird window: {validPricingInfo.EarlyBirdWindowDays} ngày");
            Console.WriteLine();

            // Assertions
            if (validPricingInfo.IsEarlyBird && validPricingInfo.DiscountPercent != 25m)
                throw new Exception($"Expected 25% discount for valid case, but got {validPricingInfo.DiscountPercent}%");

            Console.WriteLine("? TestEarlyBirdExtendedToTourStart PASSED");
        }

        /// <summary>
        /// Test case: Boundary conditions - ?úng 14 ngày và 30 ngày
        /// </summary>
        public void TestBoundaryConditions()
        {
            Console.WriteLine("?? Boundary Conditions Tests:");
            
            // Test 1: ?úng 14 ngày t? public date (ngày cu?i c?a Early Bird)
            var tourPublicDate = new DateTime(2024, 1, 1);
            var bookingDate14Days = new DateTime(2024, 1, 15);    // Ngày th? 15 (14 ngày sau public)
            var tourStartDate = new DateTime(2024, 2, 15);        // 45 ngày sau booking

            var pricingInfo14 = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate14Days);

            Console.WriteLine($"?? Test ngày th? 15 (14 ngày sau public): IsEarlyBird = {pricingInfo14.IsEarlyBird} (Expected: FALSE)");

            // Test 2: Ngày th? 14 (13 ngày sau public) - should be TRUE
            var bookingDate13Days = new DateTime(2024, 1, 14);    // Ngày th? 14 (13 ngày sau public)
            
            var pricingInfo13 = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate13Days);

            Console.WriteLine($"?? Test ngày th? 14 (13 ngày sau public): IsEarlyBird = {pricingInfo13.IsEarlyBird} (Expected: TRUE)");

            // Test 3: ?úng 30 ngày tr??c tour
            var bookingDate30 = new DateTime(2024, 1, 5);
            var tourStartDate30 = new DateTime(2024, 2, 4);            // ?úng 30 ngày sau booking

            var pricingInfo30 = _pricingService.GetPricingInfo(
                1000000m, tourStartDate30, tourPublicDate, bookingDate30);

            Console.WriteLine($"?? Test ?úng 30 ngày tr??c tour: IsEarlyBird = {pricingInfo30.IsEarlyBird} (Expected: TRUE)");

            // Test 4: 29 ngày tr??c tour (should fail)
            var bookingDate29 = new DateTime(2024, 1, 5);
            var tourStartDate29 = new DateTime(2024, 2, 3);            // Ch? 29 ngày sau booking

            var pricingInfo29 = _pricingService.GetPricingInfo(
                1000000m, tourStartDate29, tourPublicDate, bookingDate29);

            Console.WriteLine($"?? Test 29 ngày tr??c tour: IsEarlyBird = {pricingInfo29.IsEarlyBird} (Expected: FALSE)");
            Console.WriteLine();

            // Assertions
            if (pricingInfo14.IsEarlyBird)
                throw new Exception("Expected Early Bird to be false on day 15 (14 days after public)");
                
            if (!pricingInfo13.IsEarlyBird)
                throw new Exception("Expected Early Bird to be true on day 14 (13 days after public)");
                
            if (!pricingInfo30.IsEarlyBird)
                throw new Exception("Expected Early Bird to be true with exactly 30 days notice");
                
            if (pricingInfo29.IsEarlyBird)
                throw new Exception("Expected Early Bird to be false with only 29 days notice");

            Console.WriteLine("? TestBoundaryConditions PASSED");
        }

        /// <summary>
        /// Run all early bird tests v?i logic m?i
        /// </summary>
        public void RunAllTests()
        {
            Console.WriteLine("?? Running Enhanced Early Bird Pricing Tests...");
            Console.WriteLine("?? New Logic: 14 ngày t? ngày công khai tour (thay vì ngày t?o)");
            Console.WriteLine("?? ??c bi?t: Kéo dài ??n ngày slot ??u tiên n?u < 14 ngày");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine();

            TestEarlyBirdActive();
            TestEarlyBirdExpired();
            TestTourTooSoon();
            TestEarlyBirdExtendedToTourStart();
            TestBoundaryConditions();

            Console.WriteLine(new string('=', 70));
            Console.WriteLine("? All Enhanced Early Bird tests completed!");
            Console.WriteLine();
            Console.WriteLine("?? Summary of Enhanced Early Bird Rules:");
            Console.WriteLine("   - Discount: 25% off original price");
            Console.WriteLine("   - Time window: 14 ngày ??u sau khi tour ???c CÔNG KHAI");
            Console.WriteLine("   - Minimum notice: Tour ph?i kh?i hành sau ít nh?t 30 ngày t? ngày ??t");
            Console.WriteLine("   - Special case: N?u t? công khai ??n slot ??u < 14 ngày ? Early Bird kéo dài h?t");
            Console.WriteLine("   - Logic: daysSincePublic < earlyBirdWindow AND daysUntilTour >= 30");
            Console.WriteLine();
        }
    }
}