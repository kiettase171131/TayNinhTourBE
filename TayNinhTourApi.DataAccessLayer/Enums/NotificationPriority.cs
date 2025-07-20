namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// ?? ?u tiên c?a thông báo
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// ?? ?u tiên th?p
        /// </summary>
        Low = 0,

        /// <summary>
        /// ?? ?u tiên bình th??ng
        /// </summary>
        Normal = 1,

        /// <summary>
        /// ?? ?u tiên cao
        /// </summary>
        High = 2,

        /// <summary>
        /// ?? ?u tiên kh?n c?p
        /// </summary>
        Urgent = 3,

        /// <summary>
        /// ?? ?u tiên c?c k? quan tr?ng/nguy hi?m
        /// </summary>
        Critical = 4,

        /// <summary>
        /// ?? ?u tiên trung bình
        /// </summary>
        Medium = 5
    }
}