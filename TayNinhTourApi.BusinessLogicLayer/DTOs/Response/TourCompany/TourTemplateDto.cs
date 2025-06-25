using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response của TourTemplate (đã đơn giản hóa)
    /// </summary>
    public class TourTemplateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;

        public string TemplateType { get; set; } = null!;
        public string ScheduleDays { get; set; } = null!;
        public string StartLocation { get; set; } = null!;
        public string EndLocation { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Thông tin capacity tổng quan cho template
        /// </summary>
        public TemplateCapacitySummaryDto? CapacitySummary { get; set; }
    }

    /// <summary>
    /// DTO cho response chi tiết của TourTemplate (đã đơn giản hóa)
    /// </summary>
    public class TourTemplateDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;

        public TourTemplateType TemplateType { get; set; }
        public ScheduleDay ScheduleDays { get; set; }
        public string StartLocation { get; set; } = null!;
        public string EndLocation { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public UserCmsDto? CreatedBy { get; set; }
        public UserCmsDto? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Thông tin capacity chi tiết cho template
        /// </summary>
        public TemplateCapacitySummaryDto? CapacitySummary { get; set; }
    }

    /// <summary>
    /// DTO cho response summary của TourTemplate (dùng cho listing - đã đơn giản hóa)
    /// </summary>
    public class TourTemplateSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string TemplateType { get; set; } = null!;
        public string StartLocation { get; set; } = null!;
        public string EndLocation { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thông tin capacity tổng quan cho template
        /// </summary>
        public TemplateCapacitySummaryDto? CapacitySummary { get; set; }
    }

    /// <summary>
    /// DTO cho response danh sách TourTemplate với pagination
    /// </summary>
    public class ResponseGetTourTemplatesDto
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public List<TourTemplateSummaryDto> Data { get; set; } = new List<TourTemplateSummaryDto>();
        public int TotalRecord { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin capacity tổng quan của template
    /// </summary>
    public class TemplateCapacitySummaryDto
    {
        /// <summary>
        /// Tổng số slots có sẵn
        /// </summary>
        public int TotalSlots { get; set; }

        /// <summary>
        /// Số slots đang active (có thể booking)
        /// </summary>
        public int ActiveSlots { get; set; }

        /// <summary>
        /// Tổng capacity tối đa (tổng MaxGuests của tất cả operations)
        /// </summary>
        public int TotalMaxCapacity { get; set; }

        /// <summary>
        /// Tổng số khách đã booking
        /// </summary>
        public int TotalBookedGuests { get; set; }

        /// <summary>
        /// Tổng số chỗ còn trống
        /// </summary>
        public int TotalAvailableCapacity => TotalMaxCapacity - TotalBookedGuests;

        /// <summary>
        /// Phần trăm đã booking
        /// </summary>
        public decimal BookingPercentage => TotalMaxCapacity > 0 ? (decimal)TotalBookedGuests / TotalMaxCapacity * 100 : 0;

        /// <summary>
        /// Có slots nào còn chỗ trống không
        /// </summary>
        public bool HasAvailableSlots => TotalAvailableCapacity > 0;

        /// <summary>
        /// Ngày gần nhất có slot available
        /// </summary>
        public DateTime? NextAvailableDate { get; set; }

        /// <summary>
        /// Số operations đang hoạt động
        /// </summary>
        public int ActiveOperations { get; set; }
    }
}
