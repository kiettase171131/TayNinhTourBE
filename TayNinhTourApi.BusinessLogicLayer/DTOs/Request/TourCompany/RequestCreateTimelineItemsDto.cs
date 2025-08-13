using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request tạo mới timeline items (single hoặc bulk)
    /// </summary>
    public class RequestCreateTimelineItemsDto : IValidatableObject
    {
        /// <summary>
        /// ID của tour details mà timeline items này thuộc về
        /// </summary>
        [Required(ErrorMessage = "TourDetailsId là bắt buộc")]
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// Danh sách timeline items cần tạo
        /// Có thể là 1 item (single) hoặc nhiều items (bulk)
        /// </summary>
        [Required(ErrorMessage = "TimelineItems là bắt buộc")]
        [MinLength(1, ErrorMessage = "TimelineItems phải có ít nhất 1 item")]
        public List<TimelineItemCreateDto> TimelineItems { get; set; } = new List<TimelineItemCreateDto>();

        /// <summary>
        /// Custom validation to ensure timeline items are not empty and contain valid content
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            // Check if TimelineItems list exists and has at least one item
            if (TimelineItems == null || TimelineItems.Count == 0)
            {
                validationResults.Add(new ValidationResult(
                    "Phải có ít nhất 1 timeline item để tạo timeline", 
                    new[] { nameof(TimelineItems) }));
                return validationResults;
            }

            // Validate each timeline item for empty content
            for (int i = 0; i < TimelineItems.Count; i++)
            {
                var item = TimelineItems[i];
                var itemPrefix = $"TimelineItems[{i}]";

                // Check for empty or whitespace-only Activity
                if (string.IsNullOrWhiteSpace(item.Activity))
                {
                    validationResults.Add(new ValidationResult(
                        $"Timeline item #{i + 1}: Activity không được để trống hoặc chỉ chứa khoảng trắng",
                        new[] { $"{itemPrefix}.Activity" }));
                }

                // Check for empty or whitespace-only CheckInTime
                if (string.IsNullOrWhiteSpace(item.CheckInTime))
                {
                    validationResults.Add(new ValidationResult(
                        $"Timeline item #{i + 1}: CheckInTime không được để trống",
                        new[] { $"{itemPrefix}.CheckInTime" }));
                }
                else
                {
                    // Validate time format
                    if (!TimeSpan.TryParse(item.CheckInTime, out _))
                    {
                        validationResults.Add(new ValidationResult(
                            $"Timeline item #{i + 1}: CheckInTime '{item.CheckInTime}' không đúng định dạng thời gian (HH:mm)",
                            new[] { $"{itemPrefix}.CheckInTime" }));
                    }
                }
            }

            // Additional check: Ensure no completely empty timeline items exist
            var emptyItemsCount = TimelineItems.Count(item => 
                string.IsNullOrWhiteSpace(item.Activity) && 
                string.IsNullOrWhiteSpace(item.CheckInTime));

            if (emptyItemsCount > 0)
            {
                validationResults.Add(new ValidationResult(
                    $"Tìm thấy {emptyItemsCount} timeline item rỗng. Tất cả timeline items phải có nội dung hợp lệ",
                    new[] { nameof(TimelineItems) }));
            }

            return validationResults;
        }
    }

    /// <summary>
    /// DTO cho thông tin một timeline item cần tạo
    /// </summary>
    public class TimelineItemCreateDto
    {
        /// <summary>
        /// Thời gian check-in cho hoạt động này (giờ:phút)
        /// Ví dụ: 05:00, 07:00, 09:00, 10:00
        /// </summary>
        [Required(ErrorMessage = "CheckInTime là bắt buộc")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "CheckInTime phải có định dạng HH:mm (ví dụ: 09:30, 14:45)")]
        public string CheckInTime { get; set; } = string.Empty;

        /// <summary>
        /// Tên hoạt động hoặc mô tả ngắn
        /// Ví dụ: "Khởi hành", "Ăn sáng", "Ghé shop bánh tráng", "Tới Núi Bà"
        /// </summary>
        [Required(ErrorMessage = "Activity là bắt buộc")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Activity phải từ 3-255 ký tự")]
        public string Activity { get; set; } = string.Empty;

        /// <summary>
        /// ID của specialty shop liên quan (nếu có)
        /// Nullable - chỉ có giá trị khi hoạt động liên quan đến một specialty shop cụ thể
        /// </summary>
        public Guid? SpecialtyShopId { get; set; }

        /// <summary>
        /// Thứ tự sắp xếp trong timeline (tùy chọn, sẽ tự động assign nếu không có)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "SortOrder phải lớn hơn 0")]
        public int? SortOrder { get; set; }
    }
}
