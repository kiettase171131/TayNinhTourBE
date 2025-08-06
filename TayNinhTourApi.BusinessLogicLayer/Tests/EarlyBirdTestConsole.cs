using TayNinhTourApi.BusinessLogicLayer.Tests;
using TayNinhTourApi.BusinessLogicLayer.Services;

namespace TayNinhTourApi.BusinessLogicLayer.Tests
{
    /// <summary>
    /// Console app ?? test Enhanced Early Bird Logic
    /// </summary>
    public class EarlyBirdTestConsole
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("?? Enhanced Early Bird Pricing Logic Test");
            Console.WriteLine("============================================");
            Console.WriteLine();

            try
            {
                var tests = new EarlyBirdPricingTests();
                tests.RunAllTests();

                Console.WriteLine();
                Console.WriteLine("?? Manual Test Cases:");
                Console.WriteLine("--------------------");

                var pricingService = new TourPricingService();

                // Test case 1: Normal Early Bird
                Console.WriteLine("Test 1: Normal Early Bird (14 ngày ??u)");
                var result1 = pricingService.GetPricingInfo(
                    1000000m,
                    new DateTime(2024, 3, 15),  // Tour date (60+ days later)
                    new DateTime(2024, 1, 1),   // Public date
                    new DateTime(2024, 1, 10)   // Booking date (9 days after public)
                );
                Console.WriteLine($"  ? IsEarlyBird: {result1.IsEarlyBird}");
                Console.WriteLine($"  ? Final Price: {result1.FinalPrice:N0} VND");
                Console.WriteLine($"  ? Description: {result1.EarlyBirdDescription}");
                Console.WriteLine();

                // Test case 2: Extended Early Bird
                Console.WriteLine("Test 2: Extended Early Bird (tour b?t ??u s?m)");
                var result2 = pricingService.GetPricingInfo(
                    1000000m,
                    new DateTime(2024, 2, 10),  // Tour date (ch? 40 ngày t? public)
                    new DateTime(2024, 1, 1),   // Public date
                    new DateTime(2024, 1, 25)   // Booking date (24 days after public)
                );
                Console.WriteLine($"  ? IsEarlyBird: {result2.IsEarlyBird}");
                Console.WriteLine($"  ? Final Price: {result2.FinalPrice:N0} VND");
                Console.WriteLine($"  ? Early Bird Window: {result2.EarlyBirdWindowDays} days");
                Console.WriteLine($"  ? Description: {result2.EarlyBirdDescription}");
                Console.WriteLine();

                // Test case 3: Very late public (< 14 days)
                Console.WriteLine("Test 3: Tour ???c công khai mu?n (< 14 ngày ??n slot ??u)");
                var result3 = pricingService.GetPricingInfo(
                    1000000m,
                    new DateTime(2024, 1, 20),  // Tour date
                    new DateTime(2024, 1, 12),  // Public date (ch? 8 ngày tr??c tour)
                    new DateTime(2024, 1, 15)   // Booking date (3 days after public, 35+ days to tour)
                );
                Console.WriteLine($"  ? IsEarlyBird: {result3.IsEarlyBird}");
                Console.WriteLine($"  ? Early Bird Window: {result3.EarlyBirdWindowDays} days");
                Console.WriteLine($"  ? Days from public to tour: {result3.DaysFromPublicToTourStart} days");
                Console.WriteLine($"  ? End Date: {result3.EarlyBirdEndDate:dd/MM/yyyy}");
                Console.WriteLine();

                Console.WriteLine("? All tests completed successfully!");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}