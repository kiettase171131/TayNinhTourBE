namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Migration
{
    /// <summary>
    /// Kết quả migration từ Tour sang TourTemplate
    /// </summary>
    public class TourMigrationResult
    {
        /// <summary>
        /// Thời gian bắt đầu migration
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Thời gian hoàn thành migration
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Thời gian xử lý (milliseconds)
        /// </summary>
        public long ProcessingTimeMs => CompletedAt.HasValue 
            ? (long)(CompletedAt.Value - StartedAt).TotalMilliseconds 
            : 0;

        /// <summary>
        /// ID của user thực hiện migration
        /// </summary>
        public Guid MigratedById { get; set; }

        /// <summary>
        /// Có phải là dry run không (chỉ preview)
        /// </summary>
        public bool IsDryRun { get; set; }

        /// <summary>
        /// Số lượng tours migrate thành công
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Số lượng tours migrate thất bại
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Tổng số tours được xử lý
        /// </summary>
        public int TotalCount => SuccessCount + FailureCount;

        /// <summary>
        /// Tỷ lệ thành công (%)
        /// </summary>
        public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;

        /// <summary>
        /// Danh sách chi tiết migration từng tour
        /// </summary>
        public List<TourMigrationItem> MigrationItems { get; set; } = new List<TourMigrationItem>();

        /// <summary>
        /// Lỗi global nếu có
        /// </summary>
        public string? GlobalError { get; set; }

        /// <summary>
        /// Có thành công hoàn toàn không
        /// </summary>
        public bool IsCompleteSuccess => FailureCount == 0 && string.IsNullOrEmpty(GlobalError);

        /// <summary>
        /// Có thành công một phần không
        /// </summary>
        public bool IsPartialSuccess => SuccessCount > 0 && FailureCount > 0;

        /// <summary>
        /// Có thất bại hoàn toàn không
        /// </summary>
        public bool IsCompleteFailure => SuccessCount == 0 && (FailureCount > 0 || !string.IsNullOrEmpty(GlobalError));

        /// <summary>
        /// Thông báo tóm tắt kết quả
        /// </summary>
        public string SummaryMessage
        {
            get
            {
                if (IsCompleteSuccess)
                {
                    return IsDryRun 
                        ? $"Preview thành công: {SuccessCount} tours có thể migrate"
                        : $"Migration thành công: {SuccessCount} tours đã được migrate";
                }
                else if (IsPartialSuccess)
                {
                    return IsDryRun
                        ? $"Preview một phần: {SuccessCount} tours có thể migrate, {FailureCount} tours có lỗi"
                        : $"Migration một phần: {SuccessCount} tours thành công, {FailureCount} tours thất bại";
                }
                else if (IsCompleteFailure)
                {
                    return IsDryRun
                        ? $"Preview thất bại: {FailureCount} tours có lỗi"
                        : $"Migration thất bại: {FailureCount} tours không thể migrate";
                }
                else
                {
                    return "Không có tours nào để xử lý";
                }
            }
        }

        /// <summary>
        /// Lấy danh sách tours migrate thành công
        /// </summary>
        public List<TourMigrationItem> SuccessfulMigrations => 
            MigrationItems.Where(item => item.success).ToList();

        /// <summary>
        /// Lấy danh sách tours migrate thất bại
        /// </summary>
        public List<TourMigrationItem> FailedMigrations => 
            MigrationItems.Where(item => !item.success).ToList();

        /// <summary>
        /// Lấy thống kê theo loại template
        /// </summary>
        public Dictionary<string, int> GetTemplateTypeStatistics()
        {
            return SuccessfulMigrations
                .Where(item => !string.IsNullOrEmpty(item.TemplateType))
                .GroupBy(item => item.TemplateType!)
                .ToDictionary(group => group.Key, group => group.Count());
        }

        /// <summary>
        /// Lấy thống kê theo schedule day
        /// </summary>
        public Dictionary<string, int> GetScheduleDayStatistics()
        {
            return SuccessfulMigrations
                .Where(item => !string.IsNullOrEmpty(item.ScheduleDay))
                .GroupBy(item => item.ScheduleDay!)
                .ToDictionary(group => group.Key, group => group.Count());
        }
    }

    /// <summary>
    /// Chi tiết migration của một tour cụ thể
    /// </summary>
    public class TourMigrationItem
    {
        /// <summary>
        /// ID của tour gốc
        /// </summary>
        public Guid OriginalTourId { get; set; }

        /// <summary>
        /// Tiêu đề của tour gốc
        /// </summary>
        public string OriginalTourTitle { get; set; } = null!;

        /// <summary>
        /// ID của template mới được tạo
        /// </summary>
        public Guid? NewTemplateId { get; set; }

        /// <summary>
        /// Tiêu đề của template mới
        /// </summary>
        public string? NewTemplateTitle { get; set; }

        /// <summary>
        /// Loại template được tạo
        /// </summary>
        public string? TemplateType { get; set; }

        /// <summary>
        /// Schedule day được assign
        /// </summary>
        public string? ScheduleDay { get; set; }

        /// <summary>
        /// Số lượng images được migrate
        /// </summary>
        public int MigratedImagesCount { get; set; }

        /// <summary>
        /// Có thành công không
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Thông báo lỗi nếu thất bại
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Thời gian xử lý item này
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
