using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Application;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface ITourGuideApplicationService
    {
        Task<ResponseApplicationDto> SubmitAsync(SubmitApplicationDto submitApplicationDto, CurrentUserObject currentUserObject);
        Task<IEnumerable<TourGuideApplication>> GetPendingAsync();
        Task<BaseResposeDto> ApproveAsync(Guid applicationId);
        Task<BaseResposeDto> RejectAsync(Guid applicationId, string reason);
        Task<IEnumerable<TourGuideApplication>> ListByUserAsync(Guid userId);
        
    }
}
