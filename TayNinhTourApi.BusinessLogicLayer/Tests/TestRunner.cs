using System.Reflection;

namespace TayNinhTourApi.BusinessLogicLayer.Tests
{
    /// <summary>
    /// Simple test runner để chạy unit tests cho SchedulingService
    /// Không cần external testing framework
    /// </summary>
    public static class TestRunner
    {
        /// <summary>
        /// Chạy tất cả tests trong SchedulingTests
        /// </summary>
        public static TestResults RunAllTests()
        {
            var results = new TestResults();
            var testClass = new SchedulingTests();
            var testMethods = GetTestMethods(typeof(SchedulingTests));

            Console.WriteLine("=== SCHEDULING SERVICE TESTS ===");
            Console.WriteLine($"Found {testMethods.Count} test methods");
            Console.WriteLine();

            foreach (var method in testMethods)
            {
                var testResult = RunSingleTest(testClass, method);
                results.AddResult(testResult);

                Console.WriteLine($"[{(testResult.Passed ? "PASS" : "FAIL")}] {testResult.TestName}");
                if (!testResult.Passed)
                {
                    Console.WriteLine($"  Error: {testResult.ErrorMessage}");
                }
                Console.WriteLine();
            }

            Console.WriteLine("=== TEST SUMMARY ===");
            Console.WriteLine($"Total Tests: {results.TotalTests}");
            Console.WriteLine($"Passed: {results.PassedTests}");
            Console.WriteLine($"Failed: {results.FailedTests}");
            Console.WriteLine($"Success Rate: {results.SuccessRate:P2}");

            if (results.FailedTests > 0)
            {
                Console.WriteLine("\n=== FAILED TESTS ===");
                foreach (var failedTest in results.FailedTestResults)
                {
                    Console.WriteLine($"- {failedTest.TestName}: {failedTest.ErrorMessage}");
                }
            }

            return results;
        }

        /// <summary>
        /// Chạy một test cụ thể
        /// </summary>
        public static TestResult RunSpecificTest(string testMethodName)
        {
            var testClass = new SchedulingTests();
            var method = typeof(SchedulingTests).GetMethod(testMethodName);

            if (method == null)
            {
                return new TestResult
                {
                    TestName = testMethodName,
                    Passed = false,
                    ErrorMessage = "Test method not found"
                };
            }

            return RunSingleTest(testClass, method);
        }

        private static List<MethodInfo> GetTestMethods(Type testClassType)
        {
            return testClassType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m => m.Name.StartsWith("Test") && m.GetParameters().Length == 0)
                               .ToList();
        }

        private static TestResult RunSingleTest(object testInstance, MethodInfo testMethod)
        {
            var result = new TestResult
            {
                TestName = testMethod.Name
            };

            try
            {
                var startTime = DateTime.UtcNow;
                testMethod.Invoke(testInstance, null);
                var endTime = DateTime.UtcNow;

                result.Passed = true;
                result.ExecutionTime = endTime - startTime;
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
    }

    /// <summary>
    /// Kết quả của một test case
    /// </summary>
    public class TestResult
    {
        public string TestName { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
    }

    /// <summary>
    /// Tổng hợp kết quả của tất cả tests
    /// </summary>
    public class TestResults
    {
        private readonly List<TestResult> _results = new();

        public void AddResult(TestResult result)
        {
            _results.Add(result);
        }

        public int TotalTests => _results.Count;
        public int PassedTests => _results.Count(r => r.Passed);
        public int FailedTests => _results.Count(r => !r.Passed);
        public double SuccessRate => TotalTests > 0 ? (double)PassedTests / TotalTests : 0;

        public List<TestResult> PassedTestResults => _results.Where(r => r.Passed).ToList();
        public List<TestResult> FailedTestResults => _results.Where(r => !r.Passed).ToList();
        public List<TestResult> AllResults => _results.ToList();

        public TimeSpan TotalExecutionTime => TimeSpan.FromTicks(_results.Sum(r => r.ExecutionTime.Ticks));
    }

    /// <summary>
    /// Extension methods để chạy tests từ controller hoặc service khác
    /// </summary>
    public static class TestExtensions
    {
        /// <summary>
        /// Chạy tests và trả về kết quả dưới dạng string
        /// </summary>
        public static string RunTestsAsString()
        {
            var results = TestRunner.RunAllTests();

            var output = new List<string>
            {
                "=== SCHEDULING SERVICE TEST RESULTS ===",
                $"Total Tests: {results.TotalTests}",
                $"Passed: {results.PassedTests}",
                $"Failed: {results.FailedTests}",
                $"Success Rate: {results.SuccessRate:P2}",
                $"Total Execution Time: {results.TotalExecutionTime.TotalMilliseconds:F2}ms",
                ""
            };

            if (results.FailedTests > 0)
            {
                output.Add("FAILED TESTS:");
                foreach (var failedTest in results.FailedTestResults)
                {
                    output.Add($"- {failedTest.TestName}: {failedTest.ErrorMessage}");
                }
                output.Add("");
            }

            output.Add("PASSED TESTS:");
            foreach (var passedTest in results.PassedTestResults)
            {
                output.Add($"✓ {passedTest.TestName} ({passedTest.ExecutionTime.TotalMilliseconds:F2}ms)");
            }

            return string.Join("\n", output);
        }

        /// <summary>
        /// Kiểm tra xem tất cả tests có pass không
        /// </summary>
        public static bool AllTestsPass()
        {
            var results = TestRunner.RunAllTests();
            return results.FailedTests == 0;
        }
    }
}
