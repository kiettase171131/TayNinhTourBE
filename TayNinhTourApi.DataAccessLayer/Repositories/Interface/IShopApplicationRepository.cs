using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IShopApplicationRepository : IGenericRepository<ShopApplication>
    {
        Task<IEnumerable<ShopApplication>> ListByStatusAsync(ShopStatus status);
        Task<IEnumerable<ShopApplication>> ListByUserAsync(Guid userId);
    }
}
