using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Voucher;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICartRepository _cartRepository;
        private readonly IPayOsService _payOsService;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRatingRepository _ratingRepo;
        private readonly IProductReviewRepository _reviewRepo;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IVoucherCodeRepository _voucherCodeRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;

        public ProductService(IProductRepository productRepository, IMapper mapper, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor, IProductImageRepository productImageRepository, ICartRepository cartRepository, IPayOsService payOsService, IOrderRepository orderRepository, IProductReviewRepository productReview, IProductRatingRepository productRating, IVoucherRepository voucherRepository, IVoucherCodeRepository voucherCodeRepository, IOrderDetailRepository orderDetailRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _productImageRepository = productImageRepository;
            _cartRepository = cartRepository;
            _payOsService = payOsService;
            _orderRepository = orderRepository;
            _ratingRepo = productRating;
            _reviewRepo = productReview;
            _voucherRepository = voucherRepository;
            _voucherCodeRepository = voucherCodeRepository;
            _orderDetailRepository = orderDetailRepository;
        }
        public async Task<ResponseGetProductsDto> GetProductsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var include = new string[] { nameof(Product.ProductImages), nameof(Product.ProductRatings) };

            // Default values for pagination
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Predicate lọc
            var predicate = PredicateBuilder.New<Product>(x => !x.IsDeleted);

            // Lọc theo tên sản phẩm
            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(b =>
           EF.Functions.Like(b.Name, $"%{textSearch}%"));
            }

            // Lọc theo trạng thái hoạt động
            if (status.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == status);
            }

            // Lấy danh sách sản phẩm
            var products = await _productRepository.GenericGetPaginationAsync(pageIndexValue, pageSizeValue, predicate, include);

            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSizeValue);

            return new ResponseGetProductsDto
            {
                StatusCode = 200,
                Message = "Get product list successfully",
                success = true,
                Data = _mapper.Map<List<ProductDto>>(products),
                TotalRecord = totalProducts,
                TotalPages = totalPages
            };
        }
        public async Task<ResponseGetProductsDto> GetProductsByShopAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status, CurrentUserObject currentUserObject)
        {
            var include = new string[] { nameof(Product.ProductImages) };

            // Default values for pagination
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Predicate lọc
            var predicate = PredicateBuilder.New<Product>(x => !x.IsDeleted && x.CreatedById == currentUserObject.Id);

            // Lọc theo tên sản phẩm
            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(b =>
           EF.Functions.Like(b.Name, $"%{textSearch}%"));
            }

            // Lọc theo trạng thái hoạt động
            if (status.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == status);
            }

            // Lấy danh sách sản phẩm
            var products = await _productRepository.GenericGetPaginationAsync(pageIndexValue, pageSizeValue, predicate, include);

            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSizeValue);

            return new ResponseGetProductsDto
            {
                StatusCode = 200,
                Message = "Get product list successfully",
                Data = _mapper.Map<List<ProductDto>>(products),
                TotalRecord = totalProducts,
                TotalPages = totalPages
            };
        }
        public async Task<ResponseGetProductByIdDto> GetProductByIdAsync(Guid id)
        {
            var include = new string[] { nameof(Product.ProductImages) };

            var predicate = PredicateBuilder.New<Product>(x => !x.IsDeleted);

            var product = await _productRepository.GetByIdAsync(id, include);

            if (product == null || product.IsDeleted)
            {
                return new ResponseGetProductByIdDto
                {
                    StatusCode = 404,
                    Message = "Product not found"
                };
            }

            return new ResponseGetProductByIdDto
            {
                StatusCode = 200,
                success = true,
                Data = _mapper.Map<ProductDto>(product)
            };
        }
        public async Task<BaseResposeDto> DeleteProductAsync(Guid id)
        {
            // Tìm sản phẩm theo id
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null || product.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Product not found"
                };
            }

            // Đánh dấu đã xóa
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            // Lưu thay đổi
            await _productRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Product deleted succcessfully !",
                success = true
            };
        }
        public async Task<ResponseCreateProductDto> CreateProductAsync(RequestCreateProductDto request, CurrentUserObject currentUserObject)
        {
            var isSale = request.SalePercent.HasValue && request.SalePercent.Value > 0;
            var finalPrice = request.IsSale == true && request.SalePercent > 0
            ? request.Price * (1 - (request.SalePercent.Value / 100m))
            : request.Price;
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = finalPrice,
                QuantityInStock = request.QuantityInStock,
                Category = request.Category,
                IsSale = isSale,
                SalePercent = request.SalePercent ?? 0,
                ShopId = currentUserObject.Id,
                CreatedById = currentUserObject.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var uploadedUrls = new List<string>();
            if (request.Files != null && request.Files.Any())
            {
                const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp" };

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "uploads", "products", product.Id.ToString());

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var req = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{req.Scheme}://{req.Host.Value}";

                foreach (var file in request.Files)
                {
                    if (file.Length == 0)
                        continue;

                    if (file.Length > MaxFileSize)
                        return new ResponseCreateProductDto
                        {
                            StatusCode = 400,
                            Message = $"File too large. Max size is {MaxFileSize / (1024 * 1024)} MB."
                        };

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExts.Contains(ext))
                        return new ResponseCreateProductDto
                        {
                            StatusCode = 400,
                            Message = "Invalid file type. Only .png, .jpg, .jpeg, .webp are allowed."
                        };

                    var filename = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsFolder, filename);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var fileUrl = $"{baseUrl}/uploads/products/{product.Id}/{filename}";
                    uploadedUrls.Add(fileUrl);

                    product.ProductImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Url = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = currentUserObject.Id
                    });
                }
            }

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return new ResponseCreateProductDto
            {
                StatusCode = 200,
                Message = "Create successful products",
                success = true,
                ProductId = product.Id,
                ImageUrls = uploadedUrls
            };
        }
        public async Task<BaseResposeDto> UpdateProductAsync(RequestUpdateProductDto request, Guid id, CurrentUserObject currentUserObject)
        {
            var include = new string[] { nameof(Product.ProductImages) };

            var product = await _productRepository.GetByIdAsync(id, include);

            if (product == null || product.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Product not found"
                };
            }

            // Cập nhật thông tin sản phẩm
            product.Name = request.Name ?? product.Name;
            product.Description = request.Description ?? product.Description;
            
            product.QuantityInStock = request.QuantityInStock ?? product.QuantityInStock;

            // ✅ Logic tự động tính giá & giảm giá
            if (request.SalePercent.HasValue && request.SalePercent.Value > 0)
            {
                product.IsSale = true;
                product.SalePercent = request.SalePercent.Value;
                product.Price = (request.Price ?? product.Price) * (1 - request.SalePercent.Value / 100m);
            }
            else
            {
                // Nếu không giảm giá -> set về giá gốc (nếu có truyền giá mới)
                product.IsSale = false;
                product.SalePercent = 0;
                if (request.Price.HasValue)
                    product.Price = request.Price.Value;
            }

            if (request.Category.HasValue)
            {
                product.Category = request.Category.Value;
            }

            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedById = currentUserObject.Id;

            var newUploadedUrls = new List<string>();
            if (request.Files != null && request.Files.Any())
            {
                var existingImages = product.ProductImages.ToList();
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folder = Path.Combine(webRoot, "uploads", "products", product.Id.ToString());

                foreach (var oldImage in existingImages)
                {
                    var uri = new Uri(oldImage.Url);
                    var oldFileName = Path.GetFileName(uri.LocalPath);
                    var oldFilePath = Path.Combine(folder, oldFileName);

                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);

                    await _productRepository.DeleteAsync(oldImage.Id); // _repo2 là ProductImageRepo
                }

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var req = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{req.Scheme}://{req.Host.Value}";

                foreach (var file in request.Files)
                {
                    if (file.Length == 0) continue;

                    if (file.Length > MaxFileSize)
                    {
                        return new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = $"File quá lớn. Tối đa {MaxFileSize / (1024 * 1024)} MB."
                        };
                    }

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExts.Contains(ext))
                    {
                        return new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = "Định dạng không hợp lệ. Chỉ cho phép .png, .jpg, .jpeg, .webp."
                        };
                    }

                    var newFileName = $"{Guid.NewGuid()}{ext}";
                    var newFilePath = Path.Combine(folder, newFileName);
                    using var stream = new FileStream(newFilePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var fileUrl = $"{baseUrl}/uploads/products/{product.Id}/{newFileName}";
                    newUploadedUrls.Add(fileUrl);

                    var productImage = new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Url = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = currentUserObject.Id,
                        IsActive = true
                    };
                    await _productImageRepository.AddAsync(productImage);
                }
            }

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Product update successful",
                success = true
            };
        }
        public async Task<BaseResposeDto> AddToCartAsync(RequestAddMultipleToCartDto request, CurrentUserObject currentUser)
        {
            var response = new BaseResposeDto
            {
                StatusCode = 200,
                success = true,
                Message = "Đã thêm các sản phẩm vào giỏ hàng"
            };

            if (request.Items == null || !request.Items.Any())
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Danh sách sản phẩm rỗng",
                    success = false
                };
            }

            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || product.IsDeleted || !product.IsActive)
                {
                    response.ValidationErrors.Add($"Sản phẩm {item.ProductId} không tồn tại hoặc ngưng hoạt động.");
                    response.success = false;
                    continue;
                }

                if (item.Quantity <= 0)
                {
                    response.ValidationErrors.Add($"Sản phẩm {product.Name}: số lượng không hợp lệ.");
                    response.success = false;
                    continue;
                }

                var existingCart = await _cartRepository.GetFirstOrDefaultAsync(x =>
                    x.UserId == currentUser.Id && x.ProductId == item.ProductId);
                if (existingCart != null && existingCart.Quantity >= product.QuantityInStock)
                {
                    response.ValidationErrors.Add(
                        $"Sản phẩm {product.Name}: đã thêm đủ số lượng tối đa trong kho ({product.QuantityInStock}), không thể thêm nữa.");
                    response.success = false;
                    continue;
                }
                var totalQuantityRequested = item.Quantity;
                if (existingCart != null)
                    totalQuantityRequested += existingCart.Quantity;

                if (totalQuantityRequested > product.QuantityInStock)
                {
                    response.ValidationErrors.Add(
                        $"Sản phẩm {product.Name}: Số lượng bạn chọn đã đạt mức tối đa của sản phẩm này");
                    response.success = false;
                    continue;
                }
                var quantityInCart = existingCart?.Quantity ?? 0;
                var remainingStock = product.QuantityInStock - quantityInCart;

                if (remainingStock <= 0)
                {
                    response.ValidationErrors.Add(
                        $"Sản phẩm {product.Name}: đã thêm đủ số lượng tối đa trong kho ({product.QuantityInStock}), không thể thêm nữa.");
                    response.success = false;
                    continue;
                }

                int quantityToAdd = item.Quantity;
                if (item.Quantity > remainingStock)
                {
                    quantityToAdd = remainingStock;
                    response.ValidationErrors.Add(
                        $"Sản phẩm {product.Name}: chỉ còn lại {remainingStock} sản phẩm, đã thêm {remainingStock} vào giỏ hàng.");
                    response.success = false;
                }

                if (existingCart != null)
                {
                    existingCart.Quantity = item.Quantity;
                    existingCart.UpdatedAt = DateTime.UtcNow;
                    existingCart.UpdatedById = currentUser.Id;
                    await _cartRepository.UpdateAsync(existingCart);
                }
                else
                {
                    var newCart = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        UserId = currentUser.Id,    
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = currentUser.Id
                    };
                    await _cartRepository.AddAsync(newCart);
                }
            }

            await _cartRepository.SaveChangesAsync();

            if (!response.success)
            {
                response.StatusCode = 400;
                response.Message = "Có lỗi với một số sản phẩm khi thêm vào giỏ hàng.";
            }

            return response;
        }
        public async Task<ResponseGetCartDto> GetCartAsync(CurrentUserObject currentUser)
        {
            var include = new string[] { nameof(CartItem.Product), $"{nameof(CartItem.Product)}.{nameof(Product.ProductImages)}",
             $"{nameof(CartItem.Product)}.{nameof(Product.Shop)}"};

            var cartItems = await _cartRepository.GetAllAsync(x => x.UserId == currentUser.Id, include);

            var items = cartItems.Select(x => new CartItemDto
            {
                CartItemId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                ShopName = x.Product.Shop.Name,
                Quantity = x.Quantity,
                Price = x.Product.Price,
                Total = x.Quantity * x.Product.Price,
                ImageUrl = x.Product.ProductImages.FirstOrDefault()?.Url
            }).ToList();

            return new ResponseGetCartDto
            {
                StatusCode = 200,
                success = true,
                Data = items,
                TotalAmount = items.Sum(i => i.Total)
            };
        }
        public async Task<BaseResposeDto> RemoveFromCartAsync(CurrentUserObject currentUser)
        {
            var cartItems = await _cartRepository.GetAllAsync(x => x.UserId == currentUser.Id);

            if (!cartItems.Any())
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Giỏ hàng của bạn đang trống.",
                    success = false
                };
            }

            foreach (var item in cartItems)
            {
                await _cartRepository.DeleteAsync(item.Id);
            }

            await _cartRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Đã xoá toàn bộ sản phẩm khỏi giỏ hàng",
                success = true
            };
        }
        public async Task ClearCartAndUpdateInventoryAsync(Guid orderId)
        {
            try
            {
                Console.WriteLine($"ClearCartAndUpdateInventoryAsync called for order: {orderId}");

                var order = await _orderRepository.GetByIdAsync(orderId, new[] { nameof(Order.OrderDetails) });

                if (order == null)
                {
                    Console.WriteLine($"Order not found: {orderId}");
                    return;
                }

                Console.WriteLine($"Order found: {orderId}, Status: {order.Status}, OrderDetails count: {order.OrderDetails?.Count}");

                if (order.Status != OrderStatus.Paid)
                {
                    Console.WriteLine($"Order status is not PAID, current status: {order.Status}");
                    return;
                }

                // ✅ 1. Đánh dấu voucher code đã sử dụng (nếu có)
                if (!string.IsNullOrEmpty(order.VoucherCode))
                {
                    Console.WriteLine($"Processing voucher code: {order.VoucherCode}");
                    
                    var voucherCode = await _voucherCodeRepository.GetByCodeAsync(order.VoucherCode);
                    if (voucherCode == null)
                    {
                        Console.WriteLine($"Voucher code not found: {order.VoucherCode}");
                    }
                    else if (voucherCode.IsUsed)
                    {
                        Console.WriteLine($"Voucher code {order.VoucherCode} already marked as used by user {voucherCode.UsedByUserId} at {voucherCode.UsedAt}");
                    }
                    else
                    {
                        // Kiểm tra xem voucher đã được claim bởi user này chưa
                        if (voucherCode.IsClaimed && voucherCode.ClaimedByUserId != order.UserId)
                        {
                            Console.WriteLine($"WARNING: Voucher code {order.VoucherCode} was claimed by user {voucherCode.ClaimedByUserId} but being used by user {order.UserId}");
                        }
                        else if (voucherCode.IsClaimed)
                        {
                            Console.WriteLine($"Voucher code {order.VoucherCode} was properly claimed by user {order.UserId} on {voucherCode.ClaimedAt}");
                        }
                        else
                        {
                            Console.WriteLine($"Voucher code {order.VoucherCode} was used directly without claiming (backward compatibility)");
                        }

                        // Đánh dấu voucher đã sử dụng
                        voucherCode.IsUsed = true;
                        voucherCode.UsedByUserId = order.UserId;
                        voucherCode.UsedAt = DateTime.UtcNow;
                        voucherCode.UpdatedAt = DateTime.UtcNow;
                        voucherCode.UpdatedById = order.UserId;
                        
                        await _voucherCodeRepository.UpdateAsync(voucherCode);
                        await _voucherCodeRepository.SaveChangesAsync();
                        Console.WriteLine($"Voucher code {order.VoucherCode} successfully marked as used by user {order.UserId}");
                    }
                }
                else
                {
                    Console.WriteLine("No voucher code found in order");
                }

                // ✅ 2. Giảm tồn kho sản phẩm
                Console.WriteLine("Starting inventory update...");
                foreach (var detail in order.OrderDetails)
                {
                    Console.WriteLine($"Processing product: {detail.ProductId}, Quantity to subtract: {detail.Quantity}");

                    var product = await _productRepository.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        var oldQuantity = product.QuantityInStock;
                        product.QuantityInStock -= detail.Quantity;
                        if (product.QuantityInStock < 0) product.QuantityInStock = 0;

                        Console.WriteLine($"Product {detail.ProductId}: {oldQuantity} -> {product.QuantityInStock}");
                        await _productRepository.UpdateAsync(product);
                    }
                    else
                    {
                        Console.WriteLine($"Product not found: {detail.ProductId}");
                    }
                }
                await _productRepository.SaveChangesAsync();
                Console.WriteLine("Inventory update completed");

                // ✅ 3. Xóa chỉ những cart items đã được checkout, không phải toàn bộ giỏ hàng
                Console.WriteLine("Starting cart cleanup...");
                var productIdsInOrder = order.OrderDetails.Select(x => x.ProductId).ToList();
                Console.WriteLine($"Product IDs in order: {string.Join(", ", productIdsInOrder)}");

                var cartItemsToRemove = await _cartRepository.GetAllAsync(x =>
                    x.UserId == order.UserId && productIdsInOrder.Contains(x.ProductId));

                Console.WriteLine($"Cart items to remove: {cartItemsToRemove.Count()}");

                if (cartItemsToRemove.Any())
                {
                    _cartRepository.DeleteRange(cartItemsToRemove);
                    await _cartRepository.SaveChangesAsync();
                    Console.WriteLine("Cart cleanup completed");
                }
                else
                {
                    Console.WriteLine("No cart items to remove");
                }

                Console.WriteLine("ClearCartAndUpdateInventoryAsync completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClearCartAndUpdateInventoryAsync error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<CheckoutResultDto?> CheckoutCartAsync(List<Guid> cartItemIds, CurrentUserObject currentUser, Guid? myVoucherCodeId = null)
        {
            if (cartItemIds == null || !cartItemIds.Any())
                throw new ArgumentException("Danh sách sản phẩm không được để trống.");

            if (currentUser == null)
                throw new ArgumentException("Thông tin người dùng không hợp lệ.");

            var include = new[] { nameof(CartItem.Product) };

            var cartItems = await _cartRepository.GetAllAsync(
                x => cartItemIds.Contains(x.Id) && x.UserId == currentUser.Id && !x.IsDeleted,
                include);

            cartItems = cartItems
                .Where(x => x.Product != null && !x.Product.IsDeleted && x.Product.IsActive)
                .ToList();

            if (!cartItems.Any())
                throw new InvalidOperationException("Không tìm thấy sản phẩm hợp lệ trong giỏ hàng.");

            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.QuantityInStock)
                    throw new InvalidOperationException($"Sản phẩm '{item.Product.Name}' chỉ còn {item.Product.QuantityInStock} trong kho.");
            }

            var total = cartItems.Sum(x => x.Product.Price * x.Quantity);
            decimal discountAmount = 0m;
            decimal totalAfterDiscount = total;
            string? finalVoucherCode = null;

            var cartItemDtos = cartItems.Select(x => new CartItemDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity
            }).ToList();

            // Chỉ sử dụng voucher từ kho cá nhân
            if (myVoucherCodeId.HasValue)
            {
                var voucherResult = await ApplyMyVoucherForCartAsync(myVoucherCodeId.Value, cartItemDtos, currentUser);
                if (!voucherResult.success)
                    throw new InvalidOperationException(voucherResult.Message);

                totalAfterDiscount = voucherResult.FinalPrice;
                discountAmount = voucherResult.DiscountAmount;
                
                // Lấy voucher code để lưu vào order
                var userVoucher = await _voucherCodeRepository.GetUserVoucherCodeAsync(myVoucherCodeId.Value, currentUser.Id);
                finalVoucherCode = userVoucher?.Code;
            }

            if (totalAfterDiscount <= 0)
                throw new InvalidOperationException("Tổng tiền thanh toán không hợp lệ sau khi áp dụng voucher.");
                
            // Tạo PayOsOrderCode với format TNDT + 10 số
            var timestamp = VietnamTimeZoneUtility.GetVietnamNow().Ticks.ToString();
            var random = new Random().Next(100, 999).ToString(); // 3 số random
            var payOsOrderCodeString = $"TNDT{timestamp.Substring(Math.Max(0, timestamp.Length - 7))}{random}";
            
            // Lưu vào DB dưới dạng string thay vì long
            var order = new Order
            {
                UserId = currentUser.Id,
                TotalAmount = total,
                DiscountAmount = discountAmount,
                TotalAfterDiscount = totalAfterDiscount,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedById = currentUser.Id,
                VoucherCode = finalVoucherCode,
                PayOsOrderCode = payOsOrderCodeString, // Lưu string với prefix TNDT
                OrderDetails = cartItems.Select(x => new OrderDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.Product.Price,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUser.Id
                }).ToList()
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // Truyền PayOsOrderCode thay vì order.Id để URL callback hiển thị đúng mã đơn hàng
            var checkoutUrl = await _payOsService.CreatePaymentUrlAsync(
                totalAfterDiscount,
                payOsOrderCodeString, // Sử dụng PayOsOrderCode thay vì order.Id.ToString()
                "https://tndt.netlify.app"
            );

            return new CheckoutResultDto
            {
                CheckoutUrl = checkoutUrl,
                OrderId = order.Id,
                TotalOriginal = total,
                DiscountAmount = discountAmount,
                TotalAfterDiscount = totalAfterDiscount
            };
        }


        public async Task<OrderStatus> GetOrderPaymentStatusAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng");

            var status = await _payOsService.GetOrderPaymentStatusAsync(order.PayOsOrderCode.ToString());

            // Nếu muốn cập nhật status trong DB thì xử lý tại service này (không ở controller)
            if (order.Status != status)
            {
                order.Status = status;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();
            }

            return status;
        }

        public async Task<BaseResposeDto> FeedbackProductAsync(CreateProductFeedbackDto dto, Guid userId)
        {
            // Kiểm tra đơn hàng đã nhận thành công hay chưa (IsChecked = true)
            var hasCheckedOrder = await _orderDetailRepository.AnyAsync(od =>
                od.ProductId == dto.ProductId &&
                od.Order.UserId == userId &&
                od.Order.IsChecked == true);

            if (!hasCheckedOrder)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Bạn chỉ được đánh giá khi đã nhận hàng thành công."
                };
            }
            // Update or insert rating
            var existingRating = await _ratingRepo.GetFirstOrDefaultAsync(
                r => r.ProductId == dto.ProductId && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Rating = dto.Rating;
                await _ratingRepo.UpdateAsync(existingRating);
            }
            else
            {
                var newRating = new ProductRating
                {
                    ProductId = dto.ProductId,
                    UserId = userId,
                    Rating = dto.Rating
                };
                await _ratingRepo.AddAsync(newRating);
            }

            // Always add new review
            var review = new ProductReview
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Content = dto.Review
            };
            await _reviewRepo.AddAsync(review);

            // Save all changes
            await _ratingRepo.SaveChangesAsync();
            await _reviewRepo.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Product feedback submitted successfully"
            };
        }


        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId)
        {
            var includes = new[] { "User" };

            // Lấy ratings (chạy tuần tự)
            var ratings = await _ratingRepo.ListAsync(r => r.ProductId == productId);

            // Lấy reviews kèm user
            var reviews = await _reviewRepo.ListAsync(r => r.ProductId == productId, includes);

            var averageRating = ratings.Any() ? Math.Round(ratings.Average(r => r.Rating), 1) : 0;

            var reviewDtos = reviews.Select(r => new ProductReviewDto
            {
                UserName = r.User.Name,
                Content = r.Content,
                CreatedAt = r.CreatedAt
            }).ToList();

            return new ProductReviewSummaryDto
            {
                StatusCode = 200,
                Message = "Lấy thông tin đánh giá sản phẩm thành công",
                AverageRating = averageRating,
                Reviews = reviewDtos
            };
        }


        public async Task<ApplyVoucherResult> ApplyVoucherForCartAsync(string voucherCode, List<CartItemDto> cartItems)
        {
            if (!cartItems.Any())
                return new ApplyVoucherResult
                {
                    StatusCode = 400,
                    Message = "Giỏ hàng không có sản phẩm nào để áp dụng voucher.",
                    success = false
                };

            var now = DateTime.UtcNow;

            // Tìm mã voucher cụ thể
            var voucherCodeEntity = await _voucherCodeRepository.GetByCodeAsync(voucherCode.Trim());

            if (voucherCodeEntity == null)
                return new ApplyVoucherResult
                {
                    StatusCode = 404,
                    Message = "Mã voucher không tồn tại.",
                    success = false
                };

            if (voucherCodeEntity.IsUsed)
                return new ApplyVoucherResult
                {
                    StatusCode = 400,
                    Message = "Mã voucher đã được sử dụng.",
                    success = false
                };

            var voucher = voucherCodeEntity.Voucher;

            if (!voucher.IsActive || voucher.StartDate > now || voucher.EndDate < now)
                return new ApplyVoucherResult
                {
                    StatusCode = 400,
                    Message = "Voucher không hợp lệ hoặc đã hết hạn.",
                    success = false
                };

            decimal totalOriginal = 0m;

            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null) continue;
                // Nếu sản phẩm đang giảm giá thì không áp dụng voucher
                if (product.IsSale)
                {
                    return new ApplyVoucherResult
                    {
                        StatusCode = 400,
                        Message = $"Sản phẩm \"{product.Name}\" đang được giảm giá, không thể áp dụng voucher.",
                        success = false
                    };
                }
                totalOriginal += product.Price * item.Quantity;
            }

            if (totalOriginal <= 0)
            {
                return new ApplyVoucherResult
                {
                    StatusCode = 400,
                    Message = "Tổng tiền giỏ hàng không hợp lệ.",
                    success = false
                };
            }

            decimal discount = 0m;

            if (voucher.DiscountAmount > 0)
                discount = voucher.DiscountAmount;
            else if (voucher.DiscountPercent.HasValue)
                discount = totalOriginal * voucher.DiscountPercent.Value / 100m;

            if (discount > totalOriginal)
                discount = totalOriginal;

            var finalPrice = totalOriginal - discount;

            // PayOS yêu cầu >=1
            if (finalPrice < 1m)
                finalPrice = 1m;

            return new ApplyVoucherResult
            {
                StatusCode = 200,
                Message = "Áp dụng voucher thành công.",
                success = true,
                FinalPrice = finalPrice,
                DiscountAmount = discount
            };
        }

        public async Task<ResponseCreateVoucher> CreateAsync(CreateVoucherDto dto, Guid userId)
        {
            if ((dto.DiscountAmount <= 0) && (!dto.DiscountPercent.HasValue || dto.DiscountPercent <= 0))
            {
                return new ResponseCreateVoucher
                {   
                    StatusCode = 400,
                    Message = "Phải nhập số tiền giảm hoặc phần trăm giảm > 0."
                };
            }

            if (dto.DiscountAmount > 0 && dto.DiscountPercent.HasValue && dto.DiscountPercent > 0)
            {
                return new ResponseCreateVoucher
                {
                    StatusCode = 400,
                    Message = "Chỉ được chọn một trong hai: số tiền giảm hoặc phần trăm giảm."
                };
            }

            if (dto.StartDate >= dto.EndDate)
            {
                return new ResponseCreateVoucher
                {
                    StatusCode = 400,
                    Message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc."
                };
            }

            if (dto.Quantity <= 0 || dto.Quantity > 1000)
            {
                return new ResponseCreateVoucher
                {
                    StatusCode = 400,
                    Message = "Số lượng voucher phải từ 1 đến 1000."
                };
            }

            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Quantity = dto.Quantity,
                DiscountAmount = dto.DiscountAmount,
                DiscountPercent = dto.DiscountPercent,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            await _voucherRepository.AddAsync(voucher);

            // Tạo các mã voucher ngẫu nhiên
            var generatedCodes = new List<string>();
            for (int i = 0; i < dto.Quantity; i++)
            {
                string code;
                int attempts = 0;
                do
                {
                    code = GenerateVoucherCode(dto.Name);
                    attempts++;
                    if (attempts > 100) // Tránh vòng lặp vô hạn
                        throw new InvalidOperationException("Không thể tạo mã voucher duy nhất sau 100 lần thử.");
                } while (await _voucherCodeRepository.IsCodeExistsAsync(code));

                var voucherCode = new VoucherCode
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucher.Id,
                    Code = code,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId,
                    IsActive = true
                };

                await _voucherCodeRepository.AddAsync(voucherCode);
                generatedCodes.Add(code);
            }

            await _voucherRepository.SaveChangesAsync();
            await _voucherCodeRepository.SaveChangesAsync();

            return new ResponseCreateVoucher
            {
                VoucherId = voucher.Id,
                GeneratedCodes = generatedCodes,
                StatusCode = 200,
                Message = "Voucher và các mã voucher đã được tạo thành công.",
                success = true
            };
        }

        private string GenerateVoucherCode(string voucherName)
        {
            // Tạo prefix từ tên voucher (lấy 3-4 ký tự đầu, loại bỏ dấu và khoảng trắng)
            var prefix = new string(voucherName
                .Where(c => char.IsLetter(c))
                .Take(4)
                .ToArray())
                .ToUpper();

            if (string.IsNullOrEmpty(prefix))
                prefix = "VOUCHER";

            // Tạo suffix ngẫu nhiên
            var random = new Random();
            var suffix = random.Next(1000, 9999).ToString();

            // Tạo mã ngẫu nhiên
            var randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var randomPart = new string(Enumerable.Repeat(randomChars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{prefix}-{randomPart}-{suffix}";
        }
        public async Task<ResponseGetVouchersDto> GetAllVouchersAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // predicate mặc định: chưa xóa
            var predicate = PredicateBuilder.New<Voucher>(x => !x.IsDeleted);

            // lọc theo textSearch (tên voucher)
            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(x => x.Name.Contains(textSearch, StringComparison.OrdinalIgnoreCase));
            }

            // lọc theo status (IsActive)
            if (status.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == status.Value);
            }

            var vouchers = await _voucherRepository.GenericGetPaginationAsync(
                pageIndexValue,
                pageSizeValue,
                predicate,
                new[] { nameof(Voucher.VoucherCodes), $"{nameof(Voucher.VoucherCodes)}.{nameof(VoucherCode.UsedByUser)}" }
            );

            var totalVouchers = vouchers.Count();
            var totalPages = (int)Math.Ceiling((double)totalVouchers / pageSizeValue);

            return new ResponseGetVouchersDto
            {
                StatusCode = 200,
                Data = _mapper.Map<List<VoucherDto>>(vouchers),
                TotalRecord = totalVouchers,
                TotalPages = totalPages
            };
        }
        public async Task<ResponseGetVoucherDto> GetVoucherByIdAsync(Guid id)
        {
            var voucher = await _voucherRepository.GetFirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                new[] { nameof(Voucher.VoucherCodes), $"{nameof(Voucher.VoucherCodes)}.{nameof(VoucherCode.UsedByUser)}" }
            );

            if (voucher == null)
            {
                return new ResponseGetVoucherDto
                {
                    StatusCode = 404,
                    Message = "Không tìm thấy voucher."
                };
            }

            return new ResponseGetVoucherDto
            {
                StatusCode = 200,
                Data = _mapper.Map<VoucherDto>(voucher)
            };
        }
        public async Task<BaseResposeDto> UpdateVoucherAsync(Guid id, UpdateVoucherDto dto, Guid userId)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            
            if (dto.DiscountAmount.HasValue && dto.DiscountAmount <= 0 && (!dto.DiscountPercent.HasValue || dto.DiscountPercent <= 0))
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Phải nhập số tiền giảm hoặc phần trăm giảm > 0."
                };
            }

            if (dto.DiscountAmount.HasValue && dto.DiscountAmount > 0 && dto.DiscountPercent.HasValue && dto.DiscountPercent > 0)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Chỉ được chọn một trong hai: số tiền giảm hoặc phần trăm giảm."
                };
            }

            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate >= dto.EndDate)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc."
                };
            }

            if (voucher == null || voucher.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Không tìm thấy voucher."
                };
            }

            voucher.Name = dto.Name ?? voucher.Name;
            voucher.DiscountAmount = dto.DiscountAmount ?? voucher.DiscountAmount;
            voucher.DiscountPercent = dto.DiscountPercent ?? voucher.DiscountPercent;
            voucher.StartDate = dto.StartDate ?? voucher.StartDate;
            voucher.EndDate = dto.EndDate ?? voucher.EndDate;
            voucher.IsActive = dto.IsActive ?? voucher.IsActive;
            voucher.UpdatedAt = DateTime.UtcNow;
            voucher.UpdatedById = userId;

            // Không cho phép thay đổi quantity sau khi tạo voucher
            // Nếu cần thay đổi số lượng, phải tạo voucher mới

            await _voucherRepository.UpdateAsync(voucher);
            await _voucherRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Cập nhật voucher thành công.",
                success = true
            };
        }
        public async Task<BaseResposeDto> DeleteVoucherAsync(Guid id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);

            if (voucher == null || voucher.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Không tìm thấy voucher."
                };
            }

            voucher.IsDeleted = true;
            voucher.UpdatedAt = DateTime.UtcNow;

            await _voucherRepository.UpdateAsync(voucher);
            await _voucherRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Xóa voucher thành công.",
                success = true
            };
        }
        public async Task<ResponseGetOrdersDto> GetAllOrdersAsync(int? pageIndex, int? pageSize, string? payOsOrderCode, bool? status)
        {
            try
            {
                var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
                var pageSizeValue = pageSize ?? Constants.PageSizeDefault;           

                var predicate = PredicateBuilder.New<Order>(x => !x.IsDeleted);

                // lọc theo status (IsActive)
                if (status.HasValue)
                {
                    predicate = predicate.And(x => x.IsActive == status.Value);
                }
                if (!string.IsNullOrEmpty(payOsOrderCode))
                {
                    predicate = predicate.And(x => x.PayOsOrderCode == payOsOrderCode);
                }

                // lấy danh sách + phân trang
                var orders = await _orderRepository.GenericGetPaginationAsync(
                    pageIndexValue,
                    pageSizeValue,
                    predicate,
                    new[] {
                nameof(Order.OrderDetails),
                $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}"
                    }
                );

                var totalOrders = orders.Count();
                var totalPages = (int)Math.Ceiling((double)totalOrders / pageSizeValue);

                // Debug: Log thông tin orders trước khi mapping
                Console.WriteLine($"Found {totalOrders} orders to map");
                
                if (orders.Any())
                {
                    var firstOrder = orders.First();
                    Console.WriteLine($"First order - Id: {firstOrder.Id}, PayOsOrderCode: {firstOrder.PayOsOrderCode}, OrderDetails count: {firstOrder.OrderDetails?.Count}");
                }

                return new ResponseGetOrdersDto
                {
                    StatusCode = 200,
                    Data = _mapper.Map<List<OrderDto>>(orders),
                    TotalRecord = totalOrders,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllOrdersAsync error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw để controller có thể handle
            }
        }

        public async Task<ResponseGetOrdersDto> GetOrdersByUserAsync(int? pageIndex, int? pageSize, string? payOsOrderCode, bool? status, CurrentUserObject currentUserObject)
        {
            try
            {
                var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
                var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

                var predicate = PredicateBuilder.New<Order>(x => !x.IsDeleted && x.CreatedById == currentUserObject.Id);

                // lọc theo status (IsActive)
                if (status.HasValue)
                {
                    predicate = predicate.And(x => x.Status == OrderStatus.Paid || x.Status == OrderStatus.Cancelled);

                }
                if (!string.IsNullOrEmpty(payOsOrderCode))
                {
                    predicate = predicate.And(x => x.PayOsOrderCode == payOsOrderCode);
                }

                // lấy danh sách + phân trang
                var orders = await _orderRepository.GenericGetPaginationAsync(
                    pageIndexValue,
                    pageSizeValue,
                    predicate,
                    new[] {
                nameof(Order.OrderDetails),
                $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}"
                    }
                );

                var totalOrders = orders.Count();
                var totalPages = (int)Math.Ceiling((double)totalOrders / pageSizeValue);

                // Debug: Log thông tin orders trước khi mapping
                Console.WriteLine($"Found {totalOrders} orders to map");

                if (orders.Any())
                {
                    var firstOrder = orders.First();
                    Console.WriteLine($"First order - Id: {firstOrder.Id}, PayOsOrderCode: {firstOrder.PayOsOrderCode}, OrderDetails count: {firstOrder.OrderDetails?.Count}");
                }

                return new ResponseGetOrdersDto
                {
                    StatusCode = 200,
                    Data = _mapper.Map<List<OrderDto>>(orders),
                    TotalRecord = totalOrders,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllOrdersAsync error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw để controller có thể handle
            }
        }
        public async Task<ResponseGetOrdersDto> GetOrdersByCurrentShopAsync(int? pageIndex,int? pageSize,string? payOsOrderCode,bool? status,CurrentUserObject currentUserObject)
        {
            try
            {
                var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
                var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

                var shopId = currentUserObject.Id;

                var predicate = PredicateBuilder.New<Order>(x => !x.IsDeleted);

                // chỉ lấy các Order có ít nhất 1 sản phẩm thuộc shop này
                predicate = predicate.And(x => x.OrderDetails.Any(od => od.Product.ShopId == shopId));

                if (status.HasValue)
                {
                    predicate = predicate.And(x => x.IsActive == status.Value);
                }

                if (!string.IsNullOrEmpty(payOsOrderCode))
                {
                    predicate = predicate.And(x => x.PayOsOrderCode == payOsOrderCode);
                }

                var orders = await _orderRepository.GenericGetPaginationAsync(
                    pageIndexValue,
                    pageSizeValue,
                    predicate,
                    new[]
                    {
                nameof(Order.OrderDetails),
                $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}"
                    }
                );

                var totalOrders = orders.Count();
                var totalPages = (int)Math.Ceiling((double)totalOrders / pageSizeValue);

                Console.WriteLine($"Found {totalOrders} orders to map");

                if (orders.Any())
                {
                    var firstOrder = orders.First();
                    Console.WriteLine($"First order - Id: {firstOrder.Id}, PayOsOrderCode: {firstOrder.PayOsOrderCode}, OrderDetails count: {firstOrder.OrderDetails?.Count}");
                }

                return new ResponseGetOrdersDto
                {
                    StatusCode = 200,
                    Data = _mapper.Map<List<OrderDto>>(orders),
                    TotalRecord = totalOrders,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetOrdersByCurrentShopAsync error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ResponseGetAvailableVoucherCodesDto> GetAvailableVoucherCodesAsync(int? pageIndex, int? pageSize)
        {
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Lấy tất cả voucher codes chưa được claim từ voucher đang hoạt động và chưa hết hạn
            var availableCodes = await _voucherCodeRepository.GetAvailableToClaimAsync(pageIndexValue, pageSizeValue);

            var now = DateTime.UtcNow;
            var allAvailableCodes = await _voucherCodeRepository.GetAllAsync(vc => 
                !vc.IsDeleted &&
                !vc.IsClaimed &&
                vc.Voucher.IsActive &&
                vc.Voucher.StartDate <= now &&
                vc.Voucher.EndDate >= now);

            var totalCodes = allAvailableCodes.Count();
            var totalPages = (int)Math.Ceiling((double)totalCodes / pageSizeValue);

            var result = _mapper.Map<List<AvailableVoucherCodeDto>>(availableCodes);

            return new ResponseGetAvailableVoucherCodesDto
            {
                StatusCode = 200,
                Message = "Lấy danh sách mã voucher khả dụng thành công.",
                success = true,
                Data = result,
                TotalRecord = totalCodes,
                TotalPages = totalPages
            };
        }

        public async Task<ResponseClaimVoucherDto> ClaimVoucherCodeAsync(Guid voucherCodeId, CurrentUserObject currentUser)
        {
            // Tìm voucher code có thể claim
            var voucherCode = await _voucherCodeRepository.GetAvailableCodeByIdAsync(voucherCodeId);

            if (voucherCode == null)
            {
                return new ResponseClaimVoucherDto
                {
                    StatusCode = 404,
                    Message = "Mã voucher không tồn tại hoặc đã được nhận bởi người khác.",
                    success = false
                };
            }

            // Kiểm tra user đã claim voucher từ cùng voucher template chưa
            var existingClaim = await _voucherCodeRepository.GetFirstOrDefaultAsync(vc =>
                vc.VoucherId == voucherCode.VoucherId &&
                vc.ClaimedByUserId == currentUser.Id &&
                !vc.IsDeleted);

            if (existingClaim != null)
            {
                return new ResponseClaimVoucherDto
                {
                    StatusCode = 400,
                    Message = "Bạn đã nhận một mã voucher từ chương trình này rồi.",
                    success = false
                };
            }

            // Claim voucher
            voucherCode.IsClaimed = true;
            voucherCode.ClaimedByUserId = currentUser.Id;
            voucherCode.ClaimedAt = DateTime.UtcNow;
            voucherCode.UpdatedAt = DateTime.UtcNow;
            voucherCode.UpdatedById = currentUser.Id;

            await _voucherCodeRepository.UpdateAsync(voucherCode);
            await _voucherCodeRepository.SaveChangesAsync();

            var myVoucher = _mapper.Map<MyVoucherDto>(voucherCode);

            return new ResponseClaimVoucherDto
            {
                StatusCode = 200,
                Message = "Nhận mã voucher thành công!",
                success = true,
                VoucherCode = myVoucher
            };
        }

        public async Task<ResponseGetMyVouchersDto> GetMyVouchersAsync(CurrentUserObject currentUser, int? pageIndex, int? pageSize, string? status, string? textSearch)
        {
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Lấy TẤT CẢ voucher codes của user trước (không pagination)
            var allUserVouchers = await _voucherCodeRepository.GetAllAsync(vc => 
                !vc.IsDeleted && 
                vc.IsClaimed && 
                vc.ClaimedByUserId == currentUser.Id,
                new[] { nameof(VoucherCode.Voucher) });

            // Apply textSearch filter trước
            if (!string.IsNullOrEmpty(textSearch))
            {
                allUserVouchers = allUserVouchers.Where(vc => 
                    vc.Voucher.Name.Contains(textSearch, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                var nowForFilter = DateTime.UtcNow;
                allUserVouchers = status.ToLower() switch
                {
                    "active" => allUserVouchers.Where(vc => !vc.IsUsed && vc.Voucher.EndDate >= nowForFilter).ToList(),
                    "used" => allUserVouchers.Where(vc => vc.IsUsed).ToList(),
                    "expired" => allUserVouchers.Where(vc => !vc.IsUsed && vc.Voucher.EndDate < nowForFilter).ToList(),
                    _ => allUserVouchers
                };
            }

            // Tính toán thống kê từ tất cả vouchers sau khi filter
            var nowForStats = DateTime.UtcNow;
            var activeCount = allUserVouchers.Count(vc => !vc.IsUsed && vc.Voucher.EndDate >= nowForStats);
            var usedCount = allUserVouchers.Count(vc => vc.IsUsed);
            var expiredCount = allUserVouchers.Count(vc => !vc.IsUsed && vc.Voucher.EndDate < nowForStats);

            var totalUserVouchers = allUserVouchers.Count();
            var totalPages = (int)Math.Ceiling((double)totalUserVouchers / pageSizeValue);

            // Apply pagination SAU KHI đã filter
            var pagedVouchers = allUserVouchers
                .OrderByDescending(vc => vc.ClaimedAt)
                .Skip((pageIndexValue - 1) * pageSizeValue)
                .Take(pageSizeValue)
                .ToList();

            var myVouchers = _mapper.Map<List<MyVoucherDto>>(pagedVouchers);

            return new ResponseGetMyVouchersDto
            {
                StatusCode = 200,
                Message = "Lấy danh sách voucher của bạn thành công.",
                success = true,
                Data = myVouchers,
                TotalRecord = totalUserVouchers,
                TotalPages = totalPages,
                ActiveCount = activeCount,
                UsedCount = usedCount,
                ExpiredCount = expiredCount
            };
        }
        public async Task<ApplyVoucherResult> ApplyMyVoucherForCartAsync(Guid voucherCodeId, List<CartItemDto> cartItems, CurrentUserObject currentUser)
        {
            if (!cartItems.Any())
                return new ApplyVoucherResult
                {
                    StatusCode = 400,
                    Message = "Giỏ hàng không có sản phẩm nào để áp dụng voucher.",
                    success = false
                };

            // Tìm voucher code của user
            var voucherCode = await _voucherCodeRepository.GetUserVoucherCodeAsync(voucherCodeId, currentUser.Id);

            if (voucherCode == null)
            {
                return new ApplyVoucherResult
                {
                    StatusCode = 404,
                    Message = "Không tìm thấy mã voucher trong kho voucher của bạn.",
                    success = false
                };
            }

            if (voucherCode.IsUsed)
            {
                return new ApplyVoucherResult
                {
                    StatusCode = 400,
                    Message = "Mã voucher này đã được sử dụng.",
                    success = false
                };
            }

            var now = DateTime.UtcNow;
            var voucher = voucherCode.Voucher;

            if (!voucher.IsActive || voucher.StartDate > now || voucher.EndDate < now)
            {
                return new ApplyVoucherResult
                {
                    StatusCode = 400,
                    Message = "Voucher không hợp lệ hoặc đã hết hạn.",
                    success = false
                };
            }

            decimal totalOriginal = 0m;

            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null) continue;
                
                // Nếu sản phẩm đang giảm giá thì không áp dụng voucher
                if (product.IsSale)
                {
                    return new ApplyVoucherResult
                    {
                        StatusCode = 400,
                        Message = $"Sản phẩm \"{product.Name}\" đang được giảm giá, không thể áp dụng voucher.",
                        success = false
                    };
                }
                totalOriginal += product.Price * item.Quantity;
            }

            if (totalOriginal <= 0)
            {
                return new ApplyVoucherResult
                {
                    StatusCode = 400,
                    Message = "Tổng tiền giỏ hàng không hợp lệ.",
                    success = false
                };
            }

            decimal discount = 0m;

            if (voucher.DiscountAmount > 0)
                discount = voucher.DiscountAmount;
            else if (voucher.DiscountPercent.HasValue)
                discount = totalOriginal * voucher.DiscountPercent.Value / 100m;

            if (discount > totalOriginal)
                discount = totalOriginal;

            var finalPrice = totalOriginal - discount;

            // PayOS yêu cầu >=1
            if (finalPrice < 1m)
                finalPrice = 1m;

            return new ApplyVoucherResult
            {
                StatusCode = 200,
                Message = "Áp dụng voucher thành công.",
                success = true,
                FinalPrice = finalPrice,
                DiscountAmount = discount
            };
        }
    }

}

