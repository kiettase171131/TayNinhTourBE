namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response tạo shop mới
    /// </summary>
    public class ResponseCreateShopDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin shop vừa được tạo
        /// </summary>
        public ShopDto? Data { get; set; }
    }
}
