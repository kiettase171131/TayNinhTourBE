using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller để quản lý skills system
    /// Cung cấp APIs cho skill selection, validation và skill matching
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly ISkillManagementService _skillManagementService;

        public SkillController(ISkillManagementService skillManagementService)
        {
            _skillManagementService = skillManagementService;
        }

        /// <summary>
        /// Lấy tất cả skills available được nhóm theo category
        /// </summary>
        /// <returns>Skills grouped by categories</returns>
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<SkillCategoriesDto>>> GetSkillCategories()
        {
            var result = await _skillManagementService.GetSkillCategoriesAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách tất cả skills dưới dạng flat list
        /// </summary>
        /// <returns>All skills as flat list</returns>
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<SkillInfoDto>>>> GetAllSkills()
        {
            var result = await _skillManagementService.GetAllSkillsAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Validate skills string format
        /// </summary>
        /// <param name="skillsString">Skills string to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateSkillsString([FromBody] string skillsString)
        {
            var result = await _skillManagementService.ValidateSkillsStringAsync(skillsString);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Convert skills list to comma-separated string
        /// </summary>
        /// <param name="skills">List of skills</param>
        /// <returns>Comma-separated skills string</returns>
        [HttpPost("convert-to-string")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> ConvertSkillsToString([FromBody] List<TourGuideSkill> skills)
        {
            var result = await _skillManagementService.ConvertSkillsToStringAsync(skills);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Convert skills string to list of skills
        /// </summary>
        /// <param name="skillsString">Comma-separated skills string</param>
        /// <returns>List of skills</returns>
        [HttpPost("convert-from-string")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<TourGuideSkill>>>> ConvertStringToSkills([FromBody] string skillsString)
        {
            var result = await _skillManagementService.ConvertStringToSkillsAsync(skillsString);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Calculate skill match score between required skills and guide skills
        /// </summary>
        /// <param name="request">Skill match request</param>
        /// <returns>Match score (0.0 to 1.0)</returns>
        [HttpPost("calculate-match-score")]
        [Authorize(Roles = "Admin,Tour Company")]
        public async Task<ActionResult<ApiResponse<double>>> CalculateSkillMatchScore([FromBody] SkillMatchRequest request)
        {
            var result = await _skillManagementService.CalculateSkillMatchScoreAsync(request.RequiredSkills, request.GuideSkills);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get skill information by skill enum value
        /// </summary>
        /// <param name="skill">TourGuideSkill enum value</param>
        /// <returns>Skill information</returns>
        [HttpGet("info/{skill}")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<SkillInfoDto>> GetSkillInfo(TourGuideSkill skill)
        {
            try
            {
                var skillInfo = new SkillInfoDto
                {
                    Skill = skill,
                    DisplayName = TayNinhTourApi.BusinessLogicLayer.Utilities.TourGuideSkillUtility.GetDisplayName(skill),
                    EnglishName = skill.ToString(),
                    Category = GetSkillCategory(skill)
                };

                return Ok(new ApiResponse<SkillInfoDto>
                {
                    IsSuccess = true,
                    Message = "Lấy thông tin skill thành công",
                    Data = skillInfo,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<SkillInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Validate skills string (Frontend friendly endpoint)
        /// </summary>
        /// <param name="skillsString">Comma-separated skills string</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate-string")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateSkillsStringDetailed([FromBody] string skillsString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(skillsString))
                {
                    return Ok(new ApiResponse<bool>
                    {
                        IsSuccess = true,
                        Message = "Skills string trống - hợp lệ",
                        Data = true,
                        StatusCode = 200
                    });
                }

                var isValid = TourGuideSkillUtility.IsValidSkillsString(skillsString);

                return Ok(new ApiResponse<bool>
                {
                    IsSuccess = true,
                    Message = isValid ? "Skills string hợp lệ" : "Skills string chứa giá trị không hợp lệ",
                    Data = isValid,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = $"Có lỗi xảy ra khi validate skills string: {ex.Message}",
                    Data = false,
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Get all available skill names as strings (Frontend utility endpoint)
        /// </summary>
        /// <returns>List of skill names</returns>
        [HttpGet("names")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetSkillNames()
        {
            try
            {
                var allSkills = Enum.GetValues<TourGuideSkill>().ToList();
                var skillNames = allSkills.Select(skill => skill.ToString()).ToList();

                return Ok(new ApiResponse<List<string>>
                {
                    IsSuccess = true,
                    Message = "Lấy danh sách tên skills thành công",
                    Data = skillNames,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    Message = $"Có lỗi xảy ra khi lấy danh sách skill names: {ex.Message}",
                    Data = new List<string>(),
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Helper method để xác định category của skill
        /// </summary>
        private static string GetSkillCategory(TourGuideSkill skill)
        {
            if (TayNinhTourApi.BusinessLogicLayer.Utilities.TourGuideSkillUtility.SkillCategories.Languages.Contains(skill))
                return "Ngôn ngữ";
            if (TayNinhTourApi.BusinessLogicLayer.Utilities.TourGuideSkillUtility.SkillCategories.Knowledge.Contains(skill))
                return "Kiến thức chuyên môn";
            if (TayNinhTourApi.BusinessLogicLayer.Utilities.TourGuideSkillUtility.SkillCategories.Activities.Contains(skill))
                return "Kỹ năng hoạt động";
            if (TayNinhTourApi.BusinessLogicLayer.Utilities.TourGuideSkillUtility.SkillCategories.Special.Contains(skill))
                return "Kỹ năng đặc biệt";

            return "Khác";
        }
    }

    /// <summary>
    /// Request DTO for skill matching calculation
    /// </summary>
    public class SkillMatchRequest
    {
        /// <summary>
        /// Required skills string
        /// </summary>
        public string RequiredSkills { get; set; } = null!;

        /// <summary>
        /// Guide skills string
        /// </summary>
        public string GuideSkills { get; set; } = null!;
    }
}
