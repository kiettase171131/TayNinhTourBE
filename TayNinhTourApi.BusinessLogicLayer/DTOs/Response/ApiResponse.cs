namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response
{
    /// <summary>
    /// Generic API response wrapper
    /// Kế thừa từ BaseResposeDto và thêm generic Data property
    /// </summary>
    /// <typeparam name="T">Type của data response</typeparam>
    public class ApiResponse<T> : BaseResposeDto
    {
        /// <summary>
        /// Data payload của response
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Constructor mặc định
        /// </summary>
        public ApiResponse()
        {
        }

        /// <summary>
        /// Constructor với data
        /// </summary>
        /// <param name="data">Data để trả về</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="message">Message mô tả</param>
        public ApiResponse(T data, int statusCode = 200, string? message = null)
        {
            Data = data;
            StatusCode = statusCode;
            Message = message;
            success = statusCode >= 200 && statusCode < 300;
        }

        /// <summary>
        /// Constructor cho error response
        /// </summary>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="message">Error message</param>
        /// <param name="validationErrors">Validation errors</param>
        public ApiResponse(int statusCode, string message, List<string>? validationErrors = null)
        {
            StatusCode = statusCode;
            Message = message;
            success = false;
            ValidationErrors = validationErrors ?? new List<string>();
        }

        /// <summary>
        /// Tạo success response
        /// </summary>
        /// <param name="data">Data để trả về</param>
        /// <param name="message">Success message</param>
        /// <returns>ApiResponse với success status</returns>
        public static ApiResponse<T> Success(T data, string? message = null)
        {
            return new ApiResponse<T>(data, 200, message);
        }

        /// <summary>
        /// Tạo error response
        /// </summary>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="message">Error message</param>
        /// <param name="validationErrors">Validation errors</param>
        /// <returns>ApiResponse với error status</returns>
        public static ApiResponse<T> Error(int statusCode, string message, List<string>? validationErrors = null)
        {
            return new ApiResponse<T>(statusCode, message, validationErrors);
        }

        /// <summary>
        /// Tạo not found response
        /// </summary>
        /// <param name="message">Not found message</param>
        /// <returns>ApiResponse với 404 status</returns>
        public static ApiResponse<T> NotFound(string message = "Resource not found")
        {
            return new ApiResponse<T>(404, message);
        }

        /// <summary>
        /// Tạo bad request response
        /// </summary>
        /// <param name="message">Bad request message</param>
        /// <param name="validationErrors">Validation errors</param>
        /// <returns>ApiResponse với 400 status</returns>
        public static ApiResponse<T> BadRequest(string message, List<string>? validationErrors = null)
        {
            return new ApiResponse<T>(400, message, validationErrors);
        }

        /// <summary>
        /// Tạo unauthorized response
        /// </summary>
        /// <param name="message">Unauthorized message</param>
        /// <returns>ApiResponse với 401 status</returns>
        public static ApiResponse<T> Unauthorized(string message = "Unauthorized access")
        {
            return new ApiResponse<T>(401, message);
        }

        /// <summary>
        /// Tạo forbidden response
        /// </summary>
        /// <param name="message">Forbidden message</param>
        /// <returns>ApiResponse với 403 status</returns>
        public static ApiResponse<T> Forbidden(string message = "Access forbidden")
        {
            return new ApiResponse<T>(403, message);
        }
    }
}
