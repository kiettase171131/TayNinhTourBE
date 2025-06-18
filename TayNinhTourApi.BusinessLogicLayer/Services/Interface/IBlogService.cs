using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IBlogService
    {
        Task<ResponseGetBlogByIdDto> GetBlogByIdAsync(Guid id, CurrentUserObject currentUserObject);
        Task<ResponseGetBlogsDto> GetBlogsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status, CurrentUserObject currentUserObject);
        Task<ResponseGetBlogsDto> GetAcceptedBlogsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status, CurrentUserObject currentUserObject);
        Task<BaseResposeDto> UpdateBlogAsync(RequestUpdateBlogDto request, Guid id, CurrentUserObject currentUserObject);
        Task<ResponseCreateBlogDto> CreateBlogAsync(RequestCreateBlogDto request, CurrentUserObject currentUserObject);
        Task<BaseResposeDto> DeleteBlogAsync(Guid id);
    }
}
