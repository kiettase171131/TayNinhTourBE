using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.BankAccount;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.BankAccount;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý tài khoản ngân hàng của user
    /// Cung cấp các methods để CRUD bank accounts và quản lý default account
    /// </summary>
    public interface IBankAccountService
    {
        /// <summary>
        /// Lấy danh sách ngân hàng hỗ trợ
        /// </summary>
        /// <returns>Danh sách ngân hàng hỗ trợ</returns>
        Task<ApiResponse<List<SupportedBankDto>>> GetSupportedBanksAsync();

        /// <summary>
        /// Lấy danh sách tài khoản ngân hàng của user hiện tại
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách tài khoản ngân hàng</returns>
        Task<ApiResponse<List<BankAccountResponseDto>>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy thông tin chi tiết một tài khoản ngân hàng
        /// Chỉ owner hoặc admin mới có thể xem
        /// </summary>
        /// <param name="bankAccountId">ID của tài khoản ngân hàng</param>
        /// <param name="currentUserId">ID của user hiện tại</param>
        /// <returns>Thông tin chi tiết tài khoản ngân hàng</returns>
        Task<ApiResponse<BankAccountResponseDto>> GetByIdAsync(Guid bankAccountId, Guid currentUserId);

        /// <summary>
        /// Lấy tài khoản ngân hàng mặc định của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Tài khoản ngân hàng mặc định hoặc null nếu chưa có</returns>
        Task<ApiResponse<BankAccountResponseDto?>> GetDefaultByUserIdAsync(Guid userId);

        /// <summary>
        /// Tạo tài khoản ngân hàng mới
        /// </summary>
        /// <param name="createDto">Thông tin tài khoản ngân hàng</param>
        /// <param name="userId">ID của user tạo</param>
        /// <returns>Thông tin tài khoản ngân hàng vừa tạo</returns>
        Task<ApiResponse<BankAccountResponseDto>> CreateAsync(CreateBankAccountDto createDto, Guid userId);

        /// <summary>
        /// Cập nhật thông tin tài khoản ngân hàng
        /// Chỉ owner mới có thể cập nhật
        /// </summary>
        /// <param name="bankAccountId">ID của tài khoản ngân hàng</param>
        /// <param name="updateDto">Thông tin cập nhật</param>
        /// <param name="currentUserId">ID của user hiện tại</param>
        /// <returns>Thông tin tài khoản ngân hàng sau khi cập nhật</returns>
        Task<ApiResponse<BankAccountResponseDto>> UpdateAsync(Guid bankAccountId, UpdateBankAccountDto updateDto, Guid currentUserId);

        /// <summary>
        /// Xóa tài khoản ngân hàng (soft delete)
        /// Chỉ owner mới có thể xóa và không có withdrawal request nào đang pending
        /// </summary>
        /// <param name="bankAccountId">ID của tài khoản ngân hàng</param>
        /// <param name="currentUserId">ID của user hiện tại</param>
        /// <returns>Kết quả xóa</returns>
        Task<ApiResponse<bool>> DeleteAsync(Guid bankAccountId, Guid currentUserId);

        /// <summary>
        /// Đặt tài khoản ngân hàng làm mặc định
        /// Tự động unset tài khoản mặc định cũ (nếu có)
        /// </summary>
        /// <param name="bankAccountId">ID của tài khoản ngân hàng</param>
        /// <param name="currentUserId">ID của user hiện tại</param>
        /// <returns>Kết quả đặt mặc định</returns>
        Task<ApiResponse<bool>> SetDefaultAsync(Guid bankAccountId, Guid currentUserId);

        /// <summary>
        /// Kiểm tra user có tài khoản ngân hàng nào không
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu user có ít nhất 1 tài khoản ngân hàng</returns>
        Task<bool> HasBankAccountAsync(Guid userId);

        /// <summary>
        /// Validate thông tin tài khoản ngân hàng
        /// Kiểm tra duplicate, format số tài khoản, etc.
        /// </summary>
        /// <param name="bankName">Tên ngân hàng</param>
        /// <param name="accountNumber">Số tài khoản</param>
        /// <param name="userId">ID của user</param>
        /// <param name="excludeBankAccountId">ID tài khoản cần loại trừ (cho update)</param>
        /// <returns>Kết quả validation</returns>
        Task<ApiResponse<bool>> ValidateBankAccountAsync(string bankName, string accountNumber, Guid userId, Guid? excludeBankAccountId = null);
    }
}
