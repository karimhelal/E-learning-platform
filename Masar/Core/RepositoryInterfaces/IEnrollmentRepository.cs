public interface IEnrollmentRepository : IGenericRepository<Enrollment>
{
    Task<Enrollment?> GetEnrollmentAsync(int studentId, int courseId);
}