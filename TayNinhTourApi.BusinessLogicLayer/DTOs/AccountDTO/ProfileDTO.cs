using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO
{
    public class ProfileDTO
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Guid RoleId { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Avatar { get; set; } = null!;
    }
}
