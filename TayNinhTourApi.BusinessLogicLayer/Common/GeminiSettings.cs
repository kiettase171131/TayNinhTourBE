namespace TayNinhTourApi.BusinessLogicLayer.Common
{
    /// <summary>
    /// Configuration settings cho Gemini AI API
    /// </summary>
    public class GeminiSettings
    {
        /// <summary>
        /// API Key ?? truy c?p Gemini API
        /// </summary>
        public string ApiKey { get; set; } = null!;

        /// <summary>
        /// URL c?a Gemini API endpoint
        /// </summary>
        public string ApiUrl { get; set; } = null!;

        /// <summary>
        /// Model name ?? s? d?ng (ví d?: gemini-pro)
        /// </summary>
        public string Model { get; set; } = "gemini-pro";

        /// <summary>
        /// S? token t?i ?a cho response
        /// </summary>
        public int MaxTokens { get; set; } = 4096;

        /// <summary>
        /// Temperature cho AI response (0.0 - 1.0)
        /// </summary>
        public double Temperature { get; set; } = 0.7;

        /// <summary>
        /// System prompt m?c ??nh
        /// </summary>
        public string SystemPrompt { get; set; } = null!;
    }
}