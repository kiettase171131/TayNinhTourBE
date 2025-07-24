using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation
{
    /// <summary>
    /// DTO cho thông tin đầy đủ của TourOperation
    /// Sử dụng cho detail view và CRUD operations
    /// </summary>
    public class TourOperationDto
    {
        /// <summary>
        /// ID của tour operation
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của tour details
        /// </summary>
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// Giá tour cho operation này
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng khách tối đa
        /// </summary>
        public int MaxSeats { get; set; }

        /// <summary>
        /// Số lượng đã booking
        /// </summary>
        public int BookedSeats { get; set; } = 0;

        /// <summary>
        /// Mô tả bổ sung cho tour operation
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// ID của hướng dẫn viên
        /// </summary>
        public Guid? GuideId { get; set; }

        /// <summary>
        /// Tên hướng dẫn viên
        /// </summary>
        public string? GuideName { get; set; }

        /// <summary>
        /// Số điện thoại hướng dẫn viên
        /// </summary>
        public string? GuidePhone { get; set; }

        /// <summary>
        /// Ghi chú bổ sung
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }


    }
}
