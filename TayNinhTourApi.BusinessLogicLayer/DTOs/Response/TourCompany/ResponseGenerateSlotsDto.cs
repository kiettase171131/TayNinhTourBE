using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response kết quả tạo tour slots
    /// </summary>
    public class ResponseGenerateSlotsDto
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
        /// Tháng và năm được tạo slots
        /// </summary>
        public int Month { get; set; }
        public int Year { get; set; }

        /// <summary>
        /// Kết quả tạo slots có thành công không
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Thông báo kết quả
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Số slots được tạo thành công
        /// </summary>
        public int SuccessfulSlotsCount { get; set; }

        /// <summary>
        /// Số slots tạo thất bại
        /// </summary>
        public int FailedSlotsCount { get; set; }

        /// <summary>
        /// Số slots bị skip (đã tồn tại)
        /// </summary>
        public int SkippedSlotsCount { get; set; }

        /// <summary>
        /// Tổng số ngày được xử lý
        /// </summary>
        public int TotalDatesProcessed { get; set; }

        /// <summary>
        /// Danh sách slots được tạo thành công
        /// </summary>
        public List<TourSlotDto> CreatedSlots { get; set; } = new List<TourSlotDto>();

        /// <summary>
        /// Danh sách lỗi xảy ra trong quá trình tạo
        /// </summary>
        public List<SlotGenerationError> Errors { get; set; } = new List<SlotGenerationError>();

        /// <summary>
        /// Có tạo operations cùng lúc không
        /// </summary>
        public bool OperationsCreated { get; set; }

        /// <summary>
        /// Số operations được tạo thành công
        /// </summary>
        public int SuccessfulOperationsCount { get; set; }

        /// <summary>
        /// Thời gian bắt đầu tạo slots
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Thời gian hoàn thành tạo slots
        /// </summary>
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Thời gian xử lý (milliseconds)
        /// </summary>
        public long ProcessingTimeMs => (long)(CompletedAt - StartedAt).TotalMilliseconds;
    }

    /// <summary>
    /// Thông tin lỗi khi tạo slot
    /// </summary>
    public class SlotGenerationError
    {
        /// <summary>
        /// Ngày gặp lỗi
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Mã lỗi
        /// </summary>
        public string ErrorCode { get; set; } = null!;

        /// <summary>
        /// Thông báo lỗi
        /// </summary>
        public string ErrorMessage { get; set; } = null!;

        /// <summary>
        /// Chi tiết lỗi (nếu có)
        /// </summary>
        public string? Details { get; set; }
    }
}
