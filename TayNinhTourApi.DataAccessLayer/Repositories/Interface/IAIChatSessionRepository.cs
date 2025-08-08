using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IAIChatSessionRepository : IGenericRepository<AIChatSession>
    {
        /// <summary>
        /// L?y danh sách chat sessions c?a user v?i phân trang
        /// </summary>
        Task<(List<AIChatSession> Sessions, int TotalCount)> GetUserChatSessionsAsync(Guid userId, int page, int pageSize);

        /// <summary>
        /// L?y danh sách chat sessions c?a user v?i phân trang v?i l?c theo ChatType
        /// </summary>
        Task<(List<AIChatSession> Sessions, int TotalCount)> GetUserChatSessionsAsync(Guid userId, int page, int pageSize, AIChatType? chatType = null);

        /// <summary>
        /// L?y chat session v?i messages
        /// </summary>
        Task<AIChatSession?> GetSessionWithMessagesAsync(Guid sessionId, Guid userId);

        /// <summary>
        /// C?p nh?t th?i gian tin nh?n cu?i cùng
        /// </summary>
        Task UpdateLastMessageTimeAsync(Guid sessionId);

        /// <summary>
        /// ?ánh d?u session là archived
        /// </summary>
        Task ArchiveSessionAsync(Guid sessionId, Guid userId);

        /// <summary>
        /// Xóa session (soft delete)
        /// </summary>
        Task DeleteSessionAsync(Guid sessionId, Guid userId);
    }
}