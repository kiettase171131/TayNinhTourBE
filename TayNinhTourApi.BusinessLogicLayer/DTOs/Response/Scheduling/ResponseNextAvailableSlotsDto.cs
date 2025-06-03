using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Scheduling
{
    /// <summary>
    /// Response DTO cho next available slots
    /// </summary>
    public class ResponseNextAvailableSlotsDto : BaseResposeDto
    {
        /// <summary>
        /// ID của tour template (nếu có)
        /// </summary>
        public Guid? TourTemplateId { get; set; }

        /// <summary>
        /// Tên tour template (nếu có)
        /// </summary>
        public string? TourTemplateName { get; set; }

        /// <summary>
        /// Số lượng slots được yêu cầu
        /// </summary>
        public int RequestedCount { get; set; }

        /// <summary>
        /// Số lượng slots thực tế tìm được
        /// </summary>
        public int FoundCount { get; set; }

        /// <summary>
        /// Ngày bắt đầu tìm kiếm
        /// </summary>
        public DateOnly SearchStartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc tìm kiếm
        /// </summary>
        public DateOnly SearchEndDate { get; set; }

        /// <summary>
        /// Danh sách available slots
        /// </summary>
        public List<AvailableSlotInfo> AvailableSlots { get; set; } = new();

        /// <summary>
        /// Có tìm được đủ slots theo yêu cầu không
        /// </summary>
        public bool HasEnoughSlots { get; set; }

        /// <summary>
        /// Thông tin về việc tìm kiếm
        /// </summary>
        public SearchInfo SearchInfo { get; set; } = new();
    }

    /// <summary>
    /// Thông tin chi tiết về một available slot
    /// </summary>
    public class AvailableSlotInfo
    {
        /// <summary>
        /// Ngày của slot
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Tên ngày trong tuần (tiếng Việt)
        /// </summary>
        public string DayName { get; set; } = string.Empty;

        /// <summary>
        /// Ngày trong tuần (enum)
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Tháng của slot
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Năm của slot
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Số ngày từ hôm nay
        /// </summary>
        public int DaysFromToday { get; set; }

        /// <summary>
        /// Có slot nào đã tồn tại cho ngày này không (nếu có TourTemplateId)
        /// </summary>
        public bool HasExistingSlot { get; set; }
    }

    /// <summary>
    /// Thông tin về quá trình tìm kiếm
    /// </summary>
    public class SearchInfo
    {
        /// <summary>
        /// Số tháng đã tìm kiếm
        /// </summary>
        public int MonthsSearched { get; set; }

        /// <summary>
        /// Tổng số weekend dates đã kiểm tra
        /// </summary>
        public int TotalWeekendDatesChecked { get; set; }

        /// <summary>
        /// Số slots bị loại trừ do đã tồn tại
        /// </summary>
        public int ExcludedExistingSlots { get; set; }

        /// <summary>
        /// Thời gian tìm kiếm (milliseconds)
        /// </summary>
        public long SearchTimeMs { get; set; }
    }
}
