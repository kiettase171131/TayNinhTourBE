using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho SpecialtyShopApplication entity
    /// Thay thế cho ShopApplicationRepository với methods mở rộng
    /// </summary>
    public class SpecialtyShopApplicationRepository : GenericRepository<SpecialtyShopApplication>, ISpecialtyShopApplicationRepository
    {
        public SpecialtyShopApplicationRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách applications theo status
        /// </summary>
        public async Task<IEnumerable<SpecialtyShopApplication>> GetByStatusAsync(SpecialtyShopApplicationStatus status)
        {
            return await _context.SpecialtyShopApplications
                .Where(a => a.Status == status && a.IsActive)
                .Include(a => a.User)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách applications theo user ID
        /// </summary>
        public async Task<IEnumerable<SpecialtyShopApplication>> GetByUserIdAsync(Guid userId)
        {
            return await _context.SpecialtyShopApplications
                .Where(a => a.UserId == userId && a.IsActive)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy application mới nhất của user
        /// </summary>
        public async Task<SpecialtyShopApplication?> GetLatestByUserIdAsync(Guid userId)
        {
            return await _context.SpecialtyShopApplications
                .Where(a => a.UserId == userId && a.IsActive)
                .OrderByDescending(a => a.SubmittedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Kiểm tra user có application pending hoặc approved không
        /// </summary>
        public async Task<bool> HasActiveApplicationAsync(Guid userId)
        {
            return await _context.SpecialtyShopApplications
                .AnyAsync(a => a.UserId == userId 
                    && a.IsActive 
                    && (a.Status == SpecialtyShopApplicationStatus.Pending 
                        || a.Status == SpecialtyShopApplicationStatus.Approved));
        }

        /// <summary>
        /// Lấy danh sách applications với pagination và filter
        /// </summary>
        public async Task<(IEnumerable<SpecialtyShopApplication> Applications, int TotalCount)> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            SpecialtyShopApplicationStatus? status = null, 
            string? searchTerm = null)
        {
            var query = _context.SpecialtyShopApplications
                .Where(a => a.IsActive)
                .Include(a => a.User)
                .AsQueryable();

            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            // Search by shop name or user name/email
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(a => 
                    a.ShopName.ToLower().Contains(searchLower) ||
                    a.User.Name.ToLower().Contains(searchLower) ||
                    a.User.Email.ToLower().Contains(searchLower) ||
                    a.RepresentativeName.ToLower().Contains(searchLower));
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
        /// Lấy application với thông tin User và ProcessedBy
        /// </summary>
        public async Task<SpecialtyShopApplication?> GetWithUserInfoAsync(Guid id)
        {
            return await _context.SpecialtyShopApplications
                .Where(a => a.Id == id && a.IsActive)
                .Include(a => a.User)
                .Include(a => a.ProcessedBy)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy danh sách applications với thông tin User (cho admin list)
        /// </summary>
        public async Task<IEnumerable<SpecialtyShopApplication>> GetWithUserInfoAsync(SpecialtyShopApplicationStatus? status = null)
        {
            var query = _context.SpecialtyShopApplications
                .Where(a => a.IsActive)
                .Include(a => a.User)
                .Include(a => a.ProcessedBy)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return await query
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Đếm số applications theo status
        /// </summary>
        public async Task<int> CountByStatusAsync(SpecialtyShopApplicationStatus status)
        {
            return await _context.SpecialtyShopApplications
                .CountAsync(a => a.Status == status && a.IsActive);
        }

        /// <summary>
        /// Lấy applications được submit trong khoảng thời gian
        /// </summary>
        public async Task<IEnumerable<SpecialtyShopApplication>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.SpecialtyShopApplications
                .Where(a => a.IsActive 
                    && a.SubmittedAt >= fromDate 
                    && a.SubmittedAt <= toDate)
                .Include(a => a.User)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }
    }
}
