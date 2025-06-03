using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Migration;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Interface cho service migration từ Tour sang TourTemplate
    /// </summary>
    public interface ITourMigrationService
    {
        /// <summary>
        /// Migrate tất cả Tours sang TourTemplates
        /// </summary>
        /// <param name="migratedById">ID của user thực hiện migration</param>
        /// <param name="dryRun">Chỉ preview không thực sự migrate</param>
        /// <returns>Kết quả migration</returns>
        Task<TourMigrationResult> MigrateAllToursToTemplatesAsync(Guid migratedById, bool dryRun = false);

        /// <summary>
        /// Preview migration - xem trước kết quả migration mà không thực sự thay đổi data
        /// </summary>
        /// <param name="migratedById">ID của user thực hiện preview</param>
        /// <returns>Kết quả preview</returns>
        Task<TourMigrationResult> PreviewMigrationAsync(Guid migratedById);

        /// <summary>
        /// Rollback migration - khôi phục Tours từ TourTemplates (nếu cần)
        /// </summary>
        /// <param name="rollbackById">ID của user thực hiện rollback</param>
        /// <returns>Kết quả rollback</returns>
        Task<TourMigrationResult> RollbackMigrationAsync(Guid rollbackById);
    }
}
