using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Wallet;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho qu?n l� v� ti?n c?a c? TourCompany v� SpecialtyShop
    /// </summary>
    public class WalletService : BaseService, IWalletService
    {
        public WalletService(IMapper mapper, IUnitOfWork unitOfWork) : base(mapper, unitOfWork)
        {
        }

        /// <summary>
        /// L?y th�ng tin v� c?a TourCompany theo UserId
        /// </summary>
        public async Task<ApiResponse<TourCompanyWalletDto>> GetTourCompanyWalletAsync(Guid userId)
        {
            try
            {
                var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);

                if (tourCompany == null)
                {
                    return ApiResponse<TourCompanyWalletDto>.NotFound("Kh�ng t�m th?y th�ng tin c�ng ty tour. Vui l�ng li�n h? qu?n tr? vi�n.");
                }

                if (!tourCompany.IsActive)
                {
                    return ApiResponse<TourCompanyWalletDto>.BadRequest("T�i kho?n c�ng ty tour ?� b? v� hi?u h�a.");
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

                return ApiResponse<TourCompanyWalletDto>.Success(walletDto, "L?y th�ng tin v� c�ng ty tour th�nh c�ng");
            }
            catch (Exception ex)
            {
                return ApiResponse<TourCompanyWalletDto>.Error(500, $"L?i khi l?y th�ng tin v�: {ex.Message}");
            }
        }

        /// <summary>
        /// L?y th�ng tin v� c?a SpecialtyShop theo UserId
        /// </summary>
        public async Task<ApiResponse<SpecialtyShopWalletDto>> GetSpecialtyShopWalletAsync(Guid userId)
        {
            try
            {
                var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);

                if (specialtyShop == null)
                {
                    return ApiResponse<SpecialtyShopWalletDto>.NotFound("Kh�ng t�m th?y th�ng tin shop. Vui l�ng ??ng k� l�m shop tr??c.");
                }

                if (!specialtyShop.IsActive)
                {
                    return ApiResponse<SpecialtyShopWalletDto>.BadRequest("T�i kho?n shop ?� b? v� hi?u h�a.");
                }

                var walletDto = new SpecialtyShopWalletDto
                {
                    Id = specialtyShop.Id,
                    UserId = specialtyShop.UserId,
                    ShopName = specialtyShop.ShopName,
                    Wallet = specialtyShop.Wallet,
                    UpdatedAt = specialtyShop.UpdatedAt ?? specialtyShop.CreatedAt
                };

                return ApiResponse<SpecialtyShopWalletDto>.Success(walletDto, "L?y th�ng tin v� shop th�nh c�ng");
            }
            catch (Exception ex)
            {
                return ApiResponse<SpecialtyShopWalletDto>.Error(500, $"L?i khi l?y th�ng tin v�: {ex.Message}");
            }
        }

        /// <summary>
        /// L?y th�ng tin v� theo role c?a user hi?n t?i
        /// </summary>
        public async Task<ApiResponse<WalletInfoDto>> GetWalletByUserRoleAsync(Guid userId)
        {
            try
            {
                // Ki?m tra xem user c� ph?i TourCompany kh�ng
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

                    return ApiResponse<WalletInfoDto>.Success(tourCompanyWallet, "L?y th�ng tin v� c�ng ty tour th�nh c�ng");
                }

                // Ki?m tra xem user c� ph?i SpecialtyShop kh�ng
                var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);
                if (specialtyShop != null && specialtyShop.IsActive)
                {
                    var specialtyShopWallet = new WalletInfoDto
                    {
                        WalletType = "SpecialtyShop",
                        OwnerName = specialtyShop.ShopName,
                        AvailableBalance = specialtyShop.Wallet,
                        HoldBalance = null, // SpecialtyShop kh�ng c� hold balance
                        TotalBalance = specialtyShop.Wallet,
                        UpdatedAt = specialtyShop.UpdatedAt ?? specialtyShop.CreatedAt
                    };

                    return ApiResponse<WalletInfoDto>.Success(specialtyShopWallet, "L?y th�ng tin v� shop th�nh c�ng");
                }

                return ApiResponse<WalletInfoDto>.NotFound("B?n ch?a c� v� ti?n. Vui l�ng ??ng k� l�m c�ng ty tour ho?c shop ?? c� v�.");
            }
            catch (Exception ex)
            {
                return ApiResponse<WalletInfoDto>.Error(500, $"L?i khi l?y th�ng tin v�: {ex.Message}");
            }
        }

        /// <summary>
        /// Ki?m tra user c� v� kh�ng
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
        /// L?y lo?i role c� v� c?a user
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
        /// X? l� r�t ti?n t? v� (h? tr? c? TourCompany v� SpecialtyShop)
        /// Method n�y ???c g?i khi admin approve withdrawal request
        /// </summary>
        /// <param name="userId">ID c?a user s? h?u wallet</param>
        /// <param name="amount">S? ti?n c?n tr?</param>
        /// <param name="withdrawalRequestId">ID c?a withdrawal request (?? audit)</param>
        /// <returns>K?t qu? x? l� r�t ti?n</returns>
        public async Task<ApiResponse<bool>> ProcessWithdrawalAsync(Guid userId, decimal amount, Guid withdrawalRequestId)
        {
            try
            {
                if (amount <= 0)
                {
                    return ApiResponse<bool>.BadRequest("S? ti?n r�t ph?i l?n h?n 0");
                }

                // Ki?m tra lo?i v� c?a user
                var walletType = await GetUserWalletTypeAsync(userId);
                if (string.IsNullOrEmpty(walletType))
                {
                    return ApiResponse<bool>.NotFound("Kh�ng t�m th?y th�ng tin v�");
                }

                if (walletType == "TourCompany")
                {
                    var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);
                    if (tourCompany == null)
                    {
                        return ApiResponse<bool>.NotFound("Kh�ng t�m th?y th�ng tin c�ng ty tour");
                    }

                    if (!tourCompany.IsActive)
                    {
                        return ApiResponse<bool>.BadRequest("C�ng ty tour ?� b? v� hi?u h�a");
                    }

                    if (tourCompany.Wallet < amount)
                    {
                        return ApiResponse<bool>.BadRequest($"S? d? v� kh�ng ??. S? d? hi?n t?i: {tourCompany.Wallet:N0} VN?, S? ti?n c?n r�t: {amount:N0} VN?");
                    }

                    // Tr? ti?n t? v� TourCompany
                    tourCompany.Wallet -= amount;
                    await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);

                    return ApiResponse<bool>.Success(true, $"?� tr? {amount:N0} VN? t? v� c�ng ty tour {tourCompany.CompanyName}");
                }
                else if (walletType == "SpecialtyShop")
                {
                    var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);
                    if (specialtyShop == null)
                    {
                        return ApiResponse<bool>.NotFound("Kh�ng t�m th?y th�ng tin shop");
                    }

                    if (!specialtyShop.IsActive)
                    {
                        return ApiResponse<bool>.BadRequest("Shop ?� b? v� hi?u h�a");
                    }

                    if (specialtyShop.Wallet < amount)
                    {
                        return ApiResponse<bool>.BadRequest($"S? d? v� kh�ng ??. S? d? hi?n t?i: {specialtyShop.Wallet:N0} VN?, S? ti?n c?n r�t: {amount:N0} VN?");
                    }

                    // Tr? ti?n t? v� SpecialtyShop
                    specialtyShop.Wallet -= amount;
                    await _unitOfWork.SpecialtyShopRepository.UpdateAsync(specialtyShop);

                    return ApiResponse<bool>.Success(true, $"?� tr? {amount:N0} VN? t? v� shop {specialtyShop.ShopName}");
                }
                else
                {
                    return ApiResponse<bool>.BadRequest("Lo?i v� kh�ng ???c h? tr?");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error(500, $"L?i khi x? l� r�t ti?n: {ex.Message}");
            }
        }

        /// <summary>
        /// Ho�n ti?n v�o v� (h? tr? c? TourCompany v� SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID c?a user s? h?u wallet</param>
        /// <param name="amount">S? ti?n c?n ho�n</param>
        /// <param name="reason">L� do ho�n ti?n</param>
        /// <returns>K?t qu? ho�n ti?n</returns>
        public async Task<ApiResponse<bool>> RefundToWalletAsync(Guid userId, decimal amount, string reason)
        {
            try
            {
                if (amount <= 0)
                {
                    return ApiResponse<bool>.BadRequest("S? ti?n ho�n ph?i l?n h?n 0");
                }

                // Ki?m tra lo?i v� c?a user
                var walletType = await GetUserWalletTypeAsync(userId);
                if (string.IsNullOrEmpty(walletType))
                {
                    return ApiResponse<bool>.NotFound("Kh�ng t�m th?y th�ng tin v�");
                }

                if (walletType == "TourCompany")
                {
                    var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(userId);
                    if (tourCompany == null)
                    {
                        return ApiResponse<bool>.NotFound("Kh�ng t�m th?y th�ng tin c�ng ty tour");
                    }

                    if (!tourCompany.IsActive)
                    {
                        return ApiResponse<bool>.BadRequest("C�ng ty tour ?� b? v� hi?u h�a");
                    }

                    // C?ng ti?n v�o v� TourCompany
                    tourCompany.Wallet += amount;
                    await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);

                    return ApiResponse<bool>.Success(true, $"?� ho�n {amount:N0} VN? v�o v� c�ng ty tour {tourCompany.CompanyName}. L� do: {reason}");
                }
                else if (walletType == "SpecialtyShop")
                {
                    var specialtyShop = await _unitOfWork.SpecialtyShopRepository.GetByUserIdAsync(userId);
                    if (specialtyShop == null)
                    {
                        return ApiResponse<bool>.NotFound("Kh�ng t�m th?y th�ng tin shop");
                    }

                    if (!specialtyShop.IsActive)
                    {
                        return ApiResponse<bool>.BadRequest("Shop ?� b? v� hi?u h�a");
                    }

                    // C?ng ti?n v�o v� SpecialtyShop
                    specialtyShop.Wallet += amount;
                    await _unitOfWork.SpecialtyShopRepository.UpdateAsync(specialtyShop);

                    return ApiResponse<bool>.Success(true, $"?� ho�n {amount:N0} VN? v�o v� shop {specialtyShop.ShopName}. L� do: {reason}");
                }
                else
                {
                    return ApiResponse<bool>.BadRequest("Lo?i v� kh�ng ???c h? tr?");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error(500, $"L?i khi ho�n ti?n: {ex.Message}");
            }
        }

        /// <summary>
        /// Ki?m tra s? d? v� c� ?? ?? r�t kh�ng (h? tr? c? TourCompany v� SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <param name="amount">S? ti?n mu?n r�t</param>
        /// <returns>True n?u ?? s? d?</returns>
        public async Task<ApiResponse<bool>> CheckSufficientBalanceAsync(Guid userId, decimal amount)
        {
            try
            {
                // L?y th�ng tin v� theo role c?a user
                var walletResponse = await GetWalletByUserRoleAsync(userId);
                if (!walletResponse.IsSuccess)
                {
                    return ApiResponse<bool>.Error(walletResponse.StatusCode, walletResponse.Message);
                }

                var currentBalance = walletResponse.Data.AvailableBalance;
                var hasSufficientBalance = currentBalance >= amount;

                if (hasSufficientBalance)
                {
                    return ApiResponse<bool>.Success(true, "S? d? v� ?? ?? th?c hi?n r�t ti?n");
                }
                else
                {
                    return ApiResponse<bool>.Success(false, $"S? d? v� kh�ng ??. S? d? hi?n t?i: {currentBalance:N0} VN?, S? ti?n mu?n r�t: {amount:N0} VN?");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error(500, $"L?i khi ki?m tra s? d?: {ex.Message}");
            }
        }

        /// <summary>
        /// L?y s? d? v� hi?n t?i (h? tr? c? TourCompany v� SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID c?a user</param>
        /// <returns>S? d? v� hi?n t?i</returns>
        public async Task<ApiResponse<decimal>> GetCurrentWalletBalanceAsync(Guid userId)
        {
            try
            {
                var walletResponse = await GetWalletByUserRoleAsync(userId);
                if (!walletResponse.IsSuccess)
                {
                    return ApiResponse<decimal>.Error(walletResponse.StatusCode, walletResponse.Message);
                }

                return ApiResponse<decimal>.Success(walletResponse.Data.AvailableBalance, "L?y s? d? v� th�nh c�ng");
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.Error(500, $"L?i khi l?y s? d? v�: {ex.Message}");
            }
        }
    }
}