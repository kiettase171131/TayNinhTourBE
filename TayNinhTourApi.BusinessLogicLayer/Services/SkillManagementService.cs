using TayNinhTourApi.BusinessLogicLayer.DTOs.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service để quản lý skills system
    /// Cung cấp APIs để lấy danh sách skills, validate skills, và skill matching
    /// </summary>
    public class SkillManagementService : ISkillManagementService
    {
        /// <summary>
        /// Lấy tất cả skills available được nhóm theo category
        /// </summary>
        /// <returns>Skills grouped by categories</returns>
        public async Task<ApiResponse<SkillCategoriesDto>> GetSkillCategoriesAsync()
        {
            try
            {
                var skillCategories = new SkillCategoriesDto();

                // Languages
                skillCategories.Languages = TourGuideSkillUtility.SkillCategories.Languages
                    .Select(skill => new SkillInfoDto
                    {
                        Skill = skill,
                        DisplayName = TourGuideSkillUtility.GetDisplayName(skill),
                        EnglishName = skill.ToString(),
                        Category = "Ngôn ngữ"
                    }).ToList();

                // Knowledge
                skillCategories.Knowledge = TourGuideSkillUtility.SkillCategories.Knowledge
                    .Select(skill => new SkillInfoDto
                    {
                        Skill = skill,
                        DisplayName = TourGuideSkillUtility.GetDisplayName(skill),
                        EnglishName = skill.ToString(),
                        Category = "Kiến thức chuyên môn"
                    }).ToList();

                // Activities
                skillCategories.Activities = TourGuideSkillUtility.SkillCategories.Activities
                    .Select(skill => new SkillInfoDto
                    {
                        Skill = skill,
                        DisplayName = TourGuideSkillUtility.GetDisplayName(skill),
                        EnglishName = skill.ToString(),
                        Category = "Kỹ năng hoạt động"
                    }).ToList();

                // Special
                skillCategories.Special = TourGuideSkillUtility.SkillCategories.Special
                    .Select(skill => new SkillInfoDto
                    {
                        Skill = skill,
                        DisplayName = TourGuideSkillUtility.GetDisplayName(skill),
                        EnglishName = skill.ToString(),
                        Category = "Kỹ năng đặc biệt"
                    }).ToList();

                await Task.CompletedTask; // For async consistency

                return new ApiResponse<SkillCategoriesDto>
                {
                    success = true,
                    Message = "Lấy danh sách skills thành công",
                    Data = skillCategories,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SkillCategoriesDto>
                {
                    success = false,
                    Message = $"Có lỗi xảy ra khi lấy danh sách skills: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả skills dưới dạng flat list
        /// </summary>
        /// <returns>All skills as flat list</returns>
        public async Task<ApiResponse<List<SkillInfoDto>>> GetAllSkillsAsync()
        {
            try
            {
                var allSkills = new List<SkillInfoDto>();

                // Get all skills from all categories
                var skillsByCategory = TourGuideSkillUtility.GetSkillsByCategory();
                
                foreach (var category in skillsByCategory)
                {
                    var categorySkills = category.Value.Select(skill => new SkillInfoDto
                    {
                        Skill = skill,
                        DisplayName = TourGuideSkillUtility.GetDisplayName(skill),
                        EnglishName = skill.ToString(),
                        Category = category.Key
                    });
                    
                    allSkills.AddRange(categorySkills);
                }

                await Task.CompletedTask; // For async consistency

                return new ApiResponse<List<SkillInfoDto>>
                {
                    success = true,
                    Message = "Lấy danh sách tất cả skills thành công",
                    Data = allSkills.OrderBy(s => s.Category).ThenBy(s => s.DisplayName).ToList(),
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<SkillInfoDto>>
                {
                    success = false,
                    Message = $"Có lỗi xảy ra khi lấy danh sách skills: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Validate skills string format
        /// </summary>
        /// <param name="skillsString">Skills string to validate</param>
        /// <returns>Validation result</returns>
        public async Task<ApiResponse<bool>> ValidateSkillsStringAsync(string skillsString)
        {
            try
            {
                var isValid = TourGuideSkillUtility.IsValidSkillsString(skillsString);
                
                await Task.CompletedTask; // For async consistency

                return new ApiResponse<bool>
                {
                    success = true,
                    Message = isValid ? "Skills string hợp lệ" : "Skills string không hợp lệ",
                    Data = isValid,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    success = false,
                    Message = $"Có lỗi xảy ra khi validate skills: {ex.Message}",
                    Data = false,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Convert skills list to comma-separated string
        /// </summary>
        /// <param name="skills">List of skills</param>
        /// <returns>Comma-separated skills string</returns>
        public async Task<ApiResponse<string>> ConvertSkillsToStringAsync(List<TourGuideSkill> skills)
        {
            try
            {
                var skillsString = TourGuideSkillUtility.SkillsToString(skills);
                
                await Task.CompletedTask; // For async consistency

                return new ApiResponse<string>
                {
                    success = true,
                    Message = "Chuyển đổi skills thành công",
                    Data = skillsString,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    success = false,
                    Message = $"Có lỗi xảy ra khi chuyển đổi skills: {ex.Message}",
                    Data = string.Empty,
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Convert skills string to list of skills
        /// </summary>
        /// <param name="skillsString">Comma-separated skills string</param>
        /// <returns>List of skills</returns>
        public async Task<ApiResponse<List<TourGuideSkill>>> ConvertStringToSkillsAsync(string skillsString)
        {
            try
            {
                var skills = TourGuideSkillUtility.StringToSkills(skillsString);
                
                await Task.CompletedTask; // For async consistency

                return new ApiResponse<List<TourGuideSkill>>
                {
                    success = true,
                    Message = "Chuyển đổi skills string thành công",
                    Data = skills,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<TourGuideSkill>>
                {
                    success = false,
                    Message = $"Có lỗi xảy ra khi chuyển đổi skills string: {ex.Message}",
                    Data = new List<TourGuideSkill>(),
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Calculate skill match score between required skills and guide skills
        /// </summary>
        /// <param name="requiredSkills">Required skills string</param>
        /// <param name="guideSkills">Guide skills string</param>
        /// <returns>Match score (0.0 to 1.0)</returns>
        public async Task<ApiResponse<double>> CalculateSkillMatchScoreAsync(string requiredSkills, string guideSkills)
        {
            try
            {
                var matchScore = SkillsMatchingUtility.CalculateMatchScoreEnhanced(requiredSkills, guideSkills);
                
                await Task.CompletedTask; // For async consistency

                return new ApiResponse<double>
                {
                    success = true,
                    Message = "Tính toán match score thành công",
                    Data = matchScore,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<double>
                {
                    success = false,
                    Message = $"Có lỗi xảy ra khi tính toán match score: {ex.Message}",
                    Data = 0.0,
                    StatusCode = 500
                };
            }
        }
    }
}
