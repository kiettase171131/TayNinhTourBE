using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TimelineItem entity
    /// Kế thừa từ GenericRepository và implement ITimelineItemRepository
    /// </summary>
    public class TimelineItemRepository : GenericRepository<TimelineItem>, ITimelineItemRepository
    {
        public TimelineItemRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TimelineItem>> GetByTourDetailsOrderedAsync(Guid tourDetailsId, bool includeInactive = false)
        {
            var query = _context.TimelineItems
                .Include(ti => ti.TourDetails)
                .Include(ti => ti.SpecialtyShop)
                .Include(ti => ti.CreatedBy)
                .Include(ti => ti.UpdatedBy)
                .Where(ti => ti.TourDetailsId == tourDetailsId);

            if (!includeInactive)
            {
                query = query.Where(ti => ti.IsActive && !ti.IsDeleted);
            }

            return await query
                .OrderBy(ti => ti.SortOrder)
                .ThenBy(ti => ti.CheckInTime)
                .ToListAsync();
        }

        public async Task<TimelineItem?> GetWithDetailsAsync(Guid id)
        {
            return await _context.TimelineItems
                .Include(ti => ti.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(ti => ti.SpecialtyShop)
                .Include(ti => ti.CreatedBy)
                .Include(ti => ti.UpdatedBy)
                .FirstOrDefaultAsync(ti => ti.Id == id && !ti.IsDeleted);
        }

        public async Task<IEnumerable<TimelineItem>> GetBySpecialtyShopAsync(Guid specialtyShopId, bool includeInactive = false)
        {
            var query = _context.TimelineItems
                .Include(ti => ti.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(ti => ti.SpecialtyShop)
                .Where(ti => ti.SpecialtyShopId == specialtyShopId);

            if (!includeInactive)
            {
                query = query.Where(ti => ti.IsActive && !ti.IsDeleted);
            }

            return await query
                .OrderBy(ti => ti.TourDetails.Title)
                .ThenBy(ti => ti.SortOrder)
                .ToListAsync();
        }

        public async Task<int> GetMaxSortOrderAsync(Guid tourDetailsId)
        {
            var maxSortOrder = await _context.TimelineItems
                .Where(ti => ti.TourDetailsId == tourDetailsId && !ti.IsDeleted)
                .MaxAsync(ti => (int?)ti.SortOrder);

            return maxSortOrder ?? 0;
        }

        public async Task<bool> ExistsBySortOrderAsync(Guid tourDetailsId, int sortOrder, Guid? excludeId = null)
        {
            var query = _context.TimelineItems
                .Where(ti => ti.TourDetailsId == tourDetailsId &&
                            ti.SortOrder == sortOrder &&
                            !ti.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(ti => ti.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task UpdateSortOrdersAsync(Guid tourDetailsId, int fromSortOrder, int increment)
        {
            var itemsToUpdate = await _context.TimelineItems
                .Where(ti => ti.TourDetailsId == tourDetailsId &&
                            ti.SortOrder >= fromSortOrder &&
                            !ti.IsDeleted)
                .ToListAsync();

            foreach (var item in itemsToUpdate)
            {
                item.SortOrder += increment;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TimelineItem>> SearchAsync(string keyword, Guid? tourDetailsId = null, bool includeInactive = false)
        {
            var query = _context.TimelineItems
                .Include(ti => ti.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(ti => ti.SpecialtyShop)
                .Where(ti => ti.Activity.Contains(keyword) ||
                           (ti.SpecialtyShop != null && ti.SpecialtyShop.ShopName.Contains(keyword)));

            if (tourDetailsId.HasValue)
            {
                query = query.Where(ti => ti.TourDetailsId == tourDetailsId.Value);
            }

            if (!includeInactive)
            {
                query = query.Where(ti => ti.IsActive && !ti.IsDeleted);
            }

            return await query
                .OrderBy(ti => ti.TourDetails.Title)
                .ThenBy(ti => ti.SortOrder)
                .ToListAsync();
        }
    }
}
