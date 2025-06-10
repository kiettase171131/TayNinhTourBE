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

        public async Task<IEnumerable<TourSlot>> GetByTourTemplateAsync(Guid tourTemplateId)
        {
            return await _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.TourDetails)
                .Where(ts => ts.TourTemplateId == tourTemplateId)
                .OrderBy(ts => ts.TourDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourSlot>> GetByTourDetailsAsync(Guid tourDetailsId)
        {
            return await _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.TourDetails)
                .Where(ts => ts.TourDetailsId == tourDetailsId)
                .OrderBy(ts => ts.TourDate)
                .ToListAsync();
        }

        public async Task<TourSlot?> GetByDateAsync(Guid tourTemplateId, DateOnly date)
        {
            return await _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.TourDetails)
                .FirstOrDefaultAsync(ts => ts.TourTemplateId == tourTemplateId && ts.TourDate == date);
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
                .Include(ts => ts.TourDetails)
                .AsQueryable();

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

            if (!includeInactive)
            {
                query = query.Where(ts => ts.IsActive);
            }

            return await query.OrderBy(ts => ts.TourDate).ToListAsync();
        }

        public async Task<bool> SlotExistsAsync(Guid tourTemplateId, DateOnly date)
        {
            return await _context.TourSlots
                .AnyAsync(ts => ts.TourTemplateId == tourTemplateId && ts.TourDate == date);
        }

        public async Task<int> BulkUpdateStatusAsync(IEnumerable<Guid> slotIds, TourSlotStatus status)
        {
            var slots = await _context.TourSlots
                .Where(ts => slotIds.Contains(ts.Id))
                .ToListAsync();

            foreach (var slot in slots)
            {
                slot.Status = status;
                slot.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return slots.Count;
        }
    }
}
