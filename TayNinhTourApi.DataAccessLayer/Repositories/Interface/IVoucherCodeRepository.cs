using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IVoucherCodeRepository : IGenericRepository<VoucherCode>
    {
        Task<VoucherCode?> GetByCodeAsync(string code);
        Task<List<VoucherCode>> GetByVoucherIdAsync(Guid voucherId);
        Task<bool> IsCodeExistsAsync(string code);
        Task<List<VoucherCode>> GetAvailableToClaimAsync(int pageIndex, int pageSize);
        Task<List<VoucherCode>> GetClaimedByUserAsync(Guid userId, int pageIndex, int pageSize);
        Task<VoucherCode?> GetAvailableCodeByIdAsync(Guid voucherCodeId);
        Task<VoucherCode?> GetUserVoucherCodeAsync(Guid voucherCodeId, Guid userId);
    }
}