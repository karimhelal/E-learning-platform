using Core.RepositoryInterfaces;

public interface ILessonProgressRepository : IGenericRepository<LessonProgress>
{
    Task<List<LessonProgress>> GetStudentLessonProgressAsync(int studentId, List<int> lessonIds);
    Task<LessonProgress?> GetLessonProgressAsync(int studentId, int lessonId);
    Task MarkLessonAsCompletedAsync(int studentId, int lessonId, int timeSpent);
}