using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourGuideInvitation entity
    /// </summary>
    public class TourGuideInvitationRepository : GenericRepository<TourGuideInvitation>, ITourGuideInvitationRepository
    {
        public TourGuideInvitationRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TourGuideInvitation>> GetByTourDetailsAsync(Guid tourDetailsId)
        {
            return await _context.TourGuideInvitations
                .Include(i => i.TourGuide)
                .Include(i => i.CreatedBy)
                .Where(i => i.TourDetailsId == tourDetailsId && !i.IsDeleted)
                .OrderByDescending(i => i.InvitedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourGuideInvitation>> GetByGuideAsync(Guid guideId, InvitationStatus? status = null)
        {
            var query = _context.TourGuideInvitations
                .Include(i => i.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(i => i.CreatedBy)
                .Where(i => i.GuideId == guideId && !i.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            return await query
                .OrderByDescending(i => i.InvitedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourGuideInvitation>> GetPendingInvitationsAsync()
        {
            return await _context.TourGuideInvitations
                .Include(i => i.TourDetails)
                .Include(i => i.TourGuide)
                .Where(i => i.Status == InvitationStatus.Pending
                           && i.ExpiresAt > DateTime.UtcNow
                           && !i.IsDeleted)
                .OrderBy(i => i.ExpiresAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourGuideInvitation>> GetExpiredInvitationsAsync()
        {
            return await _context.TourGuideInvitations
                .Where(i => i.Status == InvitationStatus.Pending
                           && i.ExpiresAt <= DateTime.UtcNow
                           && !i.IsDeleted)
                .ToListAsync();
        }

        public async Task<int> ExpireInvitationsForTourDetailsAsync(Guid tourDetailsId, Guid? excludeInvitationId = null)
        {
            var query = _context.TourGuideInvitations
                .Where(i => i.TourDetailsId == tourDetailsId
                           && i.Status == InvitationStatus.Pending
                           && !i.IsDeleted);

            if (excludeInvitationId.HasValue)
            {
                query = query.Where(i => i.Id != excludeInvitationId.Value);
            }

            var invitationsToExpire = await query.ToListAsync();

            foreach (var invitation in invitationsToExpire)
            {
                invitation.Status = InvitationStatus.Expired;
                invitation.UpdatedAt = DateTime.UtcNow;
                _context.Set<TourGuideInvitation>().Update(invitation);
            }

            // Don't save here - let UnitOfWork handle it
            return invitationsToExpire.Count;
        }

        public async Task<bool> HasPendingInvitationAsync(Guid tourDetailsId, Guid guideId)
        {
            return await _context.TourGuideInvitations
                .AnyAsync(i => i.TourDetailsId == tourDetailsId
                              && i.GuideId == guideId
                              && i.Status == InvitationStatus.Pending
                              && !i.IsDeleted);
        }

        public async Task<TourGuideInvitation?> GetWithDetailsAsync(Guid id)
        {
            return await _context.TourGuideInvitations
                .Include(i => i.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(i => i.TourGuide)
                .Include(i => i.CreatedBy)
                .Include(i => i.UpdatedBy)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }
    }
}
