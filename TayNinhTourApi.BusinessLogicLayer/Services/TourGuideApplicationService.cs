using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// TourGuide Application Service Implementation
    /// Comprehensive implementation for tour guide application management
    /// </summary>
    public class TourGuideApplicationService : ITourGuideApplicationService
    {
        private readonly ITourGuideApplicationRepository _applicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmailSender _emailSender;

        public TourGuideApplicationService(
            ITourGuideApplicationRepository applicationRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMapper mapper,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            EmailSender emailSender)
        {
            _applicationRepository = applicationRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
        }

        /// <summary>
        /// User nộp đơn đăng ký TourGuide (Enhanced version)
        /// </summary>
        public async Task<TourGuideApplicationSubmitResponseDto> SubmitApplicationAsync(
            SubmitTourGuideApplicationDto dto,
            CurrentUserObject currentUser)
        {
            // 1. Validate user chưa có đơn active
            var hasActiveApplication = await _applicationRepository.HasActiveApplicationAsync(currentUser.Id);
            if (hasActiveApplication)
            {
                return new TourGuideApplicationSubmitResponseDto
                {
                    StatusCode = 400,
                    Message = "Bạn đã có đơn đăng ký đang chờ xử lý hoặc đã được duyệt. Vui lòng liên hệ support nếu cần hỗ trợ."
                };
            }

            try
            {
                // 2. Upload CV file
                string? cvUrl = null;
                if (dto.CurriculumVitae != null)
                {
                    cvUrl = await UploadCVFileAsync(dto.CurriculumVitae);
                }

                // 3. Tạo application entity
                var application = new TourGuideApplication
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUser.Id,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    Experience = dto.Experience.ToString(), // Convert int to string for enhanced entity
                    Languages = dto.Languages,
                    CurriculumVitae = cvUrl,
                    Status = TourGuideApplicationStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUser.Id
                };

                // 4. Lưu application
                await _applicationRepository.AddAsync(application);
                await _applicationRepository.SaveChangesAsync();

                // 5. Send confirmation email
                await _emailSender.SendTourGuideApplicationSubmittedAsync(
                    application.Email,
                    application.FullName,
                    application.SubmittedAt);

                return new TourGuideApplicationSubmitResponseDto
                {
                    StatusCode = 200,
                    Message = "Đơn đăng ký hướng dẫn viên đã được gửi thành công",
                    ApplicationId = application.Id,
                    FullName = application.FullName,
                    CurriculumVitaeUrl = application.CurriculumVitae,
                    SubmittedAt = application.SubmittedAt
                };
            }
            catch (Exception ex)
            {
                return new TourGuideApplicationSubmitResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi gửi đơn đăng ký: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// User nộp đơn đăng ký TourGuide (JSON version for API testing)
        /// </summary>
        public async Task<TourGuideApplicationSubmitResponseDto> SubmitApplicationJsonAsync(
            SubmitTourGuideApplicationJsonDto dto,
            CurrentUserObject currentUser)
        {
            // 1. Validate user chưa có đơn active
            var hasActiveApplication = await _applicationRepository.HasActiveApplicationAsync(currentUser.Id);
            if (hasActiveApplication)
            {
                return new TourGuideApplicationSubmitResponseDto
                {
                    StatusCode = 400,
                    Message = "Bạn đã có đơn đăng ký đang chờ xử lý hoặc đã được duyệt. Vui lòng liên hệ support nếu cần hỗ trợ."
                };
            }

            try
            {
                // 2. Tạo application entity (không cần upload file, dùng URL)
                var application = new TourGuideApplication
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUser.Id,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    Experience = dto.Experience, // Use experience description
                    Languages = dto.Languages,
                    CurriculumVitae = dto.CurriculumVitaeUrl, // Store URL directly
                    Status = TourGuideApplicationStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUser.Id
                };

                // 3. Lưu application
                await _applicationRepository.AddAsync(application);
                await _applicationRepository.SaveChangesAsync();

                // 4. Send confirmation email
                await _emailSender.SendTourGuideApplicationSubmittedAsync(
                    application.Email,
                    application.FullName,
                    application.SubmittedAt);

                return new TourGuideApplicationSubmitResponseDto
                {
                    StatusCode = 200,
                    Message = "Đơn đăng ký hướng dẫn viên đã được gửi thành công",
                    ApplicationId = application.Id,
                    FullName = application.FullName,
                    CurriculumVitaeUrl = application.CurriculumVitae,
                    SubmittedAt = application.SubmittedAt
                };
            }
            catch (Exception ex)
            {
                return new TourGuideApplicationSubmitResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi gửi đơn đăng ký: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// User xem danh sách đơn đăng ký của mình
        /// </summary>
        public async Task<IEnumerable<TourGuideApplicationSummaryDto>> GetMyApplicationsAsync(Guid userId)
        {
            var applications = await _applicationRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<TourGuideApplicationSummaryDto>>(applications);
        }

        /// <summary>
        /// User xem chi tiết đơn đăng ký của mình
        /// </summary>
        public async Task<TourGuideApplicationDto?> GetMyApplicationByIdAsync(Guid applicationId, Guid userId)
        {
            var application = await _applicationRepository.GetByIdAndUserIdAsync(applicationId, userId);
            return application != null ? _mapper.Map<TourGuideApplicationDto>(application) : null;
        }

        /// <summary>
        /// Admin xem danh sách tất cả đơn đăng ký với pagination
        /// </summary>
        public async Task<(IEnumerable<TourGuideApplicationSummaryDto> Applications, int TotalCount)> GetAllApplicationsAsync(
            int page = 1,
            int pageSize = 10,
            int? status = null)
        {
            var pageIndex = page - 1; // Convert to 0-based index
            TourGuideApplicationStatus? statusFilter = status.HasValue ? (TourGuideApplicationStatus)status.Value : null;

            var (applications, totalCount) = await _applicationRepository.GetPagedAsync(pageIndex, pageSize, statusFilter);
            var applicationDtos = _mapper.Map<IEnumerable<TourGuideApplicationSummaryDto>>(applications);

            return (applicationDtos, totalCount);
        }

        /// <summary>
        /// Admin xem chi tiết đơn đăng ký
        /// </summary>
        public async Task<TourGuideApplicationDto?> GetApplicationByIdAsync(Guid applicationId)
        {
            var application = await _applicationRepository.GetByIdWithDetailsAsync(applicationId);
            return application != null ? _mapper.Map<TourGuideApplicationDto>(application) : null;
        }

        /// <summary>
        /// Admin duyệt đơn đăng ký
        /// </summary>
        public async Task<BaseResposeDto> ApproveApplicationAsync(Guid applicationId, CurrentUserObject adminUser)
        {
            var application = await _applicationRepository.GetByIdWithDetailsAsync(applicationId);
            if (application == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Không tìm thấy đơn đăng ký"
                };
            }

            if (application.Status != TourGuideApplicationStatus.Pending)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Đơn đăng ký không ở trạng thái chờ xử lý"
                };
            }

            try
            {
                // 1. Get TourGuide role
                var tourGuideRole = await _roleRepository.GetRoleByNameAsync(Constants.RoleTourGuideName);
                if (tourGuideRole == null)
                {
                    tourGuideRole = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = Constants.RoleTourGuideName,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _roleRepository.AddAsync(tourGuideRole);
                    await _roleRepository.SaveChangesAsync();
                }

                // 2. Update user role
                var user = await _userRepository.GetByIdAsync(application.UserId);
                if (user == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy user"
                    };
                }

                user.RoleId = tourGuideRole.Id;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedById = adminUser.Id;

                // 3. Update application status
                application.Status = TourGuideApplicationStatus.Approved;
                application.ProcessedAt = DateTime.UtcNow;
                application.ProcessedById = adminUser.Id;
                application.UpdatedAt = DateTime.UtcNow;
                application.UpdatedById = adminUser.Id;

                await _applicationRepository.SaveChangesAsync();

                // 4. Send approval email
                await _emailSender.SendTourGuideApplicationApprovedAsync(
                    application.Email,
                    application.FullName);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đơn đăng ký đã được duyệt thành công"
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi duyệt đơn: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Admin từ chối đơn đăng ký
        /// </summary>
        public async Task<BaseResposeDto> RejectApplicationAsync(
            Guid applicationId,
            RejectTourGuideApplicationDto dto,
            CurrentUserObject adminUser)
        {
            var application = await _applicationRepository.GetByIdWithDetailsAsync(applicationId);
            if (application == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Không tìm thấy đơn đăng ký"
                };
            }

            if (application.Status != TourGuideApplicationStatus.Pending)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Đơn đăng ký không ở trạng thái chờ xử lý"
                };
            }

            try
            {
                // Update application status
                application.Status = TourGuideApplicationStatus.Rejected;
                application.RejectionReason = dto.Reason;
                application.ProcessedAt = DateTime.UtcNow;
                application.ProcessedById = adminUser.Id;
                application.UpdatedAt = DateTime.UtcNow;
                application.UpdatedById = adminUser.Id;

                await _applicationRepository.SaveChangesAsync();

                // Send rejection email
                await _emailSender.SendTourGuideApplicationRejectedAsync(
                    application.Email,
                    application.FullName,
                    dto.Reason);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đơn đăng ký đã được từ chối"
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi từ chối đơn: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Kiểm tra user có thể nộp đơn mới không
        /// </summary>
        public async Task<bool> CanSubmitNewApplicationAsync(Guid userId)
        {
            return !await _applicationRepository.HasActiveApplicationAsync(userId);
        }

        /// <summary>
        /// Upload CV file và return URL
        /// </summary>
        private async Task<string> UploadCVFileAsync(IFormFile cvFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cv");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{cvFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(fileStream);
            }

            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}/uploads/cv/{fileName}";
        }
    }
}
