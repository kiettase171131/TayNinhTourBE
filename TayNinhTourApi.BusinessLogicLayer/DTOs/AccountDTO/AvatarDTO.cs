using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO
{
    public class AvatarDTO
    {
        public IFormFile? Avatar { get; set; }
    }
}
