using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.AIChat;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AIChat;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho AI Chat functionality với hỗ trợ 3 loại chat
    /// </summary>
    public class AIChatService : IAIChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAISpecializedChatService _specializedChatService;
        private readonly IGeminiAIService _geminiAIService;
        private readonly ILogger<AIChatService> _logger;

        public AIChatService(
            IUnitOfWork unitOfWork,
            IAISpecializedChatService specializedChatService,
            IGeminiAIService geminiAIService,
            ILogger<AIChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _specializedChatService = specializedChatService;
            _geminiAIService = geminiAIService;
            _logger = logger;
        }

        public async Task<ResponseCreateChatSessionDto> CreateChatSessionAsync(RequestCreateChatSessionDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating new {ChatType} chat session for user {UserId}", request.ChatType, userId);

                // Tạo session mới với ChatType
                var session = new AIChatSession
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = request.CustomTitle ?? GetDefaultTitle(request.ChatType),
                    Status = "Active",
                    ChatType = request.ChatType,
                    LastMessageAt = DateTime.UtcNow,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.AIChatSessionRepository.AddAsync(session);

                // Nếu có tin nhắn đầu tiên, xử lý luôn
                if (!string.IsNullOrWhiteSpace(request.FirstMessage))
                {
                    // Tạo tin nhắn từ user
                    var userMessage = new AIChatMessage
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        Content = request.FirstMessage,
                        MessageType = "User",
                        CreatedById = userId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };

                    await _unitOfWork.AIChatMessageRepository.AddAsync(userMessage);

                    // Gửi đến specialized AI service
                    var aiResponse = await _specializedChatService.ProcessMessageAsync(
                        request.FirstMessage, 
                        request.ChatType, 
                        null);

                    if (aiResponse.Success)
                    {
                        // Tạo tin nhắn phản hồi từ AI
                        var aiMessage = new AIChatMessage
                        {
                            Id = Guid.NewGuid(),
                            SessionId = session.Id,
                            Content = aiResponse.Content,
                            MessageType = "AI",
                            TokensUsed = aiResponse.TokensUsed,
                            ResponseTimeMs = aiResponse.ResponseTimeMs,
                            Metadata = aiResponse.IsFallback ? "{\"isFallback\": true}" : null,
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false
                        };

                        await _unitOfWork.AIChatMessageRepository.AddAsync(aiMessage);

                        // Tạo tiêu đề tự động từ tin nhắn đầu tiên nếu cần
                        if (string.IsNullOrEmpty(request.CustomTitle))
                        {
                            var generatedTitle = await _geminiAIService.GenerateTitleAsync(request.FirstMessage);
                            session.Title = $"[{GetChatTypeDisplayName(request.ChatType)}] {generatedTitle}";
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created {ChatType} chat session {SessionId} for user {UserId}", 
                    request.ChatType, session.Id, userId);

                return new ResponseCreateChatSessionDto
                {
                    success = true,
                    Message = "Tạo phiên chat thành công",
                    StatusCode = 201,
                    ChatSession = new AIChatSessionDto
                    {
                        Id = session.Id,
                        Title = session.Title,
                        Status = session.Status,
                        ChatType = session.ChatType,
                        CreatedAt = session.CreatedAt,
                        LastMessageAt = session.LastMessageAt,
                        MessageCount = 0
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {ChatType} chat session for user {UserId}", request.ChatType, userId);
                
                return new ResponseCreateChatSessionDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi tạo phiên chat",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseSendMessageDto> SendMessageAsync(RequestSendMessageDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Sending message to session {SessionId} from user {UserId}", request.SessionId, userId);

                // Kiểm tra session tồn tại và thuộc về user
                var session = await _unitOfWork.AIChatSessionRepository.GetSessionWithMessagesAsync(request.SessionId, userId);
                if (session == null)
                {
                    return new ResponseSendMessageDto
                    {
                        success = false,
                        Message = "Không tìm thấy phiên chat",
                        StatusCode = 404
                    };
                }

                // Tạo tin nhắn từ user
                var userMessage = new AIChatMessage
                {
                    Id = Guid.NewGuid(),
                    SessionId = request.SessionId,
                    Content = request.Message,
                    MessageType = "User",
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.AIChatMessageRepository.AddAsync(userMessage);

                // Lấy context nếu cần
                List<GeminiMessage>? conversationHistory = null;
                if (request.IncludeContext)
                {
                    var contextMessages = await _unitOfWork.AIChatMessageRepository
                        .GetMessagesForContextAsync(request.SessionId, request.ContextMessageCount);
                    
                    conversationHistory = contextMessages.Select(m => new GeminiMessage
                    {
                        Role = m.MessageType == "User" ? "user" : "model",
                        Content = m.Content
                    }).ToList();
                }

                // Gửi đến specialized AI service dựa trên chat type
                var aiResponse = await _specializedChatService.ProcessMessageAsync(
                    request.Message, 
                    session.ChatType, 
                    conversationHistory);

                AIChatMessage? aiMessage = null;
                string responseMessage = "Gửi tin nhắn thành công";
                
                if (aiResponse.Success)
                {
                    // Tạo tin nhắn phản hồi từ AI
                    aiMessage = new AIChatMessage
                    {
                        Id = Guid.NewGuid(),
                        SessionId = request.SessionId,
                        Content = aiResponse.Content,
                        MessageType = "AI",
                        TokensUsed = aiResponse.TokensUsed,
                        ResponseTimeMs = aiResponse.ResponseTimeMs,
                        Metadata = aiResponse.IsFallback ? "{\"isFallback\": true}" : null,
                        CreatedById = userId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };

                    await _unitOfWork.AIChatMessageRepository.AddAsync(aiMessage);

                    // Cập nhật thời gian tin nhắn cuối
                    await _unitOfWork.AIChatSessionRepository.UpdateLastMessageTimeAsync(request.SessionId);
                    
                    if (aiResponse.IsFallback)
                    {
                        responseMessage = "Gửi tin nhắn thành công (sử dụng phản hồi tự động)";
                        _logger.LogInformation("Used fallback response for {ChatType} session {SessionId}", 
                            session.ChatType, request.SessionId);
                    }
                }
                else
                {
                    // Nếu AI không phản hồi, tạo message thông báo lỗi
                    aiMessage = new AIChatMessage
                    {
                        Id = Guid.NewGuid(),
                        SessionId = request.SessionId,
                        Content = GetErrorMessage(session.ChatType),
                        MessageType = "AI",
                        Metadata = "{\"isError\": true}",
                        CreatedById = userId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };

                    await _unitOfWork.AIChatMessageRepository.AddAsync(aiMessage);
                    responseMessage = "Gửi tin nhắn thành công nhưng AI không thể phản hồi";
                    
                    _logger.LogWarning("AI failed to respond for {ChatType} session {SessionId}: {Error}", 
                        session.ChatType, request.SessionId, aiResponse.ErrorMessage);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Processed {ChatType} message for session {SessionId}, AI response: {Success}, IsFallback: {IsFallback}", 
                    session.ChatType, request.SessionId, aiResponse.Success, aiResponse.IsFallback);

                return new ResponseSendMessageDto
                {
                    success = true,
                    Message = responseMessage,
                    StatusCode = 200,
                    UserMessage = new AIChatMessageDto
                    {
                        Id = userMessage.Id,
                        Content = userMessage.Content,
                        MessageType = userMessage.MessageType,
                        CreatedAt = userMessage.CreatedAt
                    },
                    AIResponse = new AIChatMessageDto
                    {
                        Id = aiMessage.Id,
                        Content = aiMessage.Content,
                        MessageType = aiMessage.MessageType,
                        CreatedAt = aiMessage.CreatedAt,
                        TokensUsed = aiMessage.TokensUsed,
                        ResponseTimeMs = aiMessage.ResponseTimeMs,
                        IsFallback = aiResponse.IsFallback,
                        IsError = !aiResponse.Success
                    },
                    TokensUsed = aiResponse.TokensUsed,
                    ResponseTimeMs = aiResponse.ResponseTimeMs,
                    Error = aiResponse.Success ? null : aiResponse.ErrorMessage,
                    IsFallback = aiResponse.IsFallback
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to session {SessionId}", request.SessionId);
                
                return new ResponseSendMessageDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi gửi tin nhắn",
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ResponseGetChatSessionsDto> GetChatSessionsAsync(RequestGetChatSessionsDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting chat sessions for user {UserId}, page {Page}, ChatType filter: {ChatType}", 
                    userId, request.Page, request.ChatType);

                var (sessions, totalCount) = await _unitOfWork.AIChatSessionRepository
                    .GetUserChatSessionsAsync(userId, request.Page, request.PageSize, request.ChatType);

                var sessionDtos = new List<AIChatSessionDto>();
                
                foreach (var session in sessions)
                {
                    var messageCount = await _unitOfWork.AIChatMessageRepository.GetSessionMessageCountAsync(session.Id);
                    
                    sessionDtos.Add(new AIChatSessionDto
                    {
                        Id = session.Id,
                        Title = session.Title,
                        Status = session.Status,
                        ChatType = session.ChatType,
                        CreatedAt = session.CreatedAt,
                        LastMessageAt = session.LastMessageAt,
                        MessageCount = messageCount
                    });
                }

                return new ResponseGetChatSessionsDto
                {
                    success = true,
                    Message = "Lấy danh sách phiên chat thành công",
                    StatusCode = 200,
                    Sessions = sessionDtos,
                    TotalCount = totalCount,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat sessions for user {UserId}", userId);
                
                return new ResponseGetChatSessionsDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách phiên chat",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseGetMessagesDto> GetMessagesAsync(RequestGetMessagesDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting messages for session {SessionId}, user {UserId}", request.SessionId, userId);

                var session = await _unitOfWork.AIChatSessionRepository.GetSessionWithMessagesAsync(request.SessionId, userId);
                if (session == null)
                {
                    return new ResponseGetMessagesDto
                    {
                        success = false,
                        Message = "Không tìm thấy phiên chat",
                        StatusCode = 404
                    };
                }

                var messageDtos = session.Messages.Select(m => new AIChatMessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    MessageType = m.MessageType,
                    CreatedAt = m.CreatedAt,
                    TokensUsed = m.TokensUsed,
                    ResponseTimeMs = m.ResponseTimeMs,
                    IsFallback = m.Metadata?.Contains("\"isFallback\": true") == true,
                    IsError = m.Metadata?.Contains("\"isError\": true") == true
                }).ToList();

                return new ResponseGetMessagesDto
                {
                    success = true,
                    Message = "Lấy tin nhắn thành công",
                    StatusCode = 200,
                    ChatSession = new AIChatSessionWithMessagesDto
                    {
                        Id = session.Id,
                        Title = session.Title,
                        Status = session.Status,
                        ChatType = session.ChatType,
                        CreatedAt = session.CreatedAt,
                        LastMessageAt = session.LastMessageAt,
                        Messages = messageDtos
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for session {SessionId}", request.SessionId);
                
                return new ResponseGetMessagesDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lấy tin nhắn",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseSessionActionDto> ArchiveChatSessionAsync(Guid sessionId, Guid userId)
        {
            try
            {
                await _unitOfWork.AIChatSessionRepository.ArchiveSessionAsync(sessionId, userId);
                
                return new ResponseSessionActionDto
                {
                    success = true,
                    Message = "Lưu trữ phiên chat thành công",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving session {SessionId}", sessionId);
                
                return new ResponseSessionActionDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lưu trữ phiên chat",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseSessionActionDto> DeleteChatSessionAsync(Guid sessionId, Guid userId)
        {
            try
            {
                await _unitOfWork.AIChatSessionRepository.DeleteSessionAsync(sessionId, userId);
                
                return new ResponseSessionActionDto
                {
                    success = true,
                    Message = "Xóa phiên chat thành công",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                
                return new ResponseSessionActionDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi xóa phiên chat",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseSessionActionDto> UpdateSessionTitleAsync(Guid sessionId, string newTitle, Guid userId)
        {
            try
            {
                var session = await _unitOfWork.AIChatSessionRepository.GetByIdAsync(sessionId);
                if (session == null || session.UserId != userId)
                {
                    return new ResponseSessionActionDto
                    {
                        success = false,
                        Message = "Không tìm thấy phiên chat",
                        StatusCode = 404
                    };
                }

                session.Title = newTitle;
                await _unitOfWork.AIChatSessionRepository.UpdateAsync(session);
                await _unitOfWork.SaveChangesAsync();
                
                return new ResponseSessionActionDto
                {
                    success = true,
                    Message = "Cập nhật tiêu đề thành công",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session title {SessionId}", sessionId);
                
                return new ResponseSessionActionDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi cập nhật tiêu đề",
                    StatusCode = 500
                };
            }
        }

        // Helper methods
        private string GetDefaultTitle(AIChatType chatType)
        {
            return chatType switch
            {
                AIChatType.Tour => "[Tour] Tư vấn tour du lịch",
                AIChatType.Product => "[Sản phẩm] Tư vấn mua sắm",
                AIChatType.TayNinh => "[Tây Ninh] Hỏi đáp về Tây Ninh",
                _ => "Cuộc trò chuyện mới"
            };
        }

        private string GetChatTypeDisplayName(AIChatType chatType)
        {
            return chatType switch
            {
                AIChatType.Tour => "Tour",
                AIChatType.Product => "Sản phẩm",
                AIChatType.TayNinh => "Tây Ninh",
                _ => "Chat"
            };
        }

        private string GetErrorMessage(AIChatType chatType)
        {
            return chatType switch
            {
                AIChatType.Tour => "Xin lỗi, hệ thống tư vấn tour hiện đang gặp khó khăn. Vui lòng thử lại sau hoặc liên hệ hotline để được hỗ trợ.",
                AIChatType.Product => "Xin lỗi, hệ thống tư vấn sản phẩm tạm thời không khả dụng. Bạn có thể duyệt catalog sản phẩm trực tiếp.",
                AIChatType.TayNinh => "Xin lỗi, tôi tạm thời không thể chia sẻ thông tin về Tây Ninh. Vui lòng thử lại sau.",
                _ => "Xin lỗi, tôi hiện đang gặp khó khăn kỹ thuật. Vui lòng thử lại sau hoặc liên hệ hỗ trợ."
            };
        }
    }
}