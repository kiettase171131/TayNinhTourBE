namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// ?? ?u ti�n c?a th�ng b�o
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// ?? ?u ti�n th?p
        /// </summary>
        Low = 0,

        /// <summary>
        /// ?? ?u ti�n b�nh th??ng
        /// </summary>
        Normal = 1,

        /// <summary>
        /// ?? ?u ti�n cao
        /// </summary>
        High = 2,

        /// <summary>
        /// ?? ?u ti�n kh?n c?p
        /// </summary>
        Urgent = 3,

        /// <summary>
        /// ?? ?u ti�n c?c k? quan tr?ng/nguy hi?m
        /// </summary>
        Critical = 4,

        /// <summary>
        /// ?? ?u ti�n trung b�nh
        /// </summary>
        Medium = 5
    }
}