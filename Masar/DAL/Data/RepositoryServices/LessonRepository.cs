using Core.Entities;
using Core.RepositoryInterfaces;

namespace DAL.Data.RepositoryServices
{
    public class LessonRepository : GenericRepository<Lesson>, ILessonRepository
    {
        public LessonRepository(AppDbContext context) : base(context)
        {
        }

        // Add any lesson-specific implementation methods here if needed
    }
}