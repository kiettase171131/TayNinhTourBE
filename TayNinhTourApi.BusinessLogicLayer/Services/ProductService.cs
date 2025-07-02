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
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

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
        public ProductService(IProductRepository productRepository, IMapper mapper, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor, IProductImageRepository productImageRepository, ICartRepository cartRepository,IPayOsService payOsService, IOrderRepository orderRepository,IProductReviewRepository productReview,IProductRatingRepository productRating)
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
        }
        public async Task<ResponseGetProductsDto> GetProductsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var include = new string[] { nameof(Product.ProductImages)}; 

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
                Message = "Product deleted succcessfully !"
            };
        }
        public async Task<ResponseCreateProductDto> CreateProductAsync(RequestCreateProductDto request, CurrentUserObject currentUserObject)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                QuantityInStock = request.QuantityInStock,
                Category = request.Category,
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
            product.Price = request.Price ?? product.Price;
            product.QuantityInStock = request.QuantityInStock ?? product.QuantityInStock;
            product.Category = request.Category ?? product.Category;
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
                Message = "Product update successful"
            };
        }
        public async Task<BaseResposeDto> AddToCartAsync(RequestAddToCartDto request, CurrentUserObject currentUser)
        {
            // 1. Kiểm tra sản phẩm tồn tại và còn hoạt động
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null || product.IsDeleted || !product.IsActive)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Sản phẩm không tồn tại"
                };
            }

            // 2. Kiểm tra số lượng yêu cầu
            if (request.Quantity <= 0)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Số lượng không hợp lệ"
                };
            }

            // 3. Lấy cart item hiện có (nếu có)
            var existingCart = await _cartRepository.GetFirstOrDefaultAsync(x =>
                x.UserId == currentUser.Id && x.ProductId == request.ProductId);

            var totalQuantityRequested = request.Quantity;
            if (existingCart != null)
                totalQuantityRequested += existingCart.Quantity;

            // 4. So sánh với tồn kho
            if (totalQuantityRequested > product.QuantityInStock)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = $"Chỉ còn {product.QuantityInStock} sản phẩm trong kho"
                };
            }

            // 5. Cập nhật cart
            if (existingCart != null)
            {
                existingCart.Quantity += request.Quantity;
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
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUser.Id
                };
                await _cartRepository.AddAsync(newCart);
            }

            await _cartRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Đã thêm vào giỏ hàng"
            };
        }
        public async Task<ResponseGetCartDto> GetCartAsync(CurrentUserObject currentUser)
        {
            var include = new string[] { nameof(CartItem.Product), $"{nameof(CartItem.Product)}.{nameof(Product.ProductImages)}" };

            var cartItems = await _cartRepository.GetAllAsync(x => x.UserId == currentUser.Id, include);

            var items = cartItems.Select(x => new CartItemDto
            {
                CartItemId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Quantity = x.Quantity,
                Price = x.Product.Price,
                Total = x.Quantity * x.Product.Price,
                ImageUrl = x.Product.ProductImages.FirstOrDefault()?.Url
            }).ToList();

            return new ResponseGetCartDto
            {
                StatusCode = 200,
                Data = items,
                TotalAmount = items.Sum(i => i.Total)
            };
        }
        public async Task<BaseResposeDto> RemoveFromCartAsync(Guid cartItemId, CurrentUserObject currentUser)
        {
            var cartItem = await _cartRepository.GetFirstOrDefaultAsync(x => x.Id == cartItemId && x.UserId == currentUser.Id);

            if (cartItem == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Không tìm thấy sản phẩm trong giỏ"
                };
            }

            await _cartRepository.DeleteAsync(cartItem.Id);
            await _cartRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Đã xoá khỏi giỏ hàng"
            };
        }
        //public async Task ClearCartAndMarkOrderAsPaidAsync(Guid orderId)
        //{
        //    var order = await _orderRepository.GetByIdAsync(orderId, new[] { nameof(Order.OrderDetails) });

        //    if (order == null || order.Status == "Paid")
        //        return;

        //    // ✅ 1. Đánh dấu đơn hàng đã thanh toán
        //    order.Status = "Paid";
        //    order.UpdatedAt = DateTime.UtcNow;
        //    await _orderRepository.UpdateAsync(order);
        //    await _orderRepository.SaveChangesAsync();

        //    // ✅ 2. Giảm tồn kho sản phẩm
        //    foreach (var detail in order.OrderDetails)
        //    {
        //        var product = await _productRepository.GetByIdAsync(detail.ProductId);
        //        if (product != null)
        //        {
        //            product.QuantityInStock -= detail.Quantity;
        //            if (product.QuantityInStock < 0) product.QuantityInStock = 0;

        //            await _productRepository.UpdateAsync(product);
        //        }
        //    }
        //    await _productRepository.SaveChangesAsync();

        //    // ✅ 3. Xóa giỏ hàng của user
        //    var cartItems = await _cartRepository.GetAllAsync(x => x.UserId == order.UserId);
        //    _cartRepository.DeleteRange(cartItems);
        //    await _cartRepository.SaveChangesAsync();
        //}
        public async Task<CheckoutResultDto?> CheckoutCartAsync(List<Guid> cartItemIds, CurrentUserObject currentUser)
        {
            var include = new[] { nameof(CartItem.Product) };

            var cartItems = await _cartRepository.GetAllAsync(
                x => cartItemIds.Contains(x.Id) && x.UserId == currentUser.Id && !x.IsDeleted,
                include
            );

      

            cartItems = cartItems
                .Where(x => x.Product != null && !x.Product.IsDeleted && x.Product.IsActive)
                .ToList();

            if (!cartItems.Any())
                return null;

            // Kiểm tra tồn kho
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.QuantityInStock)
                    throw new InvalidOperationException($"Sản phẩm '{item.Product.Name}' chỉ còn {item.Product.QuantityInStock} trong kho.");
            }

            var total = cartItems.Sum(x => x.Product.Price * x.Quantity);

            var order = new Order
            {
                UserId = currentUser.Id,
                TotalAmount = total,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderDetails = cartItems.Select(x => new OrderDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.Product.Price
                }).ToList()
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            var checkoutUrl = await _payOsService.CreatePaymentUrlAsync(
                total,
                order.Id.ToString(),
                "https://tndt.netlify.app"
            );

            return new CheckoutResultDto
            {
                CheckoutUrl = checkoutUrl,
                OrderId = order.Id
            };
        }

        public async Task<OrderStatus> GetOrderPaymentStatusAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng");

            var status = await _payOsService.GetOrderPaymentStatusAsync(order.Id.ToString());

            // Nếu muốn cập nhật status trong DB thì xử lý tại service này (không ở controller)
            if (order.Status != status)
            {
                order.Status = status;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();
            }

            return status;
        }

        public async Task<BaseResposeDto> RateProductAsync(CreateProductRatingDto dto, Guid userId)
        {
            var existing = await _ratingRepo.GetFirstOrDefaultAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);
            if (existing != null)
            {
                existing.Rating = dto.Rating;
                await _ratingRepo.UpdateAsync(existing);
                await _ratingRepo.SaveChangesAsync();
            }
            else
            {
                var rating = new ProductRating
                {
                    ProductId = dto.ProductId,
                    UserId = userId,
                    Rating = dto.Rating
                };
                await _ratingRepo.AddAsync(rating);
                await _ratingRepo.SaveChangesAsync();
            }
            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Product rating updated successfully"
            };
        }

        public async Task<BaseResposeDto> ReviewProductAsync(CreateProductReviewDto dto, Guid userId)
        {
            var review = new ProductReview
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Content = dto.Content
            };
            await _reviewRepo.AddAsync(review);
            await _reviewRepo.SaveChangesAsync();
            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Product review added successfully"
            };
        }

        public async Task<double> GetAverageRatingAsync(Guid productId)
        {
            var ratings = await _ratingRepo.ListAsync(r => r.ProductId == productId);
            if (!ratings.Any()) return 0;
            return ratings.Average(r => r.Rating);
        }

        public async Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(Guid productId)
        {
            var includes = new[] { "User" };
            var reviews = await _reviewRepo.ListAsync(r => r.ProductId == productId,includes);

            return reviews.Select(r => new ProductReviewDto
            {
                UserName = r.User.Name,
                Content = r.Content,
                CreatedAt = r.CreatedAt
            });
        }
    }
}
