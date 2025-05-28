using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Account;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IAccountService
    {
        Task<dynamic> UpdateProfile(EditAccountProfileDTO editAccountProfileDTO, CurrentUserObject currentUserObject);
        Task<dynamic> GetProfile(CurrentUserObject currentUserObject);
        Task<dynamic> ChangePassword(PasswordDTO password,  CurrentUserObject currentUserObject);
        Task<ResponseAvatarDTO> UpdateAvatar(AvatarDTO avatarDTO, CurrentUserObject currentUserObject);
    }
}
