using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho Shop entity
    /// Kế thừa từ GenericRepository và implement IShopRepository
    /// </summary>
    public class ShopRepository : GenericRepository<Shop>, IShopRepository
    {
        public ShopRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Shop>> GetByLocationAsync(string location, bool includeInactive = false)
        {
            var query = _context.Shops
                .Include(s => s.CreatedBy)
                .Include(s => s.UpdatedBy)
                .Where(s => s.Location.Contains(location));

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive && !s.IsDeleted);
            }

            return await query.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<IEnumerable<Shop>> GetByShopTypeAsync(string shopType, bool includeInactive = false)
        {
            var query = _context.Shops
                .Include(s => s.CreatedBy)
                .Include(s => s.UpdatedBy)
                .Where(s => s.ShopType == shopType);

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive && !s.IsDeleted);
            }

            return await query.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<IEnumerable<Shop>> SearchByNameAsync(string keyword, bool includeInactive = false)
        {
            var query = _context.Shops
                .Include(s => s.CreatedBy)
                .Include(s => s.UpdatedBy)
                .Where(s => s.Name.Contains(keyword) || 
                           (s.Description != null && s.Description.Contains(keyword)));

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive && !s.IsDeleted);
            }

            return await query.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<Shop?> GetWithDetailsAsync(Guid id)
        {
            return await _context.Shops
                .Include(s => s.CreatedBy)
                .Include(s => s.UpdatedBy)
                // TODO: Include TourDetails when relationship is established
                // .Include(s => s.TourDetails)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<IEnumerable<Shop>> GetByMinRatingAsync(decimal minRating, bool includeInactive = false)
        {
            var query = _context.Shops
                .Include(s => s.CreatedBy)
                .Include(s => s.UpdatedBy)
                .Where(s => s.Rating >= minRating);

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive && !s.IsDeleted);
            }

            return await query.OrderByDescending(s => s.Rating).ToListAsync();
        }

        public async Task<IEnumerable<Shop>> GetByRatingRangeAsync(decimal minRating, decimal maxRating, bool includeInactive = false)
        {
            var query = _context.Shops
                .Include(s => s.CreatedBy)
                .Include(s => s.UpdatedBy)
                .Where(s => s.Rating >= minRating && s.Rating <= maxRating);

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive && !s.IsDeleted);
            }

            return await query.OrderByDescending(s => s.Rating).ToListAsync();
        }

        public async Task<(IEnumerable<Shop> Shops, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            string? location = null,
            string? shopType = null,
            decimal? minRating = null,
            bool includeInactive = false)
        {
            var query = _context.Shops
                .Include(s => s.CreatedBy)
                .Include(s => s.UpdatedBy)
                .AsQueryable();

            // Apply filters
            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive && !s.IsDeleted);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(s => s.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(shopType))
            {
                query = query.Where(s => s.ShopType == shopType);
            }

            if (minRating.HasValue)
            {
                query = query.Where(s => s.Rating >= minRating.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var shops = await query
                .OrderBy(s => s.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (shops, totalCount);
        }

        public async Task<IEnumerable<Shop>> GetShopsInUseAsync(bool includeInactive = false)
        {
            // TODO: Implement when TourDetails relationship is established
            // For now, return empty list
            var query = _context.Shops
                .Include(s => s.CreatedBy)
                .Include(s => s.UpdatedBy)
                .Where(s => false); // Placeholder condition

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive && !s.IsDeleted);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> IsShopInUseAsync(Guid id)
        {
            // TODO: Check if shop is used in any TourDetails when that relationship is established
            // For now, return false
            return await Task.FromResult(false);
        }

        public async Task<IEnumerable<Shop>> GetByCreatedByAsync(Guid createdById, bool includeInactive = false)
        {
            var query = _context.Shops
                .Include(s => s.CreatedBy)
                .Include(s => s.UpdatedBy)
                .Where(s => s.CreatedById == createdById);

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive && !s.IsDeleted);
            }

            return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        }
    }
}
