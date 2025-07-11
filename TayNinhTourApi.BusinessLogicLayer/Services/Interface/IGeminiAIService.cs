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
    /// Interface cho AI Tour Data Service
    /// </summary>
    public interface IAITourDataService
    {
        /// <summary>
        /// L?y danh sách tours có s?n
        /// </summary>
        Task<List<AITourInfo>> GetAvailableToursAsync(int maxResults = 10);

        /// <summary>
        /// Tìm ki?m tours theo t? khóa
        /// </summary>
        Task<List<AITourInfo>> SearchToursAsync(string keyword, int maxResults = 10);

        /// <summary>
        /// L?y thông tin chi ti?t tour theo ID
        /// </summary>
        Task<AITourInfo?> GetTourDetailAsync(Guid tourId);

        /// <summary>
        /// L?y tours theo lo?i (FreeScenic, Paid, etc.)
        /// </summary>
        Task<List<AITourInfo>> GetToursByTypeAsync(string tourType, int maxResults = 10);

        /// <summary>
        /// L?y tours theo kho?ng giá
        /// </summary>
        Task<List<AITourInfo>> GetToursByPriceRangeAsync(decimal minPrice, decimal maxPrice, int maxResults = 10);
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
        
        /// <summary>
        /// ?ánh d?u response này là fallback content
        /// </summary>
        public bool IsFallback { get; set; } = false;
    }

    /// <summary>
    /// Model thông tin tour cho AI
    /// </summary>
    public class AITourInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string TourType { get; set; } = null!;
        public string StartLocation { get; set; } = null!;
        public string EndLocation { get; set; } = null!;
        public int MaxGuests { get; set; }
        public int AvailableSlots { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = null!;
        public List<string> SkillsRequired { get; set; } = new();
        public DateTime? NextAvailableDate { get; set; }
        public List<string> Highlights { get; set; } = new();
    }
}