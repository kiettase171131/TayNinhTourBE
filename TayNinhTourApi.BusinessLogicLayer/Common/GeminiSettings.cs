namespace TayNinhTourApi.BusinessLogicLayer.Common
{
    /// <summary>
    /// Configuration settings cho Gemini AI API
    /// </summary>
    public class GeminiSettings
    {
        /// <summary>
        /// API Key ?? truy c?p Gemini API
        /// </summary>
        public string ApiKey { get; set; } = null!;

        /// <summary>
        /// URL c?a Gemini API endpoint
        /// </summary>
        public string ApiUrl { get; set; } = null!;

        /// <summary>
        /// Model name ?? s? d?ng (ví d?: gemini-pro)
        /// </summary>
        public string Model { get; set; } = "gemini-pro";

        /// <summary>
        /// S? token t?i ?a cho response
        /// </summary>
        public int MaxTokens { get; set; } = 4096;

        /// <summary>
        /// Temperature cho AI response (0.0 - 1.0)
        /// </summary>
        public double Temperature { get; set; } = 0.7;

        /// <summary>
        /// System prompt m?c ??nh
        /// </summary>
        public string SystemPrompt { get; set; } = null!;

        /// <summary>
        /// B?t fallback mode khi Gemini không kh? d?ng
        /// </summary>
        public bool EnableFallback { get; set; } = true;

        /// <summary>
        /// Th?i gian ch? t?i ?a tr??c khi fallback (giây)
        /// </summary>
        public int FallbackTimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// Danh sách các model backup ?? th? khi model chính b? overload
        /// </summary>
        public List<AlternativeModel> AlternativeModels { get; set; } = new();
        public bool CircuitBreakerEnabled { get; set; } = true;
        public int CircuitBreakerFailureThreshold { get; set; } = 3;
        public int CircuitBreakerRecoveryTimeMinutes { get; set; } = 10;
        public bool SmartFallback { get; set; } = true;
    }

    /// <summary>
    /// C?u hình cho model backup
    /// </summary>
    public class AlternativeModel
    {
        public string Name { get; set; } = null!;
        public string ApiUrl { get; set; } = null!;
        public int MaxTokens { get; set; } = 256;
        public double Temperature { get; set; } = 0.2;
    }

    /// <summary>
    /// Configuration settings cho OpenAI API (backup)
    /// </summary>
    public class OpenAISettings
    {
        /// <summary>
        /// API Key ?? truy c?p OpenAI API
        /// </summary>
        public string ApiKey { get; set; } = null!;

        /// <summary>
        /// Model name ?? s? d?ng (ví d?: gpt-3.5-turbo)
        /// </summary>
        public string Model { get; set; } = "gpt-3.5-turbo";

        /// <summary>
        /// S? token t?i ?a cho response
        /// </summary>
        public int MaxTokens { get; set; } = 800;

        /// <summary>
        /// Temperature cho AI response (0.0 - 1.0)
        /// </summary>
        public double Temperature { get; set; } = 0.5;

        /// <summary>
        /// B?t OpenAI nh? fallback provider
        /// </summary>
        public bool IsEnabled { get; set; } = false;
    }
<<<<<<< Updated upstream
=======

    public static class QuotaTracker
    {
        private static readonly Dictionary<string, List<DateTime>> _requestHistory = new();
        private static readonly Dictionary<string, DateTime> _lastRequestTime = new();
        private static readonly Dictionary<string, int> _consecutiveFailures = new();
        private static readonly Dictionary<string, DateTime> _circuitBreakerOpenTime = new();
        private static readonly object _lock = new();

        public static bool CanMakeRequest(string apiKey, int maxRequestsPerMinute, int maxRequestsPerDay, int requestDelayMs)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                
                if (_lastRequestTime.ContainsKey(apiKey))
                {
                    var timeSinceLastRequest = (now - _lastRequestTime[apiKey]).TotalMilliseconds;
                    if (timeSinceLastRequest < requestDelayMs)
                    {
                        return false;
                    }
                }

                if (!_requestHistory.ContainsKey(apiKey))
                {
                    _requestHistory[apiKey] = new List<DateTime>();
                }

                var requests = _requestHistory[apiKey];
                var oneMinuteAgo = now.AddMinutes(-1);
                var oneDayAgo = now.AddDays(-1);

                requests.RemoveAll(time => time < oneDayAgo);

                var requestsInLastMinute = requests.Count(time => time >= oneMinuteAgo);
                if (requestsInLastMinute >= maxRequestsPerMinute)
                {
                    return false;
                }

                if (requests.Count >= maxRequestsPerDay)
                {
                    return false;
                }

                return true;
            }
        }

        public static bool IsCircuitBreakerOpen(string apiKey, int failureThreshold, int recoveryTimeMinutes)
        {
            lock (_lock)
            {
                if (!_consecutiveFailures.ContainsKey(apiKey) || _consecutiveFailures[apiKey] < failureThreshold)
                {
                    return false;
                }

                if (_circuitBreakerOpenTime.ContainsKey(apiKey))
                {
                    var openTime = _circuitBreakerOpenTime[apiKey];
                    var recoveryTime = openTime.AddMinutes(recoveryTimeMinutes);
                    
                    if (DateTime.UtcNow >= recoveryTime)
                    {
                        _consecutiveFailures[apiKey] = 0;
                        _circuitBreakerOpenTime.Remove(apiKey);
                        return false;
                    }
                }

                return true;
            }
        }

        public static void RecordFailure(string apiKey, int failureThreshold)
        {
            lock (_lock)
            {
                if (!_consecutiveFailures.ContainsKey(apiKey))
                {
                    _consecutiveFailures[apiKey] = 0;
                }

                _consecutiveFailures[apiKey]++;

                if (_consecutiveFailures[apiKey] >= failureThreshold)
                {
                    _circuitBreakerOpenTime[apiKey] = DateTime.UtcNow;
                }
            }
        }

        public static void RecordSuccess(string apiKey)
        {
            lock (_lock)
            {
                if (_consecutiveFailures.ContainsKey(apiKey))
                {
                    _consecutiveFailures[apiKey] = 0;
                }

                if (_circuitBreakerOpenTime.ContainsKey(apiKey))
                {
                    _circuitBreakerOpenTime.Remove(apiKey);
                }
            }
        }

        public static void RecordRequest(string apiKey)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                
                if (!_requestHistory.ContainsKey(apiKey))
                {
                    _requestHistory[apiKey] = new List<DateTime>();
                }

                _requestHistory[apiKey].Add(now);
                _lastRequestTime[apiKey] = now;
            }
        }

        public static int GetRequestCountToday(string apiKey)
        {
            lock (_lock)
            {
                if (!_requestHistory.ContainsKey(apiKey))
                {
                    return 0;
                }

                var now = DateTime.UtcNow;
                var oneDayAgo = now.AddDays(-1);

                _requestHistory[apiKey].RemoveAll(time => time < oneDayAgo);

                return _requestHistory[apiKey].Count;
            }
        }

        public static int GetRequestCountLastHour(string apiKey)
        {
            lock (_lock)
            {
                if (!_requestHistory.ContainsKey(apiKey))
                {
                    return 0;
                }

                var now = DateTime.UtcNow;
                var oneHourAgo = now.AddHours(-1);

                _requestHistory[apiKey].RemoveAll(time => time < oneHourAgo);

                return _requestHistory[apiKey].Count(time => time >= oneHourAgo);
            }
        }

        public static string GetQuotaStatus(string apiKey, int maxPerDay, int maxPerMinute)
        {
            lock (_lock)
            {
                var todayCount = GetRequestCountToday(apiKey);
                var hourCount = GetRequestCountLastHour(apiKey);
                return $"Today: {todayCount}/{maxPerDay}, Hour: {hourCount}";
            }
        }

        public static void ForceResetQuota(string apiKey)
        {
            lock (_lock)
            {
                if (_requestHistory.ContainsKey(apiKey))
                {
                    _requestHistory[apiKey].Clear();
                }
                if (_lastRequestTime.ContainsKey(apiKey))
                {
                    _lastRequestTime.Remove(apiKey);
                }
                if (_consecutiveFailures.ContainsKey(apiKey))
                {
                    _consecutiveFailures[apiKey] = 0;
                }
                if (_circuitBreakerOpenTime.ContainsKey(apiKey))
                {
                    _circuitBreakerOpenTime.Remove(apiKey);
                }
            }
        }
    }
>>>>>>> Stashed changes
}