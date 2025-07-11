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
    /// Service ?? tích h?p v?i Gemini AI API
    /// </summary>
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _geminiSettings;
        private readonly ILogger<GeminiAIService> _logger;

        public GeminiAIService(
            HttpClient httpClient,
            IOptions<GeminiSettings> geminiSettings,
            ILogger<GeminiAIService> logger)
        {
            _httpClient = httpClient;
            _geminiSettings = geminiSettings.Value;
            _logger = logger;
        }

        public async Task<GeminiResponse> GenerateContentAsync(string prompt, List<GeminiMessage>? conversationHistory = null)
        {
            var stopwatch = Stopwatch.StartNew();
            const int maxRetries = 3;
            const int baseDelayMs = 1000;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Gemini API attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

                    // T?o request payload v?i c?u hình t?i ?u
                    var requestPayload = CreateRequestPayload(prompt, conversationHistory);
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
                            
                            return CreateFallbackResponse(prompt, stopwatch);
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

                        // N?u là 503 (overload), fail nhanh và dùng fallback
                        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            _logger.LogWarning("Model overloaded (503), using fallback immediately");
                            return CreateFallbackResponse(prompt, stopwatch, "Gemini API is overloaded");
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
                        return CreateFallbackResponse(prompt, stopwatch, $"API request failed: {response.StatusCode}");
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Attempt {Attempt}: Request timeout, using fallback", attempt);
                    return CreateFallbackResponse(prompt, stopwatch, "Request timeout");
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
                    return CreateFallbackResponse(prompt, stopwatch, ex.Message);
                }
            }

            // N?u h?t retry attempts, tr? v? fallback response
            return CreateFallbackResponse(prompt, stopwatch, $"Gemini API failed after {maxRetries} attempts");
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

        private GeminiResponse CreateFallbackResponse(string prompt, Stopwatch stopwatch, string? errorMessage = null)
        {
            stopwatch.Stop();
            
            var fallbackContent = GenerateFallbackContent(prompt);
            
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

        private string GenerateFallbackContent(string prompt)
        {
            var lowerPrompt = prompt.ToLower();
            
            if (lowerPrompt.Contains("tây ninh") || lowerPrompt.Contains("tour") || lowerPrompt.Contains("du l?ch"))
            {
                return "Tây Ninh là ?i?m ??n du l?ch tâm linh n?i ti?ng v?i Núi Bà ?en và di tích l?ch s?. B?n mu?n bi?t v? tour nào?";
            }
            
            if (lowerPrompt.Contains("núi bà ?en") || lowerPrompt.Contains("núi bà"))
            {
                return "Núi Bà ?en cao nh?t Nam B?, có cáp treo và chùa linh thiêng. ?i?m ??n h?p d?n c?a Tây Ninh.";
            }
            
            if (lowerPrompt.Contains("chào") || lowerPrompt.Contains("xin chào") || lowerPrompt.Contains("hello"))
            {
                return "Xin chào! Tôi là tr? lý AI du l?ch Tây Ninh. Tôi có th? giúp gì cho b?n?";
            }
            
            if (lowerPrompt.Contains("giá") || lowerPrompt.Contains("chi phí"))
            {
                return "Giá tour tùy theo lo?i hình và th?i gian. Vui lòng liên h? ?? ???c t? v?n chi ti?t.";
            }
            
            if (lowerPrompt.Contains("?n") || lowerPrompt.Contains("món"))
            {
                return "Tây Ninh có bánh tráng n??ng, bánh canh cua ??ng, cà ri dê Ninh S?n. B?n mu?n bi?t thêm?";
            }
            
            return "H? th?ng AI t?m b?n. Tôi s?n sàng h? tr? v? du l?ch Tây Ninh!";
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