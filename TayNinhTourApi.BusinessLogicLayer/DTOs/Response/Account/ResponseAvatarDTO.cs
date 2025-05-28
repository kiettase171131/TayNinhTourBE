using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Account
{
    public class ResponseAvatarDTO : BaseResposeDto
    {
        public string Data { get; set; } = null!;      
    }
}
