using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot
{
    /// <summary>
    /// DTO cho response lấy danh sách tour slots với pagination
    /// </summary>
    public class ResponseGetSlotsDto
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
        /// Danh sách tour slots
        /// </summary>
        public List<TourSlotDto> Slots { get; set; } = new List<TourSlotDto>();

        /// <summary>
        /// Thông tin pagination
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();

        /// <summary>
        /// Thông tin filter đã áp dụng
        /// </summary>
        public FilterInfo AppliedFilters { get; set; } = new FilterInfo();
    }

    /// <summary>
    /// Thông tin pagination
    /// </summary>
    public class PaginationInfo
    {
        /// <summary>
        /// Trang hiện tại
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Số items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Tổng số items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Có trang trước không
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Có trang sau không
        /// </summary>
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// Thông tin filter đã áp dụng
    /// </summary>
    public class FilterInfo
    {
        /// <summary>
        /// Tour Template ID filter
        /// </summary>
        public Guid? TourTemplateId { get; set; }

        /// <summary>
        /// From Date filter
        /// </summary>
        public DateOnly? FromDate { get; set; }

        /// <summary>
        /// To Date filter
        /// </summary>
        public DateOnly? ToDate { get; set; }

        /// <summary>
        /// Status filter
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Schedule Day filter
        /// </summary>
        public string? ScheduleDay { get; set; }

        /// <summary>
        /// Search keyword
        /// </summary>
        public string? SearchKeyword { get; set; }
    }
}
