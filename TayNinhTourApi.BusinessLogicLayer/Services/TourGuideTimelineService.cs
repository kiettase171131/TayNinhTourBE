using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourGuide;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourGuide;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service for managing tour guide timeline progress
    /// </summary>
    public class TourGuideTimelineService : ITourGuideTimelineService
    {
        private readonly TayNinhTouApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TourGuideTimelineService> _logger;
        private readonly INotificationService _notificationService;

        public TourGuideTimelineService(
            TayNinhTouApiDbContext context,
            IMapper mapper,
            ILogger<TourGuideTimelineService> logger,
            INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<TimelineProgressResponse> GetTimelineWithProgressAsync(
            Guid tourSlotId,
            Guid userId,
            bool includeInactive = false,
            bool includeShopInfo = true)
        {
            try
            {
                _logger.LogInformation("Getting timeline with progress for tour slot {TourSlotId} by user {UserId}",
                    tourSlotId, userId);

                // Validate tour guide access
                if (!await ValidateTourGuideAccessAsync(tourSlotId, userId))
                {
                    throw new UnauthorizedAccessException("Tour guide không có quyền truy cập tour slot này");
                }

                // Get tour slot with tour details
                var tourSlot = await _context.TourSlots
                    .Include(ts => ts.TourDetails)
                    .ThenInclude(td => td.Timeline.Where(ti => includeInactive || ti.IsActive))
                    .ThenInclude(ti => ti.SpecialtyShop)
                    .Include(ts => ts.TimelineProgress.Where(tp => tp.IsActive))
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId);

                if (tourSlot?.TourDetails == null)
                {
                    throw new InvalidOperationException("Tour slot không tồn tại hoặc chưa được assign tour details");
                }

                // Get timeline items with progress
                var timelineItems = tourSlot.TourDetails.Timeline
                    .OrderBy(ti => ti.SortOrder)
                    .ToList();

                var progressRecords = tourSlot.TimelineProgress
                    .ToDictionary(tp => tp.TimelineItemId, tp => tp);

                // Build timeline with progress DTOs
                var timelineWithProgress = new List<TimelineWithProgressDto>();
                for (int i = 0; i < timelineItems.Count; i++)
                {
                    var item = timelineItems[i];
                    var progress = progressRecords.GetValueOrDefault(item.Id);

                    var dto = new TimelineWithProgressDto
                    {
                        Id = item.Id,
                        TourSlotId = tourSlotId,
                        ProgressId = progress?.Id,
                        Activity = item.Activity,
                        CheckInTime = TimeOnly.FromTimeSpan(item.CheckInTime),
                        SortOrder = item.SortOrder,
                        IsCompleted = progress?.IsCompleted ?? false,
                        CompletedAt = progress?.CompletedAt,
                        CompletionNotes = progress?.CompletionNotes,
                        CanComplete = await CanCompleteTimelineItemAsync(tourSlotId, item.Id, userId),
                        Position = i + 1,
                        TotalItems = timelineItems.Count,
                        CompletedByName = progress?.UpdatedBy?.Name,
                        CompletionDuration = progress?.GetCompletionDuration(),
                        StatusText = progress?.GetStatusText() ?? "Pending"
                    };

                    // Include specialty shop info if requested
                    if (includeShopInfo && item.SpecialtyShop != null)
                    {
                        dto.SpecialtyShop = _mapper.Map<DTOs.Response.SpecialtyShop.SpecialtyShopResponseDto>(item.SpecialtyShop);
                    }

                    // Set IsNext flag
                    dto.IsNext = dto.CanComplete && !dto.IsCompleted;

                    timelineWithProgress.Add(dto);
                }

                // Build summary
                var summary = new TimelineProgressSummaryDto
                {
                    TourSlotId = tourSlotId,
                    TotalItems = timelineItems.Count,
                    CompletedItems = timelineWithProgress.Count(t => t.IsCompleted),
                    NextItem = timelineWithProgress.FirstOrDefault(t => t.IsNext),
                    LastCompletedItem = timelineWithProgress.LastOrDefault(t => t.IsCompleted)
                };

                // Build tour slot info
                var tourSlotInfo = new TourSlotInfoDto
                {
                    Id = tourSlot.Id,
                    TourDate = tourSlot.TourDate,
                    CurrentBookings = tourSlot.CurrentBookings,
                    MaxGuests = tourSlot.MaxGuests,
                    Status = tourSlot.Status.ToString()
                };

                // Build tour details info
                var tourDetailsInfo = new TourDetailsInfoDto
                {
                    Id = tourSlot.TourDetails.Id,
                    Title = tourSlot.TourDetails.Title,
                    Description = tourSlot.TourDetails.Description,
                    Status = tourSlot.TourDetails.Status.ToString(),
                    ImageUrls = tourSlot.TourDetails.ImageUrls?.ToList() ?? new List<string>()
                };

                var response = new TimelineProgressResponse
                {
                    Timeline = timelineWithProgress,
                    Summary = summary,
                    TourSlot = tourSlotInfo,
                    TourDetails = tourDetailsInfo,
                    CanModifyProgress = true,
                    LastUpdated = DateTime.UtcNow
                };

                _logger.LogInformation("Successfully retrieved timeline with progress for tour slot {TourSlotId}", tourSlotId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline with progress for tour slot {TourSlotId}", tourSlotId);
                throw;
            }
        }

        public async Task<CompleteTimelineResponse> CompleteTimelineItemAsync(
            Guid tourSlotId,
            Guid timelineItemId,
            CompleteTimelineRequest request,
            Guid userId)
        {
            try
            {
                _logger.LogInformation("Completing timeline item {TimelineItemId} for tour slot {TourSlotId} by user {UserId}",
                    timelineItemId, tourSlotId, userId);

                // Validate request
                var validationErrors = request.Validate();
                if (validationErrors.Any())
                {
                    throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
                }

                // Validate tour guide access
                if (!await ValidateTourGuideAccessAsync(tourSlotId, userId))
                {
                    throw new UnauthorizedAccessException("Tour guide không có quyền truy cập tour slot này");
                }

                // Check if item can be completed (sequential validation)
                if (!await CanCompleteTimelineItemAsync(tourSlotId, timelineItemId, userId))
                {
                    throw new InvalidOperationException("Timeline item này chưa thể hoàn thành. Vui lòng hoàn thành các item trước đó theo thứ tự.");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Get or create progress record
                    var progress = await _context.TourSlotTimelineProgress
                        .FirstOrDefaultAsync(tp => tp.TourSlotId == tourSlotId && tp.TimelineItemId == timelineItemId);

                    if (progress == null)
                    {
                        progress = TourSlotTimelineProgress.Create(tourSlotId, timelineItemId, userId);
                        _context.TourSlotTimelineProgress.Add(progress);
                    }

                    // Mark as completed
                    progress.MarkAsCompleted(userId, request.Notes);

                    await _context.SaveChangesAsync();

                    // Get updated timeline item with progress
                    var completedItem = await GetTimelineItemWithProgressAsync(tourSlotId, timelineItemId);

                    // Get updated summary
                    var summary = await GetProgressSummaryAsync(tourSlotId, userId);

                    // Get next item
                    var nextItem = await GetNextTimelineItemAsync(tourSlotId, userId);

                    // Send notifications to guests
                    var notificationCount = await NotifyGuestsAboutProgressAsync(tourSlotId, timelineItemId, userId);

                    await transaction.CommitAsync();

                    var response = new CompleteTimelineResponse
                    {
                        Success = true,
                        Message = "Timeline item đã được hoàn thành thành công",
                        CompletedItem = completedItem,
                        Summary = summary,
                        NextItem = nextItem,
                        IsTimelineCompleted = summary.IsFullyCompleted,
                        CompletedAt = progress.CompletedAt ?? DateTime.UtcNow
                    };

                    if (notificationCount > 0)
                    {
                        response.Warnings.Add($"Đã gửi thông báo cho {notificationCount} khách hàng");
                    }

                    _logger.LogInformation("Successfully completed timeline item {TimelineItemId} for tour slot {TourSlotId}",
                        timelineItemId, tourSlotId);

                    return response;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing timeline item {TimelineItemId} for tour slot {TourSlotId}",
                    timelineItemId, tourSlotId);
                throw;
            }
        }

        public async Task<bool> ValidateTourGuideAccessAsync(Guid tourSlotId, Guid userId)
        {
            try
            {
                // Get tour guide record for the user
                var tourGuide = await _context.TourGuides
                    .FirstOrDefaultAsync(tg => tg.UserId == userId && tg.IsActive);

                if (tourGuide == null)
                {
                    return false;
                }

                // Check if tour guide is assigned to the tour operation for this slot
                var hasAccess = await _context.TourSlots
                    .Include(ts => ts.TourDetails)
                    .ThenInclude(td => td.TourOperation)
                    .AnyAsync(ts => ts.Id == tourSlotId &&
                                   ts.TourDetails != null &&
                                   ts.TourDetails.TourOperation != null &&
                                   ts.TourDetails.TourOperation.TourGuideId == tourGuide.Id);

                return hasAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tour guide access for tour slot {TourSlotId} and user {UserId}",
                    tourSlotId, userId);
                return false;
            }
        }

        public async Task<bool> CanCompleteTimelineItemAsync(Guid tourSlotId, Guid timelineItemId, Guid userId)
        {
            try
            {
                // Get timeline item
                var timelineItem = await _context.TimelineItems
                    .FirstOrDefaultAsync(ti => ti.Id == timelineItemId && ti.IsActive);

                if (timelineItem == null)
                {
                    return false;
                }

                // Check if already completed
                var existingProgress = await _context.TourSlotTimelineProgress
                    .FirstOrDefaultAsync(tp => tp.TourSlotId == tourSlotId &&
                                              tp.TimelineItemId == timelineItemId &&
                                              tp.IsActive);

                if (existingProgress?.IsCompleted == true)
                {
                    return false; // Already completed
                }

                // Check sequential completion - all previous items must be completed
                var incompleteEarlierItems = await _context.TourSlotTimelineProgress
                    .Include(tp => tp.TimelineItem)
                    .Where(tp => tp.TourSlotId == tourSlotId &&
                                tp.TimelineItem.TourDetailsId == timelineItem.TourDetailsId &&
                                tp.TimelineItem.SortOrder < timelineItem.SortOrder &&
                                !tp.IsCompleted &&
                                tp.IsActive)
                    .CountAsync();

                return incompleteEarlierItems == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if timeline item {TimelineItemId} can be completed for tour slot {TourSlotId}",
                    timelineItemId, tourSlotId);
                return false;
            }
        }

        public async Task<TimelineWithProgressDto?> GetNextTimelineItemAsync(Guid tourSlotId, Guid userId)
        {
            try
            {
                var timelineResponse = await GetTimelineWithProgressAsync(tourSlotId, userId, false, true);
                return timelineResponse.Timeline.FirstOrDefault(t => t.CanComplete && !t.IsCompleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next timeline item for tour slot {TourSlotId}", tourSlotId);
                return null;
            }
        }

        public async Task<TimelineProgressSummaryDto> GetProgressSummaryAsync(Guid tourSlotId, Guid userId)
        {
            try
            {
                var timelineResponse = await GetTimelineWithProgressAsync(tourSlotId, userId, false, false);
                return timelineResponse.Summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress summary for tour slot {TourSlotId}", tourSlotId);
                return new TimelineProgressSummaryDto { TourSlotId = tourSlotId };
            }
        }

        public async Task<int> CreateProgressRecordsForTourSlotAsync(Guid tourSlotId, Guid createdById)
        {
            try
            {
                _logger.LogInformation("Creating progress records for tour slot {TourSlotId}", tourSlotId);

                // Get tour slot with tour details and timeline
                var tourSlot = await _context.TourSlots
                    .Include(ts => ts.TourDetails)
                    .ThenInclude(td => td.Timeline.Where(ti => ti.IsActive))
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId);

                if (tourSlot?.TourDetails == null)
                {
                    _logger.LogWarning("Tour slot {TourSlotId} not found or has no tour details", tourSlotId);
                    return 0;
                }

                var timelineItems = tourSlot.TourDetails.Timeline.ToList();
                if (!timelineItems.Any())
                {
                    _logger.LogInformation("No timeline items found for tour slot {TourSlotId}", tourSlotId);
                    return 0;
                }

                // Check existing progress records
                var existingProgressIds = await _context.TourSlotTimelineProgress
                    .Where(tp => tp.TourSlotId == tourSlotId)
                    .Select(tp => tp.TimelineItemId)
                    .ToListAsync();

                // Create progress records for timeline items that don't have them yet
                var newProgressRecords = new List<TourSlotTimelineProgress>();
                foreach (var timelineItem in timelineItems)
                {
                    if (!existingProgressIds.Contains(timelineItem.Id))
                    {
                        var progress = TourSlotTimelineProgress.Create(tourSlotId, timelineItem.Id, createdById);
                        newProgressRecords.Add(progress);
                    }
                }

                if (newProgressRecords.Any())
                {
                    _context.TourSlotTimelineProgress.AddRange(newProgressRecords);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Created {Count} progress records for tour slot {TourSlotId}",
                    newProgressRecords.Count, tourSlotId);

                return newProgressRecords.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating progress records for tour slot {TourSlotId}", tourSlotId);
                throw;
            }
        }

        public async Task<BulkTimelineResponse> BulkCompleteTimelineItemsAsync(BulkCompleteTimelineRequest request, Guid userId)
        {
            var response = new BulkTimelineResponse
            {
                TotalCount = request.TimelineItemIds.Count
            };

            try
            {
                _logger.LogInformation("Bulk completing {Count} timeline items for tour slot {TourSlotId}",
                    request.TimelineItemIds.Count, request.TourSlotId);

                // Validate request
                var validationErrors = request.Validate();
                if (validationErrors.Any())
                {
                    throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var timelineItemId in request.TimelineItemIds)
                    {
                        try
                        {
                            var completeRequest = new CompleteTimelineRequest
                            {
                                Notes = request.Notes,
                                CompletionTime = DateTime.UtcNow
                            };

                            if (!request.RespectSequentialOrder || await CanCompleteTimelineItemAsync(request.TourSlotId, timelineItemId, userId))
                            {
                                await CompleteTimelineItemAsync(request.TourSlotId, timelineItemId, completeRequest, userId);
                                response.SuccessfulItems.Add(timelineItemId);
                                response.SuccessCount++;
                            }
                            else
                            {
                                response.FailedItems.Add(new BulkOperationError
                                {
                                    ItemId = timelineItemId,
                                    ErrorMessage = "Item không thể hoàn thành do vi phạm thứ tự sequential",
                                    ErrorCode = "SEQUENTIAL_ORDER_VIOLATION"
                                });
                                response.FailureCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            response.FailedItems.Add(new BulkOperationError
                            {
                                ItemId = timelineItemId,
                                ErrorMessage = ex.Message,
                                ErrorCode = "COMPLETION_ERROR"
                            });
                            response.FailureCount++;
                        }
                    }

                    // Get updated summary
                    response.Summary = await GetProgressSummaryAsync(request.TourSlotId, userId);

                    await transaction.CommitAsync();

                    response.Message = response.IsFullySuccessful
                        ? "Tất cả timeline items đã được hoàn thành thành công"
                        : $"Hoàn thành {response.SuccessCount}/{response.TotalCount} timeline items";

                    return response;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk completing timeline items for tour slot {TourSlotId}", request.TourSlotId);
                throw;
            }
        }

        public async Task<CompleteTimelineResponse> ResetTimelineItemAsync(
            Guid tourSlotId,
            Guid timelineItemId,
            ResetTimelineRequest request,
            Guid userId)
        {
            try
            {
                _logger.LogInformation("Resetting timeline item {TimelineItemId} for tour slot {TourSlotId}",
                    timelineItemId, tourSlotId);

                // Validate request
                var validationErrors = request.Validate();
                if (validationErrors.Any())
                {
                    throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
                }

                // Validate tour guide access
                if (!await ValidateTourGuideAccessAsync(tourSlotId, userId))
                {
                    throw new UnauthorizedAccessException("Tour guide không có quyền truy cập tour slot này");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Get progress record
                    var progress = await _context.TourSlotTimelineProgress
                        .FirstOrDefaultAsync(tp => tp.TourSlotId == tourSlotId && tp.TimelineItemId == timelineItemId);

                    if (progress == null || !progress.IsCompleted)
                    {
                        throw new InvalidOperationException("Timeline item chưa được hoàn thành hoặc không tồn tại");
                    }

                    // Reset completion
                    progress.ResetCompletion(userId);

                    // If requested, reset all subsequent items as well
                    if (request.ResetSubsequentItems)
                    {
                        var timelineItem = await _context.TimelineItems
                            .FirstOrDefaultAsync(ti => ti.Id == timelineItemId);

                        if (timelineItem != null)
                        {
                            var subsequentProgress = await _context.TourSlotTimelineProgress
                                .Include(tp => tp.TimelineItem)
                                .Where(tp => tp.TourSlotId == tourSlotId &&
                                           tp.TimelineItem.TourDetailsId == timelineItem.TourDetailsId &&
                                           tp.TimelineItem.SortOrder > timelineItem.SortOrder &&
                                           tp.IsCompleted)
                                .ToListAsync();

                            foreach (var subProgress in subsequentProgress)
                            {
                                subProgress.ResetCompletion(userId);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Get updated item and summary
                    var resetItem = await GetTimelineItemWithProgressAsync(tourSlotId, timelineItemId);
                    var summary = await GetProgressSummaryAsync(tourSlotId, userId);
                    var nextItem = await GetNextTimelineItemAsync(tourSlotId, userId);

                    return new CompleteTimelineResponse
                    {
                        Success = true,
                        Message = "Timeline item đã được reset thành công",
                        CompletedItem = resetItem,
                        Summary = summary,
                        NextItem = nextItem,
                        IsTimelineCompleted = false,
                        CompletedAt = DateTime.UtcNow
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting timeline item {TimelineItemId} for tour slot {TourSlotId}",
                    timelineItemId, tourSlotId);
                throw;
            }
        }

        public async Task<TimelineStatisticsResponse> GetTimelineStatisticsAsync(Guid tourSlotId, Guid userId)
        {
            try
            {
                // Validate access
                if (!await ValidateTourGuideAccessAsync(tourSlotId, userId))
                {
                    throw new UnauthorizedAccessException("Tour guide không có quyền truy cập tour slot này");
                }

                var progressRecords = await _context.TourSlotTimelineProgress
                    .Include(tp => tp.TimelineItem)
                    .Where(tp => tp.TourSlotId == tourSlotId && tp.IsActive)
                    .ToListAsync();

                var completedRecords = progressRecords.Where(tp => tp.IsCompleted).ToList();

                var statistics = new TimelineStatisticsResponse
                {
                    TourSlotId = tourSlotId,
                    CompletionRate = progressRecords.Count > 0 ? (double)completedRecords.Count / progressRecords.Count * 100 : 0,
                    OnTimeCompletions = completedRecords.Count(r => !IsOverdue(r)),
                    OverdueCompletions = completedRecords.Count(r => IsOverdue(r))
                };

                if (completedRecords.Any())
                {
                    var durations = completedRecords
                        .Where(r => r.GetCompletionDuration().HasValue)
                        .Select(r => r.GetCompletionDuration()!.Value.TotalMinutes)
                        .ToList();

                    if (durations.Any())
                    {
                        statistics.AverageCompletionTimeMinutes = durations.Average();
                        statistics.TotalTimeMinutes = durations.Sum();
                    }

                    // Find slowest and fastest items
                    var slowestRecord = completedRecords
                        .Where(r => r.GetCompletionDuration().HasValue)
                        .OrderByDescending(r => r.GetCompletionDuration()!.Value)
                        .FirstOrDefault();

                    var fastestRecord = completedRecords
                        .Where(r => r.GetCompletionDuration().HasValue)
                        .OrderBy(r => r.GetCompletionDuration()!.Value)
                        .FirstOrDefault();

                    if (slowestRecord != null)
                    {
                        statistics.SlowestItem = await GetTimelineItemWithProgressAsync(tourSlotId, slowestRecord.TimelineItemId);
                    }

                    if (fastestRecord != null)
                    {
                        statistics.FastestItem = await GetTimelineItemWithProgressAsync(tourSlotId, fastestRecord.TimelineItemId);
                    }

                    // Build completion trend
                    statistics.CompletionTrend = completedRecords
                        .Where(r => r.CompletedAt.HasValue)
                        .OrderBy(r => r.CompletedAt)
                        .Select((r, index) => new CompletionTrendPoint
                        {
                            Time = r.CompletedAt!.Value,
                            CompletionPercentage = (double)(index + 1) / progressRecords.Count * 100,
                            ItemsCompleted = index + 1
                        })
                        .ToList();
                }

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline statistics for tour slot {TourSlotId}", tourSlotId);
                throw;
            }
        }

        public async Task<List<CompletionTrendPoint>> GetCompletionHistoryAsync(Guid tourSlotId, Guid userId)
        {
            try
            {
                var statistics = await GetTimelineStatisticsAsync(tourSlotId, userId);
                return statistics.CompletionTrend;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completion history for tour slot {TourSlotId}", tourSlotId);
                return new List<CompletionTrendPoint>();
            }
        }

        public async Task<int> NotifyGuestsAboutProgressAsync(Guid tourSlotId, Guid timelineItemId, Guid userId)
        {
            try
            {
                // Get tour bookings for this slot
                var bookings = await _context.TourBookings
                    .Include(tb => tb.User)
                    .Where(tb => tb.TourSlotId == tourSlotId && tb.IsActive)
                    .ToListAsync();

                if (!bookings.Any())
                {
                    return 0;
                }

                // Get timeline item details
                var timelineItem = await _context.TimelineItems
                    .Include(ti => ti.SpecialtyShop)
                    .FirstOrDefaultAsync(ti => ti.Id == timelineItemId);

                if (timelineItem == null)
                {
                    return 0;
                }

                // Send notifications to all guests
                var notificationTasks = bookings.Select(async booking =>
                {
                    var message = $"Lịch trình tour đã cập nhật: {timelineItem.Activity} đã hoàn thành lúc {DateTime.Now:HH:mm}";

                    await _notificationService.CreateNotificationAsync(new DTOs.Request.Notification.CreateNotificationDto
                    {
                        UserId = booking.UserId,
                        Title = "Cập nhật lịch trình tour",
                        Message = message,
                        Type = DataAccessLayer.Enums.NotificationType.Tour,
                        Priority = DataAccessLayer.Enums.NotificationPriority.Normal,
                        Icon = "🎯"
                    });
                });

                await Task.WhenAll(notificationTasks);

                _logger.LogInformation("Sent timeline progress notifications to {Count} guests for tour slot {TourSlotId}",
                    bookings.Count, tourSlotId);

                return bookings.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications for timeline progress in tour slot {TourSlotId}", tourSlotId);
                return 0;
            }
        }

        // Helper methods
        private bool IsOverdue(TourSlotTimelineProgress progress)
        {
            if (!progress.IsCompleted || !progress.CompletedAt.HasValue)
                return false;

            var completedTime = TimeOnly.FromDateTime(progress.CompletedAt.Value);
            var checkInTime = TimeOnly.FromTimeSpan(progress.TimelineItem.CheckInTime);
            return completedTime > checkInTime;
        }

        // Helper method to get timeline item with progress
        private async Task<TimelineWithProgressDto?> GetTimelineItemWithProgressAsync(Guid tourSlotId, Guid timelineItemId)
        {
            var timelineResponse = await GetTimelineWithProgressAsync(tourSlotId, Guid.Empty, false, true);
            return timelineResponse.Timeline.FirstOrDefault(t => t.Id == timelineItemId);
        }
    }
}
