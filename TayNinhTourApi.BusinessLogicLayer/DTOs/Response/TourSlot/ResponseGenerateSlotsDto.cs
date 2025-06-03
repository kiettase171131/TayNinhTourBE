using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot
{
    /// <summary>
    /// DTO cho response sau khi generate tour slots
    /// </summary>
    public class ResponseGenerateSlotsDto
    {
        /// <summary>
        /// Có thành công không
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Thông báo kết quả
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng slots đã tạo thành công
        /// </summary>
        public int CreatedSlotsCount { get; set; }

        /// <summary>
        /// Số lượng slots bị skip (đã tồn tại)
        /// </summary>
        public int SkippedSlotsCount { get; set; }

        /// <summary>
        /// Số lượng slots bị lỗi
        /// </summary>
        public int FailedSlotsCount { get; set; }

        /// <summary>
        /// Danh sách slots đã được tạo
        /// </summary>
        public List<TourSlotDto> CreatedSlots { get; set; } = new List<TourSlotDto>();

        /// <summary>
        /// Danh sách ngày bị skip với lý do
        /// </summary>
        public List<SkippedSlotInfo> SkippedSlots { get; set; } = new List<SkippedSlotInfo>();

        /// <summary>
        /// Danh sách lỗi nếu có
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Thông tin về slot bị skip
    /// </summary>
    public class SkippedSlotInfo
    {
        /// <summary>
        /// Ngày bị skip
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Lý do skip
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}
