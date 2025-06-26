using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.Services;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller để thực hiện data migration operations
    /// Chỉ dành cho Admin và development purposes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DataMigrationController : ControllerBase
    {
        private readonly DataMigrationService _dataMigrationService;
        private readonly ILogger<DataMigrationController> _logger;

        public DataMigrationController(DataMigrationService dataMigrationService, ILogger<DataMigrationController> logger)
        {
            _dataMigrationService = dataMigrationService;
            _logger = logger;
        }

        /// <summary>
        /// Migrate Languages data to Skills format in TourGuideApplications
        /// </summary>
        /// <returns>Number of records migrated</returns>
        [HttpPost("migrate-languages-to-skills")]
        public async Task<ActionResult<ApiResponse<int>>> MigrateLanguagesToSkills()
        {
            try
            {
                _logger.LogInformation("Starting Languages to Skills migration requested by admin");

                var migratedCount = await _dataMigrationService.MigrateLanguagesToSkillsAsync();

                return Ok(new ApiResponse<int>
                {
                    IsSuccess = true,
                    Message = $"Migration completed successfully. {migratedCount} records migrated.",
                    Data = migratedCount,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Languages to Skills migration");
                return StatusCode(500, new ApiResponse<int>
                {
                    IsSuccess = false,
                    Message = $"Migration failed: {ex.Message}",
                    Data = 0,
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Validate migration results
        /// </summary>
        /// <returns>Migration validation report</returns>
        [HttpGet("validate-migration")]
        public async Task<ActionResult<ApiResponse<MigrationValidationReport>>> ValidateMigration()
        {
            try
            {
                var report = await _dataMigrationService.ValidateMigrationAsync();

                return Ok(new ApiResponse<MigrationValidationReport>
                {
                    IsSuccess = true,
                    Message = "Migration validation completed",
                    Data = report,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during migration validation");
                return StatusCode(500, new ApiResponse<MigrationValidationReport>
                {
                    IsSuccess = false,
                    Message = $"Validation failed: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Rollback migration (restore Languages field priority)
        /// </summary>
        /// <returns>Number of records rolled back</returns>
        [HttpPost("rollback-migration")]
        public async Task<ActionResult<ApiResponse<int>>> RollbackMigration()
        {
            try
            {
                _logger.LogWarning("Migration rollback requested by admin");

                var rolledBackCount = await _dataMigrationService.RollbackMigrationAsync();

                return Ok(new ApiResponse<int>
                {
                    IsSuccess = true,
                    Message = $"Rollback completed successfully. {rolledBackCount} records rolled back.",
                    Data = rolledBackCount,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during migration rollback");
                return StatusCode(500, new ApiResponse<int>
                {
                    IsSuccess = false,
                    Message = $"Rollback failed: {ex.Message}",
                    Data = 0,
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Get migration status and statistics
        /// </summary>
        /// <returns>Migration status information</returns>
        [HttpGet("migration-status")]
        public async Task<ActionResult<ApiResponse<object>>> GetMigrationStatus()
        {
            try
            {
                var report = await _dataMigrationService.ValidateMigrationAsync();

                var status = new
                {
                    TotalApplications = report.TotalApplications,
                    ApplicationsWithSkills = report.ApplicationsWithSkills,
                    ApplicationsNeedingMigration = report.ApplicationsNeedingMigration,
                    ApplicationsWithBoth = report.ApplicationsWithBoth,
                    MigrationComplete = report.IsSuccessful,
                    MigrationProgress = report.TotalApplications > 0 
                        ? (double)report.ApplicationsWithSkills / report.TotalApplications * 100 
                        : 0,
                    SampleMigrations = report.SampleMigrations.Take(5) // Show only first 5 samples
                };

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Migration status retrieved successfully",
                    Data = status,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration status");
                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Failed to get migration status: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Test skill matching with sample data
        /// </summary>
        /// <param name="requiredSkills">Required skills string</param>
        /// <param name="guideSkills">Guide skills string</param>
        /// <returns>Skill matching test results</returns>
        [HttpPost("test-skill-matching")]
        public ActionResult<ApiResponse<object>> TestSkillMatching([FromBody] SkillMatchingTestRequest request)
        {
            try
            {
                var isMatch = TayNinhTourApi.BusinessLogicLayer.Utilities.SkillsMatchingUtility
                    .MatchSkillsEnhanced(request.RequiredSkills, request.GuideSkills);
                
                var matchScore = TayNinhTourApi.BusinessLogicLayer.Utilities.SkillsMatchingUtility
                    .CalculateMatchScoreEnhanced(request.RequiredSkills, request.GuideSkills);
                
                var matchedSkills = TayNinhTourApi.BusinessLogicLayer.Utilities.SkillsMatchingUtility
                    .GetMatchedSkillsEnhanced(request.RequiredSkills, request.GuideSkills);

                var result = new
                {
                    RequiredSkills = request.RequiredSkills,
                    GuideSkills = request.GuideSkills,
                    IsMatch = isMatch,
                    MatchScore = matchScore,
                    MatchedSkills = matchedSkills,
                    MatchPercentage = matchScore * 100
                };

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Skill matching test completed",
                    Data = result,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during skill matching test");
                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Skill matching test failed: {ex.Message}",
                    StatusCode = 500
                });
            }
        }
    }

    /// <summary>
    /// Request DTO for skill matching test
    /// </summary>
    public class SkillMatchingTestRequest
    {
        /// <summary>
        /// Required skills string
        /// </summary>
        public string RequiredSkills { get; set; } = null!;

        /// <summary>
        /// Guide skills string
        /// </summary>
        public string GuideSkills { get; set; } = null!;
    }
}
