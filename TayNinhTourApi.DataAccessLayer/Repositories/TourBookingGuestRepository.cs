using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourBookingGuest entity
    /// </summary>
    public class TourBookingGuestRepository : GenericRepository<TourBookingGuest>, ITourBookingGuestRepository
    {
        public TourBookingGuestRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<List<TourBookingGuest>> GetGuestsByBookingIdAsync(Guid bookingId)
        {
            return await _context.TourBookingGuests
                .Where(g => g.TourBookingId == bookingId && !g.IsDeleted)
                .OrderBy(g => g.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<TourBookingGuest?> GetGuestByQRCodeAsync(string qrCodeData)
        {
            if (string.IsNullOrWhiteSpace(qrCodeData))
                return null;

            return await _context.TourBookingGuests
                .Include(g => g.TourBooking)
                    .ThenInclude(b => b.TourSlot)
                .Include(g => g.TourBooking)
                    .ThenInclude(b => b.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                .FirstOrDefaultAsync(g => g.QRCodeData == qrCodeData && !g.IsDeleted);
        }

        /// <inheritdoc />
        public async Task<bool> IsEmailUniqueInBookingAsync(Guid bookingId, string email, Guid? excludeGuestId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var normalizedEmail = email.Trim().ToLowerInvariant();

            var query = _context.TourBookingGuests
                .Where(g => g.TourBookingId == bookingId && 
                           g.GuestEmail.ToLower() == normalizedEmail && 
                           !g.IsDeleted);

            if (excludeGuestId.HasValue)
                query = query.Where(g => g.Id != excludeGuestId.Value);

            return !await query.AnyAsync();
        }

        /// <inheritdoc />
        public async Task<TourBookingGuest?> GetGuestWithBookingDetailsAsync(Guid guestId)
        {
            return await _context.TourBookingGuests
                .Include(g => g.TourBooking)
                    .ThenInclude(b => b.TourSlot)
                .Include(g => g.TourBooking)
                    .ThenInclude(b => b.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                .Include(g => g.TourBooking)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(g => g.Id == guestId && !g.IsDeleted);
        }

        /// <inheritdoc />
        public async Task<List<TourBookingGuest>> GetCheckedInGuestsByTourSlotAsync(Guid tourSlotId)
        {
            return await _context.TourBookingGuests
                .Include(g => g.TourBooking)
                .Where(g => g.TourBooking.TourSlotId == tourSlotId && 
                           g.IsCheckedIn && 
                           !g.IsDeleted && 
                           !g.TourBooking.IsDeleted)
                .OrderBy(g => g.CheckInTime)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> CountCheckedInGuestsByTourSlotAsync(Guid tourSlotId)
        {
            return await _context.TourBookingGuests
                .Include(g => g.TourBooking)
                .CountAsync(g => g.TourBooking.TourSlotId == tourSlotId && 
                               g.IsCheckedIn && 
                               !g.IsDeleted && 
                               !g.TourBooking.IsDeleted);
        }

        /// <inheritdoc />
        public async Task<int> BulkCheckInGuestsAsync(List<Guid> guestIds, DateTime checkInTime, string? notes = null)
        {
            if (!guestIds.Any())
                return 0;

            var guests = await _context.TourBookingGuests
                .Where(g => guestIds.Contains(g.Id) && !g.IsDeleted && !g.IsCheckedIn)
                .ToListAsync();

            foreach (var guest in guests)
            {
                guest.IsCheckedIn = true;
                guest.CheckInTime = checkInTime;
                guest.CheckInNotes = notes;
                guest.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return guests.Count;
        }
    }
}
