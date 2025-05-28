using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface ITourCompanyService
    {
        Task<ResponseGetTourDto> GetTourByIdAsync(Guid id);
        Task<ResponseGetToursDto> GetToursAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status);
        Task<BaseResposeDto> UpdateTourAsync(RequestUpdateTourDto request, Guid id, Guid updatedBy);
        Task<BaseResposeDto> CreateTourAsync(RequestCreateTourCmsDto request, Guid createdBy);
        Task<BaseResposeDto> DeleteTourAsync(Guid id);
    }
}
