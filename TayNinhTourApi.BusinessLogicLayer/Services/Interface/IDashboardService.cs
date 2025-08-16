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
        Task<AdminDashboardDto> GetDashboardAsync(int year, int month);
        Task<BloggerDashboardDto> GetBloggerStatsAsync(Guid bloggerId, int month, int year);
        Task<ShopDashboardDto> GetShopStatisticsAsync(Guid shopId, int year, int month);
        Task<List<TourDetailsStatisticDto>> GetTourDetailsStatisticsAsync();
        Task<TourCompanyStatisticDto> GetStatisticForCompanyAsync(Guid userId);
    }

}
