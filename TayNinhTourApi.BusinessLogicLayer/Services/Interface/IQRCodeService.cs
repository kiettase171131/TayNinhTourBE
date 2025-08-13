using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface for QR code generation and management
    /// </summary>
    public interface IQRCodeService
    {
        /// <summary>
        /// Generate QR code image as byte array from tour booking data
        /// </summary>
        /// <param name="booking">Tour booking entity</param>
        /// <param name="size">QR code size in pixels (default: 300)</param>
        /// <returns>QR code image as PNG byte array</returns>
        Task<byte[]> GenerateQRCodeImageAsync(TourBooking booking, int size = 300);

        /// <summary>
        /// Generate QR code image as byte array from JSON data string
        /// </summary>
        /// <param name="qrData">JSON data string</param>
        /// <param name="size">QR code size in pixels (default: 300)</param>
        /// <returns>QR code image as PNG byte array</returns>
        Task<byte[]> GenerateQRCodeImageFromDataAsync(string qrData, int size = 300);

        /// <summary>
        /// Generate QR code data JSON string from tour booking
        /// </summary>
        /// <param name="booking">Tour booking entity</param>
        /// <returns>JSON string containing booking information</returns>
        string GenerateQRCodeData(TourBooking booking);

        /// <summary>
        /// Generate individual QR code data for specific guest
        /// </summary>
        /// <param name="guest">Tour booking guest entity</param>
        /// <param name="booking">Parent tour booking entity</param>
        /// <returns>JSON string containing guest-specific information</returns>
        string GenerateGuestQRCodeData(TourBookingGuest guest, TourBooking booking);

        /// <summary>
        /// Generate QR code image for individual guest
        /// </summary>
        /// <param name="guest">Tour booking guest entity</param>
        /// <param name="booking">Parent tour booking entity</param>
        /// <param name="size">QR code size in pixels (default: 300)</param>
        /// <returns>QR code image as PNG byte array</returns>
        Task<byte[]> GenerateGuestQRCodeImageAsync(TourBookingGuest guest, TourBooking booking, int size = 300);

        /// <summary>
        /// Validate QR code data format
        /// </summary>
        /// <param name="qrData">QR code data to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidateQRCodeData(string qrData);
    }
}
