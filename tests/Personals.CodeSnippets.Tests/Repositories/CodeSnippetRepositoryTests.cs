using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Personals.CodeSnippets.Entities;
using Personals.CodeSnippets.Extensions;
using Personals.CodeSnippets.Models;
using Personals.CodeSnippets.Repositories;
using Personals.Common.Enums;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using Personals.Infrastructure.Services;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Services;

namespace Personals.CodeSnippets.Tests.Repositories;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class CodeSnippetRepositoryTests : IDisposable
{
    private SqlServerDbContext DbContext => new(_databaseFixture.ConnectionString);
    private static StubTimeProvider TimeProvider => new();

    private readonly DatabaseFixture _databaseFixture;

    private readonly Guid _userId;
    
    private Guid UserId
    {
        get => _userId;
        init
        {
            _userId = value;
            _currentUserService.UserId.Returns(value);
        }
    }

    [Fact]
    public async Task GetCodeSnippetsAsync_ShouldReturnCodeSnippets()
    {
        // Arrange
        var codeSnippets = new List<CodeSnippet>
        {
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId),
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId, "console.log('Hello, World!');", Language.JavaScript, "Sample JS snippet")
        };
        await InsertCodeSnippetsAsync(codeSnippets);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);
        
        var expectedCodeSnippets = codeSnippets.Select(l => l.ToModel()).ToList();

        // Act
        var actualCodeSnippets = (await codeSnippetRepository.GetAllCodeSnippetsAsync(1, 10)).ToList();

        // Assert
        actualCodeSnippets.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(codeSnippets.Count)
            .And.BeEquivalentTo(expectedCodeSnippets);
    }

    [Fact]
    public async Task GetCodeSnippetsAsync_WithSearchString_ShouldReturnFilteredCodeSnippets()
    {
        // Arrange
        var codeSnippets = new List<CodeSnippet>
        {
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId),
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId, "console.log('Hello, World!');", Language.JavaScript, "Sample JS snippet"),
        };
        await InsertCodeSnippetsAsync(codeSnippets);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction,  _currentUserService, TimeProvider, Logger);
        
        var expectedCodeSnippets = codeSnippets
            .Where(l => l.Title?.Contains("JS") ?? false)
            .Select(l => l.ToModel()).ToList();

        // Act
        var actualCodeSnippets = (await codeSnippetRepository.GetAllCodeSnippetsAsync(1, 10, "JS")).ToList();

        // Assert
        actualCodeSnippets.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(1)
            .And.BeEquivalentTo(expectedCodeSnippets);
    }

    [Fact]
    public async Task GetCodeSnippetsCountAsync_ShouldReturnCodeSnippetsCount()
    {
        // Arrange
        var codeSnippets = new List<CodeSnippet>
        {
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId),
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId, "console.log('Hello, World!');", Language.JavaScript, "Sample JS snippet"),
        };
        await InsertCodeSnippetsAsync(codeSnippets);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var count = await codeSnippetRepository.GetCodeSnippetsCountAsync();

        // Assert
        count.Should().Be(codeSnippets.Count);
    }

    [Fact]
    public async Task GetCodeSnippetsCountAsync_WithSearchString_ShouldReturnFilteredCodeSnippetsCount()
    {
        // Arrange
        var codeSnippets = new List<CodeSnippet>
        {
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId),
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId, "console.log('Hello, World!');", Language.JavaScript, "Sample JS snippet"),
        };
        await InsertCodeSnippetsAsync(codeSnippets);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var count = await codeSnippetRepository.GetCodeSnippetsCountAsync("JS");

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetCodeSnippetByIdAsync_ShouldReturnCodeSnippet()
    {
        // Arrange
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), UserId);
        await InsertCodeSnippetsAsync([codeSnippet]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var actualCodeSnippet = await codeSnippetRepository.GetCodeSnippetByIdAsync(codeSnippet.Id);

        // Assert
        actualCodeSnippet.Should().NotBeNull()
            .And.BeEquivalentTo(codeSnippet.ToModel());
    }

    [Fact]
    public async Task GetCodeSnippetByIdAsync_WithNonExistingId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var codeSnippetId = Guid.NewGuid();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => codeSnippetRepository.GetCodeSnippetByIdAsync(codeSnippetId));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateCodeSnippetAsync_ShouldCreateCodeSnippet()
    {
        // Arrange
        var createCodeSnippetModel = new CreateCodeSnippetModel
        {
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Title = "Hello, World!",
            Remarks = "Sample code snippet",
            Tags = ["Hello World", "Sample"],
        };
        var codeSnippet = createCodeSnippetModel.ToCodeSnippet(UserId, TimeProvider.Now);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository =
            new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var codeSnippetId = await codeSnippetRepository.CreateCodeSnippetAsync(createCodeSnippetModel);
        transaction.Commit();
        codeSnippet.Id = codeSnippetId;

        // Assert
        var actualCodeSnippet = await codeSnippetRepository.GetCodeSnippetByIdAsync(codeSnippetId);
        actualCodeSnippet.Should().NotBeNull()
            .And.BeEquivalentTo(codeSnippet.ToModel());
    }

    [Fact]
    public async Task UpdateCodeSnippetAsync_ShouldUpdateCodeSnippet()
    {
        // Arrange
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), UserId);
        await InsertCodeSnippetsAsync([codeSnippet]);

        var updateCodeSnippetModel = new UpdateCodeSnippetModel
        {
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Title = "Hello, World!",
            Remarks = "Sample code snippet",
            Tags = ["Hello World", "Sample"],
        };
        var updatedCodeSnippet = updateCodeSnippetModel.ToCodeSnippet(TimeProvider.Now);
        updatedCodeSnippet.Id = codeSnippet.Id;
        updatedCodeSnippet.UserId = UserId;
        updatedCodeSnippet.CreatedOnDate = codeSnippet.CreatedOnDate;

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        await codeSnippetRepository.UpdateCodeSnippetAsync(codeSnippet.Id, updateCodeSnippetModel);

        // Assert
        var actualCodeSnippet = await codeSnippetRepository.GetCodeSnippetByIdAsync(codeSnippet.Id);
        actualCodeSnippet.Should().NotBeNull()
            .And.BeEquivalentTo(updatedCodeSnippet.ToModel());
    }

    [Fact]
    public async Task UpdateCodeSnippetAsync_WithNonExistingId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var updateCodeSnippetModel = new UpdateCodeSnippetModel
        {
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Title = "Hello, World!",
            Remarks = "Sample code snippet",
            Tags = ["Hello World", "Sample"],
        };
        var codeSnippetId = Guid.NewGuid();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            codeSnippetRepository.UpdateCodeSnippetAsync(codeSnippetId, updateCodeSnippetModel));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteCodeSnippetAsync_ShouldDeleteCodeSnippet()
    {
        // Arrange
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), UserId);
        await InsertCodeSnippetsAsync([codeSnippet]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        await codeSnippetRepository.DeleteCodeSnippetAsync(codeSnippet.Id);

        // Assert
        var codeSnippets = (await codeSnippetRepository.GetAllCodeSnippetsAsync(1, 10)).ToList();
        codeSnippets.Should().NotBeNull()
            .And.BeEmpty();
    }

    [Fact]
    public async Task DeleteCodeSnippetAsync_WithNonExistingId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var codeSnippetId = Guid.NewGuid();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var codeSnippetRepository = new CodeSnippetRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => codeSnippetRepository.DeleteCodeSnippetAsync(codeSnippetId));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>();
    }
    
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    public CodeSnippetRepositoryTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        UserId = GetAdminUserId();
    }

    private static ILogger<CodeSnippetRepository> Logger => new NullLogger<CodeSnippetRepository>();

    private Guid GetAdminUserId()
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        const string sql = "SELECT Id FROM [dbo].[AppUsers]";
        return connection.QueryFirst<Guid>(sql, transaction: transaction);
    }

    private async Task InsertCodeSnippetsAsync(List<CodeSnippet> codeSnippets)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var codeSnippet in codeSnippets)
        {
            codeSnippet.UserId = UserId;
            await connection.ExecuteAsync(
                "INSERT INTO [dbo].[CodeSnippets] (Id, Title, Snippet, Language, Remarks, Tags, UserId, CreatedOnDate) VALUES (@Id, @Title, @Snippet, @Language, @Remarks, @Tags, @UserId, @CreatedOnDate)",
                codeSnippet, transaction);
        }

        transaction.Commit();
        connection.Close();
    }

    private async Task ClearCodeSnippetsTableAsync()
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        await connection.ExecuteAsync("DELETE FROM [dbo].[CodeSnippets]", transaction: transaction);
        transaction.Commit();
        connection.Close();
    }

    public void Dispose()
    {
        ClearCodeSnippetsTableAsync().Wait();
    }
}