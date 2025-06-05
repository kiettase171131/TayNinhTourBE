using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request tạo tour operation mới
    /// </summary>
    public class RequestCreateOperationDto
    {
        /// <summary>
        /// ID của TourSlot mà operation này thuộc về
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn tour slot")]
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// Giá tour cho operation này
        /// Có thể khác với giá gốc trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Required(ErrorMessage = "Giá tour là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng ghế tối đa cho tour operation này
        /// Có thể khác với MaxGuests trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Required(ErrorMessage = "Số ghế tối đa là bắt buộc")]
        [Range(1, 100, ErrorMessage = "Số ghế phải từ 1-100")]
        public int MaxSeats { get; set; }

        /// <summary>
        /// ID của User làm hướng dẫn viên cho tour này (optional)
        /// </summary>
        public Guid? GuideId { get; set; }

        /// <summary>
        /// Mô tả bổ sung cho tour operation
        /// Ví dụ: ghi chú về thời tiết, điều kiện đặc biệt, thay đổi lịch trình
        /// </summary>
        [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// Ghi chú bổ sung
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không quá 500 ký tự")]
        public string? Notes { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
