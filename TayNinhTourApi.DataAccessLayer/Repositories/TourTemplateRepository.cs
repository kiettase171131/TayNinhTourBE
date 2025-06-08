using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourTemplate entity
    /// Kế thừa từ GenericRepository và implement ITourTemplateRepository
    /// </summary>
    public class TourTemplateRepository : GenericRepository<TourTemplate>, ITourTemplateRepository
    {
        public TourTemplateRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TourTemplate>> GetByTemplateTypeAsync(TourTemplateType templateType, bool includeInactive = false)
        {
            var query = _context.TourTemplates
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Images)
                .Where(t => t.TemplateType == templateType);

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive && !t.IsDeleted);
            }

            return await query.OrderBy(t => t.Title).ToListAsync();
        }

        public async Task<IEnumerable<TourTemplate>> GetByCreatedByAsync(Guid createdById, bool includeInactive = false)
        {
            var query = _context.TourTemplates
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Images)
                .Where(t => t.CreatedById == createdById);

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive && !t.IsDeleted);
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }



        public async Task<IEnumerable<TourTemplate>> GetByStartLocationAsync(string startLocation, bool includeInactive = false)
        {
            var query = _context.TourTemplates
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Images)
                .Where(t => t.StartLocation.Contains(startLocation));

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive && !t.IsDeleted);
            }

            return await query.OrderBy(t => t.Title).ToListAsync();
        }

        public async Task<IEnumerable<TourTemplate>> GetByEndLocationAsync(string endLocation, bool includeInactive = false)
        {
            var query = _context.TourTemplates
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Images)
                .Where(t => t.EndLocation.Contains(endLocation));

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive && !t.IsDeleted);
            }

            return await query.OrderBy(t => t.Title).ToListAsync();
        }

        public async Task<IEnumerable<TourTemplate>> GetByScheduleDayAsync(ScheduleDay scheduleDay, bool includeInactive = false)
        {
            var query = _context.TourTemplates
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Images)
                .Where(t => (t.ScheduleDays & scheduleDay) == scheduleDay);

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive && !t.IsDeleted);
            }

            return await query.OrderBy(t => t.Title).ToListAsync();
        }

        public async Task<IEnumerable<TourTemplate>> SearchAsync(string keyword, bool includeInactive = false)
        {
            var query = _context.TourTemplates
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Images)
                .Where(t => t.Title.Contains(keyword) ||
                           (t.Description != null && t.Description.Contains(keyword)) ||
                           t.StartLocation.Contains(keyword) ||
                           t.EndLocation.Contains(keyword));

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive && !t.IsDeleted);
            }

            return await query.OrderBy(t => t.Title).ToListAsync();
        }

        public async Task<TourTemplate?> GetWithDetailsAsync(Guid id)
        {
            return await _context.TourTemplates
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Images)
                // TODO: Uncomment when TourSlots and TourDetails are implemented
                // .Include(t => t.TourSlots)
                // .Include(t => t.TourDetails)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<IEnumerable<TourTemplate>> GetPopularTemplatesAsync(int top = 10)
        {
            // TODO: Implement logic based on usage statistics when TourSlots are available
            // For now, return most recently created active templates
            return await _context.TourTemplates
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Images)
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .Take(top)
                .ToListAsync();
        }

        public async Task<bool> IsTemplateInUseAsync(Guid id)
        {
            // TODO: Check if template has any TourSlots when that entity is implemented
            // For now, return false
            return await Task.FromResult(false);
        }

        public async Task<(IEnumerable<TourTemplate> Templates, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            TourTemplateType? templateType = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? startLocation = null,
            bool includeInactive = false)
        {
            var query = _context.TourTemplates
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Images)
                .AsQueryable();

            // Apply filters
            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive && !t.IsDeleted);
            }

            if (templateType.HasValue)
            {
                query = query.Where(t => t.TemplateType == templateType.Value);
            }



            if (!string.IsNullOrEmpty(startLocation))
            {
                query = query.Where(t => t.StartLocation.Contains(startLocation));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var templates = await query
                .OrderBy(t => t.Title)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (templates, totalCount);
        }
    }
}
