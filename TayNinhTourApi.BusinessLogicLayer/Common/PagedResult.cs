namespace TayNinhTourApi.BusinessLogicLayer.Common
{
    /// <summary>
    /// Generic class cho kết quả phân trang
    /// </summary>
    /// <typeparam name="T">Type của data items</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Danh sách items trong trang hiện tại
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Tổng số items trong toàn bộ dataset
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Trang hiện tại (1-based)
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Số lượng items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Có trang trước không
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// Có trang sau không
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;

        /// <summary>
        /// Constructor mặc định
        /// </summary>
        public PagedResult()
        {
        }

        /// <summary>
        /// Constructor với parameters
        /// </summary>
        /// <param name="items">Danh sách items</param>
        /// <param name="totalCount">Tổng số items</param>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Kích thước trang</param>
        public PagedResult(List<T> items, int totalCount, int pageIndex, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        /// <summary>
        /// Tạo PagedResult từ danh sách items và thông tin phân trang
        /// </summary>
        /// <param name="items">Danh sách items</param>
        /// <param name="totalCount">Tổng số items</param>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>PagedResult instance</returns>
        public static PagedResult<T> Create(List<T> items, int totalCount, int pageIndex, int pageSize)
        {
            return new PagedResult<T>(items, totalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Tạo PagedResult rỗng
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>PagedResult rỗng</returns>
        public static PagedResult<T> Empty(int pageIndex, int pageSize)
        {
            return new PagedResult<T>(new List<T>(), 0, pageIndex, pageSize);
        }
    }
}
