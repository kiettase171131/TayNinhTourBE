using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.AIChat;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AIChat;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho AI Chat functionality
    /// </summary>
    public class AIChatService : IAIChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeminiAIService _geminiAIService;
        private readonly ILogger<AIChatService> _logger;

        public AIChatService(
            IUnitOfWork unitOfWork,
            IGeminiAIService geminiAIService,
            ILogger<AIChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _geminiAIService = geminiAIService;
            _logger = logger;
        }

        public async Task<ResponseCreateChatSessionDto> CreateChatSessionAsync(RequestCreateChatSessionDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating new chat session for user {UserId}", userId);

                // T?o session m?i
                var session = new AIChatSession
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = request.CustomTitle ?? "Cu?c trò chuy?n m?i",
                    Status = "Active",
                    LastMessageAt = DateTime.UtcNow,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.AIChatSessionRepository.AddAsync(session);

                // N?u có tin nh?n ??u tiên, x? lý luôn
                if (!string.IsNullOrWhiteSpace(request.FirstMessage))
                {
                    // T?o tin nh?n t? user
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

                    // G?i ??n AI và nh?n ph?n h?i
                    var aiResponse = await _geminiAIService.GenerateContentAsync(request.FirstMessage);

                    if (aiResponse.Success)
                    {
                        // T?o tin nh?n ph?n h?i t? AI
                        var aiMessage = new AIChatMessage
                        {
                            Id = Guid.NewGuid(),
                            SessionId = session.Id,
                            Content = aiResponse.Content,
                            MessageType = "AI",
                            TokensUsed = aiResponse.TokensUsed,
                            ResponseTimeMs = aiResponse.ResponseTimeMs,
                            CreatedById = userId,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false
                        };

                        await _unitOfWork.AIChatMessageRepository.AddAsync(aiMessage);

                        // T?o tiêu ?? t? ??ng t? tin nh?n ??u tiên
                        if (string.IsNullOrEmpty(request.CustomTitle))
                        {
                            var generatedTitle = await _geminiAIService.GenerateTitleAsync(request.FirstMessage);
                            session.Title = generatedTitle;
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created chat session {SessionId} for user {UserId}", session.Id, userId);

                return new ResponseCreateChatSessionDto
                {
                    success = true,
                    Message = "T?o phiên chat thành công",
                    StatusCode = 201,
                    ChatSession = new AIChatSessionDto
                    {
                        Id = session.Id,
                        Title = session.Title,
                        Status = session.Status,
                        CreatedAt = session.CreatedAt,
                        LastMessageAt = session.LastMessageAt,
                        MessageCount = 0
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat session for user {UserId}", userId);
                
                return new ResponseCreateChatSessionDto
                {
                    success = false,
                    Message = "Có l?i x?y ra khi t?o phiên chat",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseSendMessageDto> SendMessageAsync(RequestSendMessageDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Sending message to session {SessionId} from user {UserId}", request.SessionId, userId);

                // Ki?m tra session t?n t?i và thu?c v? user
                var session = await _unitOfWork.AIChatSessionRepository.GetSessionWithMessagesAsync(request.SessionId, userId);
                if (session == null)
                {
                    return new ResponseSendMessageDto
                    {
                        success = false,
                        Message = "Không tìm th?y phiên chat",
                        StatusCode = 404
                    };
                }

                // T?o tin nh?n t? user
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

                // L?y context n?u c?n
                List<GeminiMessage>? conversationHistory = null;
                if (request.IncludeContext)
                {
                    var contextMessages = await _unitOfWork.AIChatMessageRepository
                        .GetMessagesForContextAsync(request.SessionId, request.ContextMessageCount);
                    
                    conversationHistory = contextMessages.Select(m => new GeminiMessage
                    {
                        Role = m.MessageType,
                        Content = m.Content
                    }).ToList();
                }

                // G?i ??n AI
                var aiResponse = await _geminiAIService.GenerateContentAsync(request.Message, conversationHistory);

                AIChatMessage? aiMessage = null;
                if (aiResponse.Success)
                {
                    // T?o tin nh?n ph?n h?i t? AI
                    aiMessage = new AIChatMessage
                    {
                        Id = Guid.NewGuid(),
                        SessionId = request.SessionId,
                        Content = aiResponse.Content,
                        MessageType = "AI",
                        TokensUsed = aiResponse.TokensUsed,
                        ResponseTimeMs = aiResponse.ResponseTimeMs,
                        CreatedById = userId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };

                    await _unitOfWork.AIChatMessageRepository.AddAsync(aiMessage);

                    // C?p nh?t th?i gian tin nh?n cu?i
                    await _unitOfWork.AIChatSessionRepository.UpdateLastMessageTimeAsync(request.SessionId);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Processed message for session {SessionId}, AI response: {Success}", 
                    request.SessionId, aiResponse.Success);

                return new ResponseSendMessageDto
                {
                    success = true,
                    Message = aiResponse.Success ? "G?i tin nh?n thành công" : "G?i tin nh?n thành công nh?ng AI không ph?n h?i",
                    StatusCode = 200,
                    UserMessage = new AIChatMessageDto
                    {
                        Id = userMessage.Id,
                        Content = userMessage.Content,
                        MessageType = userMessage.MessageType,
                        CreatedAt = userMessage.CreatedAt
                    },
                    AIResponse = aiMessage != null ? new AIChatMessageDto
                    {
                        Id = aiMessage.Id,
                        Content = aiMessage.Content,
                        MessageType = aiMessage.MessageType,
                        CreatedAt = aiMessage.CreatedAt,
                        TokensUsed = aiMessage.TokensUsed,
                        ResponseTimeMs = aiMessage.ResponseTimeMs
                    } : null,
                    TokensUsed = aiResponse.TokensUsed,
                    ResponseTimeMs = aiResponse.ResponseTimeMs,
                    Error = aiResponse.Success ? null : aiResponse.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to session {SessionId}", request.SessionId);
                
                return new ResponseSendMessageDto
                {
                    success = false,
                    Message = "Có l?i x?y ra khi g?i tin nh?n",
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ResponseGetChatSessionsDto> GetChatSessionsAsync(RequestGetChatSessionsDto request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting chat sessions for user {UserId}, page {Page}", userId, request.Page);

                var (sessions, totalCount) = await _unitOfWork.AIChatSessionRepository
                    .GetUserChatSessionsAsync(userId, request.Page, request.PageSize);

                var sessionDtos = new List<AIChatSessionDto>();
                
                foreach (var session in sessions)
                {
                    var messageCount = await _unitOfWork.AIChatMessageRepository.GetSessionMessageCountAsync(session.Id);
                    
                    sessionDtos.Add(new AIChatSessionDto
                    {
                        Id = session.Id,
                        Title = session.Title,
                        Status = session.Status,
                        CreatedAt = session.CreatedAt,
                        LastMessageAt = session.LastMessageAt,
                        MessageCount = messageCount
                    });
                }

                return new ResponseGetChatSessionsDto
                {
                    success = true,
                    Message = "L?y danh sách phiên chat thành công",
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
                    Message = "Có l?i x?y ra khi l?y danh sách phiên chat",
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
                        Message = "Không tìm th?y phiên chat",
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
                    ResponseTimeMs = m.ResponseTimeMs
                }).ToList();

                return new ResponseGetMessagesDto
                {
                    success = true,
                    Message = "L?y tin nh?n thành công",
                    StatusCode = 200,
                    ChatSession = new AIChatSessionWithMessagesDto
                    {
                        Id = session.Id,
                        Title = session.Title,
                        Status = session.Status,
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
                    Message = "Có l?i x?y ra khi l?y tin nh?n",
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
                    Message = "L?u tr? phiên chat thành công",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving session {SessionId}", sessionId);
                
                return new ResponseSessionActionDto
                {
                    success = false,
                    Message = "Có l?i x?y ra khi l?u tr? phiên chat",
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
                    Message = "Có l?i x?y ra khi xóa phiên chat",
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
                        Message = "Không tìm th?y phiên chat",
                        StatusCode = 404
                    };
                }

                session.Title = newTitle;
                await _unitOfWork.AIChatSessionRepository.UpdateAsync(session);
                await _unitOfWork.SaveChangesAsync();
                
                return new ResponseSessionActionDto
                {
                    success = true,
                    Message = "C?p nh?t tiêu ?? thành công",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session title {SessionId}", sessionId);
                
                return new ResponseSessionActionDto
                {
                    success = false,
                    Message = "Có l?i x?y ra khi c?p nh?t tiêu ??",
                    StatusCode = 500
                };
            }
        }
    }
}