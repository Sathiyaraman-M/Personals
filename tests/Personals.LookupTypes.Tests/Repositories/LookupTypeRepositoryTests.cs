using Dapper;
using Personals.Common.Enums;
using Personals.Infrastructure.Exceptions;
using Personals.Infrastructure.Services;
using Personals.LookupTypes.Entities;
using Personals.LookupTypes.Extensions;
using Personals.LookupTypes.Models;
using Personals.LookupTypes.Repositories;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Personals.Infrastructure.Abstractions.Services;

namespace Personals.LookupTypes.Tests.Repositories;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class LookupTypeRepositoryTests : IDisposable
{
    private SqlServerDbContext DbContext => new(_databaseFixture.ConnectionString);
    private static StubTimeProvider TimeProvider => new();
    private static readonly Guid UserId = Guid.NewGuid();
    
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly DatabaseFixture _databaseFixture;

    public LookupTypeRepositoryTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        _currentUserService.UserId.Returns(UserId);
    }

    private static ILogger<LookupTypeRepository> Logger => new NullLogger<LookupTypeRepository>();

    public static TheoryData<LookupTypeCategory> LookupTypeCategories() => new(Enum.GetValues<LookupTypeCategory>());

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetLookupTypesAsync_ShouldReturnLookupTypes(LookupTypeCategory category)
    {
        // Arrange
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), category, UserId, "CODE_1", "Look-up Type 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, UserId, "CODE_2", "Look-up Type 2")
        };
        await InsertLookupTypesAsync(lookupTypes);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var actualLookupTypes = (await lookupTypeRepository.GetAllLookupTypesAsync(category, 1, 10)).ToList();

        // Assert
        actualLookupTypes.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(lookupTypes.Count)
            .And.BeEquivalentTo(lookupTypes);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetLookupTypesAsync_WithSearchString_ShouldReturnFilteredLookupTypes(LookupTypeCategory category)
    {
        // Arrange
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), category, UserId, "CODE_1", "Look-up Type 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, UserId, "CODE_2", "Look-up Type 2")
        };
        await InsertLookupTypesAsync(lookupTypes);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var actualLookupTypes = (await lookupTypeRepository.GetAllLookupTypesAsync(category, 1, 10, "Type 2")).ToList();

        // Assert
        actualLookupTypes.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(1)
            .And.Contain(lookupTypes[1]);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetLookupTypesCountAsync_ShouldReturnLookupTypesCount(LookupTypeCategory category)
    {
        // Arrange
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), category, UserId, "CODE_1", "Look-up Type 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, UserId, "CODE_2", "Look-up Type 2")
        };
        await InsertLookupTypesAsync(lookupTypes);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var count = await lookupTypeRepository.GetLookupTypesCountAsync(category);

        // Assert
        count.Should().Be(lookupTypes.Count);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetLookupTypesCountAsync_WithSearchString_ShouldReturnFilteredLookupTypesCount(
        LookupTypeCategory category)
    {
        // Arrange
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), category, UserId, "CODE_1", "Look-up Type 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, UserId, "CODE_2", "Look-up Type 2")
        };
        await InsertLookupTypesAsync(lookupTypes);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var count = await lookupTypeRepository.GetLookupTypesCountAsync(category, "Type 1");

        // Assert
        count.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetLookupTypeByIdAsync_ShouldReturnLookupType(LookupTypeCategory category)
    {
        // Arrange
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category, UserId);
        await InsertLookupTypesAsync([lookupType]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var actualLookupType = await lookupTypeRepository.GetLookupTypeByIdAsync(lookupType.Id);

        // Assert
        actualLookupType.Should().NotBeNull()
            .And.BeEquivalentTo(lookupType);
    }

    [Fact]
    public async Task GetLookupTypeByIdAsync_WithNonExistingId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var lookupTypeId = Guid.NewGuid();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => lookupTypeRepository.GetLookupTypeByIdAsync(lookupTypeId));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>();
    }

    [Theory]
    [InlineData(LookupTypeCategory.PaymentMethod, "UPI", "Unified Payment Interface")]
    [InlineData(LookupTypeCategory.ExpenseType, "Travel", "Travel Expenses")]
    public async Task CreateLookupTypeAsync_ShouldCreateLookupType(LookupTypeCategory category, string code,
        string name)
    {
        // Arrange
        var createLookupTypeModel = new CreateLookupTypeModel
        {
            Category = category,
            Code = code,
            Name = name,
            CreatedByUserName = "admin",
            CreatedByUserId = UserId
        };
        var lookupType = createLookupTypeModel.ToLookupType(TimeProvider.Now);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var lookupTypeId = await lookupTypeRepository.CreateLookupTypeAsync(createLookupTypeModel);
        transaction.Commit();
        lookupType.Id = lookupTypeId;

        // Assert
        var actualLookupType = await lookupTypeRepository.GetLookupTypeByIdAsync(lookupTypeId);
        actualLookupType.Should().NotBeNull()
            .And.BeEquivalentTo(lookupType);
    }

    [Theory]
    [InlineData(LookupTypeCategory.PaymentMethod, "UPI", "Unified Payment Interface")]
    [InlineData(LookupTypeCategory.ExpenseType, "Travel", "Travel Expenses")]
    public async Task UpdateLookupTypeAsync_ShouldUpdateLookupType(LookupTypeCategory category, string code,
        string name)
    {
        // Arrange
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category, UserId);
        await InsertLookupTypesAsync([lookupType]);

        var updateLookupTypeModel = new UpdateLookupTypeModel
        {
            Category = category,
            Code = code,
            Name = name,
            LastModifiedByUserName = "admin",
            LastModifiedByUserId = Guid.NewGuid()
        };
        var updatedLookupType = updateLookupTypeModel.ToLookupType(TimeProvider.Now);
        updatedLookupType.Id = lookupType.Id;
        updatedLookupType.UserId = UserId;
        updatedLookupType.CreatedByUserName = lookupType.CreatedByUserName;
        updatedLookupType.CreatedByUserId = lookupType.CreatedByUserId;
        updatedLookupType.CreatedOnDate = lookupType.CreatedOnDate;

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        await lookupTypeRepository.UpdateLookupTypeAsync(lookupType.Id, updateLookupTypeModel);

        // Assert
        var actualLookupType = await lookupTypeRepository.GetLookupTypeByIdAsync(lookupType.Id);
        actualLookupType.Should().NotBeNull()
            .And.BeEquivalentTo(updatedLookupType);
    }


    [Fact]
    public async Task UpdateLookupTypeAsync_WithDifferentCategory_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var lookupType =
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, UserId, "Travel", "Travel Expenses");
        await InsertLookupTypesAsync([lookupType]);

        var updateLookupTypeModel = new UpdateLookupTypeModel
        {
            Category = LookupTypeCategory.PaymentMethod,
            Code = "UPI",
            Name = "Unified Payment Interface",
            LastModifiedByUserName = "admin",
            LastModifiedByUserId = Guid.NewGuid()
        };
        var updatedLookupType = updateLookupTypeModel.ToLookupType(TimeProvider.Now);
        updatedLookupType.Id = lookupType.Id;
        updatedLookupType.CreatedByUserName = lookupType.CreatedByUserName;
        updatedLookupType.CreatedByUserId = lookupType.CreatedByUserId;
        updatedLookupType.CreatedOnDate = lookupType.CreatedOnDate;

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            lookupTypeRepository.UpdateLookupTypeAsync(lookupType.Id, updateLookupTypeModel));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<InvalidOperationException>();
    }

    [Theory]
    [InlineData(LookupTypeCategory.PaymentMethod, "UPI", "Unified Payment Interface")]
    [InlineData(LookupTypeCategory.ExpenseType, "Travel", "Travel Expenses")]
    public async Task UpdateLookupTypeAsync_WithNonExistingId_ShouldThrowEntityNotFoundException(
        LookupTypeCategory category, string code, string name)
    {
        // Arrange
        var updateLookupTypeModel = new UpdateLookupTypeModel
        {
            Category = category,
            Code = code,
            Name = name,
            LastModifiedByUserName = "admin",
            LastModifiedByUserId = Guid.NewGuid()
        };
        var lookupTypeId = Guid.NewGuid();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            lookupTypeRepository.UpdateLookupTypeAsync(lookupTypeId, updateLookupTypeModel));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>();
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task DeleteLookupTypeAsync_ShouldDeleteLookupType(LookupTypeCategory category)
    {
        // Arrange
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category, UserId);
        await InsertLookupTypesAsync([lookupType]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        await lookupTypeRepository.DeleteLookupTypeAsync(lookupType.Id);

        // Assert
        var lookupTypes = (await lookupTypeRepository.GetAllLookupTypesAsync(category, 1, 10)).ToList();
        lookupTypes.Should().NotBeNull()
            .And.BeEmpty();
    }

    [Fact]
    public async Task DeleteLookupTypeAsync_WithNonExistingId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var lookupTypeId = Guid.NewGuid();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var lookupTypeRepository = new LookupTypeRepository(connection, transaction, TimeProvider, _currentUserService, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => lookupTypeRepository.DeleteLookupTypeAsync(lookupTypeId));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>();
    }

    private async Task InsertLookupTypesAsync(List<LookupType> lookupTypes)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var lookupType in lookupTypes)
        {
            lookupType.UserId = UserId;
            await connection.ExecuteAsync(
                "INSERT INTO [dbo].[LookupTypes] (Id, Category, Code, Name, UserId, CreatedByUserName, CreatedByUserId, CreatedOnDate) VALUES (@Id, @Category, @Code, @Name, @UserId, @CreatedByUserName, @CreatedByUserId, @CreatedOnDate);",
                lookupType, transaction);
        }

        transaction.Commit();
        connection.Close();
    }

    private async Task ClearLookupTypesTableAsync()
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        await connection.ExecuteAsync("DELETE FROM [dbo].[LookupTypes]", transaction: transaction);
        transaction.Commit();
        connection.Close();
    }

    public void Dispose()
    {
        ClearLookupTypesTableAsync().Wait();
    }
}