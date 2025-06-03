namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response lấy danh sách shops active (cho dropdown)
    /// </summary>
    public class ResponseGetActiveShopsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách shops active
        /// </summary>
        public List<ShopSummaryDto> Data { get; set; } = new List<ShopSummaryDto>();

        /// <summary>
        /// Tổng số shops active
        /// </summary>
        public int TotalCount { get; set; }
    }
}
