using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho TourCompany entity
    /// </summary>
    public class TourCompanyRepository : GenericRepository<TourCompany>, ITourCompanyRepository
    {
        public TourCompanyRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy TourCompany theo UserId
        /// </summary>
        public async Task<TourCompany?> GetByUserIdAsync(Guid userId)
        {
            return await _context.TourCompanies
                .FirstOrDefaultAsync(tc => tc.UserId == userId && !tc.IsDeleted);
        }

        /// <summary>
        /// Lấy TourCompany theo UserId với includes
        /// </summary>
        public async Task<TourCompany?> GetByUserIdAsync(Guid userId, params string[] includes)
        {
            var query = _context.TourCompanies.AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(tc => tc.UserId == userId && !tc.IsDeleted);
        }

        /// <summary>
        /// Kiểm tra xem User có phải là TourCompany không
        /// </summary>
        public async Task<bool> IsUserTourCompanyAsync(Guid userId)
        {
            return await _context.TourCompanies
                .AnyAsync(tc => tc.UserId == userId && !tc.IsDeleted && tc.IsActive);
        }

        /// <summary>
        /// Lấy danh sách TourCompany đang hoạt động
        /// </summary>
        public async Task<(List<TourCompany> companies, int totalCount)> GetActiveTourCompaniesAsync(int pageIndex, int pageSize)
        {
            var query = _context.TourCompanies
                .Where(tc => !tc.IsDeleted && tc.IsActive)
                .Include(tc => tc.User);

            var totalCount = await query.CountAsync();

            var companies = await query
                .OrderByDescending(tc => tc.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (companies, totalCount);
        }

        /// <summary>
        /// Cập nhật wallet và revenue hold với optimistic concurrency
        /// </summary>
        public async Task<bool> UpdateFinancialAsync(Guid tourCompanyId, decimal walletChange, decimal revenueHoldChange)
        {
            try
            {
                var tourCompany = await _context.TourCompanies
                    .FirstOrDefaultAsync(tc => tc.Id == tourCompanyId && !tc.IsDeleted);

                if (tourCompany == null)
                    return false;

                // Kiểm tra số dư không âm
                var newWallet = tourCompany.Wallet + walletChange;
                var newRevenueHold = tourCompany.RevenueHold + revenueHoldChange;

                if (newWallet < 0 || newRevenueHold < 0)
                    return false;

                tourCompany.Wallet = newWallet;
                tourCompany.RevenueHold = newRevenueHold;
                tourCompany.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Optimistic concurrency conflict
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
