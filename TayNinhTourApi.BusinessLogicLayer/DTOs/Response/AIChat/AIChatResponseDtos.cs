namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AIChat
{
    /// <summary>
    /// DTO cho tin nh?n AI Chat
    /// </summary>
    public class AIChatMessageDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = null!;
        public string MessageType { get; set; } = null!; // "User" or "AI"
        public DateTime CreatedAt { get; set; }
        public int? TokensUsed { get; set; }
        public int? ResponseTimeMs { get; set; }
    }

    /// <summary>
    /// DTO cho phi�n chat AI
    /// </summary>
    public class AIChatSessionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int MessageCount { get; set; }
        public AIChatMessageDto? LastMessage { get; set; }
    }

    /// <summary>
    /// DTO cho phi�n chat v?i danh s�ch tin nh?n
    /// </summary>
    public class AIChatSessionWithMessagesDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public List<AIChatMessageDto> Messages { get; set; } = new List<AIChatMessageDto>();
    }

    /// <summary>
    /// Response DTO khi t?o phi�n chat m?i
    /// </summary>
    public class ResponseCreateChatSessionDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int StatusCode { get; set; }
        public AIChatSessionDto? ChatSession { get; set; }
    }

    /// <summary>
    /// Response DTO khi g?i tin nh?n
    /// </summary>
    public class ResponseSendMessageDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int StatusCode { get; set; }
        public AIChatMessageDto? UserMessage { get; set; }
        public AIChatMessageDto? AIResponse { get; set; }
        public int? TokensUsed { get; set; }
        public int? ResponseTimeMs { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Response DTO khi l?y danh s�ch phi�n chat
    /// </summary>
    public class ResponseGetChatSessionsDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int StatusCode { get; set; }
        public List<AIChatSessionDto> Sessions { get; set; } = new List<AIChatSessionDto>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// Response DTO khi l?y tin nh?n trong phi�n
    /// </summary>
    public class ResponseGetMessagesDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int StatusCode { get; set; }
        public AIChatSessionWithMessagesDto? ChatSession { get; set; }
    }

    /// <summary>
    /// Response DTO cho c�c thao t�c session (archive, delete)
    /// </summary>
    public class ResponseSessionActionDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int StatusCode { get; set; }
    }
}