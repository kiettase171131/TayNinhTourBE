using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ForgotPasswordDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Authentication;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IAuthenticationService
    {
        Task<BaseResposeDto> RegisterAsync(RequestRegisterDto request);
        Task<ResponseAuthenticationDto> LoginAsync(RequestLoginDto request);
        Task<ResponseVerifyOtpDto> VerifyOtpAsync(RegisterVerifyOtpRequestDto request);

        Task<BaseResposeDto> SendOTPResetPasswordAsync(SendOtpDTO request);
        Task<ResponseVerifyOtpDto> ResetPassword(ResetPasswordDTO request);


        Task<ResponseAuthenticationDto> RefreshTokenAsync(RequestRefreshTokenDto request);
        Task<ResponseAuthenticationDto> LoginWithGoogleAsync(GoogleLoginRequestDto request);    

    }
}
