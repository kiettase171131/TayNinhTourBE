using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductController(IProductService productService, IHttpContextAccessor httpContextAccessor)
        {
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
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
    }
}
