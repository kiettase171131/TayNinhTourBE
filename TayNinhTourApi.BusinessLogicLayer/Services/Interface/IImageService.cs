using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Image;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Image;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IImageService
    {
        Task<ResponseImageUploadDto> UploadImage(List<RequestImageUploadDto> imageDtos, string localRootPath, string urlPath);
    }
}
