using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request tạo mới timeline item
    /// </summary>
    public class RequestCreateTimelineItemDto : IValidatableObject
    {
        /// <summary>
        /// ID của tour details mà timeline item này thuộc về
        /// </summary>
        [Required(ErrorMessage = "TourDetailsId là bắt buộc")]
        public Guid TourDetailsId { get; set; }

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

        /// <summary>
        /// Custom validation to ensure timeline item content is not empty
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            // Check for empty or whitespace-only Activity
            if (string.IsNullOrWhiteSpace(Activity))
            {
                validationResults.Add(new ValidationResult(
                    "Activity không được để trống hoặc chỉ chứa khoảng trắng",
                    new[] { nameof(Activity) }));
            }

            // Check for empty or whitespace-only CheckInTime
            if (string.IsNullOrWhiteSpace(CheckInTime))
            {
                validationResults.Add(new ValidationResult(
                    "CheckInTime không được để trống",
                    new[] { nameof(CheckInTime) }));
            }
            else
            {
                // Validate time format (additional check beyond regex)
                if (!TimeSpan.TryParse(CheckInTime, out var timeSpan))
                {
                    validationResults.Add(new ValidationResult(
                        $"CheckInTime '{CheckInTime}' không đúng định dạng thời gian (HH:mm)",
                        new[] { nameof(CheckInTime) }));
                }
                else
                {
                    // Validate time range (00:00 to 23:59)
                    if (timeSpan.TotalDays >= 1)
                    {
                        validationResults.Add(new ValidationResult(
                            "CheckInTime phải trong khoảng 00:00 - 23:59",
                            new[] { nameof(CheckInTime) }));
                    }
                }
            }

            // Additional validation: Activity should have meaningful content (not just spaces or special characters)
            if (!string.IsNullOrWhiteSpace(Activity))
            {
                var meaningfulContent = Activity.Trim();
                if (meaningfulContent.Length < 3)
                {
                    validationResults.Add(new ValidationResult(
                        "Activity phải có ít nhất 3 ký tự có nghĩa (không tính khoảng trắng)",
                        new[] { nameof(Activity) }));
                }
            }

            return validationResults;
        }
    }
}
