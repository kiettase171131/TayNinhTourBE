namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response lấy thông tin shop theo ID
    /// </summary>
    public class ResponseGetShopByIdDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin chi tiết của shop
        /// </summary>
        public ShopDto? Data { get; set; }
    }
}
