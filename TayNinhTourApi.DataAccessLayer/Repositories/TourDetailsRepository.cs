using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourDetails entity
    /// Kế thừa từ GenericRepository và implement ITourDetailsRepository
    /// </summary>
    public class TourDetailsRepository : GenericRepository<TourDetails>, ITourDetailsRepository
    {
        public TourDetailsRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TourDetails>> GetByTourTemplateOrderedAsync(Guid tourTemplateId, bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .Where(td => td.TourTemplateId == tourTemplateId);

            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive && !td.IsDeleted);
            }

            return await query
                .OrderBy(td => td.SortOrder)
                .ThenBy(td => td.TimeSlot)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourDetails>> GetByShopAsync(Guid shopId, bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .Where(td => td.ShopId == shopId);

            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive && !td.IsDeleted);
            }

            return await query
                .OrderBy(td => td.TourTemplate.Title)
                .ThenBy(td => td.SortOrder)
                .ToListAsync();
        }

        public async Task<TourDetails?> GetWithDetailsAsync(Guid id)
        {
            return await _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .FirstOrDefaultAsync(td => td.Id == id && !td.IsDeleted);
        }

        public async Task<IEnumerable<TourDetails>> GetByTimeSlotAsync(TimeOnly timeSlot, bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .Where(td => td.TimeSlot == timeSlot);

            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive && !td.IsDeleted);
            }

            return await query
                .OrderBy(td => td.TourTemplate.Title)
                .ThenBy(td => td.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourDetails>> GetByDurationRangeAsync(int minDuration, int maxDuration, bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .Where(td => td.Duration >= minDuration && td.Duration <= maxDuration);

            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive && !td.IsDeleted);
            }

            return await query
                .OrderBy(td => td.Duration)
                .ToListAsync();
        }

        public async Task<TourDetails?> GetByTemplateAndSortOrderAsync(Guid tourTemplateId, int sortOrder)
        {
            return await _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .FirstOrDefaultAsync(td => td.TourTemplateId == tourTemplateId && 
                                          td.SortOrder == sortOrder && 
                                          !td.IsDeleted);
        }

        public async Task<int> GetMaxSortOrderAsync(Guid tourTemplateId)
        {
            var maxSortOrder = await _context.TourDetails
                .Where(td => td.TourTemplateId == tourTemplateId && !td.IsDeleted)
                .MaxAsync(td => (int?)td.SortOrder);

            return maxSortOrder ?? 0;
        }

        public async Task<int> CountByTourTemplateAsync(Guid tourTemplateId, bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Where(td => td.TourTemplateId == tourTemplateId && !td.IsDeleted);

            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive);
            }

            return await query.CountAsync();
        }

        public async Task UpdateSortOrdersAsync(Guid tourTemplateId, int fromSortOrder, int increment)
        {
            var detailsToUpdate = await _context.TourDetails
                .Where(td => td.TourTemplateId == tourTemplateId && 
                            td.SortOrder >= fromSortOrder && 
                            !td.IsDeleted)
                .ToListAsync();

            foreach (var detail in detailsToUpdate)
            {
                detail.SortOrder += increment;
                detail.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<TourDetails> Details, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? tourTemplateId = null,
            Guid? shopId = null,
            bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .AsQueryable();

            // Apply filters
            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive && !td.IsDeleted);
            }

            if (tourTemplateId.HasValue)
            {
                query = query.Where(td => td.TourTemplateId == tourTemplateId.Value);
            }

            if (shopId.HasValue)
            {
                query = query.Where(td => td.ShopId == shopId.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var details = await query
                .OrderBy(td => td.TourTemplate.Title)
                .ThenBy(td => td.SortOrder)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (details, totalCount);
        }

        public async Task<bool> CanDeleteDetailAsync(Guid id)
        {
            // TODO: Check if detail is referenced by any TourOperations when that relationship is established
            // For now, return true
            return await Task.FromResult(true);
        }

        public async Task<TourDetails?> GetLastDetailAsync(Guid tourTemplateId)
        {
            return await _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .Where(td => td.TourTemplateId == tourTemplateId && !td.IsDeleted)
                .OrderByDescending(td => td.SortOrder)
                .FirstOrDefaultAsync();
        }

        public async Task<TourDetails?> GetFirstDetailAsync(Guid tourTemplateId)
        {
            return await _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .Where(td => td.TourTemplateId == tourTemplateId && !td.IsDeleted)
                .OrderBy(td => td.SortOrder)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TourDetails>> GetByTimeRangeAsync(Guid tourTemplateId, TimeOnly startTime, TimeOnly endTime, bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .Where(td => td.TourTemplateId == tourTemplateId &&
                            td.TimeSlot >= startTime &&
                            td.TimeSlot <= endTime);

            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive && !td.IsDeleted);
            }

            return await query
                .OrderBy(td => td.SortOrder)
                .ThenBy(td => td.TimeSlot)
                .ToListAsync();
        }

        public async Task<bool> ExistsBySortOrderAsync(Guid tourTemplateId, int sortOrder, Guid? excludeId = null)
        {
            var query = _context.TourDetails
                .Where(td => td.TourTemplateId == tourTemplateId &&
                            td.SortOrder == sortOrder &&
                            !td.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(td => td.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<TourDetails>> SearchAsync(string keyword, Guid? tourTemplateId = null, bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.Shop)
                .Include(td => td.CreatedBy)
                .Include(td => td.UpdatedBy)
                .Where(td => td.Title.Contains(keyword) ||
                           (td.Description != null && td.Description.Contains(keyword)) ||
                           td.Shop.Name.Contains(keyword));

            if (tourTemplateId.HasValue)
            {
                query = query.Where(td => td.TourTemplateId == tourTemplateId.Value);
            }

            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive && !td.IsDeleted);
            }

            return await query
                .OrderBy(td => td.TourTemplate.Title)
                .ThenBy(td => td.SortOrder)
                .ToListAsync();
        }
    }
}
