using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment
{
    public class PayOSCallbackDto
    {
        public string orderCode { get; set; }
        public string status { get; set; }
    }
}
