using Microsoft.Extensions.DependencyInjection;
using Personals.Infrastructure.Abstractions.Entities;
using Personals.Infrastructure.Abstractions.Repositories;
using System.Data;

namespace Personals.Infrastructure.Repositories;

public class RepositoryFactory(IServiceProvider serviceProvider) : IRepositoryFactory
{
    public TRepository CreateRepository<TEntity, TRepository>(IDbConnection connection, IDbTransaction transaction)
        where TEntity : class, IEntity
        where TRepository : class, IRepository<TEntity>
    {
        return ActivatorUtilities.CreateInstance<TRepository>(serviceProvider, connection, transaction);
    }
}