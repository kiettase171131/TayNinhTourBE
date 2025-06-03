using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Entities;

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
            CreateMap<Tour, TourDto >().ForMember(dest => dest.Images, otp => otp.MapFrom(src => src.Images.Select(x => x.Url).ToList()));
            #endregion

            #region Blog Mapping
            CreateMap<Blog, BlogDto>().ForMember(dest => dest.ImageUrl, otp => otp.MapFrom(src => src.BlogImages.Select(x => x.Url).ToList()));
            #endregion

            #region TourDetails Mapping
            CreateMap<RequestCreateTourDetailDto, TourDetails>();
            CreateMap<RequestUpdateTourDetailDto, TourDetails>();
            CreateMap<TourDetails, TourDetailDto>();
            #endregion

            #region Shop Mapping
            CreateMap<Shop, ShopDto>();
            #endregion

            #region Timeline Mapping
            CreateMap<TourTemplate, TimelineDto>()
                .ForMember(dest => dest.TourTemplateId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TourTemplateTitle, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.TourDetails, opt => opt.MapFrom(src => src.TourDetails.OrderBy(td => td.SortOrder)));
            #endregion
        }
    }
}
