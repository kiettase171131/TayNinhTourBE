using System.Diagnostics;
using System.Text;
using System.Text.Json;
<<<<<<< Updated upstream
using System.Text.Json.Serialization;
=======
using Microsoft.Extensions.Caching.Memory;
>>>>>>> Stashed changes
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
<<<<<<< Updated upstream
    /// <summary>
    /// Service ?? tích h?p v?i Gemini AI API
    /// </summary>
=======
>>>>>>> Stashed changes
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _geminiSettings;
        private readonly ILogger<GeminiAIService> _logger;
        private readonly IAITourDataService _tourDataService;

        public GeminiAIService(
            HttpClient httpClient,
            IOptions<GeminiSettings> geminiSettings,
            ILogger<GeminiAIService> logger,
            IAITourDataService tourDataService)
        {
            _httpClient = httpClient;
            _geminiSettings = geminiSettings.Value;
            _logger = logger;
            _tourDataService = tourDataService;
        }

        public async Task<GeminiResponse> GenerateContentAsync(string prompt, List<GeminiMessage>? conversationHistory = null)
        {
            var stopwatch = Stopwatch.StartNew();
<<<<<<< Updated upstream

            // Enrich prompt v?i thông tin tour n?u có t? khóa liên quan
            var enrichedPrompt = await EnrichPromptWithTourDataAsync(prompt);

            const int maxRetries = 3;
            const int baseDelayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Gemini API attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
=======
            var cacheKey = GenerateCacheKey(prompt, conversationHistory);
            
            // Ki?m tra cache tr??c
            if (_geminiSettings.UseCache && _cache.TryGetValue(cacheKey, out GeminiResponse? cachedResponse))
            {
                stopwatch.Stop();
                _logger.LogInformation("? Using cached response for prompt: {Prompt}", 
                    prompt.Substring(0, Math.Min(50, prompt.Length)));
                
                cachedResponse!.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                return cachedResponse;
            }

            // Ki?m tra Circuit Breaker tr??c khi g?i API
            if (_geminiSettings.CircuitBreakerEnabled && 
                QuotaTracker.IsCircuitBreakerOpen(_geminiSettings.ApiKey, 
                    _geminiSettings.CircuitBreakerFailureThreshold, 
                    _geminiSettings.CircuitBreakerRecoveryTimeMinutes))
            {
                _logger.LogWarning("?? Circuit Breaker OPEN - API ?ang overload, dùng fallback ngay");
                return await CreateFallbackResponseAsync(prompt, stopwatch, "Circuit breaker open - API overloaded");
            }

            // Quota tracking
            if (_geminiSettings.EnableQuotaTracking)
            {
                var quotaToday = QuotaTracker.GetRequestCountToday(_geminiSettings.ApiKey);
                _logger.LogInformation("Current quota today: {QuotaToday}", quotaToday);

                if (!QuotaTracker.CanMakeRequest(_geminiSettings.ApiKey, 
                    _geminiSettings.RateLimitPerMinute, 
                    _geminiSettings.RateLimitPerDay,
                    _geminiSettings.RequestDelayMs))
                {
                    _logger.LogWarning("?? Rate limit exceeded - using smart fallback");
                    return await CreateFallbackResponseAsync(prompt, stopwatch, "Rate limit exceeded");
                }
            }

            var enrichedPrompt = await EnrichPromptWithTourDataAsync(prompt);

            // Smart retry v?i Circuit Breaker
            for (int attempt = 1; attempt <= _geminiSettings.MaxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("?? Gemini API attempt {Attempt}/{MaxRetries} - Adaptive timeout: {Timeout}s", 
                        attempt, _geminiSettings.MaxRetries, _geminiSettings.FallbackTimeoutSeconds);

                    if (_geminiSettings.EnableQuotaTracking)
                    {
                        QuotaTracker.RecordRequest(_geminiSettings.ApiKey);
                    }
>>>>>>> Stashed changes

                    var requestPayload = CreateRequestPayload(enrichedPrompt, conversationHistory);
                    var jsonPayload = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    });

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var url = $"{_geminiSettings.ApiUrl}?key={_geminiSettings.ApiKey}";

<<<<<<< Updated upstream
                    if (attempt == 1)
                    {
                        _logger.LogInformation("Sending request to Gemini API. Model: {Model}", _geminiSettings.Model);
                    }

                    // Timeout ng?n ?? nhanh fail-over
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                    // G?i request
=======
                    // Adaptive timeout: t?ng timeout cho retry attempts
                    var timeoutSeconds = _geminiSettings.FallbackTimeoutSeconds + (attempt - 1) * 5;
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                    
>>>>>>> Stashed changes
                    var response = await _httpClient.PostAsync(url, content, cts.Token);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("? Attempt {Attempt}: Gemini API response status: {Status}", attempt, response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
<<<<<<< Updated upstream
                        if (string.IsNullOrWhiteSpace(responseContent))
                        {
                            _logger.LogWarning("Attempt {Attempt}: Gemini API returned empty response body", attempt);
                            if (attempt < maxRetries)
                            {
                                await Task.Delay(baseDelayMs * attempt);
                                continue;
                            }

                            return await CreateFallbackResponseAsync(prompt, stopwatch);
                        }

=======
>>>>>>> Stashed changes
                        try
                        {
                            var geminiApiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (geminiApiResponse?.Candidates?.Any() == true)
                            {
                                var candidate = geminiApiResponse.Candidates[0];
                                if (candidate.Content?.Parts?.Any() == true)
                                {
                                    var part = candidate.Content.Parts[0];
                                    if (!string.IsNullOrWhiteSpace(part.Text))
                                    {
                                        var generatedText = part.Text;
                                        var tokensUsed = EstimateTokens(prompt + generatedText);

                                        stopwatch.Stop();
                                        _logger.LogInformation("Attempt {Attempt}: Gemini API success. Text length: {Length}, Tokens: {Tokens}, Time: {Time}ms",
                                            attempt, generatedText.Length, tokensUsed, stopwatch.ElapsedMilliseconds);

                                        return new GeminiResponse
                                        {
<<<<<<< Updated upstream
                                            Success = true,
                                            Content = generatedText,
                                            TokensUsed = tokensUsed,
                                            ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                                        };
=======
                                            var tokensUsed = EstimateTokens(prompt + generatedText);

                                            stopwatch.Stop();
                                            _logger.LogInformation("?? REAL AI SUCCESS! Time: {Time}ms, Tokens: {Tokens}",
                                                stopwatch.ElapsedMilliseconds, tokensUsed);

                                            // Ghi nh?n thành công cho Circuit Breaker
                                            QuotaTracker.RecordSuccess(_geminiSettings.ApiKey);

                                            var successResponse = new GeminiResponse
                                            {
                                                Success = true,
                                                Content = generatedText,
                                                TokensUsed = tokensUsed,
                                                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                                            };

                                            // Cache response lâu ?? tránh g?i API nhi?u
                                            if (_geminiSettings.UseCache)
                                            {
                                                var cacheExpiry = TimeSpan.FromMinutes(_geminiSettings.CacheExpirationMinutes);
                                                _cache.Set(cacheKey, successResponse, cacheExpiry);
                                            }

                                            return successResponse;
                                        }
>>>>>>> Stashed changes
                                    }
                                }
                            }

<<<<<<< Updated upstream
                            _logger.LogWarning("Attempt {Attempt}: Gemini API returned response but no valid content found", attempt);
                            if (attempt < maxRetries)
                            {
                                await Task.Delay(baseDelayMs * attempt);
                                continue;
                            }
=======
                            _logger.LogWarning("?? Empty response from API - recording failure");
                            QuotaTracker.RecordFailure(_geminiSettings.ApiKey, _geminiSettings.CircuitBreakerFailureThreshold);
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "No valid content in response");
>>>>>>> Stashed changes
                        }
                        catch (JsonException)
                        {
<<<<<<< Updated upstream
                            _logger.LogError(jsonEx, "Attempt {Attempt}: JSON deserialization error", attempt);
                            if (attempt < maxRetries)
                            {
                                await Task.Delay(baseDelayMs * attempt);
                                continue;
                            }
=======
                            _logger.LogWarning("?? JSON parsing error - recording failure");
                            QuotaTracker.RecordFailure(_geminiSettings.ApiKey, _geminiSettings.CircuitBreakerFailureThreshold);
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "JSON parsing error");
>>>>>>> Stashed changes
                        }
                    }
                    else
                    {
<<<<<<< Updated upstream
                        _logger.LogWarning("Attempt {Attempt}: Gemini API failed. Status: {Status}, Response: {Response}",
                            attempt, response.StatusCode, responseContent);
=======
                        _logger.LogWarning("? API failed. Status: {Status} - recording failure", response.StatusCode);
                        QuotaTracker.RecordFailure(_geminiSettings.ApiKey, _geminiSettings.CircuitBreakerFailureThreshold);

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "Quota exceeded");
                        }
>>>>>>> Stashed changes

                        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            if (attempt < _geminiSettings.MaxRetries)
                            {
                                var retryDelay = _geminiSettings.BaseDelayMs * attempt;
                                _logger.LogInformation("?? Server overload, retry in {Delay}ms...", retryDelay);
                                await Task.Delay(retryDelay);
                                continue;
                            }
                            else
                            {
                                return await CreateFallbackResponseAsync(prompt, stopwatch, "Server overloaded after retries");
                            }
                        }

<<<<<<< Updated upstream
                        // Retry on other server errors (5xx) or rate limiting (429)
                        if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            if (attempt < maxRetries)
                            {
                                var delay = baseDelayMs * (int)Math.Pow(2, attempt - 1); // Exponential backoff
                                _logger.LogInformation("Retrying in {Delay}ms...", delay);
                                await Task.Delay(delay);
                                continue;
                            }
                        }

                        // Don't retry on client errors (4xx except 429)
                        return await CreateFallbackResponseAsync(prompt, stopwatch, $"API request failed: {response.StatusCode}");
=======
                        return await CreateFallbackResponseAsync(prompt, stopwatch, $"API error: {response.StatusCode}");
>>>>>>> Stashed changes
                    }
                }
                catch (TaskCanceledException)
                {
                    var currentTimeout = _geminiSettings.FallbackTimeoutSeconds + (attempt - 1) * 5;
                    _logger.LogWarning("? Request timeout after {Timeout}s", currentTimeout);
                    QuotaTracker.RecordFailure(_geminiSettings.ApiKey, _geminiSettings.CircuitBreakerFailureThreshold);
                    
                    if (attempt < _geminiSettings.MaxRetries)
                    {
                        var retryDelay = _geminiSettings.BaseDelayMs * attempt;
                        _logger.LogInformation("?? Timeout, retry in {Delay}ms...", retryDelay);
                        await Task.Delay(retryDelay);
                        continue;
                    }
                    else
                    {
                        return await CreateFallbackResponseAsync(prompt, stopwatch, "Request timeout after retries");
                    }
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt}: Exception calling Gemini API, retrying...", attempt);
                    await Task.Delay(baseDelayMs * attempt);
                    continue;
                }
                catch (Exception ex)
                {
<<<<<<< Updated upstream
                    _logger.LogError(ex, "Final attempt failed: Error calling Gemini API");
=======
                    _logger.LogError(ex, "?? Exception calling Gemini API - recording failure");
                    QuotaTracker.RecordFailure(_geminiSettings.ApiKey, _geminiSettings.CircuitBreakerFailureThreshold);
>>>>>>> Stashed changes
                    return await CreateFallbackResponseAsync(prompt, stopwatch, ex.Message);
                }
            }

<<<<<<< Updated upstream
            // N?u h?t retry attempts, tr? v? fallback response
            return await CreateFallbackResponseAsync(prompt, stopwatch, $"Gemini API failed after {maxRetries} attempts");
=======
            return await CreateFallbackResponseAsync(prompt, stopwatch, "All attempts failed");
>>>>>>> Stashed changes
        }

        public async Task<string> GenerateTitleAsync(string firstMessage)
        {
            try
            {
                var titlePrompt = $"T?o tiêu ?? ng?n cho: {firstMessage}";
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
                    return firstMessage.Length > 30 ? firstMessage.Substring(0, 27) + "..." : firstMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating title");
                return firstMessage.Length > 30 ? firstMessage.Substring(0, 27) + "..." : firstMessage;
            }
        }

        private async Task<string> EnrichPromptWithTourDataAsync(string prompt)
        {
            try
            {
                var lowerPrompt = prompt.ToLower();
                var tourKeywords = new[] { "tour", "du l?ch", "núi bà ?en", "tây ninh", "giá", "booking", "??t tour" };

                if (!tourKeywords.Any(keyword => lowerPrompt.Contains(keyword)))
                {
                    return prompt;
                }

                var tourData = new StringBuilder();

                if (lowerPrompt.Contains("tour") || lowerPrompt.Contains("du l?ch"))
                {
                    var availableTours = await _tourDataService.GetAvailableToursAsync(3);
                    if (availableTours.Any())
                    {
                        tourData.AppendLine("\n=== TOURS HI?N CÓ ===");
                        foreach (var tour in availableTours)
                        {
<<<<<<< Updated upstream
                            tourData.AppendLine($"• {tour.Title}");
                            tourData.AppendLine($"  - T?: {tour.StartLocation} ? {tour.EndLocation}");
                            tourData.AppendLine($"  - Phí d?ch v?: {tour.Price:N0} VN?");
                            tourData.AppendLine($"  - Ch? tr?ng: {tour.AvailableSlots}/{tour.MaxGuests}");
                            tourData.AppendLine($"  - Lo?i: {tour.TourType}");

                            // Thêm thông tin v? c?u trúc phí
                            if (tour.TourType == "FreeScenic")
                            {
                                tourData.AppendLine($"  - Ghi chú: Ch? phí d?ch v?, không t?n vé vào c?a");
                            }
                            else if (tour.TourType == "PaidAttraction")
                            {
                                tourData.AppendLine($"  - Ghi chú: Phí d?ch v? + vé vào c?a ??a ?i?m");
                            }

                            if (tour.Highlights.Any())
                            {
                                tourData.AppendLine($"  - ?i?m n?i b?t: {string.Join(", ", tour.Highlights.Take(2))}");
                            }
                            tourData.AppendLine();
                        }
                    }
                }

                // Tìm ki?m c? th?
                if (lowerPrompt.Contains("núi bà ?en"))
                {
                    var nuiBaDenTours = await _tourDataService.SearchToursAsync("Núi Bà ?en", 3);
                    if (nuiBaDenTours.Any())
                    {
                        tourData.AppendLine("\n=== TOURS NÚI BÀ ?EN ===");
                        foreach (var tour in nuiBaDenTours)
                        {
                            tourData.AppendLine($"• {tour.Title} - {tour.Price:N0} VN? (phí d?ch v?)");
                        }
                    }
                }

                // Tìm theo giá n?u có t? khóa v? giá
                if (lowerPrompt.Contains("r?") || lowerPrompt.Contains("ti?t ki?m"))
                {
                    var cheapTours = await _tourDataService.GetToursByPriceRangeAsync(0, 300000, 3);
                    if (cheapTours.Any())
                    {
                        tourData.AppendLine("\n=== TOURS PHÍ D?CH V? H?P LÝ ===");
                        foreach (var tour in cheapTours)
                        {
=======
>>>>>>> Stashed changes
                            tourData.AppendLine($"• {tour.Title} - {tour.Price:N0} VN?");
                        }
                    }
                }

                return prompt + tourData.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching prompt with tour data");
                return prompt;
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

<<<<<<< Updated upstream
            // Fallback v?i thông tin tour th?c t?
            if (lowerPrompt.Contains("tour") || lowerPrompt.Contains("du l?ch"))
            {
                try
                {
                    var tours = await _tourDataService.GetAvailableToursAsync(3);
                    if (tours.Any())
                    {
                        var tourList = string.Join(", ", tours.Select(t => $"{t.Title} ({t.Price:N0} VN?)"));
                        return $"Hi?n t?i chúng tôi có các tour: {tourList}. B?n mu?n bi?t thêm chi ti?t tour nào?";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in fallback tour data");
                }

                return "Tây Ninh có nhi?u tour h?p d?n nh? th?m Núi Bà ?en, di tích l?ch s?. T?t c? tours ??u có phí d?ch v? h?p lý. B?n mu?n bi?t v? tour nào?";
=======
            // Fast pattern matching cho ph?n h?i nhanh
            if (lowerPrompt.Contains("bác h?") || lowerPrompt.Contains("tìm ???ng") || lowerPrompt.Contains("c?u n??c"))
            {
                return "Bác H? và phong trào ?ông Du có m?i liên h? sâu s?c v?i Tây Ninh. " +
                       "Chúng tôi có tour di tích l?ch s? ?? tìm hi?u v? các ??a danh liên quan!";
            }

            if (lowerPrompt.Contains("bánh tráng"))
            {
                if (lowerPrompt.Contains("giá"))
                {
                    return "Bánh tráng Tr?ng Bàng Tây Ninh: 15.000-25.000 VN?/kg. Bánh tráng n??ng: 5.000-10.000 VN?/cái.";
                }
                return "Bánh tráng Tr?ng Bàng là ??c s?n Tây Ninh t? g?o ST25. Bánh tráng n??ng mu?i tôm r?t ngon!";
            }

            if (lowerPrompt.Contains("tour") || lowerPrompt.Contains("du l?ch"))
            {
                return "Chúng tôi có tour Núi Bà ?en, tour di tích l?ch s?, tour ?m th?c v?i nhi?u m?c giá. Liên h? ?? bi?t chi ti?t!";
>>>>>>> Stashed changes
            }

            if (lowerPrompt.Contains("núi bà ?en"))
            {
<<<<<<< Updated upstream
                return "Núi Bà ?en cao nh?t Nam B?, có cáp treo và chùa linh thiêng. Chúng tôi có tour ?i Núi Bà ?en v?i d?ch v? chuyên nghi?p!";
            }

            if (lowerPrompt.Contains("chào") || lowerPrompt.Contains("xin chào") || lowerPrompt.Contains("hello"))
            {
                return "Xin chào! Tôi là tr? lý AI du l?ch Tây Ninh. B?n mu?n tìm hi?u v? tour nào? Chúng tôi có tour Núi Bà ?en, di tích l?ch s? v?i phí d?ch v? h?p lý!";
            }

            if (lowerPrompt.Contains("giá") || lowerPrompt.Contains("chi phí"))
            {
                try
                {
                    var cheapTours = await _tourDataService.GetToursByPriceRangeAsync(0, 500000, 2);
                    if (cheapTours.Any())
                    {
                        var tourInfo = string.Join(", ", cheapTours.Select(t => $"{t.Title} ({t.Price:N0} VN?)"));
                        return $"Chúng tôi có các tour v?i giá h?p lý: {tourInfo}. B?n mu?n bi?t thêm chi ti?t?";
                    }
                }
                catch { }

                return "Giá tour tùy theo lo?i hình. Chúng tôi có tour danh lam th?ng c?nh (không t?n vé vào c?a) và tour khu vui ch?i (có vé vào c?a) v?i nhi?u m?c giá khác nhau.";
            }

            if (lowerPrompt.Contains("?n") || lowerPrompt.Contains("món"))
            {
                return "Tây Ninh có bánh tráng n??ng, bánh canh cua ??ng, cà ri dê Ninh S?n. Các tour c?a chúng tôi th??ng k?t h?p th??ng th?c ?m th?c ??a ph??ng!";
            }

            if (lowerPrompt.Contains("r?") || lowerPrompt.Contains("ti?t ki?m"))
            {
                return "Chúng tôi có tour danh lam th?ng c?nh v?i giá h?p lý (ch? phí d?ch v?, không t?n vé vào c?a). ?ây là l?a ch?n ti?t ki?m ?? khám phá Tây Ninh!";
            }

            return "Tôi là tr? lý du l?ch Tây Ninh! Chúng tôi có nhi?u tour h?p d?n: Núi Bà ?en, di tích l?ch s?, ?m th?c ??a ph??ng v?i phí d?ch v? h?p lý. B?n quan tâm tour nào?";
=======
                return "Núi Bà ?en cao nh?t Nam B? (986m), có cáp treo và chùa linh thiêng. Tour ?i Núi Bà ?en v?i d?ch v? t?t nh?t!";
            }

            if (lowerPrompt.Contains("chào") || lowerPrompt.Contains("hello") || lowerPrompt.Contains("hi"))
            {
                return "Xin chào! Tôi là tr? lý AI du l?ch Tây Ninh. B?n mu?n h?i v? tour, ??a ?i?m, ?m th?c hay di tích l?ch s??";
            }

            if (lowerPrompt.Contains("giá") || lowerPrompt.Contains("bao nhiêu") || lowerPrompt.Contains("chi phí"))
            {
                return "Phí d?ch v? tour t? 100.000-500.000 VN?/ng??i tùy lo?i. Tour danh lam th?ng c?nh th??ng r? h?n tour khu vui ch?i. Liên h? ?? báo giá chi ti?t!";
            }

            return "Tôi là tr? lý AI du l?ch Tây Ninh! H?i v? tours, ??a ?i?m, ?m th?c hay di tích l?ch s? nhé.";
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
                    temperature = Math.Min(_geminiSettings.Temperature, 0.3),
                    maxOutputTokens = Math.Min(_geminiSettings.MaxTokens, 300),
                    topP = 0.7,
                    topK = 10,
                    candidateCount = 1,
                    stopSequences = new[] { "\n\n", "---" }
=======
                    temperature = Math.Min(_geminiSettings.Temperature, 0.8),
                    maxOutputTokens = Math.Min(_geminiSettings.MaxTokens, 2048),
                    topP = 0.8,
                    topK = 40,
                    candidateCount = 1
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        private class GeminiApiResponse
        {
            [JsonPropertyName("candidates")]
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            [JsonPropertyName("content")]
            public Content Content { get; set; } = null!;
        }

        private class Content
        {
            [JsonPropertyName("parts")]
            public List<Part> Parts { get; set; } = null!;
        }

        private class Part
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = null!;
=======
            return text.Length / 4;
>>>>>>> Stashed changes
        }
    }
}