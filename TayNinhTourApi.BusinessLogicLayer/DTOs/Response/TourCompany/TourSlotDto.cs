using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response tour slot với thông tin template và operation
    /// </summary>
    public class TourSlotDto
    {
        /// <summary>
        /// ID của tour slot
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của tour template
        /// </summary>
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Ngày tour cụ thể sẽ diễn ra
        /// </summary>
        public DateOnly TourDate { get; set; }

        /// <summary>
        /// Ngày trong tuần của tour
        /// </summary>
        public ScheduleDay ScheduleDay { get; set; }

        /// <summary>
        /// Tên tiếng Việt của ngày trong tuần
        /// </summary>
        public string ScheduleDayName { get; set; } = null!;

        /// <summary>
        /// Trạng thái của tour slot
        /// </summary>
        public TourSlotStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái bằng tiếng Việt
        /// </summary>
        public string StatusName { get; set; } = null!;

        /// <summary>
        /// Trạng thái hoạt động của slot
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Thông tin tour template
        /// </summary>
        public TourTemplateDto? TourTemplate { get; set; }

        /// <summary>
        /// Thông tin operation của slot (nếu có)
        /// </summary>
        public TourOperationSummaryDto? Operation { get; set; }

        /// <summary>
        /// Thời gian tạo slot
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật slot lần cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin operation của tour slot (summary view)
    /// </summary>
    public class TourOperationSummaryDto
    {
        /// <summary>
        /// ID của tour operation
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của tour slot
        /// </summary>
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// ID của hướng dẫn viên
        /// </summary>
        public Guid GuideId { get; set; }

        /// <summary>
        /// Thông tin hướng dẫn viên
        /// </summary>
        public UserCmsDto? Guide { get; set; }

        /// <summary>
        /// Số lượng booking hiện tại
        /// </summary>
        public int CurrentBookings { get; set; }

        /// <summary>
        /// Số lượng khách tối đa
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Số chỗ còn trống
        /// </summary>
        public int AvailableSpots => MaxCapacity - CurrentBookings;

        /// <summary>
        /// Giá thực tế của tour (có thể khác với giá template)
        /// </summary>
        public decimal ActualPrice { get; set; }

        /// <summary>
        /// Mô tả bổ sung cho tour operation
        /// Ví dụ: ghi chú về thời tiết, điều kiện đặc biệt, thay đổi lịch trình
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái của tour operation
        /// </summary>
        public TourOperationStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái bằng tiếng Việt
        /// </summary>
        public string StatusName { get; set; } = null!;

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
