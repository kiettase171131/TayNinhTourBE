using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetDashboardAsync(int? year = null, int? month = null);
        Task<BloggerDashboardDto> GetBloggerStatsAsync(Guid bloggerId, int? month = null, int? year = null);
        Task<ShopDashboardDto> GetShopStatisticsAsync(Guid shopId, int? year = null, int? month = null);
        Task<List<TourDetailsStatisticDto>> GetTourDetailsStatisticsAsync();
        Task<TourCompanyStatisticDto> GetStatisticForCompanyAsync(Guid userId);
        
        /// <summary>
        /// Admin lấy thống kê thu nhập của tất cả tour companies
        /// </summary>
        /// <param name="year">Năm thống kê (optional, default sẽ được set ở service)</param>
        /// <param name="month">Tháng thống kê (optional, default sẽ được set ở service)</param>
        /// <param name="pageIndex">Trang hiện tại (0-based)</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên company hoặc email)</param>
        /// <param name="isActive">Lọc theo trạng thái active (null = tất cả)</param>
        /// <returns>Thống kê thu nhập của tour companies</returns>
        Task<AdminTourCompanyRevenueOverviewDto> GetTourCompanyRevenueStatsAsync(
            int? year = null, 
            int? month = null, 
            int pageIndex = 0, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool? isActive = null);

        /// <summary>
        /// Admin lấy thống kê chi tiết của một tour company cụ thể
        /// </summary>
        /// <param name="tourCompanyId">ID của tour company</param>
        /// <param name="year">Năm thống kê (optional, default sẽ được set ở service)</param>
        /// <param name="month">Tháng thống kê (optional, default sẽ được set ở service)</param>
        /// <returns>Thống kê chi tiết của tour company</returns>
        Task<TourCompanyRevenueStatsDto?> GetTourCompanyRevenueDetailAsync(
            Guid tourCompanyId, 
            int? year = null, 
            int? month = null);
    }
}
