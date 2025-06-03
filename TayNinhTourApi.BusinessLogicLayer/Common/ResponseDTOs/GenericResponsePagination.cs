namespace TayNinhTourApi.BusinessLogicLayer.Common.ResponseDTOs
{
    public class GenericResponsePagination<T>
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public List<T>? Data { get; set; }
        public int? TotalPages { get; set; }
        public int? TotalRecord { get; set; }
        public int? TotalCount { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
