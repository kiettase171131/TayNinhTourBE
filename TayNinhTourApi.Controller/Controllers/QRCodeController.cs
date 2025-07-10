using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.QRCode;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller for QR code management - used by specialty shops for customer pickup verification
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly ISpecialtyShopRepository _specialtyShopRepository;
        private readonly IOrderRepository _orderRepository;

        public QRCodeController(IQRCodeService qrCodeService, ISpecialtyShopRepository specialtyShopRepository, IOrderRepository orderRepository)
        {
            _qrCodeService = qrCodeService;
            _specialtyShopRepository = specialtyShopRepository;
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Generate QR code for a paid order (typically called automatically after payment)
        /// </summary>
        /// <param name="orderId">Order ID to generate QR code for</param>
        /// <returns>QR code data and image URL</returns>
        [HttpPost("generate/{orderId:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GenerateQRCode(Guid orderId)
        {
            try
            {
                var result = await _qrCodeService.GenerateQRCodeAsync(orderId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"L?i khi t?o mã QR: {ex.Message}" });
            }
        }

        /// <summary>
        /// Scan and process QR code by specialty shop (combines verification and marking as used)
        /// When a specialty shop scans the QR code, it will verify the order and immediately mark it as delivered
        /// Only accessible by users with "Specialty Shop" role
        /// </summary>
        /// <param name="request">QR code scan request</param>
        /// <returns>Order details and processing result</returns>
        [HttpPost("scan")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.RoleSpecialtyShopName)]
        public async Task<IActionResult> ScanQRCode([FromBody] ScanQRCodeRequestDto request)
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
                        Message = "Shop c?a b?n hi?n ?ang ?óng c?a. Vui lòng m? shop tr??c khi quét QR.",
                        ShopInfo = new { specialtyShop.Id, specialtyShop.ShopName, specialtyShop.IsShopActive }
                    });
                }

                var shopId = specialtyShop.Id; // Use the SpecialtyShop ID, not User ID
                
                var result = await _qrCodeService.ScanAndProcessQRCodeAsync(request.QRCodeData, shopId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"L?i khi quét và x? lý mã QR: {ex.Message}" });
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
                    Message = specialtyShop != null ? "Shop found successfully" : "No shop found for this user",
                    PossibleCauses = specialtyShop == null ? new[]
                    {
                        "1. User ???c c?p role 'Specialty Shop' th? công nh?ng ch?a có SpecialtyShop record",
                        "2. SpecialtyShop application ???c approve nh?ng ch?a t?o SpecialtyShop entity", 
                        "3. SpecialtyShop record b? xóa ho?c corrupted",
                        "4. UserId mapping không ?úng"
                    } : null,
                    NextSteps = specialtyShop == null ? new[]
                    {
                        "1. Ki?m tra b?ng SpecialtyShops v?i UserId = " + currentUser.Id,
                        "2. Ki?m tra b?ng SpecialtyShopApplications",
                        "3. T?o SpecialtyShop record th? công n?u c?n",
                        "4. Ho?c liên h? admin ?? fix data"
                    } : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error getting shop info: {ex.Message}" });
            }
        }

        /// <summary>
        /// Admin endpoint to create SpecialtyShop record for user who has role but missing record
        /// </summary>
        [HttpPost("debug/create-missing-shop")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> CreateMissingSpecialtyShop([FromBody] CreateMissingShopRequest request)
        {
            try
            {
                // Check if user exists and has Specialty Shop role
                var userRepository = _orderRepository; // We'll need to add IUserRepository to constructor
                
                // For now, create a basic shop record
                var newShop = new TayNinhTourApi.DataAccessLayer.Entities.SpecialtyShop
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    ShopName = request.ShopName ?? "Shop c?n c?p nh?t thông tin",
                    Location = request.Location ?? "C?n c?p nh?t ??a ch?", 
                    PhoneNumber = request.PhoneNumber,
                    ShopType = request.ShopType ?? "General",
                    Description = "Shop ???c t?o ?? fix missing record",
                    IsActive = true,
                    IsShopActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString())
                };

                await _specialtyShopRepository.AddAsync(newShop);
                await _specialtyShopRepository.SaveChangesAsync();

                return Ok(new
                {
                    Message = "SpecialtyShop record created successfully",
                    ShopId = newShop.Id,
                    UserId = newShop.UserId,
                    ShopName = newShop.ShopName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error creating shop: {ex.Message}" });
            }
        }

        /// <summary>
        /// Debug endpoint to check order and product ownership for validation
        /// </summary>
        [HttpGet("debug/order-ownership/{orderId:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Constants.RoleSpecialtyShopName)]
        public async Task<IActionResult> CheckOrderOwnership(Guid orderId)
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
                var order = await _orderRepository.GetByIdAsync(orderId, include);

                if (order == null)
                {
                    return NotFound(new { Message = "Order not found" });
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
                        order.IsQRCodeUsed
                    },
                    Products = orderProducts,
                    Validation = new
                    {
                        HasAnyOwnedProducts = hasAnyOwnedProducts,
                        CanScanQR = hasAnyOwnedProducts && !order.IsQRCodeUsed,
                        Message = hasAnyOwnedProducts 
                            ? "Shop có quy?n quét QR này"
                            : "Shop KHÔNG có quy?n quét QR này - không bán s?n ph?m nào trong ??n hàng"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error checking order ownership: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get QR code details for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>QR code details and status</returns>
        [HttpGet("details/{orderId:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetQRCodeDetails(Guid orderId)
        {
            try
            {
                var result = await _qrCodeService.GetQRCodeDetailsAsync(orderId);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"L?i khi l?y thông tin mã QR: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get QR code image by order ID (for customer display)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>QR code details including image URL</returns>
        [HttpGet("customer/{orderId:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCustomerQRCode(Guid orderId)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                
                // First get QR code details
                var qrDetails = await _qrCodeService.GetQRCodeDetailsAsync(orderId);
                
                if (!qrDetails.success)
                {
                    return StatusCode(qrDetails.StatusCode, qrDetails);
                }

                // Verify that the current user owns this order
                if (qrDetails.Order?.UserId != currentUser.Id)
                {
                    return Forbid("B?n không có quy?n xem mã QR này");
                }

                return Ok(new
                {
                    QRCodeImageUrl = qrDetails.QRCodeImageUrl,
                    OrderCode = qrDetails.Order?.PayOsOrderCode,
                    TotalAmount = qrDetails.Order?.TotalAfterDiscount,
                    IsUsed = qrDetails.IsUsed,
                    UsedAt = qrDetails.UsedAt,
                    UsedByShopName = qrDetails.UsedByShopName,
                    Message = qrDetails.IsUsed ? "Mã QR ?ã ???c s? d?ng" : "Mã QR s?n sàng ?? s? d?ng"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"L?i khi l?y mã QR khách hàng: {ex.Message}" });
            }
        }
    }
}