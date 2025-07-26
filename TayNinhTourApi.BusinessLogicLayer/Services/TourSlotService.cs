using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
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

        public TourSlotService(
            ITourSlotRepository tourSlotRepository,
            IUnitOfWork unitOfWork,
            ILogger<TourSlotService> logger)
        {
            _tourSlotRepository = tourSlotRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                using var transaction = await _unitOfWork.BeginTransactionAsync();

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
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

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
                MaxGuests = slot.MaxGuests,
                CurrentBookings = slot.CurrentBookings,
                AvailableSpots = slot.AvailableSpots,
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
