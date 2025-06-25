using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourDetailsSpecialtyShop entity
    /// </summary>
    public interface ITourDetailsSpecialtyShopRepository : IGenericRepository<TourDetailsSpecialtyShop>
    {
        /// <summary>
        /// Lấy danh sách invitations theo TourDetails ID
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Danh sách invitations</returns>
        Task<IEnumerable<TourDetailsSpecialtyShop>> GetByTourDetailsIdAsync(Guid tourDetailsId);

        /// <summary>
        /// Lấy danh sách invitations theo SpecialtyShop ID
        /// </summary>
        /// <param name="specialtyShopId">ID của SpecialtyShop</param>
        /// <returns>Danh sách invitations</returns>
        Task<IEnumerable<TourDetailsSpecialtyShop>> GetBySpecialtyShopIdAsync(Guid specialtyShopId);

        /// <summary>
        /// Lấy invitation cụ thể theo TourDetails và SpecialtyShop
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="specialtyShopId">ID của SpecialtyShop</param>
        /// <returns>Invitation nếu tồn tại</returns>
        Task<TourDetailsSpecialtyShop?> GetByTourDetailsAndShopAsync(Guid tourDetailsId, Guid specialtyShopId);

        /// <summary>
        /// Lấy danh sách invitations theo status
        /// </summary>
        /// <param name="status">Status của invitation</param>
        /// <returns>Danh sách invitations</returns>
        Task<IEnumerable<TourDetailsSpecialtyShop>> GetByStatusAsync(ShopInvitationStatus status);

        /// <summary>
        /// Lấy danh sách invitations đã hết hạn
        /// </summary>
        /// <returns>Danh sách invitations hết hạn</returns>
        Task<IEnumerable<TourDetailsSpecialtyShop>> GetExpiredInvitationsAsync();

        /// <summary>
        /// Cập nhật status của invitation
        /// </summary>
        /// <param name="id">ID của invitation</param>
        /// <param name="status">Status mới</param>
        /// <param name="responseNote">Ghi chú phản hồi</param>
        /// <param name="updatedById">ID của user cập nhật</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateStatusAsync(Guid id, ShopInvitationStatus status, string? responseNote, Guid updatedById);

        /// <summary>
        /// Xóa tất cả invitations của một TourDetails
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Số lượng invitations đã xóa</returns>
        Task<int> DeleteByTourDetailsIdAsync(Guid tourDetailsId);
    }
}
