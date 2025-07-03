using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product;
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
        public async Task<IActionResult> GetAll( int? pageIndex,  int? pageSize,  string? textSearch,  bool? status)
        {
            var result = await _productService.GetProductsAsync(pageIndex, pageSize, textSearch, status);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("Product-ByShop")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Specialty Shop")]
        public async Task<IActionResult> GetAllByShop(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.GetProductsByShopAsync(pageIndex, pageSize, textSearch, status,currentUser);
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
        public async Task<IActionResult> AddToCart([FromBody] RequestAddToCartDto dto)
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
        [HttpDelete("RemoveCart/{cartItemId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RemoveCartItem(Guid cartItemId)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.RemoveFromCartAsync(cartItemId, currentUser);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("rate")]
        public async Task<IActionResult> RateProduct([FromBody] CreateProductRatingDto dto)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.RateProductAsync(dto, currentUser.Id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("review")]
        public async Task<IActionResult> ReviewProduct([FromBody] CreateProductReviewDto dto)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _productService.ReviewProductAsync(dto, currentUser.Id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{productId}/average-rating")]
        public async Task<IActionResult> GetAverageRating(Guid productId)
        {
            var result = await _productService.GetAverageRatingAsync(productId);
            return Ok(new { averageRating = result });
        }

        [HttpGet("{productId}/reviews")]
        public async Task<IActionResult> GetReviews(Guid productId)
        {
            var result = await _productService.GetProductReviewsAsync(productId);
            return Ok(result);
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
                // ✅ Check ModelState validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();
                    return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
                }

                // ✅ Validation cơ bản
                if (dto == null || dto.CartItemIds == null || !dto.CartItemIds.Any())
                {
                    return BadRequest(new { message = "Danh sách sản phẩm checkout không được để trống." });
                }

                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng." });
                }

                var result = await _productService.CheckoutCartAsync(dto.CartItemIds, currentUser);

                if (result == null)
                    return BadRequest(new { message = "Sản phẩm chọn không hợp lệ hoặc không đủ tồn kho." });

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message }); // Báo thiếu tồn kho
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message }); // Lỗi tham số
            }
            catch (Exception ex)
            {
                // ✅ Log chi tiết lỗi
                Console.WriteLine($"Checkout Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "Có lỗi xảy ra trong quá trình checkout", error = ex.Message });
            }
        }
    }
}
