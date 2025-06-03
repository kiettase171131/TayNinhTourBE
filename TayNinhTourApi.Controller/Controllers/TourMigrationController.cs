using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Migration;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller để migrate data từ Tour entity sang TourTemplate system
    /// Chỉ admin mới có quyền thực hiện migration
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Constants.RoleAdminName)]
    public class TourMigrationController : ControllerBase
    {
        private readonly ITourMigrationService _tourMigrationService;
        private readonly ILogger<TourMigrationController> _logger;

        public TourMigrationController(
            ITourMigrationService tourMigrationService,
            ILogger<TourMigrationController> logger)
        {
            _tourMigrationService = tourMigrationService;
            _logger = logger;
        }

        /// <summary>
        /// Preview migration - xem trước kết quả migration mà không thực sự thay đổi data
        /// </summary>
        /// <returns>Kết quả preview migration</returns>
        [HttpGet("preview")]
        public async Task<ActionResult<TourMigrationResult>> PreviewMigration()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest("User ID not found in claims.");
                }

                _logger.LogInformation("User {UserId} requested migration preview", userId);

                var result = await _tourMigrationService.PreviewMigrationAsync(userId.Value);
                
                _logger.LogInformation("Migration preview completed for user {UserId}. Success: {SuccessCount}, Failed: {FailedCount}", 
                    userId, result.SuccessCount, result.FailureCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during migration preview");
                return StatusCode(500, new { message = "Lỗi khi preview migration", error = ex.Message });
            }
        }

        /// <summary>
        /// Thực hiện migration từ Tour sang TourTemplate
        /// </summary>
        /// <param name="confirmMigration">Xác nhận thực hiện migration (phải là true)</param>
        /// <returns>Kết quả migration</returns>
        [HttpPost("execute")]
        public async Task<ActionResult<TourMigrationResult>> ExecuteMigration([FromQuery] bool confirmMigration = false)
        {
            try
            {
                if (!confirmMigration)
                {
                    return BadRequest(new { 
                        message = "Phải xác nhận migration bằng cách set confirmMigration=true",
                        warning = "Migration sẽ thay đổi dữ liệu trong database. Hãy chắc chắn bạn đã backup dữ liệu."
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest("User ID not found in claims.");
                }

                _logger.LogWarning("User {UserId} is executing Tour to TourTemplate migration", userId);

                var result = await _tourMigrationService.MigrateAllToursToTemplatesAsync(userId.Value, dryRun: false);
                
                if (result.IsCompleteSuccess)
                {
                    _logger.LogInformation("Migration completed successfully for user {UserId}. Migrated: {Count} tours", 
                        userId, result.SuccessCount);
                    return Ok(result);
                }
                else if (result.IsPartialSuccess)
                {
                    _logger.LogWarning("Migration completed with partial success for user {UserId}. Success: {SuccessCount}, Failed: {FailedCount}", 
                        userId, result.SuccessCount, result.FailureCount);
                    return Ok(result);
                }
                else
                {
                    _logger.LogError("Migration failed for user {UserId}. Failed: {FailedCount}", 
                        userId, result.FailureCount);
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during migration execution");
                return StatusCode(500, new { message = "Lỗi khi thực hiện migration", error = ex.Message });
            }
        }

        /// <summary>
        /// Rollback migration - khôi phục Tours từ TourTemplates
        /// </summary>
        /// <param name="confirmRollback">Xác nhận thực hiện rollback (phải là true)</param>
        /// <returns>Kết quả rollback</returns>
        [HttpPost("rollback")]
        public async Task<ActionResult<TourMigrationResult>> RollbackMigration([FromQuery] bool confirmRollback = false)
        {
            try
            {
                if (!confirmRollback)
                {
                    return BadRequest(new { 
                        message = "Phải xác nhận rollback bằng cách set confirmRollback=true",
                        warning = "Rollback sẽ khôi phục lại Tours từ trạng thái trước migration."
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return BadRequest("User ID not found in claims.");
                }

                _logger.LogWarning("User {UserId} is executing migration rollback", userId);

                var result = await _tourMigrationService.RollbackMigrationAsync(userId.Value);
                
                _logger.LogInformation("Rollback completed for user {UserId}. Processed: {Count} tours", 
                    userId, result.SuccessCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during migration rollback");
                return StatusCode(500, new { message = "Lỗi khi rollback migration", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thống kê migration status
        /// </summary>
        /// <returns>Thống kê về trạng thái migration</returns>
        [HttpGet("status")]
        public async Task<ActionResult<object>> GetMigrationStatus()
        {
            try
            {
                // Count tours that have been migrated (contain migration marker)
                var migratedToursCount = await CountMigratedTours();
                
                // Count total tours
                var totalToursCount = await CountTotalTours();
                
                // Count tour templates
                var tourTemplatesCount = await CountTourTemplates();

                var status = new
                {
                    TotalTours = totalToursCount,
                    MigratedTours = migratedToursCount,
                    UnmigratedTours = totalToursCount - migratedToursCount,
                    TourTemplates = tourTemplatesCount,
                    MigrationProgress = totalToursCount > 0 ? (double)migratedToursCount / totalToursCount * 100 : 0,
                    IsMigrationComplete = migratedToursCount == totalToursCount && totalToursCount > 0,
                    LastChecked = DateTime.UtcNow
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration status");
                return StatusCode(500, new { message = "Lỗi khi lấy trạng thái migration", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy current user ID từ claims
        /// </summary>
        /// <returns>User ID hoặc null nếu không tìm thấy</returns>
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        /// <summary>
        /// Đếm số tours đã được migrate
        /// </summary>
        /// <returns>Số lượng tours đã migrate</returns>
        private async Task<int> CountMigratedTours()
        {
            // This is a placeholder - implement actual counting logic
            // You might need to add this method to your repository
            return 0; // TODO: Implement actual counting
        }

        /// <summary>
        /// Đếm tổng số tours
        /// </summary>
        /// <returns>Tổng số tours</returns>
        private async Task<int> CountTotalTours()
        {
            // This is a placeholder - implement actual counting logic
            return 0; // TODO: Implement actual counting
        }

        /// <summary>
        /// Đếm số tour templates
        /// </summary>
        /// <returns>Số lượng tour templates</returns>
        private async Task<int> CountTourTemplates()
        {
            // This is a placeholder - implement actual counting logic
            return 0; // TODO: Implement actual counting
        }
    }
}
