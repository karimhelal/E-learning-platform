namespace Web.Interfaces
{
    using BLL.DTOs.Misc;
    using Web.ViewModels.Student;

    /// <summary>
    /// Service for retrieving Browse Courses page data (all courses in system)
    /// </summary>
    public interface IStudentBrowseCoursesService
    {
        Task<StudentBrowseCoursesPageData?> GetInitialBrowseDataAsync(int studentId, PagingRequestDto pagingRequest);
    }
}