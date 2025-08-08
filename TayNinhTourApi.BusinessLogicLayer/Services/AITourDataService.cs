using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho AI Tour Data - cung cấp thông tin tour cho AI
    /// </summary>
    public class AITourDataService : IAITourDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AITourDataService> _logger;

        public AITourDataService(
            IUnitOfWork unitOfWork,
            ILogger<AITourDataService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<AITourInfo>> GetAvailableToursAsync(int maxResults = 10)
        {
            try
            {
                _logger.LogInformation("Getting available tours for AI, maxResults: {MaxResults}", maxResults);

                var tourSlots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                    .Include(ts => ts.TourTemplate.CreatedBy)
                    .Include(ts => ts.TourDetails)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.AvailableSpots > 0 &&
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourTemplate.IsActive)
                    .Take(maxResults * 3) // Get more to filter
                    .ToListAsync();

                // Group by template and create AITourInfo
                var tourInfos = tourSlots
                    .GroupBy(ts => ts.TourTemplateId)
                    .Select(g => new AITourInfo
                    {
                        Id = g.Key,
                        Title = g.First().TourTemplate.Title,
                        Description = "", // You can add description if available in TourTemplate
                        Price = CalculateMinPrice(g.First().TourTemplate.TemplateType),
                        StartLocation = g.First().TourTemplate.StartLocation,
                        EndLocation = g.First().TourTemplate.EndLocation,
                        TourType = g.First().TourTemplate.TemplateType.ToString(),
                        MaxGuests = g.Max(ts => ts.MaxGuests),
                        AvailableSlots = g.Sum(ts => ts.AvailableSpots),
                        Highlights = new List<string>(),
                        AvailableDates = g.Select(ts => ts.TourDate.ToDateTime(TimeOnly.MinValue)).OrderBy(d => d).ToList(),
                        CompanyName = g.First().TourTemplate.CreatedBy.Name ?? "Công ty tour",
                        IsPublic = true // Assuming if it's in available slots, it's public
                    })
                    .Take(maxResults)
                    .ToList();

                _logger.LogInformation("Found {Count} available tours", tourInfos.Count);
                return tourInfos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tours for AI");
                return new List<AITourInfo>();
            }
        }

        public async Task<List<AITourInfo>> SearchToursAsync(string keyword, int maxResults = 10)
        {
            try
            {
                _logger.LogInformation("Searching tours with keyword: {Keyword}", keyword);

                var tourSlots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                    .Include(ts => ts.TourTemplate.CreatedBy)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.AvailableSpots > 0 &&
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourTemplate.IsActive &&
                                (ts.TourTemplate.Title.Contains(keyword) ||
                                 ts.TourTemplate.StartLocation.Contains(keyword) ||
                                 ts.TourTemplate.EndLocation.Contains(keyword)))
                    .Take(maxResults * 2)
                    .ToListAsync();

                var tourInfos = tourSlots
                    .GroupBy(ts => ts.TourTemplateId)
                    .Select(g => CreateAITourInfo(g.ToList()))
                    .Take(maxResults)
                    .ToList();

                return tourInfos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tours with keyword {Keyword}", keyword);
                return new List<AITourInfo>();
            }
        }

        public async Task<AITourInfo?> GetTourDetailAsync(Guid tourId)
        {
            try
            {
                var tourSlots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                    .Include(ts => ts.TourTemplate.CreatedBy)
                    .Include(ts => ts.TourDetails)
                    .Where(ts => ts.TourTemplateId == tourId && 
                                ts.IsActive &&
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today))
                    .ToListAsync();

                if (!tourSlots.Any())
                    return null;

                return CreateAITourInfo(tourSlots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour detail {TourId}", tourId);
                return null;
            }
        }

        public async Task<List<AITourInfo>> GetToursByTypeAsync(string tourType, int maxResults = 10)
        {
            try
            {
                if (!Enum.TryParse<TourTemplateType>(tourType, true, out var templateType))
                {
                    return new List<AITourInfo>();
                }

                var tourSlots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                    .Include(ts => ts.TourTemplate.CreatedBy)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.AvailableSpots > 0 &&
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourTemplate.IsActive &&
                                ts.TourTemplate.TemplateType == templateType)
                    .Take(maxResults * 2)
                    .ToListAsync();

                var tourInfos = tourSlots
                    .GroupBy(ts => ts.TourTemplateId)
                    .Select(g => CreateAITourInfo(g.ToList()))
                    .Take(maxResults)
                    .ToList();

                return tourInfos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tours by type {TourType}", tourType);
                return new List<AITourInfo>();
            }
        }

        public async Task<List<AITourInfo>> GetToursByPriceRangeAsync(decimal minPrice, decimal maxPrice, int maxResults = 10)
        {
            try
            {
                // Since we don't have direct price in TourSlot, we'll get all available tours first
                // and filter by estimated price (this would need to be enhanced with actual pricing logic)
                var availableTours = await GetAvailableToursAsync(maxResults * 2);
                
                return availableTours
                    .Where(t => t.Price >= minPrice && t.Price <= maxPrice)
                    .Take(maxResults)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tours by price range {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return new List<AITourInfo>();
            }
        }

        public async Task<List<AITourInfo>> GetAvailableToursByDateAsync(DateTime date, int maxResults = 10)
        {
            try
            {
                var targetDate = DateOnly.FromDateTime(date);
                
                var tourSlots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                    .Include(ts => ts.TourTemplate.CreatedBy)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.AvailableSpots > 0 &&
                                ts.TourDate == targetDate &&
                                ts.TourTemplate.IsActive)
                    .Take(maxResults * 2)
                    .ToListAsync();

                var tourInfos = tourSlots
                    .GroupBy(ts => ts.TourTemplateId)
                    .Select(g => CreateAITourInfo(g.ToList()))
                    .Take(maxResults)
                    .ToList();

                return tourInfos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tours by date {Date}", date);
                return new List<AITourInfo>();
            }
        }

        private AITourInfo CreateAITourInfo(List<TourSlot> tourSlots)
        {
            var firstSlot = tourSlots.First();
            
            return new AITourInfo
            {
                Id = firstSlot.TourTemplateId,
                Title = firstSlot.TourTemplate.Title,
                Description = "", // Add if description exists in TourTemplate
                Price = CalculateMinPrice(firstSlot.TourTemplate.TemplateType),
                StartLocation = firstSlot.TourTemplate.StartLocation,
                EndLocation = firstSlot.TourTemplate.EndLocation,
                TourType = firstSlot.TourTemplate.TemplateType.ToString(),
                MaxGuests = tourSlots.Max(ts => ts.MaxGuests),
                AvailableSlots = tourSlots.Sum(ts => ts.AvailableSpots),
                Highlights = ExtractHighlights(firstSlot.TourTemplate),
                AvailableDates = tourSlots.Select(ts => ts.TourDate.ToDateTime(TimeOnly.MinValue))
                                        .OrderBy(d => d)
                                        .ToList(),
                CompanyName = firstSlot.TourTemplate.CreatedBy.Name ?? "Công ty tour",
                IsPublic = true
            };
        }

        private decimal CalculateMinPrice(TourTemplateType templateType)
        {
            // This would need actual pricing logic from your business rules
            // For now, return a default price based on tour type
            return templateType switch
            {
                TourTemplateType.FreeScenic => 200000,
                TourTemplateType.PaidAttraction => 500000,
                _ => 300000
            };
        }

        private List<string> ExtractHighlights(TourTemplate tourTemplate)
        {
            // Extract highlights from tour template data
            // This would depend on your actual data structure
            var highlights = new List<string>();
            
            if (tourTemplate.StartLocation.Contains("Núi Bà Đen"))
                highlights.Add("Cáp treo Núi Bà Đen");
                
            if (tourTemplate.EndLocation.Contains("Chùa"))
                highlights.Add("Tham quan chùa linh thiêng");
                
            highlights.Add("Hướng dẫn viên chuyên nghiệp");
            highlights.Add("Xe đưa đón tận nơi");

            if (tourTemplate.TemplateType == TourTemplateType.FreeScenic)
            {
                highlights.Add("Không phí vé vào cửa");
            }
            else if (tourTemplate.TemplateType == TourTemplateType.PaidAttraction)
            {
                highlights.Add("Bao gồm vé vào cửa các địa điểm");
            }
            
            return highlights;
        }
    }
}