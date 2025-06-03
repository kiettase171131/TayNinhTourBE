using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho preview schedule trước khi tạo slots
    /// </summary>
    public class SchedulePreviewDto
    {
        /// <summary>
        /// ID của tour template
        /// </summary>
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Tên tour template
        /// </summary>
        public string TourTemplateTitle { get; set; } = null!;

        /// <summary>
        /// Ngày trong tuần được chọn
        /// </summary>
        public ScheduleDay ScheduleDay { get; set; }

        /// <summary>
        /// Tên tiếng Việt của ngày trong tuần
        /// </summary>
        public string ScheduleDayName { get; set; } = null!;

        /// <summary>
        /// Tháng và năm được chọn
        /// </summary>
        public int Month { get; set; }
        public int Year { get; set; }

        /// <summary>
        /// Tên tháng bằng tiếng Việt
        /// </summary>
        public string MonthName { get; set; } = null!;

        /// <summary>
        /// Danh sách các ngày sẽ được tạo slots
        /// </summary>
        public List<ScheduleDatePreview> ScheduleDates { get; set; } = new List<ScheduleDatePreview>();

        /// <summary>
        /// Tổng số slots sẽ được tạo
        /// </summary>
        public int TotalSlotsToCreate => ScheduleDates.Count(sd => sd.CanCreate);

        /// <summary>
        /// Số slots đã tồn tại
        /// </summary>
        public int ExistingSlotsCount => ScheduleDates.Count(sd => sd.AlreadyExists);

        /// <summary>
        /// Số ngày bị skip (quá khứ hoặc không hợp lệ)
        /// </summary>
        public int SkippedDatesCount => ScheduleDates.Count(sd => sd.IsSkipped);

        /// <summary>
        /// Thời gian tạo preview
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Thông tin chi tiết về một ngày trong schedule preview
    /// </summary>
    public class ScheduleDatePreview
    {
        /// <summary>
        /// Ngày cụ thể
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Ngày trong tuần
        /// </summary>
        public ScheduleDay DayOfWeek { get; set; }

        /// <summary>
        /// Tên ngày trong tuần bằng tiếng Việt
        /// </summary>
        public string DayOfWeekName { get; set; } = null!;

        /// <summary>
        /// Có thể tạo slot cho ngày này không
        /// </summary>
        public bool CanCreate { get; set; }

        /// <summary>
        /// Đã có slot cho ngày này chưa
        /// </summary>
        public bool AlreadyExists { get; set; }

        /// <summary>
        /// Ngày này có bị skip không (quá khứ, không hợp lệ)
        /// </summary>
        public bool IsSkipped { get; set; }

        /// <summary>
        /// Lý do không thể tạo hoặc bị skip
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Ngày này có phải là ngày trong quá khứ không
        /// </summary>
        public bool IsPastDate { get; set; }
    }
}
