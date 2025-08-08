using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AIChat
{
    /// <summary>
    /// Base response DTO cho AI Chat operations
    /// </summary>
    public class BaseAIChatResponseDto
    {
        public bool success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// Response DTO cho vi?c t?o chat session m?i
    /// </summary>
    public class ResponseCreateChatSessionDto : BaseAIChatResponseDto
    {
        public AIChatSessionDto? ChatSession { get; set; }
    }

    /// <summary>
    /// Response DTO cho vi?c g?i tin nh?n v?i enhanced topic redirect support
    /// </summary>
    public class ResponseSendMessageDto : BaseAIChatResponseDto
    {
        public AIChatMessageDto? UserMessage { get; set; }
        public AIChatMessageDto? AIResponse { get; set; }
        public int TokensUsed { get; set; }
        public int ResponseTimeMs { get; set; }
        public string? Error { get; set; }
        public bool IsFallback { get; set; }

        /// <summary>
        /// Indicates if AI suggests switching to different chat type
        /// </summary>
        public bool RequiresTopicRedirect { get; set; }

        /// <summary>
        /// Suggested chat type to redirect user to
        /// </summary>
        public AIChatType? SuggestedChatType { get; set; }

        /// <summary>
        /// User-friendly suggestion message for topic redirect
        /// </summary>
        public string? RedirectSuggestion { get; set; }
    }

    /// <summary>
    /// Response DTO cho vi?c l?y danh sách chat sessions
    /// </summary>
    public class ResponseGetChatSessionsDto : BaseAIChatResponseDto
    {
        public List<AIChatSessionDto> Sessions { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Response DTO cho vi?c l?y tin nh?n trong session
    /// </summary>
    public class ResponseGetMessagesDto : BaseAIChatResponseDto
    {
        public AIChatSessionWithMessagesDto? ChatSession { get; set; }
    }

    /// <summary>
    /// Response DTO cho các hành ??ng trên session (archive, delete, update)
    /// </summary>
    public class ResponseSessionActionDto : BaseAIChatResponseDto
    {
        public DateTime? ActionTimestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO ch?a thông tin chi ti?t c?a chat session
    /// </summary>
    public class AIChatSessionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public AIChatType ChatType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int MessageCount { get; set; }
    }

    /// <summary>
    /// DTO ch?a thông tin chi ti?t c?a chat message v?i topic redirect metadata
    /// </summary>
    public class AIChatMessageDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty; // "User" or "AI"
        public DateTime CreatedAt { get; set; }
        public int? TokensUsed { get; set; }
        public int? ResponseTimeMs { get; set; }
        public bool IsFallback { get; set; }
        public bool IsError { get; set; }

        /// <summary>
        /// Indicates if this message contains topic redirect suggestion
        /// </summary>
        public bool IsTopicRedirect { get; set; }

        /// <summary>
        /// Suggested ChatType if this is a redirect message
        /// </summary>
        public AIChatType? SuggestedChatType { get; set; }
    }

    /// <summary>
    /// DTO ch?a thông tin session v?i danh sách messages
    /// </summary>
    public class AIChatSessionWithMessagesDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public AIChatType ChatType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public List<AIChatMessageDto> Messages { get; set; } = new();
    }
}