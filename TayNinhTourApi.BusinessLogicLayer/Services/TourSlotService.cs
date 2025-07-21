using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho TourSlot
    /// </summary>
    public class TourSlotService : ITourSlotService
    {
        private readonly ITourSlotRepository _tourSlotRepository;
        private readonly ILogger<TourSlotService> _logger;

        public TourSlotService(
            ITourSlotRepository tourSlotRepository,
            ILogger<TourSlotService> logger)
        {
            _tourSlotRepository = tourSlotRepository;
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

        public async Task<IEnumerable<TourSlotDto>> GetAvailableSlotsAsync(
            Guid? tourTemplateId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var slots = await _tourSlotRepository.GetAvailableSlotsAsync(
                    tourTemplateId, null, fromDate, toDate, false);

                // Only return available slots
                var availableSlots = slots.Where(s => s.Status == TourSlotStatus.Available);

                return availableSlots.Select(MapToDto).OrderBy(s => s.TourDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tour slots");
                throw;
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
    }
}
