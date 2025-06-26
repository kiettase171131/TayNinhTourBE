namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Kỹ năng của hướng dẫn viên du lịch
    /// Bao gồm ngôn ngữ, kiến thức chuyên môn và kỹ năng hoạt động
    /// </summary>
    public enum TourGuideSkill
    {
        // ===== NGÔN NGỮ (LANGUAGES) =====
        
        /// <summary>
        /// Tiếng Việt - Vietnamese
        /// </summary>
        Vietnamese = 1,

        /// <summary>
        /// Tiếng Anh - English
        /// </summary>
        English = 2,

        /// <summary>
        /// Tiếng Trung - Chinese (Mandarin)
        /// </summary>
        Chinese = 3,

        /// <summary>
        /// Tiếng Nhật - Japanese
        /// </summary>
        Japanese = 4,

        /// <summary>
        /// Tiếng Hàn - Korean
        /// </summary>
        Korean = 5,

        /// <summary>
        /// Tiếng Pháp - French
        /// </summary>
        French = 6,

        /// <summary>
        /// Tiếng Đức - German
        /// </summary>
        German = 7,

        /// <summary>
        /// Tiếng Nga - Russian
        /// </summary>
        Russian = 8,

        // ===== KIẾN THỨC CHUYÊN MÔN (KNOWLEDGE AREAS) =====

        /// <summary>
        /// Lịch sử - Historical knowledge
        /// </summary>
        History = 101,

        /// <summary>
        /// Văn hóa - Cultural knowledge
        /// </summary>
        Culture = 102,

        /// <summary>
        /// Tôn giáo - Religious knowledge (đặc biệt quan trọng cho Tây Ninh với Tòa Thánh Cao Đài)
        /// </summary>
        Religion = 103,

        /// <summary>
        /// Ẩm thực - Culinary knowledge
        /// </summary>
        Cuisine = 104,

        /// <summary>
        /// Địa lý - Geography knowledge
        /// </summary>
        Geography = 105,

        /// <summary>
        /// Thiên nhiên - Nature and ecology knowledge
        /// </summary>
        Nature = 106,

        /// <summary>
        /// Nghệ thuật - Arts and crafts knowledge
        /// </summary>
        Arts = 107,

        /// <summary>
        /// Kiến trúc - Architecture knowledge
        /// </summary>
        Architecture = 108,

        // ===== KỸ NĂNG HOẠT ĐỘNG (ACTIVITY SKILLS) =====

        /// <summary>
        /// Leo núi - Mountain climbing
        /// </summary>
        MountainClimbing = 201,

        /// <summary>
        /// Trekking - Hiking and trekking
        /// </summary>
        Trekking = 202,

        /// <summary>
        /// Chụp ảnh - Photography skills
        /// </summary>
        Photography = 203,

        /// <summary>
        /// Thể thao nước - Water sports
        /// </summary>
        WaterSports = 204,

        /// <summary>
        /// Đi xe đạp - Cycling
        /// </summary>
        Cycling = 205,

        /// <summary>
        /// Cắm trại - Camping
        /// </summary>
        Camping = 206,

        /// <summary>
        /// Quan sát chim - Bird watching
        /// </summary>
        BirdWatching = 207,

        /// <summary>
        /// Thể thao mạo hiểm - Adventure sports
        /// </summary>
        AdventureSports = 208,

        // ===== KỸ NĂNG ĐẶC BIỆT (SPECIAL SKILLS) =====

        /// <summary>
        /// Sơ cứu - First aid certified
        /// </summary>
        FirstAid = 301,

        /// <summary>
        /// Lái xe - Driving skills
        /// </summary>
        Driving = 302,

        /// <summary>
        /// Nấu ăn - Cooking skills
        /// </summary>
        Cooking = 303,

        /// <summary>
        /// Hướng dẫn thiền - Meditation guidance
        /// </summary>
        Meditation = 304,

        /// <summary>
        /// Massage truyền thống - Traditional massage
        /// </summary>
        TraditionalMassage = 305
    }
}
