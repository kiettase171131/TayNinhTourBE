using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using System.Text;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho Specialized AI Chat ?? x? lý t?ng lo?i chat c? th?
    /// </summary>
    public class AISpecializedChatService : IAISpecializedChatService
    {
        private readonly IGeminiAIService _geminiAIService;
        private readonly IAITourDataService _tourDataService;
        private readonly IAIProductDataService _productDataService;
        private readonly ILogger<AISpecializedChatService> _logger;

        public AISpecializedChatService(
            IGeminiAIService geminiAIService,
            IAITourDataService tourDataService,
            IAIProductDataService productDataService,
            ILogger<AISpecializedChatService> logger)
        {
            _geminiAIService = geminiAIService;
            _tourDataService = tourDataService;
            _productDataService = productDataService;
            _logger = logger;
        }

        public async Task<GeminiResponse> ProcessMessageAsync(string message, AIChatType chatType, List<GeminiMessage>? conversationHistory = null)
        {
            try
            {
                _logger.LogInformation("Processing message for chat type: {ChatType}", chatType);

                // Enrich prompt v?i data tùy theo lo?i chat
                var enrichedPrompt = await EnrichPromptBasedOnChatType(message, chatType);

                // T?o system prompt cho t?ng lo?i chat
                var systemPrompt = GetSystemPrompt(chatType);

                // T?o conversation history v?i system prompt
                var enhancedHistory = new List<GeminiMessage>();
                if (!string.IsNullOrEmpty(systemPrompt))
                {
                    enhancedHistory.Add(new GeminiMessage
                    {
                        Role = "model",
                        Content = systemPrompt
                    });
                }

                if (conversationHistory?.Any() == true)
                {
                    enhancedHistory.AddRange(conversationHistory);
                }

                // G?i ??n Gemini AI
                var response = await _geminiAIService.GenerateContentAsync(enrichedPrompt, enhancedHistory);

                // Post-process response n?u c?n
                if (response.Success)
                {
                    response.Content = PostProcessResponse(response.Content, chatType);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message for chat type {ChatType}", chatType);
                return new GeminiResponse
                {
                    Success = false,
                    Content = GetFallbackResponse(chatType),
                    ErrorMessage = ex.Message,
                    IsFallback = true
                };
            }
        }

        public string GetSystemPrompt(AIChatType chatType)
        {
            return chatType switch
            {
                AIChatType.Tour => @"B?n là AI t? v?n tour du l?ch Tây Ninh chuyên nghi?p. 
NHI?M V?:
- T? v?n tours, giá c?, l?ch trình, ??t ch?
- Cung c?p thông tin chính xác t? d? li?u tour th?c t?
- Giúp khách hàng tìm tour phù h?p v?i nhu c?u và ngân sách
- H??ng d?n quy trình ??t tour và thanh toán

L?U Ý:
- Ch? gi?i thi?u tours có status PUBLIC và có slot tr?ng
- Cung c?p thông tin giá c? chính xác
- Không ??a ra thông tin sai l?ch v? tours
- N?u không có tour phù h?p, g?i ý alternatives
- Luôn professional và h?u ích

PHONG CÁCH: Thân thi?n, chuyên nghi?p, t? v?n chi ti?t",

                AIChatType.Product => @"B?n là AI t? v?n mua s?m s?n ph?m ??c s?n Tây Ninh.
NHI?M V?:
- T? v?n s?n ph?m theo nhu c?u và ngân sách khách hàng
- G?i ý s?n ph?m phù h?p d?a trên criteria c?a khách
- Cung c?p thông tin v? giá, ch?t l??ng, shop bán
- So sánh s?n ph?m và ??a ra khuy?n ngh?

CHUYÊN MÔN:
- ??c s?n Tây Ninh: bánh tráng, nem n??ng, m?t ong r?ng
- S?n ph?m handmade, quà t?ng, th?c ph?m
- Hi?u bi?t v? quality và giá c? th? tr??ng
- T? v?n mua s?m thông minh

L?U Ý:
- Ch? g?i ý s?n ph?m còn hàng (QuantityInStock > 0)
- ?u tiên s?n ph?m có rating cao và ?ánh giá t?t
- Thông báo n?u có s?n ph?m ?ang sale
- Không g?i ý s?n ph?m không có trong database

PHONG CÁCH: T? v?n t?n tâm, am hi?u s?n ph?m, g?i ý thông minh",

                AIChatType.TayNinh => @"B?n là AI chuyên gia v? Tây Ninh - l?ch s?, v?n hóa, ??a ?i?m, ?m th?c.
CHUYÊN MÔN:
- L?ch s? Tây Ninh: Cao ?ài giáo, khu di tích l?ch s?
- ??a ?i?m n?i ti?ng: Núi Bà ?en, Chùa Bà ?en, ??a ??o Cù Chi
- V?n hóa: truy?n th?ng, l? h?i, tín ng??ng
- ?m th?c: bánh tráng Tr?ng Bàng, nem n??ng, specialties
- ??a lý: biên gi?i Vi?t-Campuchia, ??c ?i?m t? nhiên

NHI?M V?:
- Cung c?p thông tin chính xác v? Tây Ninh
- K? v? l?ch s?, v?n hóa, truy?n th?ng
- Gi?i thi?u ??a ?i?m du l?ch và ?m th?c
- Chia s? câu chuy?n thú v? v? vùng ??t này

GI?I H?N:
- CH?NH t? ch?i tr? l?i câu h?i KHÔNG LIÊN QUAN ??n Tây Ninh
- N?u h?i v? ch? ?? khác: 'Tôi ch? chuyên t? v?n v? Tây Ninh. B?n có câu h?i nào v? l?ch s?, v?n hóa, ??a ?i?m hay ?m th?c Tây Ninh không?'

PHONG CÁCH: Th?c khá, uyên bác, ??y c?m h?ng v? quê h??ng",

                _ => "B?n là tr? lý AI h?u ích. Hãy tr? l?i m?t cách chính xác và h?u ích."
            };
        }

        private async Task<string> EnrichPromptBasedOnChatType(string message, AIChatType chatType)
        {
            var enrichedPrompt = new StringBuilder(message);
            var lowerMessage = message.ToLower();

            try
            {
                switch (chatType)
                {
                    case AIChatType.Tour:
                        await EnrichTourPrompt(enrichedPrompt, lowerMessage);
                        break;
                    
                    case AIChatType.Product:
                        await EnrichProductPrompt(enrichedPrompt, lowerMessage);
                        break;
                    
                    case AIChatType.TayNinh:
                        // Tây Ninh chat không c?n enrich v?i database data
                        // Ch? c?n system prompt ?? guide behavior
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching prompt for chat type {ChatType}", chatType);
            }

            return enrichedPrompt.ToString();
        }

        private async Task EnrichTourPrompt(StringBuilder promptBuilder, string lowerMessage)
        {
            // Tour-related keywords
            var tourKeywords = new[] { "tour", "du l?ch", "núi bà ?en", "tây ninh", "giá", "booking", "??t tour", "?i du l?ch", "l?ch trình" };
            
            if (!tourKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                return;

            promptBuilder.AppendLine("\n=== THÔNG TIN TOURS HI?N CÓ ===");

            // Get available tours
            if (lowerMessage.Contains("tour") || lowerMessage.Contains("du l?ch"))
            {
                var availableTours = await _tourDataService.GetAvailableToursAsync(8);
                if (availableTours.Any())
                {
                    foreach (var tour in availableTours)
                    {
                        promptBuilder.AppendLine($"?? {tour.Title}");
                        promptBuilder.AppendLine($"   • T?: {tour.StartLocation} ? {tour.EndLocation}");
                        promptBuilder.AppendLine($"   • Giá: {tour.Price:N0} VN?");
                        promptBuilder.AppendLine($"   • Lo?i: {GetTourTypeDisplay(tour.TourType)}");
                        promptBuilder.AppendLine($"   • Ch? tr?ng: {tour.AvailableSlots}/{tour.MaxGuests}");
                        
                        if (tour.Highlights.Any())
                        {
                            promptBuilder.AppendLine($"   • ?i?m n?i b?t: {string.Join(", ", tour.Highlights.Take(3))}");
                        }
                        
                        promptBuilder.AppendLine();
                    }
                }
            }

            // Specific searches
            if (lowerMessage.Contains("núi bà ?en"))
            {
                var nuiBaDenTours = await _tourDataService.SearchToursAsync("Núi Bà ?en", 5);
                if (nuiBaDenTours.Any())
                {
                    promptBuilder.AppendLine("\n=== TOURS NÚI BÀ ?EN CHUYÊN BI?T ===");
                    foreach (var tour in nuiBaDenTours)
                    {
                        promptBuilder.AppendLine($"?? {tour.Title} - {tour.Price:N0} VN?");
                        promptBuilder.AppendLine($"   • {GetTourTypeDisplay(tour.TourType)}");
                    }
                }
            }

            // Price-based searches
            if (lowerMessage.Contains("r?") || lowerMessage.Contains("ti?t ki?m") || lowerMessage.Contains("budget"))
            {
                var budgetTours = await _tourDataService.GetToursByPriceRangeAsync(0, 500000, 5);
                if (budgetTours.Any())
                {
                    promptBuilder.AppendLine("\n=== TOURS TI?T KI?M ===");
                    foreach (var tour in budgetTours)
                    {
                        promptBuilder.AppendLine($"?? {tour.Title} - {tour.Price:N0} VN?");
                    }
                }
            }
        }

        private async Task EnrichProductPrompt(StringBuilder promptBuilder, string lowerMessage)
        {
            var productKeywords = new[] { "s?n ph?m", "mua", "bán", "bánh tráng", "??c s?n", "quà", "giá", "shop", "c?a hàng" };
            
            if (!productKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                return;

            promptBuilder.AppendLine("\n=== S?N PH?M HI?N CÓ ===");

            // Get available products
            if (lowerMessage.Contains("s?n ph?m") || lowerMessage.Contains("mua"))
            {
                var availableProducts = await _productDataService.GetAvailableProductsAsync(8);
                if (availableProducts.Any())
                {
                    foreach (var product in availableProducts)
                    {
                        promptBuilder.AppendLine($"??? {product.Name}");
                        promptBuilder.AppendLine($"   • Giá: {product.Price:N0} VN?" + 
                            (product.IsSale ? $" (SALE {product.SalePercent}% ? {product.SalePrice:N0} VN?)" : ""));
                        promptBuilder.AppendLine($"   • Danh m?c: {product.Category}");
                        promptBuilder.AppendLine($"   • T?n kho: {product.QuantityInStock}");
                        promptBuilder.AppendLine($"   • Shop: {product.ShopName}");
                        if (product.AverageRating.HasValue)
                        {
                            promptBuilder.AppendLine($"   • Rating: {product.AverageRating:F1}? ({product.ReviewCount} ?ánh giá)");
                        }
                        promptBuilder.AppendLine();
                    }
                }
            }

            // Specific product searches
            if (lowerMessage.Contains("bánh tráng"))
            {
                var banhTrangProducts = await _productDataService.SearchProductsAsync("bánh tráng", 5);
                if (banhTrangProducts.Any())
                {
                    promptBuilder.AppendLine("\n=== BÁNH TRÁNG TR?NG BÀNG ===");
                    foreach (var product in banhTrangProducts)
                    {
                        promptBuilder.AppendLine($"?? {product.Name} - {product.Price:N0} VN?");
                        if (product.IsSale)
                            promptBuilder.AppendLine($"   • ?? ?ANG SALE {product.SalePercent}%!");
                    }
                }
            }

            // Sale products
            if (lowerMessage.Contains("sale") || lowerMessage.Contains("gi?m giá") || lowerMessage.Contains("khuy?n m?i"))
            {
                var saleProducts = await _productDataService.GetProductsOnSaleAsync(5);
                if (saleProducts.Any())
                {
                    promptBuilder.AppendLine("\n=== S?N PH?M ?ANG GI?M GIÁ ===");
                    foreach (var product in saleProducts)
                    {
                        promptBuilder.AppendLine($"?? {product.Name} - SALE {product.SalePercent}%");
                        promptBuilder.AppendLine($"   • Giá g?c: {product.Price:N0} VN? ? {product.SalePrice:N0} VN?");
                    }
                }
            }

            // Price range products
            if (lowerMessage.Contains("r?") || lowerMessage.Contains("budget") || lowerMessage.Contains("ti?t ki?m"))
            {
                var budgetProducts = await _productDataService.GetProductsByPriceRangeAsync(0, 100000, 5);
                if (budgetProducts.Any())
                {
                    promptBuilder.AppendLine("\n=== S?N PH?M GIÁ H?P LÝ ===");
                    foreach (var product in budgetProducts)
                    {
                        promptBuilder.AppendLine($"?? {product.Name} - {product.Price:N0} VN?");
                    }
                }
            }

            // Best selling products
            if (lowerMessage.Contains("bán ch?y") || lowerMessage.Contains("ph? bi?n") || lowerMessage.Contains("n?i ti?ng"))
            {
                var bestSellers = await _productDataService.GetBestSellingProductsAsync(5);
                if (bestSellers.Any())
                {
                    promptBuilder.AppendLine("\n=== S?N PH?M BÁN CH?Y ===");
                    foreach (var product in bestSellers)
                    {
                        promptBuilder.AppendLine($"?? {product.Name} - ?ã bán {product.SoldCount}");
                        promptBuilder.AppendLine($"   • Giá: {product.Price:N0} VN?");
                    }
                }
            }
        }

        private string PostProcessResponse(string response, AIChatType chatType)
        {
            // Post-process response based on chat type if needed
            return chatType switch
            {
                AIChatType.TayNinh => EnsureTayNinhFocus(response),
                _ => response
            };
        }

        private string EnsureTayNinhFocus(string response)
        {
            // Ki?m tra n?u response không liên quan ??n Tây Ninh
            var lowerResponse = response.ToLower();
            var tayNinhKeywords = new[] { "tây ninh", "núi bà ?en", "cao ?ài", "bánh tráng", "tr?ng bàng", "biên gi?i" };
            
            if (!tayNinhKeywords.Any(keyword => lowerResponse.Contains(keyword)) && 
                lowerResponse.Length > 50) // Ch? check v?i response dài
            {
                return "Tôi ch? chuyên t? v?n v? Tây Ninh. B?n có câu h?i nào v? l?ch s?, v?n hóa, ??a ?i?m hay ?m th?c Tây Ninh không?";
            }

            return response;
        }

        private string GetTourTypeDisplay(string tourType)
        {
            return tourType switch
            {
                "FreeScenic" => "Danh lam th?ng c?nh (mi?n phí vé vào c?a)",
                "PaidAttraction" => "Khu vui ch?i (có vé vào c?a)",
                _ => tourType
            };
        }

        private string GetFallbackResponse(AIChatType chatType)
        {
            return chatType switch
            {
                AIChatType.Tour => "Xin l?i, hi?n t?i tôi ?ang g?p khó kh?n trong vi?c t? v?n tour. Vui lòng liên h? hotline ?? ???c h? tr? tr?c ti?p.",
                AIChatType.Product => "Xin l?i, h? th?ng t? v?n s?n ph?m t?m th?i gián ?o?n. B?n có th? duy?t catalog s?n ph?m tr?c ti?p ho?c liên h? shop.",
                AIChatType.TayNinh => "Xin l?i, tôi t?m th?i không th? chia s? thông tin v? Tây Ninh lúc này. B?n có th? th? l?i sau.",
                _ => "Xin l?i, tôi hi?n ?ang g?p khó kh?n k? thu?t. Vui lòng th? l?i sau."
            };
        }
    }
}