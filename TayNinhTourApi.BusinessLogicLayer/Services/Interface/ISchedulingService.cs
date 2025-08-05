using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface chuyên xử lý logic tính toán lịch trình và scheduling algorithm
    /// Cung cấp các chức năng tính toán ngày weekend, generate slot dates và validation
    /// </summary>
    public interface ISchedulingService
    {
        /// <summary>
        /// Tính toán các ngày weekend (Saturday/Sunday) trong một tháng cụ thể
        /// Hỗ trợ flexible scheduling với ScheduleDay flags
        /// </summary>
        /// <param name="year">Năm cần tính toán (2024-2030)</param>
        /// <param name="month">Tháng cần tính toán (1-12)</param>
        /// <param name="scheduleDays">Các ngày trong tuần muốn tính (Saturday, Sunday hoặc cả hai)</param>
        /// <returns>Danh sách các ngày weekend được sắp xếp theo thứ tự tăng dần</returns>
        List<DateOnly> CalculateWeekendDates(int year, int month, ScheduleDay scheduleDays);

        /// <summary>
        /// Generate một số lượng cụ thể slot dates từ các ngày weekend trong tháng
        /// Ưu tiên các ngày gần nhất và có thể booking được
        /// </summary>
        /// <param name="year">Năm cần generate slots</param>
        /// <param name="month">Tháng cần generate slots</param>
        /// <param name="scheduleDays">Các ngày trong tuần muốn generate</param>
        /// <param name="numberOfSlots">Số lượng slots cần generate (mặc định 4)</param>
        /// <param name="excludePastDates">Có loại bỏ các ngày đã qua không (mặc định true)</param>
        /// <returns>Danh sách slot dates được chọn theo algorithm</returns>
        List<DateOnly> GenerateSlotDates(int year, int month, ScheduleDay scheduleDays, int numberOfSlots = 4, bool excludePastDates = true);

        /// <summary>
        /// Validate input parameters cho scheduling operations
        /// Kiểm tra tính hợp lệ của year, month và business rules
        /// </summary>
        /// <param name="year">Năm cần validate</param>
        /// <param name="month">Tháng cần validate</param>
        /// <param name="scheduleDays">Ngày trong tuần cần validate (optional)</param>
        /// <returns>Kết quả validation với chi tiết lỗi nếu có</returns>
        ResponseValidationDto ValidateScheduleInput(int year, int month, ScheduleDay? scheduleDays = null);

        /// <summary>
        /// Validate schedule input with template creation date to ensure first slot date compliance
        /// Kiểm tra tính hợp lệ của scheduling request theo quy tắc slot đầu tiên
        /// </summary>
        /// <param name="year">Năm cần validate</param>
        /// <param name="month">Tháng cần validate</param>
        /// <param name="templateCreatedAt">Ngày tạo template</param>
        /// <param name="scheduleDays">Ngày trong tuần cần validate (optional)</param>
        /// <returns>Kết quả validation bao gồm cả quy tắc slot đầu tiên</returns>
        ResponseValidationDto ValidateScheduleInputWithTemplate(int year, int month, DateTime templateCreatedAt, ScheduleDay? scheduleDays = null);

        /// <summary>
        /// Lấy danh sách các slot dates có thể booking trong tương lai
        /// Dựa trên tour template và số lượng yêu cầu
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template (null để lấy tất cả)</param>
        /// <param name="count">Số lượng slots cần lấy</param>
        /// <param name="startDate">Ngày bắt đầu tìm kiếm (mặc định từ hôm nay)</param>
        /// <returns>Danh sách slot dates available cho booking</returns>
        Task<List<DateOnly>> GetNextAvailableSlots(Guid? tourTemplateId = null, int count = 10, DateOnly? startDate = null);

        /// <summary>
        /// Tính toán số lượng weekend dates trong một khoảng thời gian
        /// Hữu ích cho planning và capacity estimation
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <param name="scheduleDays">Các ngày trong tuần muốn đếm</param>
        /// <returns>Số lượng weekend dates trong khoảng thời gian</returns>
        int CountWeekendDatesInRange(DateOnly startDate, DateOnly endDate, ScheduleDay scheduleDays);

        /// <summary>
        /// Kiểm tra một ngày cụ thể có phải là weekend theo ScheduleDay không
        /// </summary>
        /// <param name="date">Ngày cần kiểm tra</param>
        /// <param name="scheduleDays">Định nghĩa weekend days</param>
        /// <returns>True nếu ngày đó là weekend theo định nghĩa</returns>
        bool IsWeekendDate(DateOnly date, ScheduleDay scheduleDays);

        /// <summary>
        /// Lấy ngày weekend tiếp theo từ một ngày cho trước
        /// </summary>
        /// <param name="fromDate">Ngày bắt đầu tìm kiếm</param>
        /// <param name="scheduleDays">Định nghĩa weekend days</param>
        /// <returns>Ngày weekend tiếp theo hoặc null nếu không tìm thấy</returns>
        DateOnly? GetNextWeekendDate(DateOnly fromDate, ScheduleDay scheduleDays);

        /// <summary>
        /// Tính toán distribution của slots trong tháng để đảm bảo phân bố đều
        /// Hỗ trợ advanced scheduling algorithm
        /// </summary>
        /// <param name="year">Năm</param>
        /// <param name="month">Tháng</param>
        /// <param name="scheduleDays">Ngày trong tuần</param>
        /// <param name="targetSlots">Số slots mục tiêu</param>
        /// <returns>Danh sách ngày được phân bố optimal</returns>
        List<DateOnly> CalculateOptimalSlotDistribution(int year, int month, ScheduleDay scheduleDays, int targetSlots);
    }
}
