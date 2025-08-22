using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Notification;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Voucher;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
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
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ISpecialtyShopRepository _specialtyShop;
        private readonly ISpecialtyShopService _specialtyShopService;
        private readonly IAdminSettingDiscountRepository _adminsetting;

        public ProductService(IProductRepository productRepository, IMapper mapper, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor, IProductImageRepository productImageRepository, ICartRepository cartRepository, IPayOsService payOsService, IOrderRepository orderRepository, IProductReviewRepository productReview, IProductRatingRepository productRating, IVoucherRepository voucherRepository, IVoucherCodeRepository voucherCodeRepository, IOrderDetailRepository orderDetailRepository, INotificationService notificationService, IUserRepository userRepository, ISpecialtyShopRepository specialtyShop, ISpecialtyShopService specialtyShopService, IAdminSettingDiscountRepository adminsetting)
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
            _notificationService = notificationService;
            _userRepository = userRepository;
            _specialtyShop = specialtyShop;
            _specialtyShopService = specialtyShopService;
            _adminsetting = adminsetting;
        }
        public async Task<ResponseGetProductsDto> GetProductsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status, string? sortBySoldCount)
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
            // Dùng IQueryable để sort, phân trang động
            var query = _productRepository.GetQueryable()
                .Where(predicate);

            // Bao gồm các navigation property
            foreach (var inc in include)
                query = query.Include(inc);

            // Sắp xếp theo SoldCount tăng/giảm nếu có yêu cầu
            if (!string.IsNullOrEmpty(sortBySoldCount))
            {
                if (sortBySoldCount.ToLower() == "asc")
                    query = query.OrderBy(x => x.SoldCount);
                else if (sortBySoldCount.ToLower() == "desc")
                    query = query.OrderByDescending(x => x.SoldCount);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt); // default
            }

            var totalProducts = await query.CountAsync();
            var products = await query.Skip((pageIndexValue - 1) * pageSizeValue).Take(pageSizeValue).ToListAsync();
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
        public async Task<ResponseGetProductsDto> GetProductsByShopAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status, string? sortBySoldCount, CurrentUserObject currentUserObject)
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
            // Dùng IQueryable để sort, phân trang động
            var query = _productRepository.GetQueryable()
                .Where(predicate);

            // Bao gồm các navigation property
            foreach (var inc in include)
                query = query.Include(inc);

            // Sắp xếp theo SoldCount tăng/giảm nếu có yêu cầu
            if (!string.IsNullOrEmpty(sortBySoldCount))
            {
                if (sortBySoldCount.ToLower() == "asc")
                    query = query.OrderBy(x => x.SoldCount);
                else if (sortBySoldCount.ToLower() == "desc")
                    query = query.OrderByDescending(x => x.SoldCount);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt); // default
            }

            var totalProducts = await query.CountAsync();
            var products = await query.Skip((pageIndexValue - 1) * pageSizeValue).Take(pageSizeValue).ToListAsync();
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
            var shop = await _specialtyShop.GetIdByUserIdAsync(currentUserObject.Id);

            if (shop == null)
            {
                return new ResponseCreateProductDto
                {
                    StatusCode = 400,
                    Message = "User hiện tại chưa có SpecialtyShop.",
                    success = false
                };
            }
                
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
                SpecialtyShopId = shop.Value,
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

                    await _productImageRepository.DeleteAsync(oldImage.Id); // _repo2 là ProductImageRepo
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

                var order = await _orderRepository.GetByIdAsync(orderId, new[] { nameof(Order.OrderDetails), nameof(Order.Voucher) });

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

                // ✅ 1. Cập nhật voucher usage count (nếu có voucher được sử dụng)
                if (order.VoucherId.HasValue && order.Voucher != null)
                {
                    Console.WriteLine($"Processing voucher: {order.Voucher.Name}");

                    // Tăng số lượng đã sử dụng
                    order.Voucher.UsedCount += 1;
                    order.Voucher.UpdatedAt = DateTime.UtcNow;

                    await _voucherRepository.UpdateAsync(order.Voucher);
                    await _voucherRepository.SaveChangesAsync();

                    Console.WriteLine($"Voucher usage count updated: {order.Voucher.UsedCount}/{order.Voucher.Quantity}");
                }
                else
                {
                    Console.WriteLine("No voucher used in this order");
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
                        product.SoldCount += detail.Quantity; // Cập nhật số lượng đã bán
                        if (product.QuantityInStock < 0) product.QuantityInStock = 0;

                        Console.WriteLine($"Product {detail.ProductId}: {oldQuantity} -> {product.QuantityInStock}, SoldCount: {product.SoldCount}");
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

            public async Task<CheckoutResultDto ?> CheckoutCartAsync(List<Guid> cartItemIds, CurrentUserObject currentUser, Guid? voucherId = null)
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
            decimal taxAmount = cartItems.Sum(x => x.Product.Price * x.Quantity * 0.10m); // ✅ 10% thuế từ giá gốc
            decimal discountAmount = 0m;
                decimal totalAfterDiscount = total;
                Guid? finalVoucherId = null;

               
                // Gom theo shop để gọi eligibility 1 lần / shop
                var promotionMessages = new List<string>();
            // ✅ Lấy phần trăm giảm do admin cấu hình từ SystemSettings
            var tourDiscountPercent = await _adminsetting.GetTourDiscountPercentAsync();
            // Gom theo shop để xét ưu đãi
            var productsByShop = cartItems
                    .Where(ci => ci.Product != null)
                    .GroupBy(ci => ci.Product!.SpecialtyShopId)
                    .ToList();

                foreach (var grp in productsByShop)
                {
                    var shopId = grp.Key;
                    if (shopId == Guid.Empty) continue;

                    // Lấy tên shop để in thông báo
                    var shopName = await _specialtyShop
                        .GetQueryable()
                        .Where(s => s.Id == shopId)
                        .Select(s => s.ShopName)
                        .FirstOrDefaultAsync() ?? "Shop";

                    // ❗ Luật mới: chỉ cần user có tour sắp tới ghé shop này là đủ
                    var (eligible, nextDate, nextTime, _, activity, tourName) =
                        await _specialtyShopService.CheckUpcomingVisitForShopAsync(shopId, currentUser.Id);

                if (!eligible || tourDiscountPercent <= 0) continue;

                var shopSubtotal = grp.Sum(ci => ci.Product!.Price * ci.Quantity);
                var shopDiscount = Math.Round(shopSubtotal * (tourDiscountPercent / 100m), 2);

                discountAmount += shopDiscount;
                    totalAfterDiscount -= shopDiscount;

                    // Thông báo
                    var dateText = nextDate.HasValue ? nextDate.Value.ToString("dd/MM/yyyy") : "sắp tới";
                    var timeText = nextTime.HasValue ? nextTime.Value.ToString(@"hh\:mm") : "";
                    var timePart = string.IsNullOrWhiteSpace(timeText) ? "" : $" lúc {timeText}";
                    var activityPart = string.IsNullOrWhiteSpace(activity) ? "" : $" (mốc: {activity})";
                    var tourText = string.IsNullOrWhiteSpace(tourName) ? "tour sắp tới" : $"tour {tourName}";

                    promotionMessages.Add(
                        $"🎉 Chúc mừng! Bạn được giảm {tourDiscountPercent}% vì đã mua hàng tại **{shopName}**, nơi bạn sẽ ghé trong {tourText} vào {dateText}{timePart}{activityPart}."
                    );
                }
            

            // Áp dụng voucher nếu được chọn
            // ===== Voucher: CỘNG THÊM, tính trên totalAfterDiscount hiện tại =====
            if (voucherId.HasValue)
            {
                var voucher = await _voucherRepository.GetByIdAsync(voucherId.Value)
                              ?? throw new InvalidOperationException("Voucher không tồn tại hoặc không khả dụng.");
                if (!voucher.IsAvailable)
                    throw new InvalidOperationException("Voucher không khả dụng.");

                // Không cho áp voucher nếu có item đang sale (như bạn đang làm)
                foreach (var item in cartItems)
                    if (item.Product.IsSale)
                        throw new InvalidOperationException($"Sản phẩm \"{item.Product.Name}\" đang giảm giá, không thể áp dụng voucher.");

                // 🔑 TÍNH TRÊN totalAfterDiscount (đÃ trừ 10% shop nếu có)
                decimal voucherDiscount = 0m;
                if (voucher.DiscountAmount > 0)
                    voucherDiscount = voucher.DiscountAmount;
                else if (voucher.DiscountPercent.HasValue)
                    voucherDiscount = Math.Round(totalAfterDiscount * voucher.DiscountPercent.Value / 100m, 2);

                // Không vượt quá phần còn lại
                if (voucherDiscount > totalAfterDiscount)
                    voucherDiscount = totalAfterDiscount;

                // ✅ CỘNG THÊM, KHÔNG GHI ĐÈ
                discountAmount += voucherDiscount;
                totalAfterDiscount -= voucherDiscount;
                finalVoucherId = voucherId;
            }

            if (totalAfterDiscount <= 0)
                    throw new InvalidOperationException("Tổng tiền thanh toán không hợp lệ sau khi áp dụng voucher.");
            // ✅/ Tổng thanh toán cuối cùng sau giảm + thuế
            //var totalPayable = totalAfterDiscount + taxAmount;

            // Tạo PayOsOrderCode với format TNDT + 10 số sử dụng utility
            var payOsOrderCodeString = PayOsOrderCodeUtility.GeneratePayOsOrderCode();

                // Lưu vào DB dưới dạng string thay vì long
                var order = new Order
                {
                    UserId = currentUser.Id,
                    TotalAmount = total,
                    DiscountAmount = discountAmount,
                    TotalAfterDiscount = totalAfterDiscount,
                    TaxAmount = taxAmount,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUser.Id,
                    VoucherId = finalVoucherId,
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

                // === ENHANCED PAYOS: Use new PaymentTransaction system ===
                var paymentRequest = new TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment.CreatePaymentRequestDto
                {
                    OrderId = order.Id, // Link to Order (Product Payment)
                    TourBookingId = null, // NULL for product payments
                    Amount = totalAfterDiscount,
                    Description = $"Product Order {payOsOrderCodeString}"
                };

                var paymentTransaction = await _payOsService.CreatePaymentLinkAsync(paymentRequest);

                return new CheckoutResultDto    
                {
                    CheckoutUrl = paymentTransaction.CheckoutUrl ?? "",
                    OrderId = order.Id,
                    TotalOriginal = total,
                    DiscountAmount = discountAmount,
                    TaxAmount = taxAmount,
                    TotalAfterDiscount = totalAfterDiscount,
                    PromotionMessages = promotionMessages
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
            // 1. Kiểm tra user có mua sản phẩm trong đơn hàng này và đã nhận chưa
            var hasCheckedOrder = await _orderDetailRepository.AnyAsync(od =>
                od.ProductId == dto.ProductId &&
                od.OrderId == dto.OrderId &&
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

            // 2. Chặn feedback nếu đã feedback sản phẩm này trong đơn này
            var hasFeedback = await _ratingRepo.AnyAsync(r =>
                r.ProductId == dto.ProductId &&
                r.OrderId == dto.OrderId &&
                r.UserId == userId);

            if (hasFeedback)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Bạn đã đánh giá sản phẩm này trong đơn hàng này rồi."
                };
            }

            // 3. Ghi rating (1 lần / đơn)
            var rating = new ProductRating
            {
                ProductId = dto.ProductId,
                OrderId = dto.OrderId,
                UserId = userId,
                Rating = dto.Rating,
                CreatedAt = DateTime.UtcNow
            };
            await _ratingRepo.AddAsync(rating);

            // 4. Ghi review (1 lần / đơn)
            var review = new ProductReview
            {
                ProductId = dto.ProductId,
                OrderId = dto.OrderId,  
                UserId = userId,
                Content = dto.Review,
                CreatedAt = DateTime.UtcNow
            };
            await _reviewRepo.AddAsync(review);

            // 5. Save changes
            await _ratingRepo.SaveChangesAsync();
            await _reviewRepo.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Gửi đánh giá thành công."
            };
        }



        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId)
        {
            var includes = new[] { "User" };

            // 1) Lấy tất cả ratings của sản phẩm (để tính average + map sang review)
            var ratings = await _ratingRepo.ListAsync(r => r.ProductId == productId);

            // 2) Lấy tất cả reviews (kèm User)
            var reviews = await _reviewRepo.ListAsync(r => r.ProductId == productId, includes);

            // 3) Tính average rating (trên toàn bộ records rating)
            var averageRating = ratings.Any()
                ? Math.Round(ratings.Average(r => r.Rating), 1)
                : 0;

            // 4) Tạo từ điển: key = (OrderId, UserId, ProductId) -> Rating
            //    Đảm bảo mỗi đơn hàng/mỗi lần rating map đúng về review tương ứng
            var ratingByOUP = ratings
                .GroupBy(x => (x.OrderId, x.UserId, x.ProductId))
                .ToDictionary(g => g.Key, g => g.First().Rating);
            // Nếu chắc chắn 1 rating/đơn thì First() là đủ.
            // Nếu có nhiều record/đơn, đổi thành g.OrderBy(x => x.CreatedAt).First()/Last() tùy quy ước.

            // 5) Map review -> DTO, lấy đúng rating theo (OrderId, UserId, ProductId)
            var reviewDtos = reviews.Select(r =>
            {
                var key = (r.OrderId, r.UserId, r.ProductId);
                var hasRating = ratingByOUP.TryGetValue(key, out var ratingValue);

                return new ProductReviewDto
                {
                    UserName = r.User.Name,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    Rating = hasRating ? ratingValue : 0
                };
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

            // Sử dụng Vietnam timezone
            var now = VietnamTimeZoneUtility.GetVietnamNow();

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
            // Sử dụng Vietnam timezone để validate
            var now = VietnamTimeZoneUtility.GetVietnamNow();

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

            // ✅ Validation mới: StartDate phải lớn hơn hiện tại
            if (dto.StartDate <= now)
            {
                return new ResponseCreateVoucher
                {
                    StatusCode = 400,
                    Message = $"Ngày bắt đầu phải lớn hơn thời điểm hiện tại. Hiện tại: {now:dd/MM/yyyy HH:mm} (GMT+7)"
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

            if (dto.Quantity <= 0 || dto.Quantity > 10000)
            {
                return new ResponseCreateVoucher
                {
                    StatusCode = 400,
                    Message = "Số lượng voucher phải từ 1 đến 10,000."
                };
            }

            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Quantity = dto.Quantity,
                UsedCount = 0,
                DiscountAmount = dto.DiscountAmount,
                DiscountPercent = dto.DiscountPercent,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true,
                CreatedAt = now, // Sử dụng Vietnam timezone
                CreatedById = userId
            };

            await _voucherRepository.AddAsync(voucher);
            await _voucherRepository.SaveChangesAsync();

            // Gửi thông báo đến tất cả users
            await SendVoucherNotificationToAllUsersAsync(voucher);

            return new ResponseCreateVoucher
            {
                VoucherId = voucher.Id,
                VoucherName = voucher.Name,
                Quantity = voucher.Quantity,
                StatusCode = 200,
                Message = "Voucher đã được tạo thành công và thông báo đã được gửi đến tất cả người dùng.",
                success = true
            };
        }

        public async Task<ResponseGetVouchersDto> GetAllVouchersAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // predicate mặc định: chưa xóa
            var predicate = PredicateBuilder.New<Voucher>(x => !x.IsDeleted);

            // lọc theo textSearch (tên voucher) - Fix: Use EF.Functions.Like instead of Contains with StringComparison
            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(x => EF.Functions.Like(x.Name, $"%{textSearch}%"));
            }

            // lọc theo status (IsActive)
            if (status.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == status.Value);
            }
            var totalVouchers = await _voucherRepository.CountAsync(predicate);
            var vouchers = await _voucherRepository.GenericGetPaginationAsync(
                pageIndexValue,
                pageSizeValue,
                predicate,
                null // No includes needed for simplified voucher system
            );

            
            var totalPages = (int)Math.Ceiling((double)totalVouchers / pageSizeValue);

            return new ResponseGetVouchersDto
            {
                StatusCode = 200,
                Message = "Lấy danh sách voucher thành công.",
                success = true,
                Data = _mapper.Map<List<VoucherDto>>(vouchers),
                TotalRecord = totalVouchers,
                TotalPages = totalPages
            };
        }

        public async Task<ResponseGetVoucherDto> GetVoucherByIdAsync(Guid id)
        {
            var voucher = await _voucherRepository.GetFirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                null
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
                Message = "Lấy thông tin voucher thành công.",
                success = true,
                Data = _mapper.Map<VoucherDto>(voucher)
            };
        }

        public async Task<BaseResposeDto> UpdateVoucherAsync(Guid id, UpdateVoucherDto dto, Guid userId)
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

            // Sử dụng Vietnam timezone để validate
            var now = VietnamTimeZoneUtility.GetVietnamNow();

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

            // ✅ Validation mới: Nếu update StartDate, phải lớn hơn hiện tại
            if (dto.StartDate.HasValue && dto.StartDate.Value <= now)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = $"Ngày bắt đầu phải lớn hơn thời điểm hiện tại. Hiện tại: {now:dd/MM/yyyy HH:mm} (GMT+7)"
                };
            }

            // Validation cho trường hợp update cả StartDate và EndDate
            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate >= dto.EndDate)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc."
                };
            }

            // Validation cho trường hợp update chỉ StartDate (so với EndDate hiện tại)
            if (dto.StartDate.HasValue && !dto.EndDate.HasValue && dto.StartDate >= voucher.EndDate)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Ngày bắt đầu mới phải nhỏ hơn ngày kết thúc hiện tại."
                };
            }

            // Validation cho trường hợp update chỉ EndDate (so với StartDate hiện tại)
            if (!dto.StartDate.HasValue && dto.EndDate.HasValue && voucher.StartDate >= dto.EndDate)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Ngày kết thúc mới phải lớn hơn ngày bắt đầu hiện tại."
                };
            }

            var wasInactive = !voucher.IsActive;

            voucher.Name = dto.Name ?? voucher.Name;
            voucher.Description = dto.Description ?? voucher.Description;
            voucher.Quantity = dto.Quantity ?? voucher.Quantity;
            voucher.DiscountAmount = dto.DiscountAmount ?? voucher.DiscountAmount;
            voucher.DiscountPercent = dto.DiscountPercent ?? voucher.DiscountPercent;
            voucher.StartDate = dto.StartDate ?? voucher.StartDate;
            voucher.EndDate = dto.EndDate ?? voucher.EndDate;
            voucher.IsActive = dto.IsActive ?? voucher.IsActive;
            voucher.UpdatedAt = now; // Sử dụng Vietnam timezone
            voucher.UpdatedById = userId;

            await _voucherRepository.UpdateAsync(voucher);
            await _voucherRepository.SaveChangesAsync();

            // Nếu voucher được kích hoạt lại (từ inactive thành active), gửi thông báo
            if (wasInactive && voucher.IsActive)
            {
                await SendVoucherNotificationToAllUsersAsync(voucher, isUpdate: true);
            }

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
            voucher.UpdatedAt = VietnamTimeZoneUtility.GetVietnamNow(); // Sử dụng Vietnam timezone

            await _voucherRepository.UpdateAsync(voucher);
            await _voucherRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Xóa voucher thành công.",
                success = true
            };
        }

        public async Task<ResponseGetAvailableVouchersDto> GetAvailableVouchersAsync(int? pageIndex, int? pageSize)
        {
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Sử dụng Vietnam timezone
            var now = VietnamTimeZoneUtility.GetVietnamNow();
            Console.WriteLine($"[DEBUG] GetAvailableVouchersAsync called. Current Vietnam time: {now}");
            Console.WriteLine($"[DEBUG] Pagination: pageIndex={pageIndexValue}, pageSize={pageSizeValue}");

            // Step 1: Get all vouchers first for debugging
            var allVouchersInDb = await _voucherRepository.GetAllAsync(x => !x.IsDeleted);
            Console.WriteLine($"[DEBUG] Total vouchers in database (not deleted): {allVouchersInDb.Count()}");

            if (!allVouchersInDb.Any())
            {
                Console.WriteLine("[DEBUG] No vouchers found in database!");
                return new ResponseGetAvailableVouchersDto
                {
                    StatusCode = 200,
                    Message = "Không có voucher nào trong hệ thống.",
                    success = true,
                    Data = new List<AvailableVoucherDto>(),
                    TotalRecord = 0,
                    TotalPages = 0
                };
            }

            foreach (var voucher in allVouchersInDb.Take(5)) // Log first 5 vouchers
            {
                Console.WriteLine($"[DEBUG] Voucher: {voucher.Name}");
                Console.WriteLine($"  - ID: {voucher.Id}");
                Console.WriteLine($"  - IsActive: {voucher.IsActive}");
                Console.WriteLine($"  - IsDeleted: {voucher.IsDeleted}");
                Console.WriteLine($"  - StartDate: {voucher.StartDate}");
                Console.WriteLine($"  - EndDate: {voucher.EndDate}");
                Console.WriteLine($"  - Quantity: {voucher.Quantity}");
                Console.WriteLine($"  - UsedCount: {voucher.UsedCount}");
                Console.WriteLine($"  - Remaining: {voucher.Quantity - voucher.UsedCount}");
                Console.WriteLine($"  - Start check: {voucher.StartDate <= now} ({voucher.StartDate} <= {now})");
                Console.WriteLine($"  - End check: {voucher.EndDate >= now} ({voucher.EndDate} >= {now})");
                Console.WriteLine("---");
            }

            // Step 2: Filter step by step
            var activeVouchers = allVouchersInDb.Where(x => x.IsActive).ToList();
            Console.WriteLine($"[DEBUG] Active vouchers: {activeVouchers.Count}");

            var dateValidVouchers = activeVouchers.Where(x => x.StartDate <= now && x.EndDate >= now).ToList();
            Console.WriteLine($"[DEBUG] Date valid vouchers: {dateValidVouchers.Count}");

            var availableVouchers = dateValidVouchers.Where(x => (x.Quantity - x.UsedCount) > 0).ToList();
            Console.WriteLine($"[DEBUG] Final available vouchers: {availableVouchers.Count}");

            // Step 3: Apply pagination
            var totalVouchers = availableVouchers.Count;
            var totalPages = (int)Math.Ceiling((double)totalVouchers / pageSizeValue);

            Console.WriteLine($"[DEBUG] Before pagination: Total={totalVouchers}, Pages={totalPages}");

            var paginatedVouchers = availableVouchers
                .Skip((pageIndexValue - 1) * pageSizeValue)
                .Take(pageSizeValue)
                .ToList();

            Console.WriteLine($"[DEBUG] After pagination: Count={paginatedVouchers.Count}");

            // Step 4: Manual mapping instead of AutoMapper for debugging
            var result = new List<AvailableVoucherDto>();

            foreach (var voucher in paginatedVouchers)
            {
                try
                {
                    var dto = new AvailableVoucherDto
                    {
                        Id = voucher.Id,
                        Name = voucher.Name,
                        Description = voucher.Description,
                        DiscountAmount = voucher.DiscountAmount,
                        DiscountPercent = voucher.DiscountPercent,
                        RemainingCount = voucher.Quantity - voucher.UsedCount,
                        StartDate = voucher.StartDate,
                        EndDate = voucher.EndDate
                    };
                    result.Add(dto);
                    Console.WriteLine($"[DEBUG] Mapped voucher: {dto.Name}, Remaining: {dto.RemainingCount}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Error mapping voucher {voucher.Name}: {ex.Message}");
                }
            }

            Console.WriteLine($"[DEBUG] Final mapped result count: {result.Count}");

            return new ResponseGetAvailableVouchersDto
            {
                StatusCode = 200,
                Message = "Lấy danh sách voucher khả dụng thành công.",
                success = true,
                Data = result,
                TotalRecord = totalVouchers,
                TotalPages = totalPages
            };
        }

        public async Task<ResponseGetOrdersDto> GetAllOrdersAsync(int? pageIndex, int? pageSize, string? payOsOrderCode, bool? status, bool? isChecked, OrderStatus? orderStatus)
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
                if (isChecked.HasValue)
                {
                    predicate = predicate.And(x => x.IsChecked == isChecked.Value);
                }
                if (orderStatus.HasValue)
                {
                    predicate = predicate.And(x => x.Status == orderStatus.Value);
                }
                var totalOrders = await _orderRepository.CountAsync(predicate);
                // lấy danh sách + phân trang
                var orders = await _orderRepository.GenericGetPaginationAsync(
                    pageIndexValue,
                    pageSizeValue,
                    predicate,
                    new[] {
                        nameof(Order.OrderDetails),
                        $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}",
                        $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}.{nameof(Product.SpecialtyShop)}",
                        nameof(Order.User) // <-- thêm dòng này!
                    }
                );

                
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
                    success = true,
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

        public async Task<ResponseGetOrdersDto> GetOrdersByUserAsync(int? pageIndex, int? pageSize, string? payOsOrderCode, bool? status, bool? isChecked, OrderStatus? orderStatus, CurrentUserObject currentUserObject)
        {
            try
            {
                var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
                var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

                var predicate = PredicateBuilder.New<Order>(x => !x.IsDeleted && x.UserId == currentUserObject.Id);

                // lọc theo status (IsActive)
                if (status.HasValue)
                {
                    predicate = predicate.And(x => x.Status == OrderStatus.Paid || x.Status == OrderStatus.Cancelled);
                }
                if (!string.IsNullOrEmpty(payOsOrderCode))
                {
                    predicate = predicate.And(x => x.PayOsOrderCode == payOsOrderCode);
                }
                if (isChecked.HasValue)
                {
                    predicate = predicate.And(x => x.IsChecked == isChecked.Value);
                }
                if (orderStatus.HasValue)
                {
                    predicate = predicate.And(x => x.Status == orderStatus.Value);
                }
                var totalOrders = await _orderRepository.CountAsync(predicate);
                // lấy danh sách + phân trang
                var orders = await _orderRepository.GenericGetPaginationAsync(
                    pageIndexValue,
                    pageSizeValue,
                    predicate,
                    new[] {
                nameof(Order.OrderDetails),
                $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}",
                $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}.{nameof(Product.SpecialtyShop)}"
                    }
                );

               
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
                    success = true,
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

        public async Task<ResponseGetOrdersDto> GetOrdersByCurrentShopAsync(int? pageIndex, int? pageSize, string? payOsOrderCode, bool? status, bool? isChecked, OrderStatus? orderStatus, CurrentUserObject currentUserObject)
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
                if (isChecked.HasValue)
                {
                    predicate = predicate.And(x => x.IsChecked == isChecked.Value);
                }
                if (orderStatus.HasValue)
                {
                    predicate = predicate.And(x => x.Status == orderStatus.Value);
                }
                var totalOrders = await _orderRepository.CountAsync(predicate);
                var orders = await _orderRepository.GenericGetPaginationAsync(
                    pageIndexValue,
                    pageSizeValue,
                    predicate,
                    new[]
                    {
                nameof(Order.OrderDetails),
                $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Product)}",nameof(Order.User)
                    }
                );

                
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
                    success = true,
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

        private async Task SendVoucherNotificationToAllUsersAsync(Voucher voucher, bool isUpdate = false)
        {
            try
            {
                // Lấy chỉ users có role "User" thôi
                var users = await _userRepository.GetAllAsync(u => u.IsActive && !u.IsDeleted && u.Role.Name == "User");

                var title = isUpdate ? "🎁 Voucher được cập nhật!" : "🎁 Voucher mới từ TNDT!";
                var message = isUpdate
                    ? $"Voucher \"{voucher.Name}\" đã được cập nhật. Vui lòng kiểm tra danh sách voucher để không bỏ lỡ những ưu đãi hấp dẫn!"
                    : $"Bạn nhận được voucher \"{voucher.Name}\" từ TNDT! Vui lòng kiểm tra trong danh sách voucher thường xuyên để đừng bỏ lỡ những bất ngờ.";

                foreach (var user in users)
                {
                    var createNotificationDto = new CreateNotificationDto
                    {
                        UserId = user.Id,
                        Title = title,
                        Message = message,
                        Type = NotificationType.Promotion,
                        Priority = NotificationPriority.Normal,
                        ActionUrl = "/vouchers",
                        Icon = "gift",
                        AdditionalData = System.Text.Json.JsonSerializer.Serialize(new { VoucherId = voucher.Id, VoucherName = voucher.Name })
                    };

                    await _notificationService.CreateNotificationAsync(createNotificationDto);
                }

                Console.WriteLine($"Sent voucher notification to {users.Count()} users (role: User only) for voucher: {voucher.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending voucher notifications: {ex.Message}");
                // Log error nhưng không throw để không ảnh hưởng đến việc tạo voucher
            }
        }
    }

}

