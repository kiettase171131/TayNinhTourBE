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

            Console.WriteLine("=== TEST RUNNER ===");
            Console.WriteLine("No test classes found. SchedulingTests has been removed.");
            Console.WriteLine();

            Console.WriteLine("=== TEST SUMMARY ===");
            Console.WriteLine($"Total Tests: {results.TotalTests}");
            Console.WriteLine($"Passed: {results.PassedTests}");
            Console.WriteLine($"Failed: {results.FailedTests}");
            Console.WriteLine($"Success Rate: {results.SuccessRate:P2}");

            return results;
        }

        /// <summary>
        /// Chạy một test cụ thể
        /// </summary>
        public static TestResult RunSpecificTest(string testMethodName)
        {
            return new TestResult
            {
                TestName = testMethodName,
                Passed = false,
                ErrorMessage = "No test classes available. SchedulingTests has been removed."
            };
        }

        private static TestResult RunSingleTest(object testInstance, MethodInfo testMethod)
        {
            var result = new TestResult
            {
                TestName = testMethod.Name
            };

            var startTime = DateTime.UtcNow;

            try
            {
                testMethod.Invoke(testInstance, null);
                result.Passed = true;
                result.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            }
            finally
            {
                result.ExecutionTime = DateTime.UtcNow - startTime;
            }

            return result;
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
