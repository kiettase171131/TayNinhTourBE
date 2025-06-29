using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment
{
    public class PayOsStatusResponseDto
    {
        public string status { get; set; } = null!;
        [FromQuery(Name = "code")]
        public string Code { get; set; }

        [FromQuery(Name = "id")]
        public string Id { get; set; }

        [FromQuery(Name = "cancel")]
        public bool Cancel { get; set; }

        [FromQuery(Name = "status")]
        public string Status { get; set; }

        [FromQuery(Name = "orderCode")]
        public long OrderCode { get; set; }

        public IQueryCollection RawQueryCollection { get; set; }
    }

}
