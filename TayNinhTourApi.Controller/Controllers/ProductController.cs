using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Voucher;
using TayNinhTourApi.BusinessLogicLayer.Services;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPayOsService _payOsService;

        public ProductController(IProductService productService, IHttpContextAccessor httpContextAccessor, IPayOsService payOsService)
        {
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
            _payOsService = payOsService;
        }
        [HttpPost("Product")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Specialty Shop")]
        public async Task<IActionResult> Create([FromForm] RequestCreateProductDto dto)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.CreateProductAsync(dto, currentUser);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("Product")]
        public async Task<IActionResult> GetAll(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var result = await _productService.GetProductsAsync(pageIndex, pageSize, textSearch, status);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("Product-ByShop")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Specialty Shop")]
        public async Task<IActionResult> GetAllByShop(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.GetProductsByShopAsync(pageIndex, pageSize, textSearch, status, currentUser);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("Product/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("Product/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Specialty Shop")]
        public async Task<IActionResult> Update(Guid id, [FromForm] RequestUpdateProductDto dto)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.UpdateProductAsync(dto, id, currentUser);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("Product/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Specialty Shop")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("AddtoCart")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddToCart([FromBody] RequestAddMultipleToCartDto dto)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.AddToCartAsync(dto, currentUser);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách giỏ hàng của người dùng hiện tại
        /// </summary>
        [HttpGet("Cart")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCart()
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.GetCartAsync(currentUser);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Xoá 1 item khỏi giỏ hàng
        /// </summary>
        [HttpDelete("RemoveCart")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RemoveCartItem()
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.RemoveFromCartAsync(currentUser);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("reviews-ratings")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> FeedbackProduct([FromBody] CreateProductFeedbackDto dto)
        {
            // Lấy userId từ claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            Guid userId = Guid.Parse(userIdClaim.Value);

            var result = await _productService.FeedbackProductAsync(dto, userId);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{productId}/reviews-ratings")]
        public async Task<IActionResult> GetProductReviewSummary(Guid productId)
        {
            var result = await _productService.GetProductReviewSummaryAsync(productId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("orders/{orderId}/payment-status")]
        public async Task<IActionResult> GetPaymentStatus(Guid orderId)
        {
            try
            {
                var status = await _productService.GetOrderPaymentStatusAsync(orderId);
                return Ok(new { status = status.ToString() });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("checkout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Checkout([FromBody] CheckoutSelectedCartItemsDto dto)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new 
                    { 
                        message = "Dữ liệu không hợp lệ", 
                        errors = errors 
                    });
                }

                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                
                // Gọi service chỉ với myVoucherCodeId (bỏ voucherCode)
                var result = await _productService.CheckoutCartAsync(
                    dto.CartItemIds, 
                    currentUser, 
                    dto.MyVoucherCodeId);

                if (result == null)
                    return BadRequest("Sản phẩm chọn không hợp lệ hoặc không đủ tồn kho.");

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        [HttpPost("test-checkout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> TestCheckout([FromBody] object rawRequest)
        {
            try
            {
                // Debug raw request
                Console.WriteLine($"Raw request received: {System.Text.Json.JsonSerializer.Serialize(rawRequest)}");

                // Kiểm tra ModelState
                Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    var allErrors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => new { 
                            Field = x.Exception?.Data?.Keys?.Cast<object>()?.FirstOrDefault()?.ToString() ?? "Unknown",
                            Message = x.ErrorMessage 
                        })
                        .ToList();
                    
                    Console.WriteLine($"ModelState errors: {System.Text.Json.JsonSerializer.Serialize(allErrors)}");
                }

                return Ok(new 
                {
                    message = "Test endpoint - Debug info",
                    modelStateIsValid = ModelState.IsValid,
                    rawRequest = rawRequest,
                    errors = ModelState.IsValid ? null : ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Test error", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        [HttpGet("GetAll-Voucher")]


        public async Task<IActionResult> GetAllVoucher([FromQuery] int? pageIndex, [FromQuery] int? pageSize, [FromQuery] string? textSearch, [FromQuery] bool? status)
        {
            var result = await _productService.GetAllVouchersAsync(pageIndex, pageSize, textSearch, status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetVoucher/{id}")]
        public async Task<IActionResult> GetVoucherById(Guid id)
        {
            var result = await _productService.GetVoucherByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("Create-Voucher")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto dto)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.CreateAsync(dto, currentUser.Id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("Update-Voucher/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> UpdateVoucher(Guid id, [FromBody] UpdateVoucherDto dto)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.UpdateVoucherAsync(id, dto, currentUser.Id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("Voucher/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> DeleteVoucher(Guid id)
        {
            var result = await _productService.DeleteVoucherAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetAvailable-VoucherCodes")]
        public async Task<IActionResult> GetAvailableVoucherCodes([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            var result = await _productService.GetAvailableVoucherCodesAsync(pageIndex, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("Claim-VoucherCode/{voucherCodeId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ClaimVoucherCode(Guid voucherCodeId)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.ClaimVoucherCodeAsync(voucherCodeId, currentUser);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("My-Vouchers")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetMyVouchers([FromQuery] int? pageIndex, [FromQuery] int? pageSize, [FromQuery] string? status, [FromQuery] string? textSearch)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var result = await _productService.GetMyVouchersAsync(currentUser, pageIndex, pageSize, status, textSearch);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"GetMyVouchers error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách voucher",
                    Error = ex.Message,
                    success = false
                });
            }
        }
        [HttpGet("AllOrder")]
        public async Task<IActionResult> GetAllOrder(int? pageIndex, int? pageSize, string? payOsOrderCode, bool? status, bool? isChecked)
        {
            try
            {
                var result = await _productService.GetAllOrdersAsync(pageIndex, pageSize, payOsOrderCode, status,isChecked);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllOrder Controller error: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new 
                { 
                    message = "Internal server error", 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message 
                });
            }
        }
        [HttpGet("GetOrder-ByUser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetOrderByUser(int? pageIndex, int? pageSize, string? payOsOrderCode, bool? status, bool? isChecked)
        {
            try
            {
                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var result = await _productService.GetOrdersByUserAsync(pageIndex, pageSize, payOsOrderCode, status,isChecked, currentUser);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetOrderByUser Controller error: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
        [HttpGet("GetOrder-ByShop")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetOrdersByShop(int? pageIndex, int? pageSize, string? payOsOrderCode, bool? status, bool? isChecked)
        {
            CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.GetOrdersByCurrentShopAsync(pageIndex, pageSize, payOsOrderCode, status,isChecked, currentUser);
            return Ok(result);
        }

        [HttpPost("simple-checkout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SimpleCheckout([FromBody] CheckoutSelectedCartItemsDto dto)
        {
            try
            {
                // Log request info
                Console.WriteLine($"SimpleCheckout called");
                Console.WriteLine($"CartItemIds count: {dto?.CartItemIds?.Count ?? 0}");
                Console.WriteLine($"MyVoucherCodeId: {dto?.MyVoucherCodeId}");

                // Validate basic requirements
                if (dto?.CartItemIds == null || !dto.CartItemIds.Any())
                {
                    return BadRequest(new { message = "CartItemIds is required and cannot be empty" });
                }

                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                
                // Call service chỉ với myVoucherCodeId
                var result = await _productService.CheckoutCartAsync(
                    dto.CartItemIds, 
                    currentUser, 
                    dto.MyVoucherCodeId);

                if (result == null)
                    return BadRequest("Sản phẩm chọn không hợp lệ hoặc không đủ tồn kho.");

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}
