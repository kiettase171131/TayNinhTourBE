using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class AIChatSessionRepository : GenericRepository<AIChatSession>, IAIChatSessionRepository
    {
        public AIChatSessionRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<(List<AIChatSession> Sessions, int TotalCount)> GetUserChatSessionsAsync(Guid userId, int page, int pageSize)
        {
            return await GetUserChatSessionsAsync(userId, page, pageSize, null);
        }

        public async Task<(List<AIChatSession> Sessions, int TotalCount)> GetUserChatSessionsAsync(Guid userId, int page, int pageSize, AIChatType? chatType = null)
        {
            var query = _context.AIChatSessions
                .Where(s => s.UserId == userId && !s.IsDeleted && s.Status != "Deleted");

            // Filter by ChatType if specified
            if (chatType.HasValue)
            {
                query = query.Where(s => s.ChatType == chatType.Value);
            }

            query = query.OrderByDescending(s => s.LastMessageAt);

            var totalCount = await query.CountAsync();
            var sessions = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (sessions, totalCount);
        }

        public async Task<AIChatSession?> GetSessionWithMessagesAsync(Guid sessionId, Guid userId)
        {
            return await _context.AIChatSessions
                .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsDeleted);
        }

        public async Task UpdateLastMessageTimeAsync(Guid sessionId)
        {
            var session = await _context.AIChatSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.LastMessageAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ArchiveSessionAsync(Guid sessionId, Guid userId)
        {
            var session = await _context.AIChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
            
            if (session != null)
            {
                session.Status = "Archived";
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteSessionAsync(Guid sessionId, Guid userId)
        {
            var session = await _context.AIChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
            
            if (session != null)
            {
                session.Status = "Deleted";
                session.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}