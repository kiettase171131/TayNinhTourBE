using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TayNinhTourApi.BusinessLogicLayer.Common;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Interface cho Gemini AI Service
    /// </summary>
    public interface IGeminiAIService
    {
        /// <summary>
        /// G?i tin nh?n ??n Gemini AI và nh?n ph?n h?i
        /// </summary>
        Task<GeminiResponse> GenerateContentAsync(string prompt, List<GeminiMessage>? conversationHistory = null);

        /// <summary>
        /// T?o title cho chat session t? tin nh?n ??u tiên
        /// </summary>
        Task<string> GenerateTitleAsync(string firstMessage);
    }

    /// <summary>
    /// Model cho tin nh?n trong conversation v?i Gemini
    /// </summary>
    public class GeminiMessage
    {
        public string Role { get; set; } = null!; // "user" or "model"
        public string Content { get; set; } = null!;
    }

    /// <summary>
    /// Model cho response t? Gemini API
    /// </summary>
    public class GeminiResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; } = null!;
        public int TokensUsed { get; set; }
        public int ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }
}