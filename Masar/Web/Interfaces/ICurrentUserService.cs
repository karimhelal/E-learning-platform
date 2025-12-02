namespace Web.Interfaces
{
    public interface ICurrentUserService
    {
        int GetUserId();
        int GetStudentId();
        int GetInstructorId();
    }
}
