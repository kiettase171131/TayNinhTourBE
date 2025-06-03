using AutoMapper;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Migration;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service để migrate data từ Tour entity sang TourTemplate system
    /// </summary>
    public class TourMigrationService : BaseService, ITourMigrationService
    {
        private readonly ILogger<TourMigrationService> _logger;

        public TourMigrationService(
            IMapper mapper, 
            IUnitOfWork unitOfWork,
            ILogger<TourMigrationService> logger) : base(mapper, unitOfWork)
        {
            _logger = logger;
        }

        /// <summary>
        /// Migrate tất cả Tours sang TourTemplates
        /// </summary>
        /// <param name="migratedById">ID của user thực hiện migration</param>
        /// <param name="dryRun">Chỉ preview không thực sự migrate</param>
        /// <returns>Kết quả migration</returns>
        public async Task<TourMigrationResult> MigrateAllToursToTemplatesAsync(Guid migratedById, bool dryRun = false)
        {
            _logger.LogInformation("Starting migration of all Tours to TourTemplates. DryRun: {DryRun}", dryRun);

            var result = new TourMigrationResult
            {
                StartedAt = DateTime.UtcNow,
                MigratedById = migratedById,
                IsDryRun = dryRun
            };

            try
            {
                // Lấy tất cả tours chưa bị xóa
                var tours = await _unitOfWork.TourRepository.GetAllAsync(
                    predicate: t => !t.IsDeleted,
                    include: new[] { "Images", "CreatedBy" }
                );

                _logger.LogInformation("Found {Count} tours to migrate", tours.Count());

                foreach (var tour in tours)
                {
                    try
                    {
                        var migrationItem = await MigrateSingleTourAsync(tour, migratedById, dryRun);
                        result.MigrationItems.Add(migrationItem);

                        if (migrationItem.IsSuccess)
                        {
                            result.SuccessCount++;
                        }
                        else
                        {
                            result.FailureCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error migrating tour {TourId}", tour.Id);
                        result.MigrationItems.Add(new TourMigrationItem
                        {
                            OriginalTourId = tour.Id,
                            OriginalTourTitle = tour.Title,
                            IsSuccess = false,
                            ErrorMessage = ex.Message
                        });
                        result.FailureCount++;
                    }
                }

                if (!dryRun && result.SuccessCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Migration completed successfully. Migrated {Count} tours", result.SuccessCount);
                }

                result.CompletedAt = DateTime.UtcNow;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tour migration");
                result.CompletedAt = DateTime.UtcNow;
                result.GlobalError = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Migrate một tour cụ thể sang TourTemplate
        /// </summary>
        /// <param name="tour">Tour cần migrate</param>
        /// <param name="migratedById">ID của user thực hiện migration</param>
        /// <param name="dryRun">Chỉ preview không thực sự migrate</param>
        /// <returns>Kết quả migration của tour này</returns>
        private async Task<TourMigrationItem> MigrateSingleTourAsync(Tour tour, Guid migratedById, bool dryRun)
        {
            var migrationItem = new TourMigrationItem
            {
                OriginalTourId = tour.Id,
                OriginalTourTitle = tour.Title
            };

            try
            {
                // Determine TourTemplateType based on TourType
                var templateType = DetermineTourTemplateType(tour.TourType);
                
                // Determine default ScheduleDay (Saturday for now, can be customized later)
                var scheduleDay = ScheduleDay.Saturday;

                // Create TourTemplate
                var tourTemplate = new TourTemplate
                {
                    Id = Guid.NewGuid(),
                    Title = tour.Title,
                    Description = tour.Description,
                    Price = tour.Price,
                    MaxGuests = tour.MaxGuests,
                    MinGuests = 1, // Default value
                    Duration = 1, // Default 1 day
                    TemplateType = templateType,
                    ScheduleDays = scheduleDay,
                    StartLocation = "TP.HCM", // Default start location
                    EndLocation = "Tây Ninh", // Default end location
                    IsActive = tour.IsActive,
                    CreatedById = tour.CreatedById,
                    CreatedAt = tour.CreatedAt,
                    UpdatedById = migratedById,
                    UpdatedAt = DateTime.UtcNow
                };

                migrationItem.NewTemplateId = tourTemplate.Id;
                migrationItem.NewTemplateTitle = tourTemplate.Title;
                migrationItem.TemplateType = templateType.ToString();
                migrationItem.ScheduleDay = scheduleDay.ToString();

                if (!dryRun)
                {
                    // Add TourTemplate to repository
                    await _unitOfWork.TourTemplateRepository.AddAsync(tourTemplate);

                    // Migrate images if any
                    if (tour.Images?.Any() == true)
                    {
                        foreach (var image in tour.Images)
                        {
                            // Add relationship between TourTemplate and Image
                            tourTemplate.Images.Add(image);
                        }
                        migrationItem.MigratedImagesCount = tour.Images.Count;
                    }

                    // Mark original tour as migrated (add a flag or comment)
                    tour.Description = $"[MIGRATED TO TEMPLATE {tourTemplate.Id}] {tour.Description}";
                    tour.UpdatedById = migratedById;
                    tour.UpdatedAt = DateTime.UtcNow;
                    
                    await _unitOfWork.TourRepository.Update(tour);
                }
                else
                {
                    // For dry run, just count images
                    migrationItem.MigratedImagesCount = tour.Images?.Count ?? 0;
                }

                migrationItem.IsSuccess = true;
                migrationItem.Notes = $"Successfully migrated to {templateType} template with {scheduleDay} schedule";

                return migrationItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating tour {TourId}", tour.Id);
                migrationItem.IsSuccess = false;
                migrationItem.ErrorMessage = ex.Message;
                return migrationItem;
            }
        }

        /// <summary>
        /// Xác định TourTemplateType dựa trên TourType của Tour
        /// </summary>
        /// <param name="tourType">TourType từ Tour entity</param>
        /// <returns>TourTemplateType tương ứng</returns>
        private TourTemplateType DetermineTourTemplateType(string tourType)
        {
            if (string.IsNullOrEmpty(tourType))
            {
                return TourTemplateType.FreeScenic; // Default
            }

            var lowerTourType = tourType.ToLower();

            // Mapping logic based on tour type keywords
            if (lowerTourType.Contains("vui chơi") || 
                lowerTourType.Contains("công viên") || 
                lowerTourType.Contains("giải trí") ||
                lowerTourType.Contains("paid") ||
                lowerTourType.Contains("phí"))
            {
                return TourTemplateType.PaidAttraction;
            }

            // Default to FreeScenic for scenic, cultural, historical tours
            return TourTemplateType.FreeScenic;
        }

        /// <summary>
        /// Preview migration - xem trước kết quả migration mà không thực sự thay đổi data
        /// </summary>
        /// <param name="migratedById">ID của user thực hiện preview</param>
        /// <returns>Kết quả preview</returns>
        public async Task<TourMigrationResult> PreviewMigrationAsync(Guid migratedById)
        {
            return await MigrateAllToursToTemplatesAsync(migratedById, dryRun: true);
        }

        /// <summary>
        /// Rollback migration - khôi phục Tours từ TourTemplates (nếu cần)
        /// </summary>
        /// <param name="rollbackById">ID của user thực hiện rollback</param>
        /// <returns>Kết quả rollback</returns>
        public async Task<TourMigrationResult> RollbackMigrationAsync(Guid rollbackById)
        {
            _logger.LogInformation("Starting rollback of TourTemplate migration");

            var result = new TourMigrationResult
            {
                StartedAt = DateTime.UtcNow,
                MigratedById = rollbackById,
                IsDryRun = false
            };

            try
            {
                // Find tours that have been migrated (contain migration marker in description)
                var migratedTours = await _unitOfWork.TourRepository.GetAllAsync(
                    predicate: t => !t.IsDeleted && t.Description != null && t.Description.Contains("[MIGRATED TO TEMPLATE"),
                    include: new[] { "Images" }
                );

                foreach (var tour in migratedTours)
                {
                    try
                    {
                        // Remove migration marker from description
                        var originalDescription = tour.Description;
                        if (originalDescription != null && originalDescription.Contains("[MIGRATED TO TEMPLATE"))
                        {
                            var markerStart = originalDescription.IndexOf("[MIGRATED TO TEMPLATE");
                            var markerEnd = originalDescription.IndexOf("]", markerStart);
                            if (markerEnd > markerStart)
                            {
                                tour.Description = originalDescription.Substring(markerEnd + 2).Trim();
                            }
                        }

                        tour.UpdatedById = rollbackById;
                        tour.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.TourRepository.Update(tour);

                        result.MigrationItems.Add(new TourMigrationItem
                        {
                            OriginalTourId = tour.Id,
                            OriginalTourTitle = tour.Title,
                            IsSuccess = true,
                            Notes = "Rollback completed - migration marker removed"
                        });

                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error rolling back tour {TourId}", tour.Id);
                        result.MigrationItems.Add(new TourMigrationItem
                        {
                            OriginalTourId = tour.Id,
                            OriginalTourTitle = tour.Title,
                            IsSuccess = false,
                            ErrorMessage = ex.Message
                        });
                        result.FailureCount++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                result.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation("Rollback completed. Processed {Count} tours", result.SuccessCount);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during migration rollback");
                result.CompletedAt = DateTime.UtcNow;
                result.GlobalError = ex.Message;
                return result;
            }
        }
    }
}
