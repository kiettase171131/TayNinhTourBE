using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request cập nhật tour operation
    /// Tất cả fields đều optional để cho phép partial update
    /// </summary>
    public class RequestUpdateOperationDto
    {
        /// <summary>
        /// ID của TourGuide làm hướng dẫn viên cho tour này
        /// </summary>
        public Guid? GuideId { get; set; }

        /// <summary>
        /// Giá tour cho operation này
        /// Có thể khác với giá gốc trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Số lượng ghế tối đa cho tour operation này
        /// Có thể khác với MaxGuests trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Range(1, 100, ErrorMessage = "Số ghế phải từ 1-100")]
        public int? MaxSeats { get; set; }

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
        /// Trạng thái hoạt động của tour operation
        /// - true: Operation đang hoạt động và có thể booking
        /// - false: Operation tạm thời không hoạt động (guide bận, thời tiết xấu, etc.)
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
