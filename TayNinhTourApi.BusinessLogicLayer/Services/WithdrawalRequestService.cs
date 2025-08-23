using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.WithdrawalRequest;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho quản lý yêu cầu rút tiền
    /// Implement business logic và validation cho WithdrawalRequest operations
    /// </summary>
    public class WithdrawalRequestService : IWithdrawalRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWalletService _walletService;
        private readonly INotificationService _notificationService;

        public WithdrawalRequestService(
            IUnitOfWork unitOfWork,
            IWalletService walletService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _walletService = walletService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Tạo yêu cầu rút tiền mới
        /// </summary>
        public async Task<ApiResponse<WithdrawalRequestResponseDto>> CreateRequestAsync(CreateWithdrawalRequestDto createDto, Guid userId)
        {
            try
            {
                // Validate yêu cầu
                var validationResult = await ValidateWithdrawalRequestAsync(userId, createDto.Amount, createDto.BankAccountId);
                if (!validationResult.IsSuccess)
                {
                    return ApiResponse<WithdrawalRequestResponseDto>.Error(validationResult.Message);
                }

                // Kiểm tra có thể tạo yêu cầu mới không
                var canCreateResult = await CanCreateNewRequestAsync(userId);
                if (!canCreateResult.IsSuccess || !canCreateResult.Data)
                {
                    return ApiResponse<WithdrawalRequestResponseDto>.Error(canCreateResult.Message);
                }

                // Lấy thông tin ví hiện tại (hỗ trợ cả TourCompany và SpecialtyShop)
                var walletBalanceResponse = await _walletService.GetCurrentWalletBalanceAsync(userId);
                if (!walletBalanceResponse.IsSuccess)
                {
                    return ApiResponse<WithdrawalRequestResponseDto>.Error("Không thể lấy thông tin ví");
                }

                var currentBalance = walletBalanceResponse.Data;

                // Tạo withdrawal request
                var withdrawalRequest = new WithdrawalRequest
                {
                    UserId = userId,
                    BankAccountId = createDto.BankAccountId,
                    Amount = createDto.Amount,
                    Status = WithdrawalStatus.Pending,
                    RequestedAt = DateTime.UtcNow,
                    UserNotes = createDto.UserNotes?.Trim(),
                    WalletBalanceAtRequest = currentBalance,
                    WithdrawalFee = 0 // Có thể config sau
                };

                await _unitOfWork.WithdrawalRequestRepository.AddAsync(withdrawalRequest);
                await _unitOfWork.SaveChangesAsync();

                // Load bank account info
                var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(createDto.BankAccountId);
                withdrawalRequest.BankAccount = bankAccount!;

                // Tạo notification cho admin (hỗ trợ cả TourCompany và SpecialtyShop)
                var walletType = await _walletService.GetUserWalletTypeAsync(userId);
                string ownerName = "Không xác định";
                
                if (walletType == "TourCompany")
                {
                    var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);
                    if (tourCompany != null)
                    {
                        ownerName = tourCompany.CompanyName;
                    }
                }
                else if (walletType == "SpecialtyShop")
                {
                    var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);
                    if (specialtyShop != null)
                    {
                        ownerName = specialtyShop.ShopName;
                    }
                }

                await _notificationService.CreateNewWithdrawalRequestNotificationAsync(
                    withdrawalRequest.Id,
                    ownerName,
                    withdrawalRequest.Amount);

                var responseDto = MapToResponseDto(withdrawalRequest);
                return ApiResponse<WithdrawalRequestResponseDto>.Success(responseDto, "Tạo yêu cầu rút tiền thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<WithdrawalRequestResponseDto>.Error($"Lỗi khi tạo yêu cầu rút tiền: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền của user hiện tại
        /// </summary>
        public async Task<ApiResponse<PaginatedResponse<WithdrawalRequestResponseDto>>> GetByUserIdAsync(
            Guid userId, 
            WithdrawalStatus? status = null, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _unitOfWork.WithdrawalRequestRepository.GetByUserIdAsync(userId, status, pageNumber, pageSize);
                
                var responseDtos = items.Select(MapToResponseDto).ToList();
                var paginatedResponse = PaginatedResponse<WithdrawalRequestResponseDto>.Create(responseDtos, totalCount, pageNumber, pageSize);
                
                return ApiResponse<PaginatedResponse<WithdrawalRequestResponseDto>>.Success(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<WithdrawalRequestResponseDto>>.Error($"Lỗi khi lấy danh sách yêu cầu rút tiền: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một yêu cầu rút tiền
        /// </summary>
        public async Task<ApiResponse<WithdrawalRequestDetailDto>> GetByIdAsync(Guid withdrawalRequestId, Guid currentUserId)
        {
            try
            {
                var withdrawalRequest = await _unitOfWork.WithdrawalRequestRepository.GetByIdAndUserIdAsync(withdrawalRequestId, currentUserId);
                
                if (withdrawalRequest == null)
                {
                    return ApiResponse<WithdrawalRequestDetailDto>.NotFound("Không tìm thấy yêu cầu rút tiền hoặc bạn không có quyền truy cập");
                }

                // Load full details
                var withdrawalRequestWithDetails = await _unitOfWork.WithdrawalRequestRepository.GetWithDetailsAsync(withdrawalRequestId);
                var responseDto = MapToDetailDto(withdrawalRequestWithDetails!);
                
                return ApiResponse<WithdrawalRequestDetailDto>.Success(responseDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<WithdrawalRequestDetailDto>.Error($"Lỗi khi lấy thông tin yêu cầu rút tiền: {ex.Message}");
            }
        }

        /// <summary>
        /// Hủy yêu cầu rút tiền (chỉ khi status = Pending)
        /// </summary>
        public async Task<ApiResponse<bool>> CancelRequestAsync(Guid withdrawalRequestId, Guid currentUserId, string? reason = null)
        {
            try
            {
                var withdrawalRequest = await _unitOfWork.WithdrawalRequestRepository.GetByIdAndUserIdAsync(withdrawalRequestId, currentUserId);
                
                if (withdrawalRequest == null)
                {
                    return ApiResponse<bool>.NotFound("Không tìm thấy yêu cầu rút tiền hoặc bạn không có quyền hủy");
                }

                if (withdrawalRequest.Status != WithdrawalStatus.Pending)
                {
                    return ApiResponse<bool>.Error("Chỉ có thể hủy yêu cầu đang chờ xử lý");
                }

                var success = await _unitOfWork.WithdrawalRequestRepository.UpdateStatusAsync(
                    withdrawalRequestId,
                    WithdrawalStatus.Cancelled,
                    adminNotes: $"Hủy bởi user. Lý do: {reason ?? "Không có lý do"}");

                if (success)
                {
                    return ApiResponse<bool>.Success(true, "Hủy yêu cầu rút tiền thành công");
                }
                else
                {
                    return ApiResponse<bool>.Error("Không thể hủy yêu cầu rút tiền");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Lỗi khi hủy yêu cầu rút tiền: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền cho admin
        /// </summary>
        public async Task<ApiResponse<PaginatedResponse<WithdrawalRequestAdminDto>>> GetForAdminAsync(
            WithdrawalStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null)
        {
            try
            {
                var (items, totalCount) = await _unitOfWork.WithdrawalRequestRepository.GetForAdminAsync(status, pageNumber, pageSize, searchTerm);
                
                var responseDtos = items.Select(MapToAdminDto).ToList();
                var paginatedResponse = PaginatedResponse<WithdrawalRequestAdminDto>.Create(responseDtos, totalCount, pageNumber, pageSize);
                
                return ApiResponse<PaginatedResponse<WithdrawalRequestAdminDto>>.Success(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<WithdrawalRequestAdminDto>>.Error($"Lỗi khi lấy danh sách yêu cầu rút tiền cho admin: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin duyệt yêu cầu rút tiền
        /// </summary>
        public async Task<ApiResponse<bool>> ApproveRequestAsync(
            Guid withdrawalRequestId, 
            Guid adminId, 
            string? adminNotes = null, 
            string? transactionReference = null)
        {
            try
            {
                var withdrawalRequest = await _unitOfWork.WithdrawalRequestRepository.GetWithDetailsAsync(withdrawalRequestId);
                
                if (withdrawalRequest == null)
                {
                    return ApiResponse<bool>.NotFound("Không tìm thấy yêu cầu rút tiền");
                }

                if (withdrawalRequest.Status != WithdrawalStatus.Pending)
                {
                    return ApiResponse<bool>.Error("Chỉ có thể duyệt yêu cầu đang chờ xử lý");
                }

                // Kiểm tra số dư ví hiện tại (hỗ trợ cả TourCompany và SpecialtyShop)
                var balanceCheckResponse = await _walletService.CheckSufficientBalanceAsync(withdrawalRequest.UserId, withdrawalRequest.Amount);
                if (!balanceCheckResponse.IsSuccess)
                {
                    return ApiResponse<bool>.Error("Không thể kiểm tra số dư ví");
                }

                if (!balanceCheckResponse.Data)
                {
                    return ApiResponse<bool>.Error("Số dư ví không đủ để thực hiện rút tiền");
                }

                // Sử dụng WalletService để xử lý rút tiền (đã hỗ trợ cả TourCompany và SpecialtyShop)
                var withdrawalResult = await _walletService.ProcessWithdrawalAsync(
                    withdrawalRequest.UserId,
                    withdrawalRequest.Amount,
                    withdrawalRequestId);

                if (!withdrawalResult.IsSuccess)
                {
                    return ApiResponse<bool>.Error(withdrawalResult.Message);
                }

                // Cập nhật trạng thái withdrawal request
                var updateSuccess = await _unitOfWork.WithdrawalRequestRepository.UpdateStatusAsync(
                    withdrawalRequestId,
                    WithdrawalStatus.Approved,
                    adminId,
                    adminNotes,
                    transactionReference);

                if (!updateSuccess)
                {
                    // Rollback: hoàn tiền vào ví
                    await _walletService.RefundToWalletAsync(
                        withdrawalRequest.UserId,
                        withdrawalRequest.Amount,
                        "Rollback do lỗi cập nhật trạng thái withdrawal request");

                    return ApiResponse<bool>.Error("Không thể cập nhật trạng thái yêu cầu");
                }

                await _unitOfWork.SaveChangesAsync();

                // Tạo notification cho user
                var bankAccountInfo = $"{withdrawalRequest.BankAccount.BankName} - {MaskAccountNumber(withdrawalRequest.BankAccount.AccountNumber)}";
                await _notificationService.CreateWithdrawalApprovedNotificationAsync(
                    withdrawalRequest.UserId,
                    withdrawalRequestId,
                    withdrawalRequest.Amount,
                    bankAccountInfo,
                    transactionReference);

                return ApiResponse<bool>.Success(true, "Duyệt yêu cầu rút tiền thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Lỗi khi duyệt yêu cầu rút tiền: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin từ chối yêu cầu rút tiền
        /// </summary>
        public async Task<ApiResponse<bool>> RejectRequestAsync(
            Guid withdrawalRequestId, 
            Guid adminId, 
            string reason)
        {
            try
            {
                var withdrawalRequest = await _unitOfWork.WithdrawalRequestRepository.GetWithDetailsAsync(withdrawalRequestId);
                
                if (withdrawalRequest == null)
                {
                    return ApiResponse<bool>.NotFound("Không tìm thấy yêu cầu rút tiền");
                }

                if (withdrawalRequest.Status != WithdrawalStatus.Pending)
                {
                    return ApiResponse<bool>.Error("Chỉ có thể từ chối yêu cầu đang chờ xử lý");
                }

                var success = await _unitOfWork.WithdrawalRequestRepository.UpdateStatusAsync(
                    withdrawalRequestId,
                    WithdrawalStatus.Rejected,
                    adminId,
                    reason);

                if (success)
                {
                    // Tạo notification cho user
                    await _notificationService.CreateWithdrawalRejectedNotificationAsync(
                        withdrawalRequest.UserId,
                        withdrawalRequestId,
                        withdrawalRequest.Amount,
                        reason);

                    return ApiResponse<bool>.Success(true, "Từ chối yêu cầu rút tiền thành công");
                }
                else
                {
                    return ApiResponse<bool>.Error("Không thể từ chối yêu cầu rút tiền");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Lỗi khi từ chối yêu cầu rút tiền: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thống kê yêu cầu rút tiền
        /// </summary>
        public async Task<ApiResponse<WithdrawalStatsDto>> GetStatsAsync(Guid? userId = null)
        {
            try
            {
                // Implementation sẽ được thêm sau khi có đủ data
                var stats = new WithdrawalStatsDto
                {
                    TotalRequests = await _unitOfWork.WithdrawalRequestRepository.CountByStatusAsync(WithdrawalStatus.Pending) +
                                   await _unitOfWork.WithdrawalRequestRepository.CountByStatusAsync(WithdrawalStatus.Approved) +
                                   await _unitOfWork.WithdrawalRequestRepository.CountByStatusAsync(WithdrawalStatus.Rejected),
                    PendingRequests = await _unitOfWork.WithdrawalRequestRepository.CountByStatusAsync(WithdrawalStatus.Pending),
                    ApprovedRequests = await _unitOfWork.WithdrawalRequestRepository.CountByStatusAsync(WithdrawalStatus.Approved),
                    RejectedRequests = await _unitOfWork.WithdrawalRequestRepository.CountByStatusAsync(WithdrawalStatus.Rejected),
                    PendingAmount = await _unitOfWork.WithdrawalRequestRepository.GetTotalPendingAmountAsync()
                };

                return ApiResponse<WithdrawalStatsDto>.Success(stats);
            }
            catch (Exception ex)
            {
                return ApiResponse<WithdrawalStatsDto>.Error($"Lỗi khi lấy thống kê: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra user có thể tạo yêu cầu rút tiền mới không
        /// </summary>
        public async Task<ApiResponse<bool>> CanCreateNewRequestAsync(Guid userId)
        {
            try
            {
                // Kiểm tra có yêu cầu pending nào không
                var hasPendingRequest = await _unitOfWork.WithdrawalRequestRepository.HasPendingRequestAsync(userId);
                if (hasPendingRequest)
                {
                    return ApiResponse<bool>.Success(false, "Bạn đã có yêu cầu rút tiền đang chờ xử lý");
                }

                // Kiểm tra có tài khoản ngân hàng không
                var hasBankAccount = await _unitOfWork.BankAccountRepository.HasBankAccountAsync(userId);
                if (!hasBankAccount)
                {
                    return ApiResponse<bool>.Success(false, "Bạn cần thêm tài khoản ngân hàng trước khi rút tiền");
                }

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Lỗi khi kiểm tra điều kiện tạo yêu cầu: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy yêu cầu rút tiền gần nhất của user
        /// </summary>
        public async Task<ApiResponse<WithdrawalRequestResponseDto?>> GetLatestRequestAsync(Guid userId)
        {
            try
            {
                var latestRequest = await _unitOfWork.WithdrawalRequestRepository.GetLatestByUserIdAsync(userId);
                
                if (latestRequest == null)
                {
                    return ApiResponse<WithdrawalRequestResponseDto?>.Success(null, "Chưa có yêu cầu rút tiền nào");
                }

                var responseDto = MapToResponseDto(latestRequest);
                return ApiResponse<WithdrawalRequestResponseDto?>.Success(responseDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<WithdrawalRequestResponseDto?>.Error($"Lỗi khi lấy yêu cầu rút tiền gần nhất: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate yêu cầu rút tiền trước khi tạo
        /// </summary>
        public async Task<ApiResponse<bool>> ValidateWithdrawalRequestAsync(Guid userId, decimal amount, Guid bankAccountId)
        {
            try
            {
                // Validate amount
                if (amount <= 0)
                {
                    return ApiResponse<bool>.Error("Số tiền rút phải lớn hơn 0");
                }

                if (amount < 1000)
                {
                    return ApiResponse<bool>.Error("Số tiền rút tối thiểu là 1,000 VNĐ");
                }

                // Validate bank account ownership
                var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAndUserIdAsync(bankAccountId, userId);
                if (bankAccount == null)
                {
                    return ApiResponse<bool>.Error("Tài khoản ngân hàng không tồn tại hoặc không thuộc về bạn");
                }

                // Validate wallet balance (hỗ trợ cả TourCompany và SpecialtyShop)
                var walletResponse = await _walletService.GetWalletByUserRoleAsync(userId);
                if (!walletResponse.IsSuccess)
                {
                    return ApiResponse<bool>.Error("Không thể lấy thông tin ví");
                }

                var currentBalance = walletResponse.Data.AvailableBalance;
                if (currentBalance < amount)
                {
                    return ApiResponse<bool>.Error($"Số dư ví không đủ. Số dư hiện tại: {currentBalance:N0} VNĐ");
                }

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Lỗi khi validate yêu cầu rút tiền: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thống kê yêu cầu rút tiền theo role cho TourCompany và SpecialtyShop
        /// </summary>
        public async Task<ApiResponse<WithdrawalRoleStatsSummaryDto>> GetRoleStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Lấy thống kê cho TourCompany
                var tourCompanyStats = await _unitOfWork.WithdrawalRequestRepository.GetStatsByRoleAsync("Tour Company", startDate, endDate);
                
                // Lấy thống kê cho SpecialtyShop
                var specialtyShopStats = await _unitOfWork.WithdrawalRequestRepository.GetStatsByRoleAsync("Specialty Shop", startDate, endDate);

                var summary = new WithdrawalRoleStatsSummaryDto
                {
                    TourCompanyStats = new WithdrawalRoleStatsDto
                    {
                        Role = "Tour Company",
                        TotalRequests = tourCompanyStats.TotalRequests,
                        PendingRequests = tourCompanyStats.PendingRequests,
                        ApprovedRequests = tourCompanyStats.ApprovedRequests,
                        RejectedRequests = tourCompanyStats.RejectedRequests,
                        TotalAmountRequested = tourCompanyStats.TotalAmount,
                        PendingAmount = tourCompanyStats.PendingAmount,
                        ApprovedAmount = tourCompanyStats.ApprovedAmount,
                        RejectedAmount = tourCompanyStats.RejectedAmount,
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    SpecialtyShopStats = new WithdrawalRoleStatsDto
                    {
                        Role = "Specialty Shop",
                        TotalRequests = specialtyShopStats.TotalRequests,
                        PendingRequests = specialtyShopStats.PendingRequests,
                        ApprovedRequests = specialtyShopStats.ApprovedRequests,
                        RejectedRequests = specialtyShopStats.RejectedRequests,
                        TotalAmountRequested = specialtyShopStats.TotalAmount,
                        PendingAmount = specialtyShopStats.PendingAmount,
                        ApprovedAmount = specialtyShopStats.ApprovedAmount,
                        RejectedAmount = specialtyShopStats.RejectedAmount,
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    GeneratedAt = DateTime.UtcNow,
                    StartDate = startDate,
                    EndDate = endDate
                };

                return ApiResponse<WithdrawalRoleStatsSummaryDto>.Success(summary, "Lấy thống kê thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<WithdrawalRoleStatsSummaryDto>.Error($"Lỗi khi lấy thống kê theo role: {ex.Message}");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Map WithdrawalRequest entity to WithdrawalRequestResponseDto
        /// </summary>
        private static WithdrawalRequestResponseDto MapToResponseDto(WithdrawalRequest withdrawalRequest)
        {
            return new WithdrawalRequestResponseDto
            {
                Id = withdrawalRequest.Id,
                UserId = withdrawalRequest.UserId,
                BankAccountId = withdrawalRequest.BankAccountId,
                BankAccount = new BankAccountInfo
                {
                    Id = withdrawalRequest.BankAccount.Id,
                    BankName = withdrawalRequest.BankAccount.BankName,
                    MaskedAccountNumber = MaskAccountNumber(withdrawalRequest.BankAccount.AccountNumber),
                    AccountHolderName = withdrawalRequest.BankAccount.AccountHolderName
                },
                Amount = withdrawalRequest.Amount,
                WithdrawalFee = withdrawalRequest.WithdrawalFee,
                NetAmount = withdrawalRequest.NetAmount,
                Status = withdrawalRequest.Status,
                StatusName = GetStatusName(withdrawalRequest.Status),
                RequestedAt = withdrawalRequest.RequestedAt,
                ProcessedAt = withdrawalRequest.ProcessedAt,
                ProcessedByName = withdrawalRequest.ProcessedBy?.Name,
                UserNotes = withdrawalRequest.UserNotes,
                AdminNotes = withdrawalRequest.AdminNotes,
                TransactionReference = withdrawalRequest.TransactionReference,
                CanCancel = withdrawalRequest.Status == WithdrawalStatus.Pending,
                WalletBalanceAtRequest = withdrawalRequest.WalletBalanceAtRequest
            };
        }

        /// <summary>
        /// Map WithdrawalRequest entity to WithdrawalRequestDetailDto
        /// </summary>
        private static WithdrawalRequestDetailDto MapToDetailDto(WithdrawalRequest withdrawalRequest)
        {
            var baseDto = MapToResponseDto(withdrawalRequest);
            
            return new WithdrawalRequestDetailDto
            {
                Id = baseDto.Id,
                UserId = baseDto.UserId,
                BankAccountId = baseDto.BankAccountId,
                BankAccount = baseDto.BankAccount,
                Amount = baseDto.Amount,
                WithdrawalFee = baseDto.WithdrawalFee,
                NetAmount = baseDto.NetAmount,
                Status = baseDto.Status,
                StatusName = baseDto.StatusName,
                RequestedAt = baseDto.RequestedAt,
                ProcessedAt = baseDto.ProcessedAt,
                ProcessedByName = baseDto.ProcessedByName,
                UserNotes = baseDto.UserNotes,
                AdminNotes = baseDto.AdminNotes,
                TransactionReference = baseDto.TransactionReference,
                CanCancel = baseDto.CanCancel,
                WalletBalanceAtRequest = baseDto.WalletBalanceAtRequest,
                User = new UserInfo
                {
                    Id = withdrawalRequest.User.Id,
                    FullName = withdrawalRequest.User.Name,
                    Email = withdrawalRequest.User.Email,
                    PhoneNumber = withdrawalRequest.User.PhoneNumber
                },
                ProcessedBy = withdrawalRequest.ProcessedBy != null ? new AdminInfo
                {
                    Id = withdrawalRequest.ProcessedBy.Id,
                    FullName = withdrawalRequest.ProcessedBy.Name,
                    Email = withdrawalRequest.ProcessedBy.Email
                } : null
            };
        }

        /// <summary>
        /// Map WithdrawalRequest entity to WithdrawalRequestAdminDto
        /// </summary>
        private static WithdrawalRequestAdminDto MapToAdminDto(WithdrawalRequest withdrawalRequest)
        {
            var daysPending = withdrawalRequest.Status == WithdrawalStatus.Pending 
                ? (DateTime.UtcNow - withdrawalRequest.RequestedAt).Days 
                : 0;

            var priority = daysPending > 7 ? "High" : daysPending > 3 ? "Medium" : "Normal";

            return new WithdrawalRequestAdminDto
            {
                Id = withdrawalRequest.Id,
                User = new UserSummary
                {
                    Id = withdrawalRequest.User.Id,
                    FullName = withdrawalRequest.User.Name,
                    Email = withdrawalRequest.User.Email,
                    PhoneNumber = withdrawalRequest.User.PhoneNumber,
                    ShopName = withdrawalRequest.User.SpecialtyShop?.ShopName
                },
                BankAccount = new BankAccountSummary
                {
                    Id = withdrawalRequest.BankAccount.Id,
                    BankName = withdrawalRequest.BankAccount.BankName,
                    AccountNumber = withdrawalRequest.BankAccount.AccountNumber, // Full number for admin
                    AccountHolderName = withdrawalRequest.BankAccount.AccountHolderName,
                    IsVerified = withdrawalRequest.BankAccount.VerifiedAt.HasValue
                },
                Amount = withdrawalRequest.Amount,
                WithdrawalFee = withdrawalRequest.WithdrawalFee,
                NetAmount = withdrawalRequest.NetAmount,
                Status = withdrawalRequest.Status,
                StatusName = GetStatusName(withdrawalRequest.Status),
                RequestedAt = withdrawalRequest.RequestedAt,
                ProcessedAt = withdrawalRequest.ProcessedAt,
                ProcessedByName = withdrawalRequest.ProcessedBy?.Name,
                UserNotes = withdrawalRequest.UserNotes,
                AdminNotes = withdrawalRequest.AdminNotes,
                TransactionReference = withdrawalRequest.TransactionReference,
                WalletBalanceAtRequest = withdrawalRequest.WalletBalanceAtRequest,
                DaysPending = daysPending,
                Priority = priority
            };
        }

        /// <summary>
        /// Get status display name
        /// </summary>
        private static string GetStatusName(WithdrawalStatus status)
        {
            return status switch
            {
                WithdrawalStatus.Pending => "Đang chờ xử lý",
                WithdrawalStatus.Approved => "Đã duyệt",
                WithdrawalStatus.Rejected => "Bị từ chối",
                WithdrawalStatus.Cancelled => "Đã hủy",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Mask account number for security (show only last 4 digits)
        /// </summary>
        private static string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length <= 4)
                return accountNumber;

            var lastFour = accountNumber.Substring(accountNumber.Length - 4);
            var masked = new string('*', accountNumber.Length - 4) + lastFour;
            return masked;
        }

        #endregion
    }
}
