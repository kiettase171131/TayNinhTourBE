using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.BusinessLogicLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service để thực hiện migration dữ liệu từ hệ thống cũ sang hệ thống mới
    /// </summary>
    public class DataMigrationService
    {
        private readonly TayNinhTouApiDbContext _context;
        private readonly ILogger<DataMigrationService> _logger;

        public DataMigrationService(TayNinhTouApiDbContext context, ILogger<DataMigrationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Migrate Languages data to Skills format in TourGuideApplications
        /// </summary>
        /// <returns>Number of records migrated</returns>
        public async Task<int> MigrateLanguagesToSkillsAsync()
        {
            try
            {
                _logger.LogInformation("Starting migration from Languages to Skills format");

                // Get all TourGuideApplications that need migration
                var applications = await _context.TourGuideApplications
                    .Where(app => !string.IsNullOrEmpty(app.Languages) && 
                                  (string.IsNullOrEmpty(app.Skills) || app.Skills == null))
                    .ToListAsync();

                if (!applications.Any())
                {
                    _logger.LogInformation("No TourGuideApplications found that need migration");
                    return 0;
                }

                _logger.LogInformation("Found {Count} TourGuideApplications to migrate", applications.Count);

                var migratedCount = 0;
                var errors = new List<string>();

                foreach (var application in applications)
                {
                    try
                    {
                        // Convert legacy languages to new skills format
                        var newSkills = TourGuideSkillUtility.MigrateLegacyLanguages(application.Languages);
                        
                        if (!string.IsNullOrEmpty(newSkills))
                        {
                            application.Skills = newSkills;
                            application.UpdatedAt = DateTime.UtcNow;
                            migratedCount++;

                            _logger.LogDebug("Migrated application {Id}: '{OldLanguages}' -> '{NewSkills}'", 
                                application.Id, application.Languages, newSkills);
                        }
                        else
                        {
                            // Fallback to default Vietnamese if migration fails
                            application.Skills = "Vietnamese";
                            application.UpdatedAt = DateTime.UtcNow;
                            migratedCount++;

                            _logger.LogWarning("Failed to migrate languages '{Languages}' for application {Id}, using default 'Vietnamese'", 
                                application.Languages, application.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = $"Error migrating application {application.Id}: {ex.Message}";
                        errors.Add(error);
                        _logger.LogError(ex, error);
                    }
                }

                // Save changes
                if (migratedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully migrated {Count} TourGuideApplications", migratedCount);
                }

                // Log any errors
                if (errors.Any())
                {
                    _logger.LogWarning("Migration completed with {ErrorCount} errors: {Errors}", 
                        errors.Count, string.Join("; ", errors));
                }

                return migratedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate Languages to Skills");
                throw;
            }
        }

        /// <summary>
        /// Validate migration results
        /// </summary>
        /// <returns>Migration validation report</returns>
        public async Task<MigrationValidationReport> ValidateMigrationAsync()
        {
            try
            {
                var report = new MigrationValidationReport();

                // Count total applications
                report.TotalApplications = await _context.TourGuideApplications.CountAsync();

                // Count applications with Skills
                report.ApplicationsWithSkills = await _context.TourGuideApplications
                    .CountAsync(app => !string.IsNullOrEmpty(app.Skills));

                // Count applications with Languages but no Skills
                report.ApplicationsNeedingMigration = await _context.TourGuideApplications
                    .CountAsync(app => !string.IsNullOrEmpty(app.Languages) && 
                                      (string.IsNullOrEmpty(app.Skills) || app.Skills == null));

                // Count applications with both Languages and Skills
                report.ApplicationsWithBoth = await _context.TourGuideApplications
                    .CountAsync(app => !string.IsNullOrEmpty(app.Languages) && 
                                      !string.IsNullOrEmpty(app.Skills));

                // Get sample data for verification
                report.SampleMigrations = await _context.TourGuideApplications
                    .Where(app => !string.IsNullOrEmpty(app.Languages) && 
                                  !string.IsNullOrEmpty(app.Skills))
                    .Select(app => new SampleMigration
                    {
                        ApplicationId = app.Id,
                        OriginalLanguages = app.Languages,
                        MigratedSkills = app.Skills,
                        IsValidSkillFormat = TourGuideSkillUtility.IsValidSkillsString(app.Skills)
                    })
                    .Take(10)
                    .ToListAsync();

                report.successful = report.ApplicationsNeedingMigration == 0;

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate migration");
                throw;
            }
        }

        /// <summary>
        /// Rollback migration (restore Languages field priority)
        /// </summary>
        /// <returns>Number of records rolled back</returns>
        public async Task<int> RollbackMigrationAsync()
        {
            try
            {
                _logger.LogInformation("Starting migration rollback");

                var applications = await _context.TourGuideApplications
                    .Where(app => !string.IsNullOrEmpty(app.Skills))
                    .ToListAsync();

                foreach (var application in applications)
                {
                    // Clear Skills field to force using Languages field
                    application.Skills = null;
                    application.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully rolled back {Count} TourGuideApplications", applications.Count);
                return applications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rollback migration");
                throw;
            }
        }
    }

    /// <summary>
    /// Report for migration validation
    /// </summary>
    public class MigrationValidationReport
    {
        public int TotalApplications { get; set; }
        public int ApplicationsWithSkills { get; set; }
        public int ApplicationsNeedingMigration { get; set; }
        public int ApplicationsWithBoth { get; set; }
        public bool successful { get; set; }
        public List<SampleMigration> SampleMigrations { get; set; } = new();
    }

    /// <summary>
    /// Sample migration data for verification
    /// </summary>
    public class SampleMigration
    {
        public Guid ApplicationId { get; set; }
        public string? OriginalLanguages { get; set; }
        public string? MigratedSkills { get; set; }
        public bool IsValidSkillFormat { get; set; }
    }
}
