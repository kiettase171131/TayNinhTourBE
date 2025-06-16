using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop;
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
                .ForMember(dest => dest.Timeline, opt => opt.MapFrom(src => src.Timeline))
                .ForMember(dest => dest.TourOperation, opt => opt.MapFrom(src => src.TourOperation));

            CreateMap<TimelineItem, TimelineItemDto>()
                .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => src.CheckInTime.ToString(@"hh\:mm")));
            // TODO: Update SpecialtyShop mapping after DTO updates
            // .ForMember(dest => dest.SpecialtyShop, opt => opt.MapFrom(src => src.SpecialtyShop));

            CreateMap<TourOperation, TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany.TourOperationDto>();

            // Entity to DTO mappings for direct responses (simpler approach)
            // Service layer will handle response construction manually for better control
            #endregion

            // Shop mappings removed - merged into SpecialtyShop

            #region Timeline Mapping
            // TODO: Update timeline mapping for new design
            // CreateMap<TourTemplate, TimelineDto>() - Will be handled in service manually
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

            #region TourGuideApplication Mapping - TEMPORARILY COMMENTED OUT
            // CreateMap<TourGuideApplication, TourGuideApplicationDto>()
            //     .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => src.User))
            //     .ForMember(dest => dest.ProcessedByInfo, opt => opt.MapFrom(src => src.ProcessedBy));

            // CreateMap<TourGuideApplication, TourGuideApplicationSummaryDto>()
            //     .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            //     .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));
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
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));
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
    }
}
