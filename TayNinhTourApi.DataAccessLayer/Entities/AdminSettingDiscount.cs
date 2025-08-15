using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class AdminSettingDiscount : BaseEntity
    {
        
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        
    }
}
