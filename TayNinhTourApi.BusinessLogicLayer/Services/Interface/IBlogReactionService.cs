using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IBlogReactionService
    {
        Task<ResponseBlogReactionDto> ToggleReactionAsync(RequestBlogReactionDto request, CurrentUserObject currentUserObject);
    }
}
