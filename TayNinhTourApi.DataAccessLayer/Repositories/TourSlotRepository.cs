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
                    .ThenInclude(td => td.TourOperation)
                .Where(ts => ts.TourTemplateId == tourTemplateId)
                .OrderBy(ts => ts.TourDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourSlot>> GetByTourDetailsAsync(Guid tourDetailsId)
        {
            return await _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.TourDetails)
                    .ThenInclude(td => td.TourOperation)
                .Where(ts => ts.TourDetailsId == tourDetailsId)
                .OrderBy(ts => ts.TourDate)
                .ToListAsync();
        }

        public async Task<TourSlot?> GetByDateAsync(Guid tourTemplateId, DateOnly date)
        {
            return await _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.TourDetails)
                    .ThenInclude(td => td.TourOperation)
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
                    .ThenInclude(td => td.TourOperation)
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

        /// <summary>
        /// Atomic reserve capacity for a slot using database-level concurrency control
        /// CHỈ dùng khi thanh toán thành công - CỘNG CurrentBookings
        /// </summary>
        public async Task<bool> AtomicReserveCapacityAsync(Guid slotId, int guestsToReserve)
        {
            var sql = @"
                UPDATE TourSlots 
                SET 
                    CurrentBookings = CurrentBookings + @guestsToReserve,
                    UpdatedAt = @updateTime,
                    Status = CASE 
                        WHEN (CurrentBookings + @guestsToReserve) >= MaxGuests THEN @fullyBookedStatus
                        ELSE Status 
                    END
                WHERE 
                    Id = @slotId 
                    AND IsDeleted = 0 
                    AND IsActive = 1 
                    AND Status = @availableStatus
                    AND (CurrentBookings + @guestsToReserve) <= MaxGuests";

            var parameters = new[]
            {
                new MySqlConnector.MySqlParameter("@slotId", slotId.ToString()),
                new MySqlConnector.MySqlParameter("@guestsToReserve", guestsToReserve),
                new MySqlConnector.MySqlParameter("@updateTime", DateTime.UtcNow),
                new MySqlConnector.MySqlParameter("@availableStatus", (int)TourSlotStatus.Available),
                new MySqlConnector.MySqlParameter("@fullyBookedStatus", (int)TourSlotStatus.FullyBooked)
            };

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// CHỈ CHECK capacity, KHÔNG cộng CurrentBookings
        /// Dùng khi tạo booking để kiểm tra slot có đủ chỗ không
        /// </summary>
        public async Task<bool> CheckSlotCapacityAsync(Guid slotId, int guestsToCheck)
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM TourSlots 
                WHERE 
                    Id = @slotId 
                    AND IsDeleted = 0 
                    AND IsActive = 1 
                    AND Status = @availableStatus
                    AND (CurrentBookings + @guestsToCheck) <= MaxGuests";

            var parameters = new[]
            {
                new MySqlConnector.MySqlParameter("@slotId", slotId.ToString()),
                new MySqlConnector.MySqlParameter("@guestsToCheck", guestsToCheck),
                new MySqlConnector.MySqlParameter("@availableStatus", (int)TourSlotStatus.Available)
            };

            // ✅ Sửa lại: Dùng ExecuteSqlRaw để đếm, sau đó kiểm tra kết quả
            var result = await _context.TourSlots
                .FromSqlRaw(@"
                    SELECT * FROM TourSlots 
                    WHERE 
                        Id = @slotId 
                        AND IsDeleted = 0 
                        AND IsActive = 1 
                        AND Status = @availableStatus
                        AND (CurrentBookings + @guestsToCheck) <= MaxGuests", parameters)
                .CountAsync();

            return result > 0;
        }

        /// <summary>
        /// Get slot with current capacity info for booking validation
        /// </summary>
        public async Task<TourSlot?> GetSlotWithCapacityAsync(Guid slotId)
        {
            return await _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Include(ts => ts.TourDetails)
                    .ThenInclude(td => td.TourOperation)
                .Where(ts => ts.Id == slotId && !ts.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy các slots template chưa được assign tour details (slots gốc được tạo từ template)
        /// </summary>
        public async Task<IEnumerable<TourSlot>> GetUnassignedTemplateSlotsByTemplateAsync(Guid tourTemplateId, bool includeInactive = false)
        {
            var query = _context.TourSlots
                .Include(ts => ts.TourTemplate)
                .Where(ts => ts.TourTemplateId == tourTemplateId && ts.TourDetailsId == null);

            if (!includeInactive)
            {
                query = query.Where(ts => ts.IsActive);
            }

            return await query.OrderBy(ts => ts.TourDate).ToListAsync();
        }
    }
}
