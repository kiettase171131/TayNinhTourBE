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
        /// T?o phi�n chat m?i
        /// </summary>
        Task<ResponseCreateChatSessionDto> CreateChatSessionAsync(RequestCreateChatSessionDto request, Guid userId);

        /// <summary>
        /// G?i tin nh?n v� nh?n ph?n h?i t? AI
        /// </summary>
        Task<ResponseSendMessageDto> SendMessageAsync(RequestSendMessageDto request, Guid userId);

        /// <summary>
        /// L?y danh s�ch phi�n chat c?a user
        /// </summary>
        Task<ResponseGetChatSessionsDto> GetChatSessionsAsync(RequestGetChatSessionsDto request, Guid userId);

        /// <summary>
        /// L?y tin nh?n trong phi�n chat
        /// </summary>
        Task<ResponseGetMessagesDto> GetMessagesAsync(RequestGetMessagesDto request, Guid userId);

        /// <summary>
        /// Archive phi�n chat
        /// </summary>
        Task<ResponseSessionActionDto> ArchiveChatSessionAsync(Guid sessionId, Guid userId);

        /// <summary>
        /// X�a phi�n chat
        /// </summary>
        Task<ResponseSessionActionDto> DeleteChatSessionAsync(Guid sessionId, Guid userId);

        /// <summary>
        /// C?p nh?t ti�u ?? phi�n chat
        /// </summary>
        Task<ResponseSessionActionDto> UpdateSessionTitleAsync(Guid sessionId, string newTitle, Guid userId);
    }
}