using AutoMapper;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBookingRefund;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho quản lý chính sách hoàn tiền tour booking
    /// Kế thừa từ BaseService và implement IRefundPolicyService
    /// </summary>
    public class RefundPolicyService : BaseService, IRefundPolicyService
    {
        private readonly ILogger<RefundPolicyService> _logger;

        public RefundPolicyService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<RefundPolicyService> logger) : base(mapper, unitOfWork)
        {
            _logger = logger;
        }

        /// <summary>
        /// Tìm policy phù hợp cho việc hoàn tiền
        /// </summary>
        public async Task<RefundPolicy?> GetApplicablePolicyAsync(
            TourRefundType refundType, 
            int daysBeforeEvent, 
            DateTime? effectiveDate = null)
        {
            try
            {
                _logger.LogInformation("Finding applicable policy for refund type: {RefundType}, days before event: {DaysBeforeEvent}", 
                    refundType, daysBeforeEvent);

                var policy = await _unitOfWork.RefundPolicyRepository.GetApplicablePolicyAsync(
                    refundType, daysBeforeEvent, effectiveDate);

                if (policy == null)
                {
                    _logger.LogWarning("No applicable policy found for refund type: {RefundType}, days before event: {DaysBeforeEvent}", 
                        refundType, daysBeforeEvent);
                }

                return policy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding applicable policy for refund type: {RefundType}", refundType);
                return null;
            }
        }

        /// <summary>
        /// Tính toán số tiền hoàn dựa trên policy
        /// </summary>
        public async Task<RefundCalculationResult> CalculateRefundAmountAsync(
            decimal originalAmount,
            TourRefundType refundType,
            int daysBeforeEvent,
            DateTime? effectiveDate = null)
        {
            try
            {
                _logger.LogInformation("Calculating refund amount for original amount: {OriginalAmount}, refund type: {RefundType}, days before: {DaysBeforeEvent}", 
                    originalAmount, refundType, daysBeforeEvent);

                var policy = await GetApplicablePolicyAsync(refundType, daysBeforeEvent, effectiveDate);

                if (policy == null)
                {
                    return new RefundCalculationResult
                    {
                        OriginalAmount = originalAmount,
                        IsEligible = false,
                        IneligibilityReason = "Không tìm thấy chính sách hoàn tiền phù hợp",
                        DaysBeforeTour = daysBeforeEvent
                    };
                }

                // Tính toán theo policy
                var refundPercentage = policy.RefundPercentage;
                var refundAmountBeforeFee = originalAmount * (refundPercentage / 100);
                var totalProcessingFee = policy.CalculateTotalProcessingFee(originalAmount);
                var netRefundAmount = Math.Max(0, refundAmountBeforeFee - totalProcessingFee);

                var result = new RefundCalculationResult
                {
                    AppliedPolicy = policy,
                    OriginalAmount = originalAmount,
                    RefundPercentage = refundPercentage,
                    RefundAmountBeforeFee = refundAmountBeforeFee,
                    ProcessingFee = policy.ProcessingFee,
                    ProcessingFeePercentage = policy.ProcessingFeePercentage,
                    TotalProcessingFee = totalProcessingFee,
                    NetRefundAmount = netRefundAmount,
                    IsEligible = true,
                    DaysBeforeTour = daysBeforeEvent
                };

                _logger.LogInformation("Refund calculation completed. Net amount: {NetAmount}, Policy: {PolicyId}", 
                    netRefundAmount, policy.Id);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating refund amount for original amount: {OriginalAmount}", originalAmount);
                
                return new RefundCalculationResult
                {
                    OriginalAmount = originalAmount,
                    IsEligible = false,
                    IneligibilityReason = "Lỗi hệ thống khi tính toán hoàn tiền",
                    DaysBeforeTour = daysBeforeEvent
                };
            }
        }

        /// <summary>
        /// Kiểm tra tour booking có đủ điều kiện hoàn tiền không
        /// </summary>
        public async Task<ApiResponse<RefundEligibilityResponseDto>> ValidateRefundEligibilityAsync(
            Guid tourBookingId,
            DateTime? cancellationDate = null)
        {
            try
            {
                _logger.LogInformation("Validating refund eligibility for booking: {BookingId}", tourBookingId);

                // Kiểm tra booking có tồn tại và hợp lệ không
                var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(tourBookingId);
                if (booking == null)
                {
                    return ApiResponse<RefundEligibilityResponseDto>.Error("Không tìm thấy booking");
                }

                // Kiểm tra booking status
                if (booking.Status != BookingStatus.Confirmed)
                {
                    return ApiResponse<RefundEligibilityResponseDto>.Error("Chỉ có thể hoàn tiền cho booking đã confirmed");
                }

                // Kiểm tra đã có refund request chưa
                var hasRefundRequest = await _unitOfWork.TourBookingRefundRepository.HasRefundRequestAsync(tourBookingId);
                if (hasRefundRequest)
                {
                    return ApiResponse<RefundEligibilityResponseDto>.Error("Booking này đã có yêu cầu hoàn tiền");
                }

                // Lấy thông tin tour operation với details
                var tourOperation = await _unitOfWork.TourOperationRepository.GetWithDetailsAsync(booking.TourOperationId);
                if (tourOperation == null)
                {
                    return ApiResponse<RefundEligibilityResponseDto>.Error("Không tìm thấy thông tin tour");
                }

                // Tính số ngày trước tour
                var checkDate = cancellationDate ?? DateTime.UtcNow;
                var tourStartDate = tourOperation.TourDetails?.AssignedSlots?.Any() == true ?
                    tourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) :
                    DateTime.MaxValue;
                var daysBeforeTour = (int)(tourStartDate.Date - checkDate.Date).TotalDays;

                // Kiểm tra tour đã bắt đầu chưa
                if (daysBeforeTour < 0)
                {
                    return ApiResponse<RefundEligibilityResponseDto>.Error("Không thể hoàn tiền cho tour đã bắt đầu");
                }

                // Tính toán refund
                var calculation = await CalculateRefundAmountAsync(
                    booking.TotalPrice, 
                    TourRefundType.UserCancellation, 
                    daysBeforeTour);

                // Tìm policy tốt hơn tiếp theo (nếu có)
                var nextBetterPolicy = await FindNextBetterPolicyAsync(TourRefundType.UserCancellation, daysBeforeTour);

                var response = new RefundEligibilityResponseDto
                {
                    IsEligible = calculation.IsEligible,
                    IneligibilityReason = calculation.IneligibilityReason,
                    OriginalAmount = calculation.OriginalAmount,
                    EstimatedRefundAmount = calculation.RefundAmountBeforeFee,
                    EstimatedProcessingFee = calculation.TotalProcessingFee,
                    EstimatedNetAmount = calculation.NetRefundAmount,
                    RefundPercentage = calculation.RefundPercentage,
                    DaysBeforeTour = daysBeforeTour,
                    ApplicablePolicy = calculation.AppliedPolicy?.Description,
                    NextPolicyDeadline = nextBetterPolicy?.deadline,
                    NextPolicyRefundPercentage = nextBetterPolicy?.refundPercentage
                };

                // Thêm warnings và additional info
                AddRefundWarningsAndInfo(response, calculation, daysBeforeTour);

                _logger.LogInformation("Refund eligibility validation completed for booking: {BookingId}, eligible: {IsEligible}", 
                    tourBookingId, response.IsEligible);

                return ApiResponse<RefundEligibilityResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refund eligibility for booking: {BookingId}", tourBookingId);
                return ApiResponse<RefundEligibilityResponseDto>.Error("Lỗi hệ thống khi kiểm tra điều kiện hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy danh sách policies active theo loại hoàn tiền
        /// </summary>
        public async Task<ApiResponse<List<RefundPolicy>>> GetActivePoliciesByTypeAsync(
            TourRefundType refundType,
            DateTime? effectiveDate = null)
        {
            try
            {
                _logger.LogInformation("Getting active policies for refund type: {RefundType}", refundType);

                var policies = await _unitOfWork.RefundPolicyRepository.GetActivePoliciesByTypeAsync(refundType, effectiveDate);
                
                return ApiResponse<List<RefundPolicy>>.Success(policies.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active policies for refund type: {RefundType}", refundType);
                return ApiResponse<List<RefundPolicy>>.Error("Lỗi hệ thống khi lấy danh sách chính sách");
            }
        }

        /// <summary>
        /// Tạo policy mới
        /// </summary>
        public async Task<ApiResponse<RefundPolicy>> CreatePolicyAsync(RefundPolicy policy, Guid createdById)
        {
            try
            {
                _logger.LogInformation("Creating new refund policy for type: {RefundType}", policy.RefundType);

                // Validate policy
                var validationErrors = await ValidatePolicyAsync(policy);
                if (validationErrors.Any())
                {
                    return ApiResponse<RefundPolicy>.Error(string.Join("; ", validationErrors));
                }

                // Set audit fields
                policy.Id = Guid.NewGuid();
                policy.CreatedById = createdById;
                policy.CreatedAt = DateTime.UtcNow;
                policy.IsActive = true;

                await _unitOfWork.RefundPolicyRepository.AddAsync(policy);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Refund policy created successfully with ID: {PolicyId}", policy.Id);

                return ApiResponse<RefundPolicy>.Success(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund policy");
                return ApiResponse<RefundPolicy>.Error("Lỗi hệ thống khi tạo chính sách hoàn tiền");
            }
        }

        /// <summary>
        /// Validate policy business rules
        /// </summary>
        public async Task<List<string>> ValidatePolicyAsync(RefundPolicy policy, Guid? excludePolicyId = null)
        {
            var errors = new List<string>();

            try
            {
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
                    var hasConflict = await _unitOfWork.RefundPolicyRepository.HasConflictingPolicyAsync(
                        policy.RefundType,
                        policy.MinDaysBeforeEvent,
                        policy.MaxDaysBeforeEvent,
                        excludePolicyId ?? policy.Id);

                    if (hasConflict)
                        errors.Add("Có policy khác đã cover range ngày này");

                    var isPriorityUsed = await _unitOfWork.RefundPolicyRepository.IsPriorityUsedAsync(
                        policy.RefundType,
                        policy.Priority,
                        excludePolicyId ?? policy.Id);

                    if (isPriorityUsed)
                        errors.Add("Priority này đã được sử dụng cho loại hoàn tiền này");
                }

                return errors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating policy");
                errors.Add("Lỗi hệ thống khi validate policy");
                return errors;
            }
        }

        /// <summary>
        /// Tìm policy tốt hơn tiếp theo
        /// </summary>
        private async Task<(DateTime deadline, decimal refundPercentage)?> FindNextBetterPolicyAsync(
            TourRefundType refundType, 
            int currentDaysBeforeTour)
        {
            try
            {
                var policies = await _unitOfWork.RefundPolicyRepository.GetActivePoliciesByTypeAsync(refundType);
                
                var betterPolicies = policies
                    .Where(p => p.MinDaysBeforeEvent > currentDaysBeforeTour && p.RefundPercentage > 0)
                    .OrderBy(p => p.MinDaysBeforeEvent)
                    .ToList();

                if (!betterPolicies.Any())
                    return null;

                var nextPolicy = betterPolicies.First();
                var deadline = DateTime.UtcNow.AddDays(currentDaysBeforeTour - nextPolicy.MinDaysBeforeEvent);

                return (deadline, nextPolicy.RefundPercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding next better policy");
                return null;
            }
        }

        /// <summary>
        /// Cập nhật policy
        /// </summary>
        public async Task<ApiResponse<RefundPolicy>> UpdatePolicyAsync(Guid policyId, RefundPolicy policy, Guid updatedById)
        {
            try
            {
                _logger.LogInformation("Updating refund policy: {PolicyId}", policyId);

                var existingPolicy = await _unitOfWork.RefundPolicyRepository.GetByIdAsync(policyId);
                if (existingPolicy == null)
                {
                    return ApiResponse<RefundPolicy>.Error("Không tìm thấy chính sách hoàn tiền");
                }

                // Validate policy
                var validationErrors = await ValidatePolicyAsync(policy, policyId);
                if (validationErrors.Any())
                {
                    return ApiResponse<RefundPolicy>.Error(string.Join("; ", validationErrors));
                }

                // Update fields
                existingPolicy.RefundType = policy.RefundType;
                existingPolicy.MinDaysBeforeEvent = policy.MinDaysBeforeEvent;
                existingPolicy.MaxDaysBeforeEvent = policy.MaxDaysBeforeEvent;
                existingPolicy.RefundPercentage = policy.RefundPercentage;
                existingPolicy.ProcessingFee = policy.ProcessingFee;
                existingPolicy.ProcessingFeePercentage = policy.ProcessingFeePercentage;
                existingPolicy.Description = policy.Description;
                existingPolicy.Priority = policy.Priority;
                existingPolicy.EffectiveFrom = policy.EffectiveFrom;
                existingPolicy.EffectiveTo = policy.EffectiveTo;
                existingPolicy.InternalNotes = policy.InternalNotes;
                existingPolicy.UpdatedById = updatedById;
                existingPolicy.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.RefundPolicyRepository.UpdateAsync(existingPolicy);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Refund policy updated successfully: {PolicyId}", policyId);

                return ApiResponse<RefundPolicy>.Success(existingPolicy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund policy: {PolicyId}", policyId);
                return ApiResponse<RefundPolicy>.Error("Lỗi hệ thống khi cập nhật chính sách hoàn tiền");
            }
        }

        /// <summary>
        /// Activate/Deactivate policy
        /// </summary>
        public async Task<ApiResponse<bool>> UpdatePolicyStatusAsync(Guid policyId, bool isActive, Guid updatedById)
        {
            try
            {
                _logger.LogInformation("Updating policy status: {PolicyId}, active: {IsActive}", policyId, isActive);

                var success = await _unitOfWork.RefundPolicyRepository.UpdateActiveStatusAsync(policyId, isActive, updatedById);

                if (!success)
                {
                    return ApiResponse<bool>.Error("Không thể cập nhật trạng thái chính sách");
                }

                _logger.LogInformation("Policy status updated successfully: {PolicyId}", policyId);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy status: {PolicyId}", policyId);
                return ApiResponse<bool>.Error("Lỗi hệ thống khi cập nhật trạng thái chính sách");
            }
        }

        /// <summary>
        /// Xóa policy (soft delete)
        /// </summary>
        public async Task<ApiResponse<bool>> DeletePolicyAsync(Guid policyId, Guid deletedById)
        {
            try
            {
                _logger.LogInformation("Deleting refund policy: {PolicyId}", policyId);

                var policy = await _unitOfWork.RefundPolicyRepository.GetByIdAsync(policyId);
                if (policy == null)
                {
                    return ApiResponse<bool>.Error("Không tìm thấy chính sách hoàn tiền");
                }

                policy.IsDeleted = true;
                policy.UpdatedById = deletedById;
                policy.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.RefundPolicyRepository.UpdateAsync(policy);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Refund policy deleted successfully: {PolicyId}", policyId);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting refund policy: {PolicyId}", policyId);
                return ApiResponse<bool>.Error("Lỗi hệ thống khi xóa chính sách hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy danh sách policies cho admin management
        /// </summary>
        public async Task<ApiResponse<PaginatedResponse<RefundPolicy>>> GetPoliciesForAdminAsync(AdminRefundFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting policies for admin with filter");

                var (items, totalCount) = await _unitOfWork.RefundPolicyRepository.GetForAdminAsync(
                    filter.RefundType,
                    null, // isActive filter
                    filter.PageNumber,
                    filter.PageSize);

                var response = new PaginatedResponse<RefundPolicy>
                {
                    Items = items.ToList(),
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                };

                return ApiResponse<PaginatedResponse<RefundPolicy>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies for admin");
                return ApiResponse<PaginatedResponse<RefundPolicy>>.Error("Lỗi hệ thống khi lấy danh sách chính sách");
            }
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
            try
            {
                return await _unitOfWork.RefundPolicyRepository.HasConflictingPolicyAsync(
                    refundType, minDaysBeforeEvent, maxDaysBeforeEvent, excludePolicyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking conflicting policy");
                return false;
            }
        }

        /// <summary>
        /// Lấy next available priority cho loại hoàn tiền
        /// </summary>
        public async Task<int> GetNextAvailablePriorityAsync(TourRefundType refundType)
        {
            try
            {
                return await _unitOfWork.RefundPolicyRepository.GetNextAvailablePriorityAsync(refundType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next available priority for refund type: {RefundType}", refundType);
                return 1;
            }
        }

        /// <summary>
        /// Thêm warnings và additional info cho refund eligibility response
        /// </summary>
        private void AddRefundWarningsAndInfo(
            RefundEligibilityResponseDto response,
            RefundCalculationResult calculation,
            int daysBeforeTour)
        {
            // Warnings
            if (calculation.RefundPercentage < 50)
            {
                response.Warnings.Add("Bạn sẽ chỉ được hoàn lại một phần nhỏ số tiền đã trả");
            }

            if (daysBeforeTour <= 1)
            {
                response.Warnings.Add("Hủy tour trong thời gian ngắn có thể không được hoàn tiền");
            }

            if (calculation.TotalProcessingFee > 0)
            {
                response.Warnings.Add($"Sẽ có phí xử lý {calculation.TotalProcessingFee:N0} VNĐ");
            }

            // Additional info
            response.AdditionalInfo.Add($"Chính sách áp dụng: {calculation.AppliedPolicy?.Description}");
            response.AdditionalInfo.Add($"Thời gian xử lý: 3-5 ngày làm việc");

            if (response.NextPolicyDeadline.HasValue)
            {
                response.AdditionalInfo.Add($"Hủy trước {response.NextPolicyDeadline:dd/MM/yyyy} để được hoàn {response.NextPolicyRefundPercentage}%");
            }
        }

        /// <summary>
        /// Tạo default policies cho hệ thống
        /// </summary>
        public async Task<ApiResponse<bool>> CreateDefaultPoliciesAsync(Guid createdById)
        {
            try
            {
                _logger.LogInformation("Creating default refund policies");

                var success = await _unitOfWork.RefundPolicyRepository.CreateDefaultPoliciesAsync(createdById);

                if (!success)
                {
                    return ApiResponse<bool>.Error("Không thể tạo default policies");
                }

                _logger.LogInformation("Default refund policies created successfully");

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default policies");
                return ApiResponse<bool>.Error("Lỗi hệ thống khi tạo default policies");
            }
        }

        /// <summary>
        /// Lấy policy statistics
        /// </summary>
        public async Task<ApiResponse<PolicyStatistics>> GetPolicyStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting policy statistics");

                var (totalPolicies, activePolicies, expiredPolicies, expiringPolicies) =
                    await _unitOfWork.RefundPolicyRepository.GetPolicyStatisticsAsync();

                var policiesByType = new Dictionary<TourRefundType, int>();
                foreach (TourRefundType refundType in Enum.GetValues<TourRefundType>())
                {
                    var policies = await _unitOfWork.RefundPolicyRepository.GetActivePoliciesByTypeAsync(refundType);
                    policiesByType[refundType] = policies.Count();
                }

                var statistics = new PolicyStatistics
                {
                    TotalPolicies = totalPolicies,
                    ActivePolicies = activePolicies,
                    ExpiredPolicies = expiredPolicies,
                    ExpiringPolicies = expiringPolicies,
                    PoliciesByType = policiesByType
                };

                return ApiResponse<PolicyStatistics>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy statistics");
                return ApiResponse<PolicyStatistics>.Error("Lỗi hệ thống khi lấy thống kê policies");
            }
        }

        /// <summary>
        /// Clone policy với modifications
        /// </summary>
        public async Task<ApiResponse<RefundPolicy>> ClonePolicyAsync(
            Guid sourcePolicyId,
            Action<RefundPolicy> modifications,
            Guid createdById)
        {
            try
            {
                _logger.LogInformation("Cloning refund policy: {SourcePolicyId}", sourcePolicyId);

                var clonedPolicy = await _unitOfWork.RefundPolicyRepository.ClonePolicyAsync(
                    sourcePolicyId, modifications, createdById);

                if (clonedPolicy == null)
                {
                    return ApiResponse<RefundPolicy>.Error("Không thể clone policy");
                }

                _logger.LogInformation("Policy cloned successfully: {NewPolicyId}", clonedPolicy.Id);

                return ApiResponse<RefundPolicy>.Success(clonedPolicy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning policy: {SourcePolicyId}", sourcePolicyId);
                return ApiResponse<RefundPolicy>.Error("Lỗi hệ thống khi clone policy");
            }
        }

        /// <summary>
        /// Bulk update effective dates cho policies
        /// </summary>
        public async Task<ApiResponse<bool>> BulkUpdateEffectiveDatesAsync(
            List<Guid> policyIds,
            DateTime? effectiveFrom,
            DateTime? effectiveTo,
            Guid updatedById)
        {
            try
            {
                _logger.LogInformation("Bulk updating effective dates for {Count} policies", policyIds.Count);

                var success = await _unitOfWork.RefundPolicyRepository.BulkUpdateEffectiveDatesAsync(
                    policyIds, effectiveFrom, effectiveTo, updatedById);

                if (!success)
                {
                    return ApiResponse<bool>.Error("Không thể cập nhật effective dates");
                }

                _logger.LogInformation("Bulk update effective dates completed successfully");

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating effective dates");
                return ApiResponse<bool>.Error("Lỗi hệ thống khi bulk update effective dates");
            }
        }

        /// <summary>
        /// Lấy policies sắp hết hạn
        /// </summary>
        public async Task<ApiResponse<List<RefundPolicy>>> GetExpiringPoliciesAsync(int daysBeforeExpiry = 30)
        {
            try
            {
                _logger.LogInformation("Getting expiring policies within {Days} days", daysBeforeExpiry);

                var policies = await _unitOfWork.RefundPolicyRepository.GetExpiringPoliciesAsync(daysBeforeExpiry);

                return ApiResponse<List<RefundPolicy>>.Success(policies.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring policies");
                return ApiResponse<List<RefundPolicy>>.Error("Lỗi hệ thống khi lấy policies sắp hết hạn");
            }
        }

        /// <summary>
        /// Lấy policy history cho audit
        /// </summary>
        public async Task<ApiResponse<PaginatedResponse<RefundPolicy>>> GetPolicyHistoryAsync(RefundStatisticsFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting policy history");

                var policies = await _unitOfWork.RefundPolicyRepository.GetPolicyHistoryAsync(
                    filter.RefundType, filter.FromDate, filter.ToDate);

                var totalCount = policies.Count();
                var pageNumber = 1; // Default page number since RefundStatisticsFilterDto doesn't have pagination
                var pageSize = 20; // Default page size
                var pagedPolicies = policies
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PaginatedResponse<RefundPolicy>
                {
                    Items = pagedPolicies,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return ApiResponse<PaginatedResponse<RefundPolicy>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy history");
                return ApiResponse<PaginatedResponse<RefundPolicy>>.Error("Lỗi hệ thống khi lấy policy history");
            }
        }

        /// <summary>
        /// Preview refund calculation cho customer
        /// </summary>
        public async Task<ApiResponse<RefundPreviewDto>> PreviewRefundCalculationAsync(
            Guid tourBookingId,
            DateTime? cancellationDate = null)
        {
            try
            {
                _logger.LogInformation("Previewing refund calculation for booking: {BookingId}", tourBookingId);

                var eligibilityResponse = await ValidateRefundEligibilityAsync(tourBookingId, cancellationDate);

                if (!eligibilityResponse.IsSuccess || eligibilityResponse.Data == null)
                {
                    return ApiResponse<RefundPreviewDto>.Error(eligibilityResponse.Message);
                }

                var eligibility = eligibilityResponse.Data;
                var calculation = await CalculateRefundAmountAsync(
                    eligibility.OriginalAmount,
                    TourRefundType.UserCancellation,
                    eligibility.DaysBeforeTour,
                    cancellationDate);

                var preview = new RefundPreviewDto
                {
                    IsEligible = eligibility.IsEligible,
                    IneligibilityReason = eligibility.IneligibilityReason,
                    Calculation = calculation,
                    PolicyDescription = calculation.AppliedPolicy?.Description,
                    Warnings = eligibility.Warnings,
                    AdditionalInfo = eligibility.AdditionalInfo,
                    NextPolicyDeadline = eligibility.NextPolicyDeadline,
                    NextPolicyRefundPercentage = eligibility.NextPolicyRefundPercentage
                };

                return ApiResponse<RefundPreviewDto>.Success(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing refund calculation for booking: {BookingId}", tourBookingId);
                return ApiResponse<RefundPreviewDto>.Error("Lỗi hệ thống khi preview refund calculation");
            }
        }

        /// <summary>
        /// Lấy refund policy text để hiển thị cho customer
        /// </summary>
        public async Task<ApiResponse<string>> GetRefundPolicyTextAsync(TourRefundType refundType)
        {
            try
            {
                _logger.LogInformation("Getting refund policy text for type: {RefundType}", refundType);

                var policies = await _unitOfWork.RefundPolicyRepository.GetActivePoliciesByTypeAsync(refundType);

                if (!policies.Any())
                {
                    return ApiResponse<string>.Success("Chưa có chính sách hoàn tiền cho loại này.");
                }

                var policyText = string.Join("\n", policies.Select(p =>
                    $"• {p.Description} (Hoàn {p.RefundPercentage}%)"));

                return ApiResponse<string>.Success(policyText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund policy text for type: {RefundType}", refundType);
                return ApiResponse<string>.Error("Lỗi hệ thống khi lấy policy text");
            }
        }
    }
}
