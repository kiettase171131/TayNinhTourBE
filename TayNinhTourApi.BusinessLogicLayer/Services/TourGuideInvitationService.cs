using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho TourGuide invitation workflow
    /// </summary>
    public class TourGuideInvitationService : BaseService, ITourGuideInvitationService
    {
        private readonly ILogger<TourGuideInvitationService> _logger;
        private readonly EmailSender _emailSender;
        private readonly ITourCompanyNotificationService _notificationService;
        private readonly IServiceProvider _serviceProvider;

        public TourGuideInvitationService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<TourGuideInvitationService> logger,
            EmailSender emailSender,
            ITourCompanyNotificationService notificationService,
            IServiceProvider serviceProvider) : base(mapper, unitOfWork)
        {
            _logger = logger;
            _emailSender = emailSender;
            _notificationService = notificationService;
            _serviceProvider = serviceProvider;
        }

        public async Task<BaseResposeDto> CreateAutomaticInvitationsAsync(Guid tourDetailsId, Guid createdById)
        {
            try
            {
                _logger.LogInformation("Creating automatic invitations for TourDetails {TourDetailsId}", tourDetailsId);

                // 1. Lấy TourDetails với SkillsRequired
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    _logger.LogWarning("TourDetails {TourDetailsId} not found", tourDetailsId);
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                _logger.LogInformation("TourDetails found: {Title}, SkillsRequired: {SkillsRequired}",
                    tourDetails.Title, tourDetails.SkillsRequired);

                // 2. Lấy tất cả TourGuide operational records (available guides)
                var tourGuides = await _unitOfWork.TourGuideRepository.GetAvailableTourGuidesAsync();
                _logger.LogInformation("Found {Count} available tour guides in system", tourGuides.Count());

                if (!tourGuides.Any())
                {
                    _logger.LogWarning("No available tour guides found in system");
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy TourGuide nào có sẵn trong hệ thống",
                        success = false
                    };
                }

                // 3. Filter TourGuides có skills phù hợp (Enhanced skill matching)
                var matchingGuides = new List<(TourGuide guide, double matchScore, List<string> matchedSkills)>();
                foreach (var guide in tourGuides)
                {
                    _logger.LogInformation("Processing guide: {GuideId} - {GuideName} - {GuideEmail}",
                        guide.Id, guide.FullName, guide.Email);

                    // Sử dụng skills từ TourGuide record (đã có sẵn từ application)
                    var guideSkills = guide.Skills ?? string.Empty;
                    _logger.LogInformation("Guide {GuideId} skills: {GuideSkills}",
                        guide.Id, guideSkills);

                    // Sử dụng enhanced skill matching
                    var isMatch = SkillsMatchingUtility.MatchSkillsEnhanced(tourDetails.SkillsRequired, guideSkills);
                    _logger.LogInformation("Skill matching result for guide {GuideId}: {IsMatch} (Required: {RequiredSkills}, Guide: {GuideSkills})",
                        guide.Id, isMatch, tourDetails.SkillsRequired, guideSkills);

                    if (isMatch)
                    {
                        var matchScore = SkillsMatchingUtility.CalculateMatchScoreEnhanced(tourDetails.SkillsRequired, guideSkills);
                        var matchedSkills = SkillsMatchingUtility.GetMatchedSkillsEnhanced(tourDetails.SkillsRequired, guideSkills);

                        matchingGuides.Add((guide, matchScore, matchedSkills));
                        _logger.LogInformation("Guide {GuideId} added to matching list with score {MatchScore}",
                            guide.Id, matchScore);
                    }
                }

                // Sắp xếp guides theo match score giảm dần
                var sortedGuides = matchingGuides.OrderByDescending(x => x.matchScore)
                                                .ThenByDescending(x => x.matchedSkills.Count)
                                                .Select(x => x.guide)
                                                .ToList();

                _logger.LogInformation("Found {Count} matching guides after skill filtering", sortedGuides.Count);

                if (!sortedGuides.Any())
                {
                    _logger.LogWarning("No tour guides found with matching skills for TourDetails {TourDetailsId}", tourDetailsId);
                    
                    // Gửi thông báo cho TourCompany về việc không tìm thấy HDV phù hợp
                    try
                    {
                        await NotifyTourCompanyAboutNoSuitableGuidesAsync(tourDetails);
                    }
                    catch (Exception notifyEx)
                    {
                        _logger.LogWarning("Failed to send no suitable guides notification: {Error}", notifyEx.Message);
                    }
                    
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy hướng dẫn viên nào có kỹ năng phù hợp. Vui lòng vào hệ thống để xem danh sách hướng dẫn viên và tự chọn người phù hợp cho tour của bạn.",
                        success = false
                    };
                }

                // 4. Tạo invitations cho matching guides (ưu tiên guides có match score cao)
                var invitationsCreated = 0;
                var expiresAt = DateTime.UtcNow.AddHours(24); // 24 hours expiry

                foreach (var guide in sortedGuides)
                {
                    _logger.LogInformation("Checking existing invitation for guide {GuideId} and tour {TourDetailsId}",
                        guide.Id, tourDetailsId);

                    // Check xem đã có invitation nào (bất kể status) chưa
                    var existingInvitation = await _unitOfWork.TourGuideInvitationRepository
                        .GetAllAsync(inv => inv.TourDetailsId == tourDetailsId &&
                                           inv.GuideId == guide.Id &&
                                           !inv.IsDeleted);

                    var latestInvitation = existingInvitation.OrderByDescending(inv => inv.InvitedAt).FirstOrDefault();

                    if (latestInvitation != null)
                    {
                        _logger.LogInformation("Found existing invitation {InvitationId} with status {Status} for guide {GuideId}",
                            latestInvitation.Id, latestInvitation.Status, guide.Id);

                        if (latestInvitation.Status == InvitationStatus.Pending)
                        {
                            _logger.LogInformation("Skipping guide {GuideId} - {GuideName} because pending invitation already exists",
                                guide.Id, guide.FullName);
                            continue;
                        }

                        // Reuse existing invitation by updating it
                        _logger.LogInformation("Reusing existing invitation {InvitationId} for guide {GuideId} - {GuideName}",
                            latestInvitation.Id, guide.Id, guide.FullName);

                        latestInvitation.Status = InvitationStatus.Pending;
                        latestInvitation.InvitedAt = DateTime.UtcNow;
                        latestInvitation.ExpiresAt = expiresAt;
                        latestInvitation.RespondedAt = null;
                        latestInvitation.RejectionReason = null;
                        latestInvitation.UpdatedAt = DateTime.UtcNow;
                        latestInvitation.UpdatedById = createdById;

                        await _unitOfWork.TourGuideInvitationRepository.UpdateAsync(latestInvitation);
                        invitationsCreated++;

                        _logger.LogInformation("Successfully reused invitation {InvitationId} for guide {GuideId}",
                            latestInvitation.Id, guide.Id);

                        // Send invitation email
                        try
                        {
                            await _emailSender.SendTourGuideInvitationAsync(
                                guide.Email,
                                guide.FullName,
                                tourDetails.Title,
                                tourDetails.CreatedBy.Name,
                                expiresAt,
                                latestInvitation.Id.ToString()
                            );
                            _logger.LogInformation("Successfully sent invitation email to {GuideEmail}", guide.Email);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning("Failed to send invitation email to {GuideEmail}: {Error}",
                                guide.Email, emailEx.Message);
                        }

                        // 🔔 Send in-app notification to TourGuide (for reused invitation)
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                            await notificationService.CreateTourGuideInvitationNotificationAsync(
                                guide.UserId, // Use User ID for notification
                                tourDetails.Title,
                                tourDetails.CreatedBy.Name,
                                tourDetails.SkillsRequired,
                                InvitationType.Automatic.ToString(),
                                expiresAt,
                                latestInvitation.Id);

                            _logger.LogInformation("Successfully sent in-app notification to TourGuide {GuideId} for reused invitation", guide.Id);
                        }
                        catch (Exception notificationEx)
                        {
                            _logger.LogWarning("Failed to send in-app notification to TourGuide {GuideId}: {Error}",
                                guide.Id, notificationEx.Message);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Creating new invitation for guide {GuideId} - {GuideName}",
                            guide.Id, guide.FullName);

                        var invitation = new TourGuideInvitation
                        {
                            Id = Guid.NewGuid(),
                            TourDetailsId = tourDetailsId,
                            GuideId = guide.Id,
                            InvitationType = InvitationType.Automatic,
                            Status = InvitationStatus.Pending,
                            InvitedAt = DateTime.UtcNow,
                            ExpiresAt = expiresAt,
                            CreatedById = createdById,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        await _unitOfWork.TourGuideInvitationRepository.AddAsync(invitation);
                        invitationsCreated++;

                        _logger.LogInformation("Successfully created invitation {InvitationId} for guide {GuideId}",
                            invitation.Id, guide.Id);

                        // Send invitation email
                        try
                        {
                            await _emailSender.SendTourGuideInvitationAsync(
                                guide.Email,
                                guide.FullName,
                                tourDetails.Title,
                                tourDetails.CreatedBy.Name,
                                expiresAt,
                                invitation.Id.ToString()
                            );
                            _logger.LogInformation("Successfully sent invitation email to {GuideEmail}", guide.Email);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning("Failed to send invitation email to {GuideEmail}: {Error}",
                                guide.Email, emailEx.Message);
                        }

                        // 🔔 Send in-app notification to TourGuide
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                            await notificationService.CreateTourGuideInvitationNotificationAsync(
                                guide.UserId, // Use User ID for notification
                                tourDetails.Title,
                                tourDetails.CreatedBy.Name,
                                tourDetails.SkillsRequired,
                                InvitationType.Automatic.ToString(),
                                expiresAt,
                                invitation.Id);

                            _logger.LogInformation("Successfully sent in-app notification to TourGuide {GuideId}", guide.Id);
                        }
                        catch (Exception notificationEx)
                        {
                            _logger.LogWarning("Failed to send in-app notification to TourGuide {GuideId}: {Error}",
                                guide.Id, notificationEx.Message);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // 5. Update TourDetails status to AwaitingGuideAssignment ONLY if no accepted invitation exists
                var existingInvitations = await _unitOfWork.TourGuideInvitationRepository.GetByTourDetailsAsync(tourDetailsId);
                var hasAcceptedInvitation = existingInvitations.Any(inv => inv.Status == InvitationStatus.Accepted && inv.IsActive);

                if (!hasAcceptedInvitation)
                {
                    tourDetails.Status = TourDetailsStatus.AwaitingGuideAssignment;
                    tourDetails.UpdatedAt = DateTime.UtcNow;
                    tourDetails.UpdatedById = createdById;
                    await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Updated TourDetails {TourDetailsId} status to AwaitingGuideAssignment", tourDetailsId);
                }
                else
                {
                    _logger.LogInformation("TourDetails {TourDetailsId} already has accepted invitation, keeping current status: {Status}",
                        tourDetailsId, tourDetails.Status);
                }

                _logger.LogInformation("Created {Count} automatic invitations for TourDetails {TourDetailsId}",
                    invitationsCreated, tourDetailsId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Đã tạo {invitationsCreated} lời mời tự động cho các hướng dẫn viên phù hợp",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating automatic invitations for TourDetails {TourDetailsId}", tourDetailsId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi tạo lời mời: {ex.Message}",
                    success = false
                };
            }
        }

        public async Task<BaseResposeDto> CreateManualInvitationAsync(Guid tourDetailsId, Guid guideId, Guid createdById)
        {
            try
            {
                _logger.LogInformation("Creating manual invitation for TourDetails {TourDetailsId} to Guide {GuideId}",
                    tourDetailsId, guideId);

                // 1. Validate TourDetails exists
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                // 2. Validate TourGuide exists and is available
                var guide = await _unitOfWork.TourGuideRepository.GetByIdWithDetailsAsync(guideId);
                if (guide == null || !guide.IsActive || !guide.IsAvailable)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourGuide không tồn tại hoặc không có sẵn",
                        success = false
                    };
                }

                // 3. ✅ NEW: Check if tour already has an accepted tour guide
                var hasAcceptedGuide = await _unitOfWork.TourGuideInvitationRepository
                    .HasAcceptedInvitationAsync(tourDetailsId);
                if (hasAcceptedGuide)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Tour này đã có hướng dẫn viên được chấp nhận. Không thể mời thêm hướng dẫn viên khác.",
                        success = false
                    };
                }

                // 4. Check existing pending invitation
                var hasPending = await _unitOfWork.TourGuideInvitationRepository
                    .HasPendingInvitationAsync(tourDetailsId, guideId);
                if (hasPending)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "TourGuide này đã có lời mời pending cho tour này",
                        success = false
                    };
                }

                // 5. ✅ Check if guide has rejected invitation for this tour before
                var hasRejected = await _unitOfWork.TourGuideInvitationRepository
                    .HasRejectedInvitationAsync(tourDetailsId, guideId);
                if (hasRejected)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Hướng dẫn viên này đã từ chối lời mời cho tour này trước đó. Không thể mời lại.",
                        success = false
                    };
                }

                // 6. Create manual invitation
                var invitation = new TourGuideInvitation
                {
                    Id = Guid.NewGuid(),
                    TourDetailsId = tourDetailsId,
                    GuideId = guideId,
                    InvitationType = InvitationType.Manual,
                    Status = InvitationStatus.Pending,
                    InvitedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24), // 24 hours for manual invitations
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.TourGuideInvitationRepository.AddAsync(invitation);
                await _unitOfWork.SaveChangesAsync();

                // 7. Send invitation email
                try
                {
                    await _emailSender.SendTourGuideInvitationAsync(
                        guide.Email,
                        guide.FullName,
                        tourDetails.Title,
                        tourDetails.CreatedBy.Name,
                        invitation.ExpiresAt,
                        invitation.Id.ToString()
                    );
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning("Failed to send manual invitation email to {GuideEmail}: {Error}",
                        guide.Email, emailEx.Message);
                }

                // 8. 🔔 Send in-app notification to TourGuide
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    await notificationService.CreateTourGuideInvitationNotificationAsync(
                        guide.UserId, // Use User ID for notification
                        tourDetails.Title,
                        tourDetails.CreatedBy.Name,
                        tourDetails.SkillsRequired,
                        InvitationType.Manual.ToString(),
                        invitation.ExpiresAt,
                        invitation.Id);

                    _logger.LogInformation("Successfully sent in-app notification to TourGuide {GuideId} for manual invitation", guide.Id);
                }
                catch (Exception notificationEx)
                {
                    _logger.LogWarning("Failed to send in-app notification to TourGuide {GuideId}: {Error}",
                        guide.Id, notificationEx.Message);
                }

                _logger.LogInformation("Created manual invitation {InvitationId} for TourDetails {TourDetailsId}",
                    invitation.Id, tourDetailsId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã gửi lời mời thành công đến hướng dẫn viên",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual invitation for TourDetails {TourDetailsId} to Guide {GuideId}",
                    tourDetailsId, guideId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi tạo lời mời: {ex.Message}",
                    success = false
                };
            }
        }

        public async Task<BaseResposeDto> AcceptInvitationAsync(Guid invitationId, Guid guideId)
        {
            try
            {
                _logger.LogInformation("Guide {GuideId} accepting invitation {InvitationId}", guideId, invitationId);

                // 1. Get invitation
                var invitation = await _unitOfWork.TourGuideInvitationRepository.GetByIdAsync(invitationId, null);
                if (invitation == null)
                {
                    _logger.LogWarning("Invitation {InvitationId} not found", invitationId);
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Lời mời không tồn tại",
                        success = false
                    };
                }

                _logger.LogInformation("Found invitation: ID={InvitationId}, GuideId={GuideId}, Status={Status}, ExpiresAt={ExpiresAt}",
                    invitation.Id, invitation.GuideId, invitation.Status, invitation.ExpiresAt);

                // 2. Basic validations
                if (invitation.GuideId != guideId)
                {
                    _logger.LogWarning("Guide {GuideId} trying to accept invitation {InvitationId} belonging to {OwnerGuideId}",
                        guideId, invitationId, invitation.GuideId);
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền chấp nhận lời mời này",
                        success = false
                    };
                }

                if (invitation.Status != InvitationStatus.Pending)
                {
                    _logger.LogWarning("Invitation {InvitationId} has status {Status}, not Pending", invitationId, invitation.Status);
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Lời mời này đã được xử lý hoặc đã hết hạn",
                        success = false
                    };
                }

                if (invitation.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Invitation {InvitationId} expired at {ExpiresAt}, current time {Now}",
                        invitationId, invitation.ExpiresAt, DateTime.UtcNow);
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Lời mời đã hết hạn",
                        success = false
                    };
                }

                // 3. ✅ NEW LOGIC: Check for template schedule conflicts BEFORE any changes
                _logger.LogInformation("Starting template schedule conflict check for guide {GuideId} accepting TourDetails {TourDetailsId}", guideId, invitation.TourDetailsId);
                
                var templateConflictInfo = await CheckTemplateScheduleConflictsAsync(guideId, invitation.TourDetailsId, invitationId);
                
                if (templateConflictInfo.HasConflicts)
                {
                    var conflictMessages = new List<string>();
                    
                    foreach (var conflict in templateConflictInfo.ConflictingInvitations)
                    {
                        var scheduleText = GetScheduleDayText(conflict.ScheduleDays);
                        conflictMessages.Add($"Tour '{conflict.TourTitle}' ({scheduleText} - Tháng {conflict.Month}/{conflict.Year})");
                    }

                    var conflictMessage = $"❌ KHÔNG THỂ CHẤP NHẬN: Bạn đã đồng ý tham gia tour khác cùng thời gian biểu. ";
                    if (templateConflictInfo.NewTemplateInfo != null)
                    {
                        var newScheduleText = GetScheduleDayText(templateConflictInfo.NewTemplateInfo.ScheduleDays);
                        conflictMessage += $"Tour hiện tại: {newScheduleText} - Tháng {templateConflictInfo.NewTemplateInfo.Month}/{templateConflictInfo.NewTemplateInfo.Year}. ";
                    }
                    conflictMessage += $"Tour bị trùng: {string.Join("; ", conflictMessages)}. ";
                    conflictMessage += "Vui lòng kiểm tra lại lịch của bạn.";

                    _logger.LogWarning("Guide {GuideId} cannot accept invitation {InvitationId} due to template schedule conflicts: {ConflictMessages}",
                        guideId, invitationId, string.Join("; ", conflictMessages));

                    return new BaseResposeDto
                    {
                        StatusCode = 409, // 409 Conflict
                        Message = conflictMessage,
                        success = false,
                        ValidationErrors = new List<string> { "Xung đột lịch trình template với tour đã đồng ý trước đó" }
                    };
                }

                _logger.LogInformation("Guide {GuideId} has no template schedule conflicts, proceeding with acceptance", guideId);

                // 4. Update invitation status
                _logger.LogInformation("Starting invitation update process...");

                invitation.Status = InvitationStatus.Accepted;
                invitation.RespondedAt = DateTime.UtcNow;
                invitation.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updated invitation properties in memory");

                // 5. Save changes
                try
                {
                    await _unitOfWork.TourGuideInvitationRepository.UpdateAsync(invitation);
                    _logger.LogInformation("Called UpdateAsync on repository");

                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Called SaveChangesAsync on unit of work");

                    // 6. UPDATE TOURDETAILS STATUS: Khi guide accept invitation, cập nhật TourDetails status
                    await UpdateTourDetailsStatusAfterGuideAcceptanceAsync(invitation.TourDetailsId, invitationId);

                    // 🔔 SEND NOTIFICATION TO TOUR COMPANY: Gửi thông báo khi guide chấp nhận lời mời
                    await NotifyTourCompanyAboutGuideAcceptanceAsync(invitation, guideId);
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Failed to save invitation changes: {Message}. Inner: {InnerMessage}",
                        saveEx.Message, saveEx.InnerException?.Message);

                    return new BaseResposeDto
                    {
                        StatusCode = 500,
                        Message = $"Không thể lưu thay đổi: {saveEx.Message}",
                        success = false
                    };
                }

                _logger.LogInformation("Successfully accepted invitation {InvitationId}", invitationId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã chấp nhận lời mời thành công",
                    success = true
                };


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting invitation {InvitationId} by guide {GuideId}. Full exception: {FullException}",
                    invitationId, guideId, ex.ToString());
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi chấp nhận lời mời: {ex.Message}. Inner: {ex.InnerException?.Message}",
                    success = false
                };
            }
        }

        public async Task<BaseResposeDto> RejectInvitationAsync(Guid invitationId, Guid guideId, string? rejectionReason = null)
        {
            try
            {
                _logger.LogInformation("Guide {GuideId} rejecting invitation {InvitationId}", guideId, invitationId);

                // 1. Get invitation
                var invitation = await _unitOfWork.TourGuideInvitationRepository.GetWithDetailsAsync(invitationId);
                if (invitation == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Lời mời không tồn tại",
                        success = false
                    };
                }

                // 2. Verify ownership
                if (invitation.GuideId != guideId)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền từ chối lời mời này",
                        success = false
                    };
                }

                // 3. Check invitation status
                if (invitation.Status != InvitationStatus.Pending)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Lời mời này đã được xử lý",
                        success = false
                    };
                }

                // 4. Get TourGuide to get User ID for UpdatedById
                var tourGuide = await _unitOfWork.TourGuideRepository.GetByIdAsync(guideId);
                if (tourGuide == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin TourGuide",
                        success = false
                    };
                }

                // 5. Update invitation status
                invitation.Status = InvitationStatus.Rejected;
                invitation.RespondedAt = DateTime.UtcNow;
                invitation.RejectionReason = rejectionReason;
                invitation.UpdatedAt = DateTime.UtcNow;
                invitation.UpdatedById = tourGuide.UserId; // ✅ Use User ID instead of TourGuide ID

                await _unitOfWork.TourGuideInvitationRepository.UpdateAsync(invitation);
                await _unitOfWork.SaveChangesAsync();

                // 6. 🔔 Send notification to TourCompany about rejection
                try
                {
                    await NotifyTourCompanyAboutRejectionAsync(invitation.TourDetails, tourGuide, rejectionReason);
                }
                catch (Exception notifyEx)
                {
                    _logger.LogWarning("Failed to send rejection notification: {Error}", notifyEx.Message);
                    // Don't fail the main operation if notification fails
                }

                _logger.LogInformation("Guide {GuideId} successfully rejected invitation {InvitationId}",
                    guideId, invitationId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã từ chối lời mời thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting invitation {InvitationId} by guide {GuideId}",
                    invitationId, guideId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi từ chối lời mời: {ex.Message}",
                    success = false
                };
            }
        }

        public async Task<MyInvitationsResponseDto> GetMyInvitationsAsync(Guid guideId, InvitationStatus? status = null)
        {
            try
            {
                var invitations = await _unitOfWork.TourGuideInvitationRepository.GetByGuideAsync(guideId, status);

                // Map to proper DTOs
                var invitationDtos = invitations.Select(inv => new TourGuideInvitationDto
                {
                    Id = inv.Id,
                    TourDetails = new TourDetailsBasicDto
                    {
                        Id = inv.TourDetails.Id,
                        Title = inv.TourDetails.Title,
                        Description = inv.TourDetails.Description,
                        SkillsRequired = inv.TourDetails.SkillsRequired,
                        Status = inv.TourDetails.Status.ToString(),
                        CreatedAt = inv.TourDetails.CreatedAt
                    },
                    Guide = new UserBasicDto
                    {
                        Id = inv.TourGuide?.Id ?? Guid.Empty,
                        Name = inv.TourGuide?.FullName ?? "Unknown",
                        Email = inv.TourGuide?.Email ?? "Unknown",
                        PhoneNumber = inv.TourGuide?.PhoneNumber
                    },
                    CreatedBy = new UserBasicDto
                    {
                        Id = inv.CreatedBy?.Id ?? Guid.Empty,
                        Name = inv.CreatedBy?.Name ?? "Unknown",
                        Email = inv.CreatedBy?.Email ?? "Unknown",
                        PhoneNumber = inv.CreatedBy?.PhoneNumber
                    },
                    InvitationType = inv.InvitationType.ToString(),
                    Status = inv.Status.ToString(),
                    InvitedAt = inv.InvitedAt,
                    ExpiresAt = inv.ExpiresAt,
                    RespondedAt = inv.RespondedAt,
                    RejectionReason = inv.RejectionReason,
                    ImprovementSuggestion = null // TODO: Add if needed
                    // Note: HoursUntilExpiry, CanAccept, CanReject are computed properties
                }).ToList();

                // Calculate statistics
                var stats = new InvitationStatisticsDto
                {
                    TotalInvitations = invitations.Count(),
                    PendingCount = invitations.Count(i => i.Status == InvitationStatus.Pending),
                    AcceptedCount = invitations.Count(i => i.Status == InvitationStatus.Accepted),
                    RejectedCount = invitations.Count(i => i.Status == InvitationStatus.Rejected),
                    ExpiredCount = invitations.Count(i => i.Status == InvitationStatus.Expired),
                    LatestInvitation = invitations.OrderByDescending(i => i.InvitedAt).FirstOrDefault()?.InvitedAt,
                    LatestResponse = invitations.Where(i => i.RespondedAt.HasValue).OrderByDescending(i => i.RespondedAt).FirstOrDefault()?.RespondedAt
                };

                return new MyInvitationsResponseDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách lời mời thành công",
                    success = true,
                    Invitations = invitationDtos,
                    Statistics = stats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitations for guide {GuideId}", guideId);
                return new MyInvitationsResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false,
                    Invitations = new List<TourGuideInvitationDto>(),
                    Statistics = new InvitationStatisticsDto()
                };
            }
        }

        public async Task<TourDetailsInvitationsResponseDto> GetInvitationsForTourDetailsAsync(Guid tourDetailsId)
        {
            try
            {
                var invitations = await _unitOfWork.TourGuideInvitationRepository.GetByTourDetailsAsync(tourDetailsId);

                var invitationDtos = invitations.Select(inv => new TourGuideInvitationDto
                {
                    Id = inv.Id,
                    Guide = new UserBasicDto
                    {
                        Id = inv.TourGuide?.Id ?? Guid.Empty,
                        Name = inv.TourGuide?.FullName ?? "Unknown",
                        Email = inv.TourGuide?.Email ?? "Unknown",
                        PhoneNumber = inv.TourGuide?.PhoneNumber
                    },
                    CreatedBy = new UserBasicDto
                    {
                        Id = inv.CreatedBy?.Id ?? Guid.Empty,
                        Name = inv.CreatedBy?.Name ?? "Unknown",
                        Email = inv.CreatedBy?.Email ?? "Unknown",
                        PhoneNumber = inv.CreatedBy?.PhoneNumber
                    },
                    TourDetails = new TourDetailsBasicDto(), // TODO: Map properly if needed
                    InvitationType = inv.InvitationType.ToString(),
                    Status = inv.Status.ToString(),
                    InvitedAt = inv.InvitedAt,
                    ExpiresAt = inv.ExpiresAt,
                    RespondedAt = inv.RespondedAt,
                    RejectionReason = inv.RejectionReason
                }).ToList();

                // Calculate statistics
                var stats = new InvitationStatisticsDto
                {
                    TotalInvitations = invitations.Count(),
                    PendingCount = invitations.Count(i => i.Status == InvitationStatus.Pending),
                    AcceptedCount = invitations.Count(i => i.Status == InvitationStatus.Accepted),
                    RejectedCount = invitations.Count(i => i.Status == InvitationStatus.Rejected),
                    ExpiredCount = invitations.Count(i => i.Status == InvitationStatus.Expired),
                    LatestInvitation = invitations.OrderByDescending(i => i.InvitedAt).FirstOrDefault()?.InvitedAt,
                    LatestResponse = invitations.Where(i => i.RespondedAt.HasValue).OrderByDescending(i => i.RespondedAt).FirstOrDefault()?.RespondedAt
                };
                // AcceptanceRate và RejectionRate là computed properties, không cần set

                return new TourDetailsInvitationsResponseDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách lời mời thành công",
                    success = true,
                    TourDetails = new TourDetailsBasicDto(), // TODO: Map properly
                    Invitations = invitationDtos,
                    Statistics = stats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitations for TourDetails {TourDetailsId}", tourDetailsId);
                return new TourDetailsInvitationsResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false,
                    TourDetails = new TourDetailsBasicDto(),
                    Invitations = new List<TourGuideInvitationDto>(),
                    Statistics = new InvitationStatisticsDto()
                };
            }
        }

        public async Task<int> ExpireExpiredInvitationsAsync()
        {
            try
            {
                var expiredInvitations = await _unitOfWork.TourGuideInvitationRepository.GetExpiredInvitationsAsync();
                var count = 0;

                foreach (var invitation in expiredInvitations)
                {
                    invitation.Status = InvitationStatus.Expired;
                    invitation.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.TourGuideInvitationRepository.UpdateAsync(invitation);
                    count++;
                }

                if (count > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Expired {Count} invitations", count);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error expiring invitations");
                return 0;
            }
        }

        public async Task<int> TransitionToManualSelectionAsync()
        {
            try
            {
                // Find TourDetails that are Pending for more than 24 hours with no accepted invitations
                var cutoffTime = DateTime.UtcNow.AddHours(-24);
                var tourDetailsToTransition = await _unitOfWork.TourDetailsRepository
                    .GetAllAsync(td => td.Status == TourDetailsStatus.Pending && td.CreatedAt <= cutoffTime);

                var count = 0;
                foreach (var tourDetails in tourDetailsToTransition)
                {
                    // Check if any invitation was accepted
                    var allInvitations = await _unitOfWork.TourGuideInvitationRepository
                        .GetByTourDetailsAsync(tourDetails.Id);

                    var hasAcceptedInvitation = allInvitations.Any(inv => inv.Status == InvitationStatus.Accepted);

                    if (!hasAcceptedInvitation)
                    {
                        // Count expired invitations for notification
                        var expiredInvitationsCount = allInvitations.Count(inv => 
                            inv.Status == InvitationStatus.Expired || 
                            inv.Status == InvitationStatus.Rejected ||
                            (inv.Status == InvitationStatus.Pending && inv.ExpiresAt <= DateTime.UtcNow));

                        // Update status to AwaitingGuideAssignment
                        tourDetails.Status = TourDetailsStatus.AwaitingGuideAssignment;
                        tourDetails.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);
                        count++;

                        // 🔔 Send notification to TourCompany about need for manual selection
                        try
                        {
                            await NotifyTourCompanyAboutManualSelectionAsync(tourDetails, expiredInvitationsCount);
                        }
                        catch (Exception notifyEx)
                        {
                            _logger.LogWarning("Failed to send manual selection notification for TourDetails {TourDetailsId}: {Error}", 
                                tourDetails.Id, notifyEx.Message);
                        }
                    }
                }

                if (count > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Transitioned {Count} TourDetails to manual selection", count);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transitioning to manual selection");
                return 0;
            }
        }

        public async Task<int> CancelUnassignedTourDetailsAsync()
        {
            try
            {
                // First, send warning notifications for tours that will be cancelled soon (3 days before)
                var warningCutoffTime = DateTime.UtcNow.AddDays(-2); // Tours that are 2 days away from cancellation
                var toursNearCancellation = await _unitOfWork.TourDetailsRepository
                    .GetAllAsync(td => td.Status == TourDetailsStatus.AwaitingGuideAssignment && 
                                      td.CreatedAt <= warningCutoffTime && 
                                      td.CreatedAt > DateTime.UtcNow.AddDays(-5));

                foreach (var tourDetails in toursNearCancellation)
                {
                    var daysUntilCancellation = 5 - (int)(DateTime.UtcNow - tourDetails.CreatedAt).TotalDays;
                    if (daysUntilCancellation > 0 && daysUntilCancellation <= 3)
                    {
                        try
                        {
                            await NotifyTourCompanyAboutRiskCancellationAsync(tourDetails, daysUntilCancellation);
                        }
                        catch (Exception notifyEx)
                        {
                            _logger.LogWarning("Failed to send risk cancellation notification for TourDetails {TourDetailsId}: {Error}", 
                                tourDetails.Id, notifyEx.Message);
                        }
                    }
                }

                // Find TourDetails that are AwaitingGuideAssignment for more than 5 days
                var cutoffTime = DateTime.UtcNow.AddDays(-5);
                var tourDetailsToCancel = await _unitOfWork.TourDetailsRepository
                    .GetAllAsync(td => td.Status == TourDetailsStatus.AwaitingGuideAssignment && td.CreatedAt <= cutoffTime);

                var count = 0;
                foreach (var tourDetails in tourDetailsToCancel)
                {
                    tourDetails.Status = TourDetailsStatus.Cancelled;
                    tourDetails.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);

                    // Send cancellation email
                    try
                    {
                        await _emailSender.SendTourDetailsCancellationAsync(
                            tourDetails.CreatedBy.Email,
                            tourDetails.CreatedBy.Name,
                            tourDetails.Title,
                            "Không tìm được hướng dẫn viên trong thời gian quy định (5 ngày)"
                        );
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning("Failed to send cancellation email: {Error}", emailEx.Message);
                    }

                    count++;
                }

                if (count > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Cancelled {Count} unassigned TourDetails", count);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling unassigned TourDetails");
                return 0;
            }
        }

        public async Task<InvitationDetailsResponseDto> GetInvitationDetailsAsync(Guid invitationId)
        {
            try
            {
                var invitation = await _unitOfWork.TourGuideInvitationRepository.GetWithDetailsAsync(invitationId);
                if (invitation == null)
                {
                    return new InvitationDetailsResponseDto
                    {
                        StatusCode = 404,
                        Message = "Lời mời không tồn tại",
                        success = false,
                        Invitation = new TourGuideInvitationDetailDto()
                    };
                }

                var invitationDto = new
                {
                    Id = invitation.Id,
                    TourDetails = new
                    {
                        Id = invitation.TourDetails.Id,
                        Title = invitation.TourDetails.Title,
                        Description = invitation.TourDetails.Description,
                        SkillsRequired = invitation.TourDetails.SkillsRequired
                    },
                    Guide = new
                    {
                        Id = invitation.TourGuide.Id,
                        Name = invitation.TourGuide.FullName,
                        Email = invitation.TourGuide.Email
                    },
                    InvitationType = invitation.InvitationType.ToString(),
                    Status = invitation.Status.ToString(),
                    InvitedAt = invitation.InvitedAt,
                    ExpiresAt = invitation.ExpiresAt,
                    RespondedAt = invitation.RespondedAt,
                    RejectionReason = invitation.RejectionReason
                };

                return new InvitationDetailsResponseDto
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin lời mời thành công",
                    success = true,
                    Invitation = new TourGuideInvitationDetailDto() // TODO: Map properly
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitation details {InvitationId}", invitationId);
                return new InvitationDetailsResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false,
                    Invitation = new TourGuideInvitationDetailDto()
                };
            }
        }

        public async Task<BaseResposeDto> ValidateInvitationAcceptanceAsync(Guid invitationId, Guid guideId)
        {
            try
            {
                var invitation = await _unitOfWork.TourGuideInvitationRepository.GetWithDetailsAsync(invitationId);
                if (invitation == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Lời mời không tồn tại",
                        success = false
                    };
                }

                var validationErrors = new List<string>();

                // Check ownership
                if (invitation.GuideId != guideId)
                {
                    validationErrors.Add("Bạn không có quyền chấp nhận lời mời này");
                }

                // Check status
                if (invitation.Status != InvitationStatus.Pending)
                {
                    validationErrors.Add("Lời mời này đã được xử lý");
                }

                // Check expiry
                if (invitation.ExpiresAt <= DateTime.UtcNow)
                {
                    validationErrors.Add("Lời mời đã hết hạn");
                }

                // ✅ NEW: Check for template schedule conflicts using the new logic
                if (validationErrors.Count == 0) // Only check conflicts if basic validation passes
                {
                    _logger.LogInformation("Validating template schedule conflicts for guide {GuideId} and TourDetails {TourDetailsId}", guideId, invitation.TourDetailsId);

                    var templateConflictInfo = await CheckTemplateScheduleConflictsAsync(guideId, invitation.TourDetailsId, invitationId);
                    
                    if (templateConflictInfo.HasConflicts)
                    {
                        var conflictMessages = new List<string>();
                        
                        foreach (var conflict in templateConflictInfo.ConflictingInvitations)
                        {
                            var scheduleText = GetScheduleDayText(conflict.ScheduleDays);
                            conflictMessages.Add($"Tour '{conflict.TourTitle}' ({scheduleText} - Tháng {conflict.Month}/{conflict.Year})");
                        }

                        var conflictMessage = "Không thể chấp nhận do xung đột lịch trình template: ";
                        if (templateConflictInfo.NewTemplateInfo != null)
                        {
                            var newScheduleText = GetScheduleDayText(templateConflictInfo.NewTemplateInfo.ScheduleDays);
                            conflictMessage += $"Tour hiện tại ({newScheduleText} - Tháng {templateConflictInfo.NewTemplateInfo.Month}/{templateConflictInfo.NewTemplateInfo.Year}) ";
                        }
                        conflictMessage += $"trùng với: {string.Join("; ", conflictMessages)}";
                        
                        validationErrors.Add(conflictMessage);
                        
                        _logger.LogWarning("Guide {GuideId} cannot accept invitation {InvitationId} due to template schedule conflicts: {ConflictMessages}",
                            guideId, invitationId, string.Join("; ", conflictMessages));
                    }
                }

                return new BaseResposeDto
                {
                    StatusCode = validationErrors.Any() ? 400 : 200,
                    Message = validationErrors.Any() 
                        ? string.Join("; ", validationErrors)
                        : "Có thể chấp nhận lời mời",
                    success = !validationErrors.Any(),
                    ValidationErrors = validationErrors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invitation acceptance {InvitationId}", invitationId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Helper method để chuyển đổi ScheduleDay sang text tiếng Việt
        /// </summary>
        /// <param name="scheduleDay">ScheduleDay enum</param>
        /// <returns>Text tiếng Việt</returns>
        private string GetScheduleDayText(ScheduleDay scheduleDay)
        {
            return scheduleDay switch
            {
                ScheduleDay.Saturday => "Thứ 7",
                ScheduleDay.Sunday => "Chủ nhật",
                _ => scheduleDay.ToString()
            };
        }

        /// <summary>
        /// Helper method để lấy thông tin template của TourDetails để kiểm tra xung đột lịch trình
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Thông tin template (ScheduleDays, Month, Year)</returns>
        private async Task<TourTemplateScheduleInfo?> GetTourTemplateScheduleInfoAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Getting template schedule info for TourDetails {TourDetailsId}", tourDetailsId);

                // Get TourDetails with TourTemplate
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailsId, new[] { "TourTemplate" });
                if (tourDetails?.TourTemplate == null)
                {
                    _logger.LogWarning("TourDetails {TourDetailsId} or its TourTemplate not found", tourDetailsId);
                    return null;
                }

                var template = tourDetails.TourTemplate;
                var scheduleInfo = new TourTemplateScheduleInfo
                {
                    TourDetailsId = tourDetailsId,
                    TourTemplateId = template.Id,
                    ScheduleDays = template.ScheduleDays,
                    Month = template.Month,
                    Year = template.Year,
                    TemplateTitle = template.Title
                };

                _logger.LogInformation("Template schedule info for TourDetails {TourDetailsId}: {ScheduleDays} - Tháng {Month}/{Year} - '{TemplateTitle}'", 
                    tourDetailsId, template.ScheduleDays, template.Month, template.Year, template.Title);

                return scheduleInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template schedule info for TourDetails {TourDetailsId}", tourDetailsId);
                return null;
            }
        }

        /// <summary>
        /// Helper method để kiểm tra xung đột lịch trình dựa trên TourTemplate (thứ, tháng, năm)
        /// </summary>
        /// <param name="guideId">ID của guide</param>
        /// <param name="newTourDetailsId">ID của TourDetails đang được chấp nhận</param>
        /// <param name="currentInvitationId">ID của invitation hiện tại (để exclude)</param>
        /// <returns>Thông tin chi tiết về xung đột</returns>
        private async Task<TemplateScheduleConflictInfo> CheckTemplateScheduleConflictsAsync(Guid guideId, Guid newTourDetailsId, Guid currentInvitationId)
        {
            try
            {
                _logger.LogInformation("Checking template schedule conflicts for guide {GuideId} accepting TourDetails {TourDetailsId}", guideId, newTourDetailsId);

                var conflictInfo = new TemplateScheduleConflictInfo();

                // 1. Get template schedule info for the new TourDetails
                var newTemplateInfo = await GetTourTemplateScheduleInfoAsync(newTourDetailsId);
                if (newTemplateInfo == null)
                {
                    _logger.LogWarning("Cannot get template info for new TourDetails {TourDetailsId}", newTourDetailsId);
                    return conflictInfo; // Cannot determine conflicts
                }

                _logger.LogInformation("New TourDetails template: {ScheduleDays} - Tháng {Month}/{Year} - '{TemplateTitle}'", 
                    newTemplateInfo.ScheduleDays, newTemplateInfo.Month, newTemplateInfo.Year, newTemplateInfo.TemplateTitle);

                // 2. Get all accepted invitations for this guide
                var acceptedInvitations = await _unitOfWork.TourGuideInvitationRepository.ListAsync(
                    i => i.GuideId == guideId &&
                         i.Id != currentInvitationId &&
                         i.Status == InvitationStatus.Accepted &&
                         !i.IsDeleted,
                    null);

                if (!acceptedInvitations.Any())
                {
                    _logger.LogInformation("Guide {GuideId} has no other accepted invitations", guideId);
                    return conflictInfo; // No conflicts
                }

                _logger.LogInformation("Guide {GuideId} has {Count} other accepted invitations to check for conflicts", guideId, acceptedInvitations.Count());

                // 3. Check each accepted invitation for template schedule conflicts
                var conflictingInvitations = new List<ConflictingTemplateInfo>();

                foreach (var acceptedInvitation in acceptedInvitations)
                {
                    var existingTemplateInfo = await GetTourTemplateScheduleInfoAsync(acceptedInvitation.TourDetailsId);
                    
                    if (existingTemplateInfo == null) continue;

                    _logger.LogInformation("Checking against existing invitation {InvitationId}: {ScheduleDays} - Tháng {Month}/{Year} - '{TemplateTitle}'", 
                        acceptedInvitation.Id, existingTemplateInfo.ScheduleDays, existingTemplateInfo.Month, existingTemplateInfo.Year, existingTemplateInfo.TemplateTitle);

                    // ✅ KEY LOGIC: Check if templates have same schedule (ScheduleDays, Month, Year)
                    bool hasTemplateConflict = 
                        newTemplateInfo.ScheduleDays == existingTemplateInfo.ScheduleDays &&
                        newTemplateInfo.Month == existingTemplateInfo.Month &&
                        newTemplateInfo.Year == existingTemplateInfo.Year;

                    if (hasTemplateConflict)
                    {
                        conflictingInvitations.Add(new ConflictingTemplateInfo
                        {
                            InvitationId = acceptedInvitation.Id,
                            TourDetailsId = acceptedInvitation.TourDetailsId,
                            TourTitle = existingTemplateInfo.TemplateTitle,
                            ScheduleDays = existingTemplateInfo.ScheduleDays,
                            Month = existingTemplateInfo.Month,
                            Year = existingTemplateInfo.Year
                        });

                        _logger.LogInformation("Found template schedule conflict: Guide already accepted invitation {InvitationId} for tour '{TourTitle}' with same schedule: {ScheduleDays} - Tháng {Month}/{Year}", 
                            acceptedInvitation.Id, existingTemplateInfo.TemplateTitle, existingTemplateInfo.ScheduleDays, existingTemplateInfo.Month, existingTemplateInfo.Year);
                    }
                    else
                    {
                        _logger.LogInformation("No template conflict with invitation {InvitationId} - different schedule", acceptedInvitation.Id);
                    }
                }

                conflictInfo.HasConflicts = conflictingInvitations.Any();
                conflictInfo.ConflictingInvitationsCount = conflictingInvitations.Count;
                conflictInfo.ConflictingInvitations = conflictingInvitations;
                conflictInfo.NewTemplateInfo = newTemplateInfo;

                _logger.LogInformation("Template schedule conflict check result: HasConflicts={HasConflicts}, ConflictingCount={Count}", 
                    conflictInfo.HasConflicts, conflictInfo.ConflictingInvitationsCount);

                return conflictInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking template schedule conflicts for guide {GuideId} and TourDetails {TourDetailsId}", guideId, newTourDetailsId);
                return new TemplateScheduleConflictInfo(); // Return empty info on error
            }
        }

        /// <summary>
        /// Cập nhật TourDetails status sau khi guide accept lời mời
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="acceptedInvitationId">ID của invitation được accept</param>
        private async Task UpdateTourDetailsStatusAfterGuideAcceptanceAsync(Guid tourDetailsId, Guid acceptedInvitationId)
        {
            try
            {
                _logger.LogInformation("Updating TourDetails {TourDetailsId} status after guide acceptance", tourDetailsId);

                // 1. Get TourDetails
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    _logger.LogWarning("TourDetails {TourDetailsId} not found", tourDetailsId);
                    return;
                }

                // 2. Only update if currently AwaitingGuideAssignment
                if (tourDetails.Status == TourDetailsStatus.AwaitingGuideAssignment)
                {
                    // Update TourDetails status to WaitToPublic (guide assignment completed, waiting for tour company to activate public)
                    tourDetails.Status = TourDetailsStatus.WaitToPublic;
                    tourDetails.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);

                    _logger.LogInformation("Updated TourDetails {TourDetailsId} status from AwaitingGuideAssignment to WaitToPublic", tourDetailsId);

                    // 3. Update TourOperation with accepted guide information
                    await UpdateTourOperationWithGuideAsync(tourDetailsId, acceptedInvitationId);

                    // 4. Expire other pending invitations for this TourDetails
                    var expiredCount = await _unitOfWork.TourGuideInvitationRepository
                        .ExpireInvitationsForTourDetailsAsync(tourDetailsId, acceptedInvitationId);

                    _logger.LogInformation("Expired {Count} pending invitations for TourDetails {TourDetailsId}", expiredCount, tourDetailsId);

                    // 5. Save all changes
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Successfully updated TourDetails {TourDetailsId} status and expired pending invitations", tourDetailsId);
                }
                else
                {
                    _logger.LogInformation("TourDetails {TourDetailsId} status is {Status}, no update needed", tourDetailsId, tourDetails.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating TourDetails {TourDetailsId} status after guide acceptance", tourDetailsId);
                // Don't throw - this is a side effect, shouldn't break the main flow
            }
        }

        /// <summary>
        /// Cập nhật TourOperation với thông tin guide khi invitation được accept
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="acceptedInvitationId">ID của invitation được accept</param>
        private async Task UpdateTourOperationWithGuideAsync(Guid tourDetailsId, Guid acceptedInvitationId)
        {
            try
            {
                _logger.LogInformation("Updating TourOperation with guide info for TourDetails {TourDetailsId}", tourDetailsId);

                // 1. Get the accepted invitation to get guide info
                var acceptedInvitation = await _unitOfWork.TourGuideInvitationRepository.GetByIdAsync(acceptedInvitationId);
                if (acceptedInvitation == null)
                {
                    _logger.LogWarning("Accepted invitation {InvitationId} not found", acceptedInvitationId);
                    return;
                }
                _logger.LogInformation("Found accepted invitation: GuideId={GuideId}", acceptedInvitation.GuideId);

                // 2. Get TourOperation for this TourDetails
                var tourOperation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(tourDetailsId);
                if (tourOperation == null)
                {
                    _logger.LogWarning("TourOperation not found for TourDetails {TourDetailsId}", tourDetailsId);
                    return;
                }
                _logger.LogInformation("Found TourOperation: Id={OperationId}, CurrentTourGuideId={CurrentTourGuideId}",
                    tourOperation.Id, tourOperation.TourGuideId);

                // 3. Get guide User info from TourGuide
                var tourGuide = await _unitOfWork.TourGuideRepository.GetByIdAsync(acceptedInvitation.GuideId);
                if (tourGuide == null)
                {
                    _logger.LogWarning("TourGuide {GuideId} not found", acceptedInvitation.GuideId);
                    return;
                }
                _logger.LogInformation("Found TourGuide: Id={TourGuideId}, UserId={UserId}",
                    tourGuide.Id, tourGuide.UserId);

                var guideUser = await _unitOfWork.UserRepository.GetByIdAsync(tourGuide.UserId);
                if (guideUser == null)
                {
                    _logger.LogWarning("Guide User {UserId} not found", tourGuide.UserId);
                    return;
                }
                _logger.LogInformation("Found Guide User: Id={UserId}, Name={Name}, Email={Email}",
                    guideUser.Id, guideUser.Name, guideUser.Email);

                // 4. Update TourOperation with guide info
                var oldTourGuideId = tourOperation.TourGuideId;
                tourOperation.TourGuideId = tourGuide.Id; // Use TourGuide ID
                tourOperation.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updating TourOperation {OperationId}: TourGuideId {OldTourGuideId} -> {NewTourGuideId} (TourGuide: {TourGuideName})",
                    tourOperation.Id, oldTourGuideId, tourGuide.Id, tourGuide.FullName);

                await _unitOfWork.TourOperationRepository.UpdateAsync(tourOperation);

                _logger.LogInformation("Successfully updated TourOperation {OperationId} with TourGuide {TourGuideId} (User: {UserId}) for TourDetails {TourDetailsId}",
                    tourOperation.Id, tourGuide.Id, guideUser.Id, tourDetailsId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating TourOperation with guide info for TourDetails {TourDetailsId}: {Message}. StackTrace: {StackTrace}",
                    tourDetailsId, ex.Message, ex.StackTrace);
                // Don't throw - this is a side effect, shouldn't break the main flow
            }
        }

        /// <summary>
        /// Gửi thông báo cho TourCompany khi TourGuide chấp nhận lời mời
        /// </summary>
        private async Task NotifyTourCompanyAboutGuideAcceptanceAsync(TourGuideInvitation invitation, Guid guideId)
        {
            try
            {
                _logger.LogInformation("Sending guide acceptance notification to TourCompany for TourDetails {TourDetailsId}", invitation.TourDetailsId);

                // Get TourDetails and TourGuide info
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(invitation.TourDetailsId);
                var tourGuide = await _unitOfWork.TourGuideRepository.GetByIdAsync(guideId);

                if (tourDetails == null || tourGuide == null)
                {
                    _logger.LogWarning("Cannot send guide acceptance notification - TourDetails or TourGuide not found");
                    return;
                }

                await _notificationService.NotifyGuideAcceptanceAsync(
                    tourDetails.CreatedById,
                    tourDetails.Title,
                    tourGuide.FullName,
                    tourGuide.Email,
                    invitation.RespondedAt ?? DateTime.UtcNow);

                _logger.LogInformation("Successfully sent guide acceptance notification for TourDetails {TourDetailsId}", invitation.TourDetailsId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending guide acceptance notification for TourDetails {TourDetailsId}", invitation.TourDetailsId);
                // Don't throw - notification failure shouldn't break main flow
            }
        }

        /// <summary>
        /// Gửi thông báo cho TourCompany khi TourGuide từ chối lời mời
        /// </summary>
        private async Task NotifyTourCompanyAboutRejectionAsync(TourDetails tourDetails, TourGuide tourGuide, string? rejectionReason)
        {
            try
            {
                _logger.LogInformation("Sending rejection notification to TourCompany for TourDetails {TourDetailsId}", tourDetails.Id);

                await _notificationService.NotifyGuideRejectionAsync(
                    tourDetails.CreatedById,
                    tourDetails.Title,
                    tourGuide.FullName,
                    rejectionReason);

                _logger.LogInformation("Successfully sent rejection notification for TourDetails {TourDetailsId}", tourDetails.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending rejection notification for TourDetails {TourDetailsId}", tourDetails.Id);
                // Don't throw - notification failure shouldn't break main flow
            }
        }

        /// <summary>
        /// Gửi thông báo cho TourCompany khi cần tìm guide thủ công (sau 24h)
        /// </summary>
        private async Task NotifyTourCompanyAboutManualSelectionAsync(TourDetails tourDetails, int expiredInvitationsCount)
        {
            try
            {
                _logger.LogInformation("Sending manual selection notification to TourCompany for TourDetails {TourDetailsId}", tourDetails.Id);

                await _notificationService.NotifyManualGuideSelectionNeededAsync(
                    tourDetails.CreatedById,
                    tourDetails.Title,
                    expiredInvitationsCount);

                _logger.LogInformation("Successfully sent manual selection notification for TourDetails {TourDetailsId}", tourDetails.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending manual selection notification for TourDetails {TourDetailsId}", tourDetails.Id);
                // Don't throw - notification failure shouldn't break main flow
            }
        }

        /// <summary>
        /// Gửi thông báo cho TourCompany khi tour sắp bị hủy
        /// </summary>
        private async Task NotifyTourCompanyAboutRiskCancellationAsync(TourDetails tourDetails, int daysUntilCancellation)
        {
            try
            {
                _logger.LogInformation("Sending risk cancellation notification to TourCompany for TourDetails {TourDetailsId}", tourDetails.Id);

                await _notificationService.NotifyTourRiskCancellationAsync(
                    tourDetails.CreatedById,
                    tourDetails.Title,
                    daysUntilCancellation);

                _logger.LogInformation("Successfully sent risk cancellation notification for TourDetails {TourDetailsId}", tourDetails.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending risk cancellation notification for TourDetails {TourDetailsId}", tourDetails.Id);
                // Don't throw - notification failure shouldn't break main flow
            }
        }

        /// <summary>
        /// Gửi thông báo cho TourCompany khi không tìm thấy hướng dẫn viên phù hợp
        /// </summary>
        private async Task NotifyTourCompanyAboutNoSuitableGuidesAsync(TourDetails tourDetails)
        {
            try
            {
                _logger.LogInformation("Sending no suitable guides notification to TourCompany for TourDetails {TourDetailsId}", tourDetails.Id);

                // 🔔 Tạo in-app notification trực tiếp qua repository
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = tourDetails.CreatedById,
                    Title = "⚠️ Không tìm thấy hướng dẫn viên phù hợp",
                    Message = $"Tour '{tourDetails.Title}' không tìm thấy hướng dẫn viên có kỹ năng phù hợp. Vui lòng vào danh sách hướng dẫn viên để tự chọn.",
                    Type = DataAccessLayer.Enums.NotificationType.Warning,
                    Priority = DataAccessLayer.Enums.NotificationPriority.High,
                    Icon = "⚠️",
                    ActionUrl = "/guides/list",
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.NotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully created in-app notification for TourDetails {TourDetailsId}", tourDetails.Id);

                // 📧 Gửi email notification
                try
                {
                    var user = await _unitOfWork.UserRepository.GetByIdAsync(tourDetails.CreatedById);
                    if (user != null)
                    {
                        var subject = $"Cần chọn hướng dẫn viên: Tour '{tourDetails.Title}'";
                        var htmlBody = $@"<h2>Chào {user.Name},</h2>
<p>Hệ thống không tìm thấy hướng dẫn viên nào có kỹ năng phù hợp với tour <strong>'{tourDetails.Title}'</strong>.</p>
<div style='background-color: #fff3cd; padding: 20px; border-left: 4px solid #ffc107; margin: 20px 0;'>
    <h3 style='margin-top: 0; color: #856404;'>Hành động cần thực hiện:</h3>
    <p><strong>Đăng nhập hệ thống</strong> để xem danh sách hướng dẫn viên có sẵn</p>
    <p><strong>Chọn và mời hướng dẫn viên</strong> từ danh sách hệ thống</p>
    <p><strong>Liên hệ trực tiếp</strong> với hướng dẫn viên để thảo luận điều kiện</p>
    <p><strong>Xem xét điều chỉnh yêu cầu kỹ năng</strong> hoặc tăng phí để thu hút guide</p>
</div>
<br/>
<p>Trân trọng,</p>
<p>Đội ngũ Tay Ninh Tour</p>";

                        await _emailSender.SendEmailAsync(user.Email, user.Name, subject, htmlBody);
                        _logger.LogInformation("Successfully sent email notification for no suitable guides to {Email}", user.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Cannot send email - User {UserId} not found", tourDetails.CreatedById);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Error sending email notification for no suitable guides");
                    // Don't fail the main flow if email fails
                }

                _logger.LogInformation("Successfully sent no suitable guides notification for TourDetails {TourDetailsId}", tourDetails.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending no suitable guides notification for TourDetails {TourDetailsId}", tourDetails.Id);
                // Don't throw - notification failure shouldn't break main flow
            }
        }

        /// <summary>
        /// Fix TourDetails status cho các case đã có guide accept nhưng status vẫn AwaitingGuideAssignment
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails cần fix</param>
        public async Task<BaseResposeDto> FixTourDetailsStatusAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Fixing TourDetails {TourDetailsId} status", tourDetailsId);

                // 1. Get TourDetails
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                // 2. Check if has accepted invitation
                var invitations = await _unitOfWork.TourGuideInvitationRepository.GetByTourDetailsAsync(tourDetailsId);
                var acceptedInvitation = invitations.FirstOrDefault(i => i.Status == InvitationStatus.Accepted && i.IsActive);

                if (acceptedInvitation == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "TourDetails này chưa có guide nào accept invitation",
                        success = false
                    };
                }

                // 3. Update status if needed
                if (tourDetails.Status == TourDetailsStatus.AwaitingGuideAssignment)
                {
                    tourDetails.Status = TourDetailsStatus.Approved;
                    tourDetails.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);

                    // Expire other pending invitations
                    var expiredCount = await _unitOfWork.TourGuideInvitationRepository
                        .ExpireInvitationsForTourDetailsAsync(tourDetailsId, acceptedInvitation.Id);

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Fixed TourDetails {TourDetailsId} status from AwaitingGuideAssignment to Approved, expired {Count} pending invitations",
                        tourDetailsId, expiredCount);

                    return new BaseResposeDto
                    {
                        StatusCode = 200,
                        Message = $"Đã fix TourDetails status thành công. Expired {expiredCount} pending invitations.",
                        success = true
                    };
                }
                else
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 200,
                        Message = $"TourDetails đã ở status {tourDetails.Status}, không cần fix",
                        success = true
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing TourDetails {TourDetailsId} status", tourDetailsId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Debug method để manually update TourOperation với guide info
        /// </summary>
        /// <param name="invitationId">ID của invitation đã được accept</param>
        /// <returns>Kết quả debug</returns>
        public async Task<BaseResposeDto> DebugUpdateTourOperationAsync(Guid invitationId)
        {
            try
            {
                _logger.LogInformation("Debug: Starting manual TourOperation update for invitation {InvitationId}", invitationId);

                // 1. Get the invitation
                var invitation = await _unitOfWork.TourGuideInvitationRepository.GetByIdAsync(invitationId);
                if (invitation == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Invitation không tồn tại",
                        success = false
                    };
                }

                _logger.LogInformation("Debug: Found invitation - Status: {Status}, TourDetailsId: {TourDetailsId}, GuideId: {GuideId}",
                    invitation.Status, invitation.TourDetailsId, invitation.GuideId);

                if (invitation.Status != InvitationStatus.Accepted)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Invitation chưa được accept",
                        success = false
                    };
                }

                // 2. Update TourDetails status if needed
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(invitation.TourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                _logger.LogInformation("Debug: Found TourDetails - Status: {Status}", tourDetails.Status);

                if (tourDetails.Status == TourDetailsStatus.AwaitingGuideAssignment)
                {
                    tourDetails.Status = TourDetailsStatus.WaitToPublic;
                    await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);
                    _logger.LogInformation("Debug: Updated TourDetails status to WaitToPublic");
                }

                // 3. Update TourOperation
                await UpdateTourOperationWithGuideAsync(invitation.TourDetailsId, invitationId);

                // 4. Save changes
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Debug: Successfully completed manual TourOperation update");

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Debug update thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Debug: Error in manual TourOperation update for invitation {InvitationId}: {Message}",
                    invitationId, ex.Message);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi debug: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Debug method để test notification khi không tìm thấy guide phù hợp
        /// </summary>
        public async Task<BaseResposeDto> DebugTestNoSuitableGuidesNotificationAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Debug: Testing no suitable guides notification for TourDetails {TourDetailsId}", tourDetailsId);

                // Get TourDetails
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                // Test notification
                await NotifyTourCompanyAboutNoSuitableGuidesAsync(tourDetails);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Debug: Đã gửi thông báo test thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Debug: Error testing no suitable guides notification for TourDetails {TourDetailsId}", tourDetailsId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Debug error: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Class để lưu thông tin schedule của TourTemplate
        /// </summary>
        private class TourTemplateScheduleInfo
        {
            public Guid TourDetailsId { get; set; }
            public Guid TourTemplateId { get; set; }
            public ScheduleDay ScheduleDays { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
            public string TemplateTitle { get; set; } = string.Empty;
        }

        /// <summary>
        /// Class để lưu thông tin về xung đột template schedule
        /// </summary>
        private class TemplateScheduleConflictInfo
        {
            public bool HasConflicts { get; set; } = false;
            public int ConflictingInvitationsCount { get; set; } = 0;
            public List<ConflictingTemplateInfo> ConflictingInvitations { get; set; } = new();
            public TourTemplateScheduleInfo? NewTemplateInfo { get; set; }
        }

        /// <summary>
        /// Class để lưu thông tin về template bị xung đột
        /// </summary>
        private class ConflictingTemplateInfo
        {
            public Guid InvitationId { get; set; }
            public Guid TourDetailsId { get; set; }
            public string TourTitle { get; set; } = string.Empty;
            public ScheduleDay ScheduleDays { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
        }
    }
}
