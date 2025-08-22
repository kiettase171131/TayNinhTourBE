using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms.TayNinhTourApi.Business.DTOs.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    public class ResponseGetTourCompanyByIdDto : BaseResposeDto
    {
        public TourCompanyCmsDto? Data { get; set; }
    }
}
