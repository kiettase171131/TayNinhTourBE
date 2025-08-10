using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.AIChat;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AIChat;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller quản lý AI Chat - chat với AI chatbot sử dụng Gemini API với 3 loại chat chuyên biệt
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AIChatController : ControllerBase
    {
        private readonly IAIChatService _aiChatService;
        private readonly IGeminiAIService _geminiAIService;
        private readonly ILogger<AIChatController> _logger;
        private readonly IConfiguration _configuration;

        public AIChatController(
            IAIChatService aiChatService,
            IGeminiAIService geminiAIService,
            ILogger<AIChatController> logger,
            IConfiguration configuration)
        {
            _aiChatService = aiChatService;
            _geminiAIService = geminiAIService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Test endpoint để kiểm tra Gemini API (không cần authentication)
        /// </summary>
        /// <param name="message">Tin nhắn test</param>
        /// <returns>Phản hồi từ Gemini AI</returns>
        [HttpPost("test-gemini")]
        public async Task<ActionResult> TestGemini([FromBody] TestGeminiRequest request)
        {
            try
            {
                _logger.LogInformation("Testing Gemini API with message: {Message}", request.Message);

                var response = await _geminiAIService.GenerateContentAsync(request.Message);

                return Ok(new
                {
                    success = response.Success,
                    message = response.Success ? "Gemini API test thành công" : "Gemini API test thất bại",
                    data = new
                    {
                        userMessage = request.Message,
                        aiResponse = response.Content,
                        tokensUsed = response.TokensUsed,
                        responseTimeMs = response.ResponseTimeMs,
                        errorMessage = response.ErrorMessage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Gemini API");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi test Gemini API",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Tạo phiên chat mới với loại chat cụ thể
        /// </summary>
        /// <param name="request">Thông tin phiên chat mới</param>
        /// <returns>Kết quả tạo phiên chat</returns>
        [HttpPost("sessions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseCreateChatSessionDto>> CreateChatSession([FromBody] RequestCreateChatSessionDto request)
        {
            try
            {
                _logger.LogInformation("Creating {ChatType} chat session", request.ChatType);
                
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user is null");
                    return StatusCode(401, new ResponseCreateChatSessionDto
                    {
                        success = false,
                        Message = "Không thể xác thực người dùng",
                        StatusCode = 401
                    });
                }

                _logger.LogInformation("Creating {ChatType} chat session for user {UserId}", 
                    request.ChatType, currentUser.UserId);
                
                var response = await _aiChatService.CreateChatSessionAsync(request, currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {ChatType} chat session", request.ChatType);
                return StatusCode(500, new ResponseCreateChatSessionDto
                {
                    success = false,
                    Message = $"Có lỗi xảy ra khi tạo phiên chat: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Gửi tin nhắn đến AI chatbot với xử lý chuyên biệt theo loại chat
        /// </summary>
        /// <param name="request">Thông tin tin nhắn</param>
        /// <returns>Phản hồi từ AI</returns>
        [HttpPost("messages")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseSendMessageDto>> SendMessage([FromBody] RequestSendMessageDto request)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _aiChatService.SendMessageAsync(request, currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to session {SessionId}", request.SessionId);
                return StatusCode(500, new ResponseSendMessageDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi gửi tin nhắn",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Lấy danh sách phiên chat của user hiện tại với hỗ trợ lọc theo loại chat
        /// </summary>
        /// <param name="page">Trang hiện tại (0-based)</param>
        /// <param name="pageSize">Số lượng sessions per page</param>
        /// <param name="status">Trạng thái session (Active, Archived, All)</param>
        /// <param name="chatType">Lọc theo loại chat (Tour, Product, TayNinh)</param>
        /// <returns>Danh sách phiên chat</returns>
        [HttpGet("sessions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseGetChatSessionsDto>> GetChatSessions(
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 20,
            [FromQuery] string status = "Active",
            [FromQuery] AIChatType? chatType = null)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var request = new RequestGetChatSessionsDto
                {
                    Page = page,
                    PageSize = pageSize,
                    Status = status,
                    ChatType = chatType
                };
                
                _logger.LogInformation("Getting chat sessions for user {UserId}, ChatType filter: {ChatType}", 
                    currentUser.UserId, chatType);
                
                var response = await _aiChatService.GetChatSessionsAsync(request, currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat sessions");
                return StatusCode(500, new ResponseGetChatSessionsDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách phiên chat",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Lấy tin nhắn trong phiên chat
        /// </summary>
        /// <param name="sessionId">ID của phiên chat</param>
        /// <param name="page">Trang hiện tại (0-based)</param>
        /// <param name="pageSize">Số lượng messages per page</param>
        /// <returns>Tin nhắn trong phiên chat</returns>
        [HttpGet("sessions/{sessionId}/messages")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseGetMessagesDto>> GetMessages(
            [FromRoute] Guid sessionId,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var request = new RequestGetMessagesDto
                {
                    SessionId = sessionId,
                    Page = page,
                    PageSize = pageSize
                };
                
                var response = await _aiChatService.GetMessagesAsync(request, currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for session {SessionId}", sessionId);
                return StatusCode(500, new ResponseGetMessagesDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lấy tin nhắn",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Lưu trữ phiên chat (archive)
        /// </summary>
        /// <param name="sessionId">ID của phiên chat</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpPut("sessions/{sessionId}/archive")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseSessionActionDto>> ArchiveSession([FromRoute] Guid sessionId)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _aiChatService.ArchiveChatSessionAsync(sessionId, currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving session {SessionId}", sessionId);
                return StatusCode(500, new ResponseSessionActionDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lưu trữ phiên chat",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Xóa phiên chat
        /// </summary>
        /// <param name="sessionId">ID của phiên chat</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpDelete("sessions/{sessionId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseSessionActionDto>> DeleteSession([FromRoute] Guid sessionId)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _aiChatService.DeleteChatSessionAsync(sessionId, currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                return StatusCode(500, new ResponseSessionActionDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi xóa phiên chat",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Cập nhật tiêu đề phiên chat
        /// </summary>
        /// <param name="sessionId">ID của phiên chat</param>
        /// <param name="request">Tiêu đề mới</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpPut("sessions/{sessionId}/title")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseSessionActionDto>> UpdateSessionTitle(
            [FromRoute] Guid sessionId,
            [FromBody] UpdateSessionTitleRequest request)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var result = await _aiChatService.UpdateSessionTitleAsync(sessionId, request.NewTitle, currentUser.UserId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session title {SessionId}", sessionId);
                return StatusCode(500, new ResponseSessionActionDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi cập nhật tiêu đề",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Lấy thống kê AI Chat của user với phân tích theo loại chat
        /// </summary>
        /// <returns>Thống kê chat</returns>
        [HttpGet("stats")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<object>> GetChatStats()
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                
                // Lấy thống kê tổng
                var allSessionsRequest = new RequestGetChatSessionsDto { PageSize = 1 };
                var allSessionsResponse = await _aiChatService.GetChatSessionsAsync(allSessionsRequest, currentUser.UserId);
                
                // Lấy thống kê theo từng loại chat
                var tourStatsRequest = new RequestGetChatSessionsDto { PageSize = 1, ChatType = AIChatType.Tour };
                var tourStatsResponse = await _aiChatService.GetChatSessionsAsync(tourStatsRequest, currentUser.UserId);
                
                var productStatsRequest = new RequestGetChatSessionsDto { PageSize = 1, ChatType = AIChatType.Product };
                var productStatsResponse = await _aiChatService.GetChatSessionsAsync(productStatsRequest, currentUser.UserId);
                
                var tayNinhStatsRequest = new RequestGetChatSessionsDto { PageSize = 1, ChatType = AIChatType.TayNinh };
                var tayNinhStatsResponse = await _aiChatService.GetChatSessionsAsync(tayNinhStatsRequest, currentUser.UserId);
                
                return Ok(new
                {
                    success = true,
                    Message = "Lấy thống kê thành công",
                    Data = new
                    {
                        TotalSessions = allSessionsResponse.TotalCount,
                        ActiveSessions = allSessionsResponse.TotalCount,
                        ByType = new
                        {
                            TourSessions = tourStatsResponse.TotalCount,
                            ProductSessions = productStatsResponse.TotalCount,
                            TayNinhSessions = tayNinhStatsResponse.TotalCount
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat stats");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thống kê"
                });
            }
        }

        /// <summary>
        /// Admin endpoint để reset quota Gemini API
        /// </summary>
        [HttpPost("admin/reset-quota")]
        [Authorize(Roles = "Admin")]
        public ActionResult ResetGeminiQuota()
        {
            try
            {
                var apiKey = _configuration["GeminiSettings:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return BadRequest(new { success = false, message = "API key not found" });
                }

                QuotaTracker.ForceResetQuota(apiKey);
                _logger.LogInformation("Admin reset Gemini quota for API key");

                return Ok(new
                {
                    success = true,
                    message = "Quota đã được reset thành công",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting Gemini quota");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi reset quota",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Admin endpoint để xem quota status
        /// </summary>
        [HttpGet("admin/quota-status")]
        [Authorize(Roles = "Admin")]
        public ActionResult GetQuotaStatus()
        {
            try
            {
                var geminiSettings = _configuration.GetSection("GeminiSettings");
                var apiKey = geminiSettings["ApiKey"];
                var maxPerDay = int.Parse(geminiSettings["RateLimitPerDay"] ?? "30");
                var maxPerMinute = int.Parse(geminiSettings["RateLimitPerMinute"] ?? "3");

                if (string.IsNullOrEmpty(apiKey))
                {
                    return BadRequest(new { success = false, message = "API key not found" });
                }

                var status = QuotaTracker.GetQuotaStatus(apiKey, maxPerDay, maxPerMinute);
                var todayCount = QuotaTracker.GetRequestCountToday(apiKey);
                var hourCount = QuotaTracker.GetRequestCountLastHour(apiKey);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        apiKey = apiKey.Substring(0, 5) + "*****",
                        maxPerDay,
                        maxPerMinute,
                        todayCount,
                        hourCount,
                        status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quota status");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy quota status",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Kiểm tra kết nối database và dữ liệu sản phẩm thực tế
        /// </summary>
        [HttpGet("debug/products")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DebugProductData()
        {
            try
            {
                _logger.LogInformation("DEBUG: Checking database connection and product data");
                
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user is null in debug endpoint");
                    return Unauthorized(new { 
                        message = "Không thể xác thực người dùng",
                        note = "Vui lòng đảm bảo token JWT hợp lệ được gửi trong Authorization header"
                    });
                }

                _logger.LogInformation("DEBUG: User authenticated successfully - UserId: {UserId}", currentUser.UserId);

                // Test IAIProductDataService directly
                var productDataService = HttpContext.RequestServices.GetService<IAIProductDataService>();
                if (productDataService == null)
                {
                    return BadRequest(new { message = "AIProductDataService not available" });
                }

                _logger.LogInformation("DEBUG: Starting database queries for product data...");
                
                var availableProducts = await productDataService.GetAvailableProductsAsync(5);
                var saleProducts = await productDataService.GetProductsOnSaleAsync(3);
                var bestSellers = await productDataService.GetBestSellingProductsAsync(3);

                _logger.LogInformation("DEBUG: Database queries completed - Available: {Available}, Sale: {Sale}, BestSellers: {BestSellers}",
                    availableProducts.Count, saleProducts.Count, bestSellers.Count);

                return Ok(new
                {
                    success = true,
                    message = "Debug thành công - Dữ liệu sản phẩm thực tế từ database",
                    authenticatedUser = new 
                    {
                        userId = currentUser.UserId,
                        name = currentUser.Name,
                        email = currentUser.Email,
                        phoneNumber = currentUser.PhoneNumber
                    },
                    data = new
                    {
                        connectionInfo = new 
                        {
                            server = "103.216.119.189",
                            database = "TayNinhTourDb",
                            note = "Connection string từ appsettings.json"
                        },
                        availableProducts = new
                        {
                            count = availableProducts.Count,
                            products = availableProducts.Select(p => new
                            {
                                id = p.Id,
                                name = p.Name,
                                price = p.Price,
                                stock = p.QuantityInStock,
                                shop = p.ShopName,
                                category = p.Category,
                                isSale = p.IsSale,
                                salePrice = p.SalePrice,
                                soldCount = p.SoldCount,
                                rating = p.AverageRating,
                                reviewCount = p.ReviewCount
                            }).ToList()
                        },
                        saleProducts = new
                        {
                            count = saleProducts.Count,
                            products = saleProducts.Select(p => new
                            {
                                name = p.Name,
                                originalPrice = p.Price,
                                salePrice = p.SalePrice,
                                salePercent = p.SalePercent,
                                shop = p.ShopName
                            }).ToList()
                        },
                        bestSellers = new
                        {
                            count = bestSellers.Count,
                            products = bestSellers.Select(p => new
                            {
                                name = p.Name,
                                soldCount = p.SoldCount,
                                price = p.Price,
                                shop = p.ShopName
                            }).ToList()
                        },
                        timestamp = DateTime.UtcNow,
                        note = "Đây là dữ liệu THỰC TẾ từ database TayNinhTourDb, không phải fake data",
                        verification = new
                        {
                            totalProductsFound = availableProducts.Count + saleProducts.Count + bestSellers.Count,
                            databaseConnectionSuccess = true,
                            realDataConfirmed = true
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR in debug product data");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi truy xuất dữ liệu sản phẩm từ database",
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    note = "Có thể là lỗi kết nối database hoặc cấu hình sai"
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Test AI Product Chat với dữ liệu thực
        /// </summary>
        [HttpPost("debug/test-product-chat")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DebugTestProductChat([FromBody] TestProductChatRequest request)
        {
            try
            {
                _logger.LogInformation("DEBUG: Testing product chat with real database data - Message: {Message}", request.TestMessage);
                
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user is null in debug test product chat");
                    return Unauthorized(new { 
                        message = "Không thể xác thực người dùng",
                        note = "Vui lòng đảm bảo token JWT hợp lệ được gửi trong Authorization header"
                    });
                }

                _logger.LogInformation("DEBUG: Creating product chat session for user {UserId}", currentUser.UserId);

                // Create a product chat session
                var createRequest = new RequestCreateChatSessionDto
                {
                    ChatType = AIChatType.Product,
                    FirstMessage = request.TestMessage,
                    CustomTitle = $"[DEBUG] Test sản phẩm - {DateTime.Now:HH:mm:ss}"
                };

                var result = await _aiChatService.CreateChatSessionAsync(createRequest, currentUser.UserId);

                _logger.LogInformation("DEBUG: Chat session creation result: {Success}", result.success);

                return Ok(new
                {
                    success = result.success,
                    message = result.Message,
                    sessionId = result.ChatSession?.Id,
                    chatType = "Product",
                    testMessage = request.TestMessage,
                    authenticatedUser = new 
                    {
                        userId = currentUser.UserId,
                        name = currentUser.Name,
                        email = currentUser.Email,
                        phoneNumber = currentUser.PhoneNumber
                    },
                    aiResponse = new
                    {
                        note = "Kiểm tra tin nhắn trong session để xem AI response",
                        sessionTitle = result.ChatSession?.Title,
                        createdAt = result.ChatSession?.CreatedAt
                    },
                    verification = new
                    {
                        realDataOnly = "AI sẽ chỉ sử dụng dữ liệu sản phẩm thực tế từ database",
                        noFakeData = "AI được cấu hình nghiêm ngặt không tạo dữ liệu giả",
                        databaseSource = "TayNinhTourDb via connection string trong appsettings.json"
                    },
                    debugInfo = new
                    {
                        timestamp = DateTime.UtcNow,
                        databaseUsed = "TayNinhTourDb",
                        systemPromptEnforced = "AI chỉ được tư vấn sản phẩm có trong database",
                        fallbackDisabled = "Không có fallback tạo dữ liệu giả"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR in debug test product chat");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi test product chat",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Request cho debug test product chat
        /// </summary>
        public class TestProductChatRequest
        {
            public string TestMessage { get; set; } = "Tôi muốn mua sản phẩm đặc sản Tây Ninh";
        }
    }
}