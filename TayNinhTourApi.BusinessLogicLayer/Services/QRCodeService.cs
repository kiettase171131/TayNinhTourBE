using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using QRCoder;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.QRCode;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service for QR code generation and verification for customer pickup at specialty shops
    /// </summary>
    public class QRCodeService : IQRCodeService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISpecialtyShopRepository _specialtyShopRepository;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QRCodeService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            ISpecialtyShopRepository specialtyShopRepository,
            IMapper mapper,
            IHostingEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _specialtyShopRepository = specialtyShopRepository;
            _mapper = mapper;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GenerateQRCodeResponseDto> GenerateQRCodeAsync(Guid orderId)
        {
            try
            {
                var include = new[] { nameof(Order.OrderDetails), $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}" };
                var order = await _orderRepository.GetByIdAsync(orderId, include);

                if (order == null)
                {
                    return new GenerateQRCodeResponseDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm th?y ??n hàng",
                        success = false
                    };
                }

                if (order.Status != OrderStatus.Paid)
                {
                    return new GenerateQRCodeResponseDto
                    {
                        StatusCode = 400,
                        Message = "Ch? có th? t?o mã QR cho ??n hàng ?ã thanh toán",
                        success = false
                    };
                }

                // Check if QR code already exists
                if (!string.IsNullOrEmpty(order.QRCodeData) && !string.IsNullOrEmpty(order.QRCodeImageUrl))
                {
                    return new GenerateQRCodeResponseDto
                    {
                        StatusCode = 200,
                        Message = "Mã QR ?ã t?n t?i",
                        success = true,
                        QRCodeData = order.QRCodeData,
                        QRCodeImageUrl = order.QRCodeImageUrl,
                        Order = _mapper.Map<OrderDto>(order)
                    };
                }

                // Create QR code data object
                var qrData = new
                {
                    OrderId = order.Id,
                    PayOsOrderCode = order.PayOsOrderCode,
                    UserId = order.UserId,
                    TotalAmount = order.TotalAfterDiscount,
                    CreatedAt = order.CreatedAt,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                var qrDataString = JsonSerializer.Serialize(qrData);

                // Generate QR code image
                var qrImageUrl = await GenerateQRCodeImageAsync(qrDataString, order.Id);

                // Update order with QR code information
                order.QRCodeData = qrDataString;
                order.QRCodeImageUrl = qrImageUrl;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();

                return new GenerateQRCodeResponseDto
                {
                    StatusCode = 200,
                    Message = "T?o mã QR thành công",
                    success = true,
                    QRCodeData = qrDataString,
                    QRCodeImageUrl = qrImageUrl,
                    Order = _mapper.Map<OrderDto>(order)
                };
            }
            catch (Exception ex)
            {
                return new GenerateQRCodeResponseDto
                {
                    StatusCode = 500,
                    Message = $"L?i khi t?o mã QR: {ex.Message}",
                    success = false
                };
            }
        }

        public async Task<ScanQRCodeResponseDto> ScanAndProcessQRCodeAsync(string qrCodeData, Guid shopId)
        {
            try
            {
                // Validate shop exists and is active
                var shop = await _specialtyShopRepository.GetByIdAsync(shopId);
                if (shop == null || !shop.IsActive)
                {
                    return new ScanQRCodeResponseDto
                    {
                        StatusCode = 403,
                        Message = "Shop không h?p l? ho?c không ho?t ??ng",
                        success = false,
                        IsProcessed = false
                    };
                }

                // Parse QR code data
                JsonElement qrData;
                try
                {
                    qrData = JsonSerializer.Deserialize<JsonElement>(qrCodeData);
                }
                catch
                {
                    return new ScanQRCodeResponseDto
                    {
                        StatusCode = 400,
                        Message = "Mã QR không h?p l?",
                        success = false,
                        IsProcessed = false
                    };
                }

                // Extract order ID from QR data
                if (!qrData.TryGetProperty("OrderId", out var orderIdProperty))
                {
                    return new ScanQRCodeResponseDto
                    {
                        StatusCode = 400,
                        Message = "Mã QR không ch?a thông tin ??n hàng",
                        success = false,
                        IsProcessed = false
                    };
                }

                var orderIdString = orderIdProperty.GetString();
                if (!Guid.TryParse(orderIdString, out Guid orderId))
                {
                    return new ScanQRCodeResponseDto
                    {
                        StatusCode = 400,
                        Message = "Mã QR không ch?a thông tin ??n hàng h?p l?",
                        success = false,
                        IsProcessed = false
                    };
                }

                // Get order with details including products
                var include = new[] { nameof(Order.OrderDetails), $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}" };
                var order = await _orderRepository.GetByIdAsync(orderId, include);

                if (order == null)
                {
                    return new ScanQRCodeResponseDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm th?y ??n hàng",
                        success = false,
                        IsProcessed = false
                    };
                }

                // Verify QR code data matches order
                if (order.QRCodeData != qrCodeData)
                {
                    return new ScanQRCodeResponseDto
                    {
                        StatusCode = 400,
                        Message = "Mã QR không kh?p v?i ??n hàng",
                        success = false,
                        IsProcessed = false
                    };
                }

                // Check if order is paid
                if (order.Status != OrderStatus.Paid)
                {
                    return new ScanQRCodeResponseDto
                    {
                        StatusCode = 400,
                        Message = "??n hàng ch?a ???c thanh toán",
                        success = false,
                        IsProcessed = false
                    };
                }

                // ? CRITICAL VALIDATION: Check if this shop owns any products in the order
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

                    return new ScanQRCodeResponseDto
                    {
                        StatusCode = 403,
                        Message = $"Shop '{shop.ShopName}' không có quy?n quét QR này vì không bán các s?n ph?m trong ??n hàng",
                        success = false,
                        IsProcessed = false,
                        Products = productNames.Select(name => new OrderDetailDto { ProductName = name }).ToList()
                    };
                }

                // Check if QR code was already used
                bool wasAlreadyUsed = order.IsQRCodeUsed;

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
                DateTime? processedAt = null;

                if (wasAlreadyUsed)
                {
                    // QR code was already used - just return info, don't process again
                    message = "Mã QR ?ã ???c s? d?ng tr??c ?ó";
                    processedAt = order.QRCodeUsedAt;
                }
                else
                {
                    // QR code is valid and unused - mark as used
                    order.IsQRCodeUsed = true;
                    order.QRCodeUsedAt = DateTime.UtcNow;
                    order.QRCodeUsedByShopId = shopId;
                    order.UpdatedAt = DateTime.UtcNow;

                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();

                    message = "Quét mã QR thành công - ?ã giao hàng cho khách";
                    isProcessed = true;
                    processedAt = order.QRCodeUsedAt;
                }

                return new ScanQRCodeResponseDto
                {
                    StatusCode = 200,
                    Message = message,
                    success = true,
                    IsProcessed = isProcessed,
                    WasAlreadyUsed = wasAlreadyUsed,
                    ProcessedAt = processedAt,
                    ProcessedByShopName = shop.ShopName,
                    Order = _mapper.Map<OrderDto>(order),
                    Customer = customerDto,
                    Products = _mapper.Map<List<OrderDetailDto>>(order.OrderDetails)
                };
            }
            catch (Exception ex)
            {
                return new ScanQRCodeResponseDto
                {
                    StatusCode = 500,
                    Message = $"L?i khi x? lý mã QR: {ex.Message}",
                    success = false,
                    IsProcessed = false
                };
            }
        }

        public async Task<QRCodeDetailsResponseDto> GetQRCodeDetailsAsync(Guid orderId)
        {
            try
            {
                var include = new[] { nameof(Order.OrderDetails), $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}" };
                var order = await _orderRepository.GetByIdAsync(orderId, include);

                if (order == null)
                {
                    return new QRCodeDetailsResponseDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm th?y ??n hàng",
                        success = false
                    };
                }

                string? usedByShopName = null;
                if (order.QRCodeUsedByShopId.HasValue)
                {
                    var shop = await _specialtyShopRepository.GetByIdAsync(order.QRCodeUsedByShopId.Value);
                    usedByShopName = shop?.ShopName;
                }

                return new QRCodeDetailsResponseDto
                {
                    StatusCode = 200,
                    Message = "L?y thông tin mã QR thành công",
                    success = true,
                    OrderId = order.Id,
                    QRCodeData = order.QRCodeData,
                    QRCodeImageUrl = order.QRCodeImageUrl,
                    IsUsed = order.IsQRCodeUsed,
                    UsedAt = order.QRCodeUsedAt,
                    UsedByShopName = usedByShopName,
                    Order = _mapper.Map<OrderDto>(order)
                };
            }
            catch (Exception ex)
            {
                return new QRCodeDetailsResponseDto
                {
                    StatusCode = 500,
                    Message = $"L?i khi l?y thông tin mã QR: {ex.Message}",
                    success = false
                };
            }
        }

        private async Task<string> GenerateQRCodeImageAsync(string qrDataString, Guid orderId)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrDataString, QRCodeGenerator.ECCLevel.Q);
            
            using var qrCode = new PngByteQRCode(qrCodeData);
            var pngBytes = qrCode.GetGraphic(10);

            // Create directory for QR code images
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var qrCodesFolder = Path.Combine(webRoot, "uploads", "qrcodes");

            if (!Directory.Exists(qrCodesFolder))
                Directory.CreateDirectory(qrCodesFolder);

            // Save QR code image
            var fileName = $"qr_{orderId}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var filePath = Path.Combine(qrCodesFolder, fileName);

            await File.WriteAllBytesAsync(filePath, pngBytes);

            // Generate public URL
            var req = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{req.Scheme}://{req.Host.Value}";
            var fileUrl = $"{baseUrl}/uploads/qrcodes/{fileName}";

            return fileUrl;
        }
    }
}