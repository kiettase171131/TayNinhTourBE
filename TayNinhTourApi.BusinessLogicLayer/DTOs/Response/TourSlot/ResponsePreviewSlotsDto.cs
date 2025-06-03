using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot
{
    /// <summary>
    /// DTO cho response preview tour slots
    /// </summary>
    public class ResponsePreviewSlotsDto
    {
        /// <summary>
        /// Có thành công không
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Thông báo
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Tổng số slots sẽ được tạo
        /// </summary>
        public int TotalSlotsToCreate { get; set; }

        /// <summary>
        /// Số slots đã tồn tại (sẽ bị skip)
        /// </summary>
        public int ExistingSlotsCount { get; set; }

        /// <summary>
        /// Danh sách slots sẽ được tạo
        /// </summary>
        public List<PreviewSlotInfo> SlotsToCreate { get; set; } = new List<PreviewSlotInfo>();

        /// <summary>
        /// Danh sách slots đã tồn tại
        /// </summary>
        public List<PreviewSlotInfo> ExistingSlots { get; set; } = new List<PreviewSlotInfo>();

        /// <summary>
        /// Thông tin tháng được preview
        /// </summary>
        public MonthInfo MonthInfo { get; set; } = new MonthInfo();
    }

    /// <summary>
    /// Thông tin preview của một slot
    /// </summary>
    public class PreviewSlotInfo
    {
        /// <summary>
        /// Ngày tour
        /// </summary>
        public DateOnly TourDate { get; set; }

        /// <summary>
        /// Ngày trong tuần
        /// </summary>
        public ScheduleDay ScheduleDay { get; set; }

        /// <summary>
        /// Tên ngày trong tuần
        /// </summary>
        public string ScheduleDayName { get; set; } = string.Empty;

        /// <summary>
        /// Có phải slot đã tồn tại không
        /// </summary>
        public bool IsExisting { get; set; }

        /// <summary>
        /// ID của slot nếu đã tồn tại
        /// </summary>
        public Guid? ExistingSlotId { get; set; }
    }

    /// <summary>
    /// Thông tin về tháng
    /// </summary>
    public class MonthInfo
    {
        /// <summary>
        /// Tháng
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Năm
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Tên tháng
        /// </summary>
        public string MonthName { get; set; } = string.Empty;

        /// <summary>
        /// Tổng số ngày trong tháng
        /// </summary>
        public int TotalDaysInMonth { get; set; }

        /// <summary>
        /// Số weekends trong tháng
        /// </summary>
        public int TotalWeekendsInMonth { get; set; }
    }
}
