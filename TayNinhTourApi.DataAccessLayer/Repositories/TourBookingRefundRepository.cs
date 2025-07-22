using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourBookingRefund entity
    /// Kế thừa từ GenericRepository và implement ITourBookingRefundRepository
    /// </summary>
    public class TourBookingRefundRepository : GenericRepository<TourBookingRefund>, ITourBookingRefundRepository
    {
        public TourBookingRefundRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền của một customer (user phổ thông)
        /// </summary>
        public async Task<(IEnumerable<TourBookingRefund> Items, int TotalCount)> GetByCustomerIdAsync(
            Guid userId,
            TourRefundStatus? status = null,
            TourRefundType? refundType = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.TourBookingRefunds
                .Include(r => r.TourBooking)
                    .ThenInclude(b => b.TourOperation)
                        .ThenInclude(o => o.Tour)
                .Where(r => r.UserId == userId && r.IsActive && !r.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (refundType.HasValue)
            {
                query = query.Where(r => r.RefundType == refundType.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền cho admin
        /// </summary>
        public async Task<(IEnumerable<TourBookingRefund> Items, int TotalCount)> GetForAdminAsync(
            TourRefundStatus? status = null,
            TourRefundType? refundType = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.TourBookingRefunds
                .Include(r => r.User)
                .Include(r => r.TourBooking)
                    .ThenInclude(b => b.TourOperation)
                        .ThenInclude(o => o.Tour)
                .Include(r => r.ProcessedBy)
                .Where(r => r.IsActive && !r.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (refundType.HasValue)
            {
                query = query.Where(r => r.RefundType == refundType.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.RequestedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(r => r.RequestedAt <= toDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(r =>
                    r.User.Name.ToLower().Contains(lowerSearchTerm) ||
                    r.User.Email.ToLower().Contains(lowerSearchTerm) ||
                    r.TourBooking.BookingCode.ToLower().Contains(lowerSearchTerm) ||
                    r.TourBooking.TourOperation.Tour.Name.ToLower().Contains(lowerSearchTerm) ||
                    (r.CustomerBankName != null && r.CustomerBankName.ToLower().Contains(lowerSearchTerm)) ||
                    (r.CustomerAccountHolder != null && r.CustomerAccountHolder.ToLower().Contains(lowerSearchTerm)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Lấy yêu cầu hoàn tiền theo ID với đầy đủ thông tin
        /// </summary>
        public async Task<TourBookingRefund?> GetWithDetailsAsync(Guid refundId)
        {
            return await _context.TourBookingRefunds
                .Include(r => r.User)
                .Include(r => r.TourBooking)
                    .ThenInclude(b => b.TourOperation)
                        .ThenInclude(o => o.Tour)
                            .ThenInclude(t => t.TourCompany)
                .Include(r => r.ProcessedBy)
                .FirstOrDefaultAsync(r => r.Id == refundId && r.IsActive && !r.IsDeleted);
        }

        /// <summary>
        /// Lấy yêu cầu hoàn tiền theo ID với kiểm tra ownership
        /// </summary>
        public async Task<TourBookingRefund?> GetByIdAndCustomerIdAsync(Guid refundId, Guid customerId)
        {
            return await _context.TourBookingRefunds
                .Include(r => r.TourBooking)
                    .ThenInclude(b => b.TourOperation)
                        .ThenInclude(o => o.Tour)
                .FirstOrDefaultAsync(r => r.Id == refundId && r.UserId == customerId && r.IsActive && !r.IsDeleted);
        }

        /// <summary>
        /// Lấy yêu cầu hoàn tiền theo TourBooking ID
        /// </summary>
        public async Task<TourBookingRefund?> GetByTourBookingIdAsync(Guid tourBookingId)
        {
            return await _context.TourBookingRefunds
                .Include(r => r.User)
                .Include(r => r.TourBooking)
                .FirstOrDefaultAsync(r => r.TourBookingId == tourBookingId && r.IsActive && !r.IsDeleted);
        }

        /// <summary>
        /// Kiểm tra tour booking đã có yêu cầu hoàn tiền chưa
        /// </summary>
        public async Task<bool> HasRefundRequestAsync(Guid tourBookingId)
        {
            return await _context.TourBookingRefunds
                .AnyAsync(r => r.TourBookingId == tourBookingId && r.IsActive && !r.IsDeleted);
        }

        /// <summary>
        /// Kiểm tra customer có yêu cầu hoàn tiền pending nào không
        /// </summary>
        public async Task<bool> HasPendingRefundAsync(Guid customerId)
        {
            return await _context.TourBookingRefunds
                .AnyAsync(r => r.UserId == customerId && 
                              r.Status == TourRefundStatus.Pending && 
                              r.IsActive && !r.IsDeleted);
        }

        /// <summary>
        /// Đếm số lượng yêu cầu hoàn tiền theo trạng thái
        /// </summary>
        public async Task<int> CountByStatusAsync(TourRefundStatus status, TourRefundType? refundType = null)
        {
            var query = _context.TourBookingRefunds
                .Where(r => r.Status == status && r.IsActive && !r.IsDeleted);

            if (refundType.HasValue)
            {
                query = query.Where(r => r.RefundType == refundType.Value);
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Lấy tổng số tiền đang chờ hoàn (status = Pending hoặc Approved)
        /// </summary>
        public async Task<decimal> GetTotalPendingRefundAmountAsync(TourRefundType? refundType = null)
        {
            var query = _context.TourBookingRefunds
                .Where(r => (r.Status == TourRefundStatus.Pending || r.Status == TourRefundStatus.Approved) && 
                           r.IsActive && !r.IsDeleted);

            if (refundType.HasValue)
            {
                query = query.Where(r => r.RefundType == refundType.Value);
            }

            return await query.SumAsync(r => r.RequestedAmount);
        }

        /// <summary>
        /// Lấy tổng số tiền đã hoàn trong khoảng thời gian
        /// </summary>
        public async Task<decimal> GetTotalRefundedAmountAsync(DateTime fromDate, DateTime toDate, TourRefundType? refundType = null)
        {
            var query = _context.TourBookingRefunds
                .Where(r => r.Status == TourRefundStatus.Completed && 
                           r.CompletedAt >= fromDate && 
                           r.CompletedAt <= toDate && 
                           r.IsActive && !r.IsDeleted);

            if (refundType.HasValue)
            {
                query = query.Where(r => r.RefundType == refundType.Value);
            }

            return await query.SumAsync(r => r.ApprovedAmount ?? 0);
        }

        /// <summary>
        /// Lấy yêu cầu hoàn tiền gần nhất của customer
        /// </summary>
        public async Task<TourBookingRefund?> GetLatestByCustomerIdAsync(Guid customerId)
        {
            return await _context.TourBookingRefunds
                .Include(r => r.TourBooking)
                    .ThenInclude(b => b.TourOperation)
                        .ThenInclude(o => o.Tour)
                .Where(r => r.UserId == customerId && r.IsActive && !r.IsDeleted)
                .OrderByDescending(r => r.RequestedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền theo tour company
        /// </summary>
        public async Task<(IEnumerable<TourBookingRefund> Items, int TotalCount)> GetByTourCompanyIdAsync(
            Guid tourCompanyId,
            TourRefundStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.TourBookingRefunds
                .Include(r => r.User)
                .Include(r => r.TourBooking)
                    .ThenInclude(b => b.TourOperation)
                        .ThenInclude(o => o.Tour)
                .Where(r => r.TourBooking.TourOperation.Tour.TourCompanyId == tourCompanyId && 
                           r.IsActive && !r.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Cập nhật trạng thái yêu cầu hoàn tiền
        /// </summary>
        public async Task<bool> UpdateStatusAsync(
            Guid refundId,
            TourRefundStatus status,
            Guid? processedById = null,
            decimal? approvedAmount = null,
            string? adminNotes = null,
            string? transactionReference = null)
        {
            var refundRequest = await _context.TourBookingRefunds
                .FirstOrDefaultAsync(r => r.Id == refundId && r.IsActive && !r.IsDeleted);

            if (refundRequest == null) return false;

            refundRequest.Status = status;
            refundRequest.ProcessedAt = DateTime.UtcNow;
            refundRequest.ProcessedById = processedById;
            refundRequest.UpdatedAt = DateTime.UtcNow;

            if (approvedAmount.HasValue)
            {
                refundRequest.ApprovedAmount = approvedAmount.Value;
            }

            if (!string.IsNullOrWhiteSpace(adminNotes))
            {
                refundRequest.AdminNotes = adminNotes;
            }

            if (!string.IsNullOrWhiteSpace(transactionReference))
            {
                refundRequest.TransactionReference = transactionReference;
            }

            if (status == TourRefundStatus.Completed)
            {
                refundRequest.CompletedAt = DateTime.UtcNow;
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy thống kê hoàn tiền theo tháng
        /// </summary>
        public async Task<(int TotalRequests, decimal TotalRequestedAmount, int CompletedRequests, decimal TotalRefundedAmount)> GetMonthlyStatsAsync(
            int year,
            int month,
            TourRefundType? refundType = null)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var query = _context.TourBookingRefunds
                .Where(r => r.RequestedAt >= startDate &&
                           r.RequestedAt <= endDate &&
                           r.IsActive && !r.IsDeleted);

            if (refundType.HasValue)
            {
                query = query.Where(r => r.RefundType == refundType.Value);
            }

            var totalRequests = await query.CountAsync();
            var totalRequestedAmount = await query.SumAsync(r => r.RequestedAmount);

            var completedQuery = query.Where(r => r.Status == TourRefundStatus.Completed);
            var completedRequests = await completedQuery.CountAsync();
            var totalRefundedAmount = await completedQuery.SumAsync(r => r.ApprovedAmount ?? 0);

            return (totalRequests, totalRequestedAmount, completedRequests, totalRefundedAmount);
        }

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền cần xử lý tự động (auto-cancellation)
        /// </summary>
        public async Task<IEnumerable<TourBooking>> GetBookingsForAutoCancellationAsync(IEnumerable<Guid> tourOperationIds)
        {
            return await _context.TourBookings
                .Include(b => b.User)
                .Include(b => b.TourOperation)
                    .ThenInclude(o => o.Tour)
                .Where(b => tourOperationIds.Contains(b.TourOperationId) &&
                           b.Status == BookingStatus.Confirmed &&
                           !_context.TourBookingRefunds.Any(r => r.TourBookingId == b.Id && r.IsActive && !r.IsDeleted))
                .ToListAsync();
        }

        /// <summary>
        /// Tạo bulk refund requests cho auto-cancellation
        /// </summary>
        public async Task<bool> CreateBulkRefundRequestsAsync(IEnumerable<TourBookingRefund> refundRequests)
        {
            try
            {
                await _context.TourBookingRefunds.AddRangeAsync(refundRequests);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền theo admin processor
        /// </summary>
        public async Task<IEnumerable<TourBookingRefund>> GetProcessedByAdminAsync(Guid adminId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.TourBookingRefunds
                .Include(r => r.TourBooking)
                    .ThenInclude(b => b.TourOperation)
                        .ThenInclude(o => o.Tour)
                .Where(r => r.ProcessedById == adminId && r.IsActive && !r.IsDeleted);

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.ProcessedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(r => r.ProcessedAt <= toDate.Value);
            }

            return await query
                .OrderByDescending(r => r.ProcessedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra tour booking có đủ điều kiện để hoàn tiền không
        /// </summary>
        public async Task<bool> IsEligibleForRefundAsync(Guid tourBookingId)
        {
            var booking = await _context.TourBookings
                .Include(b => b.TourOperation)
                .FirstOrDefaultAsync(b => b.Id == tourBookingId);

            if (booking == null) return false;

            // Kiểm tra booking status phải là Confirmed
            if (booking.Status != BookingStatus.Confirmed) return false;

            // Kiểm tra chưa có refund request
            var hasRefundRequest = await HasRefundRequestAsync(tourBookingId);
            if (hasRefundRequest) return false;

            // Kiểm tra tour chưa bắt đầu
            if (booking.TourOperation.StartDate <= DateTime.UtcNow) return false;

            return true;
        }

        /// <summary>
        /// Lấy average processing time cho refund requests
        /// </summary>
        public async Task<double> GetAverageProcessingTimeAsync(TourRefundType? refundType = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.TourBookingRefunds
                .Where(r => r.ProcessedAt.HasValue && r.IsActive && !r.IsDeleted);

            if (refundType.HasValue)
            {
                query = query.Where(r => r.RefundType == refundType.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.RequestedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(r => r.RequestedAt <= toDate.Value);
            }

            var processedRequests = await query
                .Select(r => new { r.RequestedAt, r.ProcessedAt })
                .ToListAsync();

            if (!processedRequests.Any()) return 0;

            var totalHours = processedRequests
                .Sum(r => (r.ProcessedAt!.Value - r.RequestedAt).TotalHours);

            return totalHours / processedRequests.Count;
        }
    }
}
