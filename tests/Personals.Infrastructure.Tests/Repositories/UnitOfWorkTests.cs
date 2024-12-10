using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Repositories;
using System.Data;

namespace Personals.Infrastructure.Tests.Repositories;

public class UnitOfWorkTests
{
    [Fact]
    public void BeginTransaction_ShouldBeginTransaction_WhenNotStarted()
    {
        // Arrange
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        var dbContext = Substitute.For<IDbContext>();
        dbContext.GetConnection().Returns(connection);
        connection.BeginTransaction().Returns(transaction);
        
        var unitOfWork = new UnitOfWork(dbContext, Substitute.For<IRepositoryFactory>());
        
        // Act
        unitOfWork.BeginTransaction();
        
        // Assert
        connection.Received(1).BeginTransaction();
    }
    
    [Fact]
    public void Repository_ShouldCreateRepository_WhenNotCached()
    {
        // Arrange
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        var repositoryFactory = Substitute.For<IRepositoryFactory>();
        var dbContext = Substitute.For<IDbContext>();
        dbContext.GetConnection().Returns(connection);
        connection.BeginTransaction().Returns(transaction);
        repositoryFactory.CreateRepository<SomeEntity, SomeRepository>(connection, transaction).Returns(new SomeRepository());
        
        var unitOfWork = new UnitOfWork(dbContext, repositoryFactory);
        
        // Act
        var repository = unitOfWork.Repository<SomeEntity, ISomeRepository, SomeRepository>();
        
        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeOfType<SomeRepository>();
        repository.Should().BeAssignableTo<ISomeRepository>();
    }
    
    [Fact]
    public void Repository_ShouldReturnCachedRepository_WhenCached()
    {
        // Arrange
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        var repositoryFactory = Substitute.For<IRepositoryFactory>();
        var dbContext = Substitute.For<IDbContext>();
        dbContext.GetConnection().Returns(connection);
        connection.BeginTransaction().Returns(transaction);
        var repository = new SomeRepository();
        repositoryFactory.CreateRepository<SomeEntity, SomeRepository>(connection, transaction).Returns(repository);
        
        var unitOfWork = new UnitOfWork(dbContext, repositoryFactory);
        
        // Act
        var repository1 = unitOfWork.Repository<SomeEntity, ISomeRepository, SomeRepository>();
        var repository2 = unitOfWork.Repository<SomeEntity, ISomeRepository, SomeRepository>();
        
        // Assert
        repository1.Should().NotBeNull();
        repository1.Should().BeOfType<SomeRepository>();
        repository1.Should().BeAssignableTo<ISomeRepository>();
        
        repository2.Should().NotBeNull();
        repository2.Should().BeOfType<SomeRepository>();
        repository2.Should().BeAssignableTo<ISomeRepository>();
        
        repository1.Should().BeSameAs(repository2);
    }
    
    [Fact]
    public void CommitChanges_ShouldCommitTransaction()
    {
        // Arrange
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        var dbContext = Substitute.For<IDbContext>();
        dbContext.GetConnection().Returns(connection);
        connection.BeginTransaction().Returns(transaction);
        
        var unitOfWork = new UnitOfWork(dbContext, Substitute.For<IRepositoryFactory>());
        unitOfWork.BeginTransaction();
        
        // Act
        unitOfWork.CommitChanges();
        
        // Assert
        transaction.Received(1).Commit();
    }
    
    [Fact]
    public void CommitChanges_ShouldCleanupTransaction()
    {
        // Arrange
        var connection = Substitute.For<IDbConnection>();
        var dbContext = Substitute.For<IDbContext>();
        connection.State.Returns(ConnectionState.Open);
        dbContext.GetConnection().Returns(connection);
        
        var unitOfWork = new UnitOfWork(dbContext, Substitute.For<IRepositoryFactory>());
        
        // Act
        unitOfWork.CommitChanges();
        
        // Assert
        connection.Received(1).Close();
    }
    
    [Fact]
    public void RollbackChanges_ShouldRollbackTransaction()
    {
        // Arrange
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        var dbContext = Substitute.For<IDbContext>();
        dbContext.GetConnection().Returns(connection);
        connection.BeginTransaction().Returns(transaction);
        
        var unitOfWork = new UnitOfWork(dbContext, Substitute.For<IRepositoryFactory>());
        unitOfWork.BeginTransaction();
        
        // Act
        unitOfWork.RollbackChanges();
        
        // Assert
        transaction.Received(1).Rollback();
    }
    
    [Fact]
    public void RollbackChanges_ShouldCleanupTransaction()
    {
        // Arrange
        var connection = Substitute.For<IDbConnection>();
        var dbContext = Substitute.For<IDbContext>();
        connection.State.Returns(ConnectionState.Open);
        dbContext.GetConnection().Returns(connection);
        
        var unitOfWork = new UnitOfWork(dbContext, Substitute.For<IRepositoryFactory>());
        
        // Act
        unitOfWork.RollbackChanges();
        
        // Assert
        connection.Received(1).Close();
    }
    
    [Fact]
    public void Dispose_ShouldDisposeTransaction()
    {
        // Arrange
        var connection = Substitute.For<IDbConnection>();
        var transaction = Substitute.For<IDbTransaction>();
        var dbContext = Substitute.For<IDbContext>();
        dbContext.GetConnection().Returns(connection);
        connection.State.Returns(ConnectionState.Open);
        connection.BeginTransaction().Returns(transaction);
        
        var unitOfWork = new UnitOfWork(dbContext, Substitute.For<IRepositoryFactory>());
        unitOfWork.BeginTransaction();
        
        // Act
        unitOfWork.Dispose();
        
        // Assert
        transaction.Received(1).Dispose();
        connection.Received(1).Close();
    }
}