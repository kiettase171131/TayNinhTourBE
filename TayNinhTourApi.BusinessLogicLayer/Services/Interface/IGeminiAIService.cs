using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.DataAccessLayer.Enums;

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

        /// <summary>
        /// L?y tours có s?n slot theo ngày
        /// </summary>
        Task<List<AITourInfo>> GetAvailableToursByDateAsync(DateTime date, int maxResults = 10);
    }

    /// <summary>
    /// Interface cho AI Product Data Service
    /// </summary>
    public interface IAIProductDataService
    {
        /// <summary>
        /// L?y danh sách s?n ph?m có s?n
        /// </summary>
        Task<List<AIProductInfo>> GetAvailableProductsAsync(int maxResults = 10);

        /// <summary>
        /// Tìm ki?m s?n ph?m theo t? khóa
        /// </summary>
        Task<List<AIProductInfo>> SearchProductsAsync(string keyword, int maxResults = 10);

        /// <summary>
        /// L?y s?n ph?m theo category
        /// </summary>
        Task<List<AIProductInfo>> GetProductsByCategoryAsync(string category, int maxResults = 10);

        /// <summary>
        /// L?y s?n ph?m theo kho?ng giá
        /// </summary>
        Task<List<AIProductInfo>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, int maxResults = 10);

        /// <summary>
        /// L?y s?n ph?m ?ang gi?m giá
        /// </summary>
        Task<List<AIProductInfo>> GetProductsOnSaleAsync(int maxResults = 10);

        /// <summary>
        /// L?y s?n ph?m bán ch?y
        /// </summary>
        Task<List<AIProductInfo>> GetBestSellingProductsAsync(int maxResults = 10);
    }

    /// <summary>
    /// Interface cho Specialized AI Chat Service ?? x? lý t?ng lo?i chat
    /// </summary>
    public interface IAISpecializedChatService
    {
        /// <summary>
        /// X? lý tin nh?n theo lo?i chat c? th?
        /// </summary>
        Task<GeminiResponse> ProcessMessageAsync(string message, AIChatType chatType, List<GeminiMessage>? conversationHistory = null);

        /// <summary>
        /// T?o system prompt cho t?ng lo?i chat
        /// </summary>
        string GetSystemPrompt(AIChatType chatType);
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
        public bool IsFallback { get; set; } = false;
    }

    /// <summary>
    /// Model cho thông tin tour dành cho AI
    /// </summary>
    public class AITourInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string StartLocation { get; set; } = null!;
        public string EndLocation { get; set; } = null!;
        public string TourType { get; set; } = null!;
        public int MaxGuests { get; set; }
        public int AvailableSlots { get; set; }
        public List<string> Highlights { get; set; } = new();
        public List<DateTime> AvailableDates { get; set; } = new();
        public string CompanyName { get; set; } = null!;
        public bool IsPublic { get; set; }
    }

    /// <summary>
    /// Model cho thông tin s?n ph?m dành cho AI
    /// </summary>
    public class AIProductInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public int QuantityInStock { get; set; }
        public string Category { get; set; } = null!;
        public bool IsSale { get; set; }
        public int? SalePercent { get; set; }
        public int SoldCount { get; set; }
        public string ShopName { get; set; } = null!;
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}