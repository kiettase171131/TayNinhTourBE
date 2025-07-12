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

        /// <summary>
        /// B?t fallback mode khi Gemini không kh? d?ng
        /// </summary>
        public bool EnableFallback { get; set; } = true;

        /// <summary>
        /// Th?i gian ch? t?i ?a tr??c khi fallback (giây)
        /// </summary>
        public int FallbackTimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// Danh sách các model backup ?? th? khi model chính b? overload
        /// </summary>
        public List<AlternativeModel> AlternativeModels { get; set; } = new();
    }

    /// <summary>
    /// C?u hình cho model backup
    /// </summary>
    public class AlternativeModel
    {
        public string Name { get; set; } = null!;
        public string ApiUrl { get; set; } = null!;
        public int MaxTokens { get; set; } = 256;
        public double Temperature { get; set; } = 0.2;
    }

    /// <summary>
    /// Configuration settings cho OpenAI API (backup)
    /// </summary>
    public class OpenAISettings
    {
        /// <summary>
        /// API Key ?? truy c?p OpenAI API
        /// </summary>
        public string ApiKey { get; set; } = null!;

        /// <summary>
        /// Model name ?? s? d?ng (ví d?: gpt-3.5-turbo)
        /// </summary>
        public string Model { get; set; } = "gpt-3.5-turbo";

        /// <summary>
        /// S? token t?i ?a cho response
        /// </summary>
        public int MaxTokens { get; set; } = 800;

        /// <summary>
        /// Temperature cho AI response (0.0 - 1.0)
        /// </summary>
        public double Temperature { get; set; } = 0.5;

        /// <summary>
        /// B?t OpenAI nh? fallback provider
        /// </summary>
        public bool IsEnabled { get; set; } = false;
    }
}