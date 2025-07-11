using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service ?? t�ch h?p v?i Gemini AI API
    /// </summary>
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

            // Enrich prompt v?i th�ng tin tour n?u c� t? kh�a li�n quan
            var enrichedPrompt = await EnrichPromptWithTourDataAsync(prompt);

            const int maxRetries = 3;
            const int baseDelayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Gemini API attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

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
                        _logger.LogInformation("Sending request to Gemini API. Model: {Model}", _geminiSettings.Model);
                    }

                    // Timeout ng?n ?? nhanh fail-over
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                    // G?i request
                    var response = await _httpClient.PostAsync(url, content, cts.Token);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Attempt {Attempt}: Gemini API response status: {Status}", attempt, response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
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
                                            Success = true,
                                            Content = generatedText,
                                            TokensUsed = tokensUsed,
                                            ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                                        };
                                    }
                                }
                            }

                            _logger.LogWarning("Attempt {Attempt}: Gemini API returned response but no valid content found", attempt);
                            if (attempt < maxRetries)
                            {
                                await Task.Delay(baseDelayMs * attempt);
                                continue;
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger.LogError(jsonEx, "Attempt {Attempt}: JSON deserialization error", attempt);
                            if (attempt < maxRetries)
                            {
                                await Task.Delay(baseDelayMs * attempt);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Attempt {Attempt}: Gemini API failed. Status: {Status}, Response: {Response}",
                            attempt, response.StatusCode, responseContent);

                        // N?u l� 503 (overload), fail nhanh v� d�ng fallback
                        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            _logger.LogWarning("Model overloaded (503), using fallback immediately");
                            return await CreateFallbackResponseAsync(prompt, stopwatch, "Gemini API is overloaded");
                        }

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
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Attempt {Attempt}: Request timeout, using fallback", attempt);
                    return await CreateFallbackResponseAsync(prompt, stopwatch, "Request timeout");
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt}: Exception calling Gemini API, retrying...", attempt);
                    await Task.Delay(baseDelayMs * attempt);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Final attempt failed: Error calling Gemini API");
                    return await CreateFallbackResponseAsync(prompt, stopwatch, ex.Message);
                }
            }

            // N?u h?t retry attempts, tr? v? fallback response
            return await CreateFallbackResponseAsync(prompt, stopwatch, $"Gemini API failed after {maxRetries} attempts");
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
                            tourData.AppendLine($"  - T?: {tour.StartLocation} ? {tour.EndLocation}");
                            tourData.AppendLine($"  - Ph� d?ch v?: {tour.Price:N0} VN?");
                            tourData.AppendLine($"  - Ch? tr?ng: {tour.AvailableSlots}/{tour.MaxGuests}");
                            tourData.AppendLine($"  - Lo?i: {tour.TourType}");

                            // Th�m th�ng tin v? c?u tr�c ph�
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

                return "T�y Ninh c� nhi?u tour h?p d?n nh? th?m N�i B� ?en, di t�ch l?ch s?. T?t c? tours ??u c� ph� d?ch v? h?p l�. B?n mu?n bi?t v? tour n�o?";
            }

            if (lowerPrompt.Contains("n�i b� ?en") || lowerPrompt.Contains("n�i b�"))
            {
                return "N�i B� ?en cao nh?t Nam B?, c� c�p treo v� ch�a linh thi�ng. Ch�ng t�i c� tour ?i N�i B� ?en v?i d?ch v? chuy�n nghi?p!";
            }

            if (lowerPrompt.Contains("ch�o") || lowerPrompt.Contains("xin ch�o") || lowerPrompt.Contains("hello"))
            {
                return "Xin ch�o! T�i l� tr? l� AI du l?ch T�y Ninh. B?n mu?n t�m hi?u v? tour n�o? Ch�ng t�i c� tour N�i B� ?en, di t�ch l?ch s? v?i ph� d?ch v? h?p l�!";
            }

            if (lowerPrompt.Contains("gi�") || lowerPrompt.Contains("chi ph�"))
            {
                try
                {
                    var cheapTours = await _tourDataService.GetToursByPriceRangeAsync(0, 500000, 2);
                    if (cheapTours.Any())
                    {
                        var tourInfo = string.Join(", ", cheapTours.Select(t => $"{t.Title} ({t.Price:N0} VN?)"));
                        return $"Ch�ng t�i c� c�c tour v?i gi� h?p l�: {tourInfo}. B?n mu?n bi?t th�m chi ti?t?";
                    }
                }
                catch { }

                return "Gi� tour t�y theo lo?i h�nh. Ch�ng t�i c� tour danh lam th?ng c?nh (kh�ng t?n v� v�o c?a) v� tour khu vui ch?i (c� v� v�o c?a) v?i nhi?u m?c gi� kh�c nhau.";
            }

            if (lowerPrompt.Contains("?n") || lowerPrompt.Contains("m�n"))
            {
                return "T�y Ninh c� b�nh tr�ng n??ng, b�nh canh cua ??ng, c� ri d� Ninh S?n. C�c tour c?a ch�ng t�i th??ng k?t h?p th??ng th?c ?m th?c ??a ph??ng!";
            }

            if (lowerPrompt.Contains("r?") || lowerPrompt.Contains("ti?t ki?m"))
            {
                return "Ch�ng t�i c� tour danh lam th?ng c?nh v?i gi� h?p l� (ch? ph� d?ch v?, kh�ng t?n v� v�o c?a). ?�y l� l?a ch?n ti?t ki?m ?? kh�m ph� T�y Ninh!";
            }

            return "T�i l� tr? l� du l?ch T�y Ninh! Ch�ng t�i c� nhi?u tour h?p d?n: N�i B� ?en, di t�ch l?ch s?, ?m th?c ??a ph??ng v?i ph� d?ch v? h?p l�. B?n quan t�m tour n�o?";
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
                    temperature = Math.Min(_geminiSettings.Temperature, 0.3),
                    maxOutputTokens = Math.Min(_geminiSettings.MaxTokens, 300),
                    topP = 0.7,
                    topK = 10,
                    candidateCount = 1,
                    stopSequences = new[] { "\n\n", "---" }
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
        }
    }
}