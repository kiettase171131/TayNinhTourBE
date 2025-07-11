using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.AIChat;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AIChat;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Interface cho AI Chat Service
    /// </summary>
    public interface IAIChatService
    {
        /// <summary>
        /// T?o phiên chat m?i
        /// </summary>
        Task<ResponseCreateChatSessionDto> CreateChatSessionAsync(RequestCreateChatSessionDto request, Guid userId);

        /// <summary>
        /// G?i tin nh?n và nh?n ph?n h?i t? AI
        /// </summary>
        Task<ResponseSendMessageDto> SendMessageAsync(RequestSendMessageDto request, Guid userId);

        /// <summary>
        /// L?y danh sách phiên chat c?a user
        /// </summary>
        Task<ResponseGetChatSessionsDto> GetChatSessionsAsync(RequestGetChatSessionsDto request, Guid userId);

        /// <summary>
        /// L?y tin nh?n trong phiên chat
        /// </summary>
        Task<ResponseGetMessagesDto> GetMessagesAsync(RequestGetMessagesDto request, Guid userId);

        /// <summary>
        /// Archive phiên chat
        /// </summary>
        Task<ResponseSessionActionDto> ArchiveChatSessionAsync(Guid sessionId, Guid userId);

        /// <summary>
        /// Xóa phiên chat
        /// </summary>
        Task<ResponseSessionActionDto> DeleteChatSessionAsync(Guid sessionId, Guid userId);

        /// <summary>
        /// C?p nh?t tiêu ?? phiên chat
        /// </summary>
        Task<ResponseSessionActionDto> UpdateSessionTitleAsync(Guid sessionId, string newTitle, Guid userId);
    }
}