using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using Microsoft.Extensions.Logging;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service cung c?p thông tin tour cho AI Chatbot
    /// </summary>
    public class AITourDataService : IAITourDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AITourDataService> _logger;

        public AITourDataService(IUnitOfWork unitOfWork, ILogger<AITourDataService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<AITourInfo>> GetAvailableToursAsync(int maxResults = 10)
        {
            try
            {
                var tours = await _unitOfWork.TourTemplateRepository
                    .GetAllAsync(t => t.IsActive && !t.IsDeleted, new[] { "CreatedBy", "TourDetails.TourOperation" });

                return tours.Take(maxResults).Select(MapToAITourInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tours");
                return new List<AITourInfo>();
            }
        }

        public async Task<List<AITourInfo>> SearchToursAsync(string keyword, int maxResults = 10)
        {
            try
            {
                var tours = await _unitOfWork.TourTemplateRepository
                    .GetAllAsync(
                        t => t.IsActive && !t.IsDeleted && 
                           (t.Title.Contains(keyword) || 
                            t.StartLocation.Contains(keyword) || 
                            t.EndLocation.Contains(keyword)),
                        new[] { "CreatedBy", "TourDetails.TourOperation" });

                return tours.Take(maxResults).Select(MapToAITourInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tours with keyword: {Keyword}", keyword);
                return new List<AITourInfo>();
            }
        }

        public async Task<AITourInfo?> GetTourDetailAsync(Guid tourId)
        {
            try
            {
                var tour = await _unitOfWork.TourTemplateRepository
                    .GetByIdAsync(tourId, new[] { "CreatedBy", "TourDetails.TourOperation", "TourDetails.Timeline" });

                return tour != null ? MapToAITourInfo(tour) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour detail for ID: {TourId}", tourId);
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

                var tours = await _unitOfWork.TourTemplateRepository
                    .GetByTemplateTypeAsync(templateType, false);

                return tours.Take(maxResults).Select(MapToAITourInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tours by type: {TourType}", tourType);
                return new List<AITourInfo>();
            }
        }

        public async Task<List<AITourInfo>> GetToursByPriceRangeAsync(decimal minPrice, decimal maxPrice, int maxResults = 10)
        {
            try
            {
                var tours = await _unitOfWork.TourTemplateRepository
                    .GetAllAsync(t => t.IsActive && !t.IsDeleted, new[] { "CreatedBy", "TourDetails.TourOperation" });

                var filteredTours = tours.Where(t =>
                {
                    var operation = t.TourDetails.FirstOrDefault()?.TourOperation;
                    return operation != null && operation.Price >= minPrice && operation.Price <= maxPrice;
                });

                return filteredTours.Take(maxResults).Select(MapToAITourInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tours by price range: {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return new List<AITourInfo>();
            }
        }

        private AITourInfo MapToAITourInfo(DataAccessLayer.Entities.TourTemplate tour)
        {
            var tourDetail = tour.TourDetails.FirstOrDefault();
            var operation = tourDetail?.TourOperation;

            return new AITourInfo
            {
                Id = tour.Id,
                Title = tour.Title,
                Description = tourDetail?.Description,
                Price = operation?.Price ?? 0,
                TourType = tour.TemplateType.ToString(),
                StartLocation = tour.StartLocation,
                EndLocation = tour.EndLocation,
                MaxGuests = operation?.MaxGuests ?? 0,
                AvailableSlots = operation != null ? Math.Max(0, operation.MaxGuests - operation.CurrentBookings) : 0,
                IsActive = tour.IsActive,
                Status = tourDetail?.Status.ToString() ?? "Unknown",
                SkillsRequired = !string.IsNullOrEmpty(tourDetail?.SkillsRequired) 
                    ? tourDetail.SkillsRequired.Split(',').Select(s => s.Trim()).ToList()
                    : new List<string>(),
                NextAvailableDate = DateTime.Now.AddDays(1), // Placeholder - có th? c?i thi?n b?ng cách check TourSlots
                Highlights = GenerateTourHighlights(tour)
            };
        }

        private List<string> GenerateTourHighlights(DataAccessLayer.Entities.TourTemplate tour)
        {
            var highlights = new List<string>();

            // Thêm highlights d?a trên thông tin tour
            if (tour.TemplateType == TourTemplateType.FreeScenic)
            {
                highlights.Add("Tour danh lam th?ng c?nh - không t?n vé vào c?a");
                highlights.Add("Phí d?ch v? bao g?m guide, xe, coordination");
            }
            else if (tour.TemplateType == TourTemplateType.PaidAttraction)
            {
                highlights.Add("Tour khu vui ch?i - phí d?ch v? + vé vào c?a");
                highlights.Add("Tr?i nghi?m ??y ?? v?i h??ng d?n viên");
            }

            if (tour.StartLocation.Contains("TP.HCM") || tour.StartLocation.Contains("H? Chí Minh"))
            {
                highlights.Add("Kh?i hành t? TP.HCM thu?n ti?n");
            }

            if (tour.EndLocation.Contains("Núi Bà ?en"))
            {
                highlights.Add("Khám phá ng?n núi cao nh?t Nam B?");
                highlights.Add("Tr?i nghi?m cáp treo hi?n ??i");
                highlights.Add("Th?m các ngôi chùa linh thiêng");
            }

            if (tour.EndLocation.Contains("Tây Ninh"))
            {
                highlights.Add("Tìm hi?u v?n hóa tâm linh ??c ?áo");
                highlights.Add("Th??ng th?c ?m th?c ??a ph??ng");
            }

            return highlights;
        }
    }
}