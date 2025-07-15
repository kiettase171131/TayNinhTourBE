using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.Mapping
{
    /// <summary>
    /// AutoMapper profile cho TourTemplate entity
    /// Định nghĩa mapping giữa entities và DTOs
    /// </summary>
    public class TourTemplateMappingProfile : Profile
    {
        public TourTemplateMappingProfile()
        {
            // Mapping từ RequestCreateTourTemplateDto sang TourTemplate
            CreateMap<RequestCreateTourTemplateDto, TourTemplate>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore());

            // Mapping từ RequestUpdateTourTemplateDto sang TourTemplate (partial update)
            CreateMap<RequestUpdateTourTemplateDto, TourTemplate>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Mapping từ TourTemplate sang TourTemplateDto (response)
            CreateMap<TourTemplate, TourTemplateDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.User.Name : null))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy != null ? src.UpdatedBy.User.Name : null))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(i => i.Url).ToList() : new List<string>()))
                .ForMember(dest => dest.TemplateType, opt => opt.MapFrom(src => src.TemplateType.ToString()))
                .ForMember(dest => dest.ScheduleDays, opt => opt.MapFrom(src => src.ScheduleDays.ToString()));

            // Mapping từ TourTemplate sang TourTemplateDetailDto (detailed response)
            CreateMap<TourTemplate, TourTemplateDetailDto>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(i => i.Url).ToList() : new List<string>()));

            // Mapping từ TourTemplate sang TourTemplateSummaryDto (summary for listing)
            CreateMap<TourTemplate, TourTemplateSummaryDto>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(i => i.Url).ToList() : new List<string>()))
                .ForMember(dest => dest.TemplateType, opt => opt.MapFrom(src => src.TemplateType.ToString()));
        }
    }
}
