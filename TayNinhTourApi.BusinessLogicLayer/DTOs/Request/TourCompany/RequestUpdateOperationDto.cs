using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request cập nhật tour operation
    /// Tất cả fields đều optional để cho phép partial update
    /// </summary>
    public class RequestUpdateOperationDto
    {
        /// <summary>
        /// ID của User làm hướng dẫn viên cho tour này
        /// </summary>
        public Guid? GuideId { get; set; }

        /// <summary>
        /// Giá tour cho operation này
        /// Có thể khác với giá gốc trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá tour phải lớn hơn 0")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Số lượng khách tối đa cho tour operation này
        /// Có thể khác với MaxGuests trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Số lượng khách phải từ 1 đến 1000")]
        public int? MaxGuests { get; set; }

        /// <summary>
        /// Mô tả bổ sung cho tour operation
        /// Ví dụ: ghi chú về thời tiết, điều kiện đặc biệt, thay đổi lịch trình
        /// </summary>
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái của tour operation
        /// </summary>
        public TourOperationStatus? Status { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của tour operation
        /// - true: Operation đang hoạt động và có thể booking
        /// - false: Operation tạm thời không hoạt động (guide bận, thời tiết xấu, etc.)
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
