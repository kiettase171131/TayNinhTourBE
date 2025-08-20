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

        public TourFeedbackService(IMapper mapper, ITourFeedbackRepository tourFeedback, ITourBookingRepository bookingRepo, ITourGuideRepository tourGuideRepository, IUnitOfWork unitOfWork, ITourGuideInvitationRepository invitationRepo, ITourOperationRepository tourOperationRepository)
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
        public async Task<TourFeedbackResponse> GetTourFeedbacksBySlotAsync(Guid slotId, int? pageIndex, int? pageSize, int? minTourRating = null, int? maxTourRating = null, bool? onlyWithGuideRating = null)
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

        /// <summary>
        /// Lấy danh sách feedback của user
        /// </summary>
        public async Task<TourFeedbackResponse> GetUserFeedbacksAsync(Guid userId, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var include = new[] { "TourGuide", "TourBooking", "TourBooking.TourOperation", "TourBooking.TourOperation.TourDetails" };

                var predicate = PredicateBuilder.New<TourFeedback>(x => true);
                predicate = predicate.And(x => x.UserId == userId);

                var query = _tourFeedback.GetQueryable()
                    .Include(x => x.TourGuide)
                    .Include(x => x.TourBooking)
                        .ThenInclude(tb => tb.TourOperation)
                            .ThenInclude(to => to.TourDetails)
                    .Where(predicate);

                var total = await query.CountAsync();

                var items = await query.OrderByDescending(x => x.CreatedAt)
                                   .Skip((pageIndex - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

                var dto = _mapper.Map<List<TourFeedbackDto>>(items);

                return new TourFeedbackResponse
                {
                    StatusCode = 200,
                    success = true,
                    Message = "Lấy danh sách feedback thành công",
                    Data = dto,
                    TotalRecord = total,
                    TotalCount = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize),
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new TourFeedbackResponse
                {
                    StatusCode = 500,
                    success = false,
                    Message = $"Lỗi khi lấy danh sách feedback: {ex.Message}",
                    Data = new List<TourFeedbackDto>()
                };
            }
        }

        /// <summary>
        /// Cập nhật feedback của user (chỉ trong thời gian cho phép)
        /// </summary>
        public async Task<ResponseCreateFeedBackDto> UpdateFeedbackAsync(Guid feedbackId, Guid userId, UpdateTourFeedbackRequest request)
        {
            return await _unitOfWork.ExecuteInStrategyAsync(async () =>
            {
                await using var tx = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Lấy feedback hiện tại
                    var feedback = await _tourFeedback.GetAsync(f => f.Id == feedbackId && f.UserId == userId);
                    if (feedback == null)
                    {
                        return new ResponseCreateFeedBackDto
                        {
                            StatusCode = 404,
                            success = false,
                            Message = "Không tìm thấy feedback hoặc bạn không có quyền chỉnh sửa"
                        };
                    }

                    // Kiểm tra thời gian cho phép chỉnh sửa (7 ngày sau khi tạo)
                    var editDeadline = feedback.CreatedAt.AddDays(7);
                    if (DateTime.UtcNow > editDeadline)
                    {
                        return new ResponseCreateFeedBackDto
                        {
                            StatusCode = 400,
                            success = false,
                            Message = "Đã quá thời hạn chỉnh sửa feedback (7 ngày)"
                        };
                    }

                    // Cập nhật thông tin
                    if (request.TourRating.HasValue)
                        feedback.TourRating = request.TourRating.Value;

                    if (request.TourComment != null)
                        feedback.TourComment = request.TourComment;

                    if (request.GuideRating.HasValue)
                        feedback.GuideRating = request.GuideRating.Value;

                    if (request.GuideComment != null)
                        feedback.GuideComment = request.GuideComment;

                    feedback.UpdatedAt = DateTime.UtcNow;

                    await _tourFeedback.UpdateAsync(feedback);
                    await _unitOfWork.SaveChangesAsync();
                    await tx.CommitAsync();

                    return new ResponseCreateFeedBackDto
                    {
                        StatusCode = 200,
                        success = true,
                        Message = "Cập nhật feedback thành công",
                        FeedBackId = feedback.Id,
                        BookingId = feedback.TourBookingId,
                        SlotId = feedback.TourSlotId
                    };
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return new ResponseCreateFeedBackDto
                    {
                        StatusCode = 500,
                        success = false,
                        Message = $"Lỗi khi cập nhật feedback: {ex.Message}"
                    };
                }
            });
        }

        /// <summary>
        /// Xóa feedback của user (chỉ trong thời gian cho phép)
        /// </summary>
        public async Task<ResponseCreateFeedBackDto> DeleteFeedbackAsync(Guid feedbackId, Guid userId)
        {
            return await _unitOfWork.ExecuteInStrategyAsync(async () =>
            {
                await using var tx = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Lấy feedback hiện tại
                    var feedback = await _tourFeedback.GetAsync(f => f.Id == feedbackId && f.UserId == userId);
                    if (feedback == null)
                    {
                        return new ResponseCreateFeedBackDto
                        {
                            StatusCode = 404,
                            success = false,
                            Message = "Không tìm thấy feedback hoặc bạn không có quyền xóa"
                        };
                    }

                    // Kiểm tra thời gian cho phép xóa (24 giờ sau khi tạo)
                    var deleteDeadline = feedback.CreatedAt.AddHours(24);
                    if (DateTime.UtcNow > deleteDeadline)
                    {
                        return new ResponseCreateFeedBackDto
                        {
                            StatusCode = 400,
                            success = false,
                            Message = "Đã quá thời hạn xóa feedback (24 giờ)"
                        };
                    }

                    await _tourFeedback.DeleteAsync(feedback.Id);
                    await _unitOfWork.SaveChangesAsync();
                    await tx.CommitAsync();

                    return new ResponseCreateFeedBackDto
                    {
                        StatusCode = 200,
                        success = true,
                        Message = "Xóa feedback thành công"
                    };
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return new ResponseCreateFeedBackDto
                    {
                        StatusCode = 500,
                        success = false,
                        Message = $"Lỗi khi xóa feedback: {ex.Message}"
                    };
                }
            });
        }
    }
}
