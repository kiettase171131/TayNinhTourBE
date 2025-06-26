using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho SpecialtyShop entity
    /// Kế thừa từ GenericRepository và implement ISpecialtyShopRepository
    /// </summary>
    public class SpecialtyShopRepository : GenericRepository<SpecialtyShop>, ISpecialtyShopRepository
    {
        public SpecialtyShopRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy SpecialtyShop theo UserId với User information
        /// </summary>
        public async Task<SpecialtyShop?> GetByUserIdAsync(Guid userId)
        {
            return await _context.SpecialtyShops
                .Include(s => s.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
        }

        /// <summary>
        /// Lấy danh sách tất cả SpecialtyShops đang hoạt động
        /// </summary>
        public async Task<IEnumerable<SpecialtyShop>> GetActiveShopsAsync()
        {
            return await _context.SpecialtyShops
                .Where(s => s.IsShopActive && s.IsActive)
                .Include(s => s.User)
                .OrderBy(s => s.ShopName)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách SpecialtyShops theo loại shop
        /// </summary>
        public async Task<IEnumerable<SpecialtyShop>> GetShopsByTypeAsync(string shopType)
        {
            return await _context.SpecialtyShops
                .Where(s => s.ShopType == shopType && s.IsShopActive && s.IsActive)
                .Include(s => s.User)
                .OrderBy(s => s.ShopName)
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra xem User đã có SpecialtyShop chưa
        /// </summary>
        public async Task<bool> ExistsByUserIdAsync(Guid userId)
        {
            return await _context.SpecialtyShops
                .AnyAsync(s => s.UserId == userId && s.IsActive);
        }

        /// <summary>
        /// Lấy danh sách SpecialtyShops với phân trang
        /// </summary>
        public async Task<(IEnumerable<SpecialtyShop> Items, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, bool isActiveOnly = true)
        {
            var query = _context.SpecialtyShops.AsQueryable();

            if (isActiveOnly)
            {
                query = query.Where(s => s.IsShopActive && s.IsActive);
            }
            else
            {
                query = query.Where(s => s.IsActive);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(s => s.User)
                .OrderBy(s => s.ShopName)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Tìm kiếm SpecialtyShops theo tên hoặc địa điểm
        /// </summary>
        public async Task<IEnumerable<SpecialtyShop>> SearchAsync(string searchTerm, bool isActiveOnly = true)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetActiveShopsAsync();
            }

            var query = _context.SpecialtyShops.AsQueryable();

            if (isActiveOnly)
            {
                query = query.Where(s => s.IsShopActive && s.IsActive);
            }
            else
            {
                query = query.Where(s => s.IsActive);
            }

            return await query
                .Where(s => s.ShopName.Contains(searchTerm) || 
                           s.Location.Contains(searchTerm) ||
                           (s.Description != null && s.Description.Contains(searchTerm)) ||
                           (s.ShopType != null && s.ShopType.Contains(searchTerm)))
                .Include(s => s.User)
                .OrderBy(s => s.ShopName)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy SpecialtyShops theo rating tối thiểu
        /// </summary>
        public async Task<IEnumerable<SpecialtyShop>> GetShopsByMinRatingAsync(decimal minRating, bool isActiveOnly = true)
        {
            var query = _context.SpecialtyShops.AsQueryable();

            if (isActiveOnly)
            {
                query = query.Where(s => s.IsShopActive && s.IsActive);
            }
            else
            {
                query = query.Where(s => s.IsActive);
            }

            return await query
                .Where(s => s.Rating.HasValue && s.Rating.Value >= minRating)
                .Include(s => s.User)
                .OrderByDescending(s => s.Rating)
                .ThenBy(s => s.ShopName)
                .ToListAsync();
        }
    }
}
