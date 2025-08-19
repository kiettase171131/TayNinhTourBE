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
    /// Sửa để lấy giá thực từ TourOperation thay vì hardcode
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
                _logger.LogInformation("🔍 DEBUG: Getting REAL available tours for AI from database, maxResults: {MaxResults}", maxResults);

                // 🛠️ FIX: Replace AvailableSpots > 0 with MaxGuests > CurrentBookings
                // AvailableSpots is a computed property, not a database column
                var tourSlots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                        .ThenInclude(tt => tt.CreatedBy)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.MaxGuests > ts.CurrentBookings && // 🔧 FIX: Use database columns instead of computed property
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourTemplate.IsActive &&
                                ts.TourDetails != null && // Phải có TourDetails
                                ts.TourDetails.Status == TourDetailsStatus.Public && // Phải là PUBLIC
                                ts.TourDetails.TourOperation != null) // Phải có TourOperation với giá
                    .OrderBy(ts => ts.TourDate)
                    .Take(maxResults * 3) // Get more to filter
                    .ToListAsync();

                _logger.LogInformation("🔍 DEBUG: Raw query returned {Count} tour slots", tourSlots.Count);

                if (!tourSlots.Any())
                {
                    _logger.LogWarning("❌ DEBUG: No available tours found in database for AI to recommend - this should NOT happen based on debug data!");
                    return new List<AITourInfo>();
                }

                _logger.LogInformation("✅ DEBUG: Found {Count} real tour slots from database", tourSlots.Count);

                // Log each slot for debugging
                foreach (var slot in tourSlots)
                {
                    _logger.LogInformation("🎯 DEBUG: Slot {SlotId} - Date: {TourDate}, MaxGuests: {MaxGuests}, CurrentBookings: {CurrentBookings}, AvailableSpots: {AvailableSpots}, TourDetails: {HasTourDetails}, Operation: {HasOperation}", 
                        slot.Id, slot.TourDate, slot.MaxGuests, slot.CurrentBookings, (slot.MaxGuests - slot.CurrentBookings), slot.TourDetails != null, slot.TourDetails?.TourOperation != null);
                }

                // Group by template and create AITourInfo với giá thực từ TourOperation
                var tourInfos = new List<AITourInfo>();
                var groupedSlots = tourSlots.GroupBy(ts => ts.TourTemplateId);
                
                _logger.LogInformation("🔍 DEBUG: Grouped slots into {GroupCount} templates", groupedSlots.Count());

                foreach (var group in groupedSlots)
                {
                    try
                    {
                        var tourInfo = CreateAITourInfoWithRealPrice(group.ToList());
                        if (tourInfo != null)
                        {
                            tourInfos.Add(tourInfo);
                            _logger.LogInformation("✅ DEBUG: Created AITourInfo for template {TemplateId}: {Title}", 
                                group.Key, tourInfo.Title);
                        }
                        else
                        {
                            _logger.LogWarning("❌ DEBUG: CreateAITourInfoWithRealPrice returned null for template {TemplateId}", group.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ DEBUG: Error creating AITourInfo for template {TemplateId}", group.Key);
                    }
                }

                var finalTourInfos = tourInfos.Take(maxResults).ToList();
                
                _logger.LogInformation("🎉 DEBUG: Successfully created {Count} AITourInfo objects with REAL prices for AI recommendation", finalTourInfos.Count);
                
                // Log final tour info details
                foreach (var tour in finalTourInfos)
                {
                    _logger.LogInformation("🎯 DEBUG: Final AITourInfo - ID: {Id}, Title: {Title}, Price: {Price}, AvailableSlots: {AvailableSlots}", 
                        tour.Id, tour.Title, tour.Price, tour.AvailableSlots);
                }
                
                return finalTourInfos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 DEBUG: CRITICAL ERROR in GetAvailableToursAsync - This explains why AI gets no tours!");
                return new List<AITourInfo>();
            }
        }

        public async Task<List<AITourInfo>> SearchToursAsync(string keyword, int maxResults = 10)
        {
            try
            {
                _logger.LogInformation("Searching REAL tours with keyword: {Keyword}", keyword);

                var tourSlots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                        .ThenInclude(tt => tt.CreatedBy)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.MaxGuests > ts.CurrentBookings && // 🔧 FIX: Use database columns
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourTemplate.IsActive &&
                                ts.TourDetails != null &&
                                ts.TourDetails.Status == TourDetailsStatus.Public &&
                                ts.TourDetails.TourOperation != null &&
                                (ts.TourTemplate.Title.Contains(keyword) ||
                                 ts.TourTemplate.StartLocation.Contains(keyword) ||
                                 ts.TourTemplate.EndLocation.Contains(keyword)))
                    .OrderBy(ts => ts.TourDate)
                    .Take(maxResults * 2)
                    .ToListAsync();

                var tourInfos = tourSlots
                    .GroupBy(ts => ts.TourTemplateId)
                    .Select(g => CreateAITourInfoWithRealPrice(g.ToList()))
                    .Where(t => t != null)
                    .Take(maxResults)
                    .ToList();

                _logger.LogInformation("Found {Count} tours matching keyword '{Keyword}' with real pricing", tourInfos.Count, keyword);
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
                        .ThenInclude(tt => tt.CreatedBy)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.TourTemplateId == tourId && 
                                ts.IsActive &&
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourDetails != null &&
                                ts.TourDetails.Status == TourDetailsStatus.Public &&
                                ts.TourDetails.TourOperation != null)
                    .OrderBy(ts => ts.TourDate)
                    .ToListAsync();

                if (!tourSlots.Any())
                    return null;

                return CreateAITourInfoWithRealPrice(tourSlots);
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
                    _logger.LogWarning("Invalid tour type: {TourType}", tourType);
                    return new List<AITourInfo>();
                }

                var tourSlots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                        .ThenInclude(tt => tt.CreatedBy)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.MaxGuests > ts.CurrentBookings && // 🔧 FIX: Use database columns
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourTemplate.IsActive &&
                                ts.TourTemplate.TemplateType == templateType &&
                                ts.TourDetails != null &&
                                ts.TourDetails.Status == TourDetailsStatus.Public &&
                                ts.TourDetails.TourOperation != null)
                    .OrderBy(ts => ts.TourDate)
                    .Take(maxResults * 2)
                    .ToListAsync();

                var tourInfos = tourSlots
                    .GroupBy(ts => ts.TourTemplateId)
                    .Select(g => CreateAITourInfoWithRealPrice(g.ToList()))
                    .Where(t => t != null)
                    .Take(maxResults)
                    .ToList();

                _logger.LogInformation("Found {Count} tours of type {TourType} with real pricing", tourInfos.Count, tourType);
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
                _logger.LogInformation("Getting tours by REAL price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);

                // Lọc theo giá thực từ TourOperation
                var tourSlots = await _unitOfWork.TourSlotRepository
                    .GetQueryable()
                    .Include(ts => ts.TourTemplate)
                        .ThenInclude(tt => tt.CreatedBy)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.MaxGuests > ts.CurrentBookings && // 🔧 FIX: Use database columns
                                ts.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                ts.TourTemplate.IsActive &&
                                ts.TourDetails != null &&
                                ts.TourDetails.Status == TourDetailsStatus.Public &&
                                ts.TourDetails.TourOperation != null &&
                                ts.TourDetails.TourOperation.Price >= minPrice &&
                                ts.TourDetails.TourOperation.Price <= maxPrice)
                    .OrderBy(ts => ts.TourDetails.TourOperation.Price)
                    .Take(maxResults * 2)
                    .ToListAsync();

                var tourInfos = tourSlots
                    .GroupBy(ts => ts.TourTemplateId)
                    .Select(g => CreateAITourInfoWithRealPrice(g.ToList()))
                    .Where(t => t != null)
                    .Take(maxResults)
                    .ToList();

                _logger.LogInformation("Found {Count} tours in price range {MinPrice}-{MaxPrice} with real pricing", tourInfos.Count, minPrice, maxPrice);
                return tourInfos;
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
                        .ThenInclude(tt => tt.CreatedBy)
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td.TourOperation)
                    .Where(ts => ts.IsActive && 
                                ts.Status == TourSlotStatus.Available && 
                                ts.MaxGuests > ts.CurrentBookings && // 🔧 FIX: Use database columns
                                ts.TourDate == targetDate &&
                                ts.TourTemplate.IsActive &&
                                ts.TourDetails != null &&
                                ts.TourDetails.Status == TourDetailsStatus.Public &&
                                ts.TourDetails.TourOperation != null)
                    .OrderBy(ts => ts.TourDetails.TourOperation.Price)
                    .Take(maxResults * 2)
                    .ToListAsync();

                var tourInfos = tourSlots
                    .GroupBy(ts => ts.TourTemplateId)
                    .Select(g => CreateAITourInfoWithRealPrice(g.ToList()))
                    .Where(t => t != null)
                    .Take(maxResults)
                    .ToList();

                _logger.LogInformation("Found {Count} tours available on {Date} with real pricing", tourInfos.Count, date.ToString("yyyy-MM-dd"));
                return tourInfos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tours by date {Date}", date);
                return new List<AITourInfo>();
            }
        }

        /// <summary>
        /// Tạo AITourInfo với giá THỰC TẾ từ TourOperation thay vì hardcode
        /// </summary>
        private AITourInfo? CreateAITourInfoWithRealPrice(List<TourSlot> tourSlots)
        {
            if (!tourSlots.Any())
            {
                _logger.LogWarning("🔍 DEBUG: CreateAITourInfoWithRealPrice called with empty tourSlots list");
                return null;
            }

            var firstSlot = tourSlots.First();
            
            _logger.LogInformation("🔍 DEBUG: CreateAITourInfoWithRealPrice processing {Count} slots for template {TemplateId}", 
                tourSlots.Count, firstSlot.TourTemplateId);

            // Kiểm tra xem có TourOperation với giá thực không
            if (firstSlot.TourDetails?.TourOperation == null)
            {
                _logger.LogWarning("❌ DEBUG: TourSlot {SlotId} không có TourOperation, bỏ qua khỏi AI recommendation", firstSlot.Id);
                return null;
            }

            var tourOperation = firstSlot.TourDetails.TourOperation;
            var tourTemplate = firstSlot.TourTemplate;

            _logger.LogInformation("✅ DEBUG: Creating AITourInfo - Template: {Title}, Price: {Price}, TotalSlots: {SlotCount}", 
                tourTemplate.Title, tourOperation.Price, tourSlots.Count);

            try
            {
                var tourInfo = new AITourInfo
                {
                    Id = firstSlot.TourTemplateId,
                    Title = tourTemplate.Title,
                    Description = CreateTourDescription(tourTemplate, tourOperation),
                    Price = tourOperation.Price, // GIÁ THỰC TỪ DATABASE
                    StartLocation = tourTemplate.StartLocation,
                    EndLocation = tourTemplate.EndLocation,
                    TourType = GetTourTypeDisplay(tourTemplate.TemplateType),
                    MaxGuests = tourSlots.Max(ts => ts.MaxGuests),
                    AvailableSlots = tourSlots.Sum(ts => ts.AvailableSpots),
                    Highlights = ExtractRealHighlights(tourTemplate, tourOperation),
                    AvailableDates = tourSlots.Select(ts => ts.TourDate.ToDateTime(TimeOnly.MinValue))
                                            .OrderBy(d => d)
                                            .ToList(),
                    CompanyName = tourTemplate.CreatedBy?.Name ?? "Công ty du lịch",
                    IsPublic = firstSlot.TourDetails.Status == TourDetailsStatus.Public
                };

                _logger.LogInformation("🎉 DEBUG: Successfully created AITourInfo - ID: {Id}, Title: {Title}, AvailableSlots: {AvailableSlots}", 
                    tourInfo.Id, tourInfo.Title, tourInfo.AvailableSlots);

                return tourInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 DEBUG: Exception in CreateAITourInfoWithRealPrice for template {TemplateId}", firstSlot.TourTemplateId);
                return null;
            }
        }

        /// <summary>
        /// Tạo mô tả tour dựa trên thông tin thực tế
        /// </summary>
        private string CreateTourDescription(TourTemplate tourTemplate, TourOperation tourOperation)
        {
            var description = $"Tour {tourTemplate.TemplateType switch
            {
                TourTemplateType.FreeScenic => "tham quan danh lam thắng cảnh",
                TourTemplateType.PaidAttraction => "khám phá khu vui chơi giải trí",
                _ => "du lịch"
            }} từ {tourTemplate.StartLocation} đến {tourTemplate.EndLocation}.";

            if (!string.IsNullOrEmpty(tourOperation.Description))
            {
                description += $" {tourOperation.Description}";
            }

            return description;
        }

        /// <summary>
        /// Lấy highlights thực tế dựa trên thông tin tour
        /// </summary>
        private List<string> ExtractRealHighlights(TourTemplate tourTemplate, TourOperation tourOperation)
        {
            var highlights = new List<string>();
            
            // Highlights dựa trên tour type
            switch (tourTemplate.TemplateType)
            {
                case TourTemplateType.FreeScenic:
                    highlights.Add("🎯 Tour danh lam thắng cảnh - MIỄN PHÍ vé vào cửa");
                    break;
                case TourTemplateType.PaidAttraction:
                    highlights.Add("🎢 Tour khu vui chơi - BAO GỒM vé vào cửa");
                    break;
            }

            // Highlights dựa trên locations
            if (tourTemplate.StartLocation.Contains("Núi Bà Đen") || tourTemplate.EndLocation.Contains("Núi Bà Đen"))
                highlights.Add("⛰️ Tham quan Núi Bà Đen - Nóc nhà Đông Nam Bộ");
                
            if (tourTemplate.StartLocation.Contains("Chùa") || tourTemplate.EndLocation.Contains("Chùa") ||
                tourTemplate.StartLocation.Contains("Cao Đài") || tourTemplate.EndLocation.Contains("Cao Đài"))
                highlights.Add("🏛️ Khám phá thánh địa Cao Đài");

            // Highlights dựa trên capacity và giá
            highlights.Add($"👥 Tối đa {tourOperation.MaxGuests} khách - tour thân mật");
            highlights.Add($"💰 Giá tốt: {tourOperation.Price:N0} VNĐ/người");
            
            // Thêm highlights cố định
            highlights.Add("🚌 Xe đưa đón tận nơi");
            highlights.Add("👨‍🏫 Hướng dẫn viên chuyên nghiệp");
            highlights.Add("📱 Hỗ trợ đặt tour online 24/7");

            // Highlights từ operation notes
            if (!string.IsNullOrEmpty(tourOperation.Notes))
            {
                highlights.Add($"📝 {tourOperation.Notes}");
            }

            return highlights;
        }

        /// <summary>
        /// Hiển thị tên tour type thân thiện
        /// </summary>
        private string GetTourTypeDisplay(TourTemplateType tourType)
        {
            return tourType switch
            {
                TourTemplateType.FreeScenic => "Tour Danh Lam Thắng Cảnh (Miễn phí vé)",
                TourTemplateType.PaidAttraction => "Tour Khu Vui Chơi (Có vé vào cửa)",
                _ => tourType.ToString()
            };
        }
    }
}