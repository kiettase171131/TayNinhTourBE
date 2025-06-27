using System.Text.RegularExpressions;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// Utility class để matching skills giữa TourDetails requirements và TourGuide capabilities
    /// Hỗ trợ language code mapping và flexible matching
    /// </summary>
    public static class SkillsMatchingUtility
    {
        /// <summary>
        /// Language code mapping dictionary
        /// Maps various language representations to standardized codes
        /// </summary>
        private static readonly Dictionary<string, HashSet<string>> LanguageMapping = new()
        {
            ["vietnamese"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "vietnamese", "vn", "vi", "tiếng việt", "tieng viet", "việt nam", "viet nam" 
            },
            ["english"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "english", "en", "eng", "tiếng anh", "tieng anh", "anh" 
            },
            ["chinese"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "chinese", "cn", "zh", "mandarin", "tiếng trung", "tieng trung", "trung quốc", "trung quoc" 
            },
            ["japanese"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "japanese", "jp", "ja", "tiếng nhật", "tieng nhat", "nhật bản", "nhat ban" 
            },
            ["korean"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "korean", "kr", "ko", "tiếng hàn", "tieng han", "hàn quốc", "han quoc" 
            },
            ["french"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "french", "fr", "tiếng pháp", "tieng phap", "pháp" 
            },
            ["german"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "german", "de", "tiếng đức", "tieng duc", "đức" 
            },
            ["spanish"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "spanish", "es", "tiếng tây ban nha", "tieng tay ban nha", "tây ban nha" 
            }
        };

        /// <summary>
        /// Kiểm tra xem TourGuide có skills phù hợp với requirements không
        /// </summary>
        /// <param name="requiredSkills">Skills yêu cầu từ TourDetails (comma-separated)</param>
        /// <param name="guideLanguages">Languages của TourGuide (comma-separated)</param>
        /// <returns>True nếu có ít nhất 1 skill match</returns>
        public static bool MatchSkills(string? requiredSkills, string? guideLanguages)
        {
            if (string.IsNullOrWhiteSpace(requiredSkills) || string.IsNullOrWhiteSpace(guideLanguages))
            {
                return false;
            }

            var requiredSet = ParseSkills(requiredSkills);
            var guideSet = ParseSkills(guideLanguages);

            return HasAnyMatch(requiredSet, guideSet);
        }

        /// <summary>
        /// Tính toán match score giữa required skills và guide languages
        /// </summary>
        /// <param name="requiredSkills">Skills yêu cầu</param>
        /// <param name="guideLanguages">Languages của guide</param>
        /// <returns>Score từ 0.0 đến 1.0 (1.0 = perfect match)</returns>
        public static double CalculateMatchScore(string? requiredSkills, string? guideLanguages)
        {
            if (string.IsNullOrWhiteSpace(requiredSkills) || string.IsNullOrWhiteSpace(guideLanguages))
            {
                return 0.0;
            }

            var requiredSet = ParseSkills(requiredSkills);
            var guideSet = ParseSkills(guideLanguages);

            if (requiredSet.Count == 0)
            {
                return 0.0;
            }

            var matchCount = CountMatches(requiredSet, guideSet);
            return (double)matchCount / requiredSet.Count;
        }

        /// <summary>
        /// Lấy danh sách skills match giữa requirements và guide capabilities
        /// </summary>
        /// <param name="requiredSkills">Skills yêu cầu</param>
        /// <param name="guideLanguages">Languages của guide</param>
        /// <returns>Danh sách skills match</returns>
        public static List<string> GetMatchedSkills(string? requiredSkills, string? guideLanguages)
        {
            if (string.IsNullOrWhiteSpace(requiredSkills) || string.IsNullOrWhiteSpace(guideLanguages))
            {
                return new List<string>();
            }

            var requiredSet = ParseSkills(requiredSkills);
            var guideSet = ParseSkills(guideLanguages);
            var matches = new List<string>();

            foreach (var required in requiredSet)
            {
                foreach (var guide in guideSet)
                {
                    if (AreLanguagesEquivalent(required, guide))
                    {
                        matches.Add(required);
                        break;
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// Parse skills string thành normalized set
        /// </summary>
        /// <param name="skillsString">Comma-separated skills string</param>
        /// <returns>Set of normalized skills</returns>
        public static HashSet<string> ParseSkills(string skillsString)
        {
            if (string.IsNullOrWhiteSpace(skillsString))
            {
                return new HashSet<string>();
            }

            return skillsString
                .Split(',', ';')
                .Select(skill => NormalizeSkill(skill.Trim()))
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normalize skill string (remove accents, lowercase, trim)
        /// </summary>
        /// <param name="skill">Raw skill string</param>
        /// <returns>Normalized skill string</returns>
        private static string NormalizeSkill(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
            {
                return string.Empty;
            }

            // Remove extra whitespace and normalize
            skill = Regex.Replace(skill.Trim(), @"\s+", " ");
            
            // Convert to lowercase for comparison
            return skill.ToLowerInvariant();
        }

        /// <summary>
        /// Kiểm tra xem có bất kỳ match nào giữa 2 skill sets
        /// </summary>
        private static bool HasAnyMatch(HashSet<string> requiredSet, HashSet<string> guideSet)
        {
            foreach (var required in requiredSet)
            {
                foreach (var guide in guideSet)
                {
                    if (AreLanguagesEquivalent(required, guide))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Đếm số lượng matches giữa 2 skill sets
        /// </summary>
        private static int CountMatches(HashSet<string> requiredSet, HashSet<string> guideSet)
        {
            int count = 0;
            foreach (var required in requiredSet)
            {
                foreach (var guide in guideSet)
                {
                    if (AreLanguagesEquivalent(required, guide))
                    {
                        count++;
                        break; // Avoid double counting same required skill
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Kiểm tra xem 2 languages có equivalent không
        /// </summary>
        private static bool AreLanguagesEquivalent(string lang1, string lang2)
        {
            if (string.Equals(lang1, lang2, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check through language mapping
            foreach (var mapping in LanguageMapping.Values)
            {
                if (mapping.Contains(lang1) && mapping.Contains(lang2))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Lấy standardized language name từ input
        /// </summary>
        /// <param name="language">Input language string</param>
        /// <returns>Standardized language name hoặc original nếu không tìm thấy</returns>
        public static string GetStandardizedLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return string.Empty;
            }

            var normalized = NormalizeSkill(language);
            
            foreach (var kvp in LanguageMapping)
            {
                if (kvp.Value.Contains(normalized))
                {
                    return kvp.Key; // Return standardized name
                }
            }

            return language; // Return original if not found
        }

        /// <summary>
        /// Validate skills string format
        /// </summary>
        /// <param name="skillsString">Skills string to validate</param>
        /// <returns>True nếu format hợp lệ</returns>
        public static bool IsValidSkillsFormat(string? skillsString)
        {
            if (string.IsNullOrWhiteSpace(skillsString))
            {
                return true; // Empty is valid
            }

            // Check for reasonable length and format
            if (skillsString.Length > 500)
            {
                return false;
            }

            // Parse and check if we get valid skills
            var skills = ParseSkills(skillsString);
            return skills.Count > 0 && skills.All(skill => skill.Length <= 50);
        }

        // ===== ENHANCED SKILL MATCHING METHODS =====

        /// <summary>
        /// Kiểm tra xem TourGuide có skills phù hợp với requirements không (Enhanced version)
        /// Hỗ trợ cả legacy languages và new skill system
        /// </summary>
        /// <param name="requiredSkills">Skills yêu cầu từ TourDetails</param>
        /// <param name="guideSkills">Skills của TourGuide (có thể là legacy languages hoặc new skills)</param>
        /// <returns>True nếu có ít nhất 1 skill match</returns>
        public static bool MatchSkillsEnhanced(string? requiredSkills, string? guideSkills)
        {
            if (string.IsNullOrWhiteSpace(requiredSkills) || string.IsNullOrWhiteSpace(guideSkills))
            {
                return false;
            }

            // Convert both to TourGuideSkill enums for accurate matching
            var requiredSkillEnums = TourGuideSkillUtility.StringToSkills(requiredSkills);
            var guideSkillEnums = TourGuideSkillUtility.StringToSkills(guideSkills);

            // Check for any intersection
            return requiredSkillEnums.Intersect(guideSkillEnums).Any();
        }

        /// <summary>
        /// Tính toán match score giữa required skills và guide skills (Enhanced version)
        /// </summary>
        /// <param name="requiredSkills">Skills yêu cầu</param>
        /// <param name="guideSkills">Skills của guide</param>
        /// <returns>Score từ 0.0 đến 1.0 (1.0 = perfect match)</returns>
        public static double CalculateMatchScoreEnhanced(string? requiredSkills, string? guideSkills)
        {
            if (string.IsNullOrWhiteSpace(requiredSkills) || string.IsNullOrWhiteSpace(guideSkills))
            {
                return 0.0;
            }

            var requiredSkillEnums = TourGuideSkillUtility.StringToSkills(requiredSkills);
            var guideSkillEnums = TourGuideSkillUtility.StringToSkills(guideSkills);

            if (requiredSkillEnums.Count == 0)
            {
                return 0.0;
            }

            var matchCount = requiredSkillEnums.Intersect(guideSkillEnums).Count();
            return (double)matchCount / requiredSkillEnums.Count;
        }

        /// <summary>
        /// Lấy danh sách skills match giữa requirements và guide capabilities (Enhanced version)
        /// </summary>
        /// <param name="requiredSkills">Skills yêu cầu</param>
        /// <param name="guideSkills">Skills của guide</param>
        /// <returns>Danh sách skills match với display names</returns>
        public static List<string> GetMatchedSkillsEnhanced(string? requiredSkills, string? guideSkills)
        {
            if (string.IsNullOrWhiteSpace(requiredSkills) || string.IsNullOrWhiteSpace(guideSkills))
            {
                return new List<string>();
            }

            var requiredSkillEnums = TourGuideSkillUtility.StringToSkills(requiredSkills);
            var guideSkillEnums = TourGuideSkillUtility.StringToSkills(guideSkills);

            var matchedSkills = requiredSkillEnums.Intersect(guideSkillEnums);
            return matchedSkills.Select(skill => TourGuideSkillUtility.GetDisplayName(skill)).ToList();
        }

        /// <summary>
        /// Lấy danh sách TourGuides được sắp xếp theo độ phù hợp skills
        /// </summary>
        /// <param name="requiredSkills">Skills yêu cầu từ TourDetails</param>
        /// <param name="guides">Danh sách guides với skills của họ</param>
        /// <returns>Danh sách guides được sắp xếp theo match score giảm dần</returns>
        public static List<(T guide, double matchScore, List<string> matchedSkills)> RankGuidesBySkillMatch<T>(
            string? requiredSkills,
            IEnumerable<(T guide, string skills)> guides)
        {
            if (string.IsNullOrWhiteSpace(requiredSkills) || !guides.Any())
            {
                return new List<(T, double, List<string>)>();
            }

            var rankedGuides = new List<(T guide, double matchScore, List<string> matchedSkills)>();

            foreach (var (guide, skills) in guides)
            {
                var matchScore = CalculateMatchScoreEnhanced(requiredSkills, skills);
                var matchedSkills = GetMatchedSkillsEnhanced(requiredSkills, skills);

                if (matchScore > 0) // Only include guides with at least one matching skill
                {
                    rankedGuides.Add((guide, matchScore, matchedSkills));
                }
            }

            return rankedGuides.OrderByDescending(x => x.matchScore)
                              .ThenByDescending(x => x.matchedSkills.Count)
                              .ToList();
        }

        /// <summary>
        /// Migrate legacy language data to new skill format
        /// </summary>
        /// <param name="legacyLanguages">Old languages string</param>
        /// <returns>New skills string in TourGuideSkill format</returns>
        public static string MigrateLegacyToSkills(string? legacyLanguages)
        {
            return TourGuideSkillUtility.MigrateLegacyLanguages(legacyLanguages);
        }

        /// <summary>
        /// Validate if skills string uses new TourGuideSkill format
        /// </summary>
        /// <param name="skillsString">Skills string to check</param>
        /// <returns>True if using new format</returns>
        public static bool IsNewSkillFormat(string? skillsString)
        {
            if (string.IsNullOrWhiteSpace(skillsString))
                return true;

            return TourGuideSkillUtility.IsValidSkillsString(skillsString);
        }
    }
}
