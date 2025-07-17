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
                        Message = "Shop kh�ng h?p l? ho?c kh�ng ho?t ??ng",
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
                        Message = "Kh�ng t�m th?y ??n h�ng v?i m� PayOS n�y",
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
                        Message = "??n h�ng ch?a ???c thanh to�n",
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
                        Message = $"Shop '{shop.ShopName}' kh�ng c� quy?n check ??n h�ng n�y v� kh�ng b�n c�c s?n ph?m trong ??n h�ng",
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
                    
                    var checkedByShopName = checkedByShop?.ShopName ?? "Kh�ng x�c ??nh";
                    var checkedAtTime = order.CheckedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Kh�ng x�c ??nh";
                    
                    // Check if it's the same shop trying to check again or a different shop
                    if (order.CheckedByShopId == shopId)
                    {
                        message = $"B?n ?� check ??n h�ng n�y r?i l�c {checkedAtTime}! Kh�ng th? check l?i l?n n?a.";
                    }
                    else
                    {
                        message = $"??n h�ng n�y ?� ???c check b?i shop '{checkedByShopName}' l�c {checkedAtTime}. Kh�ng th? check l?i.";
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

                    message = "Check ??n h�ng th�nh c�ng - ?� giao h�ng cho kh�ch";
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
                    Message = $"L?i khi check ??n h�ng: {ex.Message}",
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
                        Message = "Kh�ng t�m th?y ??n h�ng v?i m� PayOS n�y",
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
                    Message = "L?y th�ng tin ??n h�ng th�nh c�ng",
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
                    Message = $"L?i khi l?y th�ng tin ??n h�ng: {ex.Message}",
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
                        Message = "Shop kh�ng h?p l? ho?c kh�ng ho?t ??ng",
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
                    Message = "L?y danh s�ch ??n h�ng th�nh c�ng",
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
                    Message = $"L?i khi l?y danh s�ch ??n h�ng: {ex.Message}",
                    success = false
                };
            }
        }
    }
}