using Personals.Infrastructure.Abstractions.Entities;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using System.Data;

namespace Personals.Infrastructure.Repositories;

public sealed class UnitOfWork(IDbContext dbContext, IRepositoryFactory repositoryFactory) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];
    private IDbTransaction? _transaction;
    private readonly IDbConnection _connection = dbContext.GetConnection();

    private IDbConnection Connection
    {
        get
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            return _connection;
        }
    }

    public void BeginTransaction()
    {
        _transaction ??= Connection.BeginTransaction();
    }

    public TRepository Repository<TEntity, TRepository, TRepositoryImpl>()
        where TEntity : class, IEntity
        where TRepository : IRepository<TEntity>
        where TRepositoryImpl : class, TRepository
    {
        if (_repositories.ContainsKey(typeof(TEntity)))
            return (TRepository)_repositories[typeof(TEntity)];

        if (_transaction == null)
            BeginTransaction();

        var repository = repositoryFactory.CreateRepository<TEntity, TRepositoryImpl>(Connection, _transaction!);
        _repositories.Add(typeof(TEntity), repository);
        return repository;
    }

    public void CommitChanges()
    {
        try
        {
            _transaction?.Commit();
        }
        finally
        {
            Cleanup();
        }
    }

    public void RollbackChanges()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            Cleanup();
        }
    }

    private void Cleanup()
    {
        _transaction?.Dispose();
        _transaction = null;

        if (Connection.State == ConnectionState.Open)
        {
            Connection.Close();
        }
    }

    public void Dispose()
    {
        Cleanup();
    }
}