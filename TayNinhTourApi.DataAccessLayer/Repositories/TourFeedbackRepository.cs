using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class TourFeedbackRepository : GenericRepository<TourFeedback>, ITourFeedbackRepository
    {
        public TourFeedbackRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }
        public Task<TourFeedback?> GetByBookingAsync(Guid bookingId, CancellationToken ct = default)
        => _context.Set<TourFeedback>().AsNoTracking()
            .FirstOrDefaultAsync(f => f.TourBookingId == bookingId, ct);

        public Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken ct = default)
            => _context.Set<TourFeedback>().AnyAsync(f => f.TourBookingId == bookingId, ct);

        public async Task<(IReadOnlyList<TourFeedback> Items, int Total)> GetByGuideAsync(Guid guideId, int page, int size, CancellationToken ct = default)
        {
            if (page <= 0) page = 1; if (size <= 0) size = 20;
            var q = _context.Set<TourFeedback>().AsNoTracking().Where(f => f.TourGuideId == guideId);
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(f => f.CreatedAt)
                               .Skip((page - 1) * size)
                               .Take(size)
                               .ToListAsync(ct);
            return (items, total);
        }

        public async Task<(IReadOnlyList<TourFeedback> Items, int Total)> GetBySlotAsync(Guid slotId, int page, int size, CancellationToken ct = default)
        {
            if (page <= 0) page = 1; if (size <= 0) size = 20;
            var q = _context.Set<TourFeedback>().AsNoTracking().Where(f => f.TourSlotId == slotId);
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(f => f.CreatedAt)
                               .Skip((page - 1) * size)
                               .Take(size)
                               .ToListAsync(ct);
            return (items, total);
        }

        public async Task<(IReadOnlyList<TourFeedback> Items, int Total)> GetByUserAsync(Guid userId, int page, int size, CancellationToken ct = default)
        {
            if (page <= 0) page = 1; if (size <= 0) size = 20;
            var q = _context.Set<TourFeedback>().AsNoTracking().Where(f => f.UserId == userId);
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(f => f.CreatedAt)
                               .Skip((page - 1) * size)
                               .Take(size)
                               .ToListAsync(ct);
            return (items, total);
        }
    }
}
