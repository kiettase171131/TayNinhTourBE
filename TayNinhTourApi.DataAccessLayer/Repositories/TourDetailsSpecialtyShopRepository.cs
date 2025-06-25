using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourDetailsSpecialtyShop entity
    /// </summary>
    public class TourDetailsSpecialtyShopRepository : GenericRepository<TourDetailsSpecialtyShop>, ITourDetailsSpecialtyShopRepository
    {
        public TourDetailsSpecialtyShopRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TourDetailsSpecialtyShop>> GetByTourDetailsIdAsync(Guid tourDetailsId)
        {
            return await _context.TourDetailsSpecialtyShops
                .Include(tdss => tdss.SpecialtyShop)
                .Include(tdss => tdss.CreatedBy)
                .Include(tdss => tdss.UpdatedBy)
                .Where(tdss => tdss.TourDetailsId == tourDetailsId && tdss.IsActive)
                .OrderBy(tdss => tdss.Priority)
                .ThenBy(tdss => tdss.InvitedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourDetailsSpecialtyShop>> GetBySpecialtyShopIdAsync(Guid specialtyShopId)
        {
            return await _context.TourDetailsSpecialtyShops
                .Include(tdss => tdss.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(tdss => tdss.CreatedBy)
                .Include(tdss => tdss.UpdatedBy)
                .Where(tdss => tdss.SpecialtyShopId == specialtyShopId && tdss.IsActive)
                .OrderByDescending(tdss => tdss.InvitedAt)
                .ToListAsync();
        }

        public async Task<TourDetailsSpecialtyShop?> GetByTourDetailsAndShopAsync(Guid tourDetailsId, Guid specialtyShopId)
        {
            return await _context.TourDetailsSpecialtyShops
                .Include(tdss => tdss.TourDetails)
                .Include(tdss => tdss.SpecialtyShop)
                .Include(tdss => tdss.CreatedBy)
                .Include(tdss => tdss.UpdatedBy)
                .FirstOrDefaultAsync(tdss => 
                    tdss.TourDetailsId == tourDetailsId && 
                    tdss.SpecialtyShopId == specialtyShopId && 
                    tdss.IsActive);
        }

        public async Task<IEnumerable<TourDetailsSpecialtyShop>> GetByStatusAsync(ShopInvitationStatus status)
        {
            return await _context.TourDetailsSpecialtyShops
                .Include(tdss => tdss.TourDetails)
                    .ThenInclude(td => td.TourTemplate)
                .Include(tdss => tdss.SpecialtyShop)
                .Include(tdss => tdss.CreatedBy)
                .Where(tdss => tdss.Status == status && tdss.IsActive)
                .OrderByDescending(tdss => tdss.InvitedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TourDetailsSpecialtyShop>> GetExpiredInvitationsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.TourDetailsSpecialtyShops
                .Include(tdss => tdss.TourDetails)
                .Include(tdss => tdss.SpecialtyShop)
                .Where(tdss => 
                    tdss.Status == ShopInvitationStatus.Pending && 
                    tdss.ExpiresAt < now && 
                    tdss.IsActive)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(Guid id, ShopInvitationStatus status, string? responseNote, Guid updatedById)
        {
            var invitation = await _context.TourDetailsSpecialtyShops
                .FirstOrDefaultAsync(tdss => tdss.Id == id && tdss.IsActive);

            if (invitation == null)
                return false;

            invitation.Status = status;
            invitation.ResponseNote = responseNote;
            invitation.RespondedAt = DateTime.UtcNow;
            invitation.UpdatedById = updatedById;
            invitation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteByTourDetailsIdAsync(Guid tourDetailsId)
        {
            var invitations = await _context.TourDetailsSpecialtyShops
                .Where(tdss => tdss.TourDetailsId == tourDetailsId && tdss.IsActive)
                .ToListAsync();

            foreach (var invitation in invitations)
            {
                invitation.IsActive = false;
                invitation.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return invitations.Count;
        }
    }
}
