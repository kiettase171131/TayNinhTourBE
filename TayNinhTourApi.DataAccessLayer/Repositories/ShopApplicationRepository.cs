using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class ShopApplicationRepository : GenericRepository<ShopApplication>, IShopApplicationRepository
    {
        public ShopApplicationRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<ShopApplication>> ListByStatusAsync(ShopStatus status)
        {
            return await _context.Set<ShopApplication>()
                         .Include(a => a.User)
                         .Where(a => a.Status == status)
                         .ToListAsync();
        }
        public async Task<IEnumerable<ShopApplication>> ListByUserAsync(Guid userId)
        {
            return await _context.Set<ShopApplication>()
                         .Include(a => a.User)
                         .Where(a => a.UserId == userId)
                         .OrderByDescending(a => a.CreatedAt)
                         .ToListAsync();
        }
    }
}
