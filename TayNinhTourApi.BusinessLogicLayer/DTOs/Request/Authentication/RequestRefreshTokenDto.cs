namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication
{
    public class RequestRefreshTokenDto
    {
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; } = null!;
    }
}
