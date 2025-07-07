using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho SpecialtyShopApplication business logic
    /// Thay thế cho ShopApplicationService với đầy đủ features theo thiết kế
    /// </summary>
    public class SpecialtyShopApplicationService : ISpecialtyShopApplicationService
    {
        private readonly ISpecialtyShopApplicationRepository _applicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly EmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SpecialtyShopApplicationService(
            ISpecialtyShopApplicationRepository applicationRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHostingEnvironment hostingEnvironment,
            EmailSender emailSender,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _applicationRepository = applicationRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _hostingEnvironment = hostingEnvironment;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// User nộp đơn đăng ký Specialty Shop
        /// </summary>
        public async Task<SpecialtyShopApplicationSubmitResponseDto> SubmitApplicationAsync(
            SubmitSpecialtyShopApplicationDto dto,
            CurrentUserObject currentUser)
        {
            // Kiểm tra user đã có đơn active chưa
            var hasActiveApplication = await _applicationRepository.HasActiveApplicationAsync(currentUser.Id);
            if (hasActiveApplication)
            {
                return new SpecialtyShopApplicationSubmitResponseDto
                {
                    StatusCode = 400,
                    Message = "You already have an active specialty shop application. Please wait for processing or contact support.",
                    success = false,
                    Instructions = "Please wait for your current application to be processed or contact support for assistance."
                };
            }

            // Note: File validation is now handled by DTO validation attributes
            // BusinessLicenseFile and Logo are required and validated at DTO level

            try
            {
                // Tạo application entity
                var application = new SpecialtyShopApplication
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUser.Id,
                    ShopName = dto.ShopName,
                    ShopDescription = dto.ShopDescription,
                    BusinessLicense = "PENDING", // Sẽ được cập nhật từ file name hoặc metadata
                    Location = dto.Location,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    Website = dto.Website,
                    ShopType = dto.ShopType,
                    OpeningHours = dto.OpeningHours,
                    ClosingHours = dto.ClosingHours,
                    RepresentativeName = dto.RepresentativeName,
                    Status = SpecialtyShopApplicationStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUser.Id
                };

                // Upload business license file
                var businessLicenseUrl = await UploadFileAsync(
                    dto.BusinessLicenseFile,
                    "businesslicense",
                    new[] { ".png", ".jpg", ".jpeg", ".webp", ".pdf", ".doc", ".docx" });

                if (businessLicenseUrl.StartsWith("Error:"))
                {
                    return new SpecialtyShopApplicationSubmitResponseDto
                    {
                        StatusCode = 400,
                        Message = businessLicenseUrl,
                        success = false,
                        Instructions = "Please check your business license file and try again. Make sure the file is in correct format and size."
                    };
                }
                application.BusinessLicenseUrl = businessLicenseUrl;

                // Cập nhật BusinessLicense từ file name (loại bỏ extension)
                application.BusinessLicense = Path.GetFileNameWithoutExtension(dto.BusinessLicenseFile.FileName);

                // Upload logo file
                var logoUrl = await UploadFileAsync(
                    dto.Logo,
                    "shoplogo",
                    new[] { ".png", ".jpg", ".jpeg", ".webp" });

                if (logoUrl.StartsWith("Error:"))
                {
                    return new SpecialtyShopApplicationSubmitResponseDto
                    {
                        StatusCode = 400,
                        Message = logoUrl,
                        success = false,
                        Instructions = "Please check your logo file and try again. Make sure the file is in correct format and size."
                    };
                }
                application.LogoUrl = logoUrl;

                // Lưu application
                await _applicationRepository.AddAsync(application);
                await _applicationRepository.SaveChangesAsync();

                // Send confirmation email
                await _emailSender.SendSpecialtyShopApplicationSubmittedAsync(
                    application.Email,
                    currentUser.Name,
                    application.ShopName);

                return new SpecialtyShopApplicationSubmitResponseDto
                {
                    StatusCode = 200,
                    Message = "Specialty shop application submitted successfully",
                    success = true,
                    ApplicationId = application.Id,
                    ShopName = application.ShopName,
                    LogoUrl = application.LogoUrl,
                    BusinessLicenseUrl = application.BusinessLicenseUrl,
                    SubmittedAt = application.SubmittedAt,
                    Instructions = "Your specialty shop application has been submitted successfully. We will review your application within 3-5 business days and notify you via email."
                };
            }
            catch (Exception ex)
            {
                return new SpecialtyShopApplicationSubmitResponseDto
                {
                    StatusCode = 500,
                    Message = $"An error occurred while submitting application: {ex.Message}",
                    success = false,
                    Instructions = "An unexpected error occurred. Please try again later or contact support if the problem persists."
                };
            }
        }

        /// <summary>
        /// User xem trạng thái đơn đăng ký của mình
        /// </summary>
        public async Task<SpecialtyShopApplicationDto?> GetMyApplicationAsync(CurrentUserObject currentUser)
        {
            var application = await _applicationRepository.GetLatestByUserIdAsync(currentUser.Id);
            if (application == null)
            {
                return null;
            }

            return _mapper.Map<SpecialtyShopApplicationDto>(application);
        }

        /// <summary>
        /// Admin lấy danh sách đơn đăng ký với pagination
        /// </summary>
        public async Task<(IEnumerable<SpecialtyShopApplicationSummaryDto> Applications, int TotalCount)> GetApplicationsAsync(
            int pageIndex = 0,
            int pageSize = 10,
            SpecialtyShopApplicationStatus? status = null,
            string? searchTerm = null)
        {
            var (applications, totalCount) = await _applicationRepository.GetPagedAsync(
                pageIndex, pageSize, status, searchTerm);

            var applicationDtos = _mapper.Map<IEnumerable<SpecialtyShopApplicationSummaryDto>>(applications);
            return (applicationDtos, totalCount);
        }

        /// <summary>
        /// Admin xem chi tiết đơn đăng ký
        /// </summary>
        public async Task<SpecialtyShopApplicationDto?> GetApplicationByIdAsync(Guid applicationId)
        {
            var application = await _applicationRepository.GetWithUserInfoAsync(applicationId);
            if (application == null)
            {
                return null;
            }

            return _mapper.Map<SpecialtyShopApplicationDto>(application);
        }

        /// <summary>
        /// Lấy danh sách đơn đăng ký pending
        /// </summary>
        public async Task<IEnumerable<SpecialtyShopApplicationSummaryDto>> GetPendingApplicationsAsync()
        {
            var applications = await _applicationRepository.GetByStatusAsync(SpecialtyShopApplicationStatus.Pending);
            return _mapper.Map<IEnumerable<SpecialtyShopApplicationSummaryDto>>(applications);
        }

        /// <summary>
        /// Đếm số đơn đăng ký theo status
        /// </summary>
        public async Task<int> CountApplicationsByStatusAsync(SpecialtyShopApplicationStatus status)
        {
            return await _applicationRepository.CountByStatusAsync(status);
        }

        /// <summary>
        /// Lấy thống kê đơn đăng ký theo khoảng thời gian
        /// </summary>
        public async Task<IEnumerable<SpecialtyShopApplicationSummaryDto>> GetApplicationsByDateRangeAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            var applications = await _applicationRepository.GetByDateRangeAsync(fromDate, toDate);
            return _mapper.Map<IEnumerable<SpecialtyShopApplicationSummaryDto>>(applications);
        }

        /// <summary>
        /// Kiểm tra user có thể nộp đơn mới không
        /// </summary>
        public async Task<bool> CanSubmitNewApplicationAsync(Guid userId)
        {
            return !await _applicationRepository.HasActiveApplicationAsync(userId);
        }

        /// <summary>
        /// Admin duyệt đơn đăng ký
        /// </summary>
        public async Task<BaseResposeDto> ApproveApplicationAsync(Guid applicationId, CurrentUserObject adminUser)
        {
            using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                var application = await _applicationRepository.GetByIdAsync(applicationId);
                if (application == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Application not found"
                    };
                }

                if (application.Status != SpecialtyShopApplicationStatus.Pending)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Application is not pending, cannot approve!"
                    };
                }

                // Get or create Specialty Shop role
                var shopRole = await _roleRepository.GetRoleByNameAsync(Constants.RoleSpecialtyShopName);
                if (shopRole == null)
                {
                    shopRole = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = Constants.RoleSpecialtyShopName,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _roleRepository.AddAsync(shopRole);
                    await _roleRepository.SaveChangesAsync();
                }

                // Update user role
                var user = await _userRepository.GetByIdAsync(application.UserId);
                if (user == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "User not found"
                    };
                }

                user.RoleId = shopRole.Id;
                user.UpdatedAt = DateTime.UtcNow;

                // Create SpecialtyShop record với đầy đủ data
                var specialtyShop = new SpecialtyShop
                {
                    Id = Guid.NewGuid(),
                    UserId = application.UserId,
                    ShopName = application.ShopName,
                    Description = application.ShopDescription,
                    Location = application.Location,
                    RepresentativeName = application.RepresentativeName,
                    Email = application.Email,
                    PhoneNumber = application.PhoneNumber,           // ⭐ NEW - Fix missing data
                    Website = application.Website,
                    BusinessLicense = application.BusinessLicense,   // ⭐ NEW - Fix missing data
                    BusinessLicenseUrl = application.BusinessLicenseUrl,
                    LogoUrl = application.LogoUrl,
                    ShopType = application.ShopType,
                    OpeningHours = application.OpeningHours,         // ⭐ NEW - Fix missing data
                    ClosingHours = application.ClosingHours,         // ⭐ NEW - Add closing hours
                    IsShopActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = application.UserId
                };

                await _unitOfWork.SpecialtyShopRepository!.AddAsync(specialtyShop);

                // Update application status
                application.Status = SpecialtyShopApplicationStatus.Approved;
                application.ProcessedAt = DateTime.UtcNow;
                application.ProcessedById = adminUser.Id;
                application.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send approval email
                await _emailSender.SendSpecialtyShopApprovalNotificationAsync(
                    application.Email,
                    user.Name,
                    application.ShopName);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Specialty shop application approved successfully"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"An error occurred while approving application: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Admin từ chối đơn đăng ký
        /// </summary>
        public async Task<BaseResposeDto> RejectApplicationAsync(
            Guid applicationId,
            RejectSpecialtyShopApplicationDto dto,
            CurrentUserObject adminUser)
        {
            try
            {
                var application = await _applicationRepository.GetByIdAsync(applicationId);
                if (application == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Application not found"
                    };
                }

                if (application.Status != SpecialtyShopApplicationStatus.Pending)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Application is not pending, cannot reject!"
                    };
                }

                // Update application status
                application.Status = SpecialtyShopApplicationStatus.Rejected;
                application.RejectionReason = dto.Reason;
                application.ProcessedAt = DateTime.UtcNow;
                application.ProcessedById = adminUser.Id;
                application.UpdatedAt = DateTime.UtcNow;

                await _applicationRepository.SaveChangesAsync();

                // Get user info for email
                var user = await _userRepository.GetByIdAsync(application.UserId);
                if (user != null)
                {
                    // Send rejection email
                    await _emailSender.SendSpecialtyShopRejectionNotificationAsync(
                        application.Email,
                        user.Name,
                        application.ShopName,
                        dto.Reason);
                }

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Specialty shop application rejected successfully"
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"An error occurred while rejecting application: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Helper method để upload file
        /// </summary>
        private async Task<string> UploadFileAsync(IFormFile file, string folder, string[] allowedExtensions)
        {
            const long MaxFileSize = 5 * 1024 * 1024; // 5MB

            if (file.Length > MaxFileSize)
            {
                return $"Error: File too large. Max size is {MaxFileSize / (1024 * 1024)} MB.";
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return $"Error: Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}";
            }

            var webRoot = _hostingEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploadFolder = Path.Combine(webRoot, "uploads", folder);
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var relativePath = Path.Combine("uploads", folder, fileName).Replace("\\", "/");

            return $"{baseUrl}/{relativePath}";
        }
    }
}
