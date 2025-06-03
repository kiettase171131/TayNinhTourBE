namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot
{
    /// <summary>
    /// DTO cho response kiểm tra conflicts khi tạo slots
    /// </summary>
    public class ResponseCheckSlotConflictsDto
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
        /// Có conflicts không
        /// </summary>
        public bool HasConflicts { get; set; }

        /// <summary>
        /// Số lượng dates bị conflict
        /// </summary>
        public int ConflictCount { get; set; }

        /// <summary>
        /// Danh sách dates bị conflict
        /// </summary>
        public List<ConflictSlotInfo> ConflictDates { get; set; } = new List<ConflictSlotInfo>();

        /// <summary>
        /// Danh sách dates có thể tạo
        /// </summary>
        public List<DateOnly> AvailableDates { get; set; } = new List<DateOnly>();
    }

    /// <summary>
    /// Thông tin về slot bị conflict
    /// </summary>
    public class ConflictSlotInfo
    {
        /// <summary>
        /// Ngày bị conflict
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// ID của slot đã tồn tại
        /// </summary>
        public Guid ExistingSlotId { get; set; }

        /// <summary>
        /// Status của slot đã tồn tại
        /// </summary>
        public string ExistingSlotStatus { get; set; } = string.Empty;

        /// <summary>
        /// Có active không
        /// </summary>
        public bool IsActive { get; set; }
    }
}
