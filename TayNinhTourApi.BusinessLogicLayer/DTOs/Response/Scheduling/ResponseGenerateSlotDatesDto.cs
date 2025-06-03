using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Scheduling
{
    /// <summary>
    /// Response DTO cho generate slot dates
    /// </summary>
    public class ResponseGenerateSlotDatesDto : BaseResposeDto
    {
        /// <summary>
        /// Năm được generate
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Tháng được generate
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Số lượng slots được yêu cầu
        /// </summary>
        public int RequestedSlots { get; set; }

        /// <summary>
        /// Số lượng slots thực tế được generate
        /// </summary>
        public int GeneratedSlots { get; set; }

        /// <summary>
        /// Danh sách slot dates được generate
        /// </summary>
        public List<SlotDateInfo> SlotDates { get; set; } = new();

        /// <summary>
        /// Có loại bỏ ngày quá khứ không
        /// </summary>
        public bool ExcludedPastDates { get; set; }

        /// <summary>
        /// Thông tin về algorithm được sử dụng
        /// </summary>
        public string AlgorithmUsed { get; set; } = string.Empty;

        /// <summary>
        /// Tổng số weekend dates có sẵn trong tháng
        /// </summary>
        public int TotalAvailableWeekendDates { get; set; }
    }

    /// <summary>
    /// Thông tin chi tiết về một slot date
    /// </summary>
    public class SlotDateInfo
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
        /// Thứ tự trong danh sách slots được generate
        /// </summary>
        public int SlotOrder { get; set; }

        /// <summary>
        /// Khoảng cách (ngày) từ slot trước đó
        /// </summary>
        public int? DaysFromPreviousSlot { get; set; }

        /// <summary>
        /// Có phải là slot được chọn bằng optimal distribution không
        /// </summary>
        public bool IsOptimallyDistributed { get; set; }
    }
}
