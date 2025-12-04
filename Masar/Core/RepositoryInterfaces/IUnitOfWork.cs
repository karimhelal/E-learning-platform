namespace Core.RepositoryInterfaces;

public interface IUnitOfWork : IDisposable
{
    // The method that actually hits the database
    Task<int> CompleteAsync();
}