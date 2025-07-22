using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
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
                .Include(td => td.TourOperation)
                .Include(td => td.Timeline)
                .Include(td => td.AssignedSlots)
                .Include(td => td.CreatedBy)
                    .ThenInclude(u => u.TourCompany)
                .Include(td => td.UpdatedBy)
                .Where(td => td.TourTemplateId == tourTemplateId);

            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive && !td.IsDeleted);
            }

            return await query
                .OrderBy(td => td.Title)
                .ToListAsync();
        }

        public async Task<TourDetails?> GetWithDetailsAsync(Guid id)
        {
            return await _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.TourOperation)
                .Include(td => td.Timeline)
                    .ThenInclude(ti => ti.SpecialtyShop)
                .Include(td => td.AssignedSlots)
                .Include(td => td.CreatedBy)
                    .ThenInclude(u => u.TourCompany)
                .Include(td => td.UpdatedBy)
                .FirstOrDefaultAsync(td => td.Id == id && !td.IsDeleted);
        }

        public async Task<IEnumerable<TourDetails>> SearchByTitleAsync(string title, bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.TourOperation)
                .Include(td => td.Timeline)
                .Include(td => td.AssignedSlots)
                .Include(td => td.CreatedBy)
                    .ThenInclude(u => u.TourCompany)
                .Include(td => td.UpdatedBy)
                .Where(td => td.Title.Contains(title));

            if (!includeInactive)
            {
                query = query.Where(td => td.IsActive && !td.IsDeleted);
            }

            return await query
                .OrderBy(td => td.Title)
                .ToListAsync();
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

        public async Task<TourDetails?> GetByTitleAsync(Guid tourTemplateId, string title)
        {
            return await _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.TourOperation)
                .Include(td => td.Timeline)
                .Include(td => td.AssignedSlots)
                .Include(td => td.CreatedBy)
                    .ThenInclude(u => u.TourCompany)
                .Include(td => td.UpdatedBy)
                .FirstOrDefaultAsync(td => td.TourTemplateId == tourTemplateId &&
                                          td.Title == title &&
                                          !td.IsDeleted);
        }

        public async Task<(IEnumerable<TourDetails> Details, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? tourTemplateId = null,
            string? titleFilter = null,
            bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.TourOperation)
                .Include(td => td.Timeline)
                .Include(td => td.AssignedSlots)
                .Include(td => td.CreatedBy)
                    .ThenInclude(u => u.TourCompany)
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

            if (!string.IsNullOrEmpty(titleFilter))
            {
                query = query.Where(td => td.Title.Contains(titleFilter));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination (pageIndex is 0-based from frontend)
            var details = await query
                .OrderBy(td => td.TourTemplate.Title)
                .ThenBy(td => td.Title)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (details, totalCount);
        }

        public async Task<bool> CanDeleteDetailAsync(Guid id)
        {
            // Check if detail has slots with bookings
            var hasBookedSlots = await _context.TourSlots
                .AnyAsync(ts => ts.TourDetailsId == id && ts.Status != TourSlotStatus.Available);

            // Check if detail has operations with bookings or assigned guides
            var hasActiveOperations = await _context.TourOperations
                .AnyAsync(to => to.TourDetailsId == id &&
                              !to.IsDeleted &&
                              (to.CurrentBookings > 0 || to.TourGuideId != null));

            // Can delete if no booked slots and no active operations with bookings/guides
            return !hasBookedSlots && !hasActiveOperations;
        }

        public async Task<IEnumerable<TourDetails>> SearchAsync(string keyword, Guid? tourTemplateId = null, bool includeInactive = false)
        {
            var query = _context.TourDetails
                .Include(td => td.TourTemplate)
                .Include(td => td.TourOperation)
                .Include(td => td.Timeline)
                .Include(td => td.AssignedSlots)
                .Include(td => td.CreatedBy)
                    .ThenInclude(u => u.TourCompany)
                .Include(td => td.UpdatedBy)
                .Where(td => td.Title.Contains(keyword) ||
                           (td.Description != null && td.Description.Contains(keyword)));

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
                .ThenBy(td => td.Title)
                .ToListAsync();
        }

        public async Task<bool> ExistsByTitleAsync(Guid tourTemplateId, string title, Guid? excludeId = null)
        {
            var query = _context.TourDetails
                .Where(td => td.TourTemplateId == tourTemplateId &&
                            td.Title == title &&
                            !td.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(td => td.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
