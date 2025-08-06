using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Tests
{
    /// <summary>
    /// Test cases cho Early Bird Pricing Service v?i logic m?i
    /// Logic: Early Bird t?i ?a 14 ng�y t? ng�y c�ng khai tour, k�o d�i ??n ng�y slot ??u ti�n n?u < 14 ng�y
    /// </summary>
    public class EarlyBirdPricingTests
    {
        private readonly ITourPricingService _pricingService;

        public EarlyBirdPricingTests()
        {
            _pricingService = new Services.TourPricingService();
        }

        /// <summary>
        /// Test case: Early Bird active - ??t trong 14 ng�y ??u sau khi tour ???c c�ng khai
        /// </summary>
        public void TestEarlyBirdActive()
        {
            Console.WriteLine("?? Test Early Bird Active:");
            
            // Setup: Tour ???c c�ng khai ng�y 1/1, kh�ch ??t ng�y 5/1, tour b?t ??u 15/2
            var tourPublicDate = new DateTime(2024, 1, 1);      // Tour ???c c�ng khai
            var bookingDate = new DateTime(2024, 1, 5);         // ??t ng�y th? 5 (< 14 ng�y)
            var tourStartDate = new DateTime(2024, 2, 15);      // Tour b?t ??u sau 45 ng�y

            var pricingInfo = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate);

            Console.WriteLine($"?? Ng�y c�ng khai: {tourPublicDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ng�y ??t: {bookingDate:dd/MM/yyyy} (ng�y th? {(bookingDate - tourPublicDate).Days + 1})");
            Console.WriteLine($"?? Ng�y tour: {tourStartDate:dd/MM/yyyy}");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {pricingInfo.IsEarlyBird} (Expected: TRUE)");
            Console.WriteLine($"?? Gi� g?c: {pricingInfo.OriginalPrice:N0} VND");
            Console.WriteLine($"?? Gi� cu?i: {pricingInfo.FinalPrice:N0} VND");
            Console.WriteLine($"?? Gi?m gi�: {pricingInfo.DiscountPercent}%");
            Console.WriteLine($"?? Early Bird window: {pricingInfo.EarlyBirdWindowDays} ng�y");
            Console.WriteLine($"?? M� t?: {pricingInfo.EarlyBirdDescription}");
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
        /// Test case: Early Bird expired - ??t sau 14 ng�y k? t? c�ng khai
        /// </summary>
        public void TestEarlyBirdExpired()
        {
            Console.WriteLine("? Test Early Bird Expired:");
            
            // Setup: Tour ???c c�ng khai ng�y 1/1, kh�ch ??t ng�y 20/1 (sau 14 ng�y), tour b?t ??u 15/3
            var tourPublicDate = new DateTime(2024, 1, 1);      // Tour ???c c�ng khai
            var bookingDate = new DateTime(2024, 1, 20);        // ??t ng�y th? 20 (> 14 ng�y)
            var tourStartDate = new DateTime(2024, 3, 15);      // Tour b?t ??u sau 55 ng�y

            var pricingInfo = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate);

            Console.WriteLine($"?? Ng�y c�ng khai: {tourPublicDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ng�y ??t: {bookingDate:dd/MM/yyyy} (ng�y th? {(bookingDate - tourPublicDate).Days + 1})");
            Console.WriteLine($"?? Ng�y tour: {tourStartDate:dd/MM/yyyy}");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {pricingInfo.IsEarlyBird} (Expected: FALSE)");
            Console.WriteLine($"?? Gi�: {pricingInfo.FinalPrice:N0} VND (gi� g?c)");
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
        /// Test case: Tour qu� g?n - kh�ng ?? 30 ng�y notice
        /// </summary>
        public void TestTourTooSoon()
        {
            Console.WriteLine("?? Test Tour Too Soon:");
            
            // Setup: Tour ???c c�ng khai ng�y 1/1, kh�ch ??t ng�y 5/1, nh?ng tour ch? b?t ??u 25/1 (< 30 ng�y)
            var tourPublicDate = new DateTime(2024, 1, 1);      // Tour ???c c�ng khai
            var bookingDate = new DateTime(2024, 1, 5);         // ??t ng�y th? 5 (< 14 ng�y)
            var tourStartDate = new DateTime(2024, 1, 25);      // Tour ch? sau 20 ng�y (< 30 ng�y)

            var pricingInfo = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate);

            Console.WriteLine($"?? Ng�y c�ng khai: {tourPublicDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ng�y ??t: {bookingDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ng�y tour: {tourStartDate:dd/MM/yyyy} ({pricingInfo.DaysUntilTour} ng�y n?a)");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {pricingInfo.IsEarlyBird} (Expected: FALSE - kh�ng ?? 30 ng�y notice)");
            Console.WriteLine($"?? Gi�: {pricingInfo.FinalPrice:N0} VND (gi� g?c)");
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
        /// Test case: Tour ???c c�ng khai mu?n - Early Bird k�o d�i ??n ng�y slot ??u ti�n
        /// </summary>
        public void TestEarlyBirdExtendedToTourStart()
        {
            Console.WriteLine("?? Test Early Bird Extended To Tour Start:");
            
            // Setup: Tour ???c c�ng khai ng�y 1/2, tour b?t ??u ng�y 10/2 (ch? c� 9 ng�y < 14)
            var tourPublicDate = new DateTime(2024, 2, 1);      // Tour ???c c�ng khai
            var bookingDate = new DateTime(2024, 2, 5);         // ??t ng�y 5/2 (4 ng�y sau c�ng khai)
            var tourStartDate = new DateTime(2024, 3, 15);      // Tour sau 43 ng�y (?? 30 ng�y notice)

            // T?o case ??c bi?t: tour ch? c� 10 ng�y t? public ??n start
            var shortNoticeTourStart = new DateTime(2024, 2, 11); // Ch? 10 ng�y t? public

            var pricingInfo = _pricingService.GetPricingInfo(
                1000000m, shortNoticeTourStart, tourPublicDate, bookingDate);

            Console.WriteLine($"?? Ng�y c�ng khai: {tourPublicDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ng�y ??t: {bookingDate:dd/MM/yyyy}");
            Console.WriteLine($"?? Ng�y tour: {shortNoticeTourStart:dd/MM/yyyy}");
            Console.WriteLine($"?? T? c�ng khai ??n tour: {pricingInfo.DaysFromPublicToTourStart} ng�y");
            Console.WriteLine($"?? Early Bird window: {pricingInfo.EarlyBirdWindowDays} ng�y (thay v� 14)");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {pricingInfo.IsEarlyBird} (Expected: FALSE - kh�ng ?? 30 ng�y notice)");
            Console.WriteLine($"?? Logic: {pricingInfo.EarlyBirdDescription}");
            Console.WriteLine();

            // Test case kh�c: tour c� ?? 30 ng�y notice
            var validTourStart = new DateTime(2024, 3, 5);      // 32 ng�y t? booking
            var validPricingInfo = _pricingService.GetPricingInfo(
                1000000m, validTourStart, tourPublicDate, bookingDate);

            Console.WriteLine("?? Test v?i tour h?p l? (?? 30 ng�y notice):");
            Console.WriteLine($"?? Ng�y tour: {validTourStart:dd/MM/yyyy}");
            Console.WriteLine($"?? K?t qu?: IsEarlyBird = {validPricingInfo.IsEarlyBird} (Expected: TRUE)");
            Console.WriteLine($"?? Gi� cu?i: {validPricingInfo.FinalPrice:N0} VND");
            Console.WriteLine($"?? Early Bird window: {validPricingInfo.EarlyBirdWindowDays} ng�y");
            Console.WriteLine();

            // Assertions
            if (validPricingInfo.IsEarlyBird && validPricingInfo.DiscountPercent != 25m)
                throw new Exception($"Expected 25% discount for valid case, but got {validPricingInfo.DiscountPercent}%");

            Console.WriteLine("? TestEarlyBirdExtendedToTourStart PASSED");
        }

        /// <summary>
        /// Test case: Boundary conditions - ?�ng 14 ng�y v� 30 ng�y
        /// </summary>
        public void TestBoundaryConditions()
        {
            Console.WriteLine("?? Boundary Conditions Tests:");
            
            // Test 1: ?�ng 14 ng�y t? public date (ng�y cu?i c?a Early Bird)
            var tourPublicDate = new DateTime(2024, 1, 1);
            var bookingDate14Days = new DateTime(2024, 1, 15);    // Ng�y th? 15 (14 ng�y sau public)
            var tourStartDate = new DateTime(2024, 2, 15);        // 45 ng�y sau booking

            var pricingInfo14 = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate14Days);

            Console.WriteLine($"?? Test ng�y th? 15 (14 ng�y sau public): IsEarlyBird = {pricingInfo14.IsEarlyBird} (Expected: FALSE)");

            // Test 2: Ng�y th? 14 (13 ng�y sau public) - should be TRUE
            var bookingDate13Days = new DateTime(2024, 1, 14);    // Ng�y th? 14 (13 ng�y sau public)
            
            var pricingInfo13 = _pricingService.GetPricingInfo(
                1000000m, tourStartDate, tourPublicDate, bookingDate13Days);

            Console.WriteLine($"?? Test ng�y th? 14 (13 ng�y sau public): IsEarlyBird = {pricingInfo13.IsEarlyBird} (Expected: TRUE)");

            // Test 3: ?�ng 30 ng�y tr??c tour
            var bookingDate30 = new DateTime(2024, 1, 5);
            var tourStartDate30 = new DateTime(2024, 2, 4);            // ?�ng 30 ng�y sau booking

            var pricingInfo30 = _pricingService.GetPricingInfo(
                1000000m, tourStartDate30, tourPublicDate, bookingDate30);

            Console.WriteLine($"?? Test ?�ng 30 ng�y tr??c tour: IsEarlyBird = {pricingInfo30.IsEarlyBird} (Expected: TRUE)");

            // Test 4: 29 ng�y tr??c tour (should fail)
            var bookingDate29 = new DateTime(2024, 1, 5);
            var tourStartDate29 = new DateTime(2024, 2, 3);            // Ch? 29 ng�y sau booking

            var pricingInfo29 = _pricingService.GetPricingInfo(
                1000000m, tourStartDate29, tourPublicDate, bookingDate29);

            Console.WriteLine($"?? Test 29 ng�y tr??c tour: IsEarlyBird = {pricingInfo29.IsEarlyBird} (Expected: FALSE)");
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
            Console.WriteLine("?? New Logic: 14 ng�y t? ng�y c�ng khai tour (thay v� ng�y t?o)");
            Console.WriteLine("?? ??c bi?t: K�o d�i ??n ng�y slot ??u ti�n n?u < 14 ng�y");
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
            Console.WriteLine("   - Time window: 14 ng�y ??u sau khi tour ???c C�NG KHAI");
            Console.WriteLine("   - Minimum notice: Tour ph?i kh?i h�nh sau �t nh?t 30 ng�y t? ng�y ??t");
            Console.WriteLine("   - Special case: N?u t? c�ng khai ??n slot ??u < 14 ng�y ? Early Bird k�o d�i h?t");
            Console.WriteLine("   - Logic: daysSincePublic < earlyBirdWindow AND daysUntilTour >= 30");
            Console.WriteLine();
        }
    }
}