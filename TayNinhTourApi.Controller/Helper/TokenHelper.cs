using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.Controller.Helper
{
    public class TokenHelper
    {
        private static TokenHelper instance;
        public static TokenHelper Instance
        {
            get { if (instance == null) instance = new TokenHelper(); return TokenHelper.instance; }
            private set { TokenHelper.instance = value; }
        }
        public async Task<CurrentUserObject> GetThisUserInfo(HttpContext httpContext)
        {
            CurrentUserObject currentUser = new();

            var checkUser = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            if (checkUser != null)
            {
                var accountIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
                if (Guid.TryParse(accountIdClaim, out var userid))
                {
                    currentUser.Id = userid;
                    currentUser.UserId = userid; // Set cả UserId để tương thích
                }
                var roleClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (Guid.TryParse(roleClaim, out var roldid))
                {
                    currentUser.RoleId = roldid;
                }
                else
                {
                    currentUser.RoleId = Guid.Empty;
                }
                currentUser.Email = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
                currentUser.Name = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty;
                currentUser.PhoneNumber = httpContext.User.Claims.FirstOrDefault(c => c.Type == "Phone")?.Value ?? string.Empty;
                return currentUser;
            }
            else
            {
                return null;
            }
        }
    }
}
