namespace Web.Interfaces
{
    using Web.ViewModels.Student;

    /// <summary>
    /// Service for retrieving Browse Tracks page data (all tracks in system)
    /// </summary>
    public interface IStudentBrowseTrackService
    {
        Task<StudentBrowseTracksPageData?> GetAllTracksAsync(int studentId);
    }
}
