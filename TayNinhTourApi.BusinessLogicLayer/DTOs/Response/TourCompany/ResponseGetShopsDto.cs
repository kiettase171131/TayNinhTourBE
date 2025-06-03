using TayNinhTourApi.BusinessLogicLayer.Common.ResponseDTOs;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response danh sách shops với pagination
    /// </summary>
    public class ResponseGetShopsDto : GenericResponsePagination<ShopDto>
    {
    }
}
