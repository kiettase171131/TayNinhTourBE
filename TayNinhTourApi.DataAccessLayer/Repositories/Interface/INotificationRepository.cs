using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho Notification entity
    /// </summary>
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        /// <summary>
        /// L?y thông báo c?a user v?i phân trang
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="pageIndex">Trang hi?n t?i (0-based)</param>
        /// <param name="pageSize">Kích th??c trang</param>
        /// <param name="isRead">L?c theo tr?ng thái ??c (null = t?t c?)</param>
        /// <param name="type">L?c theo lo?i thông báo (null = t?t c?)</param>
        /// <returns>Danh sách thông báo v?i t?ng s?</returns>
        Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetUserNotificationsAsync(
            Guid userId, 
            int pageIndex = 0, 
            int pageSize = 20, 
            bool? isRead = null,
            NotificationType? type = null);

        /// <summary>
        /// ??m s? thông báo ch?a ??c c?a user
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>S? l??ng thông báo ch?a ??c</returns>
        Task<int> GetUnreadCountAsync(Guid userId);

        /// <summary>
        /// ?ánh d?u thông báo ?ã ??c
        /// </summary>
        /// <param name="notificationId">ID c?a thông báo</param>
        /// <param name="userId">ID c?a user (?? verify ownership)</param>
        /// <returns>K?t qu? thao tác</returns>
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

        /// <summary>
        /// ?ánh d?u t?t c? thông báo c?a user ?ã ??c
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>S? l??ng thông báo ?ã c?p nh?t</returns>
        Task<int> MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// Xóa thông báo c? ?ã h?t h?n
        /// </summary>
        /// <param name="cutoffDate">Ngày cutoff</param>
        /// <returns>S? l??ng thông báo ?ã xóa</returns>
        Task<int> DeleteExpiredNotificationsAsync(DateTime cutoffDate);

        /// <summary>
        /// L?y thông báo theo ID và verify ownership
        /// </summary>
        /// <param name="notificationId">ID c?a thông báo</param>
        /// <param name="userId">ID c?a user</param>
        /// <returns>Thông báo n?u tìm th?y và thu?c v? user</returns>
        Task<Notification?> GetByIdAndUserAsync(Guid notificationId, Guid userId);
    }
}