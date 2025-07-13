using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking
{
    /// <summary>
    /// Request DTO để tính giá tour trước khi booking
    /// </summary>
    public class CalculatePriceRequest
    {
        /// <summary>
        /// ID của TourOperation
        /// </summary>
        [Required(ErrorMessage = "TourOperation ID là bắt buộc")]
        public Guid TourOperationId { get; set; }

        /// <summary>
        /// Số lượng khách
        /// </summary>
        [Required(ErrorMessage = "Số lượng khách là bắt buộc")]
        [Range(1, 50, ErrorMessage = "Số lượng khách phải từ 1 đến 50")]
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// Ngày đặt tour (optional, mặc định là ngày hiện tại)
        /// Dùng để test pricing logic với ngày khác nhau
        /// </summary>
        public DateTime? BookingDate { get; set; }
    }
}
