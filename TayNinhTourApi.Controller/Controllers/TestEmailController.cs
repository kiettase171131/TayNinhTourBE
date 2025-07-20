using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using Microsoft.EntityFrameworkCore;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Test controller for email notification functionality
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestEmailController : ControllerBase
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly EmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(
            IQRCodeService qrCodeService,
            EmailSender emailSender,
            IUnitOfWork unitOfWork,
            ILogger<TestEmailController> logger)
        {
            _qrCodeService = qrCodeService;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Test QR code generation
        /// </summary>
        [HttpGet("test-qr-generation")]
        public async Task<IActionResult> TestQRGeneration()
        {
            try
            {
                // Create test data
                var testData = "{'bookingId':'test-123','bookingCode':'TEST001','status':'Confirmed'}";
                
                // Generate QR code
                var qrCodeBytes = await _qrCodeService.GenerateQRCodeImageFromDataAsync(testData, 300);
                
                return File(qrCodeBytes, "image/png", "test-qr-code.png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing QR code generation");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Test email sending with QR code using a real booking
        /// </summary>
        [HttpPost("test-booking-email/{bookingId}")]
        public async Task<IActionResult> TestBookingEmail(Guid bookingId, [FromQuery] string? testEmail = null)
        {
            try
            {
                // Get booking from database using GetByIdAsync with includes
                var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(bookingId,
                    new[] { "TourOperation", "TourOperation.TourDetails", "TourSlot" });

                if (booking == null)
                {
                    return NotFound("Booking not found");
                }

                // Generate QR code
                var qrCodeImage = await _qrCodeService.GenerateQRCodeImageAsync(booking, 300);

                // Prepare email data
                var customerName = booking.ContactName ?? "Test Customer";
                var tourTitle = booking.TourOperation?.TourDetails?.Title ?? "Test Tour";
                var tourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
                var emailAddress = testEmail ?? booking.ContactEmail ?? "test@example.com";

                // Send email
                await _emailSender.SendTourBookingConfirmationAsync(
                    toEmail: emailAddress,
                    customerName: customerName,
                    bookingCode: booking.BookingCode,
                    tourTitle: tourTitle,
                    tourDate: tourDate,
                    numberOfGuests: booking.NumberOfGuests,
                    totalPrice: booking.TotalPrice,
                    contactPhone: booking.ContactPhone ?? "N/A",
                    qrCodeImage: qrCodeImage
                );

                return Ok(new
                {
                    message = "Test email sent successfully",
                    bookingId = booking.Id,
                    bookingCode = booking.BookingCode,
                    emailSentTo = emailAddress,
                    tourTitle = tourTitle,
                    tourDate = tourDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing booking email for booking {BookingId}", bookingId);
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Test email sending with mock data
        /// </summary>
        [HttpPost("test-mock-email")]
        public async Task<IActionResult> TestMockEmail([FromQuery] string email = "test@example.com")
        {
            try
            {
                // Create mock QR code data
                var mockQRData = @"{
                    ""bookingId"": ""12345678-1234-1234-1234-123456789012"",
                    ""bookingCode"": ""TNDT240101001"",
                    ""userId"": ""87654321-4321-4321-4321-210987654321"",
                    ""tourOperationId"": ""11111111-1111-1111-1111-111111111111"",
                    ""numberOfGuests"": 2,
                    ""totalPrice"": 1500000,
                    ""bookingDate"": ""2024-01-01T10:00:00Z"",
                    ""status"": ""Confirmed"",
                    ""tourTitle"": ""Tay Ninh Cultural Heritage Tour"",
                    ""tourDate"": ""2024-01-15T08:00:00Z""
                }";

                // Generate QR code
                var qrCodeImage = await _qrCodeService.GenerateQRCodeImageFromDataAsync(mockQRData, 300);

                // Send test email
                await _emailSender.SendTourBookingConfirmationAsync(
                    toEmail: email,
                    customerName: "Test Customer",
                    bookingCode: "TNDT240101001",
                    tourTitle: "Tay Ninh Cultural Heritage Tour",
                    tourDate: new DateTime(2024, 1, 15, 8, 0, 0),
                    numberOfGuests: 2,
                    totalPrice: 1500000,
                    contactPhone: "+84 123 456 789",
                    qrCodeImage: qrCodeImage
                );

                return Ok(new
                {
                    message = "Mock test email sent successfully",
                    emailSentTo = email,
                    qrDataLength = mockQRData.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing mock email");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get list of recent bookings for testing
        /// </summary>
        [HttpGet("recent-bookings")]
        public async Task<IActionResult> GetRecentBookings()
        {
            try
            {
                // Use GetBookingsWithPaginationAsync method from repository
                var recentBookings = await _unitOfWork.TourBookingRepository.GetBookingsWithPaginationAsync(
                    pageIndex: 0,
                    pageSize: 10);

                var result = recentBookings.Select(b => new
                {
                    b.Id,
                    b.BookingCode,
                    b.Status,
                    b.ContactName,
                    b.ContactEmail,
                    b.TotalPrice,
                    TourTitle = b.TourOperation?.TourDetails?.Title ?? "N/A",
                    b.CreatedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent bookings");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
