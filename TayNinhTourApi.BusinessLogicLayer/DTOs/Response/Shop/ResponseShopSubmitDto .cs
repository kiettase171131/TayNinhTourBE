using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Shop
{
    public class ResponseShopSubmitDto : BaseResposeDto
    {
        public string ShopName { get; set; } = null!;
        public string UrlLogo { get; set; } = string.Empty;
        public string UrlBusinessLicense { get; set; } = string.Empty;
    }
}
