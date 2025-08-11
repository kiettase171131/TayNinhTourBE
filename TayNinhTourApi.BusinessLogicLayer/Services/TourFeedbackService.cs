using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourFeedback;
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
        public async Task<ResponseCreateFeedBackDto> CreateAsync(Guid userId, CreateTourFeedbackRequest req)
        {
            // Toàn bộ đơn vị công việc chạy trong execution strategy (retry-safe)
            return await _unitOfWork.ExecuteInStrategyAsync(async () =>
            {
                await using var tx = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // 1) Lấy booking + include
                    var booking = await _bookingRepo.GetAsync(
                        b => b.Id == req.BookingId,
                        include: new[] { nameof(TourBooking.TourSlot), nameof(TourBooking.TourOperation) }
                    ) ?? throw new InvalidOperationException("Booking không tồn tại.");

                    // 2) Điều kiện nghiệp vụ
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

                    // 3) Xác định Guide nếu có GuideRating
                    Guid? tourGuideId = null;
                    if (req.GuideRating.HasValue)
                    {
                        var assignedGuideId = booking.TourOperation?.TourGuideId;

                        if (assignedGuideId == null && booking.TourSlot?.TourDetailsId is Guid detailsId)
                        {
                            var accepted = await _invitationRepo.GetLatestAcceptedByTourDetailsIdAsync(detailsId);
                            assignedGuideId = accepted?.GuideId;
                        }

                        if (assignedGuideId == null)
                        {
                            // không ghi gì vào DB nên có thể return sớm; tx sẽ tự rollback khi dispose
                            return new ResponseCreateFeedBackDto
                            {
                                StatusCode = 400,
                                success = false,
                                Message = "Không tìm thấy Tour Guide để chấm điểm."
                            };
                        }

                        tourGuideId = assignedGuideId;
                    }

                    // 4) Tạo feedback
                    var feedback = new TourFeedback
                    {
                        Id = Guid.NewGuid(),
                        TourBookingId = booking.Id,
                        TourSlotId = booking.TourSlotId!.Value,
                        UserId = userId,
                        TourRating = req.TourRating,
                        TourComment = req.TourComment,
                        GuideRating = req.GuideRating,
                        GuideComment = req.GuideComment,
                        TourGuideId = tourGuideId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.TourFeedbackRepository.AddAsync(feedback);

                    // 5) Cập nhật rating Guide (nếu có)
                    if (tourGuideId.HasValue && req.GuideRating.HasValue)
                    {
                        var guide = await _unitOfWork.TourGuideRepository.GetByIdAsync(tourGuideId.Value, include: null)
                                   ?? throw new InvalidOperationException("Tour guide không tồn tại.");

                        var newCount = guide.RatingsCount + 1;
                        guide.Rating = ((guide.Rating * guide.RatingsCount) + req.GuideRating.Value) / newCount;
                        guide.RatingsCount = newCount;

                        await _unitOfWork.TourGuideRepository.UpdateAsync(guide);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await tx.CommitAsync();

                    return new ResponseCreateFeedBackDto
                    {
                        StatusCode = 200,
                        FeedBackId = feedback.Id,
                        BookingId = booking.Id,
                        SlotId = booking.TourSlotId.Value,
                        success = true,
                        Message = "Feedback created successfully",
                    };
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
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
