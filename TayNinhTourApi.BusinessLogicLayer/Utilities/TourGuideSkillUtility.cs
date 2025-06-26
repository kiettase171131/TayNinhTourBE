using System.ComponentModel;
using System.Reflection;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// Utility class để làm việc với TourGuideSkill enum
    /// Cung cấp các phương thức chuyển đổi, validation và mapping
    /// </summary>
    public static class TourGuideSkillUtility
    {
        /// <summary>
        /// Dictionary mapping skill enum values to Vietnamese display names
        /// </summary>
        private static readonly Dictionary<TourGuideSkill, string> SkillDisplayNames = new()
        {
            // Languages
            [TourGuideSkill.Vietnamese] = "Tiếng Việt",
            [TourGuideSkill.English] = "Tiếng Anh", 
            [TourGuideSkill.Chinese] = "Tiếng Trung",
            [TourGuideSkill.Japanese] = "Tiếng Nhật",
            [TourGuideSkill.Korean] = "Tiếng Hàn",
            [TourGuideSkill.French] = "Tiếng Pháp",
            [TourGuideSkill.German] = "Tiếng Đức",
            [TourGuideSkill.Russian] = "Tiếng Nga",

            // Knowledge Areas
            [TourGuideSkill.History] = "Lịch sử",
            [TourGuideSkill.Culture] = "Văn hóa",
            [TourGuideSkill.Religion] = "Tôn giáo",
            [TourGuideSkill.Cuisine] = "Ẩm thực",
            [TourGuideSkill.Geography] = "Địa lý",
            [TourGuideSkill.Nature] = "Thiên nhiên",
            [TourGuideSkill.Arts] = "Nghệ thuật",
            [TourGuideSkill.Architecture] = "Kiến trúc",

            // Activity Skills
            [TourGuideSkill.MountainClimbing] = "Leo núi",
            [TourGuideSkill.Trekking] = "Trekking",
            [TourGuideSkill.Photography] = "Chụp ảnh",
            [TourGuideSkill.WaterSports] = "Thể thao nước",
            [TourGuideSkill.Cycling] = "Đi xe đạp",
            [TourGuideSkill.Camping] = "Cắm trại",
            [TourGuideSkill.BirdWatching] = "Quan sát chim",
            [TourGuideSkill.AdventureSports] = "Thể thao mạo hiểm",

            // Special Skills
            [TourGuideSkill.FirstAid] = "Sơ cứu",
            [TourGuideSkill.Driving] = "Lái xe",
            [TourGuideSkill.Cooking] = "Nấu ăn",
            [TourGuideSkill.Meditation] = "Hướng dẫn thiền",
            [TourGuideSkill.TraditionalMassage] = "Massage truyền thống"
        };

        /// <summary>
        /// Dictionary mapping old language codes to new skill enums for backward compatibility
        /// </summary>
        private static readonly Dictionary<string, TourGuideSkill> LegacyLanguageMapping = new(StringComparer.OrdinalIgnoreCase)
        {
            ["vietnamese"] = TourGuideSkill.Vietnamese,
            ["vn"] = TourGuideSkill.Vietnamese,
            ["vi"] = TourGuideSkill.Vietnamese,
            ["tiếng việt"] = TourGuideSkill.Vietnamese,
            ["tieng viet"] = TourGuideSkill.Vietnamese,
            ["việt nam"] = TourGuideSkill.Vietnamese,
            ["viet nam"] = TourGuideSkill.Vietnamese,

            ["english"] = TourGuideSkill.English,
            ["en"] = TourGuideSkill.English,
            ["tiếng anh"] = TourGuideSkill.English,
            ["tieng anh"] = TourGuideSkill.English,

            ["chinese"] = TourGuideSkill.Chinese,
            ["cn"] = TourGuideSkill.Chinese,
            ["zh"] = TourGuideSkill.Chinese,
            ["tiếng trung"] = TourGuideSkill.Chinese,
            ["tieng trung"] = TourGuideSkill.Chinese,
            ["mandarin"] = TourGuideSkill.Chinese,

            ["japanese"] = TourGuideSkill.Japanese,
            ["jp"] = TourGuideSkill.Japanese,
            ["ja"] = TourGuideSkill.Japanese,
            ["tiếng nhật"] = TourGuideSkill.Japanese,
            ["tieng nhat"] = TourGuideSkill.Japanese,

            ["korean"] = TourGuideSkill.Korean,
            ["kr"] = TourGuideSkill.Korean,
            ["ko"] = TourGuideSkill.Korean,
            ["tiếng hàn"] = TourGuideSkill.Korean,
            ["tieng han"] = TourGuideSkill.Korean,

            ["french"] = TourGuideSkill.French,
            ["fr"] = TourGuideSkill.French,
            ["tiếng pháp"] = TourGuideSkill.French,
            ["tieng phap"] = TourGuideSkill.French
        };

        /// <summary>
        /// Skill categories for grouping
        /// </summary>
        public static class SkillCategories
        {
            public static readonly List<TourGuideSkill> Languages = new()
            {
                TourGuideSkill.Vietnamese, TourGuideSkill.English, TourGuideSkill.Chinese,
                TourGuideSkill.Japanese, TourGuideSkill.Korean, TourGuideSkill.French,
                TourGuideSkill.German, TourGuideSkill.Russian
            };

            public static readonly List<TourGuideSkill> Knowledge = new()
            {
                TourGuideSkill.History, TourGuideSkill.Culture, TourGuideSkill.Religion,
                TourGuideSkill.Cuisine, TourGuideSkill.Geography, TourGuideSkill.Nature,
                TourGuideSkill.Arts, TourGuideSkill.Architecture
            };

            public static readonly List<TourGuideSkill> Activities = new()
            {
                TourGuideSkill.MountainClimbing, TourGuideSkill.Trekking, TourGuideSkill.Photography,
                TourGuideSkill.WaterSports, TourGuideSkill.Cycling, TourGuideSkill.Camping,
                TourGuideSkill.BirdWatching, TourGuideSkill.AdventureSports
            };

            public static readonly List<TourGuideSkill> Special = new()
            {
                TourGuideSkill.FirstAid, TourGuideSkill.Driving, TourGuideSkill.Cooking,
                TourGuideSkill.Meditation, TourGuideSkill.TraditionalMassage
            };
        }

        /// <summary>
        /// Lấy tên hiển thị tiếng Việt của skill
        /// </summary>
        /// <param name="skill">Skill enum</param>
        /// <returns>Tên hiển thị tiếng Việt</returns>
        public static string GetDisplayName(TourGuideSkill skill)
        {
            return SkillDisplayNames.TryGetValue(skill, out var displayName) 
                ? displayName 
                : skill.ToString();
        }

        /// <summary>
        /// Chuyển đổi danh sách skills thành comma-separated string
        /// </summary>
        /// <param name="skills">Danh sách skills</param>
        /// <returns>Comma-separated string</returns>
        public static string SkillsToString(IEnumerable<TourGuideSkill> skills)
        {
            if (skills == null || !skills.Any())
                return string.Empty;

            return string.Join(",", skills.Select(s => s.ToString()));
        }

        /// <summary>
        /// Chuyển đổi comma-separated string thành danh sách skills
        /// </summary>
        /// <param name="skillsString">Comma-separated string</param>
        /// <returns>Danh sách skills</returns>
        public static List<TourGuideSkill> StringToSkills(string? skillsString)
        {
            if (string.IsNullOrWhiteSpace(skillsString))
                return new List<TourGuideSkill>();

            var skills = new List<TourGuideSkill>();
            var skillNames = skillsString.Split(',', ';')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));

            foreach (var skillName in skillNames)
            {
                // Try direct enum parsing first
                if (Enum.TryParse<TourGuideSkill>(skillName, true, out var skill))
                {
                    // Check if the parsed skill is actually defined in the enum
                    var definedValues = Enum.GetValues<TourGuideSkill>();
                    if (definedValues.Contains(skill))
                    {
                        skills.Add(skill);
                        continue;
                    }
                }

                // Try legacy language mapping for backward compatibility
                if (LegacyLanguageMapping.TryGetValue(skillName, out var legacySkill))
                {
                    skills.Add(legacySkill);
                }
            }

            return skills.Distinct().ToList();
        }

        /// <summary>
        /// Migrate legacy language string to new skills format
        /// </summary>
        /// <param name="legacyLanguages">Old languages string</param>
        /// <returns>New skills string</returns>
        public static string MigrateLegacyLanguages(string? legacyLanguages)
        {
            if (string.IsNullOrWhiteSpace(legacyLanguages))
                return string.Empty;

            var skills = StringToSkills(legacyLanguages);
            return SkillsToString(skills);
        }

        /// <summary>
        /// Validate skills string format
        /// </summary>
        /// <param name="skillsString">Skills string to validate</param>
        /// <returns>True if valid</returns>
        public static bool IsValidSkillsString(string? skillsString)
        {
            if (string.IsNullOrWhiteSpace(skillsString))
                return true; // Empty is valid

            var skills = StringToSkills(skillsString);
            var originalCount = skillsString.Split(',', ';')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Count();

            return skills.Count == originalCount; // All skills were parsed successfully
        }

        /// <summary>
        /// Get all available skills grouped by category
        /// </summary>
        /// <returns>Dictionary of category name to skills</returns>
        public static Dictionary<string, List<TourGuideSkill>> GetSkillsByCategory()
        {
            return new Dictionary<string, List<TourGuideSkill>>
            {
                ["Ngôn ngữ"] = SkillCategories.Languages,
                ["Kiến thức chuyên môn"] = SkillCategories.Knowledge,
                ["Kỹ năng hoạt động"] = SkillCategories.Activities,
                ["Kỹ năng đặc biệt"] = SkillCategories.Special
            };
        }
    }
}
