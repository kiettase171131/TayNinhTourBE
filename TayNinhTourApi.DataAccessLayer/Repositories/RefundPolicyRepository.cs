using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    /// <summary>
    /// Repository implementation cho RefundPolicy entity
    /// Kế thừa từ GenericRepository và implement IRefundPolicyRepository
    /// </summary>
    public class RefundPolicyRepository : GenericRepository<RefundPolicy>, IRefundPolicyRepository
    {
        public RefundPolicyRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách policies active theo loại hoàn tiền
        /// </summary>
        public async Task<IEnumerable<RefundPolicy>> GetActivePoliciesByTypeAsync(
            TourRefundType refundType, 
            DateTime? effectiveDate = null)
        {
            var checkDate = effectiveDate ?? DateTime.UtcNow;

            return await _context.RefundPolicies
                .Where(p => p.RefundType == refundType && 
                           p.IsActive && 
                           !p.IsDeleted &&
                           p.EffectiveFrom <= checkDate &&
                           (p.EffectiveTo == null || p.EffectiveTo >= checkDate))
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.MinDaysBeforeEvent)
                .ToListAsync();
        }

        /// <summary>
        /// Tìm policy phù hợp cho số ngày trước tour
        /// </summary>
        public async Task<RefundPolicy?> GetApplicablePolicyAsync(
            TourRefundType refundType, 
            int daysBeforeEvent, 
            DateTime? effectiveDate = null)
        {
            var checkDate = effectiveDate ?? DateTime.UtcNow;

            return await _context.RefundPolicies
                .Where(p => p.RefundType == refundType && 
                           p.IsActive && 
                           !p.IsDeleted &&
                           p.EffectiveFrom <= checkDate &&
                           (p.EffectiveTo == null || p.EffectiveTo >= checkDate) &&
                           p.MinDaysBeforeEvent <= daysBeforeEvent &&
                           (p.MaxDaysBeforeEvent == null || p.MaxDaysBeforeEvent >= daysBeforeEvent))
                .OrderBy(p => p.Priority)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy tất cả policies cho admin management
        /// </summary>
        public async Task<(IEnumerable<RefundPolicy> Items, int TotalCount)> GetForAdminAsync(
            TourRefundType? refundType = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.RefundPolicies
                .Where(p => !p.IsDeleted);

            if (refundType.HasValue)
            {
                query = query.Where(p => p.RefundType == refundType.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.RefundType)
                .ThenBy(p => p.Priority)
                .ThenBy(p => p.MinDaysBeforeEvent)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Kiểm tra có policy nào conflict với range ngày không
        /// </summary>
        public async Task<bool> HasConflictingPolicyAsync(
            TourRefundType refundType,
            int minDaysBeforeEvent,
            int? maxDaysBeforeEvent,
            Guid? excludePolicyId = null)
        {
            var query = _context.RefundPolicies
                .Where(p => p.RefundType == refundType && 
                           p.IsActive && 
                           !p.IsDeleted);

            if (excludePolicyId.HasValue)
            {
                query = query.Where(p => p.Id != excludePolicyId.Value);
            }

            // Kiểm tra overlap: hai range overlap nếu max1 >= min2 && max2 >= min1
            return await query.AnyAsync(p =>
                (p.MaxDaysBeforeEvent == null || p.MaxDaysBeforeEvent >= minDaysBeforeEvent) &&
                (maxDaysBeforeEvent == null || maxDaysBeforeEvent >= p.MinDaysBeforeEvent));
        }

        /// <summary>
        /// Lấy policies đang overlap với range ngày
        /// </summary>
        public async Task<IEnumerable<RefundPolicy>> GetOverlappingPoliciesAsync(
            TourRefundType refundType,
            int minDaysBeforeEvent,
            int? maxDaysBeforeEvent,
            Guid? excludePolicyId = null)
        {
            var query = _context.RefundPolicies
                .Where(p => p.RefundType == refundType && 
                           p.IsActive && 
                           !p.IsDeleted);

            if (excludePolicyId.HasValue)
            {
                query = query.Where(p => p.Id != excludePolicyId.Value);
            }

            return await query
                .Where(p => (p.MaxDaysBeforeEvent == null || p.MaxDaysBeforeEvent >= minDaysBeforeEvent) &&
                           (maxDaysBeforeEvent == null || maxDaysBeforeEvent >= p.MinDaysBeforeEvent))
                .OrderBy(p => p.Priority)
                .ToListAsync();
        }

        /// <summary>
        /// Activate/Deactivate policy
        /// </summary>
        public async Task<bool> UpdateActiveStatusAsync(Guid policyId, bool isActive, Guid updatedById)
        {
            var policy = await _context.RefundPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId && !p.IsDeleted);

            if (policy == null) return false;

            policy.IsActive = isActive;
            policy.UpdatedById = updatedById;
            policy.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy policy theo ID với validation
        /// </summary>
        public async Task<RefundPolicy?> GetValidPolicyAsync(Guid policyId, bool includeInactive = false)
        {
            var query = _context.RefundPolicies
                .Where(p => p.Id == policyId && !p.IsDeleted);

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy danh sách policies theo priority range
        /// </summary>
        public async Task<IEnumerable<RefundPolicy>> GetByPriorityRangeAsync(
            TourRefundType refundType,
            int minPriority,
            int maxPriority)
        {
            return await _context.RefundPolicies
                .Where(p => p.RefundType == refundType && 
                           p.Priority >= minPriority && 
                           p.Priority <= maxPriority &&
                           p.IsActive && 
                           !p.IsDeleted)
                .OrderBy(p => p.Priority)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy next available priority cho loại hoàn tiền
        /// </summary>
        public async Task<int> GetNextAvailablePriorityAsync(TourRefundType refundType)
        {
            var maxPriority = await _context.RefundPolicies
                .Where(p => p.RefundType == refundType && p.IsActive && !p.IsDeleted)
                .MaxAsync(p => (int?)p.Priority);

            return (maxPriority ?? 0) + 1;
        }

        /// <summary>
        /// Kiểm tra priority đã được sử dụng chưa
        /// </summary>
        public async Task<bool> IsPriorityUsedAsync(TourRefundType refundType, int priority, Guid? excludePolicyId = null)
        {
            var query = _context.RefundPolicies
                .Where(p => p.RefundType == refundType && 
                           p.Priority == priority && 
                           p.IsActive && 
                           !p.IsDeleted);

            if (excludePolicyId.HasValue)
            {
                query = query.Where(p => p.Id != excludePolicyId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Lấy policies sắp hết hạn
        /// </summary>
        public async Task<IEnumerable<RefundPolicy>> GetExpiringPoliciesAsync(int daysBeforeExpiry = 30)
        {
            var checkDate = DateTime.UtcNow.AddDays(daysBeforeExpiry);

            return await _context.RefundPolicies
                .Where(p => p.IsActive && 
                           !p.IsDeleted &&
                           p.EffectiveTo.HasValue && 
                           p.EffectiveTo <= checkDate &&
                           p.EffectiveTo > DateTime.UtcNow)
                .OrderBy(p => p.EffectiveTo)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy policies đã hết hạn
        /// </summary>
        public async Task<IEnumerable<RefundPolicy>> GetExpiredPoliciesAsync(DateTime? asOfDate = null)
        {
            var checkDate = asOfDate ?? DateTime.UtcNow;

            return await _context.RefundPolicies
                .Where(p => p.IsActive && 
                           !p.IsDeleted &&
                           p.EffectiveTo.HasValue && 
                           p.EffectiveTo < checkDate)
                .OrderBy(p => p.EffectiveTo)
                .ToListAsync();
        }

        /// <summary>
        /// Bulk update effective dates cho policies
        /// </summary>
        public async Task<bool> BulkUpdateEffectiveDatesAsync(
            IEnumerable<Guid> policyIds,
            DateTime? effectiveFrom,
            DateTime? effectiveTo,
            Guid updatedById)
        {
            var policies = await _context.RefundPolicies
                .Where(p => policyIds.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync();

            if (!policies.Any()) return false;

            foreach (var policy in policies)
            {
                if (effectiveFrom.HasValue)
                    policy.EffectiveFrom = effectiveFrom.Value;
                
                if (effectiveTo.HasValue)
                    policy.EffectiveTo = effectiveTo.Value;

                policy.UpdatedById = updatedById;
                policy.UpdatedAt = DateTime.UtcNow;
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy policy history cho audit
        /// </summary>
        public async Task<IEnumerable<RefundPolicy>> GetPolicyHistoryAsync(
            TourRefundType? refundType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.RefundPolicies
                .Where(p => !p.IsDeleted);

            if (refundType.HasValue)
            {
                query = query.Where(p => p.RefundType == refundType.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= toDate.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Validate policy business rules
        /// </summary>
        public async Task<IEnumerable<string>> ValidatePolicyAsync(RefundPolicy policy)
        {
            var errors = new List<string>();

            // Validate basic rules
            if (policy.MinDaysBeforeEvent < 0)
                errors.Add("Số ngày tối thiểu phải >= 0");

            if (policy.MaxDaysBeforeEvent.HasValue && policy.MaxDaysBeforeEvent < policy.MinDaysBeforeEvent)
                errors.Add("Số ngày tối đa phải >= số ngày tối thiểu");

            if (policy.RefundPercentage < 0 || policy.RefundPercentage > 100)
                errors.Add("Phần trăm hoàn tiền phải từ 0-100");

            if (policy.ProcessingFee < 0)
                errors.Add("Phí xử lý phải >= 0");

            if (policy.ProcessingFeePercentage < 0 || policy.ProcessingFeePercentage > 100)
                errors.Add("Phần trăm phí xử lý phải từ 0-100");

            if (policy.Priority < 1 || policy.Priority > 100)
                errors.Add("Thứ tự ưu tiên phải từ 1-100");

            if (policy.EffectiveTo.HasValue && policy.EffectiveTo <= policy.EffectiveFrom)
                errors.Add("Ngày kết thúc phải sau ngày bắt đầu");

            // Validate business logic
            if (policy.IsActive)
            {
                var hasConflict = await HasConflictingPolicyAsync(
                    policy.RefundType,
                    policy.MinDaysBeforeEvent,
                    policy.MaxDaysBeforeEvent,
                    policy.Id);

                if (hasConflict)
                    errors.Add("Có policy khác đã cover range ngày này");

                var isPriorityUsed = await IsPriorityUsedAsync(
                    policy.RefundType,
                    policy.Priority,
                    policy.Id);

                if (isPriorityUsed)
                    errors.Add("Priority này đã được sử dụng cho loại hoàn tiền này");
            }

            return errors;
        }

        /// <summary>
        /// Lấy default policies cho từng loại hoàn tiền
        /// </summary>
        public async Task<Dictionary<TourRefundType, RefundPolicy?>> GetDefaultPoliciesAsync()
        {
            var result = new Dictionary<TourRefundType, RefundPolicy?>();

            foreach (TourRefundType refundType in Enum.GetValues<TourRefundType>())
            {
                var defaultPolicy = await _context.RefundPolicies
                    .Where(p => p.RefundType == refundType &&
                               p.IsActive &&
                               !p.IsDeleted)
                    .OrderBy(p => p.Priority)
                    .FirstOrDefaultAsync();

                result[refundType] = defaultPolicy;
            }

            return result;
        }

        /// <summary>
        /// Tạo default policies cho hệ thống
        /// </summary>
        public async Task<bool> CreateDefaultPoliciesAsync(Guid createdById)
        {
            var defaultPolicies = new List<RefundPolicy>
            {
                // User Cancellation Policies
                new RefundPolicy
                {
                    Id = Guid.NewGuid(),
                    RefundType = TourRefundType.UserCancellation,
                    MinDaysBeforeEvent = 7,
                    MaxDaysBeforeEvent = null,
                    RefundPercentage = 90,
                    ProcessingFee = 0,
                    ProcessingFeePercentage = 10,
                    Description = "Hủy tour >= 7 ngày trước: hoàn 90%, phí 10%",
                    Priority = 1,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow
                },
                new RefundPolicy
                {
                    Id = Guid.NewGuid(),
                    RefundType = TourRefundType.UserCancellation,
                    MinDaysBeforeEvent = 3,
                    MaxDaysBeforeEvent = 6,
                    RefundPercentage = 50,
                    ProcessingFee = 0,
                    ProcessingFeePercentage = 50,
                    Description = "Hủy tour 3-6 ngày trước: hoàn 50%, phí 50%",
                    Priority = 2,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow
                },
                new RefundPolicy
                {
                    Id = Guid.NewGuid(),
                    RefundType = TourRefundType.UserCancellation,
                    MinDaysBeforeEvent = 1,
                    MaxDaysBeforeEvent = 2,
                    RefundPercentage = 20,
                    ProcessingFee = 0,
                    ProcessingFeePercentage = 80,
                    Description = "Hủy tour 1-2 ngày trước: hoàn 20%, phí 80%",
                    Priority = 3,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow
                },
                new RefundPolicy
                {
                    Id = Guid.NewGuid(),
                    RefundType = TourRefundType.UserCancellation,
                    MinDaysBeforeEvent = 0,
                    MaxDaysBeforeEvent = 0,
                    RefundPercentage = 0,
                    ProcessingFee = 0,
                    ProcessingFeePercentage = 100,
                    Description = "Hủy tour trong ngày hoặc no-show: không hoàn tiền",
                    Priority = 4,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow
                },
                // Company Cancellation Policy
                new RefundPolicy
                {
                    Id = Guid.NewGuid(),
                    RefundType = TourRefundType.CompanyCancellation,
                    MinDaysBeforeEvent = 0,
                    MaxDaysBeforeEvent = null,
                    RefundPercentage = 100,
                    ProcessingFee = 0,
                    ProcessingFeePercentage = 0,
                    Description = "Company hủy tour: hoàn 100% cho khách",
                    Priority = 1,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow
                },
                // Auto Cancellation Policy
                new RefundPolicy
                {
                    Id = Guid.NewGuid(),
                    RefundType = TourRefundType.AutoCancellation,
                    MinDaysBeforeEvent = 0,
                    MaxDaysBeforeEvent = null,
                    RefundPercentage = 100,
                    ProcessingFee = 0,
                    ProcessingFeePercentage = 0,
                    Description = "Hệ thống tự động hủy tour: hoàn 100% cho khách",
                    Priority = 1,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow
                }
            };

            try
            {
                await _context.RefundPolicies.AddRangeAsync(defaultPolicies);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy policy statistics
        /// </summary>
        public async Task<(int TotalPolicies, int ActivePolicies, int ExpiredPolicies, int ExpiringPolicies)> GetPolicyStatisticsAsync()
        {
            var totalPolicies = await _context.RefundPolicies.CountAsync(p => !p.IsDeleted);
            var activePolicies = await _context.RefundPolicies.CountAsync(p => p.IsActive && !p.IsDeleted);

            var now = DateTime.UtcNow;
            var expiredPolicies = await _context.RefundPolicies
                .CountAsync(p => p.IsActive && !p.IsDeleted && p.EffectiveTo.HasValue && p.EffectiveTo < now);

            var expiringPolicies = await _context.RefundPolicies
                .CountAsync(p => p.IsActive && !p.IsDeleted && p.EffectiveTo.HasValue &&
                               p.EffectiveTo >= now && p.EffectiveTo <= now.AddDays(30));

            return (totalPolicies, activePolicies, expiredPolicies, expiringPolicies);
        }

        /// <summary>
        /// Clone policy với modifications
        /// </summary>
        public async Task<RefundPolicy?> ClonePolicyAsync(
            Guid sourcePolicyId,
            Action<RefundPolicy> modifications,
            Guid createdById)
        {
            var sourcePolicy = await GetValidPolicyAsync(sourcePolicyId, true);
            if (sourcePolicy == null) return null;

            var clonedPolicy = new RefundPolicy
            {
                Id = Guid.NewGuid(),
                RefundType = sourcePolicy.RefundType,
                MinDaysBeforeEvent = sourcePolicy.MinDaysBeforeEvent,
                MaxDaysBeforeEvent = sourcePolicy.MaxDaysBeforeEvent,
                RefundPercentage = sourcePolicy.RefundPercentage,
                ProcessingFee = sourcePolicy.ProcessingFee,
                ProcessingFeePercentage = sourcePolicy.ProcessingFeePercentage,
                Description = sourcePolicy.Description,
                Priority = sourcePolicy.Priority,
                IsActive = false, // Start as inactive
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = sourcePolicy.EffectiveTo,
                InternalNotes = $"Cloned from policy {sourcePolicyId}",
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow
            };

            // Apply modifications
            modifications(clonedPolicy);

            try
            {
                await _context.RefundPolicies.AddAsync(clonedPolicy);
                await _context.SaveChangesAsync();
                return clonedPolicy;
            }
            catch
            {
                return null;
            }
        }
    }
}
