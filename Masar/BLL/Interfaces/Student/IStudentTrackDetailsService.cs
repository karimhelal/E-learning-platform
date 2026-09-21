using BLL.DTOs.Student;
using System.Threading.Tasks;

namespace BLL.Interfaces.Student

{
    /// <summary>
    /// Service for retrieving a single track details for a student
    /// </summary>
    public interface IStudentTrackDetailsService
    {
        Task<StudentTrackDetailsDto?> GetTrackDetailsAsync(int userId, int trackId);
    }
}

