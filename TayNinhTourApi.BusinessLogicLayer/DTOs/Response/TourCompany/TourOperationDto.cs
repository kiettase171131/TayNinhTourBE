using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response thông tin tour operation đầy đủ
    /// </summary>
    public class TourOperationDto
    {
        /// <summary>
        /// ID của tour operation
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của tour details mà operation này thuộc về
        /// </summary>
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// ID của hướng dẫn viên
        /// </summary>
        public Guid GuideId { get; set; }

        /// <summary>
        /// Tên hướng dẫn viên
        /// </summary>
        public string GuideName { get; set; } = string.Empty;

        /// <summary>
        /// Email hướng dẫn viên
        /// </summary>
        public string? GuideEmail { get; set; }

        /// <summary>
        /// Số điện thoại hướng dẫn viên
        /// </summary>
        public string? GuidePhoneNumber { get; set; }

        /// <summary>
        /// Giá thực tế của tour
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng khách tối đa
        /// </summary>
        public int MaxGuests { get; set; }

        /// <summary>
        /// Số lượng booking hiện tại
        /// </summary>
        public int CurrentBookings { get; set; }

        /// <summary>
        /// Số chỗ còn trống
        /// </summary>
        public int AvailableSpots => MaxGuests - CurrentBookings;

        /// <summary>
        /// Trạng thái của tour operation
        /// </summary>
        public TourOperationStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái bằng tiếng Việt
        /// </summary>
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái hoạt động của operation
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Thời gian tạo operation
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật operation lần cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
