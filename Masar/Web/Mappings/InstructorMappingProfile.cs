using AutoMapper;
using Web.ViewModels.Instructor;
using Web.ViewModels.Instructor.Dashboard;
using Web.ViewModels.Instructor.ManageCourse;
using BLL.DTOs.Instructor;
using BLL.DTOs.Instructor.ManageCourse;

namespace Web.Mappings;

public class InstructorMappingProfile : Profile
{
    public InstructorMappingProfile()
    {
        // ================= Dashboard =================
        CreateMap<InstructorDashboardDto, InstructorDashboardDataViewModel>();
        CreateMap<InstructorGeneralStatsDto, InstructorGeneralStatsViewModel>();
        CreateMap<InstructorCurrentMonthStatsDto, InstructorCurrentMonthStatsViewModel>();
        CreateMap<InstructorTopPerformingCourseDto, InstructorTopPerformingCourseViewModel>();
        CreateMap<InstructorCourseCardDto, InstructorCourseCardViewModel>()
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level.ToString()));

        // ================= Profile =================
        CreateMap<InstructorProfileDto, InstructorProfileDataViewModel>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => $"@{src.FirstName.ToLower()}_{src.LastName.ToLower()}"))
            .ForMember(dest => dest.Initials, opt => opt.MapFrom(src => $"{src.FirstName[0]}{src.LastName[0]}".ToUpper()))
            .ForMember(dest => dest.JoinedDate, opt => opt.MapFrom(src => src.JoinedDate.ToString("MMMM yyyy")))
            .ForMember(dest => dest.Stats, opt => opt.MapFrom(src => src)); 

 
        CreateMap<InstructorProfileDto, InstructorProfileStatsViewModel>();

        CreateMap<InstructorProfileCourseDto, InstructorProfileCourseCardViewModel>()
            .ForMember(dest => dest.StatusBadgeClass, opt => opt.MapFrom(src => src.Status.ToLower() == "published" ? "badge-green" : "badge-orange"));

        // ================= Manage Course =================
        CreateMap<ManageViewModuleDto, ManageViewModuleViewModel>()
            .ForMember(dest => dest.FormattedLessonDuration, opt => opt.MapFrom(src => FormatDuration(src.DurationInMinutes ?? 0)));

        CreateMap<ManageViewLessonDto, ManageViewLessonViewModel>()
            .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.ContentType.ToString()))
            .ForMember(dest => dest.FormattedLessonDuration, opt => opt.MapFrom(src => FormatDuration(src.DurationInMinutes ?? 0)));
    }

    private static string FormatDuration(int durationInMinutes)
    {
        int h = durationInMinutes / 60;
        int m = durationInMinutes % 60;
        var parts = new System.Collections.Generic.List<string>();
        if (h > 0) parts.Add($"{h} {(h == 1 ? "hr" : "hrs")}");
        if (m > 0) parts.Add($"{m} {(m == 1 ? "min" : "mins")}");
        return parts.Count > 0 ? string.Join(" ", parts) : "---";
    }
}