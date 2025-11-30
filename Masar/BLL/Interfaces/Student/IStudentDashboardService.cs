using BLL.DTOs.Student;

namespace BLL.Interfaces.Student;

public interface IStudentDashboardService
{
    Task<StudentDashboardDto?> GetStudentDashboardAsync(int studentId);
}
