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

        /// <summary>
        /// Lấy danh sách feedback của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số lượng items per page</param>
        /// <returns>Danh sách feedback của user</returns>
        Task<TourFeedbackResponse> GetUserFeedbacksAsync(Guid userId, int pageIndex = 1, int pageSize = 10);

        /// <summary>
        /// Cập nhật feedback của user
        /// </summary>
        /// <param name="feedbackId">ID của feedback</param>
        /// <param name="userId">ID của user</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Feedback sau khi cập nhật</returns>
        Task<ResponseCreateFeedBackDto> UpdateFeedbackAsync(Guid feedbackId, Guid userId, UpdateTourFeedbackRequest request);

        /// <summary>
        /// Xóa feedback của user
        /// </summary>
        /// <param name="feedbackId">ID của feedback</param>
        /// <param name="userId">ID của user</param>
        /// <returns>Kết quả xóa</returns>
        Task<ResponseCreateFeedBackDto> DeleteFeedbackAsync(Guid feedbackId, Guid userId);
    }
}
