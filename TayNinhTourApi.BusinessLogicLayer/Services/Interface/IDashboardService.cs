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
    }

}
