using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using System.Text;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho AI Specialized Chat - xử lý từng loại chat cụ thể với topic validation
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
                _logger.LogInformation("Processing {ChatType} message: {Message}", chatType, message);

                // BƯỚC 1: Kiểm tra topic mismatch trước khi xử lý
                var topicValidation = ValidateTopicAlignment(message, chatType);
                if (!topicValidation.IsValidTopic)
                {
                    return new GeminiResponse
                    {
                        Success = true,
                        Content = topicValidation.RedirectMessage,
                        TokensUsed = 0,
                        ResponseTimeMs = 100,
                        IsFallback = true,
                        RequiresTopicRedirect = true,
                        SuggestedChatType = topicValidation.SuggestedChatType
                    };
                }

                // BƯỚC 2: Xử lý message theo chatType nếu topic phù hợp
                var systemPrompt = GetSystemPrompt(chatType);
                var enrichedPrompt = await EnrichPromptWithData(message, chatType);

                var response = await _geminiAIService.GenerateContentAsync(
                    enrichedPrompt,
                    systemPrompt,
                    conversationHistory);

                if (response.Success)
                {
                    // BƯỚC 3: Post-process để đảm bảo phản hồi đúng scope
                    response.Content = PostProcessResponse(response.Content, chatType);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {ChatType} message", chatType);

                return new GeminiResponse
                {
                    Success = false,
                    Content = GetFallbackResponse(chatType),
                    ErrorMessage = ex.Message,
                    IsFallback = true
                };
            }
        }

        /// <summary>
        /// Kiểm tra topic alignment và đưa ra gợi ý redirect nếu cần
        /// </summary>
        private TopicValidationResult ValidateTopicAlignment(string message, AIChatType currentChatType)
        {
            var lowerMessage = message.ToLower();
            var result = new TopicValidationResult { IsValidTopic = true };

            // Định nghĩa keywords cho từng ChatType
            var tourKeywords = new[] { "tour", "du lịch", "travel", "núi bà đen", "chùa", "tham quan", "lịch trình", "giá tour", "booking", "đặt tour", "hướng dẫn viên", "guide" };
            var productKeywords = new[] { "sản phẩm", "mua", "bán", "shop", "giá", "đặc sản", "bánh tráng", "mắm", "gốm sứ", "quà", "shopping", "cart", "đặt hàng", "thanh toán", "ship" };
            var tayNinhKeywords = new[] { "tây ninh", "lịch sử", "văn hóa", "cao đài", "núi bà đen", "trảng bàng", "biên giới", "chiến tranh", "địa lý", "dân tộc", "truyền thống" };

            // Kiểm tra từng ChatType hiện tại
            switch (currentChatType)
            {
                case AIChatType.Tour:
                    // Nếu đang trong Tour chat nhưng hỏi về Product
                    if (productKeywords.Any(keyword => lowerMessage.Contains(keyword)) &&
                        !tourKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                    {
                        if (IsStrongProductIntent(lowerMessage))
                        {
                            result.IsValidTopic = false;
                            result.SuggestedChatType = AIChatType.Product;
                            result.RedirectMessage = $"🛍️ **Tôi nhận thấy bạn muốn hỏi về sản phẩm!**\n\n" +
                                $"Hiện tại chúng ta đang trong phiên chat **Tư vấn Tour**, nhưng câu hỏi của bạn liên quan đến **mua sắm sản phẩm**.\n\n" +
                                $"💡 **Gợi ý:** Để được tư vấn tốt nhất về sản phẩm đặc sản Tây Ninh, bạn nên:\n" +
                                $"1. Tạo phiên chat mới với loại **\"Product Chat\"**\n" +
                                $"2. Hoặc hỏi tôi về **tours du lịch** trong phiên này\n\n" +
                                $"🎯 **Trong phiên tour này, tôi có thể giúp bạn:**\n" +
                                $"• Tư vấn tour Núi Bà Đen, chùa Cao Đài\n" +
                                $"• Thông tin giá tour và lịch trình\n" +
                                $"• Đặt tour và hướng dẫn viên\n\n" +
                                $"Bạn muốn tiếp tục hỏi về tour hay chuyển sang tư vấn sản phẩm?";
                        }
                    }
                    // Nếu hỏi về thông tin Tây Ninh chung (không liên quan tour)
                    else if (tayNinhKeywords.Any(keyword => lowerMessage.Contains(keyword)) &&
                             IsGeneralTayNinhQuestion(lowerMessage) &&
                             !tourKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                    {
                        result.IsValidTopic = false;
                        result.SuggestedChatType = AIChatType.TayNinh;
                        result.RedirectMessage = $"🏛️ **Tôi thấy bạn quan tâm đến thông tin về Tây Ninh!**\n\n" +
                            $"Hiện tại chúng ta đang trong phiên chat **Tư vấn Tour**, nhưng câu hỏi của bạn về **lịch sử/văn hóa** Tây Ninh.\n\n" +
                            $"💡 **Gợi ý:** Để biết thông tin chi tiết về Tây Ninh, bạn nên tạo phiên **\"TayNinh Chat\"** mới.\n\n" +
                            $"🎯 **Hoặc trong phiên tour này, tôi có thể tư vấn:**\n" +
                            $"• Tour tham quan các địa điểm lịch sử Tây Ninh\n" +
                            $"• Lịch trình kết hợp thăm chùa và di tích\n" +
                            $"• Giá tour và dịch vụ hướng dẫn\n\n" +
                            $"Bạn muốn biết về tour tham quan hay chuyển sang hỏi thông tin Tây Ninh?";
                    }
                    break;

                case AIChatType.Product:
                    // Nếu đang trong Product chat nhưng hỏi về Tour
                    if (tourKeywords.Any(keyword => lowerMessage.Contains(keyword)) &&
                        !productKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                    {
                        if (IsStrongTourIntent(lowerMessage))
                        {
                            result.IsValidTopic = false;
                            result.SuggestedChatType = AIChatType.Tour;
                            result.RedirectMessage = $"🚌 **Tôi thấy bạn muốn hỏi về tour du lịch!**\n\n" +
                                $"Hiện tại chúng ta đang trong phiên chat **Tư vấn Sản phẩm**, nhưng câu hỏi của bạn liên quan đến **tour du lịch**.\n\n" +
                                $"💡 **Gợi ý:** Để được tư vấn tour tốt nhất, bạn nên tạo phiên **\"Tour Chat\"** mới.\n\n" +
                                $"🎯 **Trong phiên sản phẩm này, tôi có thể giúp bạn:**\n" +
                                $"• Tư vấn đặc sản Tây Ninh làm quà\n" +
                                $"• So sánh giá và chất lượng sản phẩm\n" +
                                $"• Hướng dẫn đặt hàng và thanh toán\n\n" +
                                $"Bạn muốn tiếp tục mua sắm hay chuyển sang tư vấn tour?";
                        }
                    }
                    break;

                case AIChatType.TayNinh:
                    // TayNinh chat nghiêm ngặt hơn - chỉ trả lời về Tây Ninh
                    if (!tayNinhKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                    {
                        // Kiểm tra nếu hỏi về tour hoặc product
                        if (tourKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                        {
                            result.IsValidTopic = false;
                            result.SuggestedChatType = AIChatType.Tour;
                            result.RedirectMessage = $"🏛️ **Tôi chỉ chuyên chia sẻ thông tin về Tây Ninh!**\n\n" +
                                $"Câu hỏi của bạn về **tour du lịch**. Để được tư vấn tour tốt nhất, bạn nên tạo phiên **\"Tour Chat\"** mới.\n\n" +
                                $"🎯 **Trong phiên Tây Ninh này, tôi có thể chia sẻ:**\n" +
                                $"• Lịch sử và văn hóa Tây Ninh\n" +
                                $"• Thông tin về Cao Đài giáo\n" +
                                $"• Địa điểm di tích lịch sử\n" +
                                $"• Ẩm thực truyền thống địa phương\n\n" +
                                $"Bạn có muốn tìm hiểu về Tây Ninh không?";
                        }
                        else if (productKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                        {
                            result.IsValidTopic = false;
                            result.SuggestedChatType = AIChatType.Product;
                            result.RedirectMessage = $"🏛️ **Tôi chỉ chuyên chia sẻ thông tin về Tây Ninh!**\n\n" +
                                $"Câu hỏi của bạn về **mua sắm sản phẩm**. Để được tư vấn sản phẩm, bạn nên tạo phiên **\"Product Chat\"** mới.\n\n" +
                                $"🎯 **Hoặc tôi có thể kể về:**\n" +
                                $"• Nguồn gốc các đặc sản Tây Ninh\n" +
                                $"• Lịch sử bánh tráng Trảng Bàng\n" +
                                $"• Nghề truyền thống làm gốm sứ\n\n" +
                                $"Bạn muốn tìm hiểu về nguồn gốc đặc sản Tây Ninh không?";
                        }
                        else
                        {
                            result.IsValidTopic = false;
                            result.SuggestedChatType = null;
                            result.RedirectMessage = $"🏛️ **Tôi chỉ chuyên chia sẻ thông tin về Tây Ninh!**\n\n" +
                                $"Câu hỏi của bạn không liên quan đến Tây Ninh. Bạn có câu hỏi nào về:\n" +
                                $"• 📚 Lịch sử Tây Ninh và Cao Đài giáo\n" +
                                $"• 🏛️ Các di tích, địa danh nổi tiếng\n" +
                                $"• 🍜 Ẩm thực đặc sắc vùng đất này\n" +
                                $"• 🎭 Văn hóa và truyền thống địa phương\n\n" +
                                $"Hoặc bạn có thể tạo phiên chat khác phù hợp với nhu cầu của mình!";
                        }
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// Kiểm tra intent mạnh về sản phẩm
        /// </summary>
        private bool IsStrongProductIntent(string message)
        {
            var strongProductIntents = new[] {
                "mua", "bán", "giá", "shop", "đặt hàng", "thanh toán",
                "sản phẩm", "cart", "giỏ hàng", "shipping", "giao hàng",
                "đặc sản", "bánh tráng", "mắm", "gốm sứ"
            };
            return strongProductIntents.Count(intent => message.Contains(intent)) >= 1;
        }

        /// <summary>
        /// Kiểm tra intent mạnh về tour
        /// </summary>
        private bool IsStrongTourIntent(string message)
        {
            var strongTourIntents = new[] {
                "tour", "du lịch", "đặt tour", "booking", "lịch trình",
                "hướng dẫn viên", "guide", "tham quan", "giá tour"
            };
            return strongTourIntents.Count(intent => message.Contains(intent)) >= 1;
        }

        /// <summary>
        /// Kiểm tra câu hỏi chung về Tây Ninh (không liên quan tour)
        /// </summary>
        private bool IsGeneralTayNinhQuestion(string message)
        {
            var generalQuestions = new[] {
                "lịch sử", "văn hóa", "cao đài", "truyền thống",
                "dân tộc", "địa lý", "chiến tranh", "biên giới"
            };
            return generalQuestions.Any(q => message.Contains(q));
        }

        public string GetSystemPrompt(AIChatType chatType)
        {
            return chatType switch
            {
                AIChatType.Tour => GetTourSystemPrompt(),
                AIChatType.Product => GetProductSystemPrompt(),
                AIChatType.TayNinh => GetTayNinhSystemPrompt(),
                _ => "Bạn là AI assistant chuyên nghiệp, hỗ trợ người dùng một cách nhiệt tình và chính xác."
            };
        }

        private string GetTourSystemPrompt()
        {
            return @"Bạn là AI tư vấn tour du lịch Tây Ninh chuyên nghiệp với những đặc điểm sau:

NHIỆM VỤ CHÍNH:
- Tư vấn tours, giá cả, lịch trình, dịch vụ đặt tour
- Chỉ giới thiệu tours có sẵn, status PUBLIC và có slot trống
- Không đưa ra thông tin sai lệch về tours
- Hỗ trợ booking và liên kết với các dịch vụ tour

PHONG CÁCH GIAO TIẾP:
- Nhiệt tình, chuyên nghiệp, thân thiện
- Sử dụng emoji phù hợp (🚌 🏛️ 🎯 ✨)  
- Trả lời cụ thể, có cấu trúc rõ ràng
- Luôn đưa ra call-to-action cuối mỗi response

LƯU Ý ĐỀ PHÒNG:
- Nếu user hỏi về mua sắm sản phẩm → gợi ý chuyển sang Product Chat
- Nếu user hỏi về thông tin Tây Ninh chung → gợi ý TayNinh Chat  
- Luôn tập trung vào tư vấn TOUR, không lệch chủ đề

CÁCH TRẢ LỜI:
1. Chào hỏi nhiệt tình
2. Đưa ra thông tin tours cụ thể từ database  
3. Highlight ưu điểm và giá trị
4. Kết thúc bằng câu hỏi hoặc gợi ý tiếp theo";
        }

        private string GetProductSystemPrompt()
        {
            return @"Bạn là AI tư vấn mua sắm sản phẩm đặc sản Tây Ninh với đặc điểm:

NHIỆM VỤ CHÍNH:
- Tư vấn sản phẩm theo nhu cầu và ngân sách
- Chỉ gợi ý sản phẩm còn hàng (QuantityInStock > 0)
- Ưu tiên sản phẩm có rating cao, reviews tích cực
- Hỗ trợ so sánh và đưa ra gợi ý mua hàng

PHONG CÁCH GIAO TIẾP:
- Thân thiện như sales consultant
- Sử dụng emoji shopping (🛍️ 🔥 💎 ✨ ⭐)
- Highlight deals, sales, promotions  
- Tạo cảm giác urgency khi cần thiết

KIẾN THỨC CHUYÊN MÔN:
- Hiểu rõ 4 categories: Food, Souvenir, Jewelry, Clothing
- Am hiểu về chất lượng, xuất xứ sản phẩm
- Biết cách cross-sell và upsell phù hợp
- Hướng dẫn quy trình mua hàng

LƯU Ý ĐỀ PHÒNG:
- Nếu user hỏi về booking tour → gợi ý chuyển Tour Chat
- Nếu user hỏi thông tin Tây Ninh chung → gợi ý TayNinh Chat
- Tập trung vào MUA BÁN, không lệch sang chủ đề khác

CÁCH TRẢ LỜI:
1. Chào đón như trong shop
2. Hiển thị sản phẩm với giá, sale, rating
3. Thuyết phục bằng benefits và social proof  
4. Kết thúc bằng call-to-action mua hàng";
        }

        private string GetTayNinhSystemPrompt()
        {
            return @"Bạn là AI chuyên gia về Tây Ninh - lịch sử, văn hóa, địa điểm, ẩm thực với đặc điểm:

NHIỆM VỤ CHÍNH:
- Chia sẻ kiến thức về lịch sử, văn hóa Tây Ninh
- Giới thiệu các địa điểm, di tích lịch sử
- Kể về ẩm thực, truyền thống địa phương
- CHẶT CHẼ: Chỉ trả lời câu hỏi về Tây Ninh

PHONG CÁCH GIAO TIẾP:
- Như một guide thông thái, uyên bác
- Sử dụng emoji văn hóa (🏛️ 📚 🎭 🍜 ⛩️)
- Kể chuyện sinh động, hấp dẫn
- Thể hiện tự hao về văn hóa địa phương

KIẾN THỨC CHUYÊN SÂU:
- Lịch sử Cao Đài giáo và thánh tích
- Chiến tranh biên giới và các di tích
- Nghề truyền thống: gốm sứ, bánh tráng
- Địa lý, dân tộc, phong tục tập quán

NGHIÊM NGẶT SCOPE:
- CHẶN câu hỏi KHÔNG liên quan Tây Ninh  
- Nếu hỏi về tour → 'Tôi chỉ chia sẻ thông tin, để đặt tour bạn cần...'
- Nếu hỏi về mua sắm → 'Tôi kể về nguồn gốc, để mua bạn cần...'
- Luôn redirect về thông tin VĂN HÓA, LỊCH SỬ

CÁCH TRẢ LỜI:
1. Nhận diện chủ đề Tây Ninh trong câu hỏi
2. Kể chuyện sinh động, có chiều sâu lịch sử  
3. Liên kết với các thông tin liên quan khác
4. Kết thúc bằng câu hỏi mở để tiếp tục đối thoại";
        }

        private async Task<string> EnrichPromptWithData(string message, AIChatType chatType)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Câu hỏi của user: {message}");
            promptBuilder.AppendLine();

            // Enrichment logic remains the same
            if (chatType == AIChatType.Tour)
            {
                await EnrichWithTourData(message, promptBuilder);
            }
            else if (chatType == AIChatType.Product)
            {
                await EnrichWithProductData(message, promptBuilder);
            }

            return promptBuilder.ToString();
        }

        private async Task EnrichWithTourData(string message, StringBuilder promptBuilder)
        {
            var lowerMessage = message.ToLower();

            // Search for tours if user mentions tour-related keywords
            if (lowerMessage.Contains("tour") || lowerMessage.Contains("du lịch") ||
                lowerMessage.Contains("tham quan") || lowerMessage.Contains("núi bà đen"))
            {
                var tours = await _tourDataService.GetAvailableToursAsync(8);
                if (tours.Any())
                {
                    promptBuilder.AppendLine("=== TOURS HIỆN CÓ ===");
                    foreach (var tour in tours)
                    {
                        promptBuilder.AppendLine($"• {tour.Title}");
                        promptBuilder.AppendLine($"  - Từ: {tour.StartLocation} → {tour.EndLocation}");
                        promptBuilder.AppendLine($"  - Giá: {tour.Price:N0} VNĐ");
                        promptBuilder.AppendLine($"  - Còn: {tour.AvailableSlots} chỗ");
                        promptBuilder.AppendLine($"  - Loại: {tour.TourType}");
                        promptBuilder.AppendLine();
                    }
                }
            }

            // Price-based search
            if (lowerMessage.Contains("giá") || lowerMessage.Contains("rẻ") || lowerMessage.Contains("tiền"))
            {
                var budgetTours = await _tourDataService.GetToursByPriceRangeAsync(0, 500000, 5);
                if (budgetTours.Any())
                {
                    promptBuilder.AppendLine("=== TOURS GIÁ TỐT ===");
                    foreach (var tour in budgetTours)
                    {
                        promptBuilder.AppendLine($"• {tour.Title} - {tour.Price:N0} VNĐ");
                    }
                }
            }
        }

        private async Task EnrichWithProductData(string message, StringBuilder promptBuilder)
        {
            var lowerMessage = message.ToLower();

            // Search for products if user mentions product keywords
            if (lowerMessage.Contains("sản phẩm") || lowerMessage.Contains("mua") ||
                lowerMessage.Contains("bánh tráng") || lowerMessage.Contains("gốm sứ"))
            {
                var products = await _productDataService.GetAvailableProductsAsync(8);
                if (products.Any())
                {
                    promptBuilder.AppendLine("\n=== SẢN PHẨM CÓ SẴN ===");
                    foreach (var product in products)
                    {
                        promptBuilder.AppendLine($"• {product.Name} - {product.Price:N0} VNĐ");
                        if (product.IsSale && product.SalePrice.HasValue)
                        {
                            promptBuilder.AppendLine($"  🔥 SALE: {product.SalePrice:N0} VNĐ (giảm {product.SalePercent}%)");
                        }
                        promptBuilder.AppendLine($"  Shop: {product.ShopName}");
                        if (product.AverageRating.HasValue)
                        {
                            promptBuilder.AppendLine($"  Rating: {product.AverageRating:F1}⭐ ({product.ReviewCount} đánh giá)");
                        }
                        promptBuilder.AppendLine();
                    }
                }
            }

            // Sale products
            if (lowerMessage.Contains("giảm giá") || lowerMessage.Contains("sale") || lowerMessage.Contains("khuyến mãi"))
            {
                var saleProducts = await _productDataService.GetProductsOnSaleAsync(5);
                if (saleProducts.Any())
                {
                    promptBuilder.AppendLine("\n=== SẢN PHẨM ĐANG SALE ===");
                    foreach (var product in saleProducts)
                    {
                        promptBuilder.AppendLine($"• {product.Name} - ~~{product.Price:N0}~~ → {product.SalePrice:N0} VNĐ");
                    }
                }
            }

            // Best selling products
            if (lowerMessage.Contains("bán chạy") || lowerMessage.Contains("phổ biến") || lowerMessage.Contains("nổi tiếng"))
            {
                var bestSellers = await _productDataService.GetBestSellingProductsAsync(5);
                if (bestSellers.Any())
                {
                    promptBuilder.AppendLine("\n=== SẢN PHẨM BÁN CHẠY ===");
                    foreach (var product in bestSellers)
                    {
                        promptBuilder.AppendLine($"• {product.Name} - Đã bán {product.SoldCount}");
                        promptBuilder.AppendLine($"   • Giá: {product.Price:N0} VNĐ");
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
            // Kiểm tra nếu response không liên quan đến Tây Ninh
            var lowerResponse = response.ToLower();
            var tayNinhKeywords = new[] { "tây ninh", "núi bà đen", "cao đài", "bánh tráng", "trảng bàng", "biên giới" };

            if (!tayNinhKeywords.Any(keyword => lowerResponse.Contains(keyword)) &&
                lowerResponse.Length > 50) // Chỉ check với response dài
            {
                return "Tôi chỉ chuyên tư vấn về Tây Ninh. Bạn có câu hỏi nào về lịch sử, văn hóa, địa điểm hay ẩm thực Tây Ninh không?";
            }

            return response;
        }

        private string GetTourTypeDisplay(string tourType)
        {
            return tourType switch
            {
                "FreeScenic" => "Danh lam thắng cảnh (miễn phí vé vào cửa)",
                "PaidAttraction" => "Khu vui chơi (có vé vào cửa)",
                _ => tourType
            };
        }

        private string GetFallbackResponse(AIChatType chatType)
        {
            return chatType switch
            {
                AIChatType.Tour => "Xin lỗi, hiện tại tôi đang gặp khó khăn trong việc tư vấn tour. Vui lòng liên hệ hotline để được hỗ trợ trực tiếp.",
                AIChatType.Product => "Xin lỗi, hệ thống tư vấn sản phẩm tạm thời gián đoạn. Bạn có thể duyệt catalog sản phẩm trực tiếp hoặc liên hệ shop.",
                AIChatType.TayNinh => "Xin lỗi, tôi tạm thời không thể chia sẻ thông tin về Tây Ninh. Vui lòng thử lại sau ít phút.",
                _ => "Xin lỗi, tôi hiện đang gặp khó khăn kỹ thuật. Vui lòng thử lại sau hoặc liên hệ hỗ trợ."
            };
        }
    }

    /// <summary>
    /// Kết quả validation topic alignment
    /// </summary>
    public class TopicValidationResult
    {
        public bool IsValidTopic { get; set; }
        public string RedirectMessage { get; set; } = string.Empty;
        public AIChatType? SuggestedChatType { get; set; }
    }
}