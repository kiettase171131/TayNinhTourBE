using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation for TourGuide entity operations
    /// </summary>
    public class TourGuideRepository : GenericRepository<TourGuide>, ITourGuideRepository
    {
        public TourGuideRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get tour guide by User ID
        /// </summary>
        public async Task<TourGuide?> GetByUserIdAsync(Guid userId)
        {
            return await _context.TourGuides
                .Include(tg => tg.User)
                .Include(tg => tg.Application)
                .FirstOrDefaultAsync(tg => tg.UserId == userId && tg.IsActive);
        }

        /// <summary>
        /// Get tour guide by Application ID
        /// </summary>
        public async Task<TourGuide?> GetByApplicationIdAsync(Guid applicationId)
        {
            return await _context.TourGuides
                .Include(tg => tg.User)
                .Include(tg => tg.Application)
                .FirstOrDefaultAsync(tg => tg.ApplicationId == applicationId && tg.IsActive);
        }

        /// <summary>
        /// Get tour guide with all related data
        /// </summary>
        public async Task<TourGuide?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.TourGuides
                .Include(tg => tg.User)
                .Include(tg => tg.Application)
                .Include(tg => tg.ApprovedBy)
                .Include(tg => tg.TourOperations)
                .Include(tg => tg.Invitations)
                .FirstOrDefaultAsync(tg => tg.Id == id && tg.IsActive);
        }

        /// <summary>
        /// Get all available tour guides
        /// </summary>
        public async Task<List<TourGuide>> GetAvailableGuidesAsync()
        {
            return await _context.TourGuides
                .Include(tg => tg.User)
                .Where(tg => tg.IsAvailable && tg.IsActive)
                .OrderByDescending(tg => tg.Rating)
                .ThenBy(tg => tg.FullName)
                .ToListAsync();
        }

        /// <summary>
        /// Get tour guides by skills
        /// </summary>
        public async Task<List<TourGuide>> GetGuidesBySkillsAsync(List<int> requiredSkills)
        {
            if (!requiredSkills.Any())
                return new List<TourGuide>();

            var skillStrings = requiredSkills.Select(s => s.ToString()).ToList();

            return await _context.TourGuides
                .Include(tg => tg.User)
                .Where(tg => tg.IsActive && 
                           tg.Skills != null && 
                           skillStrings.Any(skill => tg.Skills.Contains(skill)))
                .OrderByDescending(tg => tg.Rating)
                .ToListAsync();
        }

        /// <summary>
        /// Get available tour guides by skills
        /// </summary>
        public async Task<List<TourGuide>> GetAvailableGuidesBySkillsAsync(List<int> requiredSkills)
        {
            if (!requiredSkills.Any())
                return await GetAvailableGuidesAsync();

            var skillStrings = requiredSkills.Select(s => s.ToString()).ToList();

            return await _context.TourGuides
                .Include(tg => tg.User)
                .Where(tg => tg.IsAvailable && 
                           tg.IsActive && 
                           tg.Skills != null && 
                           skillStrings.Any(skill => tg.Skills.Contains(skill)))
                .OrderByDescending(tg => tg.Rating)
                .ThenBy(tg => tg.FullName)
                .ToListAsync();
        }

        /// <summary>
        /// Get top-rated tour guides
        /// </summary>
        public async Task<List<TourGuide>> GetTopRatedGuidesAsync(int count = 10)
        {
            return await _context.TourGuides
                .Include(tg => tg.User)
                .Where(tg => tg.IsActive)
                .OrderByDescending(tg => tg.Rating)
                .ThenByDescending(tg => tg.TotalToursGuided)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Get tour guides with pagination
        /// </summary>
        public async Task<(List<TourGuide> guides, int totalCount)> GetGuidesPagedAsync(
            int pageNumber, 
            int pageSize, 
            bool? isAvailable = null)
        {
            var query = _context.TourGuides
                .Include(tg => tg.User)
                .Where(tg => tg.IsActive);

            if (isAvailable.HasValue)
            {
                query = query.Where(tg => tg.IsAvailable == isAvailable.Value);
            }

            var totalCount = await query.CountAsync();

            var guides = await query
                .OrderByDescending(tg => tg.Rating)
                .ThenBy(tg => tg.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (guides, totalCount);
        }

        /// <summary>
        /// Update tour guide availability
        /// </summary>
        public async Task<bool> UpdateAvailabilityAsync(Guid guideId, bool isAvailable)
        {
            var guide = await _context.TourGuides.FindAsync(guideId);
            if (guide == null || !guide.IsActive)
                return false;

            guide.IsAvailable = isAvailable;
            guide.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Update tour guide rating
        /// </summary>
        public async Task<bool> UpdateRatingAsync(Guid guideId, decimal newRating)
        {
            var guide = await _context.TourGuides.FindAsync(guideId);
            if (guide == null || !guide.IsActive)
                return false;

            guide.Rating = Math.Max(0, Math.Min(5, newRating)); // Ensure rating is between 0-5
            guide.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Increment tours guided count
        /// </summary>
        public async Task<bool> IncrementToursGuidedAsync(Guid guideId)
        {
            var guide = await _context.TourGuides.FindAsync(guideId);
            if (guide == null || !guide.IsActive)
                return false;

            guide.TotalToursGuided++;
            guide.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Search tour guides by name or email
        /// </summary>
        public async Task<List<TourGuide>> SearchGuidesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<TourGuide>();

            var term = searchTerm.ToLower().Trim();

            return await _context.TourGuides
                .Include(tg => tg.User)
                .Where(tg => tg.IsActive && 
                           (tg.FullName.ToLower().Contains(term) || 
                            tg.Email.ToLower().Contains(term)))
                .OrderByDescending(tg => tg.Rating)
                .ToListAsync();
        }

        /// <summary>
        /// Get tour guide statistics
        /// </summary>
        public async Task<TourGuideStatistics?> GetGuideStatisticsAsync(Guid guideId)
        {
            var guide = await _context.TourGuides
                .Include(tg => tg.TourOperations)
                    .ThenInclude(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                .FirstOrDefaultAsync(tg => tg.Id == guideId && tg.IsActive);

            if (guide == null)
                return null;

            var completedTours = guide.TourOperations.Count(to => to.Status == TourOperationStatus.Completed);

            // Get active invitations from TourGuideInvitations table directly
            var activeInvitations = await _context.TourGuideInvitations
                .CountAsync(inv => inv.GuideId == guide.UserId && inv.Status == InvitationStatus.Pending);

            // Get the most recent tour date from completed tours
            var lastTourDate = guide.TourOperations
                .Where(to => to.Status == TourOperationStatus.Completed && to.TourDetails.AssignedSlots.Any())
                .SelectMany(to => to.TourDetails.AssignedSlots)
                .OrderByDescending(slot => slot.TourDate)
                .FirstOrDefault()?.TourDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue;

            return new TourGuideStatistics
            {
                GuideId = guide.Id,
                FullName = guide.FullName,
                Rating = guide.Rating,
                TotalToursGuided = guide.TotalToursGuided,
                ActiveInvitations = activeInvitations,
                CompletedTours = completedTours,
                LastTourDate = lastTourDate,
                IsAvailable = guide.IsAvailable
            };
        }

        /// <summary>
        /// Check if user is already a tour guide
        /// </summary>
        public async Task<bool> IsUserTourGuideAsync(Guid userId)
        {
            return await _context.TourGuides
                .AnyAsync(tg => tg.UserId == userId && tg.IsActive);
        }

        /// <summary>
        /// Lấy danh sách TourGuides có sẵn (available)
        /// </summary>
        public async Task<IEnumerable<TourGuide>> GetAvailableTourGuidesAsync()
        {
            return await _context.TourGuides
                .Where(tg => tg.IsAvailable && tg.IsActive && !tg.IsDeleted)
                .Include(tg => tg.User)
                .Include(tg => tg.Application)
                .ToListAsync();
        }

        /// <summary>
        /// Get all tour guides with User information included
        /// </summary>
        public async Task<List<TourGuide>> GetAllWithUserAsync()
        {
            return await _context.TourGuides
                .Include(tg => tg.User)
                .Where(tg => !tg.IsDeleted)
                .OrderBy(tg => tg.FullName)
                .ToListAsync();
        }

    }
}
