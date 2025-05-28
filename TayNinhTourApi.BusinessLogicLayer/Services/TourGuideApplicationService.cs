using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Account;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Application;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class TourGuideApplicationService : ITourGuideApplicationService
    {
        private readonly ITourGuideApplicationRepository _appRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IHostingEnvironment _env;
        private readonly EmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;


        public TourGuideApplicationService(
            ITourGuideApplicationRepository appRepo,
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IHostingEnvironment env, EmailSender emailSender, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _appRepo = appRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _env = env;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<BaseResposeDto> ApproveAsync(Guid applicationId)
        {
            var app = await _appRepo.GetByIdAsync(applicationId);
            if (app.Status != ApplicationStatus.Pending)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Application is not pending, cannot approve !"
                };
            }


            // chuyển user thành tourguide
            var guiderole = await _roleRepo.GetRoleByNameAsync(Constants.RoleTourGuideName);
            if (guiderole == null)
            {
                guiderole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = Constants.RoleTourGuideName,
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
            app.Status = ApplicationStatus.Approved;
            app.UpdatedAt = DateTime.UtcNow;
            app.UpdatedById = app.CreatedById;

            await _appRepo.SaveChangesAsync();
            // --- Gửi email về địa chỉ app.Email, không phải user.Email ---
            await _emailSender.SendApprovalNotificationAsync(
                app.Email,            // dùng email đã nhập trong SubmitApplicationDto
                user.Name         // giữ nguyên tên user
            );
            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Application approved successfully"
            };
        }

        public async Task<IEnumerable<TourGuideApplication>> GetPendingAsync()
        {
            return await _appRepo.ListByStatusAsync(ApplicationStatus.Pending);
        }

        public async Task<IEnumerable<TourGuideApplication>> ListByUserAsync(Guid userId)
        {
            return await _appRepo.ListByUserAsync(userId);
        }

        public async Task<BaseResposeDto> RejectAsync(Guid applicationId, string reason)
        {
            var app = await _appRepo.GetByIdAsync(applicationId);
            if (app.Status != ApplicationStatus.Pending)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Application is not pending, cannot reject !"
                };
            }
            app.Status = ApplicationStatus.Rejected;
            app.RejectionReason = reason;
            app.UpdatedAt = DateTime.UtcNow;
            app.UpdatedById = app.CreatedById;
            await _appRepo.SaveChangesAsync();
            var user = await _userRepo.GetByIdAsync(app.UserId);
            if (user == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }         
            await _emailSender.SendRejectionNotificationAsync(
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

        public async Task<ResponseApplicationDto> SubmitAsync(SubmitApplicationDto submitApplicationDto, CurrentUserObject currentUserObject)
        {
            
            var existing = await _appRepo.ListByUserAsync(currentUserObject.Id);
            if (existing.Any(e =>
            e.Status == ApplicationStatus.Pending ||
            e.Status == ApplicationStatus.Approved))
            {
                return new ResponseApplicationDto
                {
                    StatusCode = 400,
                    Message = "You have already submitted an application."
                };
            }
            if (submitApplicationDto == null)
            {
                return new ResponseApplicationDto()
                {
                    StatusCode = 400,
                    Message = "No files were uploaded."
                };
            }

            var app = new TourGuideApplication
            {
                Id = Guid.NewGuid(),
                UserId = currentUserObject.Id,
                Email = submitApplicationDto.Email,
                Status = ApplicationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedById = currentUserObject.Id
            };
            // Xử lý upload CV
            if (submitApplicationDto.CurriculumVitae != null && submitApplicationDto.CurriculumVitae.Length > 0)
            {
                const long MaxFileSize = 5 * 1024 * 1024;
                if (submitApplicationDto.CurriculumVitae.Length > MaxFileSize)
                {
                    return new ResponseApplicationDto
                    {
                        StatusCode = 400,
                        Message = $"File too large. Max size is {MaxFileSize / (1024 * 1024)} MB."
                    };
                }
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp", ".pdf", ".doc", ".docx" };
                var ext = Path.GetExtension(submitApplicationDto.CurriculumVitae.FileName);
                if (!allowedExts.Contains(ext))
                {
                    return new ResponseApplicationDto
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
                var cvfolder = Path.Combine(webRoot, "uploads", "cv");
                if (!Directory.Exists(cvfolder))
                    Directory.CreateDirectory(cvfolder);
                

                // Tạo tên file duy nhất
                //var ext = Path.GetExtension(submitApplicationDto.CurriculumVitae.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(cvfolder, fileName);

                // Lưu file
                 using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await submitApplicationDto.CurriculumVitae.CopyToAsync(stream);
                }

                // Lưu đường dẫn tương đối vào DB (để frontend có thể truy cập)
                app.CurriculumVitae = Path.Combine("uploads", "cv", fileName).Replace("\\", "/");
            }
            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fullCVUrl = $"{baseUrl}/{app.CurriculumVitae}";
            app.CurriculumVitae = fullCVUrl; // Lưu URL đầy đủ vào DB
            await _appRepo.AddAsync(app);
            await _appRepo.SaveChangesAsync();
          
            return  new ResponseApplicationDto
            {
                StatusCode = 200,
                Message = "Application sent successfully",
                UrlCv = fullCVUrl,
            };
        }
    }
}
