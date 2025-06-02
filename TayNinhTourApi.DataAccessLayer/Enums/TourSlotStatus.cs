namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Định nghĩa trạng thái của tour slot
    /// </summary>
    public enum TourSlotStatus
    {
        /// <summary>
        /// Slot có sẵn để booking
        /// </summary>
        Available = 1,

        /// <summary>
        /// Slot đã được booking đầy
        /// </summary>
        FullyBooked = 2,

        /// <summary>
        /// Slot bị hủy (do thời tiết, bảo trì, etc.)
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Slot đã hoàn thành
        /// </summary>
        Completed = 4,

        /// <summary>
        /// Slot đang trong quá trình thực hiện
        /// </summary>
        InProgress = 5
    }
}
