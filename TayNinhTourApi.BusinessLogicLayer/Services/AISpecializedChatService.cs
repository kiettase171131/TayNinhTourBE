using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AISpecializedChatService> _logger;

        public AISpecializedChatService(
            IGeminiAIService geminiAIService,
            IAITourDataService tourDataService,
            IAIProductDataService productDataService,
            IUnitOfWork unitOfWork,
            ILogger<AISpecializedChatService> logger)
        {
            _geminiAIService = geminiAIService;
            _tourDataService = tourDataService;
            _productDataService = productDataService;
            _unitOfWork = unitOfWork;
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

🎯 **NHIỆM VỤ CHÍNH:**
- Tư vấn tours, giá cả, lịch trình, dịch vụ đặt tour
- CHỈ giới thiệu tours có sẵn trong dữ liệu được cung cấp từ database
- CHỈ sử dụng thông tin tour THỰC TẾ, không tạo ra tour giả
- Ưu tiên tours có nhiều slot trống và giá tốt
- Hỗ trợ booking và liên kết với các dịch vụ tour

🚨 **NGUYÊN TẮC QUAN TRỌNG:**
- NGHIÊM CẤM tạo ra thông tin tour không có trong database
- NGHIÊM CẤM bịa đặt giá cả, tên tour, hoặc thông tin lịch trình
- Nếu không có tour phù hợp trong database → nói thẳng 'Hiện tại không có tour này'
- Luôn dựa vào dữ liệu THỰC TẾ được cung cấp trong prompt
- Giá tours đã bao gồm từ TourOperation (giá thực tế, không phải ước tính)

💬 **PHONG CÁCH GIAO TIẾP:**
- Nhiệt tình, chuyên nghiệp, thân thiện như consultant du lịch
- Sử dụng emoji phù hợp (🚌 🏛️ 🎯 ✨ 💰 🗓️)  
- Trả lời cụ thể, có cấu trúc rõ ràng với bullet points
- Luôn đưa ra call-to-action cuối mỗi response
- Highlight deals tốt và slots còn ít để tạo urgency

📋 **KIẾN THỨC CHUYÊN MÔN:**
- Hiểu 2 loại tour: FreeScenic (miễn phí vé) vs PaidAttraction (có vé)
- Biết so sánh giá và value proposition của từng tour
- Hiểu lịch trình và thời gian phù hợp cho từng loại khách
- Tư vấn theo budget và sở thích của khách

⚠️ **LƯU Ý ĐỀ PHÒNG:**
- Nếu user hỏi về mua sắm sản phẩm → gợi ý chuyển sang Product Chat
- Nếu user hỏi về thông tin Tây Ninh chung → gợi ý TayNinh Chat  
- Luôn tập trung vào tư vấn TOUR CÓ THẬT, không lệch chủ đề

📝 **CÁCH TRẢ LỜI CHUẨN:**
1. **Chào hỏi nhiệt tình** với emoji phù hợp
2. **Phân tích nhu cầu** của khách (budget, thời gian, sở thích)  
3. **Giới thiệu tours cụ thể** từ database với:
   - Tên tour chính xác
   - Giá thực tế từ TourOperation
   - Số chỗ trống hiện tại
   - Ngày có tour gần nhất
   - Highlights và value proposition
4. **So sánh** ưu nhược điểm giữa các tours
5. **Tạo urgency** nếu tour có ít slot hoặc giá tốt
6. **Call-to-action** cụ thể: 'Bạn muốn đặt tour nào?' hoặc 'Cần tôi check thêm thông tin gì?'

🔢 **FORMAT HIỂN THỊ TOUR:**
```
🎯 **[TÊN TOUR]**
💰 Giá: [GIÁ THỰC] VNĐ/người
📍 Tuyến: [ĐIỂM ĐI] → [ĐIỂM ĐẾN]  
🪑 Còn: [SỐ CHỖ TRỐNG] chỗ
📅 Ngày gần nhất: [NGÀY]
⭐ Nổi bật: [HIGHLIGHTS]
```

❌ **TUYỆT ĐỐI KHÔNG ĐƯỢC:**
- Tạo ra tours không có trong dữ liệu
- Ước đoán giá hoặc thông tin không chắc chắn
- Copy paste thông tin từ tour này sang tour khác
- Đưa ra lịch trình chi tiết không có trong database

✅ **NẾU KHÔNG CÓ TOUR PHÙ HỢP:**
'Hiện tại hệ thống chưa có tour [yêu cầu của khách] phù hợp. Tuy nhiên, tôi có thể gợi ý các tours tương tự: [danh sách tours thực tế]. Hoặc bạn có thể liên hệ trực tiếp để được tư vấn thêm.'

Hãy tư vấn dựa trên dữ liệu THỰC TẾ được cung cấp và tạo trải nghiệm tư vấn chuyên nghiệp!";
        }

        private string GetProductSystemPrompt()
        {
            return @"Bạn là AI tư vấn mua sắm sản phẩm đặc sản Tây Ninh với đặc điểm:

NHIỆM VỤ CHÍNH:
- Tư vấn sản phẩm theo nhu cầu và ngân sách
- CHỈ gợi ý sản phẩm có trong dữ liệu được cung cấp từ database
- CHỈ sử dụng thông tin sản phẩm THỰC TẾ, không tạo ra sản phẩm giả
- Ưu tiên sản phẩm có rating cao, reviews tích cực
- Hỗ trợ so sánh và đưa ra gợi ý mua hàng

NGUYÊN TẮC QUAN TRỌNG:
- NGHIÊM CẤM tạo ra thông tin sản phẩm không có trong database
- NGHIÊM CẤM bịa đặt giá cả, tên sản phẩm, hoặc thông tin shop
- Nếu không có sản phẩm phù hợp trong database → nói thẳng 'Hiện tại không có sản phẩm này'
- Luôn dựa vào dữ liệu THỰC TẾ được cung cấp trong prompt

PHONG CÁCH GIAO TIẾP:
- Thân thiện như sales consultant
- Sử dụng emoji shopping (🛍️ 🔥 💎 ✨ ⭐)
- Highlight deals, sales, promotions thực tế
- Tạo cảm giác urgency khi có sản phẩm thật sắp hết hàng

KIẾN THỨC CHUYÊN MÔN:
- Chỉ tư vấn các categories có trong database: Food, Souvenir, Jewelry, Clothing
- Dựa vào thông tin stock, rating, reviews thực tế từ hệ thống
- Biết cách cross-sell và upsell từ sản phẩm có sẵn
- Hướng dẫn quy trình mua hàng

LƯU Ý ĐỀ PHÒNG:
- Nếu user hỏi về booking tour → gợi ý chuyển Tour Chat
- Nếu user hỏi thông tin Tây Ninh chung → gợi ý TayNinh Chat
- Tập trung vào MUA BÁN sản phẩm CÓ THẬT, không lệch sang chủ đề khác

CÁCH TRẢ LỜI:
1. Chào đón như trong shop
2. Hiển thị CHÍNH XÁC sản phẩm với giá, sale, rating từ database
3. Thuyết phục bằng benefits thực tế và social proof từ reviews
4. Kết thúc bằng call-to-action mua hàng
5. Nếu không có sản phẩm → thành thật nói 'Hiện tại chưa có' và đề xuất liên hệ

CẢNH BÁO: Tuyệt đối không được tạo ra thông tin sản phẩm giả, tên shop giả, hay giá cả không có trong database!";
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

            _logger.LogInformation("Enriching tour data for AI prompt with REAL database data. User message: {Message}", message);

            // 1. Luôn hiển thị tours có sẵn khi user hỏi về tour
            if (lowerMessage.Contains("tour") || lowerMessage.Contains("du lịch") ||
                lowerMessage.Contains("tham quan") || lowerMessage.Contains("núi bà đen") ||
                lowerMessage.Contains("chùa") || lowerMessage.Contains("cao đài") ||
                lowerMessage.Contains("giá") || lowerMessage.Contains("booking") ||
                lowerMessage.Contains("đặt") || lowerMessage.Contains("có tour nào") ||
                lowerMessage.Contains("slot") || lowerMessage.Contains("chỗ") || 
                lowerMessage.Contains("người") || lowerMessage.Contains("book"))
            {
                _logger.LogInformation("User asking about tours - fetching ALL available tours from database");
                var tours = await _tourDataService.GetAvailableToursAsync(10);
                
                if (tours.Any())
                {
                    _logger.LogInformation("Retrieved {Count} REAL tours from database for AI recommendation", tours.Count);
                    promptBuilder.AppendLine("\n=== TOURS CÓ SẴN THỰC TẾ TỪ DATABASE ===");
                    
                    foreach (var tour in tours)
                    {
                        promptBuilder.AppendLine($"✅ **{tour.Title}**");
                        promptBuilder.AppendLine($"   📍 Từ: {tour.StartLocation} → {tour.EndLocation}");
                        promptBuilder.AppendLine($"   💰 Giá: {tour.Price:N0} VNĐ/người (GIÁ THỰC từ TourOperation)");
                        promptBuilder.AppendLine($"   🪑 Tổng còn: {tour.AvailableSlots} chỗ trống");
                        promptBuilder.AppendLine($"   🎯 Loại: {tour.TourType}");
                        promptBuilder.AppendLine($"   🏢 Công ty: {tour.CompanyName}");
                        
                        // 🔧 NEW: Hiển thị chi tiết từng slot riêng biệt
                        if (tour.AvailableDates.Any())
                        {
                            promptBuilder.AppendLine($"   📅 **CHI TIẾT TỪNG NGÀY/SLOT:**");
                            
                            // Lấy chi tiết slots cho tour này
                            await EnrichWithDetailedSlotInfo(tour.Id, promptBuilder);
                        }

                        if (tour.Highlights.Any())
                        {
                            promptBuilder.AppendLine($"   ⭐ Nổi bật: {string.Join(", ", tour.Highlights.Take(3))}");
                        }

                        promptBuilder.AppendLine($"   🔢 ID Tour: {tour.Id}");
                        promptBuilder.AppendLine();
                    }
                    
                    promptBuilder.AppendLine("📋 **LƯU Ý QUAN TRỌNG:**");
                    promptBuilder.AppendLine("- Đây là dữ liệu THỰC TẾ từ cơ sở dữ liệu, không phải thông tin giả");
                    promptBuilder.AppendLine("- Giá đã bao gồm từ TourOperation (giá thực tế hiện tại)");
                    promptBuilder.AppendLine("- Chỉ tư vấn các tours này, KHÔNG tạo ra tours không có trong danh sách");
                    promptBuilder.AppendLine("- Tours đều có status PUBLIC và có thể đặt ngay");
                    promptBuilder.AppendLine("- Chi tiết slots giúp bạn chọn ngày phù hợp với số lượng khách");
                    promptBuilder.AppendLine();
                }
                else
                {
                    _logger.LogWarning("No tours found in database - this is a critical issue for tour consultation");
                    promptBuilder.AppendLine("\n=== CẢNH BÁO: KHÔNG CÓ TOUR NÀO ===");
                    promptBuilder.AppendLine("Hiện tại KHÔNG có tour nào trong cơ sở dữ liệu có thể đặt được.");
                    promptBuilder.AppendLine("Nguyên nhân có thể:");
                    promptBuilder.AppendLine("- Chưa có tour nào có status PUBLIC");
                    promptBuilder.AppendLine("- Tất cả tours đã hết chỗ");
                    promptBuilder.AppendLine("- Tours chưa có TourOperation với giá");
                    promptBuilder.AppendLine("- Vấn đề kết nối database");
                    promptBuilder.AppendLine();
                    promptBuilder.AppendLine("🚨 **HÃY THÔNG BÁO CHO KHÁCH HÀNG:**");
                    promptBuilder.AppendLine("'Hiện tại hệ thống chưa có tour nào sẵn sàng để đặt. Vui lòng liên hệ trực tiếp qua hotline hoặc thử lại sau.'");
                    promptBuilder.AppendLine("TUYỆT ĐỐI KHÔNG TẠO RA THÔNG TIN TOUR GIẢ!");
                    promptBuilder.AppendLine();
                }
            }

            // 2. Đặc biệt xử lý câu hỏi về số lượng khách cụ thể
            if (lowerMessage.Contains("người") || lowerMessage.Contains("khách") || 
                lowerMessage.Contains("slot") || lowerMessage.Contains("đủ") ||
                lowerMessage.Contains("chỗ") || Regex.IsMatch(lowerMessage, @"\d+\s*(người|khách|chỗ)"))
            {
                _logger.LogInformation("User asking about specific guest capacity - providing detailed slot analysis");
                
                // Extract số lượng khách từ câu hỏi
                var guestCountMatch = Regex.Match(lowerMessage, @"(\d+)\s*người");
                if (guestCountMatch.Success && int.TryParse(guestCountMatch.Groups[1].Value, out int requestedGuests))
                {
                    promptBuilder.AppendLine($"\n🔍 **PHÂN TÍCH CHO {requestedGuests} KHÁCH:**");
                    
                    var tours = await _tourDataService.GetAvailableToursAsync(10);
                    foreach (var tour in tours)
                    {
                        await AnalyzeSlotCapacityForGuests(tour.Id, requestedGuests, promptBuilder);
                    }
                }
                else
                {
                    promptBuilder.AppendLine("\n🔍 **THÔNG TIN CHI TIẾT CAPACITY CÁC SLOTS:**");
                    var tours = await _tourDataService.GetAvailableToursAsync(10);
                    foreach (var tour in tours)
                    {
                        await EnrichWithDetailedSlotInfo(tour.Id, promptBuilder);
                    }
                }
            }

            // 3. Tìm kiếm theo khoảng giá cụ thể
            if (lowerMessage.Contains("giá") || lowerMessage.Contains("rẻ") || lowerMessage.Contains("tiền") ||
                lowerMessage.Contains("budget") || lowerMessage.Contains("bao nhiêu"))
            {
                _logger.LogInformation("User asking about tour prices - fetching budget-friendly tours");
                
                // Tìm tours giá dưới 500k
                var budgetTours = await _tourDataService.GetToursByPriceRangeAsync(0, 500000, 8);
                if (budgetTours.Any())
                {
                    _logger.LogInformation("Found {Count} budget tours under 500k", budgetTours.Count);
                    promptBuilder.AppendLine("\n=== TOURS GIÁ TÔNG (DƯỚI 500K) ===");
                    foreach (var tour in budgetTours.OrderBy(t => t.Price))
                    {
                        promptBuilder.AppendLine($"💎 {tour.Title} - {tour.Price:N0} VNĐ");
                        promptBuilder.AppendLine($"   📍 {tour.StartLocation} → {tour.EndLocation}");
                        promptBuilder.AppendLine($"   🪑 Còn {tour.AvailableSlots} chỗ");
                    }
                    promptBuilder.AppendLine();
                }

                // Tìm tours cao cấp (trên 500k)
                var premiumTours = await _tourDataService.GetToursByPriceRangeAsync(500000, 2000000, 5);
                if (premiumTours.Any())
                {
                    _logger.LogInformation("Found {Count} premium tours over 500k", premiumTours.Count);
                    promptBuilder.AppendLine("\n=== TOURS CAO CẤP (TRÊN 500K) ===");
                    foreach (var tour in premiumTours.OrderBy(t => t.Price))
                    {
                        promptBuilder.AppendLine($"⭐ {tour.Title} - {tour.Price:N0} VNĐ");
                        promptBuilder.AppendLine($"   📍 {tour.StartLocation} → {tour.EndLocation}");
                        promptBuilder.AppendLine($"   🪑 Còn {tour.AvailableSlots} chỗ");
                    }
                    promptBuilder.AppendLine();
                }
            }

            // 4. Tìm kiếm theo loại tour
            if (lowerMessage.Contains("danh lam") || lowerMessage.Contains("thắng cảnh") || 
                lowerMessage.Contains("miễn phí") || lowerMessage.Contains("free"))
            {
                _logger.LogInformation("User asking about scenic tours");
                var scenicTours = await _tourDataService.GetToursByTypeAsync("FreeScenic", 6);
                if (scenicTours.Any())
                {
                    promptBuilder.AppendLine("\n=== TOURS DANH LAM THẮNG CẢNH (MIỄN PHÍ VÉ VÀO CỬA) ===");
                    foreach (var tour in scenicTours)
                    {
                        promptBuilder.AppendLine($"🏞️ {tour.Title} - {tour.Price:N0} VNĐ");
                        promptBuilder.AppendLine($"   📍 {tour.StartLocation} → {tour.EndLocation}");
                        promptBuilder.AppendLine($"   🆓 Không phí vé vào cửa các địa điểm");
                    }
                    promptBuilder.AppendLine();
                }
            }

            if (lowerMessage.Contains("vui chơi") || lowerMessage.Contains("giải trí") ||
                lowerMessage.Contains("paid") || lowerMessage.Contains("khu du lịch"))
            {
                _logger.LogInformation("User asking about attraction tours");
                var attractionTours = await _tourDataService.GetToursByTypeAsync("PaidAttraction", 6);
                if (attractionTours.Any())
                {
                    promptBuilder.AppendLine("\n=== TOURS KHU VUI CHƠI (BAO GỒM VÉ VÀO CỬA) ===");
                    foreach (var tour in attractionTours)
                    {
                        promptBuilder.AppendLine($"🎢 {tour.Title} - {tour.Price:N0} VNĐ");
                        promptBuilder.AppendLine($"   📍 {tour.StartLocation} → {tour.EndLocation}");
                        promptBuilder.AppendLine($"   🎫 Bao gồm vé vào cửa tất cả địa điểm");
                    }
                    promptBuilder.AppendLine();
                }
            }

            // 5. Tìm kiếm theo địa điểm cụ thể
            if (lowerMessage.Contains("núi bà đen"))
            {
                _logger.LogInformation("User asking about Nui Ba Den tours");
                var badenTours = await _tourDataService.SearchToursAsync("Núi Bà Đen", 5);
                if (badenTours.Any())
                {
                    promptBuilder.AppendLine("\n=== TOURS NÚI BÀ ĐEN ===");
                    foreach (var tour in badenTours)
                    {
                        promptBuilder.AppendLine($"⛰️ {tour.Title} - {tour.Price:N0} VNĐ");
                        promptBuilder.AppendLine($"   🪑 Còn {tour.AvailableSlots} chỗ");
                        if (tour.AvailableDates.Any())
                        {
                            var nextDate = tour.AvailableDates.OrderBy(d => d).FirstOrDefault().ToString("dd/MM/yyyy");
                            promptBuilder.AppendLine($"   📅 Ngày gần nhất: {nextDate}");
                        }
                    }
                    promptBuilder.AppendLine();
                }
            }

            // 6. Tìm kiếm theo thời gian (hôm nay, mai, cuối tuần)
            if (lowerMessage.Contains("hôm nay") || lowerMessage.Contains("today"))
            {
                var todayTours = await _tourDataService.GetAvailableToursByDateAsync(DateTime.Today, 5);
                if (todayTours.Any())
                {
                    promptBuilder.AppendLine("\n=== TOURS HÔM NAY ===");
                    foreach (var tour in todayTours)
                    {
                        promptBuilder.AppendLine($"🗓️ {tour.Title} - {tour.Price:N0} VNĐ - {tour.AvailableSlots} chỗ");
                    }
                    promptBuilder.AppendLine();
                }
            }

            if (lowerMessage.Contains("ngày mai") || lowerMessage.Contains("tomorrow"))
            {
                var tomorrowTours = await _tourDataService.GetAvailableToursByDateAsync(DateTime.Today.AddDays(1), 5);
                if (tomorrowTours.Any())
                {
                    promptBuilder.AppendLine("\n=== TOURS NGÀY MAI ===");
                    foreach (var tour in tomorrowTours)
                    {
                        promptBuilder.AppendLine($"📅 {tour.Title} - {tour.Price:N0} VNĐ - {tour.AvailableSlots} chỗ");
                    }
                    promptBuilder.AppendLine();
                }
            }

            // 7. Thống kê tổng quan cuối prompt
            var totalTours = await _tourDataService.GetAvailableToursAsync(100);
            if (totalTours.Any())
            {
                var totalSlots = totalTours.Sum(t => t.AvailableSlots);
                var avgPrice = totalTours.Average(t => t.Price);
                var minPrice = totalTours.Min(t => t.Price);
                var maxPrice = totalTours.Max(t => t.Price);

                promptBuilder.AppendLine($"\n📊 **THỐNG KÊ TỔNG QUAN:**");
                promptBuilder.AppendLine($"- Tổng {totalTours.Count} tours có sẵn để đặt");
                promptBuilder.AppendLine($"- Tổng {totalSlots} chỗ trống");
                promptBuilder.AppendLine($"- Giá từ {minPrice:N0} - {maxPrice:N0} VNĐ");
                promptBuilder.AppendLine($"- Giá trung bình: {avgPrice:N0} VNĐ");
                promptBuilder.AppendLine();
            }

            _logger.LogInformation("Completed tour data enrichment with {Count} tours", totalTours.Count);
        }

        /// <summary>
        /// 🔧 NEW: Lấy chi tiết từng slot của một tour cụ thể
        /// </summary>
        private async Task EnrichWithDetailedSlotInfo(Guid tourTemplateId, StringBuilder promptBuilder)
        {
            try
            {
                // Lấy chi tiết slots cho tour template này
                var slots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.TourTemplateId == tourTemplateId &&
                                ts.IsActive &&
                                ts.Status == TourSlotStatus.Available &&
                                ts.MaxGuests > ts.CurrentBookings &&
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourDetails != null &&
                                ts.TourDetails.Status == TourDetailsStatus.Public &&
                                ts.TourDetails.TourOperation != null)
                    .OrderBy(ts => ts.TourDate)
                    .ToListAsync();

                foreach (var slot in slots)
                {
                    var availableSpots = slot.MaxGuests - slot.CurrentBookings;
                    var dateStr = slot.TourDate.ToString("dd/MM/yyyy");
                    var dayOfWeek = slot.TourDate.ToDateTime(TimeOnly.MinValue).ToString("dddd", new System.Globalization.CultureInfo("vi-VN"));
                    
                    promptBuilder.AppendLine($"     🗓️ {dayOfWeek} {dateStr}: {availableSpots}/{slot.MaxGuests} chỗ trống");
                    
                    if (availableSpots >= 5)
                    {
                        promptBuilder.AppendLine($"       ✅ Đủ cho nhóm 5+ người");
                    }
                    else if (availableSpots > 0)
                    {
                        promptBuilder.AppendLine($"       ⚠️ Chỉ đủ cho nhóm {availableSpots} người");
                    }
                    else
                    {
                        promptBuilder.AppendLine($"       ❌ Đã kín chỗ");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed slot info for tour template {TourTemplateId}", tourTemplateId);
                promptBuilder.AppendLine($"     ⚠️ Không thể lấy chi tiết slots");
            }
        }

        /// <summary>
        /// 🔧 NEW: Phân tích capacity cho số lượng khách cụ thể
        /// </summary>
        private async Task AnalyzeSlotCapacityForGuests(Guid tourTemplateId, int requestedGuests, StringBuilder promptBuilder)
        {
            try
            {
                var slots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.TourTemplateId == tourTemplateId &&
                                ts.IsActive &&
                                ts.Status == TourSlotStatus.Available &&
                                ts.MaxGuests > ts.CurrentBookings &&
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourDetails != null &&
                                ts.TourDetails.Status == TourDetailsStatus.Public &&
                                ts.TourDetails.TourOperation != null)
                    .OrderBy(ts => ts.TourDate)
                    .ToListAsync();

                if (slots.Any())
                {
                    var tourTitle = slots.First().TourTemplate.Title;
                    promptBuilder.AppendLine($"\n🎯 **{tourTitle}:**");
                    
                    var suitableSlots = slots.Where(s => (s.MaxGuests - s.CurrentBookings) >= requestedGuests).ToList();
                    var unsuitableSlots = slots.Where(s => (s.MaxGuests - s.CurrentBookings) < requestedGuests && (s.MaxGuests - s.CurrentBookings) > 0).ToList();
                    
                    if (suitableSlots.Any())
                    {
                        promptBuilder.AppendLine($"   ✅ **SLOTS ĐỦ CHỖ CHO {requestedGuests} NGƯỜI:**");
                        foreach (var slot in suitableSlots)
                        {
                            var availableSpots = slot.MaxGuests - slot.CurrentBookings;
                            var dateStr = slot.TourDate.ToString("dd/MM/yyyy");
                            promptBuilder.AppendLine($"     • {dateStr}: {availableSpots} chỗ trống (đủ cho {requestedGuests} người)");
                        }
                    }
                    
                    if (unsuitableSlots.Any())
                    {
                        promptBuilder.AppendLine($"   ⚠️ **SLOTS KHÔNG ĐỦ CHỖ CHO {requestedGuests} NGƯỜI:**");
                        foreach (var slot in unsuitableSlots)
                        {
                            var availableSpots = slot.MaxGuests - slot.CurrentBookings;
                            var dateStr = slot.TourDate.ToString("dd/MM/yyyy");
                            promptBuilder.AppendLine($"     • {dateStr}: chỉ còn {availableSpots} chỗ (thiếu {requestedGuests - availableSpots} chỗ)");
                        }
                    }
                    
                    if (!suitableSlots.Any() && !unsuitableSlots.Any())
                    {
                        promptBuilder.AppendLine($"   ❌ **KHÔNG CÓ SLOT NÀO PHÙ HỢP CHO {requestedGuests} NGƯỜI**");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing slot capacity for {RequestedGuests} guests, tour {TourTemplateId}", requestedGuests, tourTemplateId);
            }
        }

        private async Task EnrichWithProductData(string message, StringBuilder promptBuilder)
        {
            var lowerMessage = message.ToLower();

            // Search for products if user mentions product keywords
            if (lowerMessage.Contains("sản phẩm") || lowerMessage.Contains("mua") ||
                lowerMessage.Contains("bánh tráng") || lowerMessage.Contains("gốm sứ"))
            {
                _logger.LogInformation("User asking about products - fetching REAL data from database");
                var products = await _productDataService.GetAvailableProductsAsync(8);
                if (products.Any())
                {
                    _logger.LogInformation("Retrieved {Count} real products from database for AI context", products.Count);
                    promptBuilder.AppendLine("\n=== SẢN PHẨM CÓ SẴN THỰC TẾ TỪ DATABASE ===");
                    foreach (var product in products)
                    {
                        promptBuilder.AppendLine($"• {product.Name} - {product.Price:N0} VNĐ");
                        if (product.IsSale && product.SalePrice.HasValue)
                        {
                            promptBuilder.AppendLine($"  🔥 SALE: {product.SalePrice:N0} VNĐ (giảm {product.SalePercent}%)");
                        }
                        promptBuilder.AppendLine($"  Shop: {product.ShopName}");
                        promptBuilder.AppendLine($"  Tồn kho: {product.QuantityInStock} sản phẩm");
                        if (product.AverageRating.HasValue)
                        {
                            promptBuilder.AppendLine($"  Rating: {product.AverageRating:F1}⭐ ({product.ReviewCount} đánh giá)");
                        }
                        promptBuilder.AppendLine($"  Đã bán: {product.SoldCount}");
                        promptBuilder.AppendLine();
                    }
                    promptBuilder.AppendLine("LƯU Ý: Đây là dữ liệu THỰC TẾ từ cơ sở dữ liệu, không phải dữ liệu giả. Chỉ tư vấn các sản phẩm này!");
                }
                else
                {
                    _logger.LogWarning("No products found in database - may indicate database connection issues");
                    promptBuilder.AppendLine("\n=== CẢNH BÁO ===");
                    promptBuilder.AppendLine("Hiện tại không có sản phẩm nào trong cơ sở dữ liệu.");
                    promptBuilder.AppendLine("Vui lòng thông báo cho khách hàng liên hệ trực tiếp để được hỗ trợ.");
                    promptBuilder.AppendLine("KHÔNG TẠO RA THÔNG TIN SẢN PHẨM GIẢ!");
                }
            }

            // Sale products
            if (lowerMessage.Contains("giảm giá") || lowerMessage.Contains("sale") || lowerMessage.Contains("khuyến mãi"))
            {
                _logger.LogInformation("User asking about sale products - fetching from database");
                var saleProducts = await _productDataService.GetProductsOnSaleAsync(5);
                if (saleProducts.Any())
                {
                    _logger.LogInformation("Found {Count} real sale products from database", saleProducts.Count);
                    promptBuilder.AppendLine("\n=== SẢN PHẨM ĐANG SALE THỰC TẾ ===");
                    foreach (var product in saleProducts)
                    {
                        promptBuilder.AppendLine($"• {product.Name} - ~~{product.Price:N0}~~ → {product.SalePrice:N0} VNĐ");
                        promptBuilder.AppendLine($"  Giảm {product.SalePercent}% - Còn {product.QuantityInStock} sản phẩm");
                    }
                    promptBuilder.AppendLine("(Dữ liệu sale thực tế từ hệ thống)");
                }
                else
                {
                    _logger.LogInformation("No sale products found in database");
                    promptBuilder.AppendLine("\n=== THÔNG BÁO ===");
                    promptBuilder.AppendLine("Hiện tại không có sản phẩm nào đang giảm giá trong hệ thống.");
                }
            }

            // Best selling products
            if (lowerMessage.Contains("bán chạy") || lowerMessage.Contains("phổ biến") || lowerMessage.Contains("nổi tiếng"))
            {
                _logger.LogInformation("User asking about best selling products - fetching from database");
                var bestSellers = await _productDataService.GetBestSellingProductsAsync(5);
                if (bestSellers.Any())
                {
                    _logger.LogInformation("Found {Count} best selling products from database", bestSellers.Count);
                    promptBuilder.AppendLine("\n=== SẢN PHẨM BÁN CHẠY THỰC TẾ ===");
                    foreach (var product in bestSellers)
                    {
                        promptBuilder.AppendLine($"• {product.Name} - Đã bán {product.SoldCount} sản phẩm");
                        promptBuilder.AppendLine($"   • Giá: {product.Price:N0} VNĐ - Còn {product.QuantityInStock} trong kho");
                    }
                    promptBuilder.AppendLine("(Dữ liệu bán chạy thực tế từ hệ thống)");
                }
                else
                {
                    _logger.LogInformation("No best selling products found in database");
                    promptBuilder.AppendLine("\n=== THÔNG BÁO ===");
                    promptBuilder.AppendLine("Chưa có dữ liệu sản phẩm bán chạy trong hệ thống.");
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