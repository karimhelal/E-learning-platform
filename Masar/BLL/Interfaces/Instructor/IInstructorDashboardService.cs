using BLL.DTOs.Instructor;

namespace BLL.Interfaces.Instructor;

public interface IInstructorDashboardService
{
    Task<InstructorDashboardDto> GetInstructorDashboardAsync(int instructorId);
}
