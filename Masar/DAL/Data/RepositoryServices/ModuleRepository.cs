using Core.Entities;
using Core.RepositoryInterfaces;

namespace DAL.Data.RepositoryServices
{
    public class ModuleRepository : GenericRepository<Module>, IModuleRepository
    {
        public ModuleRepository(AppDbContext context) : base(context)
        {
        }

        // Add any module-specific implementation methods here if needed
    }
}