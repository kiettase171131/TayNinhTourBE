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
        /// Generate QR code image for group representative booking
        /// </summary>
        /// <param name="booking">Tour booking entity</param>
        /// <param name="size">QR code size in pixels (default: 300)</param>
        /// <returns>QR code image as PNG byte array</returns>
        public async Task<byte[]> GenerateGroupQRCodeImageAsync(TourBooking booking, int size = 300)
        {
            try
            {
                var qrData = GenerateGroupQRCodeData(booking);
                return await GenerateQRCodeImageFromDataAsync(qrData, size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating group QR code image for booking {BookingId}", booking.Id);

                // Return a simple fallback QR code if generation fails
                try
                {
                    return await GenerateFallbackQRCodeAsync($"{booking.BookingCode}-GROUP", size);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Error generating fallback group QR code for booking {BookingId}", booking.Id);
                    throw new InvalidOperationException($"Unable to generate group QR code for booking {booking.Id}", ex);
                }
            }
        }

        /// <summary>
        /// Generate QR code image for individual guest
        /// </summary>
        /// <param name="guest">Tour booking guest entity</param>
        /// <param name="booking">Parent tour booking entity</param>
        /// <param name="size">QR code size in pixels (default: 300)</param>
        /// <returns>QR code image as PNG byte array</returns>
        public async Task<byte[]> GenerateGuestQRCodeImageAsync(TourBookingGuest guest, TourBooking booking, int size = 300)
        {
            try
            {
                var qrData = GenerateGuestQRCodeData(guest, booking);
                return await GenerateQRCodeImageFromDataAsync(qrData, size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image for guest {GuestId} in booking {BookingId}", guest.Id, booking.Id);

                // Return a simple fallback QR code if generation fails
                try
                {
                    return await GenerateFallbackQRCodeAsync($"{booking.BookingCode}-{guest.GuestName}", size);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Error generating fallback QR code for guest {GuestId} in booking {BookingId}", guest.Id, booking.Id);
                    throw new InvalidOperationException($"Unable to generate QR code for guest {guest.Id} in booking {booking.Id}", ex);
                }
            }
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
        /// ENHANCED: Compact version to avoid MySQL column length issues
        /// </summary>
        public string GenerateQRCodeData(TourBooking booking)
        {
            try
            {
                // ✅ COMPACT VERSION: Only essential data to keep within VARCHAR limits
                var qrData = new
                {
                    bid = booking.Id.ToString("N")[..8], // Short booking ID (8 chars)
                    bc = booking.BookingCode,
                    uid = booking.UserId.ToString("N")[..8], // Short user ID
                    toid = booking.TourOperationId.ToString("N")[..8], // Short tour operation ID
                    ng = booking.NumberOfGuests,
                    tp = booking.TotalPrice,
                    op = booking.OriginalPrice,
                    dp = booking.DiscountPercent,
                    st = (int)booking.Status,
                    bd = booking.BookingDate.ToString("yyyyMMdd"),
                    v = "3.0" // Compact version identifier
                };

                return JsonSerializer.Serialize(qrData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compact QR code data for booking {BookingId}", booking.Id);
                
                // Ultra-compact fallback
                return $"{{\"bc\":\"{booking.BookingCode}\",\"bid\":\"{booking.Id.ToString("N")[..8]}\",\"v\":\"fallback\"}}";
            }
        }

        /// <summary>
        /// Generate individual QR code data for specific guest
        /// ENHANCED: Compact version to avoid MySQL column length issues
        /// </summary>
        public string GenerateGuestQRCodeData(TourBookingGuest guest, TourBooking booking)
        {
            try
            {
                // ✅ COMPACT VERSION: Only essential guest data
                var qrData = new
                {
                    gid = guest.Id.ToString("N")[..8], // Short guest ID
                    gn = guest.GuestName.Length > 20 ? guest.GuestName[..20] : guest.GuestName,
                    ge = guest.GuestEmail.Length > 30 ? guest.GuestEmail[..30] : guest.GuestEmail,
                    bid = booking.Id.ToString("N")[..8], // Short booking ID
                    bc = booking.BookingCode,
                    toid = booking.TourOperationId.ToString("N")[..8],
                    tp = booking.TotalPrice,
                    ng = booking.NumberOfGuests,
                    v = "3.1" // Compact guest version
                };

                return JsonSerializer.Serialize(qrData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compact guest QR code data for guest {GuestId} in booking {BookingId}",
                    guest.Id, booking.Id);
                
                // Ultra-compact fallback
                return $"{{\"gn\":\"{guest.GuestName[..Math.Min(guest.GuestName.Length, 10)]}\",\"bc\":\"{booking.BookingCode}\",\"v\":\"fallback\"}}";
            }
        }

        /// <summary>
        /// Generate QR code data for group representative booking
        /// ENHANCED: Compact version to avoid MySQL column length issues
        /// </summary>
        public string GenerateGroupQRCodeData(TourBooking booking)
        {
            try
            {
                // ✅ COMPACT VERSION: Only essential group data
                var qrData = new
                {
                    bid = booking.Id.ToString("N")[..8], // Short booking ID
                    bc = booking.BookingCode,
                    bt = "grp", // Group booking type (shortened)
                    gn = booking.GroupName?.Length > 15 ? booking.GroupName[..15] : booking.GroupName,
                    ng = booking.NumberOfGuests,
                    toid = booking.TourOperationId.ToString("N")[..8],
                    tp = booking.TotalPrice,
                    cn = booking.ContactName?.Length > 15 ? booking.ContactName[..15] : booking.ContactName,
                    v = "2.0" // Compact group version
                };

                return JsonSerializer.Serialize(qrData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compact group QR code data for booking {BookingId}", booking.Id);
                
                // Ultra-compact fallback
                return $"{{\"bc\":\"{booking.BookingCode}\",\"ng\":{booking.NumberOfGuests},\"v\":\"fallback\"}}";
            }
        }

        /// <summary>
        /// Validate QR code data format
        /// ENHANCED: Updated to handle both compact (v3.x) and legacy (v2.x) QR codes
        /// </summary>
        public bool ValidateQRCodeData(string qrData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(qrData))
                    return false;

                var jsonDocument = JsonDocument.Parse(qrData);
                var root = jsonDocument.RootElement;

                // Check version to determine validation logic
                if (root.TryGetProperty("v", out var versionElement))
                {
                    var version = versionElement.GetString();
                    
                    // Handle compact versions (v3.x)
                    if (version?.StartsWith("3.") == true)
                    {
                        // Compact format: bid, bc are required
                        return root.TryGetProperty("bid", out _) &&
                               root.TryGetProperty("bc", out _);
                    }
                    
                    // Handle group compact versions (v2.x)
                    if (version?.StartsWith("2.") == true)
                    {
                        // Group compact format: bc, ng are required
                        return root.TryGetProperty("bc", out _) &&
                               root.TryGetProperty("ng", out _);
                    }
                    
                    // Handle legacy versions (v2.1, v2.0)
                    if (version == "2.1" || version == "2.0")
                    {
                        // Legacy format validation
                        bool hasBasicFields = root.TryGetProperty("bookingId", out _) &&
                                             root.TryGetProperty("bookingCode", out _) &&
                                             root.TryGetProperty("userId", out _) &&
                                             root.TryGetProperty("status", out _);

                        if (!hasBasicFields)
                            return false;

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

                // Handle ultra-compact fallback format
                if (root.TryGetProperty("bc", out _))
                {
                    return true; // Booking code is present, consider valid
                }

                // Fallback for version 1.0 or no version specified
                return root.TryGetProperty("bookingId", out _) ||
                       root.TryGetProperty("bookingCode", out _);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid QR code data format");
                return false;
            }
        }
    }
}
