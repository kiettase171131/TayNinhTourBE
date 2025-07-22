using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho BankAccount entity
    /// Cung cấp các methods để truy cập và thao tác với dữ liệu BankAccount
    /// </summary>
    public interface IBankAccountRepository : IGenericRepository<BankAccount>
    {
        /// <summary>
        /// Lấy danh sách tài khoản ngân hàng của một user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="includeInactive">Có bao gồm tài khoản không hoạt động không</param>
        /// <returns>Danh sách tài khoản ngân hàng</returns>
        Task<IEnumerable<BankAccount>> GetByUserIdAsync(Guid userId, bool includeInactive = false);

        /// <summary>
        /// Lấy tài khoản ngân hàng mặc định của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Tài khoản ngân hàng mặc định hoặc null nếu chưa có</returns>
        Task<BankAccount?> GetDefaultByUserIdAsync(Guid userId);

        /// <summary>
        /// Kiểm tra user có tài khoản ngân hàng nào không
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu user có ít nhất 1 tài khoản ngân hàng</returns>
        Task<bool> HasBankAccountAsync(Guid userId);

        /// <summary>
        /// Kiểm tra tài khoản ngân hàng đã tồn tại chưa (duplicate check)
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="bankName">Tên ngân hàng</param>
        /// <param name="accountNumber">Số tài khoản</param>
        /// <param name="excludeId">ID tài khoản cần loại trừ (cho update)</param>
        /// <returns>True nếu đã tồn tại</returns>
        Task<bool> ExistsAsync(Guid userId, string bankName, string accountNumber, Guid? excludeId = null);

        /// <summary>
        /// Lấy tài khoản ngân hàng theo ID với kiểm tra ownership
        /// </summary>
        /// <param name="bankAccountId">ID của tài khoản ngân hàng</param>
        /// <param name="userId">ID của user (để kiểm tra ownership)</param>
        /// <returns>Tài khoản ngân hàng nếu user là owner, null nếu không</returns>
        Task<BankAccount?> GetByIdAndUserIdAsync(Guid bankAccountId, Guid userId);

        /// <summary>
        /// Đếm số lượng tài khoản ngân hàng của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="includeInactive">Có bao gồm tài khoản không hoạt động không</param>
        /// <returns>Số lượng tài khoản ngân hàng</returns>
        Task<int> CountByUserIdAsync(Guid userId, bool includeInactive = false);

        /// <summary>
        /// Unset tất cả tài khoản mặc định của user (để set tài khoản mới làm default)
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Task</returns>
        Task UnsetAllDefaultAsync(Guid userId);

        /// <summary>
        /// Kiểm tra tài khoản có thể xóa không (không có withdrawal request pending)
        /// </summary>
        /// <param name="bankAccountId">ID của tài khoản ngân hàng</param>
        /// <returns>True nếu có thể xóa</returns>
        Task<bool> CanDeleteAsync(Guid bankAccountId);

        /// <summary>
        /// Lấy tài khoản ngân hàng với thông tin withdrawal request count
        /// </summary>
        /// <param name="bankAccountId">ID của tài khoản ngân hàng</param>
        /// <returns>Tài khoản ngân hàng với navigation properties</returns>
        Task<BankAccount?> GetWithWithdrawalRequestsAsync(Guid bankAccountId);

        /// <summary>
        /// Lấy danh sách tài khoản ngân hàng với thông tin admin verification
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách tài khoản ngân hàng với thông tin verifier</returns>
        Task<IEnumerable<BankAccount>> GetWithVerificationInfoAsync(Guid userId);
    }
}
