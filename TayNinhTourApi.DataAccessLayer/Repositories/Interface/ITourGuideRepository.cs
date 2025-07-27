using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface for TourGuide entity operations
    /// Provides methods for CRUD operations, availability checks, and performance tracking
    /// </summary>
    public interface ITourGuideRepository : IGenericRepository<TourGuide>
    {
        /// <summary>
        /// Get tour guide by User ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>TourGuide entity or null if not found</returns>
        Task<TourGuide?> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Get tour guide by Application ID
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <returns>TourGuide entity or null if not found</returns>
        Task<TourGuide?> GetByApplicationIdAsync(Guid applicationId);

        /// <summary>
        /// Get tour guide with all related data (User, Application, etc.)
        /// </summary>
        /// <param name="id">TourGuide ID</param>
        /// <returns>TourGuide entity with navigation properties loaded</returns>
        Task<TourGuide?> GetByIdWithDetailsAsync(Guid id);

        /// <summary>
        /// Get all available tour guides
        /// </summary>
        /// <returns>List of available tour guides</returns>
        Task<List<TourGuide>> GetAvailableGuidesAsync();

        /// <summary>
        /// Get tour guides by skills
        /// </summary>
        /// <param name="requiredSkills">List of required skill IDs</param>
        /// <returns>List of tour guides with matching skills</returns>
        Task<List<TourGuide>> GetGuidesBySkillsAsync(List<int> requiredSkills);

        /// <summary>
        /// Get available tour guides by skills
        /// </summary>
        /// <param name="requiredSkills">List of required skill IDs</param>
        /// <returns>List of available tour guides with matching skills</returns>
        Task<List<TourGuide>> GetAvailableGuidesBySkillsAsync(List<int> requiredSkills);

        /// <summary>
        /// Get top-rated tour guides
        /// </summary>
        /// <param name="count">Number of guides to return</param>
        /// <returns>List of top-rated tour guides</returns>
        Task<List<TourGuide>> GetTopRatedGuidesAsync(int count = 10);

        /// <summary>
        /// Get tour guides with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="isAvailable">Filter by availability (optional)</param>
        /// <returns>Tuple of guides list and total count</returns>
        Task<(List<TourGuide> guides, int totalCount)> GetGuidesPagedAsync(
            int pageNumber, 
            int pageSize, 
            bool? isAvailable = null);

        /// <summary>
        /// Update tour guide availability
        /// </summary>
        /// <param name="guideId">TourGuide ID</param>
        /// <param name="isAvailable">New availability status</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> UpdateAvailabilityAsync(Guid guideId, bool isAvailable);

        /// <summary>
        /// Update tour guide rating
        /// </summary>
        /// <param name="guideId">TourGuide ID</param>
        /// <param name="newRating">New rating value</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> UpdateRatingAsync(Guid guideId, decimal newRating);

        /// <summary>
        /// Increment tours guided count
        /// </summary>
        /// <param name="guideId">TourGuide ID</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> IncrementToursGuidedAsync(Guid guideId);

        /// <summary>
        /// Search tour guides by name or email
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching tour guides</returns>
        Task<List<TourGuide>> SearchGuidesAsync(string searchTerm);

        /// <summary>
        /// Get tour guide statistics
        /// </summary>
        /// <param name="guideId">TourGuide ID</param>
        /// <returns>Statistics object with tour count, rating, etc.</returns>
        Task<TourGuideStatistics?> GetGuideStatisticsAsync(Guid guideId);

        /// <summary>
        /// Check if user is already a tour guide
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user is already a tour guide</returns>
        Task<bool> IsUserTourGuideAsync(Guid userId);

        /// <summary>
        /// Lấy danh sách TourGuides có sẵn (available)
        /// </summary>
        /// <returns>Danh sách TourGuides available</returns>
        Task<IEnumerable<TourGuide>> GetAvailableTourGuidesAsync();

        /// <summary>
        /// Get all tour guides with User information included
        /// </summary>
        /// <returns>List of tour guides with User navigation property loaded</returns>
        Task<List<TourGuide>> GetAllWithUserAsync();

    }

    /// <summary>
    /// Tour guide statistics data transfer object
    /// </summary>
    public class TourGuideStatistics
    {
        public Guid GuideId { get; set; }
        public string FullName { get; set; } = null!;
        public decimal Rating { get; set; }
        public int TotalToursGuided { get; set; }
        public int ActiveInvitations { get; set; }
        public int CompletedTours { get; set; }
        public DateTime LastTourDate { get; set; }
        public bool IsAvailable { get; set; }
    }
}
