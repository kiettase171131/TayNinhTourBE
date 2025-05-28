using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Account
{
    public class ResponseGetProfileDto : BaseResposeDto
    {
        public ProfileDTO Data { get; set; } = null!;
    }
}
