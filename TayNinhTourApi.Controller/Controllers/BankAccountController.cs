using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.BankAccount;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho quản lý tài khoản ngân hàng của user
    /// Cung cấp APIs cho user quản lý bank accounts để rút tiền
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly ILogger<BankAccountController> _logger;

        public BankAccountController(IBankAccountService bankAccountService, ILogger<BankAccountController> logger)
        {
            _bankAccountService = bankAccountService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tài khoản ngân hàng của user hiện tại
        /// </summary>
        /// <returns>Danh sách tài khoản ngân hàng</returns>
        [HttpGet("my-accounts")]
        public async Task<IActionResult> GetMyBankAccounts()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Getting bank accounts for user: {UserId}", userId);

                var result = await _bankAccountService.GetByUserIdAsync(userId);
                
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bank accounts");
                return StatusCode(500, "Lỗi server khi lấy danh sách tài khoản ngân hàng");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một tài khoản ngân hàng
        /// </summary>
        /// <param name="id">ID của tài khoản ngân hàng</param>
        /// <returns>Thông tin chi tiết tài khoản ngân hàng</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBankAccountById(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Getting bank account {BankAccountId} for user: {UserId}", id, userId);

                var result = await _bankAccountService.GetByIdAsync(id, userId);
                
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else if (result.StatusCode == 404)
                {
                    return NotFound(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bank account {BankAccountId}", id);
                return StatusCode(500, "Lỗi server khi lấy thông tin tài khoản ngân hàng");
            }
        }

        /// <summary>
        /// Lấy tài khoản ngân hàng mặc định của user
        /// </summary>
        /// <returns>Tài khoản ngân hàng mặc định</returns>
        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultBankAccount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Getting default bank account for user: {UserId}", userId);

                var result = await _bankAccountService.GetDefaultByUserIdAsync(userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default bank account for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi lấy tài khoản ngân hàng mặc định");
            }
        }

        /// <summary>
        /// Tạo tài khoản ngân hàng mới
        /// </summary>
        /// <param name="createDto">Thông tin tài khoản ngân hàng</param>
        /// <returns>Thông tin tài khoản ngân hàng vừa tạo</returns>
        [HttpPost]
        public async Task<IActionResult> CreateBankAccount([FromBody] CreateBankAccountDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Creating bank account for user: {UserId}, Bank: {BankName}", userId, createDto.BankName);

                var result = await _bankAccountService.CreateAsync(createDto, userId);
                
                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetBankAccountById), new { id = result.Data.Id }, result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bank account for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi tạo tài khoản ngân hàng");
            }
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản ngân hàng
        /// </summary>
        /// <param name="id">ID của tài khoản ngân hàng</param>
        /// <param name="updateDto">Thông tin cập nhật</param>
        /// <returns>Thông tin tài khoản ngân hàng sau khi cập nhật</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBankAccount(Guid id, [FromBody] UpdateBankAccountDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Updating bank account {BankAccountId} for user: {UserId}", id, userId);

                var result = await _bankAccountService.UpdateAsync(id, updateDto, userId);
                
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else if (result.StatusCode == 404)
                {
                    return NotFound(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bank account {BankAccountId} for user: {UserId}", id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi cập nhật tài khoản ngân hàng");
            }
        }

        /// <summary>
        /// Xóa tài khoản ngân hàng
        /// </summary>
        /// <param name="id">ID của tài khoản ngân hàng</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBankAccount(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Deleting bank account {BankAccountId} for user: {UserId}", id, userId);

                var result = await _bankAccountService.DeleteAsync(id, userId);
                
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else if (result.StatusCode == 404)
                {
                    return NotFound(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bank account {BankAccountId} for user: {UserId}", id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi xóa tài khoản ngân hàng");
            }
        }

        /// <summary>
        /// Đặt tài khoản ngân hàng làm mặc định
        /// </summary>
        /// <param name="id">ID của tài khoản ngân hàng</param>
        /// <returns>Kết quả đặt mặc định</returns>
        [HttpPut("{id}/set-default")]
        public async Task<IActionResult> SetDefaultBankAccount(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Setting bank account {BankAccountId} as default for user: {UserId}", id, userId);

                var result = await _bankAccountService.SetDefaultAsync(id, userId);
                
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else if (result.StatusCode == 404)
                {
                    return NotFound(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default bank account {BankAccountId} for user: {UserId}", id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi đặt tài khoản ngân hàng mặc định");
            }
        }

        /// <summary>
        /// Kiểm tra user có tài khoản ngân hàng nào không
        /// </summary>
        /// <returns>True nếu user có ít nhất 1 tài khoản ngân hàng</returns>
        [HttpGet("has-account")]
        public async Task<IActionResult> HasBankAccount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                var hasAccount = await _bankAccountService.HasBankAccountAsync(userId);
                
                return Ok(new { hasAccount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking bank account existence for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi kiểm tra tài khoản ngân hàng");
            }
        }

        /// <summary>
        /// Validate thông tin tài khoản ngân hàng
        /// </summary>
        /// <param name="bankName">Tên ngân hàng</param>
        /// <param name="accountNumber">Số tài khoản</param>
        /// <returns>Kết quả validation</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateBankAccount([FromBody] ValidateBankAccountRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                var result = await _bankAccountService.ValidateBankAccountAsync(
                    request.BankName, 
                    request.AccountNumber, 
                    userId, 
                    request.ExcludeBankAccountId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating bank account for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi validate tài khoản ngân hàng");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Lấy User ID từ JWT token
        /// </summary>
        /// <returns>User ID hoặc Guid.Empty nếu không tìm thấy</returns>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }

    /// <summary>
    /// Request DTO cho validate bank account
    /// </summary>
    public class ValidateBankAccountRequest
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public Guid? ExcludeBankAccountId { get; set; }
    }
}
