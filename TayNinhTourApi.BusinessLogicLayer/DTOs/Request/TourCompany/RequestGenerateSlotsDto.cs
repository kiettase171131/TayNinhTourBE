using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request tự động tạo tour slots
    /// </summary>
    public class RequestGenerateSlotsDto
    {
        /// <summary>
        /// ID của tour template để tạo slots
        /// </summary>
        [Required(ErrorMessage = "TourTemplateId là bắt buộc")]
        public Guid TourTemplateId { get; set; }

        // ScheduleDay đã được loại bỏ - sẽ lấy từ TourTemplate.ScheduleDays

        /// <summary>
        /// Tháng để tạo slots (1-12)
        /// </summary>
        [Required(ErrorMessage = "Month là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Month phải từ 1 đến 12")]
        public int Month { get; set; }

        /// <summary>
        /// Năm để tạo slots (từ năm hiện tại trở đi)
        /// </summary>
        [Required(ErrorMessage = "Year là bắt buộc")]
        [Range(2024, 2030, ErrorMessage = "Year phải từ 2024 đến 2030")]
        public int Year { get; set; }

        /// <summary>
        /// Có tạo slots cho những ngày đã có slots không
        /// - true: Skip những ngày đã có slots
        /// - false: Báo lỗi nếu có ngày đã có slots
        /// </summary>
        public bool SkipExistingSlots { get; set; } = true;

        /// <summary>
        /// Có tự động tạo TourOperation cho mỗi slot không
        /// </summary>
        public bool CreateOperations { get; set; } = false;

        /// <summary>
        /// ID của guide mặc định nếu CreateOperations = true
        /// </summary>
        public Guid? DefaultGuideId { get; set; }
    }
}
