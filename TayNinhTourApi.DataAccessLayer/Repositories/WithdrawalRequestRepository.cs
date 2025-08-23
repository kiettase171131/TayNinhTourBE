using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho WithdrawalRequest entity
    /// Kế thừa từ GenericRepository và implement IWithdrawalRequestRepository
    /// </summary>
    public class WithdrawalRequestRepository : GenericRepository<WithdrawalRequest>, IWithdrawalRequestRepository
    {
        public WithdrawalRequestRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền của một user
        /// </summary>
        public async Task<(IEnumerable<WithdrawalRequest> Items, int TotalCount)> GetByUserIdAsync(
            Guid userId, 
            WithdrawalStatus? status = null, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            var query = _context.WithdrawalRequests
                .Include(w => w.BankAccount)
                .Where(w => w.UserId == userId && w.IsActive && !w.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(w => w.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(w => w.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền cho admin
        /// </summary>
        public async Task<(IEnumerable<WithdrawalRequest> Items, int TotalCount)> GetForAdminAsync(
            WithdrawalStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null)
        {
            var query = _context.WithdrawalRequests
                .Include(w => w.User)
                .Include(w => w.BankAccount)
                .Include(w => w.ProcessedBy)
                .Where(w => w.IsActive && !w.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(w => w.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(w =>
                    w.User.Name.ToLower().Contains(lowerSearchTerm) ||
                    w.User.Email.ToLower().Contains(lowerSearchTerm) ||
                    w.BankAccount.BankName.ToLower().Contains(lowerSearchTerm) ||
                    w.BankAccount.AccountNumber.Contains(searchTerm) ||
                    w.BankAccount.AccountHolderName.ToLower().Contains(lowerSearchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(w => w.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Lấy yêu cầu rút tiền theo ID với đầy đủ thông tin
        /// </summary>
        public async Task<WithdrawalRequest?> GetWithDetailsAsync(Guid withdrawalRequestId)
        {
            return await _context.WithdrawalRequests
                .Include(w => w.User)
                .Include(w => w.BankAccount)
                .Include(w => w.ProcessedBy)
                .FirstOrDefaultAsync(w => w.Id == withdrawalRequestId && w.IsActive && !w.IsDeleted);
        }

        /// <summary>
        /// Lấy yêu cầu rút tiền theo ID với kiểm tra ownership
        /// </summary>
        public async Task<WithdrawalRequest?> GetByIdAndUserIdAsync(Guid withdrawalRequestId, Guid userId)
        {
            return await _context.WithdrawalRequests
                .Include(w => w.BankAccount)
                .FirstOrDefaultAsync(w => w.Id == withdrawalRequestId && 
                                         w.UserId == userId && 
                                         w.IsActive && 
                                         !w.IsDeleted);
        }

        /// <summary>
        /// Đếm số lượng yêu cầu rút tiền theo trạng thái
        /// </summary>
        public async Task<int> CountByStatusAsync(WithdrawalStatus status)
        {
            return await _context.WithdrawalRequests
                .CountAsync(w => w.Status == status && w.IsActive && !w.IsDeleted);
        }

        /// <summary>
        /// Lấy tổng số tiền đang chờ rút (status = Pending)
        /// </summary>
        public async Task<decimal> GetTotalPendingAmountAsync()
        {
            return await _context.WithdrawalRequests
                .Where(w => w.Status == WithdrawalStatus.Pending && w.IsActive && !w.IsDeleted)
                .SumAsync(w => w.Amount);
        }

        /// <summary>
        /// Lấy tổng số tiền đã rút trong khoảng thời gian
        /// </summary>
        public async Task<decimal> GetTotalWithdrawnAmountAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.WithdrawalRequests
                .Where(w => w.Status == WithdrawalStatus.Approved && 
                           w.ProcessedAt >= fromDate && 
                           w.ProcessedAt <= toDate &&
                           w.IsActive && 
                           !w.IsDeleted)
                .SumAsync(w => w.Amount);
        }

        /// <summary>
        /// Kiểm tra user có yêu cầu rút tiền pending nào không
        /// </summary>
        public async Task<bool> HasPendingRequestAsync(Guid userId)
        {
            return await _context.WithdrawalRequests
                .AnyAsync(w => w.UserId == userId && 
                              w.Status == WithdrawalStatus.Pending &&
                              w.IsActive && 
                              !w.IsDeleted);
        }

        /// <summary>
        /// Lấy yêu cầu rút tiền gần nhất của user
        /// </summary>
        public async Task<WithdrawalRequest?> GetLatestByUserIdAsync(Guid userId)
        {
            return await _context.WithdrawalRequests
                .Include(w => w.BankAccount)
                .Where(w => w.UserId == userId && w.IsActive && !w.IsDeleted)
                .OrderByDescending(w => w.RequestedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền theo bank account
        /// </summary>
        public async Task<IEnumerable<WithdrawalRequest>> GetByBankAccountIdAsync(Guid bankAccountId)
        {
            return await _context.WithdrawalRequests
                .Where(w => w.BankAccountId == bankAccountId && w.IsActive && !w.IsDeleted)
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Cập nhật trạng thái yêu cầu rút tiền
        /// </summary>
        public async Task<bool> UpdateStatusAsync(
            Guid withdrawalRequestId,
            WithdrawalStatus status,
            Guid? processedById = null,
            string? adminNotes = null,
            string? transactionReference = null)
        {
            var withdrawalRequest = await _context.WithdrawalRequests
                .FirstOrDefaultAsync(w => w.Id == withdrawalRequestId && w.IsActive && !w.IsDeleted);

            if (withdrawalRequest == null)
                return false;

            withdrawalRequest.Status = status;
            withdrawalRequest.ProcessedAt = DateTime.UtcNow;
            withdrawalRequest.ProcessedById = processedById;
            withdrawalRequest.AdminNotes = adminNotes;
            withdrawalRequest.TransactionReference = transactionReference;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Lấy thống kê rút tiền theo tháng
        /// </summary>
        public async Task<(int TotalRequests, decimal TotalAmount, int ApprovedRequests, decimal ApprovedAmount)> GetMonthlyStatsAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var monthlyRequests = await _context.WithdrawalRequests
                .Where(w => w.RequestedAt >= startDate && 
                           w.RequestedAt <= endDate &&
                           w.IsActive && 
                           !w.IsDeleted)
                .ToListAsync();

            var totalRequests = monthlyRequests.Count;
            var totalAmount = monthlyRequests.Sum(w => w.Amount);
            var approvedRequests = monthlyRequests.Count(w => w.Status == WithdrawalStatus.Approved);
            var approvedAmount = monthlyRequests.Where(w => w.Status == WithdrawalStatus.Approved).Sum(w => w.Amount);

            return (totalRequests, totalAmount, approvedRequests, approvedAmount);
        }

        /// <summary>
        /// Lấy thống kê yêu cầu rút tiền theo role (TourCompany hoặc SpecialtyShop)
        /// </summary>
        public async Task<(int TotalRequests, int PendingRequests, int ApprovedRequests, int RejectedRequests, 
                          decimal TotalAmount, decimal PendingAmount, decimal ApprovedAmount, decimal RejectedAmount)> 
                          GetStatsByRoleAsync(string roleName, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.WithdrawalRequests
                .Include(w => w.User)
                .ThenInclude(u => u.Role)
                .Where(w => w.IsActive && !w.IsDeleted && w.User.Role.Name == roleName);

            // Filter by date range if provided
            if (startDate.HasValue)
            {
                query = query.Where(w => w.RequestedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(w => w.RequestedAt <= endDate.Value);
            }

            var requests = await query.ToListAsync();

            var totalRequests = requests.Count;
            var pendingRequests = requests.Count(w => w.Status == WithdrawalStatus.Pending);
            var approvedRequests = requests.Count(w => w.Status == WithdrawalStatus.Approved);
            var rejectedRequests = requests.Count(w => w.Status == WithdrawalStatus.Rejected);

            var totalAmount = requests.Sum(w => w.Amount);
            var pendingAmount = requests.Where(w => w.Status == WithdrawalStatus.Pending).Sum(w => w.Amount);
            var approvedAmount = requests.Where(w => w.Status == WithdrawalStatus.Approved).Sum(w => w.Amount);
            var rejectedAmount = requests.Where(w => w.Status == WithdrawalStatus.Rejected).Sum(w => w.Amount);

            return (totalRequests, pendingRequests, approvedRequests, rejectedRequests, 
                   totalAmount, pendingAmount, approvedAmount, rejectedAmount);
        }
    }
}
