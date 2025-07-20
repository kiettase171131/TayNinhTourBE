        using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ForgotPasswordDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Authentication;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService, IMemoryCache memoryCache)
        {
            _authenticationService = authenticationService;
            _memoryCache = memoryCache;
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaseResposeDto>> Register(RequestRegisterDto request)
        {
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

            // Assign the client Ip
            request.ClientIp = clientIp;

            var response = await _authenticationService.RegisterAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseAuthenticationDto>> Login(RequestLoginDto request)
        {
            var response = await _authenticationService.LoginAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<ResponseVerifyOtpDto>> VerifyOtp(RegisterVerifyOtpRequestDto request)
        {
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

            // Assign the client Ip
            request.ClientIp = clientIp;

            var response = await _authenticationService.VerifyOtpAsync(request);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPost("send-otp-reset-password")]
        public async Task<ActionResult<BaseResposeDto>> SendOTPResetPassword(SendOtpDTO request)
        {
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            // Assign the client Ip
            request.ClientIp = clientIp;
            var response = await _authenticationService.SendOTPResetPasswordAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ResponseVerifyOtpDto>> ResetPassword(ResetPasswordDTO request)
        {
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            // Assign the client Ip
            request.ClientIp = clientIp;
            var response = await _authenticationService.ResetPassword(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ResponseAuthenticationDto>> RefreshToken([FromBody] RequestRefreshTokenDto request)
        {
            var response = await _authenticationService.RefreshTokenAsync(request);

            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("login-google")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequestDto request)
        {
            var result = await _authenticationService.LoginWithGoogleAsync(request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
