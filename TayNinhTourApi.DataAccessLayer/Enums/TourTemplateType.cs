namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Định nghĩa các loại tour template có thể được tạo trong hệ thống
    /// Hệ thống mới chỉ hỗ trợ 2 loại tour chính
    /// </summary>
    public enum TourTemplateType
    {
        /// <summary>
        /// Tour danh lam thắng cảnh - Tour miễn phí tham quan các địa điểm tự nhiên, không có phí vào cửa
        /// Ví dụ: Núi Bà Đen, Chùa Cao Đài, các khu vực tham quan miễn phí
        /// </summary>
        FreeScenic = 1,

        /// <summary>
        /// Tour khu vui chơi - Tour có phí tham quan các khu vui chơi, công viên giải trí
        /// Ví dụ: Khu du lịch sinh thái, công viên nước, khu vui chơi có phí vào cửa
        /// </summary>
        PaidAttraction = 2
    }

    /// <summary>
    /// Extension methods cho TourTemplateType enum
    /// </summary>
    public static class TourTemplateTypeExtensions
    {
        /// <summary>
        /// Lấy tên tiếng Việt của loại tour template
        /// </summary>
        /// <param name="type">Loại tour template</param>
        /// <returns>Tên tiếng Việt</returns>
        public static string GetVietnameseName(this TourTemplateType type)
        {
            return type switch
            {
                TourTemplateType.FreeScenic => "Danh lam thắng cảnh",
                TourTemplateType.PaidAttraction => "Khu vui chơi",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// Lấy mô tả chi tiết của loại tour template
        /// </summary>
        /// <param name="type">Loại tour template</param>
        /// <returns>Mô tả chi tiết</returns>
        public static string GetDescription(this TourTemplateType type)
        {
            return type switch
            {
                TourTemplateType.FreeScenic => "Tour tham quan các danh lam thắng cảnh, di tích lịch sử không có phí vào cửa",
                TourTemplateType.PaidAttraction => "Tour tham quan các khu vui chơi, công viên giải trí có phí vào cửa",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Kiểm tra xem loại tour có phí vào cửa không
        /// </summary>
        /// <param name="type">Loại tour template</param>
        /// <returns>True nếu có phí vào cửa</returns>
        public static bool HasEntranceFee(this TourTemplateType type)
        {
            return type == TourTemplateType.PaidAttraction;
        }

        /// <summary>
        /// Lấy danh sách tất cả các loại tour template
        /// </summary>
        /// <returns>Danh sách các loại tour template</returns>
        public static List<TourTemplateType> GetAllTypes()
        {
            return Enum.GetValues<TourTemplateType>().ToList();
        }

        /// <summary>
        /// Lấy danh sách các loại tour template với tên tiếng Việt
        /// </summary>
        /// <returns>Dictionary với key là enum value và value là tên tiếng Việt</returns>
        public static Dictionary<TourTemplateType, string> GetAllTypesWithNames()
        {
            return GetAllTypes().ToDictionary(type => type, type => type.GetVietnameseName());
        }
    }
}
