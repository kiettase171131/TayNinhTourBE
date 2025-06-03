using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Scheduling
{
    /// <summary>
    /// Response DTO cho optimal distribution
    /// </summary>
    public class ResponseOptimalDistributionDto : BaseResposeDto
    {
        /// <summary>
        /// Năm được tính toán
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Tháng được tính toán
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Số slots mục tiêu
        /// </summary>
        public int TargetSlots { get; set; }

        /// <summary>
        /// Số slots thực tế được phân bố
        /// </summary>
        public int ActualSlots { get; set; }

        /// <summary>
        /// Tổng số weekend dates có sẵn
        /// </summary>
        public int TotalAvailableWeekendDates { get; set; }

        /// <summary>
        /// Danh sách slots được phân bố optimal
        /// </summary>
        public List<OptimalSlotInfo> OptimalSlots { get; set; } = new();

        /// <summary>
        /// Thông tin về algorithm distribution
        /// </summary>
        public DistributionAlgorithmInfo AlgorithmInfo { get; set; } = new();

        /// <summary>
        /// Có sử dụng optimal distribution không (hay trả về tất cả)
        /// </summary>
        public bool UsedOptimalDistribution { get; set; }
    }

    /// <summary>
    /// Thông tin chi tiết về một optimal slot
    /// </summary>
    public class OptimalSlotInfo
    {
        /// <summary>
        /// Ngày của slot
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Tên ngày trong tuần (tiếng Việt)
        /// </summary>
        public string DayName { get; set; } = string.Empty;

        /// <summary>
        /// Ngày trong tuần (enum)
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Thứ tự trong danh sách optimal slots
        /// </summary>
        public int OptimalOrder { get; set; }

        /// <summary>
        /// Index trong danh sách weekend dates gốc
        /// </summary>
        public int OriginalIndex { get; set; }

        /// <summary>
        /// Khoảng cách (ngày) từ slot trước đó
        /// </summary>
        public int? DaysFromPreviousSlot { get; set; }

        /// <summary>
        /// Khoảng cách (ngày) đến slot tiếp theo
        /// </summary>
        public int? DaysToNextSlot { get; set; }

        /// <summary>
        /// Có phải là slot được chọn theo step calculation không
        /// </summary>
        public bool IsCalculatedStep { get; set; }
    }

    /// <summary>
    /// Thông tin về algorithm distribution
    /// </summary>
    public class DistributionAlgorithmInfo
    {
        /// <summary>
        /// Step size được tính toán
        /// </summary>
        public double CalculatedStep { get; set; }

        /// <summary>
        /// Phương pháp distribution được sử dụng
        /// </summary>
        public string DistributionMethod { get; set; } = string.Empty;

        /// <summary>
        /// Khoảng cách trung bình giữa các slots (ngày)
        /// </summary>
        public double AverageDistanceBetweenSlots { get; set; }

        /// <summary>
        /// Độ lệch chuẩn của khoảng cách
        /// </summary>
        public double DistanceStandardDeviation { get; set; }

        /// <summary>
        /// Có đạt được phân bố đều không
        /// </summary>
        public bool IsEvenlyDistributed { get; set; }
    }
}
