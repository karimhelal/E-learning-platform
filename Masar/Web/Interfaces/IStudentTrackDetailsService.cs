using System.Threading.Tasks;
using Web.ViewModels.Student;

namespace Web.Interfaces
{
    /// <summary>
    /// Service for retrieving a single track details for a student
    /// </summary>
    public interface IStudentTrackDetailsService
    {
        Task<StudentTrackDetailsData?> GetTrackDetailsAsync(int studentId, int trackId);
    }
}

