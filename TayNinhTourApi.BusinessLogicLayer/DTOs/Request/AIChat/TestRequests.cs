namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.AIChat
{
    public class TestGeminiRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestTourRecommendationRequest
    {
        public string Query { get; set; } = string.Empty;
    }

    public class UpdateSessionTitleRequest
    {
        public string NewTitle { get; set; } = string.Empty;
    }
}