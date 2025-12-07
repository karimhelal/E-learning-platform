using BLL.DTOs.Student;

namespace BLL.Interfaces.Student;

public interface IStudentProfileService
{
    Task<StudentProfileDto?> GetStudentProfileAsync(int studentId);
    Task<bool> UpdateStudentProfileAsync(int studentId, UpdateStudentProfileDto profileDto);
}
