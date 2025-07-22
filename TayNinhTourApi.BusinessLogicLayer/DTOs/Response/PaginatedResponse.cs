namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response
{
    /// <summary>
    /// Generic paginated response DTO
    /// </summary>
    /// <typeparam name="T">Type of data items</typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// List of data items
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Create paginated response from data
        /// </summary>
        /// <param name="items">Data items</param>
        /// <param name="totalCount">Total count</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated response</returns>
        public static PaginatedResponse<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            return new PaginatedResponse<T>
            {
                Items = items.ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }
    }
}
