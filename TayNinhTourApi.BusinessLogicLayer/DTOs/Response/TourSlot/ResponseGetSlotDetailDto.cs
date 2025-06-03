using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot
{
    /// <summary>
    /// DTO cho response lấy chi tiết tour slot
    /// </summary>
    public class ResponseGetSlotDetailDto
    {
        /// <summary>
        /// Có thành công không
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Thông báo
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Thông tin chi tiết tour slot
        /// </summary>
        public TourSlotDetailDto? SlotDetail { get; set; }
    }

    /// <summary>
    /// DTO chi tiết tour slot với thông tin đầy đủ
    /// </summary>
    public class TourSlotDetailDto : TourSlotDto
    {
        /// <summary>
        /// Thông tin tour template
        /// </summary>
        public TourTemplateBasicDto? TourTemplate { get; set; }

        /// <summary>
        /// Thông tin người tạo
        /// </summary>
        public UserBasicDto? CreatedBy { get; set; }

        /// <summary>
        /// Thông tin người cập nhật cuối
        /// </summary>
        public UserBasicDto? UpdatedBy { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Ngày cập nhật cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Số lượng bookings hiện tại (nếu có)
        /// </summary>
        public int CurrentBookingsCount { get; set; }

        /// <summary>
        /// Lịch sử cập nhật gần đây
        /// </summary>
        public List<SlotUpdateHistoryDto> UpdateHistory { get; set; } = new List<SlotUpdateHistoryDto>();
    }

    /// <summary>
    /// DTO thông tin cơ bản tour template
    /// </summary>
    public class TourTemplateBasicDto
    {
        /// <summary>
        /// ID template
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên template
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả ngắn
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Giá tour
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Thời gian tour (giờ)
        /// </summary>
        public int Duration { get; set; }
    }

    /// <summary>
    /// DTO thông tin cơ bản user
    /// </summary>
    public class UserBasicDto
    {
        /// <summary>
        /// ID user
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO lịch sử cập nhật slot
    /// </summary>
    public class SlotUpdateHistoryDto
    {
        /// <summary>
        /// Thời gian cập nhật
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Người cập nhật
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả thay đổi
        /// </summary>
        public string ChangeDescription { get; set; } = string.Empty;

        /// <summary>
        /// Giá trị cũ
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// Giá trị mới
        /// </summary>
        public string? NewValue { get; set; }
    }
}
