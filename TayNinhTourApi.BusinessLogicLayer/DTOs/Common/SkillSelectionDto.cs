using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Common
{
    /// <summary>
    /// DTO for skill selection in tour guide applications and tour requirements
    /// </summary>
    public class SkillSelectionDto
    {
        /// <summary>
        /// Danh sách kỹ năng được chọn
        /// </summary>
        [Required(ErrorMessage = "Ít nhất một kỹ năng phải được chọn")]
        [MinLength(1, ErrorMessage = "Ít nhất một kỹ năng phải được chọn")]
        public List<TourGuideSkill> Skills { get; set; } = new();

        /// <summary>
        /// Kỹ năng dưới dạng comma-separated string (for backward compatibility)
        /// </summary>
        public string? SkillsString { get; set; }
    }

    /// <summary>
    /// DTO for displaying skill information
    /// </summary>
    public class SkillInfoDto
    {
        /// <summary>
        /// Skill enum value
        /// </summary>
        public TourGuideSkill Skill { get; set; }

        /// <summary>
        /// Tên hiển thị tiếng Việt
        /// </summary>
        public string DisplayName { get; set; } = null!;

        /// <summary>
        /// Tên tiếng Anh (enum name)
        /// </summary>
        public string EnglishName { get; set; } = null!;

        /// <summary>
        /// Danh mục skill (Languages, Knowledge, Activities, Special)
        /// </summary>
        public string Category { get; set; } = null!;
    }

    /// <summary>
    /// DTO for skill categories and available skills
    /// </summary>
    public class SkillCategoriesDto
    {
        /// <summary>
        /// Ngôn ngữ
        /// </summary>
        public List<SkillInfoDto> Languages { get; set; } = new();

        /// <summary>
        /// Kiến thức chuyên môn
        /// </summary>
        public List<SkillInfoDto> Knowledge { get; set; } = new();

        /// <summary>
        /// Kỹ năng hoạt động
        /// </summary>
        public List<SkillInfoDto> Activities { get; set; } = new();

        /// <summary>
        /// Kỹ năng đặc biệt
        /// </summary>
        public List<SkillInfoDto> Special { get; set; } = new();
    }

    /// <summary>
    /// Validation attribute for skill selection
    /// </summary>
    public class ValidSkillSelectionAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is List<TourGuideSkill> skills)
            {
                // Must have at least one skill
                if (!skills.Any())
                {
                    ErrorMessage = "Ít nhất một kỹ năng phải được chọn";
                    return false;
                }

                // Check for valid enum values
                foreach (var skill in skills)
                {
                    if (!Enum.IsDefined(typeof(TourGuideSkill), skill))
                    {
                        ErrorMessage = $"Kỹ năng '{skill}' không hợp lệ";
                        return false;
                    }
                }

                return true;
            }

            if (value is string skillsString)
            {
                if (string.IsNullOrWhiteSpace(skillsString))
                {
                    ErrorMessage = "Ít nhất một kỹ năng phải được chọn";
                    return false;
                }

                // Validate skills string format
                var skillsList = TayNinhTourApi.BusinessLogicLayer.Utilities.TourGuideSkillUtility.StringToSkills(skillsString);
                if (!skillsList.Any())
                {
                    ErrorMessage = "Định dạng kỹ năng không hợp lệ";
                    return false;
                }

                return true;
            }

            ErrorMessage = "Kỹ năng phải được cung cấp";
            return false;
        }
    }
}
