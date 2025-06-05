using TayNinhTourApi.BusinessLogicLayer.DTOs;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation
{
    /// <summary>
    /// Response DTO cho việc tạo tour operation
    /// </summary>
    public class ResponseCreateOperationDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin operation vừa được tạo
        /// </summary>
        public TourOperationDto? Operation { get; set; }

        /// <summary>
        /// Thông tin lỗi validation nếu có
        /// </summary>
        public new string? ValidationErrors { get; set; }
    }
}
