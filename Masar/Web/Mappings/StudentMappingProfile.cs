using AutoMapper;
using BLL.DTOs.Student;
using Web.ViewModels.Student;

namespace Web.Mappings;

public class StudentMappingProfile : Profile
{
    public StudentMappingProfile()
    {
        CreateMap<StudentProfileDto, StudentProfileDataViewModel>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.UserInitials, opt => opt.MapFrom(src => $"{src.FirstName[0]}{src.LastName[0]}".ToUpper()))
            .ForMember(dest => dest.Initials, opt => opt.MapFrom(src => $"{src.FirstName[0]}{src.LastName[0]}".ToUpper()))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => $"@{src.FirstName.ToLower()}_{src.LastName.ToLower()}"))
            .ForMember(dest => dest.JoinedDate, opt => opt.MapFrom(src => src.JoinedDate.ToString("MMMM yyyy")))
            .ForMember(dest => dest.Stats, opt => opt.MapFrom(src => src));

        CreateMap<StudentProfileDto, StudentProfileStatsViewModel>();

        CreateMap<StudentProfileCourseDto, StudentProfileCourseCardViewModel>();

        CreateMap<StudentCertificateDto, StudentCertificateViewModel>()
            .ForMember(dest => dest.IssuedDate, opt => opt.MapFrom(src => src.IssuedDate.ToString("MMM dd, yyyy")));

    }
}