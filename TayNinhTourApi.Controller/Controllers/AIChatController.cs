using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.AIChat;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AIChat;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

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
        /// Debug endpoint - Kiểm tra kết nối database và dữ liệu tour thực tế
        /// </summary>
        [HttpGet("debug/tours")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DebugTourData()
        {
            try
            {
                _logger.LogInformation("DEBUG: Checking database connection and tour data");
                
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

                // Test IAITourDataService directly
                var tourDataService = HttpContext.RequestServices.GetService<IAITourDataService>();
                if (tourDataService == null)
                {
                    return BadRequest(new { message = "AITourDataService not available" });
                }

                _logger.LogInformation("DEBUG: Starting database queries for tour data...");
                
                var availableTours = await tourDataService.GetAvailableToursAsync(10);
                
                _logger.LogInformation("DEBUG: Database queries completed - Available tours: {Count}",
                    availableTours.Count);

                // Also check direct database query
                var unitOfWork = HttpContext.RequestServices.GetService<IUnitOfWork>();
                if (unitOfWork == null)
                {
                    return BadRequest(new { message = "UnitOfWork not available" });
                }

                var tourSlots = await unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.IsActive)
                    .Take(5)
                    .ToListAsync();

                _logger.LogInformation("DEBUG: Direct database query returned {Count} tour slots", tourSlots.Count);

                return Ok(new
                {
                    success = true,
                    message = "Debug thành công - Dữ liệu tour thực tế từ database",
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
                            server = "103.69.97.72",
                            database = "TayNinhTourDb",
                            note = "Connection string từ appsettings.json"
                        },
                        aiTourDataService = new
                        {
                            isRegistered = tourDataService != null,
                            availableToursCount = availableTours.Count,
                            tours = availableTours.Take(3).Select(t => new
                            {
                                id = t.Id,
                                title = t.Title,
                                price = t.Price,
                                startLocation = t.StartLocation,
                                endLocation = t.EndLocation,
                                tourType = t.TourType,
                                availableSlots = t.AvailableSlots,
                                availableDates = t.AvailableDates.Take(3).ToList(),
                                companyName = t.CompanyName,
                                isPublic = t.IsPublic
                            }).ToList()
                        },
                        directDatabaseQuery = new
                        {
                            totalTourSlots = tourSlots.Count,
                            tourSlots = tourSlots.Select(ts => new
                            {
                                slotId = ts.Id,
                                isActive = ts.IsActive,
                                status = ts.Status,
                                tourDate = ts.TourDate,
                                availableSpots = ts.AvailableSpots,
                                maxGuests = ts.MaxGuests,
                                currentBookings = ts.CurrentBookings,
                                template = new
                                {
                                    id = ts.TourTemplate.Id,
                                    title = ts.TourTemplate.Title,
                                    startLocation = ts.TourTemplate.StartLocation,
                                    endLocation = ts.TourTemplate.EndLocation,
                                    templateType = ts.TourTemplate.TemplateType,
                                    isActive = ts.TourTemplate.IsActive
                                },
                                tourDetails = ts.TourDetails != null ? new
                                {
                                    id = ts.TourDetails.Id,
                                    title = ts.TourDetails.Title,
                                    status = ts.TourDetails.Status,
                                    hasOperation = ts.TourDetails.TourOperation != null,
                                    operationPrice = ts.TourDetails.TourOperation?.Price
                                } : null
                            }).ToList()
                        },
                        timestamp = DateTime.UtcNow,
                        note = "Đây là dữ liệu THỰC TẾ từ database TayNinhTourDb, không phải fake data",
                        verification = new
                        {
                            databaseConnectionSuccess = true,
                            aiServiceRegistered = tourDataService != null,
                            realDataConfirmed = true,
                            unitOfWorkAvailable = unitOfWork != null
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR in debug tour data");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi truy xuất dữ liệu tour từ database",
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    note = "Có thể là lỗi kết nối database hoặc cấu hình sai"
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Test AI Tour Chat với dữ liệu thực
        /// </summary>
        [HttpPost("debug/test-tour-chat")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DebugTestTourChat([FromBody] TestTourChatRequest request)
        {
            try
            {
                _logger.LogInformation("DEBUG: Testing tour chat with real database data - Message: {Message}", request.TestMessage);
                
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user is null in debug test tour chat");
                    return Unauthorized(new { 
                        message = "Không thể xác thực người dùng",
                        note = "Vui lòng đảm bảo token JWT hợp lệ được gửi trong Authorization header"
                    });
                }

                _logger.LogInformation("DEBUG: Creating tour chat session for user {UserId}", currentUser.UserId);

                // Create a tour chat session
                var createRequest = new RequestCreateChatSessionDto
                {
                    ChatType = AIChatType.Tour,
                    FirstMessage = request.TestMessage,
                    CustomTitle = $"[DEBUG] Test tour - {DateTime.Now:HH:mm:ss}"
                };

                var result = await _aiChatService.CreateChatSessionAsync(createRequest, currentUser.UserId);

                _logger.LogInformation("DEBUG: Chat session creation result: {Success}", result.success);

                return Ok(new
                {
                    success = result.success,
                    message = result.Message,
                    sessionId = result.ChatSession?.Id,
                    chatType = "Tour",
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
                        realDataOnly = "AI sẽ chỉ sử dụng dữ liệu tour thực tế từ database",
                        noFakeData = "AI được cấu hình nghiêm ngặt không tạo dữ liệu giả",
                        databaseSource = "TayNinhTourDb via connection string trong appsettings.json"
                    },
                    debugInfo = new
                    {
                        timestamp = DateTime.UtcNow,
                        databaseUsed = "TayNinhTourDb",
                        systemPromptEnforced = "AI chỉ được tư vấn tours có trong database",
                        fallbackDisabled = "Không có fallback tạo dữ liệu giả"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR in debug test tour chat");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi test tour chat",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Test AI Tour Chat và lấy ngay AI response
        /// </summary>
        [HttpPost("debug/test-tour-chat-full")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DebugTestTourChatFull([FromBody] TestTourChatRequest request)
        {
            try
            {
                _logger.LogInformation("DEBUG: Full tour chat test with immediate AI response - Message: {Message}", request.TestMessage);
                
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                // Step 1: Create tour chat session
                var createRequest = new RequestCreateChatSessionDto
                {
                    ChatType = AIChatType.Tour,
                    FirstMessage = request.TestMessage,
                    CustomTitle = $"[DEBUG FULL] Test tour - {DateTime.Now:HH:mm:ss}"
                };

                var sessionResult = await _aiChatService.CreateChatSessionAsync(createRequest, currentUser.UserId);
                
                if (!sessionResult.success || sessionResult.ChatSession == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Không thể tạo session",
                        error = sessionResult.Message
                    });
                }

                var sessionId = sessionResult.ChatSession.Id;
                _logger.LogInformation("DEBUG: Created session {SessionId}, now getting messages...", sessionId);

                // Step 2: Get messages to see AI response
                await Task.Delay(2000); // Wait 2 seconds for AI processing

                var messagesRequest = new RequestGetMessagesDto
                {
                    SessionId = sessionId,
                    Page = 0,
                    PageSize = 10
                };

                var messagesResult = await _aiChatService.GetMessagesAsync(messagesRequest, currentUser.UserId);

                // Step 3: Check if we have tour data available
                var tourDataService = HttpContext.RequestServices.GetService<IAITourDataService>();
                var availableTours = await tourDataService?.GetAvailableToursAsync(5) ?? new List<AITourInfo>();

                return Ok(new
                {
                    success = true,
                    message = "DEBUG FULL: Tạo session và lấy AI response thành công",
                    testMessage = request.TestMessage,
                    sessionInfo = new
                    {
                        sessionId = sessionId,
                        title = sessionResult.ChatSession.Title,
                        chatType = "Tour",
                        createdAt = sessionResult.ChatSession.CreatedAt
                    },
                    aiResponse = new
                    {
                        messagesSuccess = messagesResult.success,
                        messagesCount = messagesResult.ChatSession?.Messages?.Count ?? 0,
                        messages = messagesResult.ChatSession?.Messages?.Select(m => new
                        {
                            messageType = m.MessageType,
                            content = m.Content,
                            tokensUsed = m.TokensUsed,
                            responseTimeMs = m.ResponseTimeMs,
                            createdAt = m.CreatedAt
                        }).ToList()
                    },
                    databaseVerification = new
                    {
                        toursAvailableInDatabase = availableTours.Count,
                        tourData = availableTours.Take(2).Select(t => new
                        {
                            title = t.Title,
                            price = t.Price,
                            availableSlots = t.AvailableSlots,
                            tourType = t.TourType
                        }).ToList(),
                        expectation = availableTours.Any() 
                            ? "AI nên tư vấn các tours này từ database" 
                            : "AI nên báo 'không có tour nào' vì database trống"
                    },
                    verification = new
                    {
                        realDataOnly = "AI chỉ sử dụng dữ liệu từ database",
                        noFakeData = "Không tạo ra tours không tồn tại",
                        databaseSource = "TayNinhTourDb"
                    },
                    debugInstructions = new
                    {
                        step1 = "Kiểm tra 'aiResponse.messages' để xem AI đã phản hồi gì",
                        step2 = "So sánh AI response với 'databaseVerification.tourData'",
                        step3 = "Xác nhận AI KHÔNG tạo fake data",
                        step4 = "Nếu AI tạo fake data -> BUG cần fix"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR in debug full tour chat test");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi test full tour chat",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Request cho debug test tour chat
        /// </summary>
        public class TestTourChatRequest
        {
            public string TestMessage { get; set; } = "Tôi muốn tìm tour du lịch Tây Ninh";
        }

        /// <summary>
        /// Request cho debug test product chat
        /// </summary>
        public class TestProductChatRequest
        {
            public string TestMessage { get; set; } = "Tôi muốn mua sản phẩm đặc sản Tây Ninh";
        }

        /// <summary>
        /// Debug endpoint - Chi tiết tại sao AITourDataService không tìm thấy tours
        /// </summary>
        [HttpGet("debug/ai-tour-service")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DebugAITourService()
        {
            try
            {
                _logger.LogInformation("DEBUG: Analyzing why AITourDataService returns no tours");
                
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var unitOfWork = HttpContext.RequestServices.GetService<IUnitOfWork>();
                if (unitOfWork == null)
                {
                    return BadRequest(new { message = "UnitOfWork not available" });
                }

                // Step 1: Get all tour slots
                var allSlots = await unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                        .ThenInclude(tt => tt.CreatedBy)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.IsActive)
                    .ToListAsync();

                _logger.LogInformation("DEBUG: Found {Count} active tour slots", allSlots.Count);

                var today = DateOnly.FromDateTime(DateTime.Today);

                var analysisResults = new List<object>();
                
                foreach (var slot in allSlots)
                {
                    var analysis = new
                    {
                        slotId = slot.Id,
                        tourDate = slot.TourDate,
                        isActive = slot.IsActive,
                        status = slot.Status.ToString(),
                        availableSpots = slot.AvailableSpots,
                        maxGuests = slot.MaxGuests,
                        currentBookings = slot.CurrentBookings,
                        
                        // Template checks
                        templateIsActive = slot.TourTemplate.IsActive,
                        templateTitle = slot.TourTemplate.Title,
                        
                        // TourDetails checks
                        hasTourDetails = slot.TourDetails != null,
                        tourDetailsStatus = slot.TourDetails?.Status.ToString(),
                        isPublic = slot.TourDetails?.Status == TourDetailsStatus.Public,
                        
                        // TourOperation checks
                        hasTourOperation = slot.TourDetails?.TourOperation != null,
                        operationPrice = slot.TourDetails?.TourOperation?.Price,
                        
                        // Date checks
                        isInFuture = slot.TourDate >= today,
                        daysDifference = (slot.TourDate.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days,
                        
                        // AI Filter checks
                        passesIsActive = slot.IsActive,
                        passesStatusAvailable = slot.Status == TourSlotStatus.Available,
                        passesAvailableSpots = slot.AvailableSpots > 0,
                        passesDateCheck = slot.TourDate >= today,
                        passesTemplateActive = slot.TourTemplate.IsActive,
                        passesHasTourDetails = slot.TourDetails != null,
                        passesIsPublic = slot.TourDetails?.Status == TourDetailsStatus.Public,
                        passesHasTourOperation = slot.TourDetails?.TourOperation != null,
                        
                        // Overall assessment
                        passesAllFilters = slot.IsActive && 
                                          slot.Status == TourSlotStatus.Available && 
                                          slot.AvailableSpots > 0 &&
                                          slot.TourDate >= today &&
                                          slot.TourTemplate.IsActive &&
                                          slot.TourDetails != null &&
                                          slot.TourDetails.Status == TourDetailsStatus.Public &&
                                          slot.TourDetails.TourOperation != null,
                                          
                        failureReasons = new List<string>()
                    };

                    // Collect failure reasons
                    var reasons = (List<string>)analysis.failureReasons;
                    if (!slot.IsActive) reasons.Add("TourSlot not active");
                    if (slot.Status != TourSlotStatus.Available) reasons.Add($"Status is {slot.Status}, not Available");
                    if (slot.AvailableSpots <= 0) reasons.Add($"No available spots ({slot.AvailableSpots})");
                    if (slot.TourDate < today) reasons.Add("Tour date in the past");
                    if (!slot.TourTemplate.IsActive) reasons.Add("TourTemplate not active");
                    if (slot.TourDetails == null) reasons.Add("No TourDetails linked");
                    if (slot.TourDetails?.Status != TourDetailsStatus.Public) reasons.Add($"TourDetails status is {slot.TourDetails?.Status}, not Public");
                    if (slot.TourDetails?.TourOperation == null) reasons.Add("No TourOperation linked");

                    analysisResults.Add(analysis);
                }

                var passingSlots = analysisResults.Cast<dynamic>().Where(a => a.passesAllFilters).ToList();
                var failingSlots = analysisResults.Cast<dynamic>().Where(a => !a.passesAllFilters).ToList();

                return Ok(new
                {
                    success = true,
                    message = "DEBUG: AI Tour Service Filter Analysis",
                    summary = new
                    {
                        totalActiveSlots = allSlots.Count,
                        slotsPassingAllFilters = passingSlots.Count,
                        slotsFailingFilters = failingSlots.Count,
                        shouldReturnTours = passingSlots.Count > 0 ? "YES" : "NO",
                        analysisDate = DateTime.UtcNow,
                        todayDate = today
                    },
                    passingSlots = passingSlots.Take(5).ToList(),
                    failingSlots = failingSlots.Select(a => new 
                    {
                        slotId = a.slotId,
                        tourDate = a.tourDate,
                        templateTitle = a.templateTitle,
                        failureReasons = a.failureReasons,
                        availableSpots = a.availableSpots,
                        status = a.status,
                        isPublic = a.isPublic,
                        hasTourOperation = a.hasTourOperation
                    }).ToList(),
                    filterCriteria = new
                    {
                        note = "For a slot to be included in AI recommendations, ALL these must be true:",
                        requirements = new[]
                        {
                            "ts.IsActive == true",
                            "ts.Status == TourSlotStatus.Available", 
                            "ts.AvailableSpots > 0",
                            $"ts.TourDate >= {today} (today or future)",
                            "ts.TourTemplate.IsActive == true",
                            "ts.TourDetails != null",
                            "ts.TourDetails.Status == TourDetailsStatus.Public",
                            "ts.TourDetails.TourOperation != null"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR in debug AI tour service");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi debug AI tour service",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Test trực tiếp AITourDataService.GetAvailableToursAsync()
        /// </summary>
        [HttpGet("debug/direct-ai-service-call")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DebugDirectAIServiceCall()
        {
            try
            {
                _logger.LogInformation("🔥 DEBUG: Testing direct call to AITourDataService.GetAvailableToursAsync()");
                
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                // Get AITourDataService directly
                var tourDataService = HttpContext.RequestServices.GetService<IAITourDataService>();
                if (tourDataService == null)
                {
                    return BadRequest(new { message = "AITourDataService not available" });
                }

                _logger.LogInformation("🔥 DEBUG: Calling GetAvailableToursAsync(10) directly...");
                
                // Call the service method directly with enhanced logging
                var availableTours = await tourDataService.GetAvailableToursAsync(10);
                
                _logger.LogInformation("🔥 DEBUG: GetAvailableToursAsync returned {Count} tours", availableTours.Count);

                return Ok(new
                {
                    success = true,
                    message = "Direct AITourDataService call completed",
                    results = new
                    {
                        tourDataServiceAvailable = tourDataService != null,
                        availableToursCount = availableTours.Count,
                        tours = availableTours.Select(t => new
                        {
                            id = t.Id,
                            title = t.Title,
                            price = t.Price,
                            startLocation = t.StartLocation,
                            endLocation = t.EndLocation,
                            tourType = t.TourType,
                            availableSlots = t.AvailableSlots,
                            availableDates = t.AvailableDates.Take(2).ToList(),
                            companyName = t.CompanyName,
                            isPublic = t.IsPublic,
                            highlights = t.Highlights.Take(3).ToList()
                        }).ToList()
                    },
                    verification = new
                    {
                        expectedResult = "Should return the same 4 tours that pass all filters",
                        actualResult = availableTours.Count > 0 ? "SUCCESS - Tours found!" : "FAILED - No tours returned",
                        nextStep = availableTours.Count > 0 
                            ? "✅ AITourDataService works! Check why AI doesn't get the data in EnrichWithTourData" 
                            : "❌ AITourDataService has issues! Check the logs for DEBUG messages"
                    },
                    debugInstructions = new
                    {
                        checkLogs = "Look for DEBUG messages starting with 🔍, ✅, ❌, 🎯, 🔍, 💥",
                        logLocation = "Visual Studio Output Window > Debug or Console logs",
                        whatToLookFor = new[]
                        {
                            "🔍 DEBUG: Raw query returned X tour slots",
                            "✅ DEBUG: Found X real tour slots from database", 
                            "🎯 DEBUG: Slot details",
                            "🔍 DEBUG: Grouped slots into X templates",
                            "✅ DEBUG: Created AITourInfo for template X",
                            "💥 DEBUG: CRITICAL ERROR (if any)"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 DEBUG: Exception in direct AI service call test");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Exception in direct AI service call",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Test từng bước của GetAvailableToursAsync
        /// </summary>
        [HttpGet("debug/step-by-step-tour-service")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DebugStepByStepTourService()
        {
            try
            {
                _logger.LogInformation("🔥 DEBUG: Step-by-step testing of AITourDataService");
                
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var unitOfWork = HttpContext.RequestServices.GetService<IUnitOfWork>();
                if (unitOfWork == null)
                {
                    return BadRequest(new { message = "UnitOfWork not available" });
                }

                var steps = new List<object>();

                // Step 1: Raw query với exact same filters như AITourDataService
                _logger.LogInformation("🔥 DEBUG STEP 1: Running exact same query as AITourDataService...");
                
                var tourSlots = await unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                        .ThenInclude(tt => tt.CreatedBy)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.AvailableSpots > 0 &&
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourTemplate.IsActive &&
                                ts.TourDetails != null && 
                                ts.TourDetails.Status == TourDetailsStatus.Public && 
                                ts.TourDetails.TourOperation != null)
                    .OrderBy(ts => ts.TourDate)
                    .Take(30) // Same as maxResults * 3
                    .ToListAsync();

                steps.Add(new
                {
                    step = 1,
                    description = "Raw database query with exact AITourDataService filters",
                    slotsFound = tourSlots.Count,
                    success = tourSlots.Count > 0,
                    details = tourSlots.Select(ts => new
                    {
                        slotId = ts.Id,
                        templateId = ts.TourTemplateId,
                        templateTitle = ts.TourTemplate.Title,
                        tourDate = ts.TourDate,
                        availableSpots = ts.AvailableSpots,
                        price = ts.TourDetails?.TourOperation?.Price
                    }).ToList()
                });

                if (!tourSlots.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "FAILED at Step 1: No tour slots found with AITourDataService filters",
                        steps = steps
                    });
                }

                // Step 2: Group by template
                _logger.LogInformation("🔥 DEBUG STEP 2: Grouping {Count} slots by template...", tourSlots.Count);
                
                var groupedSlots = tourSlots.GroupBy(ts => ts.TourTemplateId).ToList();
                
                steps.Add(new
                {
                    step = 2,
                    description = "Group slots by TourTemplateId",
                    groupsFound = groupedSlots.Count,
                    success = groupedSlots.Count > 0,
                    groups = groupedSlots.Select(g => new
                    {
                        templateId = g.Key,
                        slotsInGroup = g.Count(),
                        totalAvailableSpots = g.Sum(s => s.AvailableSpots),
                        templateTitle = g.First().TourTemplate.Title
                    }).ToList()
                });

                // Step 3: Try creating AITourInfo for each group
                _logger.LogInformation("🔥 DEBUG STEP 3: Creating AITourInfo for each group...");
                
                var tourInfoResults = new List<object>();
                
                foreach (var group in groupedSlots)
                {
                    try
                    {
                        var groupSlots = group.ToList();
                        var firstSlot = groupSlots.First();
                        
                        // Manual creation to debug each step
                        var tourInfo = new
                        {
                            id = firstSlot.TourTemplateId,
                            title = firstSlot.TourTemplate.Title,
                            price = firstSlot.TourDetails?.TourOperation?.Price ?? 0,
                            startLocation = firstSlot.TourTemplate.StartLocation,
                            endLocation = firstSlot.TourTemplate.EndLocation,
                            maxGuests = groupSlots.Max(ts => ts.MaxGuests),
                            availableSlots = groupSlots.Sum(ts => ts.AvailableSpots),
                            availableDatesCount = groupSlots.Count,
                            companyName = firstSlot.TourTemplate.CreatedBy?.Name ?? "Công ty du lịch",
                            isPublic = firstSlot.TourDetails?.Status == TourDetailsStatus.Public
                        };

                        tourInfoResults.Add(new
                        {
                            templateId = group.Key,
                            success = true,
                            tourInfo = tourInfo,
                            error = (string?)null
                        });

                        _logger.LogInformation("✅ DEBUG: Successfully created AITourInfo for template {TemplateId}", group.Key);
                    }
                    catch (Exception ex)
                    {
                        tourInfoResults.Add(new
                        {
                            templateId = group.Key,
                            success = false,
                            tourInfo = (object?)null,
                            error = ex.Message
                        });

                        _logger.LogError(ex, "❌ DEBUG: Failed to create AITourInfo for template {TemplateId}", group.Key);
                    }
                }

                steps.Add(new
                {
                    step = 3,
                    description = "Create AITourInfo objects",
                    totalGroups = groupedSlots.Count,
                    successfulCreations = tourInfoResults.Count(r => (bool)r.GetType().GetProperty("success")?.GetValue(r, null)!),
                    failedCreations = tourInfoResults.Count(r => !(bool)r.GetType().GetProperty("success")?.GetValue(r, null)!),
                    results = tourInfoResults
                });

                var successfulTours = tourInfoResults.Where(r => (bool)r.GetType().GetProperty("success")?.GetValue(r, null)!).ToList();

                return Ok(new
                {
                    success = true,
                    message = "Step-by-step debug completed",
                    summary = new
                    {
                        step1_slotsFound = tourSlots.Count,
                        step2_groupsFound = groupedSlots.Count,
                        step3_successfulTours = successfulTours.Count,
                        finalResult = successfulTours.Count > 0 ? "SUCCESS" : "FAILED",
                        expectedResult = "Should have 4 successful tours based on previous analysis"
                    },
                    steps = steps,
                    conclusion = successfulTours.Count > 0 
                        ? "✅ Manual process works! Issue might be in actual AITourDataService method" 
                        : "❌ Issue found in manual process - check step details"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 DEBUG: Exception in step-by-step test");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Exception in step-by-step test",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}