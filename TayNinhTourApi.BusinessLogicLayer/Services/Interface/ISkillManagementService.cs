using TayNinhTourApi.BusinessLogicLayer.DTOs.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Interface cho SkillManagementService
    /// Quản lý skills system và skill matching operations
    /// </summary>
    public interface ISkillManagementService
    {
        /// <summary>
        /// Lấy tất cả skills available được nhóm theo category
        /// </summary>
        /// <returns>Skills grouped by categories</returns>
        Task<ApiResponse<SkillCategoriesDto>> GetSkillCategoriesAsync();

        /// <summary>
        /// Lấy danh sách tất cả skills dưới dạng flat list
        /// </summary>
        /// <returns>All skills as flat list</returns>
        Task<ApiResponse<List<SkillInfoDto>>> GetAllSkillsAsync();

        /// <summary>
        /// Validate skills string format
        /// </summary>
        /// <param name="skillsString">Skills string to validate</param>
        /// <returns>Validation result</returns>
        Task<ApiResponse<bool>> ValidateSkillsStringAsync(string skillsString);

        /// <summary>
        /// Convert skills list to comma-separated string
        /// </summary>
        /// <param name="skills">List of skills</param>
        /// <returns>Comma-separated skills string</returns>
        Task<ApiResponse<string>> ConvertSkillsToStringAsync(List<TourGuideSkill> skills);

        /// <summary>
        /// Convert skills string to list of skills
        /// </summary>
        /// <param name="skillsString">Comma-separated skills string</param>
        /// <returns>List of skills</returns>
        Task<ApiResponse<List<TourGuideSkill>>> ConvertStringToSkillsAsync(string skillsString);

        /// <summary>
        /// Calculate skill match score between required skills and guide skills
        /// </summary>
        /// <param name="requiredSkills">Required skills string</param>
        /// <param name="guideSkills">Guide skills string</param>
        /// <returns>Match score (0.0 to 1.0)</returns>
        Task<ApiResponse<double>> CalculateSkillMatchScoreAsync(string requiredSkills, string guideSkills);
    }
}
