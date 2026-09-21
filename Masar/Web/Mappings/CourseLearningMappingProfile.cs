using AutoMapper;
using BLL.DTOs.Classroom;
using Web.ViewModels.Classroom;
using System.Linq;

namespace Web.Mappings;

public class CourseLearningMappingProfile : Profile
{
    public CourseLearningMappingProfile()
    {
        // ================= Sidebar =================
        CreateMap<ModuleSidebarDto, ModuleSidebarViewModel>()
            .ForMember(dest => dest.FormattedModuleDuration, opt => opt.MapFrom(src => FormatDuration(src.TotalDurationInMinutes)));

        CreateMap<LessonSidebarDto, LessonSidebarViewModel>()
            .ForMember(dest => dest.FormattedLessonDuration, opt => opt.MapFrom(src => FormatDuration(src.TotalDurationInMinutes)));

        // ================= Main Content =================
        CreateMap<CourseOverviewStatsDto, ClassroomCourseOverviewStatsViewModel>()
            .ForMember(dest => dest.FormattedDuration, opt => opt.MapFrom(src => FormatDuration(src.TotalDurationInMinutes)))
            .ForMember(dest => dest.FormattedRatingAndReviews, opt => opt.MapFrom(src => $"{src.AverageRating:0.0}"))
            .ForMember(dest => dest.FormattedStudentsEnrolledCount, opt => opt.MapFrom(src => FormatCount(src.EnrolledStudentsCount, "Student")));

        CreateMap<CourseInstructorStatsDto, ClassroomCourseInstructorStatsViewModel>()
            .ForMember(dest => dest.FormattedRating, opt => opt.MapFrom(src => "4.3 Rating"))
            .ForMember(dest => dest.RoleTitle, opt => opt.MapFrom(src => "Senior Software Engineer @ Microsoft"))
            .ForMember(dest => dest.YearsOfExperience, opt => opt.MapFrom(src => src.YearsOfExperience.ToString() ?? "--"))
            .ForMember(dest => dest.FormattedCoursesCount, opt => opt.MapFrom(src => FormatCount(src.TotalCoursesCount, "Course")))
            .ForMember(dest => dest.FormattedStudentsTaughtCount, opt => opt.MapFrom(src => FormatCount(src.StudentsTaughtCount, "Student")));

        CreateMap<LessonResourceDto, LessonResourceViewModel>();
    }

    private static string FormatDuration(int durationInMinutes)
    {
        int h = durationInMinutes / 60;
        int m = durationInMinutes % 60;
        var parts = new System.Collections.Generic.List<string>();
        if (h > 0) parts.Add($"{h} {(h == 1 ? "hr" : "hrs")}");
        if (m > 0) parts.Add($"{m} {(m == 1 ? "min" : "mins")}");
        return parts.Count > 0 ? string.Join(" ", parts) : "--";
    }

    private static string FormatCount(int count, string singularWord) =>
        $"{count:N0} {(count == 1 ? singularWord : singularWord + "s")}";
}