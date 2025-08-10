using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface ITourFeedbackRepository : IGenericRepository<TourFeedback>
    {
        Task<TourFeedback?> GetByBookingAsync(Guid bookingId, CancellationToken ct = default);
        Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken ct = default);

        Task<(IReadOnlyList<TourFeedback> Items, int Total)> GetByGuideAsync(Guid guideId, int page, int size, CancellationToken ct = default);
        Task<(IReadOnlyList<TourFeedback> Items, int Total)> GetBySlotAsync(Guid slotId, int page, int size, CancellationToken ct = default);
        Task<(IReadOnlyList<TourFeedback> Items, int Total)> GetByUserAsync(Guid userId, int page, int size, CancellationToken ct = default);
    }
}
