using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Wallet;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho qu?n lý ví ti?n c?a c? TourCompany và SpecialtyShop
    /// </summary>
    public class WalletService : BaseService, IWalletService
    {
        public WalletService(IMapper mapper, IUnitOfWork unitOfWork) : base(mapper, unitOfWork)
        {
        }

        /// <summary>
        /// L?y thông tin ví c?a TourCompany theo UserId
        /// </summary>
        public async Task<ApiResponse<TourCompanyWalletDto>> GetTourCompanyWalletAsync(Guid userId)
        {
            try
            {
                var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);

                if (tourCompany == null)
                {
                    return ApiResponse<TourCompanyWalletDto>.NotFound("Không tìm th?y thông tin công ty tour. Vui lòng liên h? qu?n tr? viên.");
                }

                if (!tourCompany.IsActive)
                {
                    return ApiResponse<TourCompanyWalletDto>.BadRequest("Tài kho?n công ty tour ?ã b? vô hi?u hóa.");
                }

                var walletDto = new TourCompanyWalletDto
                {
                    Id = tourCompany.Id,
                    UserId = tourCompany.UserId,
                    CompanyName = tourCompany.CompanyName,
                    Wallet = tourCompany.Wallet,
                    RevenueHold = tourCompany.RevenueHold,
                    UpdatedAt = tourCompany.UpdatedAt ?? tourCompany.CreatedAt
                };

                return ApiResponse<TourCompanyWalletDto>.Success(walletDto, "L?y thông tin ví công ty tour thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<TourCompanyWalletDto>.Error(500, $"L?i khi l?y thông tin ví: {ex.Message}");
            }
        }

        /// <summary>
        /// L?y thông tin ví c?a SpecialtyShop theo UserId
        /// </summary>
        public async Task<ApiResponse<SpecialtyShopWalletDto>> GetSpecialtyShopWalletAsync(Guid userId)
        {
            try
            {
                var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);

                if (specialtyShop == null)
                {
                    return ApiResponse<SpecialtyShopWalletDto>.NotFound("Không tìm th?y thông tin shop. Vui lòng ??ng ký làm shop tr??c.");
                }

                if (!specialtyShop.IsActive)
                {
                    return ApiResponse<SpecialtyShopWalletDto>.BadRequest("Tài kho?n shop ?ã b? vô hi?u hóa.");
                }

                var walletDto = new SpecialtyShopWalletDto
                {
                    Id = specialtyShop.Id,
                    UserId = specialtyShop.UserId,
                    ShopName = specialtyShop.ShopName,
                    Wallet = specialtyShop.Wallet,
                    UpdatedAt = specialtyShop.UpdatedAt ?? specialtyShop.CreatedAt
                };

                return ApiResponse<SpecialtyShopWalletDto>.Success(walletDto, "L?y thông tin ví shop thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<SpecialtyShopWalletDto>.Error(500, $"L?i khi l?y thông tin ví: {ex.Message}");
            }
        }

        /// <summary>
        /// L?y thông tin ví theo role c?a user hi?n t?i
        /// </summary>
        public async Task<ApiResponse<WalletInfoDto>> GetWalletByUserRoleAsync(Guid userId)
        {
            try
            {
                // Ki?m tra xem user có ph?i TourCompany không
                var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);
                if (tourCompany != null && tourCompany.IsActive)
                {
                    var tourCompanyWallet = new WalletInfoDto
                    {
                        WalletType = "TourCompany",
                        OwnerName = tourCompany.CompanyName,
                        AvailableBalance = tourCompany.Wallet,
                        HoldBalance = tourCompany.RevenueHold,
                        TotalBalance = tourCompany.Wallet + tourCompany.RevenueHold,
                        UpdatedAt = tourCompany.UpdatedAt ?? tourCompany.CreatedAt
                    };

                    return ApiResponse<WalletInfoDto>.Success(tourCompanyWallet, "L?y thông tin ví công ty tour thành công");
                }

                // Ki?m tra xem user có ph?i SpecialtyShop không
                var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);
                if (specialtyShop != null && specialtyShop.IsActive)
                {
                    var specialtyShopWallet = new WalletInfoDto
                    {
                        WalletType = "SpecialtyShop",
                        OwnerName = specialtyShop.ShopName,
                        AvailableBalance = specialtyShop.Wallet,
                        HoldBalance = null, // SpecialtyShop không có hold balance
                        TotalBalance = specialtyShop.Wallet,
                        UpdatedAt = specialtyShop.UpdatedAt ?? specialtyShop.CreatedAt
                    };

                    return ApiResponse<WalletInfoDto>.Success(specialtyShopWallet, "L?y thông tin ví shop thành công");
                }

                return ApiResponse<WalletInfoDto>.NotFound("B?n ch?a có ví ti?n. Vui lòng ??ng ký làm công ty tour ho?c shop ?? có ví.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WalletInfoDto>.Error(500, $"L?i khi l?y thông tin ví: {ex.Message}");
            }
        }

        /// <summary>
        /// Ki?m tra user có ví không
        /// </summary>
        public async Task<bool> HasWalletAsync(Guid userId)
        {
            try
            {
                // Ki?m tra TourCompany
                var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);
                if (tourCompany != null && tourCompany.IsActive)
                    return true;

                // Ki?m tra SpecialtyShop
                var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);
                if (specialtyShop != null && specialtyShop.IsActive)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// L?y lo?i role có ví c?a user
        /// </summary>
        public async Task<string?> GetUserWalletTypeAsync(Guid userId)
        {
            try
            {
                // Ki?m tra TourCompany tr??c
                var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);
                if (tourCompany != null && tourCompany.IsActive)
                    return "TourCompany";

                // Ki?m tra SpecialtyShop
                var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);
                if (specialtyShop != null && specialtyShop.IsActive)
                    return "SpecialtyShop";

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// X? lý rút ti?n t? ví (h? tr? c? TourCompany và SpecialtyShop)
        /// Method này ???c g?i khi admin approve withdrawal request
        /// </summary>
        /// <param name="userId">ID c?a user s? h?u wallet</param>
        /// <param name="amount">S? ti?n c?n tr?</param>
        /// <param name="withdrawalRequestId">ID c?a withdrawal request (?? audit)</param>
        /// <returns>K?t qu? x? lý rút ti?n</returns>
        public async Task<ApiResponse<bool>> ProcessWithdrawalAsync(Guid userId, decimal amount, Guid withdrawalRequestId)
        {
            try
            {
                if (amount <= 0)
                {
                    return ApiResponse<bool>.BadRequest("S? ti?n rút ph?i l?n h?n 0");
                }

                // Ki?m tra lo?i ví c?a user
                var walletType = await GetUserWalletTypeAsync(userId);
                if (string.IsNullOrEmpty(walletType))
                {
                    return ApiResponse<bool>.NotFound("Không tìm th?y thông tin ví");
                }

                if (walletType == "TourCompany")
                {
                    var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);
                    if (tourCompany == null)
                    {
                        return ApiResponse<bool>.NotFound("Không tìm th?y thông tin công ty tour");
                    }

                    if (!tourCompany.IsActive)
                    {
                        return ApiResponse<bool>.BadRequest("Công ty tour ?ã b? vô hi?u hóa");
                    }

                    if (tourCompany.Wallet < amount)
                    {
                        return ApiResponse<bool>.BadRequest($"S? d? ví không ??. S? d? hi?n t?i: {tourCompany.Wallet:N0} VN?, S? ti?n c?n rút: {amount:N0} VN?");
                    }

                    // Tr? ti?n t? ví TourCompany
                    tourCompany.Wallet -= amount;
                    await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);

                    return ApiResponse<bool>.Success(true, $"?ã tr? {amount:N0} VN? t? ví công ty tour {tourCompany.CompanyName}");
                }
                else if (walletType == "SpecialtyShop")
                {
                    var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);
                    if (specialtyShop == null)
                    {
                        return ApiResponse<bool>.NotFound("Không tìm th?y thông tin shop");
                    }

                    if (!specialtyShop.IsActive)
                    {
                        return ApiResponse<bool>.BadRequest("Shop ?ã b? vô hi?u hóa");
                    }

                    if (specialtyShop.Wallet < amount)
                    {
                        return ApiResponse<bool>.BadRequest($"S? d? ví không ??. S? d? hi?n t?i: {specialtyShop.Wallet:N0} VN?, S? ti?n c?n rút: {amount:N0} VN?");
                    }

                    // Tr? ti?n t? ví SpecialtyShop
                    specialtyShop.Wallet -= amount;
                    await _unitOfWork.SpecialtyShopRepository.UpdateAsync(specialtyShop);

                    return ApiResponse<bool>.Success(true, $"?ã tr? {amount:N0} VN? t? ví shop {specialtyShop.ShopName}");
                }
                else
                {
                    return ApiResponse<bool>.BadRequest("Lo?i ví không ???c h? tr?");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error(500, $"L?i khi x? lý rút ti?n: {ex.Message}");
            }
        }

        /// <summary>
        /// Hoàn ti?n vào ví (h? tr? c? TourCompany và SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID c?a user s? h?u wallet</param>
        /// <param name="amount">S? ti?n c?n hoàn</param>
        /// <param name="reason">Lý do hoàn ti?n</param>
        /// <returns>K?t qu? hoàn ti?n</returns>
        public async Task<ApiResponse<bool>> RefundToWalletAsync(Guid userId, decimal amount, string reason)
        {
            try
            {
                if (amount <= 0)
                {
                    return ApiResponse<bool>.BadRequest("S? ti?n hoàn ph?i l?n h?n 0");
                }

                // Ki?m tra lo?i ví c?a user
                var walletType = await GetUserWalletTypeAsync(userId);
                if (string.IsNullOrEmpty(walletType))
                {
                    return ApiResponse<bool>.NotFound("Không tìm th?y thông tin ví");
                }

                if (walletType == "TourCompany")
                {
                    var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);
                    if (tourCompany == null)
                    {
                        return ApiResponse<bool>.NotFound("Không tìm th?y thông tin công ty tour");
                    }

                    if (!tourCompany.IsActive)
                    {
                        return ApiResponse<bool>.BadRequest("Công ty tour ?ã b? vô hi?u hóa");
                    }

                    // C?ng ti?n vào ví TourCompany
                    tourCompany.Wallet += amount;
                    await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);

                    return ApiResponse<bool>.Success(true, $"?ã hoàn {amount:N0} VN? vào ví công ty tour {tourCompany.CompanyName}. Lý do: {reason}");
                }
                else if (walletType == "SpecialtyShop")
                {
                    var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);
                    if (specialtyShop == null)
                    {
                        return ApiResponse<bool>.NotFound("Không tìm th?y thông tin shop");
                    }

                    if (!specialtyShop.IsActive)
                    {
                        return ApiResponse<bool>.BadRequest("Shop ?ã b? vô hi?u hóa");
                    }

                    // C?ng ti?n vào ví SpecialtyShop
                    specialtyShop.Wallet += amount;
                    await _unitOfWork.SpecialtyShopRepository.UpdateAsync(specialtyShop);

                    return ApiResponse<bool>.Success(true, $"?ã hoàn {amount:N0} VN? vào ví shop {specialtyShop.ShopName}. Lý do: {reason}");
                }
                else
                {
                    return ApiResponse<bool>.BadRequest("Lo?i ví không ???c h? tr?");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error(500, $"L?i khi hoàn ti?n: {ex.Message}");
            }
        }

        /// <summary>
        /// Ki?m tra s? d? ví có ?? ?? rút không (h? tr? c? TourCompany và SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="amount">S? ti?n mu?n rút</param>
        /// <returns>True n?u ?? s? d?</returns>
        public async Task<ApiResponse<bool>> CheckSufficientBalanceAsync(Guid userId, decimal amount)
        {
            try
            {
                // L?y thông tin ví theo role c?a user
                var walletResponse = await GetWalletByUserRoleAsync(userId);
                if (!walletResponse.IsSuccess)
                {
                    return ApiResponse<bool>.Error(walletResponse.StatusCode, walletResponse.Message);
                }

                var currentBalance = walletResponse.Data.AvailableBalance;
                var hasSufficientBalance = currentBalance >= amount;

                if (hasSufficientBalance)
                {
                    return ApiResponse<bool>.Success(true, "S? d? ví ?? ?? th?c hi?n rút ti?n");
                }
                else
                {
                    return ApiResponse<bool>.Success(false, $"S? d? ví không ??. S? d? hi?n t?i: {currentBalance:N0} VN?, S? ti?n mu?n rút: {amount:N0} VN?");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error(500, $"L?i khi ki?m tra s? d?: {ex.Message}");
            }
        }

        /// <summary>
        /// L?y s? d? ví hi?n t?i (h? tr? c? TourCompany và SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>S? d? ví hi?n t?i</returns>
        public async Task<ApiResponse<decimal>> GetCurrentWalletBalanceAsync(Guid userId)
        {
            try
            {
                var walletResponse = await GetWalletByUserRoleAsync(userId);
                if (!walletResponse.IsSuccess)
                {
                    return ApiResponse<decimal>.Error(walletResponse.StatusCode, walletResponse.Message);
                }

                return ApiResponse<decimal>.Success(walletResponse.Data.AvailableBalance, "L?y s? d? ví thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.Error(500, $"L?i khi l?y s? d? ví: {ex.Message}");
            }
        }
    }
}