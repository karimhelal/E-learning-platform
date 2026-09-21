using AutoMapper;
using Web.ViewModels.Public;
using BLL.DTOs.Instructor;
using System.Linq;

namespace Web.Mappings;

public class PublicMappingProfile : Profile
{
    public PublicMappingProfile()
    {
        CreateMap<PublicInstructorProfileDto, PublicInstructorProfileDataViewModel>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => $"@{src.FirstName.ToLower()}_{src.LastName.ToLower()}"))
            .ForMember(dest => dest.Initials, opt => opt.MapFrom(src => $"{src.FirstName[0]}{src.LastName[0]}".ToUpper()))
            .ForMember(dest => dest.JoinedDate, opt => opt.MapFrom(src => src.JoinedDate.ToString("MMMM yyyy")))
            .ForMember(dest => dest.Stats, opt => opt.MapFrom(src => src));


        CreateMap<PublicInstructorProfileDto, PublicInstructorStatsViewModel>();

        CreateMap<PublicInstructorCourseDto, PublicInstructorCourseViewModel>()
            .ForMember(dest => dest.LevelBadgeClass, opt => opt.MapFrom(src => src.Level.ToLower()))
            .ForMember(dest => dest.CategoryBadgeClass, opt => opt.MapFrom(src => GetCategoryBadgeClass(src.CategoryName)));
    }

    private static string GetCategoryBadgeClass(string categoryName)
    {
        return categoryName.ToLower() switch
        {
            "web development" => "badge-purple",
            "data science" => "badge-cyan",
            "mobile development" => "badge-green",
            "design" => "badge-pink",
            _ => "badge-purple"
        };
    }
}