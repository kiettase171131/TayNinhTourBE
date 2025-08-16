using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    public class TourCompanyStatisticDto
    {
        public int ConfirmedBookings { get; set; }
        public decimal RevenueHold { get; set; }
        public decimal Wallet { get; set; }
    }


}
