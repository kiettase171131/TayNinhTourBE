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
        /// L?y th�ng b�o c?a user v?i ph�n trang
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="pageIndex">Trang hi?n t?i (0-based)</param>
        /// <param name="pageSize">K�ch th??c trang</param>
        /// <param name="isRead">L?c theo tr?ng th�i ??c (null = t?t c?)</param>
        /// <param name="type">L?c theo lo?i th�ng b�o (null = t?t c?)</param>
        /// <returns>Danh s�ch th�ng b�o v?i t?ng s?</returns>
        Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetUserNotificationsAsync(
            Guid userId, 
            int pageIndex = 0, 
            int pageSize = 20, 
            bool? isRead = null,
            NotificationType? type = null);

        /// <summary>
        /// ??m s? th�ng b�o ch?a ??c c?a user
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>S? l??ng th�ng b�o ch?a ??c</returns>
        Task<int> GetUnreadCountAsync(Guid userId);

        /// <summary>
        /// ?�nh d?u th�ng b�o ?� ??c
        /// </summary>
        /// <param name="notificationId">ID c?a th�ng b�o</param>
        /// <param name="userId">ID c?a user (?? verify ownership)</param>
        /// <returns>K?t qu? thao t�c</returns>
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

        /// <summary>
        /// ?�nh d?u t?t c? th�ng b�o c?a user ?� ??c
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>S? l??ng th�ng b�o ?� c?p nh?t</returns>
        Task<int> MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// X�a th�ng b�o c? ?� h?t h?n
        /// </summary>
        /// <param name="cutoffDate">Ng�y cutoff</param>
        /// <returns>S? l??ng th�ng b�o ?� x�a</returns>
        Task<int> DeleteExpiredNotificationsAsync(DateTime cutoffDate);

        /// <summary>
        /// L?y th�ng b�o theo ID v� verify ownership
        /// </summary>
        /// <param name="notificationId">ID c?a th�ng b�o</param>
        /// <param name="userId">ID c?a user</param>
        /// <returns>Th�ng b�o n?u t�m th?y v� thu?c v? user</returns>
        Task<Notification?> GetByIdAndUserAsync(Guid notificationId, Guid userId);
    }
}