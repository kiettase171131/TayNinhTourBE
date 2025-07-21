using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Order;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service for order management - used by specialty shops for order checking and delivery confirmation
    /// Uses IsChecked field instead of QR code scanning for simpler order verification
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISpecialtyShopRepository _specialtyShopRepository;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            ISpecialtyShopRepository specialtyShopRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _specialtyShopRepository = specialtyShopRepository;
            _mapper = mapper;
        }

        public async Task<CheckOrderResponseDto> CheckOrderAsync(string payOsOrderCode, Guid shopId)
        {
            try
            {
                // Validate shop exists and is active
                var shop = await _specialtyShopRepository.GetByIdAsync(shopId);
                if (shop == null || !shop.IsActive)
                {
                    return new CheckOrderResponseDto
                    {
                        StatusCode = 403,
                        Message = "Shop không h?p l? ho?c không ho?t ??ng",
                        success = false,
                        IsProcessed = false
                    };
                }

                // Get order with details including products using PayOsOrderCode
                var include = new[] { nameof(Order.OrderDetails), $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}" };
                var order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == payOsOrderCode, include);

                if (order == null)
                {
                    return new CheckOrderResponseDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm th?y ??n hàng v?i mã PayOS này",
                        success = false,
                        IsProcessed = false
                    };
                }

                // Check if order is paid
                if (order.Status != OrderStatus.Paid)
                {
                    return new CheckOrderResponseDto
                    {
                        StatusCode = 400,
                        Message = "??n hàng ch?a ???c thanh toán",
                        success = false,
                        IsProcessed = false
                    };
                }

                // CRITICAL VALIDATION: Check if this shop owns any products in the order
                var shopUserId = shop.UserId; // Get the User ID associated with this shop
                var orderProductShopIds = order.OrderDetails
                    .Where(od => od.Product != null)
                    .Select(od => od.Product.ShopId) // Product.ShopId contains User ID of the shop owner
                    .Distinct()
                    .ToList();

                if (!orderProductShopIds.Contains(shopUserId))
                {
                    // This shop doesn't sell any products in this order
                    var productNames = order.OrderDetails
                        .Where(od => od.Product != null)
                        .Select(od => od.Product.Name)
                        .ToList();

                    return new CheckOrderResponseDto
                    {
                        StatusCode = 403,
                        Message = $"Shop '{shop.ShopName}' không có quy?n check ??n hàng này vì không bán các s?n ph?m trong ??n hàng",
                        success = false,
                        IsProcessed = false,
                        Products = productNames.Select(name => new OrderDetailDto { ProductName = name }).ToList()
                    };
                }

                // Check if order was already checked
                bool wasAlreadyChecked = order.IsChecked;

                // Get customer information
                var customer = await _userRepository.GetByIdAsync(order.UserId);
                var customerDto = customer != null ? new CustomerInfoDto
                {
                    UserId = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber
                } : null;

                string message;
                bool isProcessed = false;
                DateTime? checkedAt = null;
                int statusCode = 200;

                if (wasAlreadyChecked)
                {
                    // Get information about who checked this order
                    var checkedByShop = order.CheckedByShopId.HasValue ? 
                        await _specialtyShopRepository.GetByIdAsync(order.CheckedByShopId.Value) : null;
                    
                    var checkedByShopName = checkedByShop?.ShopName ?? "Không xác ??nh";
                    var checkedAtTime = order.CheckedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Không xác ??nh";
                    
                    // Check if it's the same shop trying to check again or a different shop
                    if (order.CheckedByShopId == shopId)
                    {
                        message = $"B?n ?ã check ??n hàng này r?i lúc {checkedAtTime}! Không th? check l?i l?n n?a.";
                    }
                    else
                    {
                        message = $"??n hàng này ?ã ???c check b?i shop '{checkedByShopName}' lúc {checkedAtTime}. Không th? check l?i.";
                    }
                    
                    statusCode = 409; // Conflict status code for already processed
                    checkedAt = order.CheckedAt;
                }
                else
                {
                    // Order is valid and not checked - mark as checked
                    order.IsChecked = true;
                    order.CheckedAt = DateTime.UtcNow;
                    order.CheckedByShopId = shopId;
                    order.UpdatedAt = DateTime.UtcNow;

                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();

                    message = "Check ??n hàng thành công - ?ã giao hàng cho khách";
                    isProcessed = true;
                    checkedAt = order.CheckedAt;
                }

                return new CheckOrderResponseDto
                {
                    StatusCode = statusCode,
                    Message = message,
                    success = true,
                    IsProcessed = isProcessed,
                    WasAlreadyChecked = wasAlreadyChecked,
                    CheckedAt = checkedAt,
                    CheckedByShopName = shop.ShopName,
                    Order = _mapper.Map<OrderDto>(order),
                    Customer = customerDto,
                    Products = _mapper.Map<List<OrderDetailDto>>(order.OrderDetails)
                };
            }
            catch (Exception ex)
            {
                return new CheckOrderResponseDto
                {
                    StatusCode = 500,
                    Message = $"L?i khi check ??n hàng: {ex.Message}",
                    success = false,
                    IsProcessed = false
                };
            }
        }

        public async Task<OrderDetailsResponseDto> GetOrderDetailsAsync(string payOsOrderCode)
        {
            try
            {
                var include = new[] { nameof(Order.OrderDetails), $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}" };
                var order = await _orderRepository.GetFirstOrDefaultAsync(x => x.PayOsOrderCode == payOsOrderCode, include);

                if (order == null)
                {
                    return new OrderDetailsResponseDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm th?y ??n hàng v?i mã PayOS này",
                        success = false
                    };
                }

                string? checkedByShopName = null;
                if (order.CheckedByShopId.HasValue)
                {
                    var shop = await _specialtyShopRepository.GetByIdAsync(order.CheckedByShopId.Value);
                    checkedByShopName = shop?.ShopName;
                }

                return new OrderDetailsResponseDto
                {
                    StatusCode = 200,
                    Message = "L?y thông tin ??n hàng thành công",
                    success = true,
                    OrderId = order.Id,
                    PayOsOrderCode = order.PayOsOrderCode,
                    IsChecked = order.IsChecked,
                    CheckedAt = order.CheckedAt,
                    CheckedByShopName = checkedByShopName,
                    Order = _mapper.Map<OrderDto>(order)
                };
            }
            catch (Exception ex)
            {
                return new OrderDetailsResponseDto
                {
                    StatusCode = 500,
                    Message = $"L?i khi l?y thông tin ??n hàng: {ex.Message}",
                    success = false
                };
            }
        }

        public async Task<OrderListResponseDto> GetCheckableOrdersAsync(Guid shopId)
        {
            try
            {
                // Get shop information
                var shop = await _specialtyShopRepository.GetByIdAsync(shopId);
                if (shop == null || !shop.IsActive)
                {
                    return new OrderListResponseDto
                    {
                        StatusCode = 403,
                        Message = "Shop không h?p l? ho?c không ho?t ??ng",
                        success = false
                    };
                }

                // Get all paid orders that contain products from this shop
                var include = new[] { nameof(Order.OrderDetails), $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}" };
                var allPaidOrders = await _orderRepository.GetAllAsync(
                    o => o.Status == OrderStatus.Paid,
                    include);

                // Filter orders that contain products from this shop
                var shopUserId = shop.UserId;
                var checkableOrders = allPaidOrders
                    .Where(order => order.OrderDetails.Any(od => od.Product != null && od.Product.ShopId == shopUserId))
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();

                return new OrderListResponseDto
                {
                    StatusCode = 200,
                    Message = "L?y danh sách ??n hàng thành công",
                    success = true,
                    Orders = _mapper.Map<List<OrderDto>>(checkableOrders),
                    TotalCount = checkableOrders.Count
                };
            }
            catch (Exception ex)
            {
                return new OrderListResponseDto
                {
                    StatusCode = 500,
                    Message = $"L?i khi l?y danh sách ??n hàng: {ex.Message}",
                    success = false
                };
            }
        }
    }
}