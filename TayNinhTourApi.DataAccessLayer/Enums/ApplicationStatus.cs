namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Legacy ApplicationStatus enum
    /// Kept for backward compatibility, use TourGuideApplicationStatus for new implementations
    /// </summary>
    [Obsolete("Use TourGuideApplicationStatus instead")]
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
