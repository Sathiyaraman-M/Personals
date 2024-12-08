using Expensive.Infrastructure.Abstractions.Entities;
using Expensive.Infrastructure.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Expensive.Infrastructure.Repositories;

public class RepositoryFactory(IServiceProvider serviceProvider) : IRepositoryFactory
{
    public TRepository CreateRepository<TEntity, TRepository>(IDbConnection connection, IDbTransaction transaction)
        where TEntity : class, IEntity
        where TRepository : class, IRepository<TEntity>
    {
        return ActivatorUtilities.CreateInstance<TRepository>(serviceProvider, connection, transaction);
    }
}