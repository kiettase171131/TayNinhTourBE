using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.AIChat
{
    /// <summary>
    /// DTO ?? t?o phiên chat m?i
    /// </summary>
    public class RequestCreateChatSessionDto
    {
        /// <summary>
        /// Lo?i chat session (Tour, Product, TayNinh)
        /// </summary>
        public AIChatType ChatType { get; set; }

        /// <summary>
        /// Tin nh?n ??u tiên (optional, có th? t?o session tr?ng)
        /// </summary>
        public string? FirstMessage { get; set; }

        /// <summary>
        /// Tiêu ?? tùy ch?nh cho phiên chat (optional, s? auto-generate n?u không có)
        /// </summary>
        public string? CustomTitle { get; set; }
    }

    /// <summary>
    /// DTO ?? g?i tin nh?n ??n AI
    /// </summary>
    public class RequestSendMessageDto
    {
        /// <summary>
        /// ID c?a phiên chat
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// N?i dung tin nh?n t? user
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Có include context t? các tin nh?n tr??c không
        /// </summary>
        public bool IncludeContext { get; set; } = true;

        /// <summary>
        /// S? l??ng tin nh?n context t?i ?a
        /// </summary>
        public int ContextMessageCount { get; set; } = 10;
    }

    /// <summary>
    /// DTO ?? l?y danh sách phiên chat
    /// </summary>
    public class RequestGetChatSessionsDto
    {
        /// <summary>
        /// Trang hi?n t?i (0-based)
        /// </summary>
        public int Page { get; set; } = 0;

        /// <summary>
        /// S? l??ng sessions per page
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Tr?ng thái session (Active, Archived, All)
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// L?c theo lo?i chat (optional)
        /// </summary>
        public AIChatType? ChatType { get; set; }
    }

    /// <summary>
    /// DTO ?? l?y tin nh?n trong phiên chat
    /// </summary>
    public class RequestGetMessagesDto
    {
        /// <summary>
        /// ID c?a phiên chat
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// Trang hi?n t?i (0-based)
        /// </summary>
        public int Page { get; set; } = 0;

        /// <summary>
        /// S? l??ng messages per page
        /// </summary>
        public int PageSize { get; set; } = 50;
    }
}