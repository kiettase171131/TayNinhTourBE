using System.Diagnostics;
using System.Text;
using System.Text.Json;
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
            
            try
            {
                // T?o request payload
                var requestPayload = CreateRequestPayload(prompt, conversationHistory);
                var jsonPayload = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // T?o URL v?i API key
                var url = $"{_geminiSettings.ApiUrl}?key={_geminiSettings.ApiKey}";

                _logger.LogInformation("Sending request to Gemini API");

                // G?i request
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    var geminiApiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent);
                    
                    if (geminiApiResponse?.Candidates?.Any() == true)
                    {
                        var generatedText = geminiApiResponse.Candidates[0].Content.Parts[0].Text;
                        var tokensUsed = EstimateTokens(prompt + generatedText);

                        _logger.LogInformation("Gemini API request successful. Tokens: {Tokens}, Time: {Time}ms", 
                            tokensUsed, stopwatch.ElapsedMilliseconds);

                        return new GeminiResponse
                        {
                            Success = true,
                            Content = generatedText,
                            TokensUsed = tokensUsed,
                            ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Gemini API returned empty response");
                        return new GeminiResponse
                        {
                            Success = false,
                            Content = string.Empty,
                            ErrorMessage = "API tr? v? ph?n h?i r?ng"
                        };
                    }
                }
                else
                {
                    _logger.LogError("Gemini API request failed. Status: {Status}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    
                    return new GeminiResponse
                    {
                        Success = false,
                        Content = string.Empty,
                        ErrorMessage = $"API request failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error calling Gemini API");
                
                return new GeminiResponse
                {
                    Success = false,
                    Content = string.Empty,
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    ErrorMessage = ex.Message
                };
            }
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

            // Thêm conversation history n?u có
            if (conversationHistory?.Any() == true)
            {
                foreach (var message in conversationHistory)
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
                    topP = 0.8,
                    topK = 10
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
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            public Content Content { get; set; } = null!;
        }

        private class Content
        {
            public List<Part> Parts { get; set; } = null!;
        }

        private class Part
        {
            public string Text { get; set; } = null!;
        }
    }
}