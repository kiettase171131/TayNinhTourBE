using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using ApplicationStatus = TayNinhTourApi.DataAccessLayer.Enums.ApplicationStatus;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Enhanced repository implementation cho TourGuideApplication entity
    /// Nâng cấp từ version cũ với đầy đủ methods
    /// </summary>
    public class TourGuideApplicationRepository : GenericRepository<TourGuideApplication>, ITourGuideApplicationRepository
    {
        public TourGuideApplicationRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách applications theo status (Enhanced version)
        /// </summary>
        public async Task<IEnumerable<TourGuideApplication>> GetByStatusAsync(TourGuideApplicationStatus status)
        {
            return await _context.TourGuideApplications
                .Where(a => a.Status == status && a.IsActive)
                .Include(a => a.User)
                .Include(a => a.ProcessedBy)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách applications theo user ID (Enhanced version)
        /// </summary>
        public async Task<IEnumerable<TourGuideApplication>> GetByUserIdAsync(Guid userId)
        {
            return await _context.TourGuideApplications
                .Where(a => a.UserId == userId && a.IsActive)
                .Include(a => a.User)
                .Include(a => a.ProcessedBy)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy application theo ID với thông tin User và ProcessedBy
        /// </summary>
        public async Task<TourGuideApplication?> GetByIdWithDetailsAsync(Guid applicationId)
        {
            return await _context.TourGuideApplications
                .Where(a => a.Id == applicationId && a.IsActive)
                .Include(a => a.User)
                .Include(a => a.ProcessedBy)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy application của user theo ID (để check ownership)
        /// </summary>
        public async Task<TourGuideApplication?> GetByIdAndUserIdAsync(Guid applicationId, Guid userId)
        {
            return await _context.TourGuideApplications
                .Where(a => a.Id == applicationId && a.UserId == userId && a.IsActive)
                .Include(a => a.User)
                .Include(a => a.ProcessedBy)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy danh sách applications với pagination và filter
        /// </summary>
        public async Task<(IEnumerable<TourGuideApplication> Applications, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            TourGuideApplicationStatus? status = null,
            string? searchTerm = null)
        {
            var query = _context.TourGuideApplications
                .Where(a => a.IsActive)
                .Include(a => a.User)
                .Include(a => a.ProcessedBy)
                .AsQueryable();

            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            // Search by full name, phone, email, or user name/email
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(a =>
                    a.FullName.ToLower().Contains(searchLower) ||
                    a.PhoneNumber.ToLower().Contains(searchLower) ||
                    a.Email.ToLower().Contains(searchLower) ||
                    a.User.Name.ToLower().Contains(searchLower) ||
                    a.User.Email.ToLower().Contains(searchLower));
            }

            var totalCount = await query.CountAsync();

            var applications = await query
                .OrderByDescending(a => a.SubmittedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (applications, totalCount);
        }

        /// <summary>
        /// Kiểm tra user có application pending hoặc approved không
        /// </summary>
        public async Task<bool> HasActiveApplicationAsync(Guid userId)
        {
            return await _context.TourGuideApplications
                .AnyAsync(a => a.UserId == userId &&
                              a.IsActive &&
                              (a.Status == TourGuideApplicationStatus.Pending ||
                               a.Status == TourGuideApplicationStatus.Approved));
        }

        /// <summary>
        /// Lấy application mới nhất của user
        /// </summary>
        public async Task<TourGuideApplication?> GetLatestByUserIdAsync(Guid userId)
        {
            return await _context.TourGuideApplications
                .Where(a => a.UserId == userId && a.IsActive)
                .Include(a => a.User)
                .Include(a => a.ProcessedBy)
                .OrderByDescending(a => a.SubmittedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Đếm số applications theo status
        /// </summary>
        public async Task<int> CountByStatusAsync(TourGuideApplicationStatus status)
        {
            return await _context.TourGuideApplications
                .CountAsync(a => a.Status == status && a.IsActive);
        }

        // Legacy methods for backward compatibility
        [Obsolete("Use GetByStatusAsync instead")]
        public async Task<IEnumerable<TourGuideApplication>> ListByStatusAsync(ApplicationStatus status)
        {
            // Convert old enum to new enum
            var newStatus = status switch
            {
                ApplicationStatus.Pending => TourGuideApplicationStatus.Pending,
                ApplicationStatus.Approved => TourGuideApplicationStatus.Approved,
                ApplicationStatus.Rejected => TourGuideApplicationStatus.Rejected,
                _ => TourGuideApplicationStatus.Pending
            };

            return await GetByStatusAsync(newStatus);
        }

        [Obsolete("Use GetByUserIdAsync instead")]
        public async Task<IEnumerable<TourGuideApplication>> ListByUserAsync(Guid userId)
        {
            return await GetByUserIdAsync(userId);
        }
    }
}
