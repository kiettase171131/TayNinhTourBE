using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourSlot entity
    /// Kế thừa từ GenericRepository và implement ITourSlotRepository
    /// </summary>
    public class TourSlotRepository : GenericRepository<TourSlot>, ITourSlotRepository
    {
        public TourSlotRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TourSlot>> GetByTourTemplateAsync(Guid tourTemplateId, bool includeInactive = false)
        {
            var query = _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                .Where(ts => ts.TourTemplateId == tourTemplateId);

            if (!includeInactive)
            {
                query = query.Where(ts => ts.IsActive && !ts.IsDeleted);
            }

            return await query.OrderBy(ts => ts.TourDate).ToListAsync();
        }

        public async Task<IEnumerable<TourSlot>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, bool includeInactive = false)
        {
            var query = _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                .Where(ts => ts.TourDate >= startDate && ts.TourDate <= endDate);

            if (!includeInactive)
            {
                query = query.Where(ts => ts.IsActive && !ts.IsDeleted);
            }

            return await query.OrderBy(ts => ts.TourDate).ToListAsync();
        }

        public async Task<IEnumerable<TourSlot>> GetByScheduleDayAsync(ScheduleDay scheduleDay, bool includeInactive = false)
        {
            var query = _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                .Where(ts => ts.ScheduleDay == scheduleDay);

            if (!includeInactive)
            {
                query = query.Where(ts => ts.IsActive && !ts.IsDeleted);
            }

            return await query.OrderBy(ts => ts.TourDate).ToListAsync();
        }

        public async Task<IEnumerable<TourSlot>> GetByStatusAsync(TourSlotStatus status, bool includeInactive = false)
        {
            var query = _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                .Where(ts => ts.Status == status);

            if (!includeInactive)
            {
                query = query.Where(ts => ts.IsActive && !ts.IsDeleted);
            }

            return await query.OrderBy(ts => ts.TourDate).ToListAsync();
        }

        public async Task<TourSlot?> GetByTemplateAndDateAsync(Guid tourTemplateId, DateOnly tourDate)
        {
            return await _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                .FirstOrDefaultAsync(ts => ts.TourTemplateId == tourTemplateId && 
                                          ts.TourDate == tourDate && 
                                          !ts.IsDeleted);
        }

        public async Task<TourSlot?> GetWithDetailsAsync(Guid id)
        {
            return await _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                // TODO: Include TourOperations when relationship is established
                // .Include(ts => ts.TourOperations)
                .FirstOrDefaultAsync(ts => ts.Id == id && !ts.IsDeleted);
        }

        public async Task<int> CountAvailableSlotsAsync(Guid tourTemplateId, DateOnly fromDate, DateOnly toDate)
        {
            return await _context.TourSlots
                .Where(ts => ts.TourTemplateId == tourTemplateId &&
                            ts.TourDate >= fromDate &&
                            ts.TourDate <= toDate &&
                            ts.Status == TourSlotStatus.Available &&
                            ts.IsActive &&
                            !ts.IsDeleted)
                .CountAsync();
        }

        public async Task<IEnumerable<TourSlot>> GetAvailableSlotsAsync(
            Guid? tourTemplateId = null,
            ScheduleDay? scheduleDay = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            bool includeInactive = false)
        {
            var query = _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                .Where(ts => ts.Status == TourSlotStatus.Available);

            if (!includeInactive)
            {
                query = query.Where(ts => ts.IsActive && !ts.IsDeleted);
            }

            if (tourTemplateId.HasValue)
            {
                query = query.Where(ts => ts.TourTemplateId == tourTemplateId.Value);
            }

            if (scheduleDay.HasValue)
            {
                query = query.Where(ts => (ts.TourTemplate.ScheduleDays & scheduleDay.Value) == scheduleDay.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(ts => ts.TourDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(ts => ts.TourDate <= toDate.Value);
            }

            return await query.OrderBy(ts => ts.TourDate).ToListAsync();
        }

        public async Task<IEnumerable<TourSlot>> GetUpcomingSlotsAsync(Guid? tourTemplateId = null, int top = 10)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var query = _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                .Where(ts => ts.TourDate >= today && 
                            ts.IsActive && 
                            !ts.IsDeleted);

            if (tourTemplateId.HasValue)
            {
                query = query.Where(ts => ts.TourTemplateId == tourTemplateId.Value);
            }

            return await query
                .OrderBy(ts => ts.TourDate)
                .Take(top)
                .ToListAsync();
        }

        public async Task<bool> HasTourOperationAsync(Guid id)
        {
            // TODO: Check if tour slot has any TourOperations when that relationship is established
            // For now, return false
            return await Task.FromResult(false);
        }

        public async Task<(IEnumerable<TourSlot> Slots, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? tourTemplateId = null,
            ScheduleDay? scheduleDay = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            bool includeInactive = false)
        {
            var query = _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                .AsQueryable();

            // Apply filters
            if (!includeInactive)
            {
                query = query.Where(ts => ts.IsActive && !ts.IsDeleted);
            }

            if (tourTemplateId.HasValue)
            {
                query = query.Where(ts => ts.TourTemplateId == tourTemplateId.Value);
            }

            if (scheduleDay.HasValue)
            {
                query = query.Where(ts => ts.ScheduleDay == scheduleDay.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(ts => ts.TourDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(ts => ts.TourDate <= toDate.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var slots = await query
                .OrderBy(ts => ts.TourDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (slots, totalCount);
        }

        public async Task<bool> CanDeleteSlotAsync(Guid id)
        {
            // TODO: Check if slot has any TourOperations or bookings when those relationships are established
            // For now, return true
            return await Task.FromResult(true);
        }

        public async Task<IEnumerable<TourSlot>> GetAvailableSlotsAsync(DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var query = _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.CreatedBy)
                .Include(ts => ts.UpdatedBy)
                .Where(ts => ts.Status == TourSlotStatus.Available &&
                            ts.IsActive &&
                            !ts.IsDeleted);

            if (fromDate.HasValue)
            {
                query = query.Where(ts => ts.TourDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(ts => ts.TourDate <= toDate.Value);
            }

            return await query.OrderBy(ts => ts.TourDate).ToListAsync();
        }

        public async Task<bool> HasSlotsInDateRangeAsync(Guid tourTemplateId, DateOnly startDate, DateOnly endDate)
        {
            return await _context.TourSlots
                .AnyAsync(ts => ts.TourTemplateId == tourTemplateId &&
                               ts.TourDate >= startDate &&
                               ts.TourDate <= endDate &&
                               !ts.IsDeleted);
        }
    }
}
