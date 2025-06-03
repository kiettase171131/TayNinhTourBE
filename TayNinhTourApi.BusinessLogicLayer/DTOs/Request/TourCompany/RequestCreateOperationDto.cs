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
        /// ID của User làm hướng dẫn viên cho tour này
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn hướng dẫn viên")]
        public Guid GuideId { get; set; }

        /// <summary>
        /// Giá tour cho operation này
        /// Có thể khác với giá gốc trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập giá tour")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá tour phải lớn hơn 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng khách tối đa cho tour operation này
        /// Có thể khác với MaxGuests trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số lượng khách tối đa")]
        [Range(1, 1000, ErrorMessage = "Số lượng khách phải từ 1 đến 1000")]
        public int MaxGuests { get; set; }

        /// <summary>
        /// Mô tả bổ sung cho tour operation
        /// Ví dụ: ghi chú về thời tiết, điều kiện đặc biệt, thay đổi lịch trình
        /// </summary>
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }
    }
}
