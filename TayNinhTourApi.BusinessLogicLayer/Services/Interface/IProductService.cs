using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IProductService
    {
        Task<ResponseGetProductsDto> GetProductsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status);
        Task<ResponseGetProductByIdDto> GetProductByIdAsync(Guid id);
        Task<BaseResposeDto> DeleteProductAsync(Guid id);
        Task<ResponseCreateProductDto> CreateProductAsync(RequestCreateProductDto request, CurrentUserObject currentUserObject);
        Task<BaseResposeDto> UpdateProductAsync(RequestUpdateProductDto request, Guid id, CurrentUserObject currentUserObject);
    }
}
