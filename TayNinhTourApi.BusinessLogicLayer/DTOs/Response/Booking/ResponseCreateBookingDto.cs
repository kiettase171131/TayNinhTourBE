using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Booking
{
    /// <summary>
    /// DTO cho response tạo booking
    /// </summary>
    public class ResponseCreateBookingDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin booking vừa tạo
        /// </summary>
        public ResponseBookingDto? BookingData { get; set; }

        /// <summary>
        /// Mã booking để khách hàng tra cứu
        /// </summary>
        public string? BookingCode { get; set; }

        /// <summary>
        /// Thông tin capacity còn lại sau khi booking
        /// </summary>
        public CapacityInfoDto? CapacityInfo { get; set; }
    }

    /// <summary>
    /// DTO thông tin capacity
    /// </summary>
    public class CapacityInfoDto
    {
        /// <summary>
        /// Số ghế tối đa
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Số ghế đã booking
        /// </summary>
        public int BookedCapacity { get; set; }

        /// <summary>
        /// Số ghế còn trống
        /// </summary>
        public int AvailableCapacity { get; set; }

        /// <summary>
        /// Phần trăm đã booking
        /// </summary>
        public decimal BookingPercentage { get; set; }

        /// <summary>
        /// Tour đã full chưa
        /// </summary>
        public bool IsFull => AvailableCapacity <= 0;
    }
}
