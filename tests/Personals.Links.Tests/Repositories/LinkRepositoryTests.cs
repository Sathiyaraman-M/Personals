using Dapper;
using Personals.Infrastructure.Exceptions;
using Personals.Infrastructure.Services;
using Personals.Links.Entities;
using Personals.Links.Extensions;
using Personals.Links.Models;
using Personals.Links.Repositories;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Personals.Infrastructure.Abstractions.Services;

namespace Personals.Links.Tests.Repositories;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class LinkRepositoryTests : IDisposable
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
    
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    public LinkRepositoryTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        UserId = GetAdminUserId();
    }

    private static ILogger<LinkRepository> Logger => new NullLogger<LinkRepository>();


    [Fact]
    public async Task GetLinksAsync_ShouldReturnLinks()
    {
        // Arrange
        var links = new List<Link>
        {
            LinkFactory.Create(Guid.NewGuid(), UserId),
            LinkFactory.Create(Guid.NewGuid(), UserId, "https://www.microsoft.com", "Microsoft", "Microsoft Corporation"),
        };
        await InsertLinksAsync(links);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);
        
        var expectedLinks = links.Select(l => l.ToModel()).ToList();

        // Act
        var actualLinks = (await linkRepository.GetAllLinksAsync(1, 10)).ToList();

        // Assert
        actualLinks.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(links.Count)
            .And.BeEquivalentTo(expectedLinks);
    }

    [Fact]
    public async Task GetLinksAsync_WithSearchString_ShouldReturnFilteredLinks()
    {
        // Arrange
        var links = new List<Link>
        {
            LinkFactory.Create(Guid.NewGuid(), UserId),
            LinkFactory.Create(Guid.NewGuid(), UserId, "https://www.microsoft.com", "Microsoft", "Microsoft Corporation"),
        };
        await InsertLinksAsync(links);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction,  _currentUserService, TimeProvider, Logger);
        
        var expectedLinks = links
            .Where(l => l.Title?.Contains("Microsoft") ?? false)
            .Select(l => l.ToModel()).ToList();

        // Act
        var actualLinks = (await linkRepository.GetAllLinksAsync(1, 10, "Microsoft")).ToList();

        // Assert
        actualLinks.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(1)
            .And.BeEquivalentTo(expectedLinks);
    }

    [Fact]
    public async Task GetLinksCountAsync_ShouldReturnLinksCount()
    {
        // Arrange
        var links = new List<Link>
        {
            LinkFactory.Create(Guid.NewGuid(), UserId),
            LinkFactory.Create(Guid.NewGuid(), UserId, "https://www.microsoft.com", "Microsoft", "Microsoft Corporation"),
        };
        await InsertLinksAsync(links);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var count = await linkRepository.GetLinksCountAsync();

        // Assert
        count.Should().Be(links.Count);
    }

    [Fact]
    public async Task GetLinksCountAsync_WithSearchString_ShouldReturnFilteredLinksCount()
    {
        // Arrange
        var links = new List<Link>
        {
            LinkFactory.Create(Guid.NewGuid(), UserId),
            LinkFactory.Create(Guid.NewGuid(), UserId, "https://www.microsoft.com", "Microsoft", "Microsoft Corporation"),
        };
        await InsertLinksAsync(links);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var count = await linkRepository.GetLinksCountAsync("Microsoft");

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetLinkByIdAsync_ShouldReturnLink()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), UserId);
        await InsertLinksAsync([link]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var actualLink = await linkRepository.GetLinkByIdAsync(link.Id);

        // Assert
        actualLink.Should().NotBeNull()
            .And.BeEquivalentTo(link.ToModel());
    }

    [Fact]
    public async Task GetLinkByIdAsync_WithNonExistingId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var linkId = Guid.NewGuid();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => linkRepository.GetLinkByIdAsync(linkId));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateLinkAsync_ShouldCreateLink()
    {
        // Arrange
        var createLinkModel = new CreateLinkModel
        {
            Url = "https://www.microsoft.com",
            Title = "Microsoft",
            Description = "Microsoft Corporation",
            Tags = ["Microsoft", "Software", "Technology"],
        };
        var link = createLinkModel.ToLink(UserId, TimeProvider.Now);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var linkId = await linkRepository.CreateLinkAsync(createLinkModel);
        transaction.Commit();
        link.Id = linkId;

        // Assert
        var actualLink = await linkRepository.GetLinkByIdAsync(linkId);
        actualLink.Should().NotBeNull()
            .And.BeEquivalentTo(link.ToModel());
    }

    [Fact]
    public async Task UpdateLinkAsync_ShouldUpdateLink()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), UserId);
        await InsertLinksAsync([link]);

        var updateLinkModel = new UpdateLinkModel
        {
            Url = "https://www.microsoft.com",
            Title = "Microsoft",
            Description = "Microsoft Corporation",
            Tags = ["Microsoft", "Software", "Technology"],
        };
        var updatedLink = updateLinkModel.ToLink(TimeProvider.Now);
        updatedLink.Id = link.Id;
        updatedLink.UserId = UserId;
        updatedLink.CreatedOnDate = link.CreatedOnDate;

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        await linkRepository.UpdateLinkAsync(link.Id, updateLinkModel);

        // Assert
        var actualLink = await linkRepository.GetLinkByIdAsync(link.Id);
        actualLink.Should().NotBeNull()
            .And.BeEquivalentTo(updatedLink.ToModel());
    }

    [Fact]
    public async Task UpdateLinkAsync_WithNonExistingId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var updateLinkModel = new UpdateLinkModel
        {
            Url = "https://www.microsoft.com",
            Title = "Microsoft",
            Description = "Microsoft Corporation",
            Tags = ["Microsoft", "Software", "Technology"]
        };
        var linkId = Guid.NewGuid();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            linkRepository.UpdateLinkAsync(linkId, updateLinkModel));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteLinkAsync_ShouldDeleteLink()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), UserId);
        await InsertLinksAsync([link]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        await linkRepository.DeleteLinkAsync(link.Id);

        // Assert
        var links = (await linkRepository.GetAllLinksAsync(1, 10)).ToList();
        links.Should().NotBeNull()
            .And.BeEmpty();
    }

    [Fact]
    public async Task DeleteLinkAsync_WithNonExistingId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var linkId = Guid.NewGuid();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var linkRepository = new LinkRepository(connection, transaction, _currentUserService, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => linkRepository.DeleteLinkAsync(linkId));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>();
    }

    private Guid GetAdminUserId()
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        const string sql = "SELECT Id FROM [dbo].[AppUsers]";
        return connection.QueryFirst<Guid>(sql, transaction: transaction);
    }

    private async Task InsertLinksAsync(List<Link> links)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var link in links)
        {
            link.UserId = UserId;
            await connection.ExecuteAsync(
                "INSERT INTO [dbo].[Links] (Id, Url, Title, Description, Tags, UserId, CreatedOnDate) VALUES (@Id, @Url, @Title, @Description, @Tags, @UserId, @CreatedOnDate)",
                link, transaction);
        }

        transaction.Commit();
        connection.Close();
    }

    private async Task ClearLinksTableAsync()
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        await connection.ExecuteAsync("DELETE FROM [dbo].[Links]", transaction: transaction);
        transaction.Commit();
        connection.Close();
    }

    public void Dispose()
    {
        ClearLinksTableAsync().Wait();
    }
}