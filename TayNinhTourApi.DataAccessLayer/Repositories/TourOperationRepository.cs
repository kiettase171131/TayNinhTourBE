using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourOperation entity
    /// Kế thừa từ GenericRepository và implement ITourOperationRepository
    /// </summary>
    public class TourOperationRepository : GenericRepository<TourOperation>, ITourOperationRepository
    {
        public TourOperationRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<TourOperation?> GetByTourSlotAsync(Guid tourSlotId)
        {
            return await _context.TourOperations
                .Include(to => to.TourSlot)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .FirstOrDefaultAsync(to => to.TourSlotId == tourSlotId && !to.IsDeleted);
        }

        public async Task<IEnumerable<TourOperation>> GetByStatusAsync(TourOperationStatus status, bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourSlot)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.Status == status);

            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            return await query.OrderBy(to => to.TourSlot.TourDate).ToListAsync();
        }

        public async Task<IEnumerable<TourOperation>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourSlot)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.TourSlot.TourDate >= startDate && to.TourSlot.TourDate <= endDate);

            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            return await query.OrderBy(to => to.TourSlot.TourDate).ToListAsync();
        }

        public async Task<TourOperation?> GetWithDetailsAsync(Guid id)
        {
            return await _context.TourOperations
                .Include(to => to.TourSlot)
                    .ThenInclude(ts => ts.TourTemplate)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .FirstOrDefaultAsync(to => to.Id == id && !to.IsDeleted);
        }

        public async Task<IEnumerable<TourOperation>> GetByGuideAndDateRangeAsync(Guid guideId, DateOnly startDate, DateOnly endDate, bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourSlot)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.GuideId == guideId && 
                            to.TourSlot.TourDate >= startDate && 
                            to.TourSlot.TourDate <= endDate);

            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            return await query.OrderBy(to => to.TourSlot.TourDate).ToListAsync();
        }

        public async Task<bool> IsGuideBusyOnDateAsync(Guid guideId, DateOnly tourDate, Guid? excludeOperationId = null)
        {
            var query = _context.TourOperations
                .Where(to => to.GuideId == guideId && 
                            to.TourSlot.TourDate == tourDate && 
                            to.IsActive && 
                            !to.IsDeleted);

            if (excludeOperationId.HasValue)
            {
                query = query.Where(to => to.Id != excludeOperationId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<TourOperation>> GetUpcomingOperationsByGuideAsync(Guid guideId, int top = 10)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            return await _context.TourOperations
                .Include(to => to.TourSlot)
                    .ThenInclude(ts => ts.TourTemplate)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.GuideId == guideId && 
                            to.TourSlot.TourDate >= today && 
                            to.IsActive && 
                            !to.IsDeleted)
                .OrderBy(to => to.TourSlot.TourDate)
                .Take(top)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourOperation>> GetByGuideAsync(Guid guideId, bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourSlot)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.GuideId == guideId);

            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            return await query.OrderByDescending(to => to.TourSlot.TourDate).ToListAsync();
        }

        public async Task<IEnumerable<TourOperation>> GetOperationsByGuideAsync(Guid guideId, bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourSlot)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.GuideId == guideId);

            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            return await query.OrderByDescending(to => to.TourSlot.TourDate).ToListAsync();
        }

        public async Task<int> CountByGuideAndMonthAsync(Guid guideId, int year, int month, bool includeInactive = false)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var query = _context.TourOperations
                .Where(to => to.GuideId == guideId && 
                            to.TourSlot.TourDate >= startDate && 
                            to.TourSlot.TourDate <= endDate);

            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            return await query.CountAsync();
        }

        public async Task<bool> CanDeleteOperationAsync(Guid id)
        {
            var operation = await _context.TourOperations
                .Include(to => to.TourSlot)
                .FirstOrDefaultAsync(to => to.Id == id && !to.IsDeleted);

            if (operation == null)
                return false;

            // Check if operation is in the past
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (operation.TourSlot.TourDate < today)
                return false;

            // Check if operation is already completed or cancelled
            if (operation.Status == TourOperationStatus.Completed || 
                operation.Status == TourOperationStatus.Cancelled)
                return false;

            return true;
        }

        public async Task<(IEnumerable<TourOperation> Operations, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? guideId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourSlot)
                    .ThenInclude(ts => ts.TourTemplate)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .AsQueryable();

            // Apply filters
            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            if (guideId.HasValue)
            {
                query = query.Where(to => to.GuideId == guideId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(to => to.TourSlot.TourDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(to => to.TourSlot.TourDate <= toDate.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var operations = await query
                .OrderBy(to => to.TourSlot.TourDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (operations, totalCount);
        }

        public async Task<IEnumerable<TourOperation>> GetActiveOperationsAsync(DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var query = _context.TourOperations
                .Include(to => to.TourSlot)
                    .ThenInclude(ts => ts.TourTemplate)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.Status == TourOperationStatus.Scheduled ||
                            to.Status == TourOperationStatus.InProgress);

            if (fromDate.HasValue)
            {
                query = query.Where(to => to.TourSlot.TourDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(to => to.TourSlot.TourDate <= toDate.Value);
            }

            return await query
                .Where(to => to.IsActive && !to.IsDeleted)
                .OrderBy(to => to.TourSlot.TourDate)
                .ToListAsync();
        }

        public async Task<object> GetGuideStatisticsAsync(Guid guideId, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var query = _context.TourOperations
                .Where(to => to.GuideId == guideId && to.IsActive && !to.IsDeleted);

            if (fromDate.HasValue)
            {
                query = query.Where(to => to.TourSlot.TourDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(to => to.TourSlot.TourDate <= toDate.Value);
            }

            var totalOperations = await query.CountAsync();
            var completedOperations = await query.CountAsync(to => to.Status == TourOperationStatus.Completed);
            var cancelledOperations = await query.CountAsync(to => to.Status == TourOperationStatus.Cancelled);
            var upcomingOperations = await query.CountAsync(to => to.Status == TourOperationStatus.Scheduled);

            return new
            {
                TotalOperations = totalOperations,
                CompletedOperations = completedOperations,
                CancelledOperations = cancelledOperations,
                UpcomingOperations = upcomingOperations,
                CompletionRate = totalOperations > 0 ? (double)completedOperations / totalOperations * 100 : 0
            };
        }

        public async Task<IEnumerable<Guid>> GetAvailableGuidesOnDateAsync(DateOnly tourDate)
        {
            // Get all guides who are NOT busy on the specified date
            var busyGuideIds = await _context.TourOperations
                .Where(to => to.TourSlot.TourDate == tourDate &&
                            to.IsActive &&
                            !to.IsDeleted)
                .Select(to => to.GuideId)
                .ToListAsync();

            // TODO: This should query from Users table where Role is Guide
            // For now, return empty list as we don't have guide role filtering implemented
            return new List<Guid>();
        }
    }
}
