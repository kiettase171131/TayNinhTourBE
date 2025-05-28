using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface ITourGuideApplicationRepository : IGenericRepository<TourGuideApplication>
    {
        Task<IEnumerable<TourGuideApplication>> ListByStatusAsync(ApplicationStatus status);
        Task<IEnumerable<TourGuideApplication>> ListByUserAsync(Guid userId);
        
    }
}
