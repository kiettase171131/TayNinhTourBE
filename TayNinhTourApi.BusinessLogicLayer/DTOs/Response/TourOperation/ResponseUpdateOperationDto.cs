using TayNinhTourApi.BusinessLogicLayer.DTOs;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation
{
    /// <summary>
    /// Response DTO cho việc cập nhật tour operation
    /// </summary>
    public class ResponseUpdateOperationDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin operation sau khi cập nhật
        /// </summary>
        public TourOperationDto? Operation { get; set; }

        /// <summary>
        /// Thông tin lỗi validation nếu có
        /// </summary>
        public new string? ValidationErrors { get; set; }
    }
}
