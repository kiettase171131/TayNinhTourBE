using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// Response DTO cho việc tạo tour template
    /// </summary>
    public class ResponseCreateTourTemplateDto : BaseResposeDto
    {
        public TourTemplateDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc cập nhật tour template
    /// </summary>
    public class ResponseUpdateTourTemplateDto : BaseResposeDto
    {
        public TourTemplateDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc lấy tour template theo ID
    /// </summary>
    public class ResponseGetTourTemplateDto : BaseResposeDto
    {
        public TourTemplateDetailDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc xóa tour template
    /// </summary>
    public class ResponseDeleteTourTemplateDto : BaseResposeDto
    {
        public bool Success { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc sao chép tour template
    /// </summary>
    public class ResponseCopyTourTemplateDto : BaseResposeDto
    {
        public TourTemplateDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc thay đổi trạng thái active
    /// </summary>
    public class ResponseSetActiveStatusDto : BaseResposeDto
    {
        public bool Success { get; set; }
        public bool NewStatus { get; set; }
    }

    /// <summary>
    /// Response DTO cho thống kê tour templates
    /// </summary>
    public class ResponseTourTemplateStatisticsDto : BaseResposeDto
    {
        public TourTemplateStatistics? Data { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê tour templates (đã đơn giản hóa)
    /// </summary>
    public class TourTemplateStatistics
    {
        public int TotalTemplates { get; set; }
        public int ActiveTemplates { get; set; }
        public int InactiveTemplates { get; set; }
        public int DeletedTemplates { get; set; }
        public Dictionary<string, int> TemplatesByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> TemplatesByLocation { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> TemplatesByMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> TemplatesByYear { get; set; } = new Dictionary<string, int>();
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>
    /// Response DTO cho search tour templates
    /// </summary>
    public class ResponseSearchTourTemplatesDto : BaseResposeDto
    {
        public List<TourTemplateSummaryDto> Data { get; set; } = new List<TourTemplateSummaryDto>();
        public int TotalResults { get; set; }
        public string SearchKeyword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO cho popular tour templates
    /// </summary>
    public class ResponsePopularTourTemplatesDto : BaseResposeDto
    {
        public List<TourTemplateSummaryDto> Data { get; set; } = new List<TourTemplateSummaryDto>();
        public int RequestedCount { get; set; }
    }

    /// <summary>
    /// Response DTO cho validation check
    /// </summary>
    public class ResponseValidationDto : BaseResposeDto
    {
        public bool IsValid { get; set; }
        public new List<string> ValidationErrors { get; set; } = new List<string>();
        public Dictionary<string, List<string>> FieldErrors { get; set; } = new Dictionary<string, List<string>>();
    }

    /// <summary>
    /// Response DTO cho check delete capability
    /// </summary>
    public class ResponseCanDeleteDto : BaseResposeDto
    {
        public bool CanDelete { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<string> BlockingReasons { get; set; } = new List<string>();
    }
}
