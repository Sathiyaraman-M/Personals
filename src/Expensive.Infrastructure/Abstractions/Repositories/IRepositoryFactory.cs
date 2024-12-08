using Expensive.Infrastructure.Abstractions.Entities;
using System.Data;

namespace Expensive.Infrastructure.Abstractions.Repositories;

public interface IRepositoryFactory
{
    TRepository CreateRepository<TEntity, TRepository>(IDbConnection connection, IDbTransaction transaction)
        where TEntity : class, IEntity
        where TRepository : class, IRepository<TEntity>;
}