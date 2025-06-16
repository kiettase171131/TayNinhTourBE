using AutoMapper;
using Microsoft.Extensions.Logging;
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

        public TourGuideInvitationService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<TourGuideInvitationService> logger,
            EmailSender emailSender) : base(mapper, unitOfWork)
        {
            _logger = logger;
            _emailSender = emailSender;
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
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        IsSuccess = false
                    };
                }

                // 2. Lấy tất cả TourGuides (users với role TourGuide)
                var tourGuides = await _unitOfWork.UserRepository.GetUsersByRoleAsync("TourGuide");
                if (!tourGuides.Any())
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy TourGuide nào trong hệ thống",
                        IsSuccess = false
                    };
                }

                // 3. Filter TourGuides có skills phù hợp
                var matchingGuides = new List<User>();
                foreach (var guide in tourGuides)
                {
                    // Lấy languages từ TourGuideApplication
                    var application = await GetTourGuideApplicationAsync(guide.Id);
                    if (application != null &&
                        SkillsMatchingUtility.MatchSkills(tourDetails.SkillsRequired, application.Languages))
                    {
                        matchingGuides.Add(guide);
                    }
                }

                if (!matchingGuides.Any())
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy TourGuide nào có skills phù hợp",
                        IsSuccess = false
                    };
                }

                // 4. Tạo invitations cho matching guides
                var invitationsCreated = 0;
                var expiresAt = DateTime.UtcNow.AddHours(24); // 24 hours expiry

                foreach (var guide in matchingGuides)
                {
                    // Check xem đã có invitation chưa
                    var existingInvitation = await _unitOfWork.TourGuideInvitationRepository
                        .HasPendingInvitationAsync(tourDetailsId, guide.Id);

                    if (!existingInvitation)
                    {
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

                        // Send invitation email
                        try
                        {
                            await _emailSender.SendTourGuideInvitationAsync(
                                guide.Email,
                                guide.Name,
                                tourDetails.Title,
                                tourDetails.CreatedBy.Name,
                                expiresAt,
                                invitation.Id.ToString()
                            );
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning("Failed to send invitation email to {GuideEmail}: {Error}",
                                guide.Email, emailEx.Message);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // 5. Update TourDetails status to AwaitingGuideAssignment
                tourDetails.Status = TourDetailsStatus.AwaitingGuideAssignment;
                tourDetails.UpdatedAt = DateTime.UtcNow;
                tourDetails.UpdatedById = createdById;
                await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created {Count} automatic invitations for TourDetails {TourDetailsId}",
                    invitationsCreated, tourDetailsId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Đã tạo {invitationsCreated} lời mời tự động cho các hướng dẫn viên phù hợp",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating automatic invitations for TourDetails {TourDetailsId}", tourDetailsId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi tạo lời mời: {ex.Message}",
                    IsSuccess = false
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
                        IsSuccess = false
                    };
                }

                // 2. Validate Guide exists and has TourGuide role
                var guide = await _unitOfWork.UserRepository.GetByIdAsync(guideId, new[] { "Role" });
                if (guide == null || guide.Role.Name != "TourGuide")
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourGuide không tồn tại hoặc không có quyền hướng dẫn viên",
                        IsSuccess = false
                    };
                }

                // 3. Check existing invitation
                var hasExisting = await _unitOfWork.TourGuideInvitationRepository
                    .HasPendingInvitationAsync(tourDetailsId, guideId);
                if (hasExisting)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "TourGuide này đã có lời mời pending cho tour này",
                        IsSuccess = false
                    };
                }

                // 4. Create manual invitation
                var invitation = new TourGuideInvitation
                {
                    Id = Guid.NewGuid(),
                    TourDetailsId = tourDetailsId,
                    GuideId = guideId,
                    InvitationType = InvitationType.Manual,
                    Status = InvitationStatus.Pending,
                    InvitedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(3), // 3 days for manual invitations
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.TourGuideInvitationRepository.AddAsync(invitation);
                await _unitOfWork.SaveChangesAsync();

                // 5. Send invitation email
                try
                {
                    await _emailSender.SendTourGuideInvitationAsync(
                        guide.Email,
                        guide.Name,
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

                _logger.LogInformation("Created manual invitation {InvitationId} for TourDetails {TourDetailsId}",
                    invitation.Id, tourDetailsId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã gửi lời mời thành công đến hướng dẫn viên",
                    IsSuccess = true
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
                    IsSuccess = false
                };
            }
        }

        public async Task<BaseResposeDto> AcceptInvitationAsync(Guid invitationId, Guid guideId)
        {
            try
            {
                _logger.LogInformation("Guide {GuideId} accepting invitation {InvitationId}", guideId, invitationId);

                // 1. Get invitation with details
                var invitation = await _unitOfWork.TourGuideInvitationRepository.GetWithDetailsAsync(invitationId);
                if (invitation == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Lời mời không tồn tại",
                        IsSuccess = false
                    };
                }

                // 2. Verify ownership
                if (invitation.GuideId != guideId)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền chấp nhận lời mời này",
                        IsSuccess = false
                    };
                }

                // 3. Check invitation status and expiry
                if (invitation.Status != InvitationStatus.Pending)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Lời mời này đã được xử lý hoặc đã hết hạn",
                        IsSuccess = false
                    };
                }

                if (invitation.ExpiresAt <= DateTime.UtcNow)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Lời mời đã hết hạn",
                        IsSuccess = false
                    };
                }

                // 4. Update invitation status
                invitation.Status = InvitationStatus.Accepted;
                invitation.RespondedAt = DateTime.UtcNow;
                invitation.UpdatedAt = DateTime.UtcNow;
                invitation.UpdatedById = guideId;

                await _unitOfWork.TourGuideInvitationRepository.UpdateAsync(invitation);

                // 5. Assign guide to TourOperation
                var tourDetails = invitation.TourDetails;
                if (tourDetails.TourOperation == null)
                {
                    // Create TourOperation if not exists
                    var tourOperation = new TourOperation
                    {
                        Id = Guid.NewGuid(),
                        TourDetailsId = tourDetails.Id,
                        GuideId = guideId,
                        Price = 0, // Will be set later
                        MaxGuests = 20, // Default value
                        Status = TourOperationStatus.Scheduled,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = guideId,
                        IsActive = true
                    };
                    await _unitOfWork.TourOperationRepository.AddAsync(tourOperation);
                }
                else
                {
                    tourDetails.TourOperation.GuideId = guideId;
                    tourDetails.TourOperation.UpdatedAt = DateTime.UtcNow;
                    tourDetails.TourOperation.UpdatedById = guideId;
                    await _unitOfWork.TourOperationRepository.UpdateAsync(tourDetails.TourOperation);
                }

                // 6. Update TourDetails status to AwaitingAdminApproval
                tourDetails.Status = TourDetailsStatus.AwaitingAdminApproval;
                tourDetails.UpdatedAt = DateTime.UtcNow;
                tourDetails.UpdatedById = guideId;
                await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);

                // 7. Expire all other pending invitations for this TourDetails
                await _unitOfWork.TourGuideInvitationRepository
                    .ExpireInvitationsForTourDetailsAsync(tourDetails.Id, invitationId);

                await _unitOfWork.SaveChangesAsync();

                // 8. Send confirmation emails
                try
                {
                    // To guide
                    await _emailSender.SendGuideAssignmentConfirmationAsync(
                        invitation.Guide.Email,
                        invitation.Guide.Name,
                        tourDetails.Title,
                        tourDetails.CreatedBy.Name
                    );

                    // To admin (get first admin)
                    var admins = await _unitOfWork.UserRepository.ListAdminsAsync();
                    if (admins.Any())
                    {
                        var admin = admins.First();
                        await _emailSender.SendAdminApprovalRequestAsync(
                            admin.Email,
                            admin.Name,
                            tourDetails.Title,
                            tourDetails.CreatedBy.Name,
                            invitation.Guide.Name
                        );
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning("Failed to send confirmation emails: {Error}", emailEx.Message);
                }

                _logger.LogInformation("Guide {GuideId} successfully accepted invitation {InvitationId}",
                    guideId, invitationId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã chấp nhận lời mời thành công. Đang chờ admin phê duyệt.",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting invitation {InvitationId} by guide {GuideId}",
                    invitationId, guideId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi chấp nhận lời mời: {ex.Message}",
                    IsSuccess = false
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
                        IsSuccess = false
                    };
                }

                // 2. Verify ownership
                if (invitation.GuideId != guideId)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền từ chối lời mời này",
                        IsSuccess = false
                    };
                }

                // 3. Check invitation status
                if (invitation.Status != InvitationStatus.Pending)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Lời mời này đã được xử lý",
                        IsSuccess = false
                    };
                }

                // 4. Update invitation status
                invitation.Status = InvitationStatus.Rejected;
                invitation.RespondedAt = DateTime.UtcNow;
                invitation.RejectionReason = rejectionReason;
                invitation.UpdatedAt = DateTime.UtcNow;
                invitation.UpdatedById = guideId;

                await _unitOfWork.TourGuideInvitationRepository.UpdateAsync(invitation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Guide {GuideId} successfully rejected invitation {InvitationId}",
                    guideId, invitationId);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã từ chối lời mời thành công",
                    IsSuccess = true
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
                    IsSuccess = false
                };
            }
        }

        public async Task<MyInvitationsResponseDto> GetMyInvitationsAsync(Guid guideId, InvitationStatus? status = null)
        {
            try
            {
                var invitations = await _unitOfWork.TourGuideInvitationRepository.GetByGuideAsync(guideId, status);

                // Map to DTOs (simplified for now)
                var invitationDtos = invitations.Select(inv => new
                {
                    Id = inv.Id,
                    TourTitle = inv.TourDetails.Title,
                    TourCompany = inv.CreatedBy.Name,
                    InvitationType = inv.InvitationType.ToString(),
                    Status = inv.Status.ToString(),
                    InvitedAt = inv.InvitedAt,
                    ExpiresAt = inv.ExpiresAt,
                    RespondedAt = inv.RespondedAt
                }).ToList();

                return new MyInvitationsResponseDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách lời mời thành công",
                    IsSuccess = true,
                    Invitations = new List<TourGuideInvitationDto>(), // TODO: Map properly
                    Statistics = new InvitationStatisticsDto()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitations for guide {GuideId}", guideId);
                return new MyInvitationsResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    IsSuccess = false,
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

                var invitationDtos = invitations.Select(inv => new
                {
                    Id = inv.Id,
                    GuideName = inv.Guide.Name,
                    GuideEmail = inv.Guide.Email,
                    InvitationType = inv.InvitationType.ToString(),
                    Status = inv.Status.ToString(),
                    InvitedAt = inv.InvitedAt,
                    ExpiresAt = inv.ExpiresAt,
                    RespondedAt = inv.RespondedAt,
                    RejectionReason = inv.RejectionReason
                }).ToList();

                return new TourDetailsInvitationsResponseDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách lời mời thành công",
                    IsSuccess = true,
                    TourDetails = new TourDetailsBasicDto(), // TODO: Map properly
                    Invitations = new List<TourGuideInvitationDto>(),
                    Statistics = new InvitationStatisticsDto()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitations for TourDetails {TourDetailsId}", tourDetailsId);
                return new TourDetailsInvitationsResponseDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    IsSuccess = false,
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
                    var hasAcceptedInvitation = await _unitOfWork.TourGuideInvitationRepository
                        .GetByTourDetailsAsync(tourDetails.Id);

                    if (!hasAcceptedInvitation.Any(inv => inv.Status == InvitationStatus.Accepted))
                    {
                        tourDetails.Status = TourDetailsStatus.AwaitingGuideAssignment;
                        tourDetails.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);
                        count++;
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
                        IsSuccess = false,
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
                        Id = invitation.Guide.Id,
                        Name = invitation.Guide.Name,
                        Email = invitation.Guide.Email
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
                    IsSuccess = true,
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
                    IsSuccess = false,
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
                        IsSuccess = false
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

                // TODO: Add more validations (conflicts, availability, etc.)

                return new BaseResposeDto
                {
                    StatusCode = validationErrors.Any() ? 400 : 200,
                    Message = validationErrors.Any() ? "Validation failed" : "Có thể chấp nhận lời mời",
                    IsSuccess = !validationErrors.Any(),
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
                    IsSuccess = false
                };
            }
        }

        /// <summary>
        /// Helper method để lấy TourGuideApplication của một user
        /// TODO: Implement when TourGuideApplicationRepository is available
        /// </summary>
        private async Task<TourGuideApplication?> GetTourGuideApplicationAsync(Guid userId)
        {
            try
            {
                // TODO: Implement when TourGuideApplicationRepository is added to UnitOfWork
                // var applications = await _unitOfWork.TourGuideApplicationRepository
                //     .GetAllAsync(app => app.UserId == userId && app.Status == TourGuideApplicationStatus.Approved);
                // return applications.FirstOrDefault();

                await Task.CompletedTask; // Placeholder
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
