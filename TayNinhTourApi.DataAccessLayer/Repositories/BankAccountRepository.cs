using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho BankAccount entity
    /// Kế thừa từ GenericRepository và implement IBankAccountRepository
    /// </summary>
    public class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
    {
        public BankAccountRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách tài khoản ngân hàng của một user
        /// </summary>
        public async Task<IEnumerable<BankAccount>> GetByUserIdAsync(Guid userId, bool includeInactive = false)
        {
            var query = _context.BankAccounts
                .Where(b => b.UserId == userId);

            if (!includeInactive)
            {
                query = query.Where(b => b.IsActive && !b.IsDeleted);
            }

            return await query
                .OrderByDescending(b => b.IsDefault)
                .ThenBy(b => b.BankName)
                .ThenBy(b => b.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tài khoản ngân hàng mặc định của user
        /// </summary>
        public async Task<BankAccount?> GetDefaultByUserIdAsync(Guid userId)
        {
            return await _context.BankAccounts
                .FirstOrDefaultAsync(b => b.UserId == userId && 
                                         b.IsDefault && 
                                         b.IsActive && 
                                         !b.IsDeleted);
        }

        /// <summary>
        /// Kiểm tra user có tài khoản ngân hàng nào không
        /// </summary>
        public async Task<bool> HasBankAccountAsync(Guid userId)
        {
            return await _context.BankAccounts
                .AnyAsync(b => b.UserId == userId && b.IsActive && !b.IsDeleted);
        }

        /// <summary>
        /// Kiểm tra tài khoản ngân hàng đã tồn tại chưa (duplicate check)
        /// </summary>
        public async Task<bool> ExistsAsync(Guid userId, string bankName, string accountNumber, Guid? excludeId = null)
        {
            var query = _context.BankAccounts
                .Where(b => b.UserId == userId && 
                           b.BankName == bankName && 
                           b.AccountNumber == accountNumber &&
                           b.IsActive && 
                           !b.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(b => b.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Lấy tài khoản ngân hàng theo ID với kiểm tra ownership
        /// </summary>
        public async Task<BankAccount?> GetByIdAndUserIdAsync(Guid bankAccountId, Guid userId)
        {
            return await _context.BankAccounts
                .FirstOrDefaultAsync(b => b.Id == bankAccountId && 
                                         b.UserId == userId && 
                                         b.IsActive && 
                                         !b.IsDeleted);
        }

        /// <summary>
        /// Đếm số lượng tài khoản ngân hàng của user
        /// </summary>
        public async Task<int> CountByUserIdAsync(Guid userId, bool includeInactive = false)
        {
            var query = _context.BankAccounts
                .Where(b => b.UserId == userId);

            if (!includeInactive)
            {
                query = query.Where(b => b.IsActive && !b.IsDeleted);
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Unset tất cả tài khoản mặc định của user (để set tài khoản mới làm default)
        /// </summary>
        public async Task UnsetAllDefaultAsync(Guid userId)
        {
            var defaultAccounts = await _context.BankAccounts
                .Where(b => b.UserId == userId && b.IsDefault && b.IsActive && !b.IsDeleted)
                .ToListAsync();

            foreach (var account in defaultAccounts)
            {
                account.IsDefault = false;
            }

            if (defaultAccounts.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Kiểm tra tài khoản có thể xóa không (không có withdrawal request pending)
        /// </summary>
        public async Task<bool> CanDeleteAsync(Guid bankAccountId)
        {
            var hasPendingWithdrawals = await _context.WithdrawalRequests
                .AnyAsync(w => w.BankAccountId == bankAccountId && 
                              w.Status == WithdrawalStatus.Pending &&
                              w.IsActive && 
                              !w.IsDeleted);

            return !hasPendingWithdrawals;
        }

        /// <summary>
        /// Lấy tài khoản ngân hàng với thông tin withdrawal request count
        /// </summary>
        public async Task<BankAccount?> GetWithWithdrawalRequestsAsync(Guid bankAccountId)
        {
            return await _context.BankAccounts
                .Include(b => b.WithdrawalRequests)
                .FirstOrDefaultAsync(b => b.Id == bankAccountId && b.IsActive && !b.IsDeleted);
        }

        /// <summary>
        /// Lấy danh sách tài khoản ngân hàng với thông tin admin verification
        /// </summary>
        public async Task<IEnumerable<BankAccount>> GetWithVerificationInfoAsync(Guid userId)
        {
            return await _context.BankAccounts
                .Include(b => b.VerifiedBy)
                .Where(b => b.UserId == userId && b.IsActive && !b.IsDeleted)
                .OrderByDescending(b => b.IsDefault)
                .ThenBy(b => b.BankName)
                .ThenBy(b => b.CreatedAt)
                .ToListAsync();
        }
    }
}
