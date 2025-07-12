namespace TayNinhTourApi.BusinessLogicLayer.Common
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; } = "";
        public string ApiUrl { get; set; } = "";
        public string Model { get; set; } = "gemini-1.5-flash";
        public int MaxTokens { get; set; } = 1024;
        public double Temperature { get; set; } = 0.7;
        public string SystemPrompt { get; set; } = "";
        public bool EnableFallback { get; set; } = true;
        public int FallbackTimeoutSeconds { get; set; } = 15;
        public int MaxRetries { get; set; } = 1;
        public int BaseDelayMs { get; set; } = 5000;
        public bool UseCache { get; set; } = true;
        public int CacheExpirationMinutes { get; set; } = 120;
        public int RateLimitPerMinute { get; set; } = 5;
        public int RateLimitPerDay { get; set; } = 40;
        public bool EnableQuotaTracking { get; set; } = true;
        public int RequestDelayMs { get; set; } = 12000;
        public List<AlternativeModel> AlternativeModels { get; set; } = new();
    }

    public class AlternativeModel
    {
        public string Name { get; set; } = "";
        public string ApiUrl { get; set; } = "";
        public int MaxTokens { get; set; } = 1024;
        public double Temperature { get; set; } = 0.5;
    }

    public class OpenAISettings
    {
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gpt-3.5-turbo";
        public int MaxTokens { get; set; } = 800;
        public double Temperature { get; set; } = 0.7;
        public bool IsEnabled { get; set; } = false;
    }

    public static class QuotaTracker
    {
        private static readonly Dictionary<string, List<DateTime>> _requestHistory = new();
        private static readonly Dictionary<string, DateTime> _lastRequestTime = new();
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

                return _requestHistory[apiKey].Count(time => time >= oneHourAgo);
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

        public static TimeSpan GetTimeUntilNextRequest(string apiKey, int requestDelayMs)
        {
            lock (_lock)
            {
                if (!_lastRequestTime.ContainsKey(apiKey))
                {
                    return TimeSpan.Zero;
                }

                var now = DateTime.UtcNow;
                var timeSinceLastRequest = now - _lastRequestTime[apiKey];
                var requiredDelay = TimeSpan.FromMilliseconds(requestDelayMs);

                if (timeSinceLastRequest >= requiredDelay)
                {
                    return TimeSpan.Zero;
                }

                return requiredDelay - timeSinceLastRequest;
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
            }
        }

        public static string GetQuotaStatus(string apiKey, int maxPerDay, int maxPerMinute)
        {
            lock (_lock)
            {
                var todayCount = GetRequestCountToday(apiKey);
                var hourCount = GetRequestCountLastHour(apiKey);
                var nextRequestIn = GetTimeUntilNextRequest(apiKey, 12000);
                
                return $"Today: {todayCount}/{maxPerDay}, Last hour: {hourCount}, Next: {nextRequestIn.TotalSeconds:F0}s";
            }
        }
    }
}