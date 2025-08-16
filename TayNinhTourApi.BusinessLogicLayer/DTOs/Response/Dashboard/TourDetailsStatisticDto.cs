using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    public class TourDetailsStatisticDto
    {
        public string StatusGroup { get; set; } = string.Empty;
        public int Count { get; set; }
    }

}
