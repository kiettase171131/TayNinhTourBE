using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Account;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Application;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Image;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Repositories;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;



namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly BcryptUtility _bcryptUtility;
        private readonly IHostingEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;



        public AccountService(BcryptUtility bcryptUtility, IUserRepository userRepository, IMapper mapper, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _bcryptUtility = bcryptUtility;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<dynamic> ChangePassword(PasswordDTO password, CurrentUserObject currentUserObject)
        {
            var account = await _userRepository.GetByIdAsync(currentUserObject.Id);
            if (account == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            if (_bcryptUtility.VerifyPassword(password.OldPassword, account.PasswordHash))
            {
                account.PasswordHash = _bcryptUtility.HashPassword(password.NewPassword);
                await _userRepository.UpdateAsync(account);
                await _userRepository.SaveChangesAsync();
                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Change Password successfully"
                };
            }
            else
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Old password is incorrect"
                };
            }
        }

        public async Task<dynamic> GetProfile(CurrentUserObject currentUserObject)
        {
            var account = await _userRepository.GetByIdAsync(currentUserObject.Id);
            ProfileDTO profile = new ProfileDTO()
            {
                Email = account.Email,
                Name = account.Name,
                RoleId = account.RoleId,
                Avatar = account.Avatar,
                PhoneNumber = account.PhoneNumber,
            };
            return new ResponseGetProfileDto
            {
                StatusCode = 200,
                Data = profile,
            };
        }

        public async Task<ResponseAvatarDTO> UpdateAvatar(AvatarDTO avatarDTO, CurrentUserObject currentUserObject)
        {

            var user = await _userRepository.GetByIdAsync(currentUserObject.Id);
            if (user == null)
            {
                return new ResponseAvatarDTO()
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }
            if (avatarDTO.Avatar == null || avatarDTO.Avatar.Length == 0)
            {
                return new ResponseAvatarDTO
                {
                    StatusCode = 400,
                    Message = "No file was uploaded."
                };
            }
            if (avatarDTO.Avatar != null && avatarDTO.Avatar.Length > 0)
            {
                const long MaxFileSize = 5 * 1024 * 1024;
                if (avatarDTO.Avatar.Length > MaxFileSize)
                {
                    return new ResponseAvatarDTO
                    {
                        StatusCode = 400,
                        Message = $"File too large. Max size is {MaxFileSize / (1024 * 1024)} MB."
                    };
                }
                // 1. Kiểm tra định dạng file
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var ext = Path.GetExtension(avatarDTO.Avatar.FileName).ToLowerInvariant();
                if (!allowedExts.Contains(ext))
                {
                    return new ResponseAvatarDTO
                    {
                        StatusCode = 400,
                        Message = "Invalid file type. Only .png, .jpg, .jpeg, .webp are allowed."
                    };
                }
                var webRoot = _env.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    // fallback nếu WebRootPath chưa được thiết lập
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                // Tạo folder nếu chưa có
                var avatarsFolder = Path.Combine(webRoot, "images", "avatars");
                if (!Directory.Exists(avatarsFolder))
                    Directory.CreateDirectory(avatarsFolder);

                // Đổi tên file để tránh trùng

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(avatarsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await avatarDTO.Avatar.CopyToAsync(stream);

                // Cập nhật đường dẫn lưu trong db (tương đối so với wwwroot)
                user.Avatar = Path.Combine("images", "avatars", fileName).Replace("\\", "/");

            }
            // Lấy Request từ HttpContext
            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fullAvatarUrl = $"{baseUrl}/{user.Avatar}";
            user.Avatar = fullAvatarUrl;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return new ResponseAvatarDTO
            {
                StatusCode = 200,
                Message = "Update Avatar successfully",
                Data = fullAvatarUrl
            };



        }

        public async Task<dynamic> UpdateProfile(EditAccountProfileDTO editAccountProfileDTO, CurrentUserObject currentUserObject)
        {
            //var account = await _userRepository.GetByIdAsync(currentUserObject.Id);
            //account.Name = editAccountProfileDTO.Name;
            //account.PhoneNumber = editAccountProfileDTO.PhoneNumber;
            //account.Avatar = editAccountProfileDTO.Avatar;
            //var update = await _userRepository.UpdateAsync(account);
            //await _userRepository.SaveChangesAsync();
            //return new BaseResposeDto
            //{
            //    StatusCode = 200,
            //    Message = "Update Profile successfully"
            //};

            var user = await _userRepository.GetByIdAsync(currentUserObject.Id);
            if (user == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }


            user.Name = editAccountProfileDTO.Name;
            user.PhoneNumber = editAccountProfileDTO.PhoneNumber;

            //// Xử lý upload avatar
            //if (editAccountProfileDTO.Avatar != null && editAccountProfileDTO.Avatar.Length > 0)
            //{
            //    var webRoot = _env.WebRootPath;
            //    if (string.IsNullOrEmpty(webRoot))
            //    {
            //        // fallback nếu WebRootPath chưa được thiết lập
            //        webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            //    }
            //    // Tạo folder nếu chưa có
            //    var avatarsFolder = Path.Combine(webRoot, "images", "avatars");
            //    if (!Directory.Exists(avatarsFolder))
            //        Directory.CreateDirectory(avatarsFolder);

            //    // Đổi tên file để tránh trùng
            //    var ext = Path.GetExtension(editAccountProfileDTO.Avatar.FileName);
            //    var fileName = $"{Guid.NewGuid()}{ext}";
            //    var filePath = Path.Combine(avatarsFolder, fileName);

            //    using var stream = new FileStream(filePath, FileMode.Create);
            //    await editAccountProfileDTO.Avatar.CopyToAsync(stream);

            //    // Cập nhật đường dẫn lưu trong db (tương đối so với wwwroot)
            //    user.Avatar = Path.Combine("images", "avatars", fileName).Replace("\\", "/");
            //}

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Update Profile successfully"
            };
        }

    }
}




