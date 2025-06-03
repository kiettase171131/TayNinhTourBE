using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot
{
    /// <summary>
    /// DTO cho request lấy danh sách tour slots với filtering
    /// </summary>
    public class RequestGetSlotsDto
    {
        /// <summary>
        /// ID của tour template để filter (optional)
        /// </summary>
        public Guid? TourTemplateId { get; set; }

        /// <summary>
        /// Ngày bắt đầu để filter (optional)
        /// </summary>
        public DateOnly? FromDate { get; set; }

        /// <summary>
        /// Ngày kết thúc để filter (optional)
        /// </summary>
        public DateOnly? ToDate { get; set; }

        /// <summary>
        /// Status của slots để filter (optional)
        /// </summary>
        public TourSlotStatus? Status { get; set; }

        /// <summary>
        /// Ngày trong tuần để filter (optional)
        /// </summary>
        public ScheduleDay? ScheduleDay { get; set; }

        /// <summary>
        /// Có bao gồm slots không active không
        /// Mặc định là false
        /// </summary>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// Số trang (bắt đầu từ 1)
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// Số lượng items per page
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Từ khóa tìm kiếm (tìm trong tên template)
        /// </summary>
        public string? SearchKeyword { get; set; }

        /// <summary>
        /// Sắp xếp theo (TourDate, CreatedAt, etc.)
        /// Mặc định là TourDate
        /// </summary>
        public string SortBy { get; set; } = "TourDate";

        /// <summary>
        /// Thứ tự sắp xếp (asc, desc)
        /// Mặc định là asc
        /// </summary>
        public string SortOrder { get; set; } = "asc";
    }
}
