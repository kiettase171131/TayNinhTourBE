using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho AI Product Data - cung c?p thông tin s?n ph?m cho AI
    /// </summary>
    public class AIProductDataService : IAIProductDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AIProductDataService> _logger;

        public AIProductDataService(
            IUnitOfWork unitOfWork,
            ILogger<AIProductDataService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<AIProductInfo>> GetAvailableProductsAsync(int maxResults = 10)
        {
            try
            {
                _logger.LogInformation("Getting available products for AI from DATABASE, maxResults: {MaxResults}", maxResults);

                // Log that we're querying the real database
                _logger.LogInformation("Executing query against TayNinhTourDb database for active products with stock > 0");

                var products = await _unitOfWork.ProductRepository
                    .GetQueryable()
                    .Include(p => p.Shop)
                    .Include(p => p.SpecialtyShop)
                    .Include(p => p.ProductRatings)
                    .Where(p => p.IsActive && 
                               !p.IsDeleted &&
                               p.QuantityInStock > 0)
                    .OrderByDescending(p => p.SoldCount)
                    .Take(maxResults)
                    .ToListAsync();

                var productInfos = products.Select(p => new AIProductInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    SalePrice = p.IsSale && p.SalePercent.HasValue ? 
                              p.Price * (100 - p.SalePercent.Value) / 100 : null,
                    QuantityInStock = p.QuantityInStock,
                    Category = GetCategoryDisplayName(p.Category),
                    IsSale = p.IsSale,
                    SalePercent = p.SalePercent,
                    SoldCount = p.SoldCount,
                    ShopName = !string.IsNullOrEmpty(p.Shop?.Name) ? 
                              p.Shop.Name : 
                              p.SpecialtyShop?.ShopName ?? "Shop không xác định",
                    AverageRating = p.ProductRatings.Any() ? 
                                  p.ProductRatings.Average(r => r.Rating) : null,
                    ReviewCount = p.ProductRatings.Count
                }).ToList();

                _logger.LogInformation("SUCCESS: Found {Count} REAL products from database (not fake data)", productInfos.Count);
                
                // Log sample product names to verify real data
                if (productInfos.Any())
                {
                    var sampleNames = string.Join(", ", productInfos.Take(3).Select(p => p.Name));
                    _logger.LogInformation("Sample product names from DB: {SampleNames}", sampleNames);
                }
                else
                {
                    _logger.LogWarning("No products found in database - check if Product table has active products with stock > 0");
                }

                return productInfos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR: Failed to get products from database - this might cause AI to generate fake data");
                return new List<AIProductInfo>();
            }
        }

        public async Task<List<AIProductInfo>> SearchProductsAsync(string keyword, int maxResults = 10)
        {
            try
            {
                _logger.LogInformation("Searching products with keyword: {Keyword}", keyword);

                var products = await _unitOfWork.ProductRepository
                    .GetQueryable()
                    .Include(p => p.Shop)
                    .Include(p => p.SpecialtyShop)
                    .Include(p => p.ProductRatings)
                    .Where(p => p.IsActive && 
                               !p.IsDeleted &&
                               p.QuantityInStock > 0 &&
                               (p.Name.Contains(keyword) || 
                                (p.Description != null && p.Description.Contains(keyword))))
                    .OrderByDescending(p => p.SoldCount)
                    .Take(maxResults)
                    .ToListAsync();

                return products.Select(CreateAIProductInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with keyword {Keyword}", keyword);
                return new List<AIProductInfo>();
            }
        }

        public async Task<List<AIProductInfo>> GetProductsByCategoryAsync(string category, int maxResults = 10)
        {
            try
            {
                if (!Enum.TryParse<ProductCategory>(category, true, out var productCategory))
                {
                    return new List<AIProductInfo>();
                }

                var products = await _unitOfWork.ProductRepository
                    .GetQueryable()
                    .Include(p => p.Shop)
                    .Include(p => p.SpecialtyShop)
                    .Include(p => p.ProductRatings)
                    .Where(p => p.IsActive && 
                               !p.IsDeleted &&
                               p.QuantityInStock > 0 &&
                               p.Category == productCategory)
                    .OrderByDescending(p => p.SoldCount)
                    .Take(maxResults)
                    .ToListAsync();

                return products.Select(CreateAIProductInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category {Category}", category);
                return new List<AIProductInfo>();
            }
        }

        public async Task<List<AIProductInfo>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, int maxResults = 10)
        {
            try
            {
                var products = await _unitOfWork.ProductRepository
                    .GetQueryable()
                    .Include(p => p.Shop)
                    .Include(p => p.SpecialtyShop)
                    .Include(p => p.ProductRatings)
                    .Where(p => p.IsActive && 
                               !p.IsDeleted &&
                               p.QuantityInStock > 0 &&
                               p.Price >= minPrice && 
                               p.Price <= maxPrice)
                    .OrderBy(p => p.Price)
                    .Take(maxResults)
                    .ToListAsync();

                return products.Select(CreateAIProductInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by price range {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return new List<AIProductInfo>();
            }
        }

        public async Task<List<AIProductInfo>> GetProductsOnSaleAsync(int maxResults = 10)
        {
            try
            {
                var products = await _unitOfWork.ProductRepository
                    .GetQueryable()
                    .Include(p => p.Shop)
                    .Include(p => p.SpecialtyShop)
                    .Include(p => p.ProductRatings)
                    .Where(p => p.IsActive && 
                               !p.IsDeleted &&
                               p.QuantityInStock > 0 &&
                               p.IsSale && 
                               p.SalePercent.HasValue)
                    .OrderByDescending(p => p.SalePercent)
                    .Take(maxResults)
                    .ToListAsync();

                return products.Select(CreateAIProductInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products on sale");
                return new List<AIProductInfo>();
            }
        }

        public async Task<List<AIProductInfo>> GetBestSellingProductsAsync(int maxResults = 10)
        {
            try
            {
                var products = await _unitOfWork.ProductRepository
                    .GetQueryable()
                    .Include(p => p.Shop)
                    .Include(p => p.SpecialtyShop)
                    .Include(p => p.ProductRatings)
                    .Where(p => p.IsActive && 
                               !p.IsDeleted &&
                               p.QuantityInStock > 0 &&
                               p.SoldCount > 0)
                    .OrderByDescending(p => p.SoldCount)
                    .Take(maxResults)
                    .ToListAsync();

                return products.Select(CreateAIProductInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best selling products");
                return new List<AIProductInfo>();
            }
        }

        private AIProductInfo CreateAIProductInfo(Product product)
        {
            return new AIProductInfo
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                SalePrice = product.IsSale && product.SalePercent.HasValue
                    ? product.Price * (100 - product.SalePercent.Value) / 100
                    : null,
                QuantityInStock = product.QuantityInStock,
                Category = GetCategoryDisplayName(product.Category),
                IsSale = product.IsSale,
                SalePercent = product.SalePercent,
                SoldCount = product.SoldCount,
                ShopName = !string.IsNullOrEmpty(product.Shop?.Name)
                    ? product.Shop.Name
                    : product.SpecialtyShop?.ShopName ?? "Shop không xác định",
                AverageRating = product.ProductRatings?.Any() == true
                    ? product.ProductRatings.Average(r => r.Rating)
                    : null,
                ReviewCount = product.ProductRatings?.Count ?? 0
            };
        }

        private string GetCategoryDisplayName(ProductCategory category)
        {
            return category switch
            {
                ProductCategory.Food => "Thực phẩm",
                ProductCategory.Souvenir => "Quà lưu niệm",
                ProductCategory.Jewelry => "Trang sức",
                ProductCategory.Clothing => "Quần áo",
                _ => category.ToString()
            };
        }

    }
}