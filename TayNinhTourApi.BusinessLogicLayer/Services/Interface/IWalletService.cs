using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Wallet;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho qu?n l� v� ti?n c?a c? TourCompany v� SpecialtyShop
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// L?y th�ng tin v� c?a TourCompany theo UserId
        /// </summary>
        /// <param name="userId">ID c?a User c� role Tour Company</param>
        /// <returns>Th�ng tin v� TourCompany</returns>
        Task<ApiResponse<TourCompanyWalletDto>> GetTourCompanyWalletAsync(Guid userId);

        /// <summary>
        /// L?y th�ng tin v� c?a SpecialtyShop theo UserId
        /// </summary>
        /// <param name="userId">ID c?a User c� role Specialty Shop</param>
        /// <returns>Th�ng tin v� SpecialtyShop</returns>
        Task<ApiResponse<SpecialtyShopWalletDto>> GetSpecialtyShopWalletAsync(Guid userId);

        /// <summary>
        /// L?y th�ng tin v� theo role c?a user hi?n t?i
        /// T? ??ng detect role v� tr? v? v� t??ng ?ng
        /// </summary>
        /// <param name="userId">ID c?a User</param>
        /// <returns>Th�ng tin v� d?ng t?ng qu�t</returns>
        Task<ApiResponse<WalletInfoDto>> GetWalletByUserRoleAsync(Guid userId);

        /// <summary>
        /// Ki?m tra user c� v� kh�ng (c� role TourCompany ho?c SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID c?a User</param>
        /// <returns>True n?u user c� v�</returns>
        Task<bool> HasWalletAsync(Guid userId);

        /// <summary>
        /// L?y lo?i role c� v� c?a user
        /// </summary>
        /// <param name="userId">ID c?a User</param>
        /// <returns>Role name ho?c null n?u kh�ng c� v�</returns>
        Task<string?> GetUserWalletTypeAsync(Guid userId);

        /// <summary>
        /// Xử lý rút tiền từ ví (hỗ trợ cả TourCompany và SpecialtyShop)
        /// Method này được gọi khi admin approve withdrawal request
        /// </summary>
        /// <param name="userId">ID của user sở hữu wallet</param>
        /// <param name="amount">Số tiền cần trừ</param>
        /// <param name="withdrawalRequestId">ID của withdrawal request (để audit)</param>
        /// <returns>Kết quả xử lý rút tiền</returns>
        Task<ApiResponse<bool>> ProcessWithdrawalAsync(Guid userId, decimal amount, Guid withdrawalRequestId);

        /// <summary>
        /// Hoàn tiền vào ví (hỗ trợ cả TourCompany và SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID của user sở hữu wallet</param>
        /// <param name="amount">Số tiền cần hoàn</param>
        /// <param name="reason">Lý do hoàn tiền</param>
        /// <returns>Kết quả hoàn tiền</returns>
        Task<ApiResponse<bool>> RefundToWalletAsync(Guid userId, decimal amount, string reason);

        /// <summary>
        /// Kiểm tra số dư ví có đủ để rút không (hỗ trợ cả TourCompany và SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="amount">Số tiền muốn rút</param>
        /// <returns>True nếu đủ số dư</returns>
        Task<ApiResponse<bool>> CheckSufficientBalanceAsync(Guid userId, decimal amount);

        /// <summary>
        /// Lấy số dư ví hiện tại (hỗ trợ cả TourCompany và SpecialtyShop)
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Số dư ví hiện tại</returns>
        Task<ApiResponse<decimal>> GetCurrentWalletBalanceAsync(Guid userId);
    }
}