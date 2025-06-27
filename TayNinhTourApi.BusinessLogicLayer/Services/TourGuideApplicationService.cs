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
        private readonly IFileStorageService _fileStorageService;

        public TourGuideApplicationService(
            ITourGuideApplicationRepository applicationRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMapper mapper,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            EmailSender emailSender,
            IFileStorageService fileStorageService)
        {
            _applicationRepository = applicationRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
            _fileStorageService = fileStorageService;
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
                // 2. Upload CV file with enhanced storage
                FileStorageResult? fileResult = null;
                if (dto.CurriculumVitae != null)
                {
                    fileResult = await _fileStorageService.StoreCvFileAsync(dto.CurriculumVitae, currentUser.Id);
                    if (!fileResult.IsSuccess)
                    {
                        return new TourGuideApplicationSubmitResponseDto
                        {
                            StatusCode = 400,
                            Message = $"Lỗi upload CV: {fileResult.ErrorMessage}"
                        };
                    }
                }

                // 3. Tạo application entity
                var application = new TourGuideApplication
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUser.Id,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    Experience = dto.Experience, // Experience is now string in DTO
                    Languages = dto.Languages, // Keep for backward compatibility
                    Skills = GetSkillsFromDto(dto), // Enhanced skill system
                    CurriculumVitae = fileResult?.AccessUrl,
                    CvOriginalFileName = fileResult?.OriginalFileName,
                    CvFileSize = fileResult?.FileSize,
                    CvContentType = fileResult?.ContentType,
                    CvFilePath = fileResult?.FilePath,
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
                    Languages = dto.Languages, // Keep for backward compatibility
                    Skills = GetSkillsFromJsonDto(dto), // Enhanced skill system
                    CurriculumVitae = dto.CurriculumVitaeUrl, // Store URL directly
                    Status = TourGuideApplicationStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUser.Id
                };

                // 3. Lưu application
                await _applicationRepository.AddAsync(application);
                await _applicationRepository.SaveChangesAsync();

                // 4. Send confirmation email (with error handling)
                try
                {
                    await _emailSender.SendTourGuideApplicationSubmittedAsync(
                        application.Email,
                        application.FullName,
                        application.SubmittedAt);
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the application submission
                    Console.WriteLine($"Email sending failed: {emailEx.Message}");
                }

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

                // Clean up CV file for rejected applications (optional - keep for audit trail)
                // await CleanupCvFileAsync(application.CvFilePath);

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
        /// Cleanup CV file when application is deleted
        /// </summary>
        private async Task CleanupCvFileAsync(string? filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                await _fileStorageService.DeleteCvFileAsync(filePath);
            }
        }

        /// <summary>
        /// Admin method to clean up orphaned CV files
        /// </summary>
        public async Task<BaseResposeDto> CleanupOrphanedFilesAsync()
        {
            try
            {
                var applications = await _applicationRepository.GetAllAsync();
                var cleanedCount = 0;

                foreach (var app in applications)
                {
                    if (!string.IsNullOrEmpty(app.CvFilePath) && !_fileStorageService.FileExists(app.CvFilePath))
                    {
                        // File doesn't exist, clear the path from database
                        app.CvFilePath = null;
                        app.CurriculumVitae = null;
                        cleanedCount++;
                    }
                }

                if (cleanedCount > 0)
                {
                    await _applicationRepository.SaveChangesAsync();
                }

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Cleaned up {cleanedCount} orphaned file references"
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Error cleaning up orphaned files: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Helper method để extract skills từ DTO
        /// Hỗ trợ cả Skills list và SkillsString, với fallback về Languages
        /// </summary>
        /// <param name="dto">SubmitTourGuideApplicationDto</param>
        /// <returns>Skills string for database storage</returns>
        private static string? GetSkillsFromDto(SubmitTourGuideApplicationDto dto)
        {
            // Priority 1: Skills list (new system)
            if (dto.Skills != null && dto.Skills.Any())
            {
                return TourGuideSkillUtility.SkillsToString(dto.Skills);
            }

            // Priority 2: SkillsString (API compatibility)
            if (!string.IsNullOrWhiteSpace(dto.SkillsString))
            {
                // Validate and normalize the skills string
                if (TourGuideSkillUtility.IsValidSkillsString(dto.SkillsString))
                {
                    return dto.SkillsString;
                }
            }

            // Priority 3: Languages (backward compatibility)
            if (!string.IsNullOrWhiteSpace(dto.Languages))
            {
                return TourGuideSkillUtility.MigrateLegacyLanguages(dto.Languages);
            }

            // Default: Vietnamese if nothing is provided
            return "Vietnamese";
        }

        /// <summary>
        /// Helper method để extract skills từ JSON DTO
        /// Hỗ trợ SkillsString với fallback về Languages
        /// </summary>
        /// <param name="dto">SubmitTourGuideApplicationJsonDto</param>
        /// <returns>Skills string for database storage</returns>
        private static string? GetSkillsFromJsonDto(SubmitTourGuideApplicationJsonDto dto)
        {
            // Priority 1: SkillsString (new system)
            if (!string.IsNullOrWhiteSpace(dto.SkillsString))
            {
                // Validate and normalize the skills string
                if (TourGuideSkillUtility.IsValidSkillsString(dto.SkillsString))
                {
                    return dto.SkillsString;
                }
            }

            // Priority 2: Languages (backward compatibility)
            if (!string.IsNullOrWhiteSpace(dto.Languages))
            {
                return TourGuideSkillUtility.MigrateLegacyLanguages(dto.Languages);
            }

            // Default: Vietnamese if nothing is provided
            return "Vietnamese";
        }
    }
}
