namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Định nghĩa các loại hoạt động trong timeline của tour
    /// Được sử dụng để phân loại các điểm dừng và hoạt động trong lịch trình tour
    /// </summary>
    public enum TimelineType
    {
        /// <summary>
        /// Điểm khởi hành - nơi bắt đầu tour
        /// </summary>
        Departure = 1,

        /// <summary>
        /// Điểm đến - nơi kết thúc tour hoặc điểm đến chính
        /// </summary>
        Arrival = 2,

        /// <summary>
        /// Hoạt động tham quan - các điểm tham quan, di tích, danh lam thắng cảnh
        /// </summary>
        Sightseeing = 3,

        /// <summary>
        /// Hoạt động giải trí - các hoạt động vui chơi, thể thao, trò chơi
        /// </summary>
        Entertainment = 4,

        /// <summary>
        /// Bữa ăn - điểm dừng để dùng bữa (sáng, trưa, tối, ăn nhẹ)
        /// </summary>
        Meal = 5,

        /// <summary>
        /// Mua sắm - ghé thăm các cửa hàng, chợ, trung tâm thương mại
        /// </summary>
        Shopping = 6,

        /// <summary>
        /// Nghỉ ngơi - thời gian nghỉ ngơi, thư giãn
        /// </summary>
        Rest = 7,

        /// <summary>
        /// Di chuyển - thời gian di chuyển giữa các điểm
        /// </summary>
        Transportation = 8,

        /// <summary>
        /// Nhận phòng - check-in khách sạn hoặc nơi lưu trú
        /// </summary>
        CheckIn = 9,

        /// <summary>
        /// Trả phòng - check-out khách sạn hoặc nơi lưu trú
        /// </summary>
        CheckOut = 10,

        /// <summary>
        /// Hoạt động văn hóa - tham gia các hoạt động văn hóa địa phương
        /// </summary>
        Cultural = 11,

        /// <summary>
        /// Hoạt động thể thao - các hoạt động thể thao, phiêu lưu
        /// </summary>
        Sports = 12,

        /// <summary>
        /// Chụp ảnh - điểm dừng chuyên để chụp ảnh, selfie
        /// </summary>
        Photography = 13,

        /// <summary>
        /// Tự do - thời gian tự do cho khách hàng
        /// </summary>
        FreeTime = 14,

        /// <summary>
        /// Khác - các hoạt động khác không thuộc các loại trên
        /// </summary>
        Other = 15
    }

    /// <summary>
    /// Extension methods cho TimelineType enum
    /// </summary>
    public static class TimelineTypeExtensions
    {
        /// <summary>
        /// Kiểm tra xem loại timeline có yêu cầu thời gian cụ thể không
        /// </summary>
        /// <param name="type">Loại timeline</param>
        /// <returns>True nếu yêu cầu thời gian cụ thể</returns>
        public static bool RequiresSpecificTime(this TimelineType type)
        {
            return type switch
            {
                TimelineType.Departure => true,
                TimelineType.Arrival => true,
                TimelineType.Meal => true,
                TimelineType.CheckIn => true,
                TimelineType.CheckOut => true,
                TimelineType.Transportation => true,
                _ => false
            };
        }

        /// <summary>
        /// Kiểm tra xem loại timeline có thể linh hoạt về thời gian không
        /// </summary>
        /// <param name="type">Loại timeline</param>
        /// <returns>True nếu có thể linh hoạt</returns>
        public static bool IsFlexibleTime(this TimelineType type)
        {
            return type switch
            {
                TimelineType.Sightseeing => true,
                TimelineType.Entertainment => true,
                TimelineType.Shopping => true,
                TimelineType.Rest => true,
                TimelineType.Cultural => true,
                TimelineType.Sports => true,
                TimelineType.Photography => true,
                TimelineType.FreeTime => true,
                TimelineType.Other => true,
                _ => false
            };
        }

        /// <summary>
        /// Lấy mô tả tiếng Việt của loại timeline
        /// </summary>
        /// <param name="type">Loại timeline</param>
        /// <returns>Mô tả tiếng Việt</returns>
        public static string GetVietnameseDescription(this TimelineType type)
        {
            return type switch
            {
                TimelineType.Departure => "Khởi hành",
                TimelineType.Arrival => "Đến nơi",
                TimelineType.Sightseeing => "Tham quan",
                TimelineType.Entertainment => "Giải trí",
                TimelineType.Meal => "Dùng bữa",
                TimelineType.Shopping => "Mua sắm",
                TimelineType.Rest => "Nghỉ ngơi",
                TimelineType.Transportation => "Di chuyển",
                TimelineType.CheckIn => "Nhận phòng",
                TimelineType.CheckOut => "Trả phòng",
                TimelineType.Cultural => "Văn hóa",
                TimelineType.Sports => "Thể thao",
                TimelineType.Photography => "Chụp ảnh",
                TimelineType.FreeTime => "Thời gian tự do",
                TimelineType.Other => "Khác",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// Lấy icon CSS class phù hợp cho loại timeline
        /// </summary>
        /// <param name="type">Loại timeline</param>
        /// <returns>CSS class cho icon</returns>
        public static string GetIconClass(this TimelineType type)
        {
            return type switch
            {
                TimelineType.Departure => "fas fa-plane-departure",
                TimelineType.Arrival => "fas fa-plane-arrival",
                TimelineType.Sightseeing => "fas fa-camera",
                TimelineType.Entertainment => "fas fa-gamepad",
                TimelineType.Meal => "fas fa-utensils",
                TimelineType.Shopping => "fas fa-shopping-bag",
                TimelineType.Rest => "fas fa-bed",
                TimelineType.Transportation => "fas fa-bus",
                TimelineType.CheckIn => "fas fa-key",
                TimelineType.CheckOut => "fas fa-sign-out-alt",
                TimelineType.Cultural => "fas fa-theater-masks",
                TimelineType.Sports => "fas fa-running",
                TimelineType.Photography => "fas fa-camera-retro",
                TimelineType.FreeTime => "fas fa-clock",
                TimelineType.Other => "fas fa-ellipsis-h",
                _ => "fas fa-map-marker-alt"
            };
        }
    }
}
