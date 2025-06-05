using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface để quản lý thông tin user hiện tại trong request context
    /// Cung cấp các phương thức để lấy thông tin user từ JWT token và HTTP context
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Lấy ID của user hiện tại từ JWT token
        /// </summary>
        /// <returns>User ID hoặc Guid.Empty nếu không authenticated</returns>
        Guid GetCurrentUserId();

        /// <summary>
        /// Lấy thông tin đầy đủ của user hiện tại
        /// </summary>
        /// <returns>CurrentUserObject hoặc null nếu không authenticated</returns>
        Task<CurrentUserObject?> GetCurrentUserAsync();

        /// <summary>
        /// Kiểm tra user hiện tại có được authenticated không
        /// </summary>
        /// <returns>True nếu user đã authenticated</returns>
        bool IsAuthenticated();

        /// <summary>
        /// Lấy Role ID của user hiện tại
        /// </summary>
        /// <returns>Role ID hoặc Guid.Empty nếu không có</returns>
        Guid GetCurrentUserRoleId();

        /// <summary>
        /// Lấy email của user hiện tại
        /// </summary>
        /// <returns>Email hoặc null nếu không có</returns>
        string? GetCurrentUserEmail();

        /// <summary>
        /// Lấy tên của user hiện tại
        /// </summary>
        /// <returns>Tên user hoặc null nếu không có</returns>
        string? GetCurrentUserName();
    }
}
