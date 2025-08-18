using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourBooking entity
    /// </summary>
    public class TourBookingRepository : GenericRepository<TourBooking>, ITourBookingRepository
    {
        public TourBookingRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách bookings theo TourOperation ID
        /// </summary>
        public async Task<List<TourBooking>> GetBookingsByOperationIdAsync(Guid tourOperationId, bool includeInactive = false)
        {
            var query = _context.TourBookings
                .Include(tb => tb.User)
                .Include(tb => tb.TourOperation)
                .Where(tb => tb.TourOperationId == tourOperationId);

            if (!includeInactive)
            {
                query = query.Where(tb => tb.Status != BookingStatus.CancelledByCustomer 
                                       && tb.Status != BookingStatus.CancelledByCompany);
            }

            return await query
                .OrderByDescending(tb => tb.BookingDate)
                .ToListAsync();
        }

        /// <summary>
        /// Đếm số lượng bookings active cho một TourOperation
        /// </summary>
        public async Task<int> GetActiveBookingsCountAsync(Guid tourOperationId)
        {
            return await _context.TourBookings
                .Where(tb => tb.TourOperationId == tourOperationId
                          && tb.Status != BookingStatus.CancelledByCustomer
                          && tb.Status != BookingStatus.CancelledByCompany)
                .CountAsync();
        }

        /// <summary>
        /// Tính tổng số guests đã booking cho một TourOperation
        /// </summary>
        public async Task<int> GetTotalBookedGuestsAsync(Guid tourOperationId)
        {
            return await _context.TourBookings
                .Where(tb => tb.TourOperationId == tourOperationId
                          && tb.Status != BookingStatus.CancelledByCustomer
                          && tb.Status != BookingStatus.CancelledByCompany)
                .SumAsync(tb => tb.NumberOfGuests);
        }

        /// <summary>
        /// Kiểm tra capacity còn lại cho một TourOperation
        /// </summary>
        public async Task<int> GetAvailableCapacityAsync(Guid tourOperationId, int maxCapacity)
        {
            var bookedGuests = await GetTotalBookedGuestsAsync(tourOperationId);
            return Math.Max(0, maxCapacity - bookedGuests);
        }

        /// <summary>
        /// Kiểm tra xem có thể booking thêm số guests này không
        /// </summary>
        public async Task<bool> CanBookAsync(Guid tourOperationId, int requestedGuests, int maxCapacity)
        {
            var availableCapacity = await GetAvailableCapacityAsync(tourOperationId, maxCapacity);
            return availableCapacity >= requestedGuests;
        }

        /// <summary>
        /// Kiểm tra và reserve capacity với optimistic concurrency control
        /// CHỈ reserve TourSlot capacity, KHÔNG cộng TourOperation.CurrentBookings
        /// TourOperation.CurrentBookings chỉ được cộng khi thanh toán thành công
        /// </summary>
        public async Task<bool> TryReserveCapacityAsync(Guid tourOperationId, int requestedGuests)
        {
            try
            {
                // Lấy TourOperation để kiểm tra capacity
                var tourOperation = await _context.TourOperations
                    .FirstOrDefaultAsync(to => to.Id == tourOperationId);

                if (tourOperation == null)
                    return false;

                // Kiểm tra capacity available (chỉ check, không update)
                var availableCapacity = tourOperation.MaxGuests - tourOperation.CurrentBookings;
                if (availableCapacity < requestedGuests)
                    return false;

                // ❌ KHÔNG cộng TourOperation.CurrentBookings ở đây nữa
                // Chỉ cộng khi thanh toán thành công trong HandlePaymentSuccessAsync
                // tourOperation.CurrentBookings += requestedGuests; // REMOVED
                // tourOperation.UpdatedAt = DateTime.UtcNow; // REMOVED
                // await _context.SaveChangesAsync(); // REMOVED

                // ✅ Capacity check passed, let TourSlotService handle slot reservation
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Có conflict, capacity đã bị thay đổi bởi request khác
                return false;
            }
        }

        /// <summary>
        /// Release capacity khi hủy booking
        /// </summary>
        public async Task<bool> ReleaseCapacityAsync(Guid tourOperationId, int guestsToRelease)
        {
            try
            {
                var tourOperation = await _context.TourOperations
                    .FirstOrDefaultAsync(to => to.Id == tourOperationId);

                if (tourOperation == null)
                    return false;

                // Update CurrentBookings
                tourOperation.CurrentBookings = Math.Max(0, tourOperation.CurrentBookings - guestsToRelease);
                tourOperation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách bookings của một user
        /// </summary>
        public async Task<List<TourBooking>> GetBookingsByUserIdAsync(Guid userId, bool includeInactive = false)
        {
            var query = _context.TourBookings
                .Include(tb => tb.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                .Where(tb => tb.UserId == userId);

            if (!includeInactive)
            {
                query = query.Where(tb => tb.Status != BookingStatus.CancelledByCustomer 
                                       && tb.Status != BookingStatus.CancelledByCompany);
            }

            return await query
                .OrderByDescending(tb => tb.BookingDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy booking theo booking code
        /// </summary>
        public async Task<TourBooking?> GetBookingByCodeAsync(string bookingCode)
        {
            return await _context.TourBookings
                .Include(tb => tb.User)
                .Include(tb => tb.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                .FirstOrDefaultAsync(tb => tb.BookingCode == bookingCode);
        }

        /// <summary>
        /// Kiểm tra booking code đã tồn tại chưa
        /// </summary>
        public async Task<bool> IsBookingCodeExistsAsync(string bookingCode)
        {
            return await _context.TourBookings
                .AnyAsync(tb => tb.BookingCode == bookingCode);
        }

        /// <summary>
        /// Cập nhật trạng thái booking
        /// </summary>
        public async Task<bool> UpdateBookingStatusAsync(Guid bookingId, BookingStatus newStatus, string? reason = null)
        {
            var booking = await _context.TourBookings.FindAsync(bookingId);
            if (booking == null) return false;

            booking.Status = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;

            if (newStatus == BookingStatus.Confirmed)
            {
                booking.ConfirmedDate = DateTime.UtcNow;
            }
            else if (newStatus == BookingStatus.CancelledByCustomer || newStatus == BookingStatus.CancelledByCompany)
            {
                booking.CancelledDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(reason))
                {
                    booking.CancellationReason = reason;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Lấy thống kê bookings theo TourOperation
        /// </summary>
        public async Task<Dictionary<BookingStatus, int>> GetBookingStatisticsAsync(Guid tourOperationId)
        {
            return await _context.TourBookings
                .Where(tb => tb.TourOperationId == tourOperationId)
                .GroupBy(tb => tb.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Lấy danh sách bookings với pagination
        /// </summary>
        public async Task<List<TourBooking>> GetBookingsWithPaginationAsync(
            int pageIndex, 
            int pageSize, 
            Guid? tourOperationId = null, 
            Guid? userId = null, 
            BookingStatus? status = null)
        {
            var query = _context.TourBookings
                .Include(tb => tb.User)
                .Include(tb => tb.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.TourTemplate)
                .AsQueryable();

            if (tourOperationId.HasValue)
                query = query.Where(tb => tb.TourOperationId == tourOperationId.Value);

            if (userId.HasValue)
                query = query.Where(tb => tb.UserId == userId.Value);

            if (status.HasValue)
                query = query.Where(tb => tb.Status == status.Value);

            return await query
                .OrderByDescending(tb => tb.BookingDate)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Đếm tổng số bookings theo filter
        /// </summary>
        public async Task<int> CountBookingsAsync(
            Guid? tourOperationId = null, 
            Guid? userId = null, 
            BookingStatus? status = null)
        {
            var query = _context.TourBookings.AsQueryable();

            if (tourOperationId.HasValue)
                query = query.Where(tb => tb.TourOperationId == tourOperationId.Value);

            if (userId.HasValue)
                query = query.Where(tb => tb.UserId == userId.Value);

            if (status.HasValue)
                query = query.Where(tb => tb.Status == status.Value);

            return await query.CountAsync();
        }

        /// <summary>
        /// Lấy tour booking theo PayOS order code
        /// </summary>
        public async Task<TourBooking?> GetByPayOsOrderCodeAsync(string payOsOrderCode)
        {
            return await _context.TourBookings
                .Include(tb => tb.User)
                .Include(tb => tb.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.CreatedBy)
                            .ThenInclude(u => u.TourCompany)
                .FirstOrDefaultAsync(tb => tb.PayOsOrderCode == payOsOrderCode && !tb.IsDeleted);
        }

        /// <summary>
        /// Lấy danh sách bookings của một user với filter nâng cao
        /// </summary>
        public async Task<(List<TourBooking> bookings, int totalCount)> GetUserBookingsWithFilterAsync(
            Guid userId,
            int pageIndex = 0,
            int pageSize = 10,
            BookingStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null)
        {
            var query = _context.TourBookings
                .Include(tb => tb.Guests.Where(g => !g.IsDeleted))
                .Include(tb => tb.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.CreatedBy)  // User who created the tour (tour company)
                            .ThenInclude(u => u.TourCompany)  // Tour company info
                .Include(tb => tb.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                .Include(tb => tb.TourOperation)
                    .ThenInclude(to => to.TourGuide)
                .Include(tb => tb.User)
                .Where(tb => tb.UserId == userId && !tb.IsDeleted);

            // Filter by status
            if (status.HasValue)
            {
                // Handle the status mapping
                switch (status.Value)
                {
                    case BookingStatus.Confirmed:
                        query = query.Where(tb => tb.Status == BookingStatus.Confirmed);
                        break;
                    case BookingStatus.Pending:
                        query = query.Where(tb => tb.Status == BookingStatus.Pending);
                        break;
                    case BookingStatus.CancelledByCustomer:
                    case BookingStatus.CancelledByCompany:
                        // Map "cancel" to both cancelled statuses
                        query = query.Where(tb => tb.Status == BookingStatus.CancelledByCustomer || tb.Status == BookingStatus.CancelledByCompany);
                        break;
                    default:
                        query = query.Where(tb => tb.Status == status.Value);
                        break;
                }
            }

            // Filter by date range (booking date)
            if (startDate.HasValue)
            {
                query = query.Where(tb => tb.BookingDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                // Include the entire end date
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(tb => tb.BookingDate <= endOfDay);
            }

            // Filter by search term (tour company name)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(tb => 
                    // Search in tour company name
                    (tb.TourOperation.TourDetails.CreatedBy.TourCompany != null && 
                     tb.TourOperation.TourDetails.CreatedBy.TourCompany.CompanyName.ToLower().Contains(searchLower)) ||
                    // Also search in tour title as fallback
                    tb.TourOperation.TourDetails.Title.ToLower().Contains(searchLower) ||
                    // Search in user/company name who created the tour
                    tb.TourOperation.TourDetails.CreatedBy.Name.ToLower().Contains(searchLower)
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var bookings = await query
                .OrderByDescending(tb => tb.BookingDate)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (bookings, totalCount);
        }
    }
}
