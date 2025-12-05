using BLL.DTOs.Course;
using BLL.DTOs.Instructor;
using BLL.DTOs.Misc;
using BLL.Services;


namespace BLL.Interfaces;

public interface ICourseService
{
    Task<PagedResultDto<InstructorCourseDto>> GetCoursesByInctructorAsync(int instructorId, PagingRequestDto request);
    InstructorCourseBasicDetailsDto? GetInstructorCourseBasicDetails(int instructorId, int courseId);
    InstructorCourseContentDetailsDto? GetInstructorCourseContentDetails(int instructorId, int courseId);
    IQueryable<InstructorTopPerformingCourseDto> GetInstructorTopPerformingCourses(int instructorId, int topN);


    Task<BrowseResultDto<CourseBrowseCardDto>> GetInitialBrowsePageCoursesAsync(PagingRequestDto request);
    Task<BrowseResultDto<CourseBrowseCardDto>> GetAllCoursesFilteredForBrowsingPagedAsync(BrowseRequestDto request);
    
    Task<CourseDetailsDto?> GetCourseDetailsByIdAsync(int courseId);
    
    Task<FilterGroupsDto> GetFilterSectionConfig();
    Task<FilterGroupsStatsDto> GetFilterGroupsStats();

}
