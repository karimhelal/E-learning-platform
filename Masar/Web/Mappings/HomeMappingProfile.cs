using AutoMapper;
using System.Linq;
using Web.ViewModels.Home;
using Core.Entities;

namespace Web.Mappings;

public class HomeMappingProfile : Profile
{
    public HomeMappingProfile()
    {
        // ================= Browse Tracks =================
        CreateMap<Track, HomeTrackCardViewModel>()
            .ForMember(dest => dest.TrackId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? "Explore this learning track"))
            .ForMember(dest => dest.CoursesCount, opt => opt.MapFrom(src => src.TrackCourses != null ? src.TrackCourses.Count : 0))
            .ForMember(dest => dest.StudentsCount, opt => opt.MapFrom(src => src.Enrollments != null ? src.Enrollments.Count : 0))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Categories != null && src.Categories.Any() ? src.Categories.First().Name : "Learning Track"))
            .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => GetCategoryIcon(src.Categories != null && src.Categories.Any() ? src.Categories.First().Name : null)))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => "Beginner"))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => 4.8m))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.TrackCourses != null ? src.TrackCourses.SelectMany(tc => tc.Course != null && tc.Course.Categories != null ? tc.Course.Categories : new List<Category>()).Select(c => c.Name).Distinct().Take(4).ToList() : new List<string>()))
            .ForMember(dest => dest.CoursesPreview, opt => opt.MapFrom(src => src.TrackCourses != null ? src.TrackCourses.Take(3) : null));

        // ================= Featured Tracks  =================
        CreateMap<Track, HomeFeaturedTrackViewModel>()
            .ForMember(dest => dest.TrackId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => src.Categories != null && src.Categories.Any() ? src.Categories.First().Name : "Learning Track"))
            .ForMember(dest => dest.IconClass, opt => opt.MapFrom(src => GetCategoryIcon(src.Categories != null && src.Categories.Any() ? src.Categories.First().Name : null)))
            .ForMember(dest => dest.CoursesCount, opt => opt.MapFrom(src => src.TrackCourses != null ? src.TrackCourses.Count : 0))
            .ForMember(dest => dest.StudentsCount, opt => opt.MapFrom(src => src.Enrollments != null ? src.Enrollments.Count : 0));

        CreateMap<Track_Course, HomeCoursePreviewViewModel>()
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Course != null ? src.Course.Id : 0))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : ""))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Course != null ? src.Course.Level.ToString() : "Beginner"));
    }

    private static string GetCategoryIcon(string? categoryName)
    {
        return categoryName?.ToLower() switch
        {
            "web development" => "fa-laptop-code",
            "data science" => "fa-brain",
            "mobile development" => "fa-mobile-alt",
            "design" => "fa-palette",
            "cloud computing" => "fa-cloud",
            "devops" => "fa-server",
            _ => "fa-book"
        };
    }
}