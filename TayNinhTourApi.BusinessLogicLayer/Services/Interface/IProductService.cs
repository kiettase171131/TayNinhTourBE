using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IProductService
    {
        Task<ResponseGetProductsDto> GetProductsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status);
        Task<ResponseGetProductsDto> GetProductsByShopAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status, CurrentUserObject currentUserObject);
        Task<ResponseGetProductByIdDto> GetProductByIdAsync(Guid id);
        Task<BaseResposeDto> DeleteProductAsync(Guid id);
        Task<ResponseCreateProductDto> CreateProductAsync(RequestCreateProductDto request, CurrentUserObject currentUserObject);
        Task<BaseResposeDto> UpdateProductAsync(RequestUpdateProductDto request, Guid id, CurrentUserObject currentUserObject);
        Task<BaseResposeDto> AddToCartAsync(RequestAddToCartDto request, CurrentUserObject currentUser);
        Task<ResponseGetCartDto> GetCartAsync(CurrentUserObject currentUser);
        Task<BaseResposeDto> RemoveFromCartAsync(Guid cartItemId, CurrentUserObject currentUser);
        Task ClearCartAndUpdateInventoryAsync(Guid orderId);
        Task<CheckoutResultDto?> CheckoutCartAsync(List<Guid> cartItemIds, CurrentUserObject currentUser);
        Task<OrderStatus> GetOrderPaymentStatusAsync(Guid orderId);
        Task<BaseResposeDto> RateProductAsync(CreateProductRatingDto dto, Guid userId);
        Task<BaseResposeDto> ReviewProductAsync(CreateProductReviewDto dto, Guid userId);
        Task<double> GetAverageRatingAsync(Guid productId);
        Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(Guid productId);
    }
}
