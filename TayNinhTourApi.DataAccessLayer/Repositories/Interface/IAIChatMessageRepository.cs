using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IAIChatMessageRepository : IGenericRepository<AIChatMessage>
    {
        /// <summary>
        /// L?y danh s�ch messages c?a session v?i ph�n trang
        /// </summary>
        Task<List<AIChatMessage>> GetSessionMessagesAsync(Guid sessionId, int page, int pageSize);

        /// <summary>
        /// L?y s? l??ng messages trong session
        /// </summary>
        Task<int> GetSessionMessageCountAsync(Guid sessionId);

        /// <summary>
        /// L?y messages context cho AI (l?y n tin nh?n g?n nh?t)
        /// </summary>
        Task<List<AIChatMessage>> GetMessagesForContextAsync(Guid sessionId, int messageCount = 10);

        /// <summary>
        /// X�a messages c? trong session (gi? l?i n tin nh?n g?n nh?t)
        /// </summary>
        Task CleanupOldMessagesAsync(Guid sessionId, int keepMessageCount = 50);
    }
}