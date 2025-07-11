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

                    // T?o request payload
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
                        // Ch? log request payload ? development
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("Request payload: {Payload}", jsonPayload);
                        }
                    }

                    // G?i request
                    var response = await _httpClient.PostAsync(url, content);
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
                            
                            stopwatch.Stop();
                            return new GeminiResponse
                            {
                                Success = false,
                                Content = string.Empty,
                                ErrorMessage = "API tr? v? response body r?ng sau nhi?u l?n th?",
                                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                            };
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

                        // Retry on server errors (5xx) or rate limiting (429) or overloaded (503)
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
                        stopwatch.Stop();
                        return new GeminiResponse
                        {
                            Success = false,
                            Content = string.Empty,
                            ErrorMessage = $"API request failed: {response.StatusCode} - {responseContent}",
                            ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                        };
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
                    stopwatch.Stop();
                    _logger.LogError(ex, "Final attempt failed: Error calling Gemini API");
                    
                    return new GeminiResponse
                    {
                        Success = false,
                        Content = string.Empty,
                        ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                        ErrorMessage = ex.Message
                    };
                }
            }

            stopwatch.Stop();
            return new GeminiResponse
            {
                Success = false,
                Content = string.Empty,
                ErrorMessage = $"Gemini API failed after {maxRetries} attempts",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }

        public async Task<string> GenerateTitleAsync(string firstMessage)
        {
            try
            {
                var titlePrompt = $"Hãy t?o m?t tiêu ?? ng?n g?n (t?i ?a 50 ký t?) cho cu?c trò chuy?n b?t ??u b?ng tin nh?n: \"{firstMessage}\". Ch? tr? v? tiêu ??, không c?n gi?i thích thêm.";
                
                var response = await GenerateContentAsync(titlePrompt);
                
                if (response.Success && !string.IsNullOrWhiteSpace(response.Content))
                {
                    // Làm s?ch và c?t ng?n tiêu ??
                    var title = response.Content.Trim().Replace("\"", "");
                    if (title.Length > 50)
                    {
                        title = title.Substring(0, 47) + "...";
                    }
                    return title;
                }
                else
                {
                    // Fallback: t?o tiêu ?? t? tin nh?n ??u tiên
                    var fallbackTitle = firstMessage.Length > 30 
                        ? firstMessage.Substring(0, 27) + "..." 
                        : firstMessage;
                    return fallbackTitle;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating title");
                
                // Fallback: t?o tiêu ?? t? tin nh?n ??u tiên
                var fallbackTitle = firstMessage.Length > 30 
                    ? firstMessage.Substring(0, 27) + "..." 
                    : firstMessage;
                return fallbackTitle;
            }
        }

        private object CreateRequestPayload(string prompt, List<GeminiMessage>? conversationHistory)
        {
            var contents = new List<object>();

            // Thêm system message
            if (!string.IsNullOrEmpty(_geminiSettings.SystemPrompt))
            {
                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = _geminiSettings.SystemPrompt } }
                });
            }

            // Thêm conversation history n?u có (gi?i h?n context)
            if (conversationHistory?.Any() == true)
            {
                // Ch? l?y 3 tin nh?n g?n nh?t ?? gi?m context
                var recentHistory = conversationHistory.TakeLast(3);
                foreach (var message in recentHistory)
                {
                    contents.Add(new
                    {
                        role = message.Role == "AI" ? "model" : "user",
                        parts = new[] { new { text = message.Content } }
                    });
                }
            }

            // Thêm prompt hi?n t?i
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
                    temperature = _geminiSettings.Temperature,
                    maxOutputTokens = _geminiSettings.MaxTokens,
                    topP = 0.9, // T?ng topP ?? ?u tiên câu tr? l?i chính xác h?n
                    topK = 20,  // T?ng topK ?? có nhi?u l?a ch?n t? h?n
                    candidateCount = 1, // Ch? t?o 1 candidate ?? nhanh h?n
                    stopSequences = new[] { "\n\n\n", "---" } // D?ng khi g?p d?u hi?u k?t thúc
                }
            };
        }

        private int EstimateTokens(string text)
        {
            // ??c tính ??n gi?n: 1 token ? 4 ký t? cho ti?ng Vi?t
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        // Classes cho deserialize Gemini API response
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