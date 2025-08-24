using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TayNinhTourApi.BusinessLogicLayer.Common.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RoleNameEnum
    {
        [EnumMember(Value = "Admin")]
        Admin,

        [EnumMember(Value = "Tour Company")]
        TourCompany,

        [EnumMember(Value = "User")]
        User,

        [EnumMember(Value = "Tour Guide")]
        TourGuide,

        [EnumMember(Value = "Blogger")]
        Blogger,

        [EnumMember(Value = "Specialty Shop")]
        SpecialtyShop
    }
}
