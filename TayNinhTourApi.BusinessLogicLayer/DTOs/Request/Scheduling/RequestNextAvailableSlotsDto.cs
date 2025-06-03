using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Scheduling
{
    /// <summary>
    /// DTO cho request lấy next available slots
    /// </summary>
    public class RequestNextAvailableSlotsDto
    {
        /// <summary>
        /// ID của tour template (null để lấy tất cả)
        /// </summary>
        public Guid? TourTemplateId { get; set; }

        /// <summary>
        /// Số lượng slots cần lấy
        /// </summary>
        [Range(1, 50, ErrorMessage = "Số lượng slots phải từ 1 đến 50")]
        public int Count { get; set; } = 10;

        /// <summary>
        /// Ngày bắt đầu tìm kiếm (mặc định từ hôm nay)
        /// </summary>
        public DateOnly? StartDate { get; set; }
    }
}
