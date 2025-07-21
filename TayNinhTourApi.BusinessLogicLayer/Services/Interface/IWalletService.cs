using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Wallet;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho qu?n lý ví ti?n c?a c? TourCompany và SpecialtyShop
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// L?y thông tin ví c?a TourCompany theo UserId
        /// </summary>
        /// <param name="userId">ID c?a User có role Tour Company</param>
        /// <returns>Thông tin ví TourCompany</returns>
        Task<ApiResponse<TourCompanyWalletDto>> GetTourCompanyWalletAsync(Guid userId);

        /// <summary>
        /// L?y thông tin ví c?a SpecialtyShop theo UserId
        /// </summary>
        /// <param name="userId">ID c?a User có role Specialty Shop</param>
        /// <returns>Thông tin ví SpecialtyShop</returns>
        Task<ApiResponse<SpecialtyShopWalletDto>> GetSpecialtyShopWalletAsync(Guid userId);

        /// <summary>
        /// L?y thông tin ví theo role c?a user hi?n t?i
        /// T? ??ng detect role và tr? v? ví t??ng ?ng
        /// </summary>
        /// <param name="userId">ID c?a User</param>
        /// <returns>Thông tin ví d?ng t?ng quát</returns>
        Task<ApiResponse<WalletInfoDto>> GetWalletByUserRoleAsync(Guid userId);

        /// <summary>
        /// Ki?m tra user có ví không (có role TourCompany ho?c SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID c?a User</param>
        /// <returns>True n?u user có ví</returns>
        Task<bool> HasWalletAsync(Guid userId);

        /// <summary>
        /// L?y lo?i role có ví c?a user
        /// </summary>
        /// <param name="userId">ID c?a User</param>
        /// <returns>Role name ho?c null n?u không có ví</returns>
        Task<string?> GetUserWalletTypeAsync(Guid userId);
    }
}