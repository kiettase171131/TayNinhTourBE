using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// Response DTO cho việc lấy danh sách tour details
    /// </summary>
    public class ResponseGetTourDetailsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách tour details
        /// </summary>
        public List<TourDetailDto> Data { get; set; } = new List<TourDetailDto>();

        /// <summary>
        /// Tổng số tour details
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc lấy một tour detail
    /// </summary>
    public class ResponseGetTourDetailDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin tour detail
        /// </summary>
        public TourDetailDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc tìm kiếm tour details
    /// </summary>
    public class ResponseSearchTourDetailsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách tour details tìm được
        /// </summary>
        public List<TourDetailDto> Data { get; set; } = new List<TourDetailDto>();

        /// <summary>
        /// Tổng số kết quả
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Từ khóa tìm kiếm
        /// </summary>
        public string SearchKeyword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO cho việc lấy tour details với pagination
    /// </summary>
    public class ResponseGetTourDetailsPaginatedDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách tour details
        /// </summary>
        public List<TourDetailDto> Data { get; set; } = new List<TourDetailDto>();

        /// <summary>
        /// Tổng số records
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Trang hiện tại
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Số items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int TotalPages { get; set; }
    }
}
