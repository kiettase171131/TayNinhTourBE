using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using System.Text;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho Specialized AI Chat ?? x? l� t?ng lo?i chat c? th?
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

                // Enrich prompt v?i data t�y theo lo?i chat
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
                AIChatType.Tour => @"B?n l� AI t? v?n tour du l?ch T�y Ninh chuy�n nghi?p. 
NHI?M V?:
- T? v?n tours, gi� c?, l?ch tr�nh, ??t ch?
- Cung c?p th�ng tin ch�nh x�c t? d? li?u tour th?c t?
- Gi�p kh�ch h�ng t�m tour ph� h?p v?i nhu c?u v� ng�n s�ch
- H??ng d?n quy tr�nh ??t tour v� thanh to�n

L?U �:
- Ch? gi?i thi?u tours c� status PUBLIC v� c� slot tr?ng
- Cung c?p th�ng tin gi� c? ch�nh x�c
- Kh�ng ??a ra th�ng tin sai l?ch v? tours
- N?u kh�ng c� tour ph� h?p, g?i � alternatives
- Lu�n professional v� h?u �ch

PHONG C�CH: Th�n thi?n, chuy�n nghi?p, t? v?n chi ti?t",

                AIChatType.Product => @"B?n l� AI t? v?n mua s?m s?n ph?m ??c s?n T�y Ninh.
NHI?M V?:
- T? v?n s?n ph?m theo nhu c?u v� ng�n s�ch kh�ch h�ng
- G?i � s?n ph?m ph� h?p d?a tr�n criteria c?a kh�ch
- Cung c?p th�ng tin v? gi�, ch?t l??ng, shop b�n
- So s�nh s?n ph?m v� ??a ra khuy?n ngh?

CHUY�N M�N:
- ??c s?n T�y Ninh: b�nh tr�ng, nem n??ng, m?t ong r?ng
- S?n ph?m handmade, qu� t?ng, th?c ph?m
- Hi?u bi?t v? quality v� gi� c? th? tr??ng
- T? v?n mua s?m th�ng minh

L?U �:
- Ch? g?i � s?n ph?m c�n h�ng (QuantityInStock > 0)
- ?u ti�n s?n ph?m c� rating cao v� ?�nh gi� t?t
- Th�ng b�o n?u c� s?n ph?m ?ang sale
- Kh�ng g?i � s?n ph?m kh�ng c� trong database

PHONG C�CH: T? v?n t?n t�m, am hi?u s?n ph?m, g?i � th�ng minh",

                AIChatType.TayNinh => @"B?n l� AI chuy�n gia v? T�y Ninh - l?ch s?, v?n h�a, ??a ?i?m, ?m th?c.
CHUY�N M�N:
- L?ch s? T�y Ninh: Cao ?�i gi�o, khu di t�ch l?ch s?
- ??a ?i?m n?i ti?ng: N�i B� ?en, Ch�a B� ?en, ??a ??o C� Chi
- V?n h�a: truy?n th?ng, l? h?i, t�n ng??ng
- ?m th?c: b�nh tr�ng Tr?ng B�ng, nem n??ng, specialties
- ??a l�: bi�n gi?i Vi?t-Campuchia, ??c ?i?m t? nhi�n

NHI?M V?:
- Cung c?p th�ng tin ch�nh x�c v? T�y Ninh
- K? v? l?ch s?, v?n h�a, truy?n th?ng
- Gi?i thi?u ??a ?i?m du l?ch v� ?m th?c
- Chia s? c�u chuy?n th� v? v? v�ng ??t n�y

GI?I H?N:
- CH?NH t? ch?i tr? l?i c�u h?i KH�NG LI�N QUAN ??n T�y Ninh
- N?u h?i v? ch? ?? kh�c: 'T�i ch? chuy�n t? v?n v? T�y Ninh. B?n c� c�u h?i n�o v? l?ch s?, v?n h�a, ??a ?i?m hay ?m th?c T�y Ninh kh�ng?'

PHONG C�CH: Th?c kh�, uy�n b�c, ??y c?m h?ng v? qu� h??ng",

                _ => "B?n l� tr? l� AI h?u �ch. H�y tr? l?i m?t c�ch ch�nh x�c v� h?u �ch."
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
                        // T�y Ninh chat kh�ng c?n enrich v?i database data
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
            var tourKeywords = new[] { "tour", "du l?ch", "n�i b� ?en", "t�y ninh", "gi�", "booking", "??t tour", "?i du l?ch", "l?ch tr�nh" };
            
            if (!tourKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                return;

            promptBuilder.AppendLine("\n=== TH�NG TIN TOURS HI?N C� ===");

            // Get available tours
            if (lowerMessage.Contains("tour") || lowerMessage.Contains("du l?ch"))
            {
                var availableTours = await _tourDataService.GetAvailableToursAsync(8);
                if (availableTours.Any())
                {
                    foreach (var tour in availableTours)
                    {
                        promptBuilder.AppendLine($"?? {tour.Title}");
                        promptBuilder.AppendLine($"   � T?: {tour.StartLocation} ? {tour.EndLocation}");
                        promptBuilder.AppendLine($"   � Gi�: {tour.Price:N0} VN?");
                        promptBuilder.AppendLine($"   � Lo?i: {GetTourTypeDisplay(tour.TourType)}");
                        promptBuilder.AppendLine($"   � Ch? tr?ng: {tour.AvailableSlots}/{tour.MaxGuests}");
                        
                        if (tour.Highlights.Any())
                        {
                            promptBuilder.AppendLine($"   � ?i?m n?i b?t: {string.Join(", ", tour.Highlights.Take(3))}");
                        }
                        
                        promptBuilder.AppendLine();
                    }
                }
            }

            // Specific searches
            if (lowerMessage.Contains("n�i b� ?en"))
            {
                var nuiBaDenTours = await _tourDataService.SearchToursAsync("N�i B� ?en", 5);
                if (nuiBaDenTours.Any())
                {
                    promptBuilder.AppendLine("\n=== TOURS N�I B� ?EN CHUY�N BI?T ===");
                    foreach (var tour in nuiBaDenTours)
                    {
                        promptBuilder.AppendLine($"?? {tour.Title} - {tour.Price:N0} VN?");
                        promptBuilder.AppendLine($"   � {GetTourTypeDisplay(tour.TourType)}");
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
            var productKeywords = new[] { "s?n ph?m", "mua", "b�n", "b�nh tr�ng", "??c s?n", "qu�", "gi�", "shop", "c?a h�ng" };
            
            if (!productKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                return;

            promptBuilder.AppendLine("\n=== S?N PH?M HI?N C� ===");

            // Get available products
            if (lowerMessage.Contains("s?n ph?m") || lowerMessage.Contains("mua"))
            {
                var availableProducts = await _productDataService.GetAvailableProductsAsync(8);
                if (availableProducts.Any())
                {
                    foreach (var product in availableProducts)
                    {
                        promptBuilder.AppendLine($"??? {product.Name}");
                        promptBuilder.AppendLine($"   � Gi�: {product.Price:N0} VN?" + 
                            (product.IsSale ? $" (SALE {product.SalePercent}% ? {product.SalePrice:N0} VN?)" : ""));
                        promptBuilder.AppendLine($"   � Danh m?c: {product.Category}");
                        promptBuilder.AppendLine($"   � T?n kho: {product.QuantityInStock}");
                        promptBuilder.AppendLine($"   � Shop: {product.ShopName}");
                        if (product.AverageRating.HasValue)
                        {
                            promptBuilder.AppendLine($"   � Rating: {product.AverageRating:F1}? ({product.ReviewCount} ?�nh gi�)");
                        }
                        promptBuilder.AppendLine();
                    }
                }
            }

            // Specific product searches
            if (lowerMessage.Contains("b�nh tr�ng"))
            {
                var banhTrangProducts = await _productDataService.SearchProductsAsync("b�nh tr�ng", 5);
                if (banhTrangProducts.Any())
                {
                    promptBuilder.AppendLine("\n=== B�NH TR�NG TR?NG B�NG ===");
                    foreach (var product in banhTrangProducts)
                    {
                        promptBuilder.AppendLine($"?? {product.Name} - {product.Price:N0} VN?");
                        if (product.IsSale)
                            promptBuilder.AppendLine($"   � ?? ?ANG SALE {product.SalePercent}%!");
                    }
                }
            }

            // Sale products
            if (lowerMessage.Contains("sale") || lowerMessage.Contains("gi?m gi�") || lowerMessage.Contains("khuy?n m?i"))
            {
                var saleProducts = await _productDataService.GetProductsOnSaleAsync(5);
                if (saleProducts.Any())
                {
                    promptBuilder.AppendLine("\n=== S?N PH?M ?ANG GI?M GI� ===");
                    foreach (var product in saleProducts)
                    {
                        promptBuilder.AppendLine($"?? {product.Name} - SALE {product.SalePercent}%");
                        promptBuilder.AppendLine($"   � Gi� g?c: {product.Price:N0} VN? ? {product.SalePrice:N0} VN?");
                    }
                }
            }

            // Price range products
            if (lowerMessage.Contains("r?") || lowerMessage.Contains("budget") || lowerMessage.Contains("ti?t ki?m"))
            {
                var budgetProducts = await _productDataService.GetProductsByPriceRangeAsync(0, 100000, 5);
                if (budgetProducts.Any())
                {
                    promptBuilder.AppendLine("\n=== S?N PH?M GI� H?P L� ===");
                    foreach (var product in budgetProducts)
                    {
                        promptBuilder.AppendLine($"?? {product.Name} - {product.Price:N0} VN?");
                    }
                }
            }

            // Best selling products
            if (lowerMessage.Contains("b�n ch?y") || lowerMessage.Contains("ph? bi?n") || lowerMessage.Contains("n?i ti?ng"))
            {
                var bestSellers = await _productDataService.GetBestSellingProductsAsync(5);
                if (bestSellers.Any())
                {
                    promptBuilder.AppendLine("\n=== S?N PH?M B�N CH?Y ===");
                    foreach (var product in bestSellers)
                    {
                        promptBuilder.AppendLine($"?? {product.Name} - ?� b�n {product.SoldCount}");
                        promptBuilder.AppendLine($"   � Gi�: {product.Price:N0} VN?");
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
            // Ki?m tra n?u response kh�ng li�n quan ??n T�y Ninh
            var lowerResponse = response.ToLower();
            var tayNinhKeywords = new[] { "t�y ninh", "n�i b� ?en", "cao ?�i", "b�nh tr�ng", "tr?ng b�ng", "bi�n gi?i" };
            
            if (!tayNinhKeywords.Any(keyword => lowerResponse.Contains(keyword)) && 
                lowerResponse.Length > 50) // Ch? check v?i response d�i
            {
                return "T�i ch? chuy�n t? v?n v? T�y Ninh. B?n c� c�u h?i n�o v? l?ch s?, v?n h�a, ??a ?i?m hay ?m th?c T�y Ninh kh�ng?";
            }

            return response;
        }

        private string GetTourTypeDisplay(string tourType)
        {
            return tourType switch
            {
                "FreeScenic" => "Danh lam th?ng c?nh (mi?n ph� v� v�o c?a)",
                "PaidAttraction" => "Khu vui ch?i (c� v� v�o c?a)",
                _ => tourType
            };
        }

        private string GetFallbackResponse(AIChatType chatType)
        {
            return chatType switch
            {
                AIChatType.Tour => "Xin l?i, hi?n t?i t�i ?ang g?p kh� kh?n trong vi?c t? v?n tour. Vui l�ng li�n h? hotline ?? ???c h? tr? tr?c ti?p.",
                AIChatType.Product => "Xin l?i, h? th?ng t? v?n s?n ph?m t?m th?i gi�n ?o?n. B?n c� th? duy?t catalog s?n ph?m tr?c ti?p ho?c li�n h? shop.",
                AIChatType.TayNinh => "Xin l?i, t�i t?m th?i kh�ng th? chia s? th�ng tin v? T�y Ninh l�c n�y. B?n c� th? th? l?i sau.",
                _ => "Xin l?i, t�i hi?n ?ang g?p kh� kh?n k? thu?t. Vui l�ng th? l?i sau."
            };
        }
    }
}