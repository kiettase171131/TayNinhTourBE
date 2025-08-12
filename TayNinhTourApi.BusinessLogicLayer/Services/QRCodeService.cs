using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using Microsoft.Extensions.Logging;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service for generating QR codes for tour bookings
    /// FIXED: Replaced SkiaSharp with System.Drawing to avoid server deployment issues
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

                // Return a simple fallback QR code if generation fails
                try
                {
                    return await GenerateFallbackQRCodeAsync(booking.BookingCode, size);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Error generating fallback QR code for booking {BookingId}", booking.Id);
                    throw new InvalidOperationException($"Unable to generate QR code for booking {booking.Id}", ex);
                }
            }
        }

        /// <summary>
        /// Generate QR code image as byte array from JSON data string
        /// FIXED: Uses System.Drawing instead of SkiaSharp for better server compatibility
        /// </summary>
        public Task<byte[]> GenerateQRCodeImageFromDataAsync(string qrData, int size = 300)
        {
            try
            {
                _logger.LogInformation("Generating QR code with size {Size}", size);

                // Generate QR code using QRCoder
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.M);

                // Use PngByteQRCode for direct PNG generation (more compatible)
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);

                _logger.LogInformation("Successfully generated QR code image with {ByteCount} bytes", qrCodeBytes.Length);

                // If size is different from default, we could resize here
                // But for compatibility, we'll return the generated size
                return Task.FromResult(qrCodeBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image from data");

                // Try alternative method using BitmapByteQRCode
                try
                {
                    _logger.LogInformation("Attempting alternative QR code generation method");

                    using var qrGenerator = new QRCodeGenerator();
                    using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.M);

                    var qrCode = new BitmapByteQRCode(qrCodeData);
                    var qrCodeBytes = qrCode.GetGraphic(20);

                    _logger.LogInformation("Alternative QR code generation successful with {ByteCount} bytes", qrCodeBytes.Length);

                    return Task.FromResult(qrCodeBytes);
                }
                catch (Exception altEx)
                {
                    _logger.LogError(altEx, "Alternative QR code generation also failed");
                    throw new InvalidOperationException("Unable to generate QR code using any available method", ex);
                }
            }
        }

        /// <summary>
        /// Generate a simple fallback QR code with just booking code
        /// Used when main QR generation fails
        /// </summary>
        private async Task<byte[]> GenerateFallbackQRCodeAsync(string bookingCode, int size = 300)
        {
            try
            {
                _logger.LogWarning("Generating fallback QR code for booking code: {BookingCode}", bookingCode);

                var fallbackData = new
                {
                    BookingCode = bookingCode,
                    Type = "Fallback",
                    GeneratedAt = DateTime.UtcNow
                };

                var fallbackJson = JsonSerializer.Serialize(fallbackData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(fallbackJson, QRCodeGenerator.ECCLevel.M);

                var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);

                _logger.LogInformation("Fallback QR code generated successfully with {ByteCount} bytes", qrCodeBytes.Length);

                return qrCodeBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate fallback QR code");
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
                    OriginalPrice = booking.OriginalPrice, // Gi� g?c (tr??c discount)
                    DiscountPercent = booking.DiscountPercent, // % gi?m gi�
                    TotalPrice = booking.TotalPrice, // Gi� cu?i c�ng (sau discount)
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
                    Version = "2.1" // Updated version to reflect server-compatible implementation
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
        /// Generate individual QR code data for specific guest
        /// Contains guest-specific info + booking context for tour guide check-in
        /// </summary>
        public string GenerateGuestQRCodeData(TourBookingGuest guest, TourBooking booking)
        {
            try
            {
                var qrData = new
                {
                    // Guest-specific information
                    GuestId = guest.Id,
                    GuestName = guest.GuestName,
                    GuestEmail = guest.GuestEmail,
                    GuestPhone = guest.GuestPhone,

                    // Booking context
                    BookingId = booking.Id,
                    BookingCode = booking.BookingCode,
                    TourOperationId = booking.TourOperationId,
                    TourSlotId = booking.TourSlotId,

                    // Pricing information (shared across all guests)
                    TotalBookingPrice = booking.TotalPrice,
                    NumberOfGuests = booking.NumberOfGuests,
                    OriginalPrice = booking.OriginalPrice,
                    DiscountPercent = booking.DiscountPercent,

                    // Tour information
                    TourTitle = booking.TourOperation?.TourDetails?.Title ?? "Tour Experience",
                    TourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow,

                    // Check-in status
                    IsCheckedIn = guest.IsCheckedIn,
                    CheckInTime = guest.CheckInTime,

                    // Metadata
                    GeneratedAt = DateTime.UtcNow,
                    QRType = "IndividualGuest",
                    Version = "3.0" // New version for individual guest QR codes
                };

                return JsonSerializer.Serialize(qrData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating guest QR code data for guest {GuestId} in booking {BookingId}",
                    guest.Id, booking.Id);
                throw;
            }
        }

        /// <summary>
        /// Generate QR code image for individual guest
        /// </summary>
        public async Task<byte[]> GenerateGuestQRCodeImageAsync(TourBookingGuest guest, TourBooking booking, int size = 300)
        {
            try
            {
                var qrData = GenerateGuestQRCodeData(guest, booking);
                return await GenerateQRCodeImageFromDataAsync(qrData, size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating guest QR code image for guest {GuestId} in booking {BookingId}",
                    guest.Id, booking.Id);

                // Fallback: generate simple QR with guest name and booking code
                try
                {
                    var fallbackData = $"Guest: {guest.GuestName}, Booking: {booking.BookingCode}, ID: {guest.Id}";
                    return await GenerateQRCodeImageFromDataAsync(fallbackData, size);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Error generating fallback guest QR code for guest {GuestId}", guest.Id);
                    throw new InvalidOperationException($"Unable to generate QR code for guest {guest.Id}", ex);
                }
            }
        }

        /// <summary>
        /// Validate QR code data format
        /// ENHANCED: Updated to handle version 2.1 QR codes with server-compatible generation
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
                    if (version == "2.1" || version == "2.0")
                    {
                        // Version 2.x should have enhanced pricing information
                        return root.TryGetProperty("originalPrice", out _) &&
                               root.TryGetProperty("totalPrice", out _) &&
                               root.TryGetProperty("discountPercent", out _) &&
                               root.TryGetProperty("priceType", out _);
                    }
                }

                // Handle fallback QR codes
                if (root.TryGetProperty("type", out var typeElement))
                {
                    var type = typeElement.GetString();
                    if (type == "Fallback")
                    {
                        return root.TryGetProperty("bookingCode", out _);
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
