using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Order;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller for order management - used by specialty shops for order checking and delivery confirmation
    /// Replaces the QR code functionality with a simpler checking system
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ISpecialtyShopRepository _specialtyShopRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderService orderService, ISpecialtyShopRepository specialtyShopRepository, IOrderRepository orderRepository)
        {
            _orderService = orderService;
            _specialtyShopRepository = specialtyShopRepository;
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Check/mark an order as delivered by specialty shop using PayOsOrderCode
        /// When a specialty shop checks the order, it will verify the order and immediately mark it as delivered
        /// Only accessible by users with "Specialty Shop" role and only for orders containing their products
        /// </summary>
        /// <param name="request">Order check request with PayOsOrderCode</param>
        /// <returns>Order details and processing result</returns>
        [HttpPost("check")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.RoleSpecialtyShopName)]
        public async Task<IActionResult> CheckOrder([FromBody] CheckOrderRequestDto request)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                
                // Get the specialty shop by user ID (User -> SpecialtyShop relationship via UserId)
                var specialtyShop = await _specialtyShopRepository.GetByUserIdAsync(currentUser.Id);
                
                if (specialtyShop == null)
                {
                    return BadRequest(new { 
                        Message = "Không tìm th?y thông tin shop cho user này. Vui lòng liên h? admin.",
                        UserInfo = new { currentUser.Id, currentUser.Name, currentUser.Email },
                        Debug = "User has SpecialtyShop role but no SpecialtyShop record found"
                    });
                }

                if (!specialtyShop.IsActive)
                {
                    return BadRequest(new { 
                        Message = "Shop c?a b?n ?ã b? vô hi?u hóa. Vui lòng liên h? admin.",
                        ShopInfo = new { specialtyShop.Id, specialtyShop.ShopName, specialtyShop.IsActive }
                    });
                }

                if (!specialtyShop.IsShopActive)
                {
                    return BadRequest(new { 
                        Message = "Shop c?a b?n hi?n ?ang ?óng c?a. Vui lòng m? shop tr??c khi check ??n hàng.",
                        ShopInfo = new { specialtyShop.Id, specialtyShop.ShopName, specialtyShop.IsShopActive }
                    });
                }

                var shopId = specialtyShop.Id; // Use the SpecialtyShop ID, not User ID
                
                var result = await _orderService.CheckOrderAsync(request.PayOsOrderCode, shopId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"L?i khi check ??n hàng: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get list of orders that can be checked by the current shop
        /// </summary>
        /// <param name="request">Request parameters for filtering</param>
        /// <returns>List of checkable orders</returns>
        [HttpGet("checkable")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.RoleSpecialtyShopName)]
        public async Task<IActionResult> GetCheckableOrders([FromQuery] GetOrdersRequestDto request)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var specialtyShop = await _specialtyShopRepository.GetByUserIdAsync(currentUser.Id);
                
                if (specialtyShop == null)
                {
                    return BadRequest(new { Message = "Không tìm th?y thông tin shop cho user này." });
                }

                var result = await _orderService.GetCheckableOrdersAsync(specialtyShop.Id);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"L?i khi l?y danh sách ??n hàng: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get order details for checking purposes using PayOsOrderCode
        /// </summary>
        /// <param name="payOsOrderCode">PayOS Order Code</param>
        /// <returns>Order details and status</returns>
        [HttpGet("details/{payOsOrderCode}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetOrderDetailsByPayOsCode(string payOsOrderCode)
        {
            try
            {
                var result = await _orderService.GetOrderDetailsAsync(payOsOrderCode);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"L?i khi l?y thông tin ??n hàng: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get order details for checking purposes using OrderId (legacy support)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order details and status</returns>
        [HttpGet("details/id/{orderId:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetOrderDetails(Guid orderId)
        {
            try
            {
                // Legacy support - find order by ID and get PayOsOrderCode
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { Message = "Không tìm th?y ??n hàng" });
                }

                var result = await _orderService.GetOrderDetailsAsync(order.PayOsOrderCode);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"L?i khi l?y thông tin ??n hàng: {ex.Message}" });
            }
        }

        /// <summary>
        /// Debug endpoint to check current shop info
        /// </summary>
        [HttpGet("debug/shop-info")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.RoleSpecialtyShopName)]
        public async Task<IActionResult> GetCurrentShopInfo()
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var specialtyShop = await _specialtyShopRepository.GetByUserIdAsync(currentUser.Id);
                
                return Ok(new
                {
                    CurrentUser = new
                    {
                        currentUser.Id,
                        currentUser.Name,
                        currentUser.Email,
                        currentUser.RoleId
                    },
                    SpecialtyShop = specialtyShop != null ? new
                    {
                        specialtyShop.Id,
                        specialtyShop.UserId,
                        specialtyShop.ShopName,
                        specialtyShop.IsActive,
                        specialtyShop.IsShopActive,
                        specialtyShop.Location,
                        specialtyShop.ShopType
                    } : null,
                    Message = specialtyShop != null ? "Shop found successfully" : "No shop found for this user"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error getting shop info: {ex.Message}" });
            }
        }

        /// <summary>
        /// Debug endpoint to test JWT authentication without role requirement
        /// </summary>
        [HttpGet("debug/auth-test")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> TestAuthentication()
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                
                if (currentUser == null)
                {
                    return Unauthorized(new { Message = "Cannot parse user info from token" });
                }

                var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                
                return Ok(new
                {
                    Message = "Authentication successful",
                    CurrentUser = new
                    {
                        currentUser.Id,
                        currentUser.UserId,
                        currentUser.Name,
                        currentUser.Email,
                        currentUser.RoleId,
                        currentUser.PhoneNumber
                    },
                    AllClaims = allClaims,
                    UserRole = User.IsInRole(Constants.RoleSpecialtyShopName) ? "Has Specialty Shop Role" : "No Specialty Shop Role"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error testing authentication: {ex.Message}", StackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Debug endpoint to check order and product ownership for validation using PayOsOrderCode
        /// </summary>
        [HttpGet("debug/order-ownership/{payOsOrderCode}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.RoleSpecialtyShopName)]
        public async Task<IActionResult> CheckOrderOwnership(string payOsOrderCode)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var specialtyShop = await _specialtyShopRepository.GetByUserIdAsync(currentUser.Id);

                if (specialtyShop == null)
                {
                    return BadRequest(new { Message = "No shop found for current user" });
                }

                var include = new[] { "OrderDetails", "OrderDetails.Product" };
                var order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == payOsOrderCode, include);

                if (order == null)
                {
                    return NotFound(new { Message = "Order not found with PayOS Order Code: " + payOsOrderCode });
                }

                var shopUserId = specialtyShop.UserId;
                var orderProducts = order.OrderDetails.Select(od => new
                {
                    ProductId = od.Product?.Id,
                    ProductName = od.Product?.Name,
                    ProductShopId = od.Product?.ShopId,
                    IsOwnedByCurrentShop = od.Product?.ShopId == shopUserId,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                }).ToList();

                var hasAnyOwnedProducts = orderProducts.Any(p => p.IsOwnedByCurrentShop);

                return Ok(new
                {
                    CurrentShop = new
                    {
                        specialtyShop.Id,
                        specialtyShop.ShopName,
                        specialtyShop.UserId
                    },
                    Order = new
                    {
                        order.Id,
                        order.PayOsOrderCode,
                        order.Status,
                        order.TotalAfterDiscount,
                        order.IsChecked
                    },
                    Products = orderProducts,
                    Validation = new
                    {
                        HasAnyOwnedProducts = hasAnyOwnedProducts,
                        CanCheckOrder = hasAnyOwnedProducts && !order.IsChecked,
                        Message = hasAnyOwnedProducts 
                            ? "Shop có quy?n check ??n hàng này"
                            : "Shop KHÔNG có quy?n check ??n hàng này - không bán s?n ph?m nào trong ??n hàng"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error checking order ownership: {ex.Message}" });
            }
        }
    }
}