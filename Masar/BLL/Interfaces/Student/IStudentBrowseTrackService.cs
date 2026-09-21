namespace BLL.Interfaces.Student
{
    using BLL.DTOs.Student;

    /// <summary>
    /// Service for retrieving Browse Tracks page data (all tracks in system)
    /// </summary>
    public interface IStudentBrowseTrackService
    {
        Task<StudentBrowseTracksPageDto?> GetAllTracksAsync(int studentId);
    }
}
