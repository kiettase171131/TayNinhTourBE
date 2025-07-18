namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Lo?i thông báo trong h? th?ng
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Thông báo chung
        /// </summary>
        General = 0,

        /// <summary>
        /// Thông báo v? booking
        /// </summary>
        Booking = 1,

        /// <summary>
        /// Thông báo v? tour
        /// </summary>
        Tour = 2,

        /// <summary>
        /// Thông báo v? h??ng d?n viên
        /// </summary>
        TourGuide = 3,

        /// <summary>
        /// Thông báo v? thanh toán
        /// </summary>
        Payment = 4,

        /// <summary>
        /// Thông báo v? ví ti?n
        /// </summary>
        Wallet = 5,

        /// <summary>
        /// Thông báo h? th?ng
        /// </summary>
        System = 6,

        /// <summary>
        /// Thông báo khuy?n mãi
        /// </summary>
        Promotion = 7,

        /// <summary>
        /// Thông báo c?nh báo
        /// </summary>
        Warning = 8,

        /// <summary>
        /// Thông báo l?i/v?n ??
        /// </summary>
        Error = 9
    }
}