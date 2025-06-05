using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation để quản lý thông tin user hiện tại trong request context
    /// Sử dụng HttpContext và JWT claims để lấy thông tin user
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy ID của user hiện tại từ JWT token
        /// </summary>
        public Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Fallback to custom claim name used in JWT
                userIdClaim = _httpContextAccessor.HttpContext?.User
                    .FindFirst("Id")?.Value;
            }

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Lấy thông tin đầy đủ của user hiện tại
        /// </summary>
        public async Task<CurrentUserObject?> GetCurrentUserAsync()
        {
            if (!IsAuthenticated())
                return null;

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return null;

            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                    return null;

                return new CurrentUserObject
                {
                    Id = user.Id,
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    PhoneNumber = user.PhoneNumber
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra user hiện tại có được authenticated không
        /// </summary>
        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }

        /// <summary>
        /// Lấy Role ID của user hiện tại
        /// </summary>
        public Guid GetCurrentUserRoleId()
        {
            var roleIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("RoleId")?.Value;

            return Guid.TryParse(roleIdClaim, out var roleId) ? roleId : Guid.Empty;
        }

        /// <summary>
        /// Lấy email của user hiện tại
        /// </summary>
        public string? GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Lấy tên của user hiện tại
        /// </summary>
        public string? GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
