using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface ICmsService
    {
        Task<BaseResposeDto> DeleteUserAsync(Guid id);
        Task<ResponseGetTourDto> GetTourByIdAsync(Guid id);
        Task<ResponseGetToursDto> GetToursAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status);
        Task<ResponseGetUsersCmsDto> GetUserAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status);
        Task<ResponseGetUserByIdCmsDto> GetUserByIdAsync(Guid id);
        Task<BaseResposeDto> UpdateTourAsync(RequestUpdateTourCmsDto request, Guid id, Guid updatedBy);
        Task<ResponseGetBlogsDto> GetBlogsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status);
        Task<BaseResposeDto> UpdateBlogAsync(RequestUpdateBlogCmsDto request, Guid id, Guid updatedById);
        Task<BaseResposeDto> UpdateUserAsync(RequestUpdateUserCmsDto request, Guid id);
        Task<BaseResposeDto> CreateUserAsync(RequestCreateUserDto request);
        Task<ResponseGetSpecialtyShopsDto> GetSpecialtyShopsAsync(int? pageIndex,int? pageSize,string? textSearch,bool? isActive);
        Task<ResponseGetTourGuidesDto> GetTourGuidesAsync(int? pageIndex,int? pageSize,string? textSearch,bool? isAvailable,bool? isActive);
        Task<decimal> GetTourDiscountPercentAsync();
        Task UpdateTourDiscountPercentAsync(decimal newPercent);
    }
}
