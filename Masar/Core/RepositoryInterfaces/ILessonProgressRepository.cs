using Core.Entities;

namespace Core.RepositoryInterfaces;

public interface ILessonProgressRepository : IGenericRepository<LessonProgress>
{
    IQueryable<LessonProgress> GetStudentLessonProgressQueryable(int studentId, int lessonId);
    IQueryable<LessonProgress> GetAllLessonProgressForStudentQueryable(int studentId);

    Task<LessonProgress?> GetStudentLessonProgressAsync(int studentId, int lessonId);
    Task AddOrUpdateAsync(LessonProgress lessonProgress);
}
