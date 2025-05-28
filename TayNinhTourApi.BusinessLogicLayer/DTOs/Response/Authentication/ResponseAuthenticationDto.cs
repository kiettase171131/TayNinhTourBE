namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Authentication
{
    public class ResponseAuthenticationDto : BaseResposeDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenExpirationTime { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Avatar { get; set; } = null!;
    }
}
