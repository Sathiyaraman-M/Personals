using Expensive.Infrastructure.Abstractions.Entities;

namespace Expensive.Infrastructure.Abstractions.Repositories;

public interface IUnitOfWork : IDisposable
{
    TRepository Repository<TEntity, TRepository, TRepositoryImpl>()
        where TEntity : class, IEntity
        where TRepository : IRepository<TEntity>
        where TRepositoryImpl : class, TRepository;

    void BeginTransaction();

    void CommitChanges();

    void RollbackChanges();
}