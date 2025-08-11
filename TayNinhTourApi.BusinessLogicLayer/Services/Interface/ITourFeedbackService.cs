using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourFeedback;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourFeedback;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface ITourFeedbackService
    {
        Task<ResponseCreateFeedBackDto> CreateAsync(Guid userId, CreateTourFeedbackRequest req);
        Task<TourFeedbackResponse> GetTourFeedbacksBySlotAsync(Guid slotId, int? pageIndex, int? pageSize, int? minTourRating = null, int? maxTourRating = null, bool? onlyWithGuideRating = null);
       
    }
}
