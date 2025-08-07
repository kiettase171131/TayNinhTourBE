using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    // DEPRECATED: VoucherCode system has been simplified
    // This class is kept temporarily to avoid breaking existing references
    // TODO: Remove this class and all references after system migration is complete
    public class VoucherCodeRepository : GenericRepository<VoucherCode>, IVoucherCodeRepository
    {
        public VoucherCodeRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        // All methods now return empty or throw NotImplementedException since VoucherCode is deprecated
        public async Task<VoucherCode?> GetByCodeAsync(string code)
        {
            throw new NotImplementedException("VoucherCode system has been deprecated. Use Voucher directly instead.");
        }

        public async Task<List<VoucherCode>> GetByVoucherIdAsync(Guid voucherId)
        {
            return new List<VoucherCode>(); // Return empty list
        }

        public async Task<bool> IsCodeExistsAsync(string code)
        {
            return false; // No voucher codes exist in new system
        }

        public async Task<List<VoucherCode>> GetAvailableToClaimAsync(int pageIndex, int pageSize)
        {
            return new List<VoucherCode>(); // Return empty list
        }

        public async Task<List<VoucherCode>> GetClaimedByUserAsync(Guid userId, int pageIndex, int pageSize)
        {
            return new List<VoucherCode>(); // Return empty list
        }

        public async Task<VoucherCode?> GetAvailableCodeByIdAsync(Guid voucherCodeId)
        {
            return null; // No voucher codes available in new system
        }

        public async Task<VoucherCode?> GetUserVoucherCodeAsync(Guid voucherCodeId, Guid userId)
        {
            return null; // No user voucher codes in new system
        }
    }
}