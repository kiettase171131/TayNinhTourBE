using AutoMapper;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBookingRefund;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBookingRefund;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho quản lý yêu cầu hoàn tiền tour booking
    /// Kế thừa từ BaseService và implement ITourBookingRefundService
    /// </summary>
    public class TourBookingRefundService : BaseService, ITourBookingRefundService
    {
        private readonly ILogger<TourBookingRefundService> _logger;
        private readonly IRefundPolicyService _refundPolicyService;

        public TourBookingRefundService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<TourBookingRefundService> logger,
            IRefundPolicyService refundPolicyService) : base(mapper, unitOfWork)
        {
            _logger = logger;
            _refundPolicyService = refundPolicyService;
        }

        #region Customer Operations

        /// <summary>
        /// Tạo yêu cầu hoàn tiền mới từ customer (user cancellation)
        /// </summary>
        public async Task<ApiResponse<TourRefundRequestDto>> CreateRefundRequestAsync(
            CreateTourRefundRequestDto createDto, 
            Guid customerId)
        {
            try
            {
                _logger.LogInformation("Creating refund request for customer: {CustomerId}, booking: {BookingId}", 
                    customerId, createDto.TourBookingId);

                // Kiểm tra eligibility
                var eligibilityResponse = await _refundPolicyService.ValidateRefundEligibilityAsync(createDto.TourBookingId);
                if (!eligibilityResponse.IsSuccess || !eligibilityResponse.Data!.IsEligible)
                {
                    return ApiResponse<TourRefundRequestDto>.Error(
                        eligibilityResponse.Data?.IneligibilityReason ?? "Không đủ điều kiện hoàn tiền");
                }

                // Lấy thông tin booking
                var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(createDto.TourBookingId);
                if (booking == null || booking.UserId != customerId)
                {
                    return ApiResponse<TourRefundRequestDto>.Error("Không tìm thấy booking hoặc không có quyền truy cập");
                }

                // Tính toán refund amount
                var calculationResponse = await CalculateRefundAmountAsync(
                    createDto.TourBookingId, 
                    TourRefundType.UserCancellation);

                if (!calculationResponse.IsSuccess || calculationResponse.Data == null)
                {
                    return ApiResponse<TourRefundRequestDto>.Error("Không thể tính toán số tiền hoàn");
                }

                var calculation = calculationResponse.Data;

                // Tạo refund request entity
                var refundRequest = new TourBookingRefund
                {
                    Id = Guid.NewGuid(),
                    TourBookingId = createDto.TourBookingId,
                    UserId = customerId,
                    RefundType = TourRefundType.UserCancellation,
                    RefundReason = createDto.RefundReason,
                    OriginalAmount = calculation.OriginalAmount,
                    RequestedAmount = calculation.RefundAmountBeforeFee,
                    ProcessingFee = calculation.TotalProcessingFee,
                    Status = TourRefundStatus.Pending,
                    CustomerNotes = createDto.CustomerNotes,
                    CustomerBankName = createDto.BankInfo.BankName,
                    CustomerAccountNumber = createDto.BankInfo.AccountNumber,
                    CustomerAccountHolder = createDto.BankInfo.AccountHolderName,
                    DaysBeforeTour = calculation.DaysBeforeTour,
                    RefundPercentage = calculation.RefundPercentage,
                    RequestedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Lưu vào database
                await _unitOfWork.TourBookingRefundRepository.AddAsync(refundRequest);
                await _unitOfWork.SaveChangesAsync();

                // Cập nhật booking status
                booking.Status = BookingStatus.CancellationRequested;
                booking.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Refund request created successfully: {RefundRequestId}", refundRequest.Id);

                // Map to DTO
                var responseDto = await MapToTourRefundRequestDto(refundRequest);
                return ApiResponse<TourRefundRequestDto>.Success(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund request for customer: {CustomerId}", customerId);
                return ApiResponse<TourRefundRequestDto>.Error("Lỗi hệ thống khi tạo yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền của customer
        /// </summary>
        public async Task<ApiResponse<PaginatedResponse<TourRefundRequestDto>>> GetCustomerRefundRequestsAsync(
            Guid customerId, 
            CustomerRefundFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting refund requests for customer: {CustomerId}", customerId);

                var (items, totalCount) = await _unitOfWork.TourBookingRefundRepository.GetByCustomerIdAsync(
                    customerId,
                    filter.Status,
                    filter.RefundType,
                    filter.PageNumber,
                    filter.PageSize);

                var dtoItems = new List<TourRefundRequestDto>();
                foreach (var item in items)
                {
                    var dto = await MapToTourRefundRequestDto(item);
                    dtoItems.Add(dto);
                }

                var response = new PaginatedResponse<TourRefundRequestDto>
                {
                    Items = dtoItems,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                };

                return ApiResponse<PaginatedResponse<TourRefundRequestDto>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund requests for customer: {CustomerId}", customerId);
                return ApiResponse<PaginatedResponse<TourRefundRequestDto>>.Error("Lỗi hệ thống khi lấy danh sách yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết yêu cầu hoàn tiền của customer
        /// </summary>
        public async Task<ApiResponse<TourRefundRequestDto>> GetCustomerRefundRequestByIdAsync(
            Guid refundRequestId, 
            Guid customerId)
        {
            try
            {
                _logger.LogInformation("Getting refund request: {RefundRequestId} for customer: {CustomerId}", 
                    refundRequestId, customerId);

                var refundRequest = await _unitOfWork.TourBookingRefundRepository.GetByIdAndCustomerIdAsync(
                    refundRequestId, customerId);

                if (refundRequest == null)
                {
                    return ApiResponse<TourRefundRequestDto>.Error("Không tìm thấy yêu cầu hoàn tiền");
                }

                var dto = await MapToTourRefundRequestDto(refundRequest);
                return ApiResponse<TourRefundRequestDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund request: {RefundRequestId} for customer: {CustomerId}", 
                    refundRequestId, customerId);
                return ApiResponse<TourRefundRequestDto>.Error("Lỗi hệ thống khi lấy thông tin yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Hủy yêu cầu hoàn tiền (chỉ khi status = Pending)
        /// </summary>
        public async Task<ApiResponse<bool>> CancelRefundRequestAsync(
            Guid refundRequestId, 
            Guid customerId, 
            CancelRefundRequestDto cancelDto)
        {
            try
            {
                _logger.LogInformation("Cancelling refund request: {RefundRequestId} by customer: {CustomerId}", 
                    refundRequestId, customerId);

                var refundRequest = await _unitOfWork.TourBookingRefundRepository.GetByIdAndCustomerIdAsync(
                    refundRequestId, customerId);

                if (refundRequest == null)
                {
                    return ApiResponse<bool>.Error("Không tìm thấy yêu cầu hoàn tiền");
                }

                if (refundRequest.Status != TourRefundStatus.Pending)
                {
                    return ApiResponse<bool>.Error("Chỉ có thể hủy yêu cầu đang chờ xử lý");
                }

                // Cập nhật status
                refundRequest.Status = TourRefundStatus.Cancelled;
                refundRequest.AdminNotes = $"Hủy bởi customer: {cancelDto.CancellationReason}";
                refundRequest.ProcessedAt = DateTime.UtcNow;
                refundRequest.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourBookingRefundRepository.UpdateAsync(refundRequest);

                // Cập nhật booking status về Confirmed
                var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(refundRequest.TourBookingId);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Confirmed;
                    booking.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.TourBookingRepository.UpdateAsync(booking);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Refund request cancelled successfully: {RefundRequestId}", refundRequestId);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling refund request: {RefundRequestId}", refundRequestId);
                return ApiResponse<bool>.Error("Lỗi hệ thống khi hủy yêu cầu hoàn tiền");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Map TourBookingRefund entity to TourRefundRequestDto
        /// </summary>
        private async Task<TourRefundRequestDto> MapToTourRefundRequestDto(TourBookingRefund refundRequest)
        {
            // Load related data if not already loaded
            if (refundRequest.TourBooking == null)
            {
                refundRequest = await _unitOfWork.TourBookingRefundRepository.GetWithDetailsAsync(refundRequest.Id) 
                    ?? refundRequest;
            }

            var dto = new TourRefundRequestDto
            {
                Id = refundRequest.Id,
                TourBookingId = refundRequest.TourBookingId,
                RefundType = refundRequest.RefundType,
                RefundTypeName = GetRefundTypeName(refundRequest.RefundType),
                RefundReason = refundRequest.RefundReason,
                OriginalAmount = refundRequest.OriginalAmount,
                RequestedAmount = refundRequest.RequestedAmount,
                ApprovedAmount = refundRequest.ApprovedAmount,
                ProcessingFee = refundRequest.ProcessingFee,
                NetRefundAmount = (refundRequest.ApprovedAmount ?? refundRequest.RequestedAmount) - refundRequest.ProcessingFee,
                Status = refundRequest.Status,
                StatusName = GetStatusName(refundRequest.Status),
                StatusColor = GetStatusColor(refundRequest.Status),
                RequestedAt = refundRequest.RequestedAt,
                ProcessedAt = refundRequest.ProcessedAt,
                CompletedAt = refundRequest.CompletedAt,
                ProcessedByName = refundRequest.ProcessedBy?.Name,
                CustomerNotes = refundRequest.CustomerNotes,
                AdminNotes = refundRequest.AdminNotes,
                TransactionReference = refundRequest.TransactionReference,
                DaysBeforeTour = refundRequest.DaysBeforeTour,
                RefundPercentage = refundRequest.RefundPercentage,
                CanCancel = refundRequest.Status == TourRefundStatus.Pending,
                CanUpdateBankInfo = refundRequest.Status == TourRefundStatus.Pending || refundRequest.Status == TourRefundStatus.Approved
            };

            // Map tour booking info
            if (refundRequest.TourBooking != null)
            {
                dto.TourBooking = new RefundTourBookingInfo
                {
                    Id = refundRequest.TourBooking.Id,
                    BookingCode = refundRequest.TourBooking.BookingCode,
                    TourName = refundRequest.TourBooking.TourOperation?.Tour?.Name ?? "N/A",
                    TourStartDate = refundRequest.TourBooking.TourOperation?.StartDate ?? DateTime.MinValue,
                    TourEndDate = refundRequest.TourBooking.TourOperation?.EndDate ?? DateTime.MinValue,
                    NumberOfGuests = refundRequest.TourBooking.NumberOfGuests,
                    TotalPrice = refundRequest.TourBooking.TotalPrice,
                    BookingStatus = refundRequest.TourBooking.Status,
                    TourCompanyName = refundRequest.TourBooking.TourOperation?.Tour?.TourCompany?.Name ?? "N/A"
                };
            }

            // Map bank info
            if (!string.IsNullOrEmpty(refundRequest.CustomerBankName))
            {
                dto.BankInfo = new RefundBankInfo
                {
                    BankName = refundRequest.CustomerBankName,
                    MaskedAccountNumber = MaskAccountNumber(refundRequest.CustomerAccountNumber),
                    AccountHolderName = refundRequest.CustomerAccountHolder ?? ""
                };
            }

            // Generate timeline
            dto.Timeline = GenerateRefundTimeline(refundRequest);

            return dto;
        }

        /// <summary>
        /// Get refund type display name
        /// </summary>
        private string GetRefundTypeName(TourRefundType refundType)
        {
            return refundType switch
            {
                TourRefundType.UserCancellation => "Khách hàng hủy",
                TourRefundType.CompanyCancellation => "Công ty hủy",
                TourRefundType.AutoCancellation => "Hệ thống hủy",
                _ => refundType.ToString()
            };
        }

        /// <summary>
        /// Get status display name
        /// </summary>
        private string GetStatusName(TourRefundStatus status)
        {
            return status switch
            {
                TourRefundStatus.Pending => "Chờ xử lý",
                TourRefundStatus.Approved => "Đã duyệt",
                TourRefundStatus.Rejected => "Từ chối",
                TourRefundStatus.Completed => "Hoàn thành",
                TourRefundStatus.Cancelled => "Đã hủy",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Get status color for UI
        /// </summary>
        private string GetStatusColor(TourRefundStatus status)
        {
            return status switch
            {
                TourRefundStatus.Pending => "orange",
                TourRefundStatus.Approved => "blue",
                TourRefundStatus.Rejected => "red",
                TourRefundStatus.Completed => "green",
                TourRefundStatus.Cancelled => "gray",
                _ => "gray"
            };
        }

        /// <summary>
        /// Mask account number for security
        /// </summary>
        private string MaskAccountNumber(string? accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length <= 4)
                return accountNumber ?? "";

            return accountNumber.Substring(0, 4) + new string('*', accountNumber.Length - 4);
        }

        /// <summary>
        /// Generate timeline for refund request
        /// </summary>
        private List<RefundTimelineItem> GenerateRefundTimeline(TourBookingRefund refundRequest)
        {
            var timeline = new List<RefundTimelineItem>
            {
                new RefundTimelineItem
                {
                    Timestamp = refundRequest.RequestedAt,
                    Title = "Yêu cầu hoàn tiền được tạo",
                    Description = $"Khách hàng tạo yêu cầu hoàn tiền với lý do: {refundRequest.RefundReason}",
                    Type = RefundTimelineType.Created,
                    Icon = "plus-circle",
                    Color = "blue"
                }
            };

            if (refundRequest.ProcessedAt.HasValue)
            {
                var timelineItem = new RefundTimelineItem
                {
                    Timestamp = refundRequest.ProcessedAt.Value,
                    PerformedBy = refundRequest.ProcessedBy?.Name,
                    Type = refundRequest.Status switch
                    {
                        TourRefundStatus.Approved => RefundTimelineType.Approved,
                        TourRefundStatus.Rejected => RefundTimelineType.Rejected,
                        TourRefundStatus.Cancelled => RefundTimelineType.Cancelled,
                        _ => RefundTimelineType.Created
                    }
                };

                switch (refundRequest.Status)
                {
                    case TourRefundStatus.Approved:
                        timelineItem.Title = "Yêu cầu được duyệt";
                        timelineItem.Description = $"Admin duyệt hoàn tiền {refundRequest.ApprovedAmount:N0} VNĐ";
                        timelineItem.Icon = "check-circle";
                        timelineItem.Color = "green";
                        break;
                    case TourRefundStatus.Rejected:
                        timelineItem.Title = "Yêu cầu bị từ chối";
                        timelineItem.Description = $"Admin từ chối với lý do: {refundRequest.AdminNotes}";
                        timelineItem.Icon = "x-circle";
                        timelineItem.Color = "red";
                        break;
                    case TourRefundStatus.Cancelled:
                        timelineItem.Title = "Yêu cầu bị hủy";
                        timelineItem.Description = "Khách hàng hủy yêu cầu hoàn tiền";
                        timelineItem.Icon = "minus-circle";
                        timelineItem.Color = "gray";
                        break;
                }

                timeline.Add(timelineItem);
            }

            if (refundRequest.CompletedAt.HasValue)
            {
                timeline.Add(new RefundTimelineItem
                {
                    Timestamp = refundRequest.CompletedAt.Value,
                    Title = "Hoàn tiền hoàn thành",
                    Description = $"Đã chuyển {refundRequest.ApprovedAmount:N0} VNĐ vào tài khoản khách hàng",
                    PerformedBy = refundRequest.ProcessedBy?.Name,
                    Type = RefundTimelineType.Completed,
                    Icon = "dollar-sign",
                    Color = "green"
                });
            }

            return timeline.OrderBy(t => t.Timestamp).ToList();
        }

        #endregion

        #region Additional Customer Operations

        /// <summary>
        /// Cập nhật thông tin ngân hàng của yêu cầu hoàn tiền
        /// </summary>
        public async Task<ApiResponse<bool>> UpdateRefundBankInfoAsync(
            Guid refundRequestId,
            Guid customerId,
            UpdateRefundBankInfoDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating bank info for refund request: {RefundRequestId} by customer: {CustomerId}",
                    refundRequestId, customerId);

                var refundRequest = await _unitOfWork.TourBookingRefundRepository.GetByIdAndCustomerIdAsync(
                    refundRequestId, customerId);

                if (refundRequest == null)
                {
                    return ApiResponse<bool>.Error("Không tìm thấy yêu cầu hoàn tiền");
                }

                if (refundRequest.Status != TourRefundStatus.Pending && refundRequest.Status != TourRefundStatus.Approved)
                {
                    return ApiResponse<bool>.Error("Chỉ có thể cập nhật thông tin ngân hàng khi yêu cầu đang chờ xử lý hoặc đã được duyệt");
                }

                // Cập nhật thông tin ngân hàng
                refundRequest.CustomerBankName = updateDto.BankInfo.BankName;
                refundRequest.CustomerAccountNumber = updateDto.BankInfo.AccountNumber;
                refundRequest.CustomerAccountHolder = updateDto.BankInfo.AccountHolderName;
                refundRequest.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(updateDto.ChangeReason))
                {
                    refundRequest.CustomerNotes = (refundRequest.CustomerNotes ?? "") +
                        $"\n[{DateTime.UtcNow:dd/MM/yyyy HH:mm}] Cập nhật thông tin ngân hàng: {updateDto.ChangeReason}";
                }

                await _unitOfWork.TourBookingRefundRepository.UpdateAsync(refundRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Bank info updated successfully for refund request: {RefundRequestId}", refundRequestId);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bank info for refund request: {RefundRequestId}", refundRequestId);
                return ApiResponse<bool>.Error("Lỗi hệ thống khi cập nhật thông tin ngân hàng");
            }
        }

        #endregion

        #region System Operations

        /// <summary>
        /// Tính toán số tiền hoàn cho một booking
        /// </summary>
        public async Task<ApiResponse<RefundCalculationResult>> CalculateRefundAmountAsync(
            Guid tourBookingId,
            TourRefundType refundType,
            DateTime? cancellationDate = null)
        {
            try
            {
                _logger.LogInformation("Calculating refund amount for booking: {BookingId}, type: {RefundType}",
                    tourBookingId, refundType);

                // Lấy thông tin booking
                var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(tourBookingId);
                if (booking == null)
                {
                    return ApiResponse<RefundCalculationResult>.Error("Không tìm thấy booking");
                }

                // Lấy thông tin tour operation
                var tourOperation = await _unitOfWork.TourOperationRepository.GetByIdAsync(booking.TourOperationId);
                if (tourOperation == null)
                {
                    return ApiResponse<RefundCalculationResult>.Error("Không tìm thấy thông tin tour");
                }

                // Tính số ngày trước tour
                var checkDate = cancellationDate ?? DateTime.UtcNow;
                var daysBeforeTour = Math.Max(0, (int)(tourOperation.StartDate.Date - checkDate.Date).TotalDays);

                // Sử dụng RefundPolicyService để tính toán
                var calculation = await _refundPolicyService.CalculateRefundAmountAsync(
                    booking.TotalPrice, refundType, daysBeforeTour, checkDate);

                return ApiResponse<RefundCalculationResult>.Success(calculation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating refund amount for booking: {BookingId}", tourBookingId);
                return ApiResponse<RefundCalculationResult>.Error("Lỗi hệ thống khi tính toán số tiền hoàn");
            }
        }

        /// <summary>
        /// Xử lý hoàn tiền cho company cancellation
        /// </summary>
        public async Task<ApiResponse<CompanyCancellationResult>> ProcessCompanyCancellationAsync(
            List<Guid> tourBookingIds,
            string cancellationReason,
            Guid processedById)
        {
            try
            {
                _logger.LogInformation("Processing company cancellation for {Count} bookings", tourBookingIds.Count);

                var result = new CompanyCancellationResult();
                var refundRequests = new List<TourBookingRefund>();

                foreach (var bookingId in tourBookingIds)
                {
                    try
                    {
                        // Kiểm tra booking
                        var booking = await _unitOfWork.TourBookingRepository.GetByIdAsync(bookingId);
                        if (booking == null || booking.Status != BookingStatus.Confirmed)
                        {
                            result.FailedBookingIds.Add(bookingId);
                            result.Errors.Add($"Booking {bookingId}: Không tìm thấy hoặc không hợp lệ");
                            continue;
                        }

                        // Kiểm tra đã có refund request chưa
                        var hasRefundRequest = await _unitOfWork.TourBookingRefundRepository.HasRefundRequestAsync(bookingId);
                        if (hasRefundRequest)
                        {
                            result.FailedBookingIds.Add(bookingId);
                            result.Errors.Add($"Booking {bookingId}: Đã có yêu cầu hoàn tiền");
                            continue;
                        }

                        // Tính toán refund
                        var calculationResponse = await CalculateRefundAmountAsync(
                            bookingId, TourRefundType.CompanyCancellation);

                        if (!calculationResponse.IsSuccess || calculationResponse.Data == null)
                        {
                            result.FailedBookingIds.Add(bookingId);
                            result.Errors.Add($"Booking {bookingId}: Không thể tính toán hoàn tiền");
                            continue;
                        }

                        var calculation = calculationResponse.Data;

                        // Tạo refund request
                        var refundRequest = new TourBookingRefund
                        {
                            Id = Guid.NewGuid(),
                            TourBookingId = bookingId,
                            UserId = booking.UserId,
                            RefundType = TourRefundType.CompanyCancellation,
                            RefundReason = cancellationReason,
                            OriginalAmount = calculation.OriginalAmount,
                            RequestedAmount = calculation.RefundAmountBeforeFee,
                            ApprovedAmount = calculation.RefundAmountBeforeFee, // Auto approve for company cancellation
                            ProcessingFee = calculation.TotalProcessingFee,
                            Status = TourRefundStatus.Approved, // Auto approve
                            DaysBeforeTour = calculation.DaysBeforeTour,
                            RefundPercentage = calculation.RefundPercentage,
                            ProcessedById = processedById,
                            RequestedAt = DateTime.UtcNow,
                            ProcessedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        refundRequests.Add(refundRequest);

                        // Cập nhật booking status
                        booking.Status = BookingStatus.CancelledByCompany;
                        booking.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.TourBookingRepository.UpdateAsync(booking);

                        result.SuccessfulBookingIds.Add(bookingId);
                        result.TotalRefundAmount += calculation.RefundAmountBeforeFee;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing company cancellation for booking: {BookingId}", bookingId);
                        result.FailedBookingIds.Add(bookingId);
                        result.Errors.Add($"Booking {bookingId}: Lỗi xử lý - {ex.Message}");
                    }
                }

                // Bulk create refund requests
                if (refundRequests.Any())
                {
                    var success = await _unitOfWork.TourBookingRefundRepository.CreateBulkRefundRequestsAsync(refundRequests);
                    if (success)
                    {
                        result.CreatedRefundRequests = refundRequests.Count;
                        result.ProcessedBookings = result.SuccessfulBookingIds.Count;
                    }
                    else
                    {
                        result.Errors.Add("Lỗi khi tạo bulk refund requests");
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Company cancellation processed. Success: {Success}, Failed: {Failed}",
                    result.SuccessfulBookingIds.Count, result.FailedBookingIds.Count);

                return ApiResponse<CompanyCancellationResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing company cancellation");
                return ApiResponse<CompanyCancellationResult>.Error("Lỗi hệ thống khi xử lý company cancellation");
            }
        }

        #endregion

        #region Validation and Utilities

        /// <summary>
        /// Kiểm tra tour booking có đủ điều kiện hoàn tiền không
        /// </summary>
        public async Task<ApiResponse<RefundEligibilityResponseDto>> CheckRefundEligibilityAsync(
            Guid tourBookingId,
            DateTime? cancellationDate = null)
        {
            try
            {
                return await _refundPolicyService.ValidateRefundEligibilityAsync(tourBookingId, cancellationDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking refund eligibility for booking: {BookingId}", tourBookingId);
                return ApiResponse<RefundEligibilityResponseDto>.Error("Lỗi hệ thống khi kiểm tra điều kiện hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy refund request theo tour booking ID
        /// </summary>
        public async Task<ApiResponse<TourRefundRequestDto?>> GetRefundRequestByBookingIdAsync(Guid tourBookingId)
        {
            try
            {
                _logger.LogInformation("Getting refund request for booking: {BookingId}", tourBookingId);

                var refundRequest = await _unitOfWork.TourBookingRefundRepository.GetByTourBookingIdAsync(tourBookingId);

                if (refundRequest == null)
                {
                    return ApiResponse<TourRefundRequestDto?>.Success(null);
                }

                var dto = await MapToTourRefundRequestDto(refundRequest);
                return ApiResponse<TourRefundRequestDto?>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund request for booking: {BookingId}", tourBookingId);
                return ApiResponse<TourRefundRequestDto?>.Error("Lỗi hệ thống khi lấy thông tin refund request");
            }
        }

        /// <summary>
        /// Kiểm tra customer có refund request pending nào không
        /// </summary>
        public async Task<ApiResponse<bool>> HasPendingRefundRequestAsync(Guid customerId)
        {
            try
            {
                var hasPending = await _unitOfWork.TourBookingRefundRepository.HasPendingRefundAsync(customerId);
                return ApiResponse<bool>.Success(hasPending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking pending refund requests for customer: {CustomerId}", customerId);
                return ApiResponse<bool>.Error("Lỗi hệ thống khi kiểm tra pending refund requests");
            }
        }

        #endregion

        #region Admin Operations - Placeholder Methods

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền cho admin
        /// </summary>
        public async Task<ApiResponse<PaginatedResponse<AdminTourRefundDto>>> GetAdminRefundRequestsAsync(AdminRefundFilterDto filter)
        {
            // TODO: Implement admin refund requests listing
            throw new NotImplementedException("Admin refund requests listing will be implemented in next phase");
        }

        /// <summary>
        /// Lấy thông tin chi tiết yêu cầu hoàn tiền cho admin
        /// </summary>
        public async Task<ApiResponse<AdminTourRefundDto>> GetAdminRefundRequestByIdAsync(Guid refundRequestId)
        {
            // TODO: Implement admin refund request detail
            throw new NotImplementedException("Admin refund request detail will be implemented in next phase");
        }

        /// <summary>
        /// Admin approve yêu cầu hoàn tiền
        /// </summary>
        public async Task<ApiResponse<bool>> ApproveRefundAsync(Guid refundRequestId, Guid adminId, ApproveRefundDto approveDto)
        {
            // TODO: Implement admin approve refund
            throw new NotImplementedException("Admin approve refund will be implemented in next phase");
        }

        /// <summary>
        /// Admin reject yêu cầu hoàn tiền
        /// </summary>
        public async Task<ApiResponse<bool>> RejectRefundAsync(Guid refundRequestId, Guid adminId, RejectRefundDto rejectDto)
        {
            // TODO: Implement admin reject refund
            throw new NotImplementedException("Admin reject refund will be implemented in next phase");
        }

        /// <summary>
        /// Admin confirm đã chuyển tiền thủ công
        /// </summary>
        public async Task<ApiResponse<bool>> ConfirmTransferAsync(Guid refundRequestId, Guid adminId, ConfirmTransferDto confirmDto)
        {
            // TODO: Implement admin confirm transfer
            throw new NotImplementedException("Admin confirm transfer will be implemented in next phase");
        }

        /// <summary>
        /// Bulk approve/reject nhiều refund requests
        /// </summary>
        public async Task<ApiResponse<BulkRefundActionResult>> BulkProcessRefundRequestsAsync(Guid adminId, BulkRefundActionDto bulkActionDto)
        {
            // TODO: Implement bulk process refund requests
            throw new NotImplementedException("Bulk process refund requests will be implemented in next phase");
        }

        /// <summary>
        /// Điều chỉnh số tiền hoàn
        /// </summary>
        public async Task<ApiResponse<bool>> AdjustRefundAmountAsync(Guid refundRequestId, Guid adminId, AdjustRefundAmountDto adjustDto)
        {
            // TODO: Implement adjust refund amount
            throw new NotImplementedException("Adjust refund amount will be implemented in next phase");
        }

        /// <summary>
        /// Reassign refund request cho admin khác
        /// </summary>
        public async Task<ApiResponse<bool>> ReassignRefundRequestAsync(Guid refundRequestId, Guid currentAdminId, ReassignRefundDto reassignDto)
        {
            // TODO: Implement reassign refund request
            throw new NotImplementedException("Reassign refund request will be implemented in next phase");
        }

        /// <summary>
        /// Xử lý hoàn tiền cho auto cancellation
        /// </summary>
        public async Task<ApiResponse<AutoCancellationResult>> ProcessAutoCancellationAsync(List<Guid> tourOperationIds, string cancellationReason)
        {
            // TODO: Implement auto cancellation processing
            throw new NotImplementedException("Auto cancellation processing will be implemented in next phase");
        }

        /// <summary>
        /// Lấy dashboard thống kê refund cho admin
        /// </summary>
        public async Task<ApiResponse<AdminRefundDashboardDto>> GetRefundDashboardAsync(RefundStatisticsFilterDto filter)
        {
            // TODO: Implement refund dashboard
            throw new NotImplementedException("Refund dashboard will be implemented in next phase");
        }

        /// <summary>
        /// Lấy thống kê refund theo tháng
        /// </summary>
        public async Task<ApiResponse<MonthlyRefundStats>> GetMonthlyRefundStatsAsync(int year, int month, TourRefundType? refundType = null)
        {
            // TODO: Implement monthly refund stats
            throw new NotImplementedException("Monthly refund stats will be implemented in next phase");
        }

        /// <summary>
        /// Export refund data
        /// </summary>
        public async Task<ApiResponse<ExportFileResult>> ExportRefundDataAsync(ExportRefundFilterDto filter)
        {
            // TODO: Implement export refund data
            throw new NotImplementedException("Export refund data will be implemented in next phase");
        }

        #endregion
    }
}
