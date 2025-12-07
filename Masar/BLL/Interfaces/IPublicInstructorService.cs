using BLL.DTOs.Instructor;

namespace BLL.Interfaces;

public interface IPublicInstructorService
{
    Task<PublicInstructorProfileDto?> GetInstructorPublicProfileAsync(int instructorId);
}