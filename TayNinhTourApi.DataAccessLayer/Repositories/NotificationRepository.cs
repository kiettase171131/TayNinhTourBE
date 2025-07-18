using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho Notification entity
    /// </summary>
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// L?y thông báo c?a user v?i phân trang
        /// </summary>
        public async Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetUserNotificationsAsync(
            Guid userId, 
            int pageIndex = 0, 
            int pageSize = 20, 
            bool? isRead = null,
            NotificationType? type = null)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId && n.IsActive)
                .AsQueryable();

            // Apply filters
            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(n => n.Type == type.Value);
            }

            // Filter out expired notifications
            var now = DateTime.UtcNow;
            query = query.Where(n => n.ExpiresAt == null || n.ExpiresAt > now);

            // Get total count before paging
            var totalCount = await query.CountAsync();

            // Apply paging and ordering
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Include(n => n.User)
                .ToListAsync();

            return (notifications, totalCount);
        }

        /// <summary>
        /// ??m s? thông báo ch?a ??c c?a user
        /// </summary>
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await _context.Notifications
                .Where(n => n.UserId == userId && 
                           !n.IsRead && 
                           n.IsActive &&
                           (n.ExpiresAt == null || n.ExpiresAt > now))
                .CountAsync();
        }

        /// <summary>
        /// ?ánh d?u thông báo ?ã ??c
        /// </summary>
        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && n.IsActive);

            if (notification == null || notification.IsRead)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// ?ánh d?u t?t c? thông báo c?a user ?ã ??c
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && 
                           !n.IsRead && 
                           n.IsActive &&
                           (n.ExpiresAt == null || n.ExpiresAt > now))
                .ToListAsync();

            if (!unreadNotifications.Any())
                return 0;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                notification.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return unreadNotifications.Count;
        }

        /// <summary>
        /// Xóa thông báo c? ?ã h?t h?n
        /// </summary>
        public async Task<int> DeleteExpiredNotificationsAsync(DateTime cutoffDate)
        {
            var expiredNotifications = await _context.Notifications
                .Where(n => n.ExpiresAt != null && n.ExpiresAt <= cutoffDate)
                .ToListAsync();

            if (!expiredNotifications.Any())
                return 0;

            _context.Notifications.RemoveRange(expiredNotifications);
            await _context.SaveChangesAsync();
            return expiredNotifications.Count;
        }

        /// <summary>
        /// L?y thông báo theo ID và verify ownership
        /// </summary>
        public async Task<Notification?> GetByIdAndUserAsync(Guid notificationId, Guid userId)
        {
            return await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && n.IsActive);
        }
    }
}