using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourFeedback;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class TourFeedbackService : ITourFeedbackService
    {
        private readonly IMapper _mapper;
        private readonly ITourFeedbackRepository _tourFeedback;
        private readonly ITourBookingRepository _bookingRepo;
        private readonly ITourGuideRepository _tourGuideRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITourGuideInvitationRepository _invitationRepo;
        private readonly ITourOperationRepository _operationRepo;

        public TourFeedbackService(IMapper mapper, ITourFeedbackRepository tourFeedback, ITourBookingRepository bookingRepo, ITourGuideRepository tourGuideRepository,IUnitOfWork unitOfWork, ITourGuideInvitationRepository invitationRepo,ITourOperationRepository tourOperationRepository)
        {
            _mapper = mapper;
            _tourFeedback = tourFeedback;
            _bookingRepo = bookingRepo;
            _tourGuideRepository = tourGuideRepository;
            _unitOfWork = unitOfWork;
            _invitationRepo = invitationRepo;
            _operationRepo = tourOperationRepository;
        }
        public async Task<TourFeedbackDto> CreateAsync(Guid userId, Guid bookingId, int tourRating,string? tourComment,int? guideRating,string? guideComment)
        {
            await using var tx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1) Lấy booking + slot + operation bằng repository
                var booking = await _bookingRepo.GetAsync(
                    b => b.Id == bookingId,
                    include: new[] { nameof(TourBooking.TourSlot), nameof(TourBooking.TourOperation) }
                ) ?? throw new InvalidOperationException("Booking không tồn tại.");

                // 2) Các điều kiện nghiệp vụ
                if (booking.UserId != userId)
                    throw new UnauthorizedAccessException("Bạn không thể feedback booking của người khác.");

                if (booking.TourSlotId == null)
                    throw new InvalidOperationException("Booking chưa gắn slot, không thể feedback.");

                if (booking.TourSlot!.Status != TourSlotStatus.Completed)
                    throw new InvalidOperationException("Chỉ feedback khi slot đã hoàn thành.");

                if (booking.Status != BookingStatus.Completed)
                    throw new InvalidOperationException("Chỉ feedback khi booking đã hoàn thành.");

                if (await _tourFeedback.ExistsForBookingAsync(booking.Id))
                    throw new InvalidOperationException("Booking này đã được feedback.");

                // 3) Xác định Guide để chấm (nếu có GuideRating)
                Guid? tourGuideId = null;
                if (guideRating.HasValue)
                {
                    // Ưu tiên guide được assign ở Operation
                    var assignedGuideId = booking.TourOperation?.TourGuideId;

                    // Fallback: tìm invitation Accepted gần nhất theo TourDetails của slot
                    if (assignedGuideId == null && booking.TourSlot?.TourDetailsId is Guid detailsId)
                    {
                        var accepted = await _invitationRepo.GetLatestAcceptedByTourDetailsIdAsync(detailsId);
                        assignedGuideId = accepted?.GuideId;
                    }

                    if (assignedGuideId == null)
                        throw new InvalidOperationException("Slot này không có tour guide để chấm.");

                    tourGuideId = assignedGuideId;
                }

                // 4) Tạo feedback (repo)
                var feedback = new TourFeedback
                {
                    Id = Guid.NewGuid(),
                    TourBookingId = booking.Id,
                    TourSlotId = booking.TourSlotId!.Value,
                    UserId = userId,
                    TourRating = tourRating,
                    TourComment = tourComment,
                    GuideRating = guideRating,
                    GuideComment = guideComment,
                    TourGuideId = tourGuideId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _tourFeedback.AddAsync(feedback);
                await _tourFeedback.SaveChangesAsync();

                // 5) Cập nhật rating Guide (nếu có)
                if (tourGuideId.HasValue && guideRating.HasValue)
                {
                    var guide = await _tourGuideRepository.GetByIdAsync(tourGuideId.Value, include: null)
                               ?? throw new InvalidOperationException("Tour guide không tồn tại.");

                    var newCount = guide.RatingsCount + 1;
                    guide.Rating = ((guide.Rating * guide.RatingsCount) + guideRating.Value) / newCount;
                    guide.RatingsCount = newCount;

                    await _tourGuideRepository.UpdateAsync(guide);
                }

                await _unitOfWork.SaveChangesAsync();
                await tx.CommitAsync();

                return _mapper.Map<TourFeedbackDto>(feedback);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        // GET: feedback theo Slot
        public async Task<TourFeedbackResponse> GetTourFeedbacksBySlotAsync(Guid slotId,int? pageIndex, int? pageSize,int? minTourRating = null,int? maxTourRating = null,bool? onlyWithGuideRating = null)
        {
            var include = new[] { "TourGuide" };
            var pageIdx = pageIndex ?? Constants.PageIndexDefault;
            var pageSz = pageSize ?? Constants.PageSizeDefault;
            if (minTourRating.HasValue && maxTourRating.HasValue && minTourRating > maxTourRating)
            {
                return new TourFeedbackResponse
                {
                    StatusCode = 400,
                    success = false,
                    Message = "minTourRating phải ≤ maxTourRating",
                    Data = new List<TourFeedbackDto>()
                };
            }

            var predicate = PredicateBuilder.New<TourFeedback>(x => true);
            predicate = predicate.And(x => x.TourSlotId == slotId);

            if (minTourRating.HasValue) predicate = predicate.And(x => x.TourRating >= minTourRating.Value);
            if (maxTourRating.HasValue) predicate = predicate.And(x => x.TourRating <= maxTourRating.Value);
            if (onlyWithGuideRating == true) predicate = predicate.And(x => x.GuideRating != null);
            

            var q = _tourFeedback.GetQueryable().Include(x => x.TourGuide).Where(predicate);

            var total = await q.CountAsync();

            var items = await q.OrderByDescending(x => x.CreatedAt)
                               .Skip((pageIdx - 1) * pageSz)
                               .Take(pageSz)
                               .ToListAsync();

            var dto = _mapper.Map<List<TourFeedbackDto>>(items);

            return new TourFeedbackResponse
            {
                StatusCode = 200,
                success = true,
                Message = "List Feedback Successfully",
                Data = dto,
                TotalRecord = total,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling((double)total / pageSz),
                PageIndex = pageIdx,
                PageSize = pageSz
            };
        }
    }
}
