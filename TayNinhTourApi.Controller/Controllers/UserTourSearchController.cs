using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller x? lý tìm ki?m tour cho user (không c?n authentication)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserTourSearchController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserTourSearchController> _logger;

        public UserTourSearchController(
            IUnitOfWork unitOfWork,
            ILogger<UserTourSearchController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Tìm ki?m tour theo schedule day, tháng, n?m, ?i?m ??n và text search
        /// </summary>
        /// <param name="scheduleDay">Th? trong tu?n (Saturday ho?c Sunday)</param>
        /// <param name="month">Tháng (1-12)</param>
        /// <param name="year">N?m</param>
        /// <param name="destination">?i?m ??n (tìm ki?m trong EndLocation c?a TourTemplate)</param>
        /// <param name="textSearch">Text tìm ki?m trong title c?a TourDetails</param>
        /// <param name="pageIndex">Trang hi?n t?i (m?c ??nh: 1)</param>
        /// <param name="pageSize">S? l??ng items per page (m?c ??nh: 10, t?i ?a: 50)</param>
        /// <returns>Danh sách tours tìm ???c</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchTours(
            [FromQuery] ScheduleDay? scheduleDay = null,
            [FromQuery] int? month = null,
            [FromQuery] int? year = null,
            [FromQuery] string? destination = null,
            [FromQuery] string? textSearch = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Searching tours with filters - ScheduleDay: {ScheduleDay}, Month: {Month}, Year: {Year}, Destination: {Destination}, TextSearch: {TextSearch}, Page: {PageIndex}, Size: {PageSize}",
                    scheduleDay, month, year, destination, textSearch, pageIndex, pageSize);

                // Validate parameters
                if (pageIndex < 1) pageIndex = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 50) pageSize = 50;

                if (month.HasValue && (month.Value < 1 || month.Value > 12))
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Tháng ph?i t? 1 ??n 12"
                    });
                }

                if (year.HasValue && (year.Value < 2024 || year.Value > 2030))
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "N?m ph?i t? 2024 ??n 2030"
                    });
                }

                // Validate schedule day - only Saturday or Sunday allowed
                if (scheduleDay.HasValue && scheduleDay.Value != ScheduleDay.Saturday && scheduleDay.Value != ScheduleDay.Sunday)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Hi?n t?i h? th?ng ch? h? tr? tour vào th? 7 (Saturday) và ch? nh?t (Sunday)"
                    });
                }

                // Build query for TourDetails with related entities
                var query = _unitOfWork.TourDetailsRepository.GetQueryable()
                    .Include(td => td.TourTemplate)
                    .Include(td => td.TourTemplate.Images)
                    .Include(td => td.TourTemplate.CreatedBy)
                    .Include(td => td.TourOperation)
                    .Include(td => td.AssignedSlots.Where(ts => ts.IsActive && !ts.IsDeleted))
                    .Where(td => td.IsActive && 
                                !td.IsDeleted && 
                                td.Status == TourDetailsStatus.Public &&
                                td.TourTemplate.IsActive && 
                                !td.TourTemplate.IsDeleted);

                // Apply filters

                // Filter by schedule day
                if (scheduleDay.HasValue)
                {
                    query = query.Where(td => td.TourTemplate.ScheduleDays == scheduleDay.Value);
                }

                // Filter by month and year
                if (month.HasValue)
                {
                    query = query.Where(td => td.TourTemplate.Month == month.Value);
                }

                if (year.HasValue)
                {
                    query = query.Where(td => td.TourTemplate.Year == year.Value);
                }

                // Filter by destination (search in EndLocation)
                if (!string.IsNullOrEmpty(destination))
                {
                    query = query.Where(td => td.TourTemplate.EndLocation.Contains(destination));
                }

                // Filter by text search (search in TourDetails title)
                if (!string.IsNullOrEmpty(textSearch))
                {
                    query = query.Where(td => td.Title.Contains(textSearch));
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination and get results
                var tourDetails = await query
                    .OrderByDescending(td => td.CreatedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to response DTOs
                var result = tourDetails.Select(td => new TourSearchResultDto
                {
                    Id = td.Id,
                    Title = td.Title,
                    Description = td.Description,
                    Status = td.Status.ToString(),
                    SkillsRequired = td.SkillsRequired,
                    ImageUrls = td.ImageUrls,
                    CreatedAt = td.CreatedAt,
                    
                    // TourTemplate information
                    TourTemplate = new TourTemplateBasicDto
                    {
                        Id = td.TourTemplate.Id,
                        Title = td.TourTemplate.Title,
                        TemplateType = td.TourTemplate.TemplateType.ToString(),
                        ScheduleDays = td.TourTemplate.ScheduleDays.ToString(),
                        ScheduleDaysVietnamese = td.TourTemplate.ScheduleDays.GetVietnameseName(),
                        StartLocation = td.TourTemplate.StartLocation,
                        EndLocation = td.TourTemplate.EndLocation,
                        Month = td.TourTemplate.Month,
                        Year = td.TourTemplate.Year,
                        Images = td.TourTemplate.Images.Select(img => new ImageDto
                        {
                            Id = img.Id,
                            Url = img.Url
                        }).ToList(),
                        CreatedBy = new CreatedByDto
                        {
                            Id = td.TourTemplate.CreatedBy.Id,
                            Name = td.TourTemplate.CreatedBy.Name,
                            Email = td.TourTemplate.CreatedBy.Email
                        }
                    },
                    
                    // TourOperation information
                    TourOperation = td.TourOperation != null ? new TourOperationBasicDto
                    {
                        Id = td.TourOperation.Id,
                        Price = td.TourOperation.Price,
                        MaxGuests = td.TourOperation.MaxGuests,
                        Description = td.TourOperation.Description,
                        Notes = td.TourOperation.Notes,
                        Status = td.TourOperation.Status.ToString(),
                        CurrentBookings = td.TourOperation.CurrentBookings
                    } : null,
                    
                    // Available slots information
                    AvailableSlots = td.AssignedSlots
                        .Where(slot => slot.TourDate >= DateOnly.FromDateTime(DateTime.Today) &&
                                      slot.Status == TourSlotStatus.Available &&
                                      slot.AvailableSpots > 0)
                        .Select(slot => new AvailableSlotDto
                        {
                            Id = slot.Id,
                            TourDate = slot.TourDate,
                            Status = slot.Status.ToString(),
                            MaxGuests = slot.MaxGuests,
                            CurrentBookings = slot.CurrentBookings,
                            AvailableSpots = slot.AvailableSpots
                        })
                        .OrderBy(slot => slot.TourDate)
                        .ToList()
                }).ToList();

                // Calculate pagination info
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var hasNextPage = pageIndex < totalPages;
                var hasPreviousPage = pageIndex > 1;

                var response = new
                {
                    StatusCode = 200,
                    Message = $"Tìm th?y {totalCount} tour phù h?p",
                    Data = new
                    {
                        Tours = result,
                        Pagination = new
                        {
                            TotalCount = totalCount,
                            PageIndex = pageIndex,
                            PageSize = pageSize,
                            TotalPages = totalPages,
                            HasNextPage = hasNextPage,
                            HasPreviousPage = hasPreviousPage
                        },
                        SearchCriteria = new
                        {
                            ScheduleDay = scheduleDay?.ToString(),
                            ScheduleDayVietnamese = scheduleDay?.GetVietnameseName(),
                            Month = month,
                            Year = year,
                            Destination = destination,
                            TextSearch = textSearch
                        }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tours");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi tìm ki?m tour",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// L?y danh sách các tùy ch?n filter có s?n
        /// </summary>
        /// <returns>Danh sách tùy ch?n filter</returns>
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilterOptions()
        {
            try
            {
                _logger.LogInformation("Getting tour filter options");

                // Get available schedule days
                var availableScheduleDays = new[]
                {
                    new { Value = ScheduleDay.Saturday.ToString(), Display = ScheduleDay.Saturday.GetVietnameseName() },
                    new { Value = ScheduleDay.Sunday.ToString(), Display = ScheduleDay.Sunday.GetVietnameseName() }
                };

                // Get available destinations from TourTemplates
                var availableDestinations = await _unitOfWork.TourTemplateRepository.GetQueryable()
                    .Where(tt => tt.IsActive && !tt.IsDeleted)
                    .Select(tt => tt.EndLocation)
                    .Distinct()
                    .OrderBy(location => location)
                    .ToListAsync();

                // Get available months and years from TourTemplates
                var availableMonthsYears = await _unitOfWork.TourTemplateRepository.GetQueryable()
                    .Where(tt => tt.IsActive && !tt.IsDeleted)
                    .Select(tt => new { tt.Month, tt.Year })
                    .Distinct()
                    .OrderBy(my => my.Year)
                    .ThenBy(my => my.Month)
                    .ToListAsync();

                var response = new
                {
                    StatusCode = 200,
                    Message = "L?y tùy ch?n filter thành công",
                    Data = new
                    {
                        ScheduleDays = availableScheduleDays,
                        Destinations = availableDestinations,
                        AvailableMonthsYears = availableMonthsYears,
                        YearRange = new { Min = 2024, Max = 2030 },
                        SupportedScheduleDays = new[] { "Saturday", "Sunday" },
                        Note = "Hi?n t?i h? th?ng ch? h? tr? tour vào th? 7 và ch? nh?t"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filter options");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có l?i x?y ra khi l?y tùy ch?n filter",
                    Error = ex.Message
                });
            }
        }
    }

    // ===== Response DTOs =====

    /// <summary>
    /// DTO cho k?t qu? tìm ki?m tour
    /// </summary>
    public class TourSearchResultDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? SkillsRequired { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public TourTemplateBasicDto TourTemplate { get; set; } = new TourTemplateBasicDto();
        public TourOperationBasicDto? TourOperation { get; set; }
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new List<AvailableSlotDto>();
    }

    /// <summary>
    /// DTO cho thông tin c? b?n TourTemplate
    /// </summary>
    public class TourTemplateBasicDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public string ScheduleDays { get; set; } = string.Empty;
        public string ScheduleDaysVietnamese { get; set; } = string.Empty;
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public List<ImageDto> Images { get; set; } = new List<ImageDto>();
        public CreatedByDto CreatedBy { get; set; } = new CreatedByDto();
    }

    /// <summary>
    /// DTO cho thông tin c? b?n TourOperation
    /// </summary>
    public class TourOperationBasicDto
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public int MaxGuests { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CurrentBookings { get; set; }
    }

    /// <summary>
    /// DTO cho slot có s?n
    /// </summary>
    public class AvailableSlotDto
    {
        public Guid Id { get; set; }
        public DateOnly TourDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int MaxGuests { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSpots { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin ng??i t?o
    /// </summary>
    public class CreatedByDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho hình ?nh
    /// </summary>
    public class ImageDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}