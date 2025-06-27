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

        public async Task<TourOperation?> GetByTourDetailsAsync(Guid tourDetailsId)
        {
            return await _context.TourOperations
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.AssignedSlots)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .FirstOrDefaultAsync(to => to.TourDetailsId == tourDetailsId && !to.IsDeleted);
        }

        public async Task<IEnumerable<TourOperation>> GetByStatusAsync(TourOperationStatus status, bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.Status == status);

            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            return await query.OrderBy(to => to.TourDetails.Title).ToListAsync();
        }

        public async Task<TourOperation?> GetWithDetailsAsync(Guid id)
        {
            return await _context.TourOperations
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.Timeline)
                        .ThenInclude(ti => ti.SpecialtyShop)
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.AssignedSlots)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .FirstOrDefaultAsync(to => to.Id == id && !to.IsDeleted);
        }

        public async Task<IEnumerable<TourOperation>> GetByGuideAsync(Guid guideId, bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.GuideId == guideId);

            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            return await query.OrderBy(to => to.TourDetails.Title).ToListAsync();
        }

        public async Task<bool> CanDeleteOperationAsync(Guid id)
        {
            var operation = await _context.TourOperations
                .Include(to => to.TourDetails)
                .FirstOrDefaultAsync(to => to.Id == id && !to.IsDeleted);

            if (operation == null)
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
            Guid? tourTemplateId = null,
            bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
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

            if (tourTemplateId.HasValue)
            {
                query = query.Where(to => to.TourDetails.TourTemplateId == tourTemplateId.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination (pageIndex is 0-based from frontend)
            var operations = await query
                .OrderBy(to => to.TourDetails.Title)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (operations, totalCount);
        }

        public async Task<IEnumerable<TourOperation>> GetOperationsByDateAsync(DateOnly date, bool includeInactive = false)
        {
            var query = _context.TourOperations
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(to => to.TourDetails)
                    .ThenInclude(td => td.AssignedSlots)
                .Include(to => to.Guide)
                .Include(to => to.CreatedBy)
                .Include(to => to.UpdatedBy)
                .Where(to => to.TourDetails.AssignedSlots.Any(slot => slot.TourDate == date));

            if (!includeInactive)
            {
                query = query.Where(to => to.IsActive && !to.IsDeleted);
            }

            return await query.OrderBy(to => to.TourDetails.Title).ToListAsync();
        }
    }
}
