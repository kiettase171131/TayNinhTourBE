using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Shop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Shop;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IShopApplicationService
    {
        Task<IEnumerable<ShopApplication>> GetPendingAsync();
        Task<IEnumerable<ShopApplication>> ListByUserAsync(Guid userId);
        Task<ResponseShopSubmitDto> SubmitAsync(RequestShopSubmitDto requestShopSubmit, CurrentUserObject currentUserObject);
        Task<BaseResposeDto> ApproveAsync(Guid applicationId);
        Task<BaseResposeDto> RejectAsync(Guid applicationId, string reason);
    }
}
