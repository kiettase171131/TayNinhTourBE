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
    }
}