using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IAIChatSessionRepository : IGenericRepository<AIChatSession>
    {
        /// <summary>
        /// L?y danh s�ch chat sessions c?a user v?i ph�n trang
        /// </summary>
        Task<(List<AIChatSession> Sessions, int TotalCount)> GetUserChatSessionsAsync(Guid userId, int page, int pageSize);

        /// <summary>
        /// L?y danh s�ch chat sessions c?a user v?i ph�n trang v?i l?c theo ChatType
        /// </summary>
        Task<(List<AIChatSession> Sessions, int TotalCount)> GetUserChatSessionsAsync(Guid userId, int page, int pageSize, AIChatType? chatType = null);

        /// <summary>
        /// L?y chat session v?i messages
        /// </summary>
        Task<AIChatSession?> GetSessionWithMessagesAsync(Guid sessionId, Guid userId);

        /// <summary>
        /// C?p nh?t th?i gian tin nh?n cu?i c�ng
        /// </summary>
        Task UpdateLastMessageTimeAsync(Guid sessionId);

        /// <summary>
        /// ?�nh d?u session l� archived
        /// </summary>
        Task ArchiveSessionAsync(Guid sessionId, Guid userId);

        /// <summary>
        /// X�a session (soft delete)
        /// </summary>
        Task DeleteSessionAsync(Guid sessionId, Guid userId);
    }
}