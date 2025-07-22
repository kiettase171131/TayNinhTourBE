using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.BankAccount;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.BankAccount;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho quản lý tài khoản ngân hàng của user
    /// Implement business logic và validation cho BankAccount operations
    /// </summary>
    public class BankAccountService : IBankAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy danh sách tài khoản ngân hàng của user hiện tại
        /// </summary>
        public async Task<ApiResponse<List<BankAccountResponseDto>>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                var bankAccounts = await _unitOfWork.BankAccountRepository.GetWithVerificationInfoAsync(userId);
                
                var responseDtos = bankAccounts.Select(MapToResponseDto).ToList();
                
                return ApiResponse<List<BankAccountResponseDto>>.Success(responseDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BankAccountResponseDto>>.Error($"Lỗi khi lấy danh sách tài khoản ngân hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một tài khoản ngân hàng
        /// </summary>
        public async Task<ApiResponse<BankAccountResponseDto>> GetByIdAsync(Guid bankAccountId, Guid currentUserId)
        {
            try
            {
                var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAndUserIdAsync(bankAccountId, currentUserId);
                
                if (bankAccount == null)
                {
                    return ApiResponse<BankAccountResponseDto>.NotFound("Không tìm thấy tài khoản ngân hàng hoặc bạn không có quyền truy cập");
                }

                var bankAccountWithDetails = await _unitOfWork.BankAccountRepository.GetWithWithdrawalRequestsAsync(bankAccountId);
                var responseDto = MapToResponseDto(bankAccountWithDetails!);
                
                return ApiResponse<BankAccountResponseDto>.Success(responseDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountResponseDto>.Error($"Lỗi khi lấy thông tin tài khoản ngân hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy tài khoản ngân hàng mặc định của user
        /// </summary>
        public async Task<ApiResponse<BankAccountResponseDto?>> GetDefaultByUserIdAsync(Guid userId)
        {
            try
            {
                var defaultBankAccount = await _unitOfWork.BankAccountRepository.GetDefaultByUserIdAsync(userId);
                
                if (defaultBankAccount == null)
                {
                    return ApiResponse<BankAccountResponseDto?>.Success(null, "Chưa có tài khoản ngân hàng mặc định");
                }

                var responseDto = MapToResponseDto(defaultBankAccount);
                return ApiResponse<BankAccountResponseDto?>.Success(responseDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountResponseDto?>.Error($"Lỗi khi lấy tài khoản ngân hàng mặc định: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo tài khoản ngân hàng mới
        /// </summary>
        public async Task<ApiResponse<BankAccountResponseDto>> CreateAsync(CreateBankAccountDto createDto, Guid userId)
        {
            try
            {
                // Validate duplicate
                var validationResult = await ValidateBankAccountAsync(createDto.BankName, createDto.AccountNumber, userId);
                if (!validationResult.IsSuccess)
                {
                    return ApiResponse<BankAccountResponseDto>.Error(validationResult.Message);
                }

                // Nếu đây là tài khoản đầu tiên hoặc được đặt làm default
                var hasExistingAccounts = await _unitOfWork.BankAccountRepository.HasBankAccountAsync(userId);
                var shouldSetDefault = createDto.IsDefault || !hasExistingAccounts;

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Unset default cũ nếu cần
                    if (shouldSetDefault)
                    {
                        await _unitOfWork.BankAccountRepository.UnsetAllDefaultAsync(userId);
                    }

                    // Tạo bank account mới
                    var bankAccount = new BankAccount
                    {
                        UserId = userId,
                        BankName = createDto.BankName.Trim(),
                        AccountNumber = createDto.AccountNumber.Trim(),
                        AccountHolderName = createDto.AccountHolderName.Trim(),
                        IsDefault = shouldSetDefault,
                        Notes = createDto.Notes?.Trim(),
                        IsActive = true
                    };

                    await _unitOfWork.BankAccountRepository.AddAsync(bankAccount);
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var responseDto = MapToResponseDto(bankAccount);
                    return ApiResponse<BankAccountResponseDto>.Success(responseDto, "Tạo tài khoản ngân hàng thành công");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountResponseDto>.Error($"Lỗi khi tạo tài khoản ngân hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản ngân hàng
        /// </summary>
        public async Task<ApiResponse<BankAccountResponseDto>> UpdateAsync(Guid bankAccountId, UpdateBankAccountDto updateDto, Guid currentUserId)
        {
            try
            {
                var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAndUserIdAsync(bankAccountId, currentUserId);
                
                if (bankAccount == null)
                {
                    return ApiResponse<BankAccountResponseDto>.NotFound("Không tìm thấy tài khoản ngân hàng hoặc bạn không có quyền cập nhật");
                }

                // Validate duplicate (exclude current account)
                var validationResult = await ValidateBankAccountAsync(updateDto.BankName, updateDto.AccountNumber, currentUserId, bankAccountId);
                if (!validationResult.IsSuccess)
                {
                    return ApiResponse<BankAccountResponseDto>.Error(validationResult.Message);
                }

                // Update properties
                bankAccount.BankName = updateDto.BankName.Trim();
                bankAccount.AccountNumber = updateDto.AccountNumber.Trim();
                bankAccount.AccountHolderName = updateDto.AccountHolderName.Trim();
                bankAccount.Notes = updateDto.Notes?.Trim();

                await _unitOfWork.BankAccountRepository.UpdateAsync(bankAccount);
                await _unitOfWork.SaveChangesAsync();

                var responseDto = MapToResponseDto(bankAccount);
                return ApiResponse<BankAccountResponseDto>.Success(responseDto, "Cập nhật tài khoản ngân hàng thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<BankAccountResponseDto>.Error($"Lỗi khi cập nhật tài khoản ngân hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa tài khoản ngân hàng (soft delete)
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteAsync(Guid bankAccountId, Guid currentUserId)
        {
            try
            {
                var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAndUserIdAsync(bankAccountId, currentUserId);
                
                if (bankAccount == null)
                {
                    return ApiResponse<bool>.NotFound("Không tìm thấy tài khoản ngân hàng hoặc bạn không có quyền xóa");
                }

                // Kiểm tra có thể xóa không
                var canDelete = await _unitOfWork.BankAccountRepository.CanDeleteAsync(bankAccountId);
                if (!canDelete)
                {
                    return ApiResponse<bool>.Error("Không thể xóa tài khoản ngân hàng này vì có yêu cầu rút tiền đang chờ xử lý");
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Soft delete
                    await _unitOfWork.BankAccountRepository.DeleteAsync(bankAccount.Id);
                    
                    // Nếu xóa tài khoản default, set tài khoản khác làm default
                    if (bankAccount.IsDefault)
                    {
                        var remainingAccounts = await _unitOfWork.BankAccountRepository.GetByUserIdAsync(currentUserId);
                        var firstRemaining = remainingAccounts.FirstOrDefault();
                        if (firstRemaining != null)
                        {
                            firstRemaining.IsDefault = true;
                            await _unitOfWork.BankAccountRepository.UpdateAsync(firstRemaining);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return ApiResponse<bool>.Success(true, "Xóa tài khoản ngân hàng thành công");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Lỗi khi xóa tài khoản ngân hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Đặt tài khoản ngân hàng làm mặc định
        /// </summary>
        public async Task<ApiResponse<bool>> SetDefaultAsync(Guid bankAccountId, Guid currentUserId)
        {
            try
            {
                var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAndUserIdAsync(bankAccountId, currentUserId);
                
                if (bankAccount == null)
                {
                    return ApiResponse<bool>.NotFound("Không tìm thấy tài khoản ngân hàng hoặc bạn không có quyền cập nhật");
                }

                if (bankAccount.IsDefault)
                {
                    return ApiResponse<bool>.Success(true, "Tài khoản này đã là tài khoản mặc định");
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Unset tất cả default cũ
                    await _unitOfWork.BankAccountRepository.UnsetAllDefaultAsync(currentUserId);
                    
                    // Set account này làm default
                    bankAccount.IsDefault = true;
                    await _unitOfWork.BankAccountRepository.UpdateAsync(bankAccount);
                    
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return ApiResponse<bool>.Success(true, "Đặt tài khoản mặc định thành công");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Lỗi khi đặt tài khoản mặc định: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra user có tài khoản ngân hàng nào không
        /// </summary>
        public async Task<bool> HasBankAccountAsync(Guid userId)
        {
            return await _unitOfWork.BankAccountRepository.HasBankAccountAsync(userId);
        }

        /// <summary>
        /// Validate thông tin tài khoản ngân hàng
        /// </summary>
        public async Task<ApiResponse<bool>> ValidateBankAccountAsync(string bankName, string accountNumber, Guid userId, Guid? excludeBankAccountId = null)
        {
            try
            {
                // Kiểm tra duplicate
                var exists = await _unitOfWork.BankAccountRepository.ExistsAsync(userId, bankName, accountNumber, excludeBankAccountId);
                if (exists)
                {
                    return ApiResponse<bool>.Error("Tài khoản ngân hàng này đã tồn tại");
                }

                // Có thể thêm validation khác ở đây (format số tài khoản, bank name, etc.)
                
                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Lỗi khi validate tài khoản ngân hàng: {ex.Message}");
            }
        }

        /// <summary>
        /// Map BankAccount entity to BankAccountResponseDto
        /// </summary>
        private static BankAccountResponseDto MapToResponseDto(BankAccount bankAccount)
        {
            return new BankAccountResponseDto
            {
                Id = bankAccount.Id,
                UserId = bankAccount.UserId,
                BankName = bankAccount.BankName,
                AccountNumber = bankAccount.AccountNumber,
                MaskedAccountNumber = MaskAccountNumber(bankAccount.AccountNumber),
                AccountHolderName = bankAccount.AccountHolderName,
                IsDefault = bankAccount.IsDefault,
                Notes = bankAccount.Notes,
                VerifiedAt = bankAccount.VerifiedAt,
                VerifiedByName = bankAccount.VerifiedBy?.Name,
                IsActive = bankAccount.IsActive,
                CreatedAt = bankAccount.CreatedAt,
                UpdatedAt = bankAccount.UpdatedAt,
                WithdrawalRequestCount = bankAccount.WithdrawalRequests?.Count ?? 0,
                CanDelete = bankAccount.WithdrawalRequests?.All(w => w.Status != DataAccessLayer.Enums.WithdrawalStatus.Pending) ?? true
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
    }
}
