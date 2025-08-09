using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho TourSlot
    /// </summary>
    public class TourSlotService : ITourSlotService
    {
        private readonly ITourSlotRepository _tourSlotRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TourSlotService> _logger;
        private readonly EmailSender _emailSender;

        public TourSlotService(
            ITourSlotRepository tourSlotRepository,
            IUnitOfWork unitOfWork,
            ILogger<TourSlotService> logger,
            EmailSender emailSender)
        {
            _tourSlotRepository = tourSlotRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task<IEnumerable<TourSlotDto>> GetSlotsAsync(
            Guid? tourTemplateId = null,
            Guid? tourDetailsId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            ScheduleDay? scheduleDay = null,
            bool includeInactive = false)
        {
            try
            {
                IEnumerable<DataAccessLayer.Entities.TourSlot> slots;

                if (tourDetailsId.HasValue)
                {
                    // Lấy slots của TourDetails cụ thể
                    slots = await _tourSlotRepository.GetByTourDetailsAsync(tourDetailsId.Value);
                }
                else if (tourTemplateId.HasValue)
                {
                    // Lấy slots của TourTemplate cụ thể
                    slots = await _tourSlotRepository.GetByTourTemplateAsync(tourTemplateId.Value);
                }
                else
                {
                    // Lấy slots với filter
                    slots = await _tourSlotRepository.GetAvailableSlotsAsync(
                        tourTemplateId, scheduleDay, fromDate, toDate, includeInactive);
                }

                // Apply additional filters if needed
                if (fromDate.HasValue)
                {
                    slots = slots.Where(s => s.TourDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    slots = slots.Where(s => s.TourDate <= toDate.Value);
                }

                if (scheduleDay.HasValue)
                {
                    slots = slots.Where(s => s.ScheduleDay == scheduleDay.Value);
                }

                if (!includeInactive)
                {
                    slots = slots.Where(s => s.IsActive);
                }

                return slots.Select(MapToDto).OrderBy(s => s.TourDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour slots with filters");
                throw;
            }
        }

        public async Task<TourSlotDto?> GetSlotByIdAsync(Guid id)
        {
            try
            {
                var slot = await _tourSlotRepository.GetByIdAsync(id);
                return slot != null ? MapToDto(slot) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour slot by ID: {SlotId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<TourSlotDto>> GetSlotsByTourDetailsAsync(Guid tourDetailsId)
        {
            try
            {
                var slots = await _tourSlotRepository.GetByTourDetailsAsync(tourDetailsId);
                return slots.Select(MapToDto).OrderBy(s => s.TourDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour slots for TourDetails: {TourDetailsId}", tourDetailsId);
                throw;
            }
        }

        public async Task<IEnumerable<TourSlotDto>> GetSlotsByTourTemplateAsync(Guid tourTemplateId)
        {
            try
            {
                var slots = await _tourSlotRepository.GetByTourTemplateAsync(tourTemplateId);
                return slots.Select(MapToDto).OrderBy(s => s.TourDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour slots for TourTemplate: {TourTemplateId}", tourTemplateId);
                throw;
            }
        }

        public async Task<IEnumerable<TourSlotDto>> GetUnassignedTemplateSlotsByTemplateAsync(Guid tourTemplateId, bool includeInactive = false)
        {
            try
            {
                var slots = await _tourSlotRepository.GetUnassignedTemplateSlotsByTemplateAsync(tourTemplateId, includeInactive);
                return slots.Select(MapToDto).OrderBy(s => s.TourDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unassigned template slots for TourTemplate: {TourTemplateId}", tourTemplateId);
                throw;
            }
        }

        public async Task<(bool Success, string Message, int CustomersNotified)> CancelPublicTourSlotAsync(Guid slotId, string reason, Guid tourCompanyUserId)
        {
            try
            {
                _logger.LogInformation("=== STARTING CANCEL TOUR SLOT ===");
                _logger.LogInformation("SlotId: {SlotId}, UserId: {UserId}, Reason: {Reason}", slotId, tourCompanyUserId, reason);

                // ✅ Sử dụng execution strategy để handle MySQL retry policy với transactions
                var executionStrategy = _unitOfWork.GetExecutionStrategy();

                return await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // 1. Lấy thông tin slot với tất cả related data
                        _logger.LogInformation("Step 1: Loading slot data with includes...");
                        var slot = await _unitOfWork.TourSlotRepository.GetQueryable()
                            .Include(s => s.TourDetails)
                                .ThenInclude(td => td!.TourOperation)
                            .Include(s => s.TourTemplate)
                            .Include(s => s.Bookings.Where(b => !b.IsDeleted && 
                                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)))
                                .ThenInclude(b => b.User)
                            .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsDeleted);

                        if (slot == null)
                        {
                            _logger.LogWarning("Step 1 FAILED: TourSlot not found - SlotId: {SlotId}", slotId);
                            return (false, "Không tìm thấy tour slot", 0);
                        }

                        _logger.LogInformation("Step 1 SUCCESS: Found slot - ID: {SlotId}, IsActive: {IsActive}, TourDetailsId: {TourDetailsId}", 
                            slot.Id, slot.IsActive, slot.TourDetailsId);

                        // 2. Validate business rules
                        _logger.LogInformation("Step 2: Validating business rules...");

                        if (slot.TourDetailsId == null)
                        {
                            _logger.LogWarning("Step 2 FAILED: TourSlot has no TourDetails assigned - SlotId: {SlotId}", slotId);
                            return (false, "Slot này chưa được assign tour details, không thể hủy", 0);
                        }

                        if (slot.TourDetails == null)
                        {
                            _logger.LogError("Step 2 FAILED: TourDetails is null despite TourDetailsId being set - SlotId: {SlotId}, TourDetailsId: {TourDetailsId}", 
                                slotId, slot.TourDetailsId);
                            return (false, "Không thể truy cập thông tin tour details", 0);
                        }

                        _logger.LogInformation("Step 2: TourDetails loaded - ID: {TourDetailsId}, Status: {Status}, CreatedById: {CreatedById}", 
                            slot.TourDetails.Id, slot.TourDetails.Status, slot.TourDetails.CreatedById);

                        if (slot.TourDetails.Status != TourDetailsStatus.Public)
                        {
                            _logger.LogWarning("Step 2 FAILED: TourDetails is not public - SlotId: {SlotId}, Status: {Status}", 
                                slotId, slot.TourDetails.Status);
                            return (false, "Chỉ có thể hủy tour đang ở trạng thái Public", 0);
                        }

                        // 3. Kiểm tra quyền sở hữu tour
                        _logger.LogInformation("Step 3: Checking ownership - TourDetailsCreatedById: {CreatedById}, CurrentUserId: {UserId}", 
                            slot.TourDetails.CreatedById, tourCompanyUserId);

                        if (slot.TourDetails.CreatedById != tourCompanyUserId)
                        {
                            _logger.LogWarning("Step 3 FAILED: User does not own this TourDetails - SlotId: {SlotId}, TourDetailsCreatedById: {CreatedById}, CurrentUserId: {UserId}", 
                                slotId, slot.TourDetails.CreatedById, tourCompanyUserId);
                            return (false, "Bạn không có quyền hủy tour này", 0);
                        }

                        if (!slot.IsActive)
                        {
                            _logger.LogWarning("Step 3 FAILED: TourSlot is not active - SlotId: {SlotId}", slotId);
                            return (false, "Tour slot đã bị hủy trước đó", 0);
                        }

                        _logger.LogInformation("Step 3 SUCCESS: All validations passed");

                        // 4. Lấy danh sách bookings cần xử lý
                        _logger.LogInformation("Step 4: Processing bookings...");
                        var affectedBookings = slot.Bookings.Where(b => !b.IsDeleted && 
                            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)).ToList();

                        _logger.LogInformation("Step 4: Found {BookingCount} affected bookings for slot {SlotId}", affectedBookings.Count, slotId);

                        // 5. Deactivate slot và set status
                        _logger.LogInformation("Step 5: Updating slot status...");
                        slot.IsActive = false;
                        slot.Status = TourSlotStatus.Cancelled;
                        slot.UpdatedAt = DateTime.UtcNow;
                        slot.UpdatedById = tourCompanyUserId;

                        await _unitOfWork.TourSlotRepository.UpdateAsync(slot);
                        _logger.LogInformation("Step 5 SUCCESS: Slot status updated");

                        // 6. Update các bookings thành cancelled
                        _logger.LogInformation("Step 6: Cancelling bookings...");
                        var affectedCustomers = new List<AffectedCustomerInfo>();

                        foreach (var booking in affectedBookings)
                        {
                            _logger.LogInformation("Processing booking {BookingId} - {BookingCode}", booking.Id, booking.BookingCode);

                            // Cancel booking
                            booking.Status = BookingStatus.CancelledByCompany;
                            booking.CancelledDate = DateTime.UtcNow;
                            booking.CancellationReason = reason;
                            booking.UpdatedAt = DateTime.UtcNow;
                            booking.UpdatedById = tourCompanyUserId;

                            await _unitOfWork.TourBookingRepository.UpdateAsync(booking);

                            // Tạo thông tin khách hàng bị ảnh hưởng - Ưu tiên ContactEmail từ booking
                            var customerInfo = new AffectedCustomerInfo
                            {
                                BookingId = booking.Id,
                                BookingCode = booking.BookingCode,
                                CustomerName = !string.IsNullOrEmpty(booking.ContactName) ? booking.ContactName : booking.User?.Name ?? "Khách hàng",
                                CustomerEmail = !string.IsNullOrEmpty(booking.ContactEmail) ? booking.ContactEmail : booking.User?.Email ?? "",
                                NumberOfGuests = booking.NumberOfGuests,
                                RefundAmount = booking.TotalPrice,
                                EmailSent = false
                            };

                            affectedCustomers.Add(customerInfo);
                            _logger.LogInformation("Added customer info - Name: {Name}, Email: {Email}", customerInfo.CustomerName, customerInfo.CustomerEmail);
                        }

                        _logger.LogInformation("Step 6 SUCCESS: Updated {BookingCount} bookings", affectedBookings.Count);

                        // 7. Release capacity từ TourOperation
                        _logger.LogInformation("Step 7: Releasing capacity...");
                        if (slot.TourDetails.TourOperation != null)
                        {
                            var totalGuestsToRelease = affectedBookings.Sum(b => b.NumberOfGuests);
                            var oldCurrentBookings = slot.TourDetails.TourOperation.CurrentBookings;
                            
                            slot.TourDetails.TourOperation.CurrentBookings = Math.Max(0, 
                                slot.TourDetails.TourOperation.CurrentBookings - totalGuestsToRelease);
                            
                            await _unitOfWork.TourOperationRepository.UpdateAsync(slot.TourDetails.TourOperation);
                            
                            _logger.LogInformation("Step 7 SUCCESS: Released {GuestCount} guests - CurrentBookings: {Old} -> {New}", 
                                totalGuestsToRelease, oldCurrentBookings, slot.TourDetails.TourOperation.CurrentBookings);
                        }
                        else
                        {
                            _logger.LogWarning("Step 7 WARNING: No TourOperation found to release capacity");
                        }

                        // 8. Commit transaction
                        _logger.LogInformation("Step 8: Saving changes...");
                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();
                        _logger.LogInformation("Step 8 SUCCESS: Transaction committed successfully");

                        // 9. Gửi email thông báo cho khách hàng (OUTSIDE transaction - non-blocking)
                        _logger.LogInformation("Step 9: Sending customer emails (post-transaction)...");
                        int customersNotified = 0;
                        
                        if (_emailSender == null)
                        {
                            _logger.LogWarning("Step 9 SKIPPED: EmailSender service is null");
                        }
                        else
                        {
                            try
                            {
                                customersNotified = await SendCancellationEmailsToCustomersAsync(
                                    affectedCustomers, slot.TourDetails!.Title, slot.TourDate, reason);
                                _logger.LogInformation("Step 9 SUCCESS: Notified {CustomersNotified} customers via email", customersNotified);
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, "Step 9 FAILED: Error sending customer emails - EmailException: {EmailError}", emailEx.Message);
                                _logger.LogWarning("Step 9 CONTINUED: Cancellation was successful despite email failure");
                                customersNotified = 0; // Reset to 0 vì không gửi được email
                            }
                        }

                        // 10. Gửi thông báo cho tour company (OUTSIDE transaction - non-blocking)
                        _logger.LogInformation("Step 10: Sending company notification (post-transaction)...");
                        try
                        {
                            if (_emailSender == null)
                            {
                                _logger.LogWarning("Step 10 SKIPPED: EmailSender service is null");
                            }
                            else
                            {
                                await NotifyTourCompanyAboutCancellationAsync(
                                    tourCompanyUserId, slot.TourDetails!.Title, slot.TourDate, affectedBookings.Count, reason);
                                _logger.LogInformation("Step 10 SUCCESS: Company notification sent");
                            }
                        }
                        catch (Exception notifyEx)
                        {
                            _logger.LogError(notifyEx, "Step 10 FAILED: Error sending company notification - NotificationException: {NotifyError}", notifyEx.Message);
                            _logger.LogWarning("Step 10 CONTINUED: Cancellation was successful despite notification failure");
                        }

                        _logger.LogInformation("=== CANCEL TOUR SLOT COMPLETED SUCCESSFULLY ===");
                        _logger.LogInformation("Final result - SlotId: {SlotId}, AffectedBookings: {BookingCount}, CustomersNotified: {CustomersNotified}", 
                            slotId, affectedBookings.Count, customersNotified);

                        // Tạo message phù hợp dựa trên việc gửi email có thành công không
                        var successMessage = customersNotified > 0 
                            ? $"Hủy tour thành công. Đã thông báo email cho {customersNotified}/{affectedCustomers.Count} khách hàng và xử lý {affectedBookings.Count} booking."
                            : $"Hủy tour thành công. Đã xử lý {affectedBookings.Count} booking. (Email thông báo có thể gửi chậm do sự cố kỹ thuật)";

                        return (true, successMessage, customersNotified);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Transaction failed, rolling back - Exception: {ExceptionType}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
                        
                        try
                        {
                            await transaction.RollbackAsync();
                            _logger.LogInformation("Transaction rolled back successfully");
                        }
                        catch (Exception rollbackEx)
                        {
                            _logger.LogError(rollbackEx, "CRITICAL: Error during transaction rollback - RollbackException: {RollbackError}", rollbackEx.Message);
                        }
                        
                        // Re-throw để execution strategy có thể handle
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("=== CANCEL TOUR SLOT FAILED ===");
                _logger.LogError(ex, "Critical error cancelling tour slot - SlotId: {SlotId}, UserId: {UserId}", slotId, tourCompanyUserId);
                _logger.LogError("Exception type: {ExceptionType}", ex.GetType().Name);
                _logger.LogError("Exception message: {ExceptionMessage}", ex.Message);
                _logger.LogError("Inner exception: {InnerException}", ex.InnerException?.Message ?? "None");
                
                // Log stack trace chỉ khi cần thiết (không phải lỗi business logic)
                if (!(ex is InvalidOperationException || ex is ArgumentException))
                {
                    _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                }
                
                return (false, $"Có lỗi xảy ra khi hủy tour: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Gửi email thông báo hủy tour cho khách hàng
        /// </summary>
        private async Task<int> SendCancellationEmailsToCustomersAsync(
            List<AffectedCustomerInfo> affectedCustomers, 
            string tourTitle, 
            DateOnly tourDate, 
            string reason)
        {
            int successCount = 0;

            foreach (var customer in affectedCustomers)
            {
                try
                {
                    // Kiểm tra email hợp lệ trước khi gửi
                    if (string.IsNullOrEmpty(customer.CustomerEmail) || !IsValidEmail(customer.CustomerEmail))
                    {
                        _logger.LogWarning("Invalid email address for customer {CustomerName} with booking {BookingCode}: {Email}", 
                            customer.CustomerName, customer.BookingCode, customer.CustomerEmail);
                        customer.EmailSent = false;
                        continue;
                    }

                    var subject = $"🚨 Thông báo hủy tour: {tourTitle}";
                    var htmlBody = $@"
                        <h2>Kính chào {customer.CustomerName},</h2>
                        
                        <div style='background-color: #f8d7da; padding: 20px; border-left: 4px solid #dc3545; margin: 15px 0;'>
                            <h3 style='margin-top: 0; color: #721c24;'>🚨 THÔNG BÁO HỦY TOUR</h3>
                            <p style='font-size: 16px; margin-bottom: 0;'>
                                Chúng tôi rất tiếc phải thông báo rằng tour <strong>'{tourTitle}'</strong> dự kiến ngày <strong>{tourDate:dd/MM/yyyy}</strong> đã bị hủy.
                            </p>
                        </div>
                        
                        <h3>📋 Thông tin booking của bạn:</h3>
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #6c757d; margin: 10px 0;'>
                            <ul style='margin: 0; list-style: none; padding: 0;'>
                                <li><strong>🆔 Mã booking:</strong> {customer.BookingCode}</li>
                                <li><strong>👥 Số lượng khách:</strong> {customer.NumberOfGuests}</li>
                                <li><strong>💰 Số tiền:</strong> {customer.RefundAmount:N0} VNĐ</li>
                            </ul>
                        </div>
                        
                        <h3>📝 Lý do hủy tour:</h3>
                        <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 10px 0;'>
                            <p style='font-style: italic; margin: 0;'>{reason}</p>
                        </div>
                        
                        <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0;'>
                            <h3 style='margin-top: 0; color: #155724;'>💰 HOÀN TIỀN TỰ ĐỘNG</h3>
                            <p style='font-size: 16px; margin-bottom: 10px;'>
                                <strong>Số tiền {customer.RefundAmount:N0} VNĐ sẽ được hoàn trả đầy đủ</strong>
                            </p>
                            <ul style='margin-bottom: 0;'>
                                <li>⏰ <strong>Thời gian:</strong> 3-5 ngày làm việc</li>
                                <li>💳 <strong>Phương thức:</strong> Hoàn về tài khoản thanh toán gốc</li>
                                <li>📧 <strong>Xác nhận:</strong> Bạn sẽ nhận email xác nhận khi tiền được hoàn</li>
                                <li>📞 <strong>Hỗ trợ:</strong> Nhân viên sẽ liên hệ để hỗ trợ thủ tục hoàn tiền</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #004085;'>🎯 Gợi ý cho bạn:</h4>
                            <ul style='margin-bottom: 0;'>
                                <li><strong>Khám phá tour khác:</strong> Xem danh sách tour tương tự trên website</li>
                                <li><strong>Đặt lại sau:</strong> Tour có thể được mở lại với lịch trình mới</li>
                                <li><strong>Nhận ưu đãi:</strong> Theo dõi để nhận thông báo khuyến mãi đặc biệt</li>
                                <li><strong>Voucher bù đắp:</strong> Chúng tôi sẽ gửi voucher giảm giá cho lần đặt tour tiếp theo</li>
                            </ul>
                        </div>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='#' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-right: 10px;'>
                                🔍 Xem tour khác
                            </a>
                            <a href='#' style='background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                📞 Liên hệ hỗ trợ
                            </a>
                        </div>
                        
                        <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #721c24;'>🙏 Lời xin lỗi chân thành</h4>
                            <p style='margin-bottom: 0;'>
                                Chúng tôi thành thật xin lỗi vì sự bất tiện này. Đây là quyết định khó khăn nhưng cần thiết để đảm bảo an toàn và chất lượng dịch vụ cho quý khách. 
                                <strong>Nhân viên của chúng tôi sẽ liên hệ trực tiếp để hỗ trợ quá trình hoàn tiền trong thời gian sớm nhất.</strong>
                            </p>
                        </div>
                        
                        <p><strong>📞 Cần hỗ trợ khẩn cấp?</strong> Liên hệ hotline: <a href='tel:1900-xxx-xxx'>1900-xxx-xxx</a> hoặc email: support@tayninhour.com</p>
                        
                        <br/>
                        <p>Cảm ơn sự thông cảm của quý khách,</p>
                        <p><strong>Đội ngũ Tay Ninh Tour</strong></p>";

                    await _emailSender.SendEmailAsync(customer.CustomerEmail, customer.CustomerName, subject, htmlBody);
                    customer.EmailSent = true;
                    successCount++;
                    
                    _logger.LogInformation("Cancellation email sent successfully to {CustomerEmail} for booking {BookingCode}", 
                        customer.CustomerEmail, customer.BookingCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send cancellation email to customer {CustomerEmail} for booking {BookingCode}", 
                        customer.CustomerEmail, customer.BookingCode);
                    customer.EmailSent = false;
                }
            }

            return successCount;
        }

        /// <summary>
        /// Validate email address format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi thông báo cho tour company về việc hủy tour
        /// </summary>
        private async Task NotifyTourCompanyAboutCancellationAsync(
            Guid tourCompanyUserId, 
            string tourTitle, 
            DateOnly tourDate, 
            int affectedBookingsCount, 
            string reason)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(tourCompanyUserId);
                if (user == null) return;

                var subject = $"✅ Xác nhận hủy tour: {tourTitle}";
                var htmlBody = $@"
                    <h2>Chào {user.Name},</h2>
                    
                    <div style='background-color: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 15px 0;'>
                        <h3 style='margin-top: 0; color: #155724;'>✅ HỦY TOUR THÀNH CÔNG</h3>
                        <p style='font-size: 16px; margin-bottom: 0;'>
                            Tour <strong>'{tourTitle}'</strong> ngày <strong>{tourDate:dd/MM/yyyy}</strong> đã được hủy thành công.
                        </p>
                    </div>
                    
                    <h3>📊 Thống kê:</h3>
                    <ul>
                        <li><strong>Số booking bị ảnh hưởng:</strong> {affectedBookingsCount}</li>
                        <li><strong>Lý do hủy:</strong> {reason}</li>
                        <li><strong>Thời gian hủy:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
                    </ul>
                    
                    <div style='background-color: #cce5ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h4 style='margin-top: 0; color: #004085;'>📋 Những gì đã được xử lý:</h4>
                        <ul style='margin-bottom: 0;'>
                            <li>✅ Slot tour đã được deactivate</li>
                            <li>✅ Tất cả booking đã được hủy</li>
                            <li>✅ Khách hàng đã được thông báo qua email</li>
                            <li>✅ Tiền sẽ được hoàn trả tự động trong 3-5 ngày</li>
                        </ul>
                    </div>
                    
                    <br/>
                    <p>Cảm ơn bạn đã sử dụng hệ thống một cách có trách nhiệm.</p>
                    <p>Trân trọng,</p>
                    <p>Đội ngũ Tay Ninh Tour</p>";

                await _emailSender.SendEmailAsync(user.Email, user.Name, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to tour company {TourCompanyUserId}", tourCompanyUserId);
            }
        }

        public async Task<IEnumerable<TourSlotDto>> GetAvailableSlotsAsync(
            Guid? tourTemplateId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var slots = await _tourSlotRepository.GetAvailableSlotsAsync(
                    tourTemplateId, null, fromDate, toDate, false);

                // Only return available slots with capacity
                var availableSlots = slots.Where(s => s.Status == TourSlotStatus.Available && s.AvailableSpots > 0);

                return availableSlots.Select(MapToDto).OrderBy(s => s.TourDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tour slots");
                throw;
            }
        }

        public async Task<bool> CanBookSlotAsync(Guid slotId, int requestedGuests)
        {
            try
            {
                var slot = await _tourSlotRepository.GetSlotWithCapacityAsync(slotId);
                if (slot == null)
                {
                    _logger.LogWarning("TourSlot not found: {SlotId}", slotId);
                    return false;
                }

                // Kiểm tra các điều kiện cơ bản
                if (!slot.IsActive || slot.Status != TourSlotStatus.Available)
                {
                    _logger.LogDebug("TourSlot {SlotId} is not available. IsActive: {IsActive}, Status: {Status}", 
                        slotId, slot.IsActive, slot.Status);
                    return false;
                }

                // Kiểm tra ngày tour
                if (slot.TourDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    _logger.LogDebug("TourSlot {SlotId} is in the past. TourDate: {TourDate}", 
                        slotId, slot.TourDate);
                    return false;
                }

                // Kiểm tra capacity
                var availableSpots = slot.AvailableSpots;
                var canBook = availableSpots >= requestedGuests;
                
                _logger.LogDebug("Capacity check for slot {SlotId}: Available={Available}, Requested={Requested}, CanBook={CanBook}", 
                    slotId, availableSpots, requestedGuests, canBook);
                
                return canBook;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if slot can be booked: {SlotId}", slotId);
                return false;
            }
        }

        public async Task<bool> ReserveSlotCapacityAsync(Guid slotId, int guestsToReserve)
        {
            try
            {
                // ✅ KHÔNG dùng AtomicReserveCapacityAsync nữa vì nó cộng CurrentBookings
                // CHỈ check capacity, không cộng CurrentBookings khi tạo booking
                var success = await _tourSlotRepository.CheckSlotCapacityAsync(slotId, guestsToReserve);
                
                if (success)
                {
                    _logger.LogInformation("Capacity check passed for {Guests} guests in slot {SlotId} - NO CurrentBookings updated yet (pending payment)", 
                        guestsToReserve, slotId);
                }
                else
                {
                    _logger.LogWarning("Capacity check failed for {Guests} guests in slot {SlotId}. Slot may be unavailable, fully booked, or insufficient capacity.", 
                        guestsToReserve, slotId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slot capacity: {SlotId}", slotId);
                return false;
            }
        }

        /// <summary>
        /// CHỈ dùng khi thanh toán thành công - CẬP NHẬT CurrentBookings
        /// </summary>
        public async Task<bool> ConfirmSlotCapacityAsync(Guid slotId, int guestsToConfirm)
        {
            try
            {
                // ✅ Dùng AtomicReserveCapacityAsync để CẬP NHẬT CurrentBookings
                var success = await _tourSlotRepository.AtomicReserveCapacityAsync(slotId, guestsToConfirm);
                
                if (success)
                {
                    _logger.LogInformation("Successfully updated CurrentBookings (+{Guests}) for slot {SlotId} after payment confirmation", 
                        guestsToConfirm, slotId);
                }
                else
                {
                    _logger.LogWarning("Failed to update CurrentBookings for {Guests} guests in slot {SlotId}.", 
                        guestsToConfirm, slotId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming slot capacity: {SlotId}", slotId);
                return false;
            }
        }

        public async Task<bool> ReleaseSlotCapacityAsync(Guid slotId, int guestsToRelease)
        {
            try
            {
                // ✅ Không tạo transaction mới - sử dụng transaction hiện tại nếu có
                var slot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .FirstOrDefaultAsync(s => s.Id == slotId);

                if (slot == null)
                {
                    _logger.LogWarning("TourSlot not found for release: {SlotId}", slotId);
                    return false;
                }

                // Update current bookings
                slot.CurrentBookings = Math.Max(0, slot.CurrentBookings - guestsToRelease);
                slot.UpdatedAt = DateTime.UtcNow;

                // Update status if no longer fully booked
                if (slot.Status == TourSlotStatus.FullyBooked && slot.AvailableSpots > 0)
                {
                    slot.Status = TourSlotStatus.Available;
                }

                await _unitOfWork.TourSlotRepository.UpdateAsync(slot);
                // ✅ Không gọi SaveChanges ở đây - để caller quyết định khi nào save

                _logger.LogInformation("Released {Guests} guests for slot {SlotId}. New capacity: {Current}/{Max}",
                    guestsToRelease, slotId, slot.CurrentBookings, slot.MaxGuests);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing slot capacity: {SlotId}", slotId);
                return false;
            }
        }

        /// <summary>
        /// Release slot capacity với transaction riêng (dùng khi không có transaction hiện tại)
        /// </summary>
        public async Task<bool> ReleaseSlotCapacityWithTransactionAsync(Guid slotId, int guestsToRelease)
        {
            try
            {
                // ✅ Sử dụng execution strategy để handle retry logic với transactions
                var executionStrategy = _unitOfWork.GetExecutionStrategy();

                return await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();

                    var result = await ReleaseSlotCapacityAsync(slotId, guestsToRelease);
                    if (result)
                    {
                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                    }

                    return result;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing slot capacity with transaction: {SlotId}", slotId);
                return false;
            }
        }

        public async Task<bool> UpdateSlotCapacityAsync(Guid slotId, int maxGuests)
        {
            try
            {
                var slot = await _unitOfWork.TourSlotRepository.GetByIdAsync(slotId);
                if (slot == null)
                {
                    _logger.LogWarning("TourSlot not found for capacity update: {SlotId}", slotId);
                    return false;
                }

                slot.MaxGuests = maxGuests;
                slot.UpdatedAt = DateTime.UtcNow;

                // Update status based on new capacity
                if (slot.CurrentBookings >= maxGuests)
                {
                    slot.Status = TourSlotStatus.FullyBooked;
                }
                else if (slot.Status == TourSlotStatus.FullyBooked)
                {
                    slot.Status = TourSlotStatus.Available;
                }

                await _unitOfWork.TourSlotRepository.UpdateAsync(slot);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated capacity for slot {SlotId} to {MaxGuests}. Current bookings: {Current}", 
                    slotId, maxGuests, slot.CurrentBookings);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slot capacity: {SlotId}", slotId);
                return false;
            }
        }

        public async Task<bool> SyncSlotsCapacityAsync(Guid tourDetailsId, int maxGuests)
        {
            try
            {
                var slots = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Where(s => s.TourDetailsId == tourDetailsId)
                    .ToListAsync();

                if (!slots.Any())
                {
                    _logger.LogInformation("No slots found for TourDetails {TourDetailsId}", tourDetailsId);
                    return true;
                }

                // Use execution strategy to handle transaction properly
                var executionStrategy = _unitOfWork.GetExecutionStrategy();

                return await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _unitOfWork.BeginTransactionAsync();

                    foreach (var slot in slots)
                    {
                        slot.MaxGuests = maxGuests;
                        slot.UpdatedAt = DateTime.UtcNow;

                        // Update status based on new capacity
                        if (slot.CurrentBookings >= maxGuests)
                        {
                            slot.Status = TourSlotStatus.FullyBooked;
                        }
                        else if (slot.Status == TourSlotStatus.FullyBooked)
                        {
                            slot.Status = TourSlotStatus.Available;
                        }

                        await _unitOfWork.TourSlotRepository.UpdateAsync(slot);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Synced capacity for {Count} slots in TourDetails {TourDetailsId} to {MaxGuests}",
                        slots.Count, tourDetailsId, maxGuests);

                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing slots capacity for TourDetails {TourDetailsId}", tourDetailsId);
                return false;
            }
        }

        /// <summary>
        /// Get capacity summary for debugging purposes
        /// </summary>
        public async Task<(bool IsValid, string DebugInfo)> GetSlotCapacityDebugInfoAsync(Guid slotId)
        {
            try
            {
                var slot = await _tourSlotRepository.GetSlotWithCapacityAsync(slotId);
                if (slot == null)
                {
                    return (false, $"Slot {slotId} not found");
                }

                var debugInfo = $@"
Slot Debug Info for {slotId}:
- IsActive: {slot.IsActive}
- Status: {slot.Status} ({(int)slot.Status})
- TourDate: {slot.TourDate}
- MaxGuests: {slot.MaxGuests}
- CurrentBookings: {slot.CurrentBookings}
- AvailableSpots: {slot.AvailableSpots}
- IsDeleted: {slot.IsDeleted}
- TourDetailsId: {slot.TourDetailsId}
- TourDetails Status: {slot.TourDetails?.Status}
- TourOperation IsActive: {slot.TourDetails?.TourOperation?.IsActive}
- TourOperation CurrentBookings: {slot.TourDetails?.TourOperation?.CurrentBookings}
- TourOperation MaxGuests: {slot.TourDetails?.TourOperation?.MaxGuests}";

                var isValid = slot.IsActive && 
                             slot.Status == TourSlotStatus.Available && 
                             slot.TourDate > DateOnly.FromDateTime(DateTime.UtcNow) &&
                             slot.AvailableSpots > 0;

                return (isValid, debugInfo);
            }
            catch (Exception ex)
            {
                return (false, $"Error getting debug info: {ex.Message}");
            }
        }

        /// <summary>
        /// Map TourSlot entity to DTO
        /// </summary>
        private TourSlotDto MapToDto(DataAccessLayer.Entities.TourSlot slot)
        {
            // Determine capacity info
            // MaxGuests: Use TourOperation.MaxGuests if available (shared capacity for all slots)
            // CurrentBookings: Always use slot.CurrentBookings (specific to this slot)
            int maxGuests = slot.MaxGuests;
            int currentBookings = slot.CurrentBookings;

            if (slot.TourDetails?.TourOperation != null)
            {
                maxGuests = slot.TourDetails.TourOperation.MaxGuests;
                // Keep currentBookings from slot (slot-specific bookings)
            }

            int availableSpots = maxGuests - currentBookings;

            var dto = new TourSlotDto
            {
                Id = slot.Id,
                TourTemplateId = slot.TourTemplateId,
                TourDetailsId = slot.TourDetailsId,
                TourDate = slot.TourDate,
                ScheduleDay = slot.ScheduleDay,
                ScheduleDayName = GetScheduleDayName(slot.ScheduleDay),
                Status = slot.Status,
                StatusName = GetStatusName(slot.Status),
                MaxGuests = maxGuests,
                CurrentBookings = currentBookings,
                AvailableSpots = availableSpots,
                IsActive = slot.IsActive,
                CreatedAt = slot.CreatedAt,
                UpdatedAt = slot.UpdatedAt,
                FormattedDate = slot.TourDate.ToString("dd/MM/yyyy"),
                FormattedDateWithDay = $"{GetScheduleDayName(slot.ScheduleDay)} - {slot.TourDate.ToString("dd/MM/yyyy")}"

            };

            // Map TourTemplate info if available
            if (slot.TourTemplate != null)
            {
                dto.TourTemplate = new TourTemplateInfo
                {
                    Id = slot.TourTemplate.Id,
                    Title = slot.TourTemplate.Title,
                    StartLocation = slot.TourTemplate.StartLocation,
                    EndLocation = slot.TourTemplate.EndLocation,
                    TemplateType = slot.TourTemplate.TemplateType
                };
            }

            // Map TourDetails info if available
            if (slot.TourDetails != null)
            {
                dto.TourDetails = new TourDetailsInfo
                {
                    Id = slot.TourDetails.Id,
                    Title = slot.TourDetails.Title,
                    Description = slot.TourDetails.Description,
                    Status = slot.TourDetails.Status,
                    StatusName = GetTourDetailsStatusName(slot.TourDetails.Status)
                };

                // Map TourOperation info if TourDetails has one
                if (slot.TourDetails.TourOperation != null)
                {
                    dto.TourOperation = new TourOperationInfo
                    {
                        Id = slot.TourDetails.TourOperation.Id,
                        Price = slot.TourDetails.TourOperation.Price,
                        MaxGuests = slot.TourDetails.TourOperation.MaxGuests,
                        CurrentBookings = slot.TourDetails.TourOperation.CurrentBookings,
                        AvailableSpots = slot.TourDetails.TourOperation.MaxGuests - slot.TourDetails.TourOperation.CurrentBookings,
                        Status = slot.TourDetails.TourOperation.Status,
                        IsActive = slot.TourDetails.TourOperation.IsActive
                    };
                }
            }

            return dto;
        }

        /// <summary>
        /// Lấy tên ngày trong tuần bằng tiếng Việt
        /// </summary>
        private string GetScheduleDayName(ScheduleDay scheduleDay)
        {
            return scheduleDay switch
            {
                ScheduleDay.Sunday => "Chủ nhật",
                ScheduleDay.Saturday => "Thứ bảy",
                _ => scheduleDay.ToString()
            };
        }

        /// <summary>
        /// Lấy tên trạng thái slot bằng tiếng Việt
        /// </summary>
        private string GetStatusName(TourSlotStatus status)
        {
            return status switch
            {
                TourSlotStatus.Available => "Có sẵn",
                TourSlotStatus.FullyBooked => "Đã đầy",
                TourSlotStatus.Cancelled => "Đã hủy",
                TourSlotStatus.Completed => "Hoàn thành",
                TourSlotStatus.InProgress => "Đang thực hiện",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Lấy tên trạng thái TourDetails bằng tiếng Việt
        /// </summary>
        private string GetTourDetailsStatusName(TourDetailsStatus status)
        {
            return status switch
            {
                TourDetailsStatus.Pending => "Chờ duyệt",
                TourDetailsStatus.Approved => "Đã duyệt",
                TourDetailsStatus.Rejected => "Từ chối",
                TourDetailsStatus.Suspended => "Tạm ngưng",
                TourDetailsStatus.AwaitingGuideAssignment => "Chờ phân công hướng dẫn viên",
                TourDetailsStatus.Cancelled => "Đã hủy",
                TourDetailsStatus.AwaitingAdminApproval => "Chờ admin duyệt",
                TourDetailsStatus.WaitToPublic => "Chờ công khai",
                TourDetailsStatus.Public => "Công khai",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Lấy chi tiết slot với thông tin tour và danh sách user đã book
        /// </summary>
        public async Task<TourSlotWithBookingsDto?> GetSlotWithTourDetailsAndBookingsAsync(Guid slotId)
        {
            try
            {
                // Get slot with all related data
                var slot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(s => s.TourTemplate)
                    .Include(s => s.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(s => s.Bookings.Where(b => !b.IsDeleted))
                        .ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsDeleted);

                if (slot == null)
                {
                    _logger.LogWarning("TourSlot not found: {SlotId}", slotId);
                    return null;
                }

                // Map basic slot info
                var slotDto = MapToDto(slot);

                // Create result DTO
                var result = new TourSlotWithBookingsDto
                {
                    Slot = slotDto,
                    BookedUsers = new List<BookedUserInfo>(),
                    Statistics = new BookingStatistics()
                };

                // Map TourDetails summary if available
                if (slot.TourDetails != null)
                {
                    result.TourDetails = new TourDetailsSummary
                    {
                        Id = slot.TourDetails.Id,
                        Title = slot.TourDetails.Title,
                        Description = slot.TourDetails.Description ?? string.Empty,
                        ImageUrls = slot.TourDetails.ImageUrls ?? new List<string>(),
                        SkillsRequired = !string.IsNullOrEmpty(slot.TourDetails.SkillsRequired) 
                            ? slot.TourDetails.SkillsRequired.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList()
                            : new List<string>(),
                        Status = slot.TourDetails.Status,
                        StatusName = GetTourDetailsStatusName(slot.TourDetails.Status),
                        CreatedAt = slot.TourDetails.CreatedAt,
                        TourTemplate = slotDto.TourTemplate,
                        TourOperation = slotDto.TourOperation
                    };
                }

                // Map booked users
                var bookings = slot.Bookings.Where(b => !b.IsDeleted).ToList();
                
                foreach (var booking in bookings)
                {
                    var bookedUser = new BookedUserInfo
                    {
                        BookingId = booking.Id,
                        UserId = booking.UserId,
                        UserName = booking.User?.Name ?? "N/A",
                        UserEmail = booking.User?.Email,
                        ContactName = booking.ContactName,
                        ContactPhone = booking.ContactPhone,
                        ContactEmail = booking.ContactEmail,
                        NumberOfGuests = booking.NumberOfGuests,
                        TotalPrice = booking.TotalPrice,
                        OriginalPrice = booking.OriginalPrice,
                        DiscountPercent = booking.DiscountPercent,
                        Status = booking.Status,
                        StatusName = GetBookingStatusName(booking.Status),
                        BookingDate = booking.BookingDate,
                        ConfirmedDate = booking.ConfirmedDate,
                        BookingCode = booking.BookingCode,
                        CustomerNotes = booking.CustomerNotes
                    };

                    result.BookedUsers.Add(bookedUser);
                }

                // Calculate statistics
                result.Statistics = CalculateBookingStatistics(bookings, slot.MaxGuests);

                _logger.LogInformation("Retrieved slot {SlotId} with {BookingCount} bookings", slotId, bookings.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot with tour details and bookings: {SlotId}", slotId);
                throw;
            }
        }

        /// <summary>
        /// Lấy tên trạng thái booking bằng tiếng Việt
        /// </summary>
        private string GetBookingStatusName(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "Chờ xử lý",
                BookingStatus.Confirmed => "Đã xác nhận",
                BookingStatus.CancelledByCustomer => "Hủy bởi khách hàng",
                BookingStatus.CancelledByCompany => "Hủy bởi công ty",
                BookingStatus.Completed => "Hoàn thành",
                BookingStatus.Refunded => "Đã hoàn tiền",
                BookingStatus.NoShow => "Không xuất hiện",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Tính toán thống kê booking
        /// </summary>
        private BookingStatistics CalculateBookingStatistics(List<DataAccessLayer.Entities.TourBooking> bookings, int maxGuests)
        {
            var stats = new BookingStatistics();

            if (!bookings.Any())
            {
                return stats;
            }

            stats.TotalBookings = bookings.Count;
            stats.TotalGuests = bookings.Sum(b => b.NumberOfGuests);
            stats.ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed);
            stats.PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending);
            stats.CancelledBookings = bookings.Count(b => b.Status == BookingStatus.CancelledByCustomer || b.Status == BookingStatus.CancelledByCompany);
            
            stats.TotalRevenue = bookings.Sum(b => b.TotalPrice);
            stats.ConfirmedRevenue = bookings
                .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                .Sum(b => b.TotalPrice);

            // Calculate occupancy rate
            if (maxGuests > 0)
            {
                var confirmedGuests = bookings
                    .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    .Sum(b => b.NumberOfGuests);
                stats.OccupancyRate = Math.Round((double)confirmedGuests / maxGuests * 100, 2);
            }

            return stats;
        }
    }
}
