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
    }
}