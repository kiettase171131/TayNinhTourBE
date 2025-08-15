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
    public class AdminSettingDiscountRepository : GenericRepository<AdminSettingDiscount>, IAdminSettingDiscountRepository

    {
        public AdminSettingDiscountRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }
        public async Task<decimal> GetTourDiscountPercentAsync()
        {
            var setting = await _context.AdminSettingDiscounts
                .FirstOrDefaultAsync(s => s.Key == "TourDiscountPercent");

            if (setting == null) return 0m;

            return decimal.TryParse(setting.Value, out var percent) ? percent : 0m;
        }

        public async Task UpdateTourDiscountPercentAsync(decimal newPercent)
        {
            var setting = await _context.AdminSettingDiscounts
                .FirstOrDefaultAsync(s => s.Key == "TourDiscountPercent");

            if (setting == null)
            {
                setting = new AdminSettingDiscount
                {
                    Id = Guid.NewGuid(),
                    Key = "TourDiscountPercent",
                    Value = newPercent.ToString("0.##"),
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.AdminSettingDiscounts.AddAsync(setting);
            }
            else
            {
                setting.Value = newPercent.ToString("0.##");
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
