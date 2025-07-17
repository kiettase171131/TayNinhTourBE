using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Wallet;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho qu?n l� v� ti?n c?a Tour Company v� Specialty Shop
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(
            IWalletService walletService,
            ICurrentUserService currentUserService,
            ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// L?y th�ng tin v� c?a user hi?n t?i
        /// T? ??ng detect role (Tour Company ho?c Specialty Shop) v� tr? v? v� t??ng ?ng
        /// </summary>
        /// <returns>Th�ng tin v� theo role c?a user</returns>
        [HttpGet("my-wallet")]
        public async Task<IActionResult> GetMyWallet()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("User ID not found in authentication context");
                    return BadRequest("Kh�ng t�m th?y th�ng tin user trong token");
                }

                _logger.LogInformation("Getting wallet information for user: {UserId}", userId);

                var result = await _walletService.GetWalletByUserRoleAsync(userId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet information");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "L?i h? th?ng khi l?y th�ng tin v�",
                    Success = false
                });
            }
        }

        /// <summary>
        /// L?y th�ng tin v� Tour Company c?a user hi?n t?i
        /// Ch? user c� role "Tour Company" m?i c� th? g?i
        /// </summary>
        /// <returns>Th�ng tin v� Tour Company chi ti?t</returns>
        [HttpGet("tour-company")]
        [Authorize(Roles = "Tour Company")]
        public async Task<IActionResult> GetTourCompanyWallet()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("Tour Company user ID not found in authentication context");
                    return BadRequest("Kh�ng t�m th?y th�ng tin user trong token");
                }

                _logger.LogInformation("Getting Tour Company wallet for user: {UserId}", userId);

                var result = await _walletService.GetTourCompanyWalletAsync(userId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Tour Company wallet information");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "L?i h? th?ng khi l?y th�ng tin v� c�ng ty tour",
                    Success = false
                });
            }
        }

        /// <summary>
        /// L?y th�ng tin v� Specialty Shop c?a user hi?n t?i
        /// Ch? user c� role "Specialty Shop" m?i c� th? g?i
        /// </summary>
        /// <returns>Th�ng tin v� Specialty Shop</returns>
        [HttpGet("specialty-shop")]
        [Authorize(Roles = "Specialty Shop")]
        public async Task<IActionResult> GetSpecialtyShopWallet()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("Specialty Shop user ID not found in authentication context");
                    return BadRequest("Kh�ng t�m th?y th�ng tin user trong token");
                }

                _logger.LogInformation("Getting Specialty Shop wallet for user: {UserId}", userId);

                var result = await _walletService.GetSpecialtyShopWalletAsync(userId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Specialty Shop wallet information");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "L?i h? th?ng khi l?y th�ng tin v� shop",
                    Success = false
                });
            }
        }

        /// <summary>
        /// Ki?m tra user hi?n t?i c� v� ti?n kh�ng
        /// </summary>
        /// <returns>True n?u user c� v�, False n?u kh�ng c�</returns>
        [HttpGet("has-wallet")]
        public async Task<IActionResult> HasWallet()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return BadRequest("Kh�ng t�m th?y th�ng tin user trong token");
                }

                var hasWallet = await _walletService.HasWalletAsync(userId);
                var walletType = hasWallet ? await _walletService.GetUserWalletTypeAsync(userId) : null;

                return Ok(new
                {
                    StatusCode = 200,
                    Message = hasWallet ? "User c� v� ti?n" : "User ch?a c� v� ti?n",
                    Success = true,
                    Data = new
                    {
                        HasWallet = hasWallet,
                        WalletType = walletType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking wallet status");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "L?i h? th?ng khi ki?m tra tr?ng th�i v�",
                    Success = false
                });
            }
        }

        /// <summary>
        /// L?y lo?i v� c?a user hi?n t?i
        /// </summary>
        /// <returns>Lo?i v� (TourCompany ho?c SpecialtyShop) ho?c null n?u kh�ng c�</returns>
        [HttpGet("wallet-type")]
        public async Task<IActionResult> GetWalletType()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return BadRequest("Kh�ng t�m th?y th�ng tin user trong token");
                }

                var walletType = await _walletService.GetUserWalletTypeAsync(userId);

                return Ok(new
                {
                    StatusCode = 200,
                    Message = walletType != null ? "L?y lo?i v� th�nh c�ng" : "User ch?a c� v�",
                    Success = true,
                    Data = new
                    {
                        WalletType = walletType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet type");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "L?i h? th?ng khi l?y lo?i v�",
                    Success = false
                });
            }
        }
    }
}