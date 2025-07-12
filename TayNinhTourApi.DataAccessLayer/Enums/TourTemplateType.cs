namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Định nghĩa các loại tour template có thể được tạo trong hệ thống
    /// Tất cả tours đều có phí dịch vụ, chỉ khác về vé vào cửa
    /// </summary>
    public enum TourTemplateType
    {
        /// <summary>
        /// Tour danh lam thắng cảnh - Không tốn vé vào cửa nhưng vẫn có phí dịch vụ tour
        /// Ví dụ: Núi Bà Đen, Chùa Cao Đài, các khu vực tham quan không thu vé
        /// Khách vẫn phải trả phí dịch vụ cho hướng dẫn viên, xe, coordination
        /// </summary>
        FreeScenic = 1,

        /// <summary>
        /// Tour khu vui chơi - Có phí vào cửa PLUS phí dịch vụ tour
        /// Ví dụ: Khu du lịch sinh thái, công viên nước, khu vui chơi có phí vào cửa
        /// Khách phải trả: phí dịch vụ tour + vé vào cửa địa điểm
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
                TourTemplateType.FreeScenic => "Tour tham quan các danh lam thắng cảnh không thu vé vào cửa, chỉ phí dịch vụ",
                TourTemplateType.PaidAttraction => "Tour tham quan các khu vui chơi có phí vào cửa, cộng phí dịch vụ",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Kiểm tra xem loại tour có phí vào cửa không
        /// </summary>
        /// <param name="type">Loại tour template</param>
        /// <returns>True nếu có phí vào cửa (ngoài phí dịch vụ)</returns>
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

        /// <summary>
        /// Lấy thông tin về cấu trúc phí
        /// </summary>
        /// <param name="type">Loại tour template</param>
        /// <returns>Thông tin cấu trúc phí</returns>
        public static string GetPriceStructure(this TourTemplateType type)
        {
            return type switch
            {
                TourTemplateType.FreeScenic => "Chỉ phí dịch vụ tour (guide, xe, coordination)",
                TourTemplateType.PaidAttraction => "Phí dịch vụ tour + vé vào cửa địa điểm",
                _ => "Không xác định"
            };
        }
    }
}
