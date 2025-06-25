using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Booking;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Booking;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region User Mapping
            CreateMap<RequestRegisterDto, User>();
            CreateMap<User, UserCmsDto>();
            #endregion

            #region Tour Mapping
            CreateMap<RequestCreateTourCmsDto, Tour>().ForMember(dest => dest.Images, otp => otp.Ignore());
            CreateMap<Tour, TourDto>().ForMember(dest => dest.Images, otp => otp.MapFrom(src => src.Images.Select(x => x.Url).ToList()));
            #endregion

            #region Blog Mapping
            CreateMap<Blog, BlogDto>().ForMember(dest => dest.ImageUrl, otp => otp.MapFrom(src => src.BlogImages.Select(x => x.Url).ToList()));
            #endregion

            #region TourDetails Mapping
            // Request to Entity mappings
            CreateMap<RequestCreateTourDetailDto, TourDetails>();
            CreateMap<RequestUpdateTourDetailDto, TourDetails>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Entity to DTO mappings
            CreateMap<TourDetails, TourDetailDto>()
                .ForMember(dest => dest.TourTemplateName, opt => opt.MapFrom(src => src.TourTemplate.Title))
                .ForMember(dest => dest.AssignedSlotsCount, opt => opt.MapFrom(src => src.AssignedSlots.Count))
                .ForMember(dest => dest.TimelineItemsCount, opt => opt.MapFrom(src => src.Timeline.Count))
                .ForMember(dest => dest.InvitedShopsCount, opt => opt.MapFrom(src => src.InvitedSpecialtyShops.Count))
                .ForMember(dest => dest.Timeline, opt => opt.MapFrom(src => src.Timeline))
                .ForMember(dest => dest.TourOperation, opt => opt.MapFrom(src => src.TourOperation))
                .ForMember(dest => dest.InvitedSpecialtyShops, opt => opt.MapFrom(src => src.InvitedSpecialtyShops));

            CreateMap<TimelineItem, TimelineItemDto>()
                .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => src.CheckInTime.ToString(@"hh\:mm")));
            // TODO: Update SpecialtyShop mapping after DTO updates
            // .ForMember(dest => dest.SpecialtyShop, opt => opt.MapFrom(src => src.SpecialtyShop));

            // TourDetailsSpecialtyShop mappings
            CreateMap<TourDetailsSpecialtyShop, TourDetailsSpecialtyShopDto>()
                .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => GetShopInvitationStatusText(src.Status)))
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.ExpiresAt < DateTime.UtcNow && src.Status == ShopInvitationStatus.Pending))
                .ForMember(dest => dest.DaysRemaining, opt => opt.MapFrom(src => src.Status == ShopInvitationStatus.Pending ? Math.Max(0, (int)(src.ExpiresAt - DateTime.UtcNow).TotalDays) : 0))
                .ForMember(dest => dest.SpecialtyShop, opt => opt.MapFrom(src => src.SpecialtyShop));

            CreateMap<SpecialtyShop, SpecialtyShopSummaryDto>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<TourOperation, TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany.TourOperationDto>();

            // Entity to DTO mappings for direct responses (simpler approach)
            // Service layer will handle response construction manually for better control
            #endregion

            // Shop mappings removed - merged into SpecialtyShop

            #region Timeline Mapping
            // TODO: Update timeline mapping for new design
            // CreateMap<TourTemplate, TimelineDto>() - Will be handled in service manually
            #endregion
            #region Product Mapping
            CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ProductImages.Select(i => i.Url).ToList()));


            #endregion

            #region SpecialtyShop Mapping
            CreateMap<SpecialtyShop, SpecialtyShopResponseDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.Avatar))
                .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.User.Role.Name));


            // All Shop mappings removed - using SpecialtyShop only
            #endregion

            #region SpecialtyShopApplication Mapping
            CreateMap<SpecialtyShopApplication, SpecialtyShopApplicationDto>()
                .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.ProcessedByInfo, opt => opt.MapFrom(src => src.ProcessedBy));

            CreateMap<SpecialtyShopApplication, SpecialtyShopApplicationSummaryDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<User, UserSummaryDto>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
            #endregion

            #region TourGuideApplication Mapping
            CreateMap<TourGuideApplication, TourGuideApplicationDto>()
                .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.ProcessedByInfo, opt => opt.MapFrom(src => src.ProcessedBy))
                .ForMember(dest => dest.CurriculumVitaeUrl, opt => opt.MapFrom(src => src.CurriculumVitae))
                .ForMember(dest => dest.CvOriginalFileName, opt => opt.MapFrom(src => src.CvOriginalFileName))
                .ForMember(dest => dest.CvFileSize, opt => opt.MapFrom(src => src.CvFileSize))
                .ForMember(dest => dest.CvContentType, opt => opt.MapFrom(src => src.CvContentType))
                .ForMember(dest => dest.CvFilePath, opt => opt.MapFrom(src => src.CvFilePath));

            CreateMap<TourGuideApplication, TourGuideApplicationSummaryDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));
            #endregion

            #region TourOperation Mapping
            CreateMap<RequestCreateOperationDto, TourOperation>()
                .ForMember(dest => dest.MaxGuests, opt => opt.MapFrom(src => src.MaxSeats));
            CreateMap<RequestUpdateOperationDto, TourOperation>()
                .ForMember(dest => dest.MaxGuests, opt => opt.MapFrom(src => src.MaxSeats))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TourOperation, TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>()
                .ForMember(dest => dest.MaxSeats, opt => opt.MapFrom(src => src.MaxGuests))
                .ForMember(dest => dest.GuideName, opt => opt.MapFrom(src => src.Guide != null ? src.Guide.Name : null))
                .ForMember(dest => dest.GuidePhone, opt => opt.MapFrom(src => src.Guide != null ? src.Guide.PhoneNumber : null));
            CreateMap<TourOperation, OperationSummaryDto>()
                .ForMember(dest => dest.TourDate, opt => opt.Ignore()) // Will set in service
                .ForMember(dest => dest.GuideName, opt => opt.MapFrom(src => src.Guide != null ? src.Guide.Name : null))
                .ForMember(dest => dest.GuideEmail, opt => opt.MapFrom(src => src.Guide != null ? src.Guide.Email : null))
                .ForMember(dest => dest.GuidePhoneNumber, opt => opt.MapFrom(src => src.Guide != null ? src.Guide.PhoneNumber : null))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.CurrentBookings, opt => opt.Ignore()); // Will set in service
            CreateMap<TourOperation, TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany.TourOperationDto>()
                .ForMember(dest => dest.GuideName, opt => opt.MapFrom(src => src.Guide != null ? src.Guide.Name : null))
                .ForMember(dest => dest.GuideEmail, opt => opt.MapFrom(src => src.Guide != null ? src.Guide.Email : null))
                .ForMember(dest => dest.GuidePhoneNumber, opt => opt.MapFrom(src => src.Guide != null ? src.Guide.PhoneNumber : null))
                .ForMember(dest => dest.CurrentBookings, opt => opt.Ignore()) // Will set in service
                .ForMember(dest => dest.StatusName, opt => opt.Ignore()); // Will set in service
            #endregion

            #region TourBooking Mapping
            CreateMap<RequestCreateBookingDto, TourBooking>()
                .ForMember(dest => dest.NumberOfGuests, opt => opt.MapFrom(src => src.TotalGuests))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.BookingDate, opt => opt.Ignore())
                .ForMember(dest => dest.BookingCode, opt => opt.Ignore());
            CreateMap<TourBooking, ResponseBookingDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : "N/A"))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.TotalGuests, opt => opt.MapFrom(src => src.NumberOfGuests))
                .ForMember(dest => dest.StatusName, opt => opt.Ignore()) // Will set in service
                .ForMember(dest => dest.TourOperation, opt => opt.Ignore()); // Will set in service
            #endregion
        }

        /// <summary>
        /// Helper method để convert TourSlotStatus sang tên tiếng Việt
        /// </summary>
        private static string GetTourSlotStatusName(TourSlotStatus status)
        {
            return status switch
            {
                TourSlotStatus.Available => "Có sẵn",
                TourSlotStatus.FullyBooked => "Đã đầy",
                TourSlotStatus.Cancelled => "Đã hủy",
                TourSlotStatus.Completed => "Đã hoàn thành",
                TourSlotStatus.InProgress => "Đang thực hiện",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Helper method để convert TourOperationStatus sang tên tiếng Việt
        /// </summary>
        private static string GetTourOperationStatusName(TourOperationStatus status)
        {
            return status switch
            {
                TourOperationStatus.Scheduled => "Đã lên lịch",
                TourOperationStatus.InProgress => "Đang thực hiện",
                TourOperationStatus.Completed => "Đã hoàn thành",
                TourOperationStatus.Cancelled => "Đã hủy",
                TourOperationStatus.Postponed => "Đã hoãn",
                TourOperationStatus.PendingConfirmation => "Chờ xác nhận",
                _ => status.ToString()
            };
        }

        private static string GetShopInvitationStatusText(ShopInvitationStatus status)
        {
            return status switch
            {
                ShopInvitationStatus.Pending => "Chờ phản hồi",
                ShopInvitationStatus.Accepted => "Đã chấp nhận",
                ShopInvitationStatus.Declined => "Đã từ chối",
                ShopInvitationStatus.Expired => "Đã hết hạn",
                ShopInvitationStatus.Cancelled => "Đã hủy",
                _ => status.ToString()
            };
        }
    }
}
