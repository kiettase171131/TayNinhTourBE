using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Shop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Shop;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using Microsoft.AspNetCore.Hosting;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Common;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class ShopApplicationService : IShopApplicationService
    {
        private readonly IShopApplicationRepository _shopApplicationRepository;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IHostingEnvironment _env;
        private readonly EmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ShopApplicationService(
           IShopApplicationRepository appRepo,
           IUserRepository userRepo,
           IRoleRepository roleRepo,
           IHostingEnvironment env, EmailSender emailSender, IHttpContextAccessor httpContextAccessor)
        {
            _shopApplicationRepository = appRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _env = env;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;

        }
        public async Task<BaseResposeDto> ApproveAsync(Guid applicationId)
        {
            var app = await _shopApplicationRepository.GetByIdAsync(applicationId);
            if (app.Status != ShopStatus.Pending)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Application is not pending, cannot approve !"
                };
            }
            var guiderole = await _roleRepo.GetRoleByNameAsync(Constants.RoleShopName);
            if (guiderole == null)
            {
                guiderole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = Constants.RoleShopName,
                    CreatedAt = DateTime.UtcNow
                };
                await _roleRepo.AddAsync(guiderole);
                await _roleRepo.SaveChangesAsync();
            }
            var user = await _userRepo.GetByIdAsync(app.UserId);
            if (user == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }
            user.RoleId = guiderole.Id;              // gán quan hệ FK thay vì user.Name
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedById = app.CreatedById;

            // update trạng thái hồ sơ
            app.Status = ShopStatus.Approved;
            app.UpdatedAt = DateTime.UtcNow;
            app.UpdatedById = app.CreatedById;

            await _shopApplicationRepository.SaveChangesAsync();
            // --- Gửi email về địa chỉ app.Email, không phải user.Email ---
            await _emailSender.SendShopApprovalNotificationAsync(
                app.Email,            // dùng email đã nhập trong SubmitApplicationDto
                user.Name         // giữ nguyên tên user
            );
            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Application approved successfully"
            };
        }
        public async Task<BaseResposeDto> RejectAsync(Guid applicationId, string reason)
        {
            var app = await _shopApplicationRepository.GetByIdAsync(applicationId);
            if (app.Status != ShopStatus.Pending)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Application is not pending, cannot reject !"
                };
            }
            app.Status = ShopStatus.Rejected;
            app.RejectionReason = reason;
            app.UpdatedAt = DateTime.UtcNow;
            app.UpdatedById = app.CreatedById;
            await _shopApplicationRepository.SaveChangesAsync();
            var user = await _userRepo.GetByIdAsync(app.UserId);
            if (user == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }
            await _emailSender.SendShopRejectionNotificationAsync(
                app.Email,
                user.Name,
                reason

            );
            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Application rejected successfully"
            };
        }
        public async Task<IEnumerable<ShopApplication>> GetPendingAsync()
        {
            return await _shopApplicationRepository.ListByStatusAsync(ShopStatus.Pending);
        }
        public async Task<IEnumerable<ShopApplication>> ListByUserAsync(Guid userId)
        {
            return await _shopApplicationRepository.ListByUserAsync(userId);
        }
        public async Task<ResponseShopSubmitDto> SubmitAsync(RequestShopSubmitDto requestShopSubmit, CurrentUserObject currentUserObject)
        {

            var existing = await _shopApplicationRepository.ListByUserAsync(currentUserObject.Id);
            if (existing.Any(e =>
            e.Status == ShopStatus.Pending ||
            e.Status == ShopStatus.Approved))
            {
                return new ResponseShopSubmitDto
                {
                    StatusCode = 400,
                    Message = "You have already submitted an application."
                };
            }
            if (requestShopSubmit == null)
            {
                return new ResponseShopSubmitDto()
                {
                    StatusCode = 400,
                    Message = "No files were uploaded."
                };
            }

            var app = new ShopApplication
            {
                Id = Guid.NewGuid(),
                UserId = currentUserObject.Id,
                Email = requestShopSubmit.Email,
                Name = requestShopSubmit.ShopName,
                Description = requestShopSubmit.Description,
                Website = requestShopSubmit.Website,
                Location = requestShopSubmit.Location,
                RepresentativeName = requestShopSubmit.RepresentativeName,
                ShopType = requestShopSubmit.ShopType,
                Status = ShopStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedById = currentUserObject.Id
            };
            // Xử lý upload Business License
            if (requestShopSubmit.BusinessLicense != null && requestShopSubmit.BusinessLicense.Length > 0)
            {
                const long MaxFileSize = 5 * 1024 * 1024;
                if (requestShopSubmit.BusinessLicense.Length > MaxFileSize)
                {
                    return new ResponseShopSubmitDto
                    {
                        StatusCode = 400,
                        Message = $"File too large. Max size is {MaxFileSize / (1024 * 1024)} MB."
                    };
                }
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp", ".pdf", ".doc", ".docx" };
                var ext = Path.GetExtension(requestShopSubmit.BusinessLicense.FileName);
                if (!allowedExts.Contains(ext))
                {
                    return new ResponseShopSubmitDto
                    {
                        StatusCode = 400,
                        Message = "Invalid file type. Only .png, .jpg, .jpeg, .webp, .pdf, .doc, .docx are allowed."
                    };
                }
                // Thư mục wwwroot/uploads/cv
                var webRoot = _env.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    // fallback nếu WebRootPath chưa được thiết lập
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                // Tạo folder nếu chưa có
                var cvfolder = Path.Combine(webRoot, "uploads", "businesslicense");
                if (!Directory.Exists(cvfolder))
                    Directory.CreateDirectory(cvfolder);


                // Tạo tên file duy nhất
                //var ext = Path.GetExtension(submitApplicationDto.CurriculumVitae.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(cvfolder, fileName);

                // Lưu file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await requestShopSubmit.BusinessLicense.CopyToAsync(stream);
                }

                // Lưu đường dẫn tương đối vào DB (để frontend có thể truy cập)
                app.BusinessLicenseUrl = Path.Combine("uploads", "businesslicense", fileName).Replace("\\", "/");
            }
            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fullUrl = $"{baseUrl}/{app.BusinessLicenseUrl}";
            app.BusinessLicenseUrl = fullUrl; // Lưu URL đầy đủ vào DB
            // Xử lý upload Logo
            if (requestShopSubmit.Logo != null && requestShopSubmit.Logo.Length > 0)
            {
                const long MaxFileSize = 5 * 1024 * 1024;
                if (requestShopSubmit.Logo.Length > MaxFileSize)
                {
                    return new ResponseShopSubmitDto
                    {
                        StatusCode = 400,
                        Message = $"File too large. Max size is {MaxFileSize / (1024 * 1024)} MB."
                    };
                }
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var ext = Path.GetExtension(requestShopSubmit.Logo.FileName);
                if (!allowedExts.Contains(ext))
                {
                    return new ResponseShopSubmitDto
                    {
                        StatusCode = 400,
                        Message = "Invalid file type. Only .png, .jpg, .jpeg, .webp are allowed."
                    };
                }
                // Thư mục wwwroot/uploads/cv
                var webRoot = _env.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    // fallback nếu WebRootPath chưa được thiết lập
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                // Tạo folder nếu chưa có
                var cvfolder = Path.Combine(webRoot, "uploads", "shoplogo");
                if (!Directory.Exists(cvfolder))
                    Directory.CreateDirectory(cvfolder);


                // Tạo tên file duy nhất
                //var ext = Path.GetExtension(submitApplicationDto.CurriculumVitae.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(cvfolder, fileName);

                // Lưu file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await requestShopSubmit.Logo.CopyToAsync(stream);
                }

                // Lưu đường dẫn tương đối vào DB (để frontend có thể truy cập)
                app.LogoUrl = Path.Combine("uploads", "shoplogo", fileName).Replace("\\", "/");
            }
            var request2 = _httpContextAccessor.HttpContext!.Request;
            var baseUrl2 = $"{request2.Scheme}://{request2.Host}";
            var fullUrl2 = $"{baseUrl2}/{app.LogoUrl}";
            app.LogoUrl = fullUrl2; // Lưu URL đầy đủ vào DB

            await _shopApplicationRepository.AddAsync(app);
            await _shopApplicationRepository.SaveChangesAsync();

            return new ResponseShopSubmitDto
            {
                StatusCode = 200,
                Message = "Application sent successfully",
                ShopName = app.Name,
                UrlBusinessLicense = fullUrl,
                UrlLogo = fullUrl2
            };
        }
    }
}
