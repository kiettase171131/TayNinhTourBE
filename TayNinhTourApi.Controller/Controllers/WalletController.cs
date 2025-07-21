using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Wallet;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho qu?n lý ví ti?n c?a Tour Company và Specialty Shop
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
        /// L?y thông tin ví c?a user hi?n t?i
        /// T? ??ng detect role (Tour Company ho?c Specialty Shop) và tr? v? ví t??ng ?ng
        /// </summary>
        /// <returns>Thông tin ví theo role c?a user</returns>
        [HttpGet("my-wallet")]
        public async Task<IActionResult> GetMyWallet()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("User ID not found in authentication context");
                    return BadRequest("Không tìm th?y thông tin user trong token");
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
                    Message = "L?i h? th?ng khi l?y thông tin ví",
                    Success = false
                });
            }
        }

        /// <summary>
        /// L?y thông tin ví Tour Company c?a user hi?n t?i
        /// Ch? user có role "Tour Company" m?i có th? g?i
        /// </summary>
        /// <returns>Thông tin ví Tour Company chi ti?t</returns>
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
                    return BadRequest("Không tìm th?y thông tin user trong token");
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
                    Message = "L?i h? th?ng khi l?y thông tin ví công ty tour",
                    Success = false
                });
            }
        }

        /// <summary>
        /// L?y thông tin ví Specialty Shop c?a user hi?n t?i
        /// Ch? user có role "Specialty Shop" m?i có th? g?i
        /// </summary>
        /// <returns>Thông tin ví Specialty Shop</returns>
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
                    return BadRequest("Không tìm th?y thông tin user trong token");
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
                    Message = "L?i h? th?ng khi l?y thông tin ví shop",
                    Success = false
                });
            }
        }

        /// <summary>
        /// Ki?m tra user hi?n t?i có ví ti?n không
        /// </summary>
        /// <returns>True n?u user có ví, False n?u không có</returns>
        [HttpGet("has-wallet")]
        public async Task<IActionResult> HasWallet()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return BadRequest("Không tìm th?y thông tin user trong token");
                }

                var hasWallet = await _walletService.HasWalletAsync(userId);
                var walletType = hasWallet ? await _walletService.GetUserWalletTypeAsync(userId) : null;

                return Ok(new
                {
                    StatusCode = 200,
                    Message = hasWallet ? "User có ví ti?n" : "User ch?a có ví ti?n",
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
                    Message = "L?i h? th?ng khi ki?m tra tr?ng thái ví",
                    Success = false
                });
            }
        }

        /// <summary>
        /// L?y lo?i ví c?a user hi?n t?i
        /// </summary>
        /// <returns>Lo?i ví (TourCompany ho?c SpecialtyShop) ho?c null n?u không có</returns>
        [HttpGet("wallet-type")]
        public async Task<IActionResult> GetWalletType()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return BadRequest("Không tìm th?y thông tin user trong token");
                }

                var walletType = await _walletService.GetUserWalletTypeAsync(userId);

                return Ok(new
                {
                    StatusCode = 200,
                    Message = walletType != null ? "L?y lo?i ví thành công" : "User ch?a có ví",
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
                    Message = "L?i h? th?ng khi l?y lo?i ví",
                    Success = false
                });
            }
        }
    }
}