using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IAdminSettingDiscountRepository : IGenericRepository<AdminSettingDiscount>
    {
        Task<decimal> GetTourDiscountPercentAsync();
        Task UpdateTourDiscountPercentAsync(decimal newPercent);
    }
}
