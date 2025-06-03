using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot
{
    /// <summary>
    /// DTO cho response lấy danh sách upcoming tour slots
    /// </summary>
    public class ResponseGetUpcomingSlotsDto
    {
        /// <summary>
        /// Có thành công không
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Thông báo
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách upcoming slots
        /// </summary>
        public List<TourSlotDto> UpcomingSlots { get; set; } = new List<TourSlotDto>();

        /// <summary>
        /// Tổng số upcoming slots
        /// </summary>
        public int TotalUpcomingSlots { get; set; }

        /// <summary>
        /// Số slots được trả về
        /// </summary>
        public int ReturnedSlotsCount { get; set; }

        /// <summary>
        /// Ngày hiện tại (reference)
        /// </summary>
        public DateOnly CurrentDate { get; set; }
    }
}
