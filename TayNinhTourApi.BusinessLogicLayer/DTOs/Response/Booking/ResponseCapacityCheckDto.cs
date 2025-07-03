using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Booking
{
    /// <summary>
    /// DTO cho response kiểm tra capacity
    /// </summary>
    public class ResponseCapacityCheckDto : BaseResposeDto
    {
        /// <summary>
        /// ID của TourOperation
        /// </summary>
        public Guid TourOperationId { get; set; }

        /// <summary>
        /// Thông tin capacity hiện tại
        /// </summary>
        public CapacityInfoDto CapacityInfo { get; set; } = new CapacityInfoDto();

        /// <summary>
        /// Có thể booking với số lượng yêu cầu không
        /// </summary>
        public bool CanBook { get; set; }

        /// <summary>
        /// Số lượng khách tối đa có thể booking thêm
        /// </summary>
        public int MaxAdditionalGuests { get; set; }

        /// <summary>
        /// Thông báo cho user
        /// </summary>
        public string? UserMessage { get; set; }

        /// <summary>
        /// Thời gian kiểm tra
        /// </summary>
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}
