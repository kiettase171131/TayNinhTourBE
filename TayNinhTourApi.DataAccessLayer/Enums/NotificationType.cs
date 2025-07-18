namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Lo?i th�ng b�o trong h? th?ng
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Th�ng b�o chung
        /// </summary>
        General = 0,

        /// <summary>
        /// Th�ng b�o v? booking
        /// </summary>
        Booking = 1,

        /// <summary>
        /// Th�ng b�o v? tour
        /// </summary>
        Tour = 2,

        /// <summary>
        /// Th�ng b�o v? h??ng d?n vi�n
        /// </summary>
        TourGuide = 3,

        /// <summary>
        /// Th�ng b�o v? thanh to�n
        /// </summary>
        Payment = 4,

        /// <summary>
        /// Th�ng b�o v? v� ti?n
        /// </summary>
        Wallet = 5,

        /// <summary>
        /// Th�ng b�o h? th?ng
        /// </summary>
        System = 6,

        /// <summary>
        /// Th�ng b�o khuy?n m�i
        /// </summary>
        Promotion = 7,

        /// <summary>
        /// Th�ng b�o c?nh b�o
        /// </summary>
        Warning = 8,

        /// <summary>
        /// Th�ng b�o l?i/v?n ??
        /// </summary>
        Error = 9
    }
}