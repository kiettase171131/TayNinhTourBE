using Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

        public AccountController(
            IAccountService accountService,
            ITourGuideApplicationService tourGuideApplicationService,
            IBlogReactionService blogReactionService,
            IUnitOfWork unitOfWork,
            ILogger<AccountController> logger,
            ICurrentUserService currentUserService)
        {
            _accountService = accountService;
            _tourGuideApplicationService = tourGuideApplicationService;
            _reactionService = blogReactionService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("tourguide-application")]

        public async Task<IActionResult> Submit([FromForm] SubmitApplicationDto submitApplicationDto)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _tourGuideApplicationService.SubmitAsync(submitApplicationDto, currentUserObject);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("View-tourguideapplication")]
        public async Task<IActionResult> ListMyApplications()
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var list = await _tourGuideApplicationService.ListByUserAsync(currentUserObject.Id);
            return Ok(list);
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

                // Lấy users có role Guide
                var guides = await _unitOfWork.UserRepository.GetUsersByRoleAsync("Guide");

                if (!includeInactive)
                {
                    guides = guides.Where(g => g.IsActive).ToList();
                }

                var guideDtos = guides.Select(guide => new GuideDto
                {
                    Id = guide.Id,
                    FullName = guide.Name,
                    Email = guide.Email,
                    PhoneNumber = guide.PhoneNumber,
                    IsActive = guide.IsActive,
                    IsAvailable = true, // Default, sẽ check chi tiết ở endpoint khác
                    ExperienceYears = 0, // TODO: Implement when User entity has these fields
                    Specialization = null,
                    AverageRating = null,
                    CompletedTours = 0,
                    JoinedDate = guide.CreatedAt,
                    CurrentStatus = guide.IsActive ? "Available" : "Inactive"
                }).OrderBy(g => g.FullName).ToList();

                _logger.LogInformation("Found {Count} guides", guideDtos.Count);
                return Ok(guideDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guides list");
                return StatusCode(500, new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Lỗi hệ thống khi lấy danh sách hướng dẫn viên"
                });
            }
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
                        IsSuccess = false,
                        Message = "Không thể chọn ngày trong quá khứ"
                    });
                }

                // 1. Lấy tất cả guides active
                var allGuides = await _unitOfWork.UserRepository.GetUsersByRoleAsync("Guide");
                var activeGuides = allGuides.Where(g => g.IsActive).ToList();

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

                // 4. Lấy danh sách guide IDs đã busy
                var busyGuideIds = existingOperations
                    .Where(op => op.GuideId.HasValue)
                    .Select(op => op.GuideId.Value)
                    .ToHashSet();

                // 5. Filter available guides
                var availableGuides = activeGuides
                    .Where(guide => !busyGuideIds.Contains(guide.Id))
                    .Select(guide => new GuideDto
                    {
                        Id = guide.Id,
                        FullName = guide.Name,
                        Email = guide.Email,
                        PhoneNumber = guide.PhoneNumber,
                        IsActive = guide.IsActive,
                        IsAvailable = true,
                        ExperienceYears = 0, // TODO: Implement when User entity has these fields
                        Specialization = null,
                        AverageRating = null,
                        CompletedTours = 0,
                        JoinedDate = guide.CreatedAt,
                        CurrentStatus = "Available"
                    })
                    .OrderBy(g => g.FullName)
                    .ToList();

                _logger.LogInformation("Found {Count} available guides for {Date}", availableGuides.Count, date);
                return Ok(availableGuides);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available guides for date {Date}", date);
                return StatusCode(500, new BaseResposeDto
                {
                    IsSuccess = false,
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
    }
}
