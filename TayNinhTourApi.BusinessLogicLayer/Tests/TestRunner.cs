using System.Reflection;

namespace TayNinhTourApi.BusinessLogicLayer.Tests
{
    /// <summary>
    /// Simple test runner để chạy unit tests
    /// Không cần external testing framework
    /// </summary>
    public static class TestRunner
    {
        /// <summary>
        /// Chạy tất cả tests có sẵn
        /// </summary>
        public static TestResults RunAllTests()
        {
            var results = new TestResults();

            Console.WriteLine("🚀 === TAYNINHTOURAI TEST RUNNER ===");
            Console.WriteLine("Running all available tests...");
            Console.WriteLine();

            // Run Early Bird Pricing Tests
            RunEarlyBirdPricingTests(results);

            Console.WriteLine();
            Console.WriteLine("=== TEST SUMMARY ===");
            Console.WriteLine($"Total Tests: {results.TotalTests}");
            Console.WriteLine($"Passed: {results.PassedTests} ✅");
            Console.WriteLine($"Failed: {results.FailedTests} ❌");
            Console.WriteLine($"Success Rate: {results.SuccessRate:P2}");
            Console.WriteLine($"Total Execution Time: {results.TotalExecutionTime.TotalMilliseconds:F2}ms");

            if (results.FailedTests > 0)
            {
                Console.WriteLine();
                Console.WriteLine("❌ FAILED TESTS:");
                foreach (var failedTest in results.FailedTestResults)
                {
                    Console.WriteLine($"   • {failedTest.TestName}: {failedTest.ErrorMessage}");
                }
            }

            Console.WriteLine();
            Console.WriteLine(results.FailedTests == 0 
                ? "🎉 All tests passed successfully!" 
                : $"⚠️  {results.FailedTests} test(s) failed. Please review.");

            return results;
        }

        /// <summary>
        /// Chạy Early Bird Pricing Tests
        /// </summary>
        private static void RunEarlyBirdPricingTests(TestResults results)
        {
            Console.WriteLine("🎯 Running Early Bird Pricing Tests...");
            Console.WriteLine("─────────────────────────────────────");

            try
            {
                var earlyBirdTests = new EarlyBirdPricingTests();

                // Test 1: Early Bird Active
                RunTestMethod(results, "EarlyBirdActive", () => earlyBirdTests.TestEarlyBirdActive());

                // Test 2: Early Bird Expired
                RunTestMethod(results, "EarlyBirdExpired", () => earlyBirdTests.TestEarlyBirdExpired());

                // Test 3: Tour Too Soon
                RunTestMethod(results, "TourTooSoon", () => earlyBirdTests.TestTourTooSoon());

                // Test 4: Boundary Conditions
                RunTestMethod(results, "BoundaryConditions", () => earlyBirdTests.TestBoundaryConditions());

                Console.WriteLine("📊 Early Bird Pricing Tests completed.");
            }
            catch (Exception ex)
            {
                results.AddResult(new TestResult
                {
                    TestName = "EarlyBirdPricingTests_Initialization",
                    Passed = false,
                    ErrorMessage = $"Failed to initialize Early Bird tests: {ex.Message}",
                    ExecutionTime = TimeSpan.Zero
                });
                Console.WriteLine($"❌ Failed to run Early Bird tests: {ex.Message}");
            }
        }

        /// <summary>
        /// Chạy một test method cụ thể và track kết quả
        /// </summary>
        private static void RunTestMethod(TestResults results, string testName, Action testMethod)
        {
            var result = new TestResult
            {
                TestName = testName
            };

            var startTime = DateTime.UtcNow;

            try
            {
                Console.Write($"  Running {testName}... ");
                testMethod.Invoke();
                result.Passed = true;
                result.ErrorMessage = null;
                Console.WriteLine("✅ PASSED");
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.ErrorMessage = ex.Message;
                Console.WriteLine($"❌ FAILED - {ex.Message}");
            }
            finally
            {
                result.ExecutionTime = DateTime.UtcNow - startTime;
                results.AddResult(result);
            }
        }

        /// <summary>
        /// Chạy một test cụ thể theo tên
        /// </summary>
        public static TestResult RunSpecificTest(string testMethodName)
        {
            Console.WriteLine($"🎯 Running specific test: {testMethodName}");
            Console.WriteLine("─────────────────────────────────────");

            try
            {
                var earlyBirdTests = new EarlyBirdPricingTests();

                return testMethodName.ToLower() switch
                {
                    "earlybirdactive" or "testearlyBirdactive" => 
                        RunSingleTestMethod("EarlyBirdActive", () => earlyBirdTests.TestEarlyBirdActive()),
                    
                    "earlyBirdexpired" or "testearlyBirdexpired" => 
                        RunSingleTestMethod("EarlyBirdExpired", () => earlyBirdTests.TestEarlyBirdExpired()),
                    
                    "tourtoosoon" or "testtourtoosoon" => 
                        RunSingleTestMethod("TourTooSoon", () => earlyBirdTests.TestTourTooSoon()),
                    
                    "boundaryconditions" or "testboundaryconditions" => 
                        RunSingleTestMethod("BoundaryConditions", () => earlyBirdTests.TestBoundaryConditions()),
                    
                    "all" or "runalltests" => 
                        RunAllEarlyBirdTests(),
                    
                    _ => new TestResult
                    {
                        TestName = testMethodName,
                        Passed = false,
                        ErrorMessage = $"Test method '{testMethodName}' not found. Available tests: EarlyBirdActive, EarlyBirdExpired, TourTooSoon, BoundaryConditions, All"
                    }
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    TestName = testMethodName,
                    Passed = false,
                    ErrorMessage = $"Error running test: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Chạy tất cả Early Bird tests và trả về kết quả đầu tiên fail (nếu có)
        /// </summary>
        private static TestResult RunAllEarlyBirdTests()
        {
            var results = RunAllTests();
            
            if (results.FailedTests > 0)
            {
                return results.FailedTestResults.First();
            }
            
            return new TestResult
            {
                TestName = "AllEarlyBirdTests",
                Passed = true,
                ErrorMessage = null,
                ExecutionTime = results.TotalExecutionTime
            };
        }

        /// <summary>
        /// Chạy một test method đơn lẻ
        /// </summary>
        private static TestResult RunSingleTestMethod(string testName, Action testMethod)
        {
            var result = new TestResult
            {
                TestName = testName
            };

            var startTime = DateTime.UtcNow;

            try
            {
                Console.WriteLine($"▶️  Executing {testName}...");
                testMethod.Invoke();
                result.Passed = true;
                result.ErrorMessage = null;
                Console.WriteLine($"✅ {testName} PASSED");
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.ErrorMessage = ex.Message;
                Console.WriteLine($"❌ {testName} FAILED - {ex.Message}");
            }
            finally
            {
                result.ExecutionTime = DateTime.UtcNow - startTime;
                Console.WriteLine($"⏱️  Execution time: {result.ExecutionTime.TotalMilliseconds:F2}ms");
            }

            return result;
        }

        /// <summary>
        /// Hiển thị danh sách tests có sẵn
        /// </summary>
        public static void ShowAvailableTests()
        {
            Console.WriteLine("📋 Available Tests:");
            Console.WriteLine("──────────────────");
            Console.WriteLine("Early Bird Pricing Tests:");
            Console.WriteLine("  • EarlyBirdActive - Test early bird discount when conditions are met");
            Console.WriteLine("  • EarlyBirdExpired - Test no discount when booking after 15 days");
            Console.WriteLine("  • TourTooSoon - Test no discount when tour starts within 30 days");
            Console.WriteLine("  • BoundaryConditions - Test edge cases at 15 and 30 day boundaries");
            Console.WriteLine("  • All - Run all Early Bird tests");
            Console.WriteLine();
            Console.WriteLine("Usage examples:");
            Console.WriteLine("  TestRunner.RunSpecificTest(\"EarlyBirdActive\")");
            Console.WriteLine("  TestRunner.RunSpecificTest(\"All\")");
            Console.WriteLine("  TestRunner.RunAllTests()");
        }

        /// <summary>
        /// Chạy performance test cho Early Bird pricing
        /// </summary>
        public static void RunPerformanceTest(int iterations = 1000)
        {
            Console.WriteLine($"🚀 Running Early Bird Performance Test ({iterations} iterations)...");
            Console.WriteLine("─────────────────────────────────────────────────────────────");

            var earlyBirdTests = new EarlyBirdPricingTests();
            var startTime = DateTime.UtcNow;
            var successCount = 0;
            var failCount = 0;

            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    earlyBirdTests.TestEarlyBirdActive();
                    earlyBirdTests.TestEarlyBirdExpired();
                    earlyBirdTests.TestTourTooSoon();
                    successCount++;
                }
                catch
                {
                    failCount++;
                }
            }

            var totalTime = DateTime.UtcNow - startTime;
            var avgTimePerIteration = totalTime.TotalMilliseconds / iterations;

            Console.WriteLine($"✅ Performance Test Results:");
            Console.WriteLine($"   Total iterations: {iterations}");
            Console.WriteLine($"   Successful: {successCount}");
            Console.WriteLine($"   Failed: {failCount}");
            Console.WriteLine($"   Success rate: {(double)successCount / iterations:P2}");
            Console.WriteLine($"   Total time: {totalTime.TotalMilliseconds:F2}ms");
            Console.WriteLine($"   Average time per iteration: {avgTimePerIteration:F4}ms");
            Console.WriteLine($"   Operations per second: {iterations / totalTime.TotalSeconds:F0}");
        }
    }

    /// <summary>
    /// Kết quả của một test case đơn lẻ
    /// </summary>
    public class TestResult
    {
        public string TestName { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    /// <summary>
    /// Kết quả tổng hợp của tất cả test cases
    /// </summary>
    public class TestResults
    {
        public List<TestResult> AllResults { get; set; } = new List<TestResult>();
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public TimeSpan TotalExecutionTime { get; set; }

        public int TotalTests => AllResults.Count;
        public int PassedTests => AllResults.Count(r => r.Passed);
        public int FailedTests => AllResults.Count(r => !r.Passed);
        public double SuccessRate => TotalTests > 0 ? (double)PassedTests / TotalTests : 0;

        public IEnumerable<TestResult> PassedTestResults => AllResults.Where(r => r.Passed);
        public IEnumerable<TestResult> FailedTestResults => AllResults.Where(r => !r.Passed);

        public void AddResult(TestResult result)
        {
            AllResults.Add(result);
            TotalExecutionTime = DateTime.UtcNow - StartTime;
        }
    }
}
