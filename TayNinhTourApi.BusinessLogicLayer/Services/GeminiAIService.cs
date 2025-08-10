using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service ?? tích h?p v?i Gemini AI API v?i rate limiting và caching
    /// </summary>
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _geminiSettings;
        private readonly ILogger<GeminiAIService> _logger;
        private readonly IAITourDataService _tourDataService;
        private readonly IMemoryCache _cache;

        public GeminiAIService(
            HttpClient httpClient,
            IOptions<GeminiSettings> geminiSettings,
            ILogger<GeminiAIService> logger,
            IAITourDataService tourDataService,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _geminiSettings = geminiSettings.Value;
            _logger = logger;
            _tourDataService = tourDataService;
            _cache = cache;
        }

        public async Task<GeminiResponse> GenerateContentAsync(string prompt, List<GeminiMessage>? conversationHistory = null)
        {
            var stopwatch = Stopwatch.StartNew();

            // T?o cache key t? prompt và conversation history
            var cacheKey = GenerateCacheKey(prompt, conversationHistory);
            
            // Ki?m tra cache tr??c
            if (_geminiSettings.UseCache && _cache.TryGetValue(cacheKey, out GeminiResponse? cachedResponse))
            {
                stopwatch.Stop();
                _logger.LogInformation("Using cached response for prompt: {Prompt}", 
                    prompt.Substring(0, Math.Min(50, prompt.Length)));
                
                cachedResponse!.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                return cachedResponse;
            }

            // Log quota status tr??c khi g?i API
            if (_geminiSettings.EnableQuotaTracking)
            {
                var quotaToday = QuotaTracker.GetRequestCountToday(_geminiSettings.ApiKey);
                _logger.LogInformation("Current quota today: {QuotaToday}", quotaToday);

                if (!QuotaTracker.CanMakeRequest(_geminiSettings.ApiKey, 
                    _geminiSettings.RateLimitPerMinute, 
                    _geminiSettings.RateLimitPerDay,
                    _geminiSettings.RequestDelayMs))
                {
                    _logger.LogWarning("Rate limit exceeded.");
                    
                    var waitTime = QuotaTracker.GetTimeUntilNextRequest(_geminiSettings.ApiKey, _geminiSettings.RequestDelayMs);
                    if (waitTime > TimeSpan.Zero && waitTime < TimeSpan.FromMinutes(2))
                    {
                        _logger.LogInformation("Waiting {Seconds}s before next request", waitTime.TotalSeconds);
                        await Task.Delay(waitTime);
                    }
                    else
                    {
                        _logger.LogWarning("Wait time too long ({Seconds}s), using fallback immediately", waitTime.TotalSeconds);
                        return await CreateFallbackResponseAsync(prompt, stopwatch, "Rate limit exceeded - using fallback");
                    }
                }
            }

            // Enrich prompt v?i thông tin tour n?u có t? khóa liên quan
            var enrichedPrompt = await EnrichPromptWithTourDataAsync(prompt);

            // Ch? th? 1 l?n ?? ti?t ki?m quota
            for (int attempt = 1; attempt <= _geminiSettings.MaxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Gemini API attempt {Attempt}/{MaxRetries}", attempt, _geminiSettings.MaxRetries);

                    // Ghi nhãn request ?? tracking quota
                    if (_geminiSettings.EnableQuotaTracking)
                    {
                        QuotaTracker.RecordRequest(_geminiSettings.ApiKey);
                    }

                    // T?o request payload v?i c?u hình t?i ?u
                    var requestPayload = CreateRequestPayload(enrichedPrompt, conversationHistory);
                    var jsonPayload = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    });
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // T?o URL v?i API key
                    var url = $"{_geminiSettings.ApiUrl}?key={_geminiSettings.ApiKey}";

                    if (attempt == 1)
                    {
                        _logger.LogInformation("Sending request to Gemini API. Model: {Model}, Quota today: {QuotaToday}", 
                            _geminiSettings.Model, 
                            _geminiSettings.EnableQuotaTracking ? QuotaTracker.GetRequestCountToday(_geminiSettings.ApiKey) : 0);
                    }

                    // Timeout ?? nhanh fail-over
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_geminiSettings.FallbackTimeoutSeconds));

                    // G?i request
                    var response = await _httpClient.PostAsync(url, content, cts.Token);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Attempt {Attempt}: Gemini API response status: {Status}", attempt, response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
                        if (string.IsNullOrWhiteSpace(responseContent))
                        {
                            _logger.LogWarning("Attempt {Attempt}: Gemini API returned empty response body", attempt);
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "Empty response from API");
                        }

                        try
                        {
                            using var jsonDoc = JsonDocument.Parse(responseContent);
                            
                            if (jsonDoc.RootElement.TryGetProperty("candidates", out var candidatesElement) &&
                                candidatesElement.ValueKind == JsonValueKind.Array &&
                                candidatesElement.GetArrayLength() > 0)
                            {
                                var firstCandidate = candidatesElement[0];
                                if (firstCandidate.TryGetProperty("content", out var contentElement) &&
                                    contentElement.TryGetProperty("parts", out var partsElement) &&
                                    partsElement.ValueKind == JsonValueKind.Array &&
                                    partsElement.GetArrayLength() > 0)
                                {
                                    var firstPart = partsElement[0];
                                    if (firstPart.TryGetProperty("text", out var textElement))
                                    {
                                        var generatedText = textElement.GetString();
                                        if (!string.IsNullOrWhiteSpace(generatedText))
                                        {
                                            var tokensUsed = EstimateTokens(prompt + generatedText);

                                            stopwatch.Stop();
                                            _logger.LogInformation("? Gemini API SUCCESS! Text length: {Length}, Tokens: {Tokens}, Time: {Time}ms",
                                                generatedText.Length, tokensUsed, stopwatch.ElapsedMilliseconds);

                                            var successResponse = new GeminiResponse
                                            {
                                                Success = true,
                                                Content = generatedText,
                                                TokensUsed = tokensUsed,
                                                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                                            };

                                            // Cache response
                                            if (_geminiSettings.UseCache)
                                            {
                                                var cacheExpiry = TimeSpan.FromMinutes(_geminiSettings.CacheExpirationMinutes);
                                                _cache.Set(cacheKey, successResponse, cacheExpiry);
                                                _logger.LogInformation("Response cached for {Minutes} minutes", _geminiSettings.CacheExpirationMinutes);
                                            }

                                            return successResponse;
                                        }
                                    }
                                }
                            }

                            _logger.LogWarning("Attempt {Attempt}: Gemini API returned response but no valid content found", attempt);
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "No valid content in API response");
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger.LogError(jsonEx, "Attempt {Attempt}: JSON deserialization error", attempt);
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "JSON parsing error");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("? Attempt {Attempt}: Gemini API failed. Status: {Status}, Response: {Response}",
                            attempt, response.StatusCode, responseContent.Substring(0, Math.Min(500, responseContent.Length)));

                        // N?u là 429 (rate limit), không retry - dùng fallback ngay
                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            _logger.LogError("?? QUOTA EXCEEDED! Using fallback response.");
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "Quota exceeded - API returned 429");
                        }

                        // N?u là 503 (overload), fail nhanh và dùng fallback
                        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            _logger.LogWarning("Model overloaded (503), using fallback immediately");
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "Gemini API is overloaded");
                        }

                        // Không retry cho các l?i khác ?? ti?t ki?m quota
                        return await CreateFallbackResponseAsync(prompt, stopwatch, $"API request failed: {response.StatusCode}");
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Attempt {Attempt}: Request timeout, using fallback", attempt);
                    return await CreateFallbackResponseAsync(prompt, stopwatch, "Request timeout");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Attempt {Attempt}: Exception calling Gemini API", attempt);
                    return await CreateFallbackResponseAsync(prompt, stopwatch, ex.Message);
                }
            }

            // Fallback cu?i cùng
            return await CreateFallbackResponseAsync(prompt, stopwatch, $"Gemini API failed after {_geminiSettings.MaxRetries} attempts");
        }

        public async Task<string> GenerateTitleAsync(string firstMessage)
        {
            try
            {
                var titlePrompt = $"Tạo tiêu đề ngắn cho: {firstMessage}";

                var response = await GenerateContentAsync(titlePrompt);

                if (response.Success && !string.IsNullOrWhiteSpace(response.Content))
                {
                    var title = response.Content.Trim().Replace("\"", "");
                    if (title.Length > 50)
                    {
                        title = title.Substring(0, 47) + "...";
                    }
                    return title;
                }
                else
                {
                    var fallbackTitle = firstMessage.Length > 30
                        ? firstMessage.Substring(0, 27) + "..."
                        : firstMessage;
                    return fallbackTitle;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating title");

                var fallbackTitle = firstMessage.Length > 30
                    ? firstMessage.Substring(0, 27) + "..."
                    : firstMessage;
                return fallbackTitle;
            }
        }

        private string GenerateCacheKey(string prompt, List<GeminiMessage>? conversationHistory, string? systemPrompt = null)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"gemini:{prompt.GetHashCode()}");
            
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                keyBuilder.Append($":system:{systemPrompt.GetHashCode()}");
            }
            
            if (conversationHistory?.Any() == true)
            {
                var historyHash = string.Join(",", conversationHistory.Select(m => $"{m.Role}:{m.Content.GetHashCode()}"));
                keyBuilder.Append($":history:{historyHash.GetHashCode()}");
            }
            
            return keyBuilder.ToString();
        }

        private async Task<string> EnrichPromptWithTourDataAsync(string prompt)
        {
            try
            {
                var lowerPrompt = prompt.ToLower();

                // Ki?m tra t? khóa liên quan ??n tour
                var tourKeywords = new[] { "tour", "du lịch", "núi bà đen", "tây ninh", "giá", "booking", "đặt tour", "đi du lịch" };

                if (!tourKeywords.Any(keyword => lowerPrompt.Contains(keyword)))
                {
                    return prompt; // Không liên quan ??n tour
                }

                var tourData = new StringBuilder();

                // L?y thông tin tours ph? bi?n
                if (lowerPrompt.Contains("tour") || lowerPrompt.Contains("du lịch"))
                {
                    var availableTours = await _tourDataService.GetAvailableToursAsync(5);
                    if (availableTours.Any())
                    {
                        tourData.AppendLine("\n=== THÔNG TIN TOURS HIỆN CÓ ===");
                        foreach (var tour in availableTours)
                        {
                            tourData.AppendLine($"• {tour.Title}");
                            tourData.AppendLine($"  - Từ: {tour.StartLocation} đến {tour.EndLocation}");
                            tourData.AppendLine($"  - Phí dịch vụ: {tour.Price:N0} VNĐ");
                            tourData.AppendLine($"  - Chỗ trống: {tour.AvailableSlots}/{tour.MaxGuests}");
                            tourData.AppendLine($"  - Loại: {tour.TourType}");

                            if (tour.TourType == "FreeScenic")
                            {
                                tourData.AppendLine($"  - Ghi chú: Chỉ phí dịch vụ, không tốn vé vào cửa");
                            }
                            else if (tour.TourType == "PaidAttraction")
                            {
                                tourData.AppendLine($"  - Ghi chú: Phí dịch vụ + vé vào cửa địa điểm");
                            }

                            if (tour.Highlights.Any())
                            {
                                tourData.AppendLine($"  - Điểm nổi bật: {string.Join(", ", tour.Highlights.Take(2))}");
                            }
                            tourData.AppendLine();

                        }
                    }
                }

                // Tìm ki?m c? th?
                if (lowerPrompt.Contains("núi bà đen"))
                {
                    var nuiBaDenTours = await _tourDataService.SearchToursAsync("Núi Bà Đen", 3);
                    if (nuiBaDenTours.Any())
                    {
                        tourData.AppendLine("\n=== TOURS NÚI BÀ ĐEN ===");
                        foreach (var tour in nuiBaDenTours)
                        {
                            tourData.AppendLine($"• {tour.Title} - {tour.Price:N0} VN? (phí dịch vụ)");
                        }
                    }
                }

                // Tìm theo giá n?u có t? khóa v? giá
                if (lowerPrompt.Contains("rẻ") || lowerPrompt.Contains("tiết kiệm"))
                {
                    var cheapTours = await _tourDataService.GetToursByPriceRangeAsync(0, 300000, 3);
                    if (cheapTours.Any())
                    {
                        tourData.AppendLine("\n=== TOURS PHÍ DỊCH VỤ HỢP LÝ ===");
                        foreach (var tour in cheapTours)
                        {
                            tourData.AppendLine($"• {tour.Title} - {tour.Price:N0} VNĐ");
                            if (tour.TourType == "FreeScenic")
                            {
                                tourData.AppendLine($"  (Không tốn vé vào cửa, chỉ phí dịch vụ)");
                            }
                        }
                    }
                }


                return prompt + tourData.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching prompt with tour data");
                return prompt; // Tr? v? prompt g?c n?u có l?i
            }
        }

        private async Task<GeminiResponse> CreateFallbackResponseAsync(string prompt, Stopwatch stopwatch, string? errorMessage = null)
        {
            stopwatch.Stop();

            var fallbackContent = await GenerateFallbackContentAsync(prompt);

            _logger.LogInformation("Using fallback response for prompt: {Prompt}", prompt.Substring(0, Math.Min(50, prompt.Length)));

            return new GeminiResponse
            {
                Success = true,
                Content = fallbackContent,
                TokensUsed = EstimateTokens(fallbackContent),
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ErrorMessage = errorMessage,
                IsFallback = true
            };
        }

        private async Task<string> GenerateFallbackContentAsync(string prompt)
        {
            var lowerPrompt = prompt.ToLower();

            // Fallback cho sản phẩm - luôn lấy từ database
            if (lowerPrompt.Contains("sản phẩm") || lowerPrompt.Contains("mua") || lowerPrompt.Contains("shop") || 
                lowerPrompt.Contains("bánh tráng") || lowerPrompt.Contains("gốm sứ") || lowerPrompt.Contains("đặc sản"))
            {
                return "Xin lỗi, hệ thống AI tạm thời không khả dụng. Tuy nhiên, bạn có thể xem danh sách sản phẩm thực tế " +
                       "trong hệ thống của chúng tôi hoặc liên hệ trực tiếp với các shop để được tư vấn chi tiết. " +
                       "Vui lòng thử lại sau hoặc sử dụng chức năng duyệt sản phẩm trực tiếp.";
            }

            // Fallback cho tour - lấy thông tin thực từ database
            if (lowerPrompt.Contains("tour") || lowerPrompt.Contains("du lịch"))
            {
                try
                {
                    var tours = await _tourDataService.GetAvailableToursAsync(3);
                    if (tours.Any())
                    {
                        var tourInfo = new StringBuilder();
                        tourInfo.AppendLine("Hiện tại chúng tôi có các tour sau (dữ liệu từ hệ thống):");
                        foreach (var tour in tours)
                        {
                            tourInfo.AppendLine($"• {tour.Title}");
                            tourInfo.AppendLine($"  - Từ: {tour.StartLocation} → {tour.EndLocation}");
                            tourInfo.AppendLine($"  - Giá dịch vụ: {tour.Price:N0} VNĐ");
                            tourInfo.AppendLine($"  - Còn {tour.AvailableSlots} chỗ trống");
                            if (tour.TourType == "FreeScenic")
                            {
                                tourInfo.AppendLine($"  - Loại: Danh lam thắng cảnh (không tốn vé vào cửa)");
                            }
                            else if (tour.TourType == "PaidAttraction")
                            {
                                tourInfo.AppendLine($"  - Loại: Khu vui chơi (có vé vào cửa)");
                            }
                            tourInfo.AppendLine();
                        }
                        tourInfo.AppendLine("(Đây là dữ liệu thực từ hệ thống của chúng tôi, không phải dữ liệu giả)");
                        return tourInfo.ToString();
                    }
                    else
                    {
                        return "Hiện tại không có tour nào khả dụng trong hệ thống. " +
                               "Vui lòng liên hệ trực tiếp để biết thêm thông tin chi tiết.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting real tour data for fallback");
                    return "Xin lỗi, không thể truy xuất thông tin tour từ cơ sở dữ liệu hiện tại. " +
                           "Vui lòng thử lại sau hoặc liên hệ trực tiếp để được hỗ trợ.";
                }
            }

            // Fallback cho Núi Bà Đen - thông tin thực tế, không phải tạo dữ liệu
            if (lowerPrompt.Contains("núi bà đen") || lowerPrompt.Contains("núi bà"))
            {
                try
                {
                    var nuiBaDenTours = await _tourDataService.SearchToursAsync("Núi Bà Đen", 3);
                    if (nuiBaDenTours.Any())
                    {
                        var nuiBaDenInfo = new StringBuilder();
                        nuiBaDenInfo.AppendLine("Thông tin tour Núi Bà Đen hiện có trong hệ thống:");
                        foreach (var tour in nuiBaDenTours)
                        {
                            nuiBaDenInfo.AppendLine($"• {tour.Title} - {tour.Price:N0} VNĐ");
                        }
                        nuiBaDenInfo.AppendLine("Vui lòng liên hệ để biết thêm chi tiết và đặt tour.");
                        return nuiBaDenInfo.ToString();
                    }
                    else
                    {
                        return "Hiện tại chưa có tour Núi Bà Đen nào khả dụng trong hệ thống. " +
                               "Vui lòng liên hệ để được thông báo khi có tour mới.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting Núi Bà Đen tour data for fallback");
                    return "Không thể truy xuất thông tin tour Núi Bà Đen từ cơ sở dữ liệu. " +
                           "Vui lòng liên hệ trực tiếp để được hỗ trợ.";
                }
            }

            // Fallback chung - không tạo dữ liệu giả
            return "Xin lỗi, hệ thống AI tạm thời không khả dụng và không thể truy xuất thông tin từ cơ sở dữ liệu. " +
                   "Vui lòng thử lại sau hoặc liên hệ trực tiếp để được hỗ trợ chi tiết.";
        }

        private object CreateRequestPayload(string prompt, List<GeminiMessage>? conversationHistory, string? systemPrompt = null)
        {
            var contents = new List<object>();

            // Use provided systemPrompt or fall back to default
            var effectiveSystemPrompt = systemPrompt ?? _geminiSettings.SystemPrompt;
            if (!string.IsNullOrEmpty(effectiveSystemPrompt))
            {
                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = effectiveSystemPrompt } }
                });
            }

            if (conversationHistory?.Any() == true)
            {
                var lastMessage = conversationHistory.Last();
                contents.Add(new
                {
                    role = lastMessage.Role == "AI" ? "model" : "user",
                    parts = new[] { new { text = lastMessage.Content } }
                });
            }

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = prompt } }
            });

            return new
            {
                contents = contents,
                generationConfig = new
                {
                    temperature = Math.Min(_geminiSettings.Temperature, 0.8),
                    maxOutputTokens = Math.Min(_geminiSettings.MaxTokens, 2048),
                    topP = 0.8,
                    topK = 40,
                    candidateCount = 1,
                    stopSequences = new[] { "\n\n\n", "---" }
                },
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_ONLY_HIGH" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_ONLY_HIGH" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_ONLY_HIGH" },
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_ONLY_HIGH" }
                }
            };
        }

        private int EstimateTokens(string text)
        {
            return text.Length / 4; // ??c l??ng s? tokens t? ?? dài text
        }

        public async Task<GeminiResponse> GenerateContentAsync(string message)
        {
            return await GenerateContentAsync(message, conversationHistory: null);
        }

        public async Task<GeminiResponse> GenerateContentAsync(string message, string systemPrompt)
        {
            return await GenerateContentAsync(message, systemPrompt, conversationHistory: null);
        }

        public async Task<GeminiResponse> GenerateContentAsync(string message, string systemPrompt, List<GeminiMessage>? conversationHistory = null)
        {
            var stopwatch = Stopwatch.StartNew();

            // Tạo cache key từ prompt và conversation history
            var cacheKey = GenerateCacheKey(message, conversationHistory, systemPrompt);
            
            // Kiểm tra cache trước
            if (_geminiSettings.UseCache && _cache.TryGetValue(cacheKey, out GeminiResponse? cachedResponse))
            {
                stopwatch.Stop();
                _logger.LogInformation("Using cached response for prompt: {Prompt}", 
                    message.Substring(0, Math.Min(50, message.Length)));
                
                cachedResponse!.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                return cachedResponse;
            }

            // Log quota status trước khi gọi API
            if (_geminiSettings.EnableQuotaTracking)
            {
                var quotaToday = QuotaTracker.GetRequestCountToday(_geminiSettings.ApiKey);
                _logger.LogInformation("Current quota today: {QuotaToday}", quotaToday);

                if (!QuotaTracker.CanMakeRequest(_geminiSettings.ApiKey, 
                    _geminiSettings.RateLimitPerMinute, 
                    _geminiSettings.RateLimitPerDay,
                    _geminiSettings.RequestDelayMs))
                {
                    _logger.LogWarning("Rate limit exceeded.");
                    
                    var waitTime = QuotaTracker.GetTimeUntilNextRequest(_geminiSettings.ApiKey, _geminiSettings.RequestDelayMs);
                    if (waitTime > TimeSpan.Zero && waitTime < TimeSpan.FromMinutes(2))
                    {
                        _logger.LogInformation("Waiting {Seconds}s before next request", waitTime.TotalSeconds);
                        await Task.Delay(waitTime);
                    }
                    else
                    {
                        _logger.LogWarning("Wait time too long ({Seconds}s), using fallback immediately", waitTime.TotalSeconds);
                        return await CreateFallbackResponseAsync(message, stopwatch, "Rate limit exceeded - using fallback");
                    }
                }
            }

            // Enrich prompt với thông tin tour nếu có từ khóa liên quan
            var enrichedPrompt = await EnrichPromptWithTourDataAsync(message);

            // Chỉ thử 1 lần để tiết kiệm quota
            for (int attempt = 1; attempt <= _geminiSettings.MaxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Gemini API attempt {Attempt}/{MaxRetries}", attempt, _geminiSettings.MaxRetries);

                    // Ghi nhận request để tracking quota
                    if (_geminiSettings.EnableQuotaTracking)
                    {
                        QuotaTracker.RecordRequest(_geminiSettings.ApiKey);
                    }

                    // Tạo request payload với cấu hình tối ưu, sử dụng systemPrompt parameter
                    var requestPayload = CreateRequestPayload(enrichedPrompt, conversationHistory, systemPrompt);
                    var jsonPayload = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    });
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Tạo URL v?i API key
                    var url = $"{_geminiSettings.ApiUrl}?key={_geminiSettings.ApiKey}";

                    if (attempt == 1)
                    {
                        _logger.LogInformation("Sending request to Gemini API. Model: {Model}, Quota today: {QuotaToday}", 
                            _geminiSettings.Model, 
                            _geminiSettings.EnableQuotaTracking ? QuotaTracker.GetRequestCountToday(_geminiSettings.ApiKey) : 0);
                    }

                    // Timeout ?? nhanh fail-over
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_geminiSettings.FallbackTimeoutSeconds));

                    // G?i request
                    var response = await _httpClient.PostAsync(url, content, cts.Token);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Attempt {Attempt}: Gemini API response status: {Status}", attempt, response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
                        if (string.IsNullOrWhiteSpace(responseContent))
                        {
                            _logger.LogWarning("Attempt {Attempt}: Gemini API returned empty response body", attempt);
                            return await CreateFallbackResponseAsync(message, stopwatch, "Empty response from API");
                        }

                        try
                        {
                            using var jsonDoc = JsonDocument.Parse(responseContent);
                            
                            if (jsonDoc.RootElement.TryGetProperty("candidates", out var candidatesElement) &&
                                candidatesElement.ValueKind == JsonValueKind.Array &&
                                candidatesElement.GetArrayLength() > 0)
                            {
                                var firstCandidate = candidatesElement[0];
                                if (firstCandidate.TryGetProperty("content", out var contentElement) &&
                                    contentElement.TryGetProperty("parts", out var partsElement) &&
                                    partsElement.ValueKind == JsonValueKind.Array &&
                                    partsElement.GetArrayLength() > 0)
                                {
                                    var firstPart = partsElement[0];
                                    if (firstPart.TryGetProperty("text", out var textElement))
                                    {
                                        var generatedText = textElement.GetString();
                                        if (!string.IsNullOrWhiteSpace(generatedText))
                                        {
                                            var tokensUsed = EstimateTokens(message + generatedText);

                                            stopwatch.Stop();
                                            _logger.LogInformation("✓ Gemini API SUCCESS! Text length: {Length}, Tokens: {Tokens}, Time: {Time}ms",
                                                generatedText.Length, tokensUsed, stopwatch.ElapsedMilliseconds);

                                            var successResponse = new GeminiResponse
                                            {
                                                Success = true,
                                                Content = generatedText,
                                                TokensUsed = tokensUsed,
                                                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                                            };

                                            // Cache response
                                            if (_geminiSettings.UseCache)
                                            {
                                                var cacheExpiry = TimeSpan.FromMinutes(_geminiSettings.CacheExpirationMinutes);
                                                _cache.Set(cacheKey, successResponse, cacheExpiry);
                                                _logger.LogInformation("Response cached for {Minutes} minutes", _geminiSettings.CacheExpirationMinutes);
                                            }

                                            return successResponse;
                                        }
                                    }
                                }
                            }

                            _logger.LogWarning("Attempt {Attempt}: Gemini API returned response but no valid content found", attempt);
                            return await CreateFallbackResponseAsync(message, stopwatch, "No valid content in API response");
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger.LogError(jsonEx, "Attempt {Attempt}: JSON deserialization error", attempt);
                            return await CreateFallbackResponseAsync(message, stopwatch, "JSON parsing error");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("✗ Attempt {Attempt}: Gemini API failed. Status: {Status}, Response: {Response}",
                            attempt, response.StatusCode, responseContent.Substring(0, Math.Min(500, responseContent.Length)));

                        // Nếu là 429 (rate limit), không retry - dùng fallback ngay
                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            _logger.LogError("✗✗ QUOTA EXCEEDED! Using fallback response.");
                            return await CreateFallbackResponseAsync(message, stopwatch, "Quota exceeded - API returned 429");
                        }

                        // Nếu là 503 (overload), fail nhanh và dùng fallback
                        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            _logger.LogWarning("Model overloaded (503), using fallback immediately");
                            return await CreateFallbackResponseAsync(message, stopwatch, "Gemini API is overloaded");
                        }

                        // Không retry cho các lỗi khác để tiết kiệm quota
                        return await CreateFallbackResponseAsync(message, stopwatch, $"API request failed: {response.StatusCode}");
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Attempt {Attempt}: Request timeout, using fallback", attempt);
                    return await CreateFallbackResponseAsync(message, stopwatch, "Request timeout");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Attempt {Attempt}: Exception calling Gemini API", attempt);
                    return await CreateFallbackResponseAsync(message, stopwatch, ex.Message);
                }
            }

            // Fallback cuối cùng
            return await CreateFallbackResponseAsync(message, stopwatch, $"Gemini API failed after {_geminiSettings.MaxRetries} attempts");
        }

        public async Task<bool> CheckApiStatusAsync()
        {
            try
            {
                _logger.LogInformation("Checking Gemini API status...");

                // Use a simple test message
                var testResponse = await GenerateContentAsync("Hello", conversationHistory: null);
                
                var isHealthy = testResponse.Success && !string.IsNullOrEmpty(testResponse.Content);
                
                _logger.LogInformation("Gemini API health check result: {IsHealthy}", isHealthy);
                return isHealthy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Gemini API status");
                return false;
            }
        }
    }
}