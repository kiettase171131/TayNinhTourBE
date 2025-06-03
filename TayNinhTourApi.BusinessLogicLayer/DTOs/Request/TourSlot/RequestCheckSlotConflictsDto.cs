using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot
{
    /// <summary>
    /// DTO cho request kiểm tra conflicts khi tạo slots mới
    /// </summary>
    public class RequestCheckSlotConflictsDto
    {
        /// <summary>
        /// ID của tour template cần kiểm tra conflicts
        /// </summary>
        [Required(ErrorMessage = "Tour Template ID là bắt buộc")]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Danh sách dates cần kiểm tra conflicts
        /// </summary>
        [Required(ErrorMessage = "Danh sách dates là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 date để kiểm tra")]
        public IEnumerable<DateOnly> Dates { get; set; } = new List<DateOnly>();
    }
}
