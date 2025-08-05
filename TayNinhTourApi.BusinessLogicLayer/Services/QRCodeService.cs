using QRCoder;
using SkiaSharp;
using System.Text.Json;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using Microsoft.Extensions.Logging;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service for generating QR codes for tour bookings
    /// </summary>
    public class QRCodeService : IQRCodeService
    {
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(ILogger<QRCodeService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generate QR code image as byte array from tour booking data
        /// </summary>
        public async Task<byte[]> GenerateQRCodeImageAsync(TourBooking booking, int size = 300)
        {
            try
            {
                var qrData = GenerateQRCodeData(booking);
                return await GenerateQRCodeImageFromDataAsync(qrData, size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image for booking {BookingId}", booking.Id);
                throw;
            }
        }

        /// <summary>
        /// Generate QR code image as byte array from JSON data string
        /// </summary>
        public Task<byte[]> GenerateQRCodeImageFromDataAsync(string qrData, int size = 300)
        {
            try
            {
                // Generate QR code using QRCoder
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.M);

                // Use BitmapByteQRCode to get byte array directly
                var qrCode = new BitmapByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);

                // Convert byte array to SKBitmap for resizing
                using var originalBitmap = SKBitmap.Decode(qrCodeBytes);

                // Resize to desired size
                using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(size, size), SKFilterQuality.High);

                // Convert to PNG byte array
                using var image = SKImage.FromBitmap(resizedBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                return Task.FromResult(data.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image from data");
                throw;
            }
        }

        /// <summary>
        /// Generate QR code data JSON string from tour booking
        /// ENHANCED: Include both original and final pricing information
        /// </summary>
        public string GenerateQRCodeData(TourBooking booking)
        {
            try
            {
                var qrData = new
                {
                    BookingId = booking.Id,
                    BookingCode = booking.BookingCode,
                    UserId = booking.UserId,
                    TourOperationId = booking.TourOperationId,
                    NumberOfGuests = booking.NumberOfGuests,
                    
                    // PRICING INFORMATION - Enhanced to show both original and final prices
                    OriginalPrice = booking.OriginalPrice, // Giá g?c (tr??c discount)
                    DiscountPercent = booking.DiscountPercent, // % gi?m giá
                    TotalPrice = booking.TotalPrice, // Giá cu?i cùng (sau discount)
                    PriceType = booking.DiscountPercent > 0 ? "Early Bird" : "Standard",
                    
                    BookingDate = booking.BookingDate,
                    ConfirmedDate = booking.ConfirmedDate,
                    Status = booking.Status.ToString(),
                    ContactName = booking.ContactName,
                    ContactPhone = booking.ContactPhone,
                    ContactEmail = booking.ContactEmail,
                    
                    // Add tour information if available
                    TourTitle = booking.TourOperation?.TourDetails?.Title,
                    TourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue),
                    
                    // Add verification timestamp
                    GeneratedAt = DateTime.UtcNow,
                    Version = "2.0" // Updated version to reflect enhanced pricing info
                };

                return JsonSerializer.Serialize(qrData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code data for booking {BookingId}", booking.Id);
                throw;
            }
        }

        /// <summary>
        /// Validate QR code data format
        /// ENHANCED: Updated to handle version 2.0 QR codes with pricing information
        /// </summary>
        public bool ValidateQRCodeData(string qrData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(qrData))
                    return false;

                var jsonDocument = JsonDocument.Parse(qrData);
                var root = jsonDocument.RootElement;

                // Check required fields for all versions
                bool hasBasicFields = root.TryGetProperty("bookingId", out _) &&
                                     root.TryGetProperty("bookingCode", out _) &&
                                     root.TryGetProperty("userId", out _) &&
                                     root.TryGetProperty("status", out _);

                if (!hasBasicFields)
                    return false;

                // Check version-specific fields
                if (root.TryGetProperty("version", out var versionElement))
                {
                    var version = versionElement.GetString();
                    if (version == "2.0")
                    {
                        // Version 2.0 should have enhanced pricing information
                        return root.TryGetProperty("originalPrice", out _) &&
                               root.TryGetProperty("totalPrice", out _) &&
                               root.TryGetProperty("discountPercent", out _) &&
                               root.TryGetProperty("priceType", out _);
                    }
                }

                // Fallback for version 1.0 or no version specified
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid QR code data format");
                return false;
            }
        }
    }
}
