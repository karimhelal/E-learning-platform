namespace BLL.Interfaces.Student
{
    using BLL.DTOs.Misc;
    using BLL.DTOs.Student;

    /// <summary>
    /// Service for retrieving Browse Courses page data (all courses in system)
    /// </summary>
    public interface IStudentBrowseCoursesService
    {
        Task<StudentBrowseCoursesPageDto?> GetInitialBrowseDataAsync(int studentId, PagingRequestDto pagingRequest);
    }
}