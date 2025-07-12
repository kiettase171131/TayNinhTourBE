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
    /// Service ?? t�ch h?p v?i Gemini AI API v?i rate limiting v� caching
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

            // T?o cache key t? prompt v� conversation history
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

            // Enrich prompt v?i th�ng tin tour n?u c� t? kh�a li�n quan
            var enrichedPrompt = await EnrichPromptWithTourDataAsync(prompt);

            // Ch? th? 1 l?n ?? ti?t ki?m quota
            for (int attempt = 1; attempt <= _geminiSettings.MaxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Gemini API attempt {Attempt}/{MaxRetries}", attempt, _geminiSettings.MaxRetries);

                    // Ghi nh?n request ?? tracking quota
                    if (_geminiSettings.EnableQuotaTracking)
                    {
                        QuotaTracker.RecordRequest(_geminiSettings.ApiKey);
                    }

                    // T?o request payload v?i c?u h�nh t?i ?u
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

                        // N?u l� 429 (rate limit), kh�ng retry - d�ng fallback ngay
                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            _logger.LogError("?? QUOTA EXCEEDED! Using fallback response.");
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "Quota exceeded - API returned 429");
                        }

                        // N?u l� 503 (overload), fail nhanh v� d�ng fallback
                        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            _logger.LogWarning("Model overloaded (503), using fallback immediately");
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "Gemini API is overloaded");
                        }

                        // Kh�ng retry cho c�c l?i kh�c ?? ti?t ki?m quota
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

            // Fallback cu?i c�ng
            return await CreateFallbackResponseAsync(prompt, stopwatch, $"Gemini API failed after {_geminiSettings.MaxRetries} attempts");
        }

        public async Task<string> GenerateTitleAsync(string firstMessage)
        {
            try
            {
                var titlePrompt = $"T?o ti�u ?? ng?n cho: {firstMessage}";

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

        private string GenerateCacheKey(string prompt, List<GeminiMessage>? conversationHistory)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"gemini:{prompt.GetHashCode()}");
            
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

                // Ki?m tra t? kh�a li�n quan ??n tour
                var tourKeywords = new[] { "tour", "du l?ch", "n�i b� ?en", "t�y ninh", "gi�", "booking", "??t tour", "?i du l?ch" };

                if (!tourKeywords.Any(keyword => lowerPrompt.Contains(keyword)))
                {
                    return prompt; // Kh�ng li�n quan ??n tour
                }

                var tourData = new StringBuilder();

                // L?y th�ng tin tours ph? bi?n
                if (lowerPrompt.Contains("tour") || lowerPrompt.Contains("du l?ch"))
                {
                    var availableTours = await _tourDataService.GetAvailableToursAsync(5);
                    if (availableTours.Any())
                    {
                        tourData.AppendLine("\n=== TH�NG TIN TOURS HI?N C� ===");
                        foreach (var tour in availableTours)
                        {
                            tourData.AppendLine($"� {tour.Title}");
                            tourData.AppendLine($"  - T?: {tour.StartLocation} ??n {tour.EndLocation}");
                            tourData.AppendLine($"  - Ph� d?ch v?: {tour.Price:N0} VN?");
                            tourData.AppendLine($"  - Ch? tr?ng: {tour.AvailableSlots}/{tour.MaxGuests}");
                            tourData.AppendLine($"  - Lo?i: {tour.TourType}");

                            if (tour.TourType == "FreeScenic")
                            {
                                tourData.AppendLine($"  - Ghi ch�: Ch? ph� d?ch v?, kh�ng t?n v� v�o c?a");
                            }
                            else if (tour.TourType == "PaidAttraction")
                            {
                                tourData.AppendLine($"  - Ghi ch�: Ph� d?ch v? + v� v�o c?a ??a ?i?m");
                            }

                            if (tour.Highlights.Any())
                            {
                                tourData.AppendLine($"  - ?i?m n?i b?t: {string.Join(", ", tour.Highlights.Take(2))}");
                            }
                            tourData.AppendLine();
                        }
                    }
                }

                // T�m ki?m c? th?
                if (lowerPrompt.Contains("n�i b� ?en"))
                {
                    var nuiBaDenTours = await _tourDataService.SearchToursAsync("N�i B� ?en", 3);
                    if (nuiBaDenTours.Any())
                    {
                        tourData.AppendLine("\n=== TOURS N�I B� ?EN ===");
                        foreach (var tour in nuiBaDenTours)
                        {
                            tourData.AppendLine($"� {tour.Title} - {tour.Price:N0} VN? (ph� d?ch v?)");
                        }
                    }
                }

                // T�m theo gi� n?u c� t? kh�a v? gi�
                if (lowerPrompt.Contains("r?") || lowerPrompt.Contains("ti?t ki?m"))
                {
                    var cheapTours = await _tourDataService.GetToursByPriceRangeAsync(0, 300000, 3);
                    if (cheapTours.Any())
                    {
                        tourData.AppendLine("\n=== TOURS PH� D?CH V? H?P L� ===");
                        foreach (var tour in cheapTours)
                        {
                            tourData.AppendLine($"� {tour.Title} - {tour.Price:N0} VN?");
                            if (tour.TourType == "FreeScenic")
                            {
                                tourData.AppendLine($"  (Kh�ng t?n v� v�o c?a, ch? ph� d?ch v?)");
                            }
                        }
                    }
                }

                return prompt + tourData.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching prompt with tour data");
                return prompt; // Tr? v? prompt g?c n?u c� l?i
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

            // Fallback cho b�nh tr�ng T�y Ninh
            if (lowerPrompt.Contains("b�nh tr�ng") || lowerPrompt.Contains("banh trang"))
            {
                if (lowerPrompt.Contains("gi�") || lowerPrompt.Contains("bao nhi�u"))
                {
                    return "B�nh tr�ng Tr?ng B�ng T�y Ninh c� gi� kho?ng 15.000-25.000 VN?/kg t�y lo?i. " +
                           "B�nh tr�ng n??ng mu?i t�m kho?ng 5.000-10.000 VN?/c�i. " +
                           "C�c tour c?a ch�ng t�i th??ng gh� mua b�nh tr�ng l�m qu�!";
                }
                return "B�nh tr�ng Tr?ng B�ng l� ??c s?n n?i ti?ng T�y Ninh, l�m t? g?o ST25. " +
                       "C� b�nh tr�ng n??ng mu?i t�m, b�nh tr�ng cu?n th?t n??ng r?t ngon!";
            }

            // Fallback v?i th�ng tin tour th?c t?
            if (lowerPrompt.Contains("tour") || lowerPrompt.Contains("du l?ch"))
            {
                try
                {
                    var tours = await _tourDataService.GetAvailableToursAsync(3);
                    if (tours.Any())
                    {
                        var tourList = string.Join(", ", tours.Select(t => $"{t.Title} ({t.Price:N0} VN?)"));
                        return $"Hi?n t?i ch�ng t�i c� c�c tour: {tourList}. B?n mu?n bi?t th�m chi ti?t tour n�o?";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in fallback tour data");
                }

                return "Ch�ng t�i c� tour danh lam th?ng c?nh (kh�ng t?n v� v�o c?a) v� tour khu vui ch?i " +
                       "(c� v� v�o c?a) v?i nhi?u m?c gi� kh�c nhau. Li�n h? ?? bi?t th�m chi ti?t!";
            }

            if (lowerPrompt.Contains("n�i b� ?en") || lowerPrompt.Contains("n�i b�"))
            {
                return "N�i B� ?en cao nh?t Nam B? (986m), c� c�p treo v� ch�a linh thi�ng. " +
                       "Ch�ng t�i c� tour ?i N�i B� ?en v?i d?ch v? t?t nh?t!";
            }

            return "Xin l?i, t�i kh�ng hi?u y�u c?u c?a b?n. Vui l�ng cung c?p th�m th�ng tin chi ti?t.";
        }

        private object CreateRequestPayload(string prompt, List<GeminiMessage>? conversationHistory)
        {
            var contents = new List<object>();

            if (!string.IsNullOrEmpty(_geminiSettings.SystemPrompt))
            {
                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = _geminiSettings.SystemPrompt } }
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
            return text.Length / 4; // ??c l??ng s? tokens t? ?? d�i text
        }
    }
}