using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class AIChatMessageRepository : GenericRepository<AIChatMessage>, IAIChatMessageRepository
    {
        public AIChatMessageRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<List<AIChatMessage>> GetSessionMessagesAsync(Guid sessionId, int page, int pageSize)
        {
            return await _context.AIChatMessages
                .Where(m => m.SessionId == sessionId && !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetSessionMessageCountAsync(Guid sessionId)
        {
            return await _context.AIChatMessages
                .CountAsync(m => m.SessionId == sessionId && !m.IsDeleted);
        }

        public async Task<List<AIChatMessage>> GetMessagesForContextAsync(Guid sessionId, int messageCount = 10)
        {
            return await _context.AIChatMessages
                .Where(m => m.SessionId == sessionId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Take(messageCount)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task CleanupOldMessagesAsync(Guid sessionId, int keepMessageCount = 50)
        {
            var messagesToDelete = await _context.AIChatMessages
                .Where(m => m.SessionId == sessionId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(keepMessageCount)
                .ToListAsync();

            foreach (var message in messagesToDelete)
            {
                message.IsDeleted = true;
            }

            if (messagesToDelete.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}