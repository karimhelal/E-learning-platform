using BLL.DTOs.Instructor;

namespace BLL.Interfaces.Instructor;

public interface IInstructorProfileService
{
    Task<InstructorProfileDto?> GetInstructorProfileAsync(int instructorId);
    Task<bool> UpdateInstructorProfileAsync(int instructorId, UpdateInstructorProfileDto profileDto);
}