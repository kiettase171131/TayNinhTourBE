using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourIncident entity
    /// Kế thừa từ GenericRepository và implement ITourIncidentRepository
    /// </summary>
    public class TourIncidentRepository : GenericRepository<TourIncident>, ITourIncidentRepository
    {
        public TourIncidentRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TourIncident>> GetByTourOperationAsync(Guid tourOperationId)
        {
            return await _context.TourIncidents
                .Include(i => i.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                .Include(i => i.ReportedByGuide)
                .Where(i => i.TourOperationId == tourOperationId && !i.IsDeleted)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourIncident>> GetByTourGuideAsync(Guid guideId)
        {
            return await _context.TourIncidents
                .Include(i => i.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                .Include(i => i.ReportedByGuide)
                .Where(i => i.ReportedByGuideId == guideId && !i.IsDeleted)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourIncident>> GetBySeverityAsync(string severity)
        {
            return await _context.TourIncidents
                .Include(i => i.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                .Include(i => i.ReportedByGuide)
                .Where(i => i.Severity == severity && !i.IsDeleted)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourIncident>> GetByStatusAsync(string status)
        {
            return await _context.TourIncidents
                .Include(i => i.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                .Include(i => i.ReportedByGuide)
                .Where(i => i.Status == status && !i.IsDeleted)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourIncident>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.TourIncidents
                .Include(i => i.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                .Include(i => i.ReportedByGuide)
                .Where(i => i.ReportedAt >= fromDate && i.ReportedAt <= toDate && !i.IsDeleted)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }
    }
}
