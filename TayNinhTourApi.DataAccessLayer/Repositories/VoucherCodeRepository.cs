using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class VoucherCodeRepository : GenericRepository<VoucherCode>, IVoucherCodeRepository
    {
        public VoucherCodeRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<VoucherCode?> GetByCodeAsync(string code)
        {
            return await _context.VoucherCodes
                .Include(vc => vc.Voucher)
                .Include(vc => vc.ClaimedByUser)
                .Include(vc => vc.UsedByUser)
                .FirstOrDefaultAsync(vc => vc.Code == code && !vc.IsDeleted);
        }

        public async Task<List<VoucherCode>> GetByVoucherIdAsync(Guid voucherId)
        {
            return await _context.VoucherCodes
                .Include(vc => vc.ClaimedByUser)
                .Include(vc => vc.UsedByUser)
                .Where(vc => vc.VoucherId == voucherId && !vc.IsDeleted)
                .OrderBy(vc => vc.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsCodeExistsAsync(string code)
        {
            return await _context.VoucherCodes
                .AnyAsync(vc => vc.Code == code && !vc.IsDeleted);
        }

        public async Task<List<VoucherCode>> GetAvailableToClaimAsync(int pageIndex, int pageSize)
        {
            var now = DateTime.UtcNow;
            
            return await _context.VoucherCodes
                .Include(vc => vc.Voucher)
                .Where(vc => !vc.IsDeleted &&
                            !vc.IsClaimed &&
                            vc.Voucher.IsActive &&
                            vc.Voucher.StartDate <= now &&
                            vc.Voucher.EndDate >= now)
                .OrderBy(vc => vc.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<VoucherCode>> GetClaimedByUserAsync(Guid userId, int pageIndex, int pageSize)
        {
            return await _context.VoucherCodes
                .Include(vc => vc.Voucher)
                .Where(vc => !vc.IsDeleted &&
                            vc.IsClaimed &&
                            vc.ClaimedByUserId == userId)
                .OrderByDescending(vc => vc.ClaimedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<VoucherCode?> GetAvailableCodeByIdAsync(Guid voucherCodeId)
        {
            var now = DateTime.UtcNow;
            
            return await _context.VoucherCodes
                .Include(vc => vc.Voucher)
                .FirstOrDefaultAsync(vc => vc.Id == voucherCodeId &&
                                          !vc.IsDeleted &&
                                          !vc.IsClaimed &&
                                          vc.Voucher.IsActive &&
                                          vc.Voucher.StartDate <= now &&
                                          vc.Voucher.EndDate >= now);
        }

        public async Task<VoucherCode?> GetUserVoucherCodeAsync(Guid voucherCodeId, Guid userId)
        {
            return await _context.VoucherCodes
                .Include(vc => vc.Voucher)
                .FirstOrDefaultAsync(vc => vc.Id == voucherCodeId &&
                                          !vc.IsDeleted &&
                                          vc.IsClaimed &&
                                          vc.ClaimedByUserId == userId);
        }
    }
}