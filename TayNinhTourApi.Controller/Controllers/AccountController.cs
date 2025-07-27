﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.User;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

using TayNinhTourApi.BusinessLogicLayer.DTOs.Request;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.Services;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITourGuideApplicationService _tourGuideApplicationService;
        private readonly IBlogReactionService _reactionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountController> _logger;
        private readonly ICurrentUserService _currentUserService;

        private readonly ISpecialtyShopApplicationService _specialtyShopApplicationService;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            IAccountService accountService,
            ITourGuideApplicationService tourGuideApplicationService,
            IBlogReactionService blogReactionService,
            IUnitOfWork unitOfWork,
            ILogger<AccountController> logger,
            ICurrentUserService currentUserService,

            ISpecialtyShopApplicationService specialtyShopApplicationService,
            IWebHostEnvironment environment)
        {
            _accountService = accountService;
            _tourGuideApplicationService = tourGuideApplicationService;
            _reactionService = blogReactionService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;

            _specialtyShopApplicationService = specialtyShopApplicationService;
            _environment = environment;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(PasswordDTO password)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _accountService.ChangePassword(password, currentUserObject);
            return StatusCode(result.StatusCode, result);

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit-profile")]
        public async Task<IActionResult> UpdateProfile(EditAccountProfileDTO editAccountProfileDTO)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _accountService.UpdateProfile(editAccountProfileDTO, currentUserObject);
            return StatusCode(result.StatusCode, result);

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _accountService.GetProfile(currentUserObject);
            return StatusCode(result.StatusCode, result);

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit-Avatar")]

        public async Task<IActionResult> UpdateAvatar([FromForm] AvatarDTO avatarDTO)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _accountService.UpdateAvatar(avatarDTO, currentUserObject);
            return StatusCode(result.StatusCode, result);

        }


        /// <summary>
        /// Lấy danh sách tất cả hướng dẫn viên
        /// </summary>
        /// <param name="includeInactive">Có bao gồm guides không active không</param>
        /// <returns>Danh sách guides</returns>
        [HttpGet("guides")]
        [Authorize(Roles = Constants.RoleTourCompanyName)]
        [ProducesResponseType(typeof(List<GuideDto>), 200)]
        public async Task<ActionResult<List<GuideDto>>> GetGuides(
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting guides list, includeInactive: {IncludeInactive}", includeInactive);

                // Lấy tour guides từ bảng TourGuides với User information
                var tourGuides = await _unitOfWork.TourGuideRepository.GetAllWithUserAsync();

                // Include User information
                var guidesWithUsers = tourGuides.Where(tg => tg.User != null);

                if (!includeInactive)
                {
                    guidesWithUsers = guidesWithUsers.Where(tg => tg.IsActive && tg.User.IsActive);
                }

                var guideDtos = guidesWithUsers.Select(tourGuide => new GuideDto
                {
                    Id = tourGuide.Id, // Use TourGuide.Id for backend compatibility
                    FullName = tourGuide.FullName,
                    Email = tourGuide.Email,
                    PhoneNumber = tourGuide.PhoneNumber,
                    IsActive = tourGuide.IsActive && tourGuide.User.IsActive,
                    IsAvailable = tourGuide.IsAvailable,
                    ExperienceYears = CalculateExperienceYears(tourGuide.Experience),
                    Specialization = tourGuide.Skills,
                    AverageRating = tourGuide.Rating,
                    CompletedTours = tourGuide.TotalToursGuided,
                    JoinedDate = tourGuide.ApprovedAt != default ? tourGuide.ApprovedAt : tourGuide.CreatedAt,
                    CurrentStatus = (tourGuide.IsActive && tourGuide.User.IsActive && tourGuide.IsAvailable) ? "Available" : "Inactive"
                }).OrderBy(g => g.FullName).ToList();

                _logger.LogInformation("Found {Count} guides", guideDtos.Count);
                return Ok(guideDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guides list");
                return StatusCode(500, new BaseResposeDto
                {
                    success = false,
                    Message = "Lỗi hệ thống khi lấy danh sách hướng dẫn viên"
                });
            }
        }

        /// <summary>
        /// Helper method to calculate experience years from experience text
        /// </summary>
        private int CalculateExperienceYears(string? experience)
        {
            if (string.IsNullOrEmpty(experience))
                return 0;

            // Try to extract number from experience text (e.g., "5 years" -> 5)
            var match = System.Text.RegularExpressions.Regex.Match(experience, @"(\d+)\s*(?:year|năm)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success && int.TryParse(match.Groups[1].Value, out int years))
            {
                return years;
            }

            return 0; // Default if can't parse
        }

        /// <summary>
        /// Lấy danh sách hướng dẫn viên available cho ngày cụ thể
        /// </summary>
        /// <param name="date">Ngày cần check availability</param>
        /// <param name="excludeOperationId">Loại trừ operation ID (khi update)</param>
        /// <returns>Danh sách guides available</returns>
        [HttpGet("guides/available")]
        [Authorize(Roles = Constants.RoleTourCompanyName)]
        [ProducesResponseType(typeof(List<GuideDto>), 200)]
        [ProducesResponseType(typeof(BaseResposeDto), 400)]
        public async Task<ActionResult<List<GuideDto>>> GetAvailableGuides(
            [FromQuery] DateOnly date,
            [FromQuery] Guid? excludeOperationId = null)
        {
            try
            {
                _logger.LogInformation("Getting available guides for date {Date}", date);

                if (date < DateOnly.FromDateTime(DateTime.Today))
                {
                    return BadRequest(new BaseResposeDto
                    {
                        success = false,
                        Message = "Không thể chọn ngày trong quá khứ"
                    });
                }

                // 1. Lấy tất cả tour guides active
                var allTourGuides = await _unitOfWork.TourGuideRepository.GetAllWithUserAsync();
                var activeTourGuides = allTourGuides.Where(tg => tg.IsActive && tg.User.IsActive && tg.IsAvailable).ToList();

                // 2. Lấy các operations đã có trong ngày đó
                var existingOperations = await _unitOfWork.TourOperationRepository
                    .GetOperationsByDateAsync(date);

                // 3. Loại trừ operation đang update (nếu có)
                if (excludeOperationId.HasValue)
                {
                    existingOperations = existingOperations
                        .Where(op => op.Id != excludeOperationId.Value)
                        .ToList();
                }

                // 4. Lấy danh sách tour guide IDs đã busy (TourOperation.TourGuideId references TourGuides.Id)
                var busyTourGuideIds = existingOperations
                    .Where(op => op.TourGuideId.HasValue)
                    .Select(op => op.TourGuideId.Value)
                    .ToHashSet();

                // 5. Filter available guides
                var availableGuides = activeTourGuides
                    .Where(tourGuide => !busyTourGuideIds.Contains(tourGuide.Id))
                    .Select(tourGuide => new GuideDto
                    {
                        Id = tourGuide.Id, // Use TourGuide.Id for backend compatibility
                        FullName = tourGuide.FullName,
                        Email = tourGuide.Email,
                        PhoneNumber = tourGuide.PhoneNumber,
                        IsActive = tourGuide.IsActive && tourGuide.User.IsActive,
                        IsAvailable = tourGuide.IsAvailable,
                        ExperienceYears = CalculateExperienceYears(tourGuide.Experience),
                        Specialization = tourGuide.Skills,
                        AverageRating = tourGuide.Rating,
                        CompletedTours = tourGuide.TotalToursGuided,
                        JoinedDate = tourGuide.ApprovedAt != default ? tourGuide.ApprovedAt : tourGuide.CreatedAt,
                        CurrentStatus = "Available"
                    })
                    .OrderBy(g => g.FullName)
                    .ToList();

                _logger.LogInformation("Found {Count} available guides for date {Date}", availableGuides.Count, date.ToString());
                return Ok(availableGuides);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available guides for date {Date}", date.ToString());
                return StatusCode(500, new BaseResposeDto
                {
                    success = false,
                    Message = "Lỗi hệ thống khi lấy danh sách hướng dẫn viên"
                });
            }
        }

        /// <summary>
        /// Debug endpoint để test CurrentUserService
        /// </summary>
        [HttpGet("debug/current-user")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DebugCurrentUser()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                var isAuthenticated = _currentUserService.IsAuthenticated();
                var userEmail = _currentUserService.GetCurrentUserEmail();
                var userName = _currentUserService.GetCurrentUserName();
                var roleId = _currentUserService.GetCurrentUserRoleId();

                var currentUser = await _currentUserService.GetCurrentUserAsync();

                return Ok(new
                {
                    UserId = userId,
                    IsAuthenticated = isAuthenticated,
                    Email = userEmail,
                    Name = userName,
                    RoleId = roleId,
                    CurrentUser = currentUser,
                    Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }


        // ===== NEW SPECIALTY SHOP APPLICATION ENDPOINTS =====



        /// <summary>
        /// User nộp đơn đăng ký Specialty Shop (NEW FLOW)
        /// </summary>
        [HttpPost("test-specialty-shop-valid")]
        public async Task<IActionResult> TestSpecialtyShopValid([FromForm] SubmitSpecialtyShopApplicationDto dto)
        {
            try
            {
                // Validate model state first
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Validation failed",
                        Errors = errors,
                        Success = false
                    });
                }

                // Mock successful response with detailed data
                var mockResponse = new
                {
                    StatusCode = 201,
                    Message = "Specialty shop application submitted successfully",
                    Success = true,
                    Data = new
                    {
                        ApplicationId = Guid.NewGuid(),
                        ShopName = dto.ShopName,
                        Location = dto.Location,
                        PhoneNumber = dto.PhoneNumber,
                        Email = dto.Email,
                        Website = dto.Website,
                        ShopType = dto.ShopType,
                        ShopDescription = dto.ShopDescription,
                        OpeningHours = dto.OpeningHours,
                        ClosingHours = dto.ClosingHours,
                        RepresentativeName = dto.RepresentativeName,
                        Status = "Pending", // 0 = Pending
                        SubmittedAt = DateTime.UtcNow,
                        ProcessedAt = (DateTime?)null,
                        RejectionReason = (string?)null,
                        Files = new
                        {
                            BusinessLicenseFile = new
                            {
                                FileName = dto.BusinessLicenseFile?.FileName,
                                FileSize = dto.BusinessLicenseFile?.Length,
                                ContentType = dto.BusinessLicenseFile?.ContentType,
                                MockUrl = $"https://api.tayninhtour.com/files/business-license/{Guid.NewGuid()}.pdf"
                            },
                            Logo = new
                            {
                                FileName = dto.Logo?.FileName,
                                FileSize = dto.Logo?.Length,
                                ContentType = dto.Logo?.ContentType,
                                MockUrl = $"https://api.tayninhtour.com/files/logos/{Guid.NewGuid()}.png"
                            }
                        },
                        ValidationSummary = new
                        {
                            AllRequiredFieldsProvided = true,
                            FilesValidated = true,
                            TimeFormatValid = !string.IsNullOrEmpty(dto.OpeningHours) || !string.IsNullOrEmpty(dto.ClosingHours),
                            EmailFormatValid = true,
                            PhoneFormatValid = true,
                            WebsiteFormatValid = !string.IsNullOrEmpty(dto.Website)
                        }
                    }
                };

                return StatusCode(201, mockResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing specialty shop valid case");
                return StatusCode(500, new {
                    StatusCode = 500,
                    Message = "Internal server error",
                    Error = ex.Message,
                    Success = false
                });
            }
        }





        /// <summary>
        /// User nộp đơn đăng ký Specialty Shop (NEW FLOW)
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("specialty-shop-application")]
        public async Task<IActionResult> SubmitSpecialtyShopApplication([FromForm] SubmitSpecialtyShopApplicationDto dto)
        {
            try
            {
                // Validate model state first
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = errors
                    });
                }

                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var result = await _specialtyShopApplicationService.SubmitApplicationAsync(dto, currentUser);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting specialty shop application");
                return StatusCode(500, new { Error = "An error occurred while submitting application", Details = ex.Message });
            }
        }

        /// <summary>
        /// User xem trạng thái đơn đăng ký Specialty Shop của mình (NEW FLOW)
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("my-specialty-shop-application")]
        public async Task<IActionResult> GetMySpecialtyShopApplication()
        {
            try
            {
                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var application = await _specialtyShopApplicationService.GetMyApplicationAsync(currentUser);

                if (application == null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        Message = "No specialty shop application found",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<SpecialtyShopApplicationDto>
                {
                    success = true,
                    Message = "Specialty shop application retrieved successfully",
                    Data = application
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving specialty shop application");
                return StatusCode(500, new { Error = "An error occurred while retrieving application", Details = ex.Message });
            }
        }

        // ===== TOUR GUIDE APPLICATION ENDPOINTS =====

        /// <summary>
        /// User nộp đơn đăng ký TourGuide với file upload
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("tourguide-application/upload")]
        public async Task<IActionResult> SubmitTourGuideApplicationWithUpload([FromForm] SubmitTourGuideApplicationDto dto)
        {
            try
            {
                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var result = await _tourGuideApplicationService.SubmitApplicationAsync(dto, currentUser);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting tour guide application with file upload");
                return StatusCode(500, new { Error = "An error occurred while submitting application", Details = ex.Message });
            }
        }



        /// <summary>
        /// User xem danh sách đơn đăng ký TourGuide của mình
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("my-tourguide-applications")]
        public async Task<IActionResult> GetMyTourGuideApplications()
        {
            try
            {
                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var applications = await _tourGuideApplicationService.GetMyApplicationsAsync(currentUser.Id);

                return Ok(new ApiResponse<IEnumerable<TourGuideApplicationSummaryDto>>
                {
                    success = true,
                    Message = "Tour guide applications retrieved successfully",
                    Data = applications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tour guide applications");
                return StatusCode(500, new { Error = "An error occurred while retrieving applications", Details = ex.Message });
            }
        }

        /// <summary>
        /// User xem chi tiết đơn đăng ký TourGuide của mình
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("my-tourguide-application/{applicationId}")]
        public async Task<IActionResult> GetMyTourGuideApplication(Guid applicationId)
        {
            try
            {
                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var application = await _tourGuideApplicationService.GetMyApplicationByIdAsync(applicationId, currentUser.Id);

                if (application == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        success = false,
                        Message = "Tour guide application not found or you don't have permission to view it",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<TourGuideApplicationDto>
                {
                    success = true,
                    Message = "Tour guide application retrieved successfully",
                    Data = application
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tour guide application {ApplicationId}", applicationId);
                return StatusCode(500, new { Error = "An error occurred while retrieving application", Details = ex.Message });
            }
        }

        /// <summary>
        /// Download CV file của tour guide application
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("tourguide-application/{applicationId}/cv")]
        public async Task<IActionResult> DownloadCvFile(Guid applicationId)
        {
            try
            {
                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);

                // Get application to check permissions
                TourGuideApplicationDto? application = null;

                // Check if user is admin or the owner of the application
                // Get user's role to check permissions
                var user = await _unitOfWork.UserRepository.GetByIdAsync(currentUser.Id, new[] { "Role" });
                if (user?.Role?.Name == Constants.RoleAdminName)
                {
                    application = await _tourGuideApplicationService.GetApplicationByIdAsync(applicationId);
                }
                else
                {
                    application = await _tourGuideApplicationService.GetMyApplicationByIdAsync(applicationId, currentUser.Id);
                }

                if (application == null)
                {
                    return NotFound(new { Error = "Application not found or access denied" });
                }

                if (string.IsNullOrEmpty(application.CvFilePath))
                {
                    return NotFound(new { Error = "CV file not found" });
                }

                // Get file path
                var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fullPath = Path.Combine(webRoot, application.CvFilePath.Replace("/", "\\"));

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(new { Error = "CV file not found on server" });
                }

                // Return file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                var contentType = application.CvContentType ?? "application/octet-stream";
                var fileName = application.CvOriginalFileName ?? "cv.pdf";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading CV file for application {ApplicationId}", applicationId);
                return StatusCode(500, new { Error = "An error occurred while downloading the file" });
            }
        }

        /// <summary>
        /// Kiểm tra user có thể nộp đơn TourGuide mới không
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("can-submit-tourguide-application")]
        public async Task<IActionResult> CanSubmitTourGuideApplication()
        {
            try
            {
                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var canSubmit = await _tourGuideApplicationService.CanSubmitNewApplicationAsync(currentUser.Id);

                return Ok(new ApiResponse<object>
                {
                    success = true,
                    Message = canSubmit ? "You can submit a new application" : "You already have an active application",
                    Data = new { CanSubmit = canSubmit }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tour guide application eligibility");
                return StatusCode(500, new { Error = "An error occurred while checking eligibility", Details = ex.Message });
            }
        }


    }
}
