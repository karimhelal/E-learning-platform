namespace Web.Interfaces
{
    using Web.ViewModels.Student;

    /// <summary>
    /// Service for retrieving Browse Courses page data (all courses in system)
    /// </summary>
    public interface IStudentBrowseCoursesService
    {
        Task<StudentBrowseCoursesPageData?> GetAllCoursesAsync(int studentId);
    }
}