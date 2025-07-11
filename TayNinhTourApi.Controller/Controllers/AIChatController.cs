using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.AIChat;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AIChat;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller qu?n l� AI Chat - chat v?i AI chatbot s? d?ng Gemini API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AIChatController : ControllerBase
    {
        private readonly IAIChatService _aiChatService;
        private readonly IGeminiAIService _geminiAIService;
        private readonly ILogger<AIChatController> _logger;

        public AIChatController(
            IAIChatService aiChatService,
            IGeminiAIService geminiAIService,
            ILogger<AIChatController> logger)
        {
            _aiChatService = aiChatService;
            _geminiAIService = geminiAIService;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint ?? ki?m tra Gemini API (kh�ng c?n authentication)
        /// </summary>
        /// <param name="message">Tin nh?n test</param>
        /// <returns>Ph?n h?i t? Gemini AI</returns>
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
                    message = response.Success ? "Gemini API test th�nh c�ng" : "Gemini API test th?t b?i",
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
                    message = "C� l?i x?y ra khi test Gemini API",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// T?o phi�n chat m?i
        /// </summary>
        /// <param name="request">Th�ng tin phi�n chat m?i</param>
        /// <returns>K?t qu? t?o phi�n chat</returns>
        [HttpPost("sessions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseCreateChatSessionDto>> CreateChatSession([FromBody] RequestCreateChatSessionDto request)
        {
            try
            {
                _logger.LogInformation("Creating chat session, getting current user...");
                
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user is null");
                    return StatusCode(401, new ResponseCreateChatSessionDto
                    {
                        success = false,
                        Message = "Kh�ng th? x�c th?c ng??i d�ng",
                        StatusCode = 401
                    });
                }

                _logger.LogInformation("Current user ID: {UserId}, Email: {Email}", currentUser.UserId, currentUser.Email);
                
                var response = await _aiChatService.CreateChatSessionAsync(request, currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat session. Stack trace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new ResponseCreateChatSessionDto
                {
                    success = false,
                    Message = $"C� l?i x?y ra khi t?o phi�n chat: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// G?i tin nh?n ??n AI chatbot
        /// </summary>
        /// <param name="request">Th�ng tin tin nh?n</param>
        /// <returns>Ph?n h?i t? AI</returns>
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
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new ResponseSendMessageDto
                {
                    success = false,
                    Message = "C� l?i x?y ra khi g?i tin nh?n",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// L?y danh s�ch phi�n chat c?a user hi?n t?i
        /// </summary>
        /// <param name="page">Trang hi?n t?i (0-based)</param>
        /// <param name="pageSize">S? l??ng sessions per page</param>
        /// <param name="status">Tr?ng th�i session (Active, Archived, All)</param>
        /// <returns>Danh s�ch phi�n chat</returns>
        [HttpGet("sessions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseGetChatSessionsDto>> GetChatSessions(
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 20,
            [FromQuery] string status = "Active")
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var request = new RequestGetChatSessionsDto
                {
                    Page = page,
                    PageSize = pageSize,
                    Status = status
                };
                
                var response = await _aiChatService.GetChatSessionsAsync(request, currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat sessions");
                return StatusCode(500, new ResponseGetChatSessionsDto
                {
                    success = false,
                    Message = "C� l?i x?y ra khi l?y danh s�ch phi�n chat",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// L?y tin nh?n trong phi�n chat
        /// </summary>
        /// <param name="sessionId">ID c?a phi�n chat</param>
        /// <param name="page">Trang hi?n t?i (0-based)</param>
        /// <param name="pageSize">S? l??ng messages per page</param>
        /// <returns>Tin nh?n trong phi�n chat</returns>
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
                    Message = "C� l?i x?y ra khi l?y tin nh?n",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// L?u tr? phi�n chat (archive)
        /// </summary>
        /// <param name="sessionId">ID c?a phi�n chat</param>
        /// <returns>K?t qu? thao t�c</returns>
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
                    Message = "C� l?i x?y ra khi l?u tr? phi�n chat",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// X�a phi�n chat
        /// </summary>
        /// <param name="sessionId">ID c?a phi�n chat</param>
        /// <returns>K?t qu? thao t�c</returns>
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
                    Message = "C� l?i x?y ra khi x�a phi�n chat",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// C?p nh?t ti�u ?? phi�n chat
        /// </summary>
        /// <param name="sessionId">ID c?a phi�n chat</param>
        /// <param name="request">Ti�u ?? m?i</param>
        /// <returns>K?t qu? thao t�c</returns>
        [HttpPut("sessions/{sessionId}/title")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseSessionActionDto>> UpdateSessionTitle(
            [FromRoute] Guid sessionId,
            [FromBody] UpdateSessionTitleRequest request)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _aiChatService.UpdateSessionTitleAsync(sessionId, request.Title, currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session title {SessionId}", sessionId);
                return StatusCode(500, new ResponseSessionActionDto
                {
                    success = false,
                    Message = "C� l?i x?y ra khi c?p nh?t ti�u ??",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// L?y th?ng k� AI Chat c?a user
        /// </summary>
        /// <returns>Th?ng k� chat</returns>
        [HttpGet("stats")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<object>> GetChatStats()
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                
                // L?y th?ng k� c? b?n
                var sessionsRequest = new RequestGetChatSessionsDto { PageSize = 1 };
                var sessionsResponse = await _aiChatService.GetChatSessionsAsync(sessionsRequest, currentUser.UserId);
                
                return Ok(new
                {
                    success = true,
                    Message = "L?y th?ng k� th�nh c�ng",
                    Data = new
                    {
                        TotalSessions = sessionsResponse.TotalCount,
                        ActiveSessions = sessionsResponse.TotalCount // C� th? m? r?ng ?? ??m ri�ng active sessions
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat stats");
                return StatusCode(500, new
                {
                    success = false,
                    Message = "C� l?i x?y ra khi l?y th?ng k�"
                });
            }
        }
    }

    /// <summary>
    /// Request DTO ?? c?p nh?t ti�u ?? session
    /// </summary>
    public class UpdateSessionTitleRequest
    {
        public string Title { get; set; } = null!;
    }

    /// <summary>
    /// Request DTO ?? test Gemini API
    /// </summary>
    public class TestGeminiRequest
    {
        public string Message { get; set; } = null!;
    }
}