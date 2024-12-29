using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute.ExceptionExtensions;
using Personals.Common.Contracts.Links;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using Personals.Infrastructure.Services;
using Personals.Links.Abstractions.Repositories;
using Personals.Links.Entities;
using Personals.Links.Extensions;
using Personals.Links.Models;
using Personals.Links.Repositories;
using Personals.Server;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace Personals.Links.Tests.Controllers;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class LinkControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private SqlServerDbContext DbContext => new(_databaseFixture.ConnectionString);
    
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _databaseFixture;
    private readonly Guid _userId;
    private readonly string _jwtBearer;

    public LinkControllerTests(WebApplicationFactory<Program> factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _userId = GetAdminUserId();
        _jwtBearer = TestJwtBearerBuilder
            .CreateWithDefaultClaims()
            .WithUserId(_userId.ToString())
            .Build();
    }
    
    [Fact]
    public async Task GetLinksAsync_ReturnsPaginatedListOfLinks()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var links = new List<Link>
        {
            LinkFactory.Create(Guid.NewGuid(), _userId),
            LinkFactory.Create(Guid.NewGuid(), _userId, "https://www.bing.com", "Bing", "Search Engine", ["Search", "Engine"]),
        };
        await InsertLinksAsync(links);
        var expectedResponses = links
            .OrderByDescending(x => x.CreatedOnDate)
            .ThenBy(x => x.Url)
            .ToModels().ToResponses();

        // Act
        var response = await client.GetFromJsonAsync<PaginatedResult<LinkResponse>>("/api/links?page=1&pageSize=10");

        // Assert
        var result = response.Should().NotBeNull().And.BeOfType<PaginatedResult<LinkResponse>>().Subject;
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull()
            .And.BeOfType<List<LinkResponse>>().Which.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResponses);
    }
    
    [Fact]
    public async Task GetLinksAsync_ReturnsPaginatedListOfLinks_WhenSearchQueryIsProvided()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var links = new List<Link>
        {
            LinkFactory.Create(Guid.NewGuid(), _userId),
            LinkFactory.Create(Guid.NewGuid(), _userId, "https://www.bing.com", "Bing", "Search Engine", ["Search", "Engine"]),
        };
        await InsertLinksAsync(links);
        var searchTerm = "bing";
        var expectedResponses = links
            .Where(x => x.Url.Contains(searchTerm))
            .OrderByDescending(x => x.CreatedOnDate)
            .ThenBy(x => x.Url)
            .ToModels().ToResponses();

        // Act
        var response = await client.GetFromJsonAsync<PaginatedResult<LinkResponse>>($"/api/links?page=1&pageSize=10&search={searchTerm}");

        // Assert
        var result = response.Should().NotBeNull().And.BeOfType<PaginatedResult<LinkResponse>>().Subject;
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull()
            .And.BeOfType<List<LinkResponse>>().Which.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResponses);
    }
    
    [Fact]
    public async Task GetLinksAsync_ReturnsPaginatedListOfLinks_WhenPageIsLessThanOne()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync("/api/links?page=0&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetLinksAsync_ReturnsPaginatedListOfLinks_WhenPageSizeIsLessThanOne()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync("/api/links?page=1&pageSize=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetLinksAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/links?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task GetLinksAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var linkRepository = Substitute.For<ILinkRepository>();
            linkRepository.GetAllLinksAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<Link, ILinkRepository, LinkRepository>()
                .Returns(linkRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync("/api/links?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetLinkAsync_ReturnsLink_ForGivenId()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var link = LinkFactory.Create(Guid.NewGuid(), _userId);
        await InsertLinksAsync([link]);

        // Act
        var response =
            await client.GetFromJsonAsync<SuccessfulResult<LinkResponse>>($"/api/links/{link.Id}");

        // Assert
        var result = response.Should().NotBeNull().And.BeOfType<SuccessfulResult<LinkResponse>>().Subject;
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull().And.BeEquivalentTo(link.ToModel().ToResponse());
    }
    
    [Fact]
    public async Task GetLinkAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/links/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLinkAsync_ReturnsNotFound_WhenLinkDoesNotExist()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/links/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLinkAsync_ReturnsBadRequest_WhenLinkIdIsEmptyGuid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/links/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLinkAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var linkRepository = Substitute.For<ILinkRepository>();
            linkRepository.GetLinkByIdAsync(Arg.Any<Guid>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<Link, ILinkRepository, LinkRepository>()
                .Returns(linkRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/links/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }
    
    [Fact]
    public async Task CreateLinkAsync_ReturnsCreatedLink()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var linkRequest = new CreateLinkRequest
        {
            Url = "https://www.google.com",
            Title = "Google",
            Description = "Search Engine",
            Tags = ["Search", "Engine"]
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/links", linkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult<LinkResponse>>();
        result.Should().NotBeNull()
            .And.BeOfType<SuccessfulResult<LinkResponse>>().Which.Succeeded.Should().BeTrue();
        var link = result!.Data;
        link.Url.Should().Be(linkRequest.Url);
        link.Title.Should().Be(linkRequest.Title);
        link.Description.Should().Be(linkRequest.Description);
        link.Tags.Should().BeEquivalentTo(linkRequest.Tags);
    }
    
    [Fact]
    public async Task CreateLinkAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var linkRequest = new CreateLinkRequest
        {
            Url = "https://www.google.com",
            Title = "Google",
            Description = "Search Engine",
            Tags = ["Search", "Engine"]
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/links", linkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateLinkAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var linkRepository = Substitute.For<ILinkRepository>();
            linkRepository.CreateLinkAsync(Arg.Any<CreateLinkModel>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<Link, ILinkRepository, LinkRepository>()
                .Returns(linkRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);
        var linkRequest = new CreateLinkRequest
        {
            Url = "https://www.google.com",
            Title = "Google",
            Description = "Search Engine",
            Tags = ["Search", "Engine"]
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/links", linkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLinkAsync_ReturnsUpdatedLink()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var link = LinkFactory.Create(Guid.NewGuid(), _userId);
        await InsertLinksAsync([link]);
        var linkRequest = new UpdateLinkRequest
        {
            Url = "https://www.bing.com",
            Title = "Bing",
            Description = "Search Engine",
            Tags = ["Search", "Engine"]
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/links/{link.Id}", linkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult<LinkResponse>>();
        result.Should().NotBeNull()
            .And.BeOfType<SuccessfulResult<LinkResponse>>().Which.Succeeded.Should().BeTrue();
        result!.Data.Url.Should().Be(linkRequest.Url);
        result.Data.Title.Should().Be(linkRequest.Title);
        result.Data.Description.Should().Be(linkRequest.Description);
        result.Data.Tags.Should().BeEquivalentTo(linkRequest.Tags);
    }
    
    [Fact]
    public async Task UpdateLinkAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var linkRequest = new UpdateLinkRequest { Url = "https://www.bing.com", Title = "Bing" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/links/{Guid.NewGuid()}", linkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateLinkAsync_ReturnsNotFound_WhenLinkDoesNotExist()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var linkRequest = new UpdateLinkRequest { Url = "https://www.bing.com", Title = "Bing" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/links/{Guid.NewGuid()}", linkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateLinkAsync_ReturnsBadRequest_WhenLinkIdIsEmptyGuid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var linkRequest = new UpdateLinkRequest { Url = "https://www.bing.com", Title = "Bing" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/links/{Guid.Empty}", linkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLinkAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var linkRepository = Substitute.For<ILinkRepository>();
            linkRepository.UpdateLinkAsync(Arg.Any<Guid>(), Arg.Any<UpdateLinkModel>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<Link, ILinkRepository, LinkRepository>()
                .Returns(linkRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);
        var linkRequest = new UpdateLinkRequest { Url = "https://www.bing.com", Title = "Bing" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/links/{Guid.NewGuid()}", linkRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }
    
    [Fact]
    public async Task DeleteLinkAsync_ReturnsOk()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var link = LinkFactory.Create(Guid.NewGuid(), _userId);
        await InsertLinksAsync([link]);

        // Act
        var response = await client.DeleteAsync($"/api/links/{link.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult>();
        result.Should().NotBeNull()
            .And.BeOfType<SuccessfulResult>().Which.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteLinkAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/links/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteLinkAsync_ReturnsNotFound_WhenLinkDoesNotExist()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/links/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteLinkAsync_ReturnsBadRequest_WhenLinkIdIsEmptyGuid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/links/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteLinkAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var linkRepository = Substitute.For<ILinkRepository>();
            linkRepository.DeleteLinkAsync(Arg.Any<Guid>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<Link, ILinkRepository, LinkRepository>()
                .Returns(linkRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/links/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    private WebApplicationFactory<Program> GetCustomWebApplicationFactory(
        Action<IServiceCollection>? configureServices = null)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(_userId);
        return _factory.GetCustomWebApplicationFactory(services =>
        {
            services.RemoveAll<IDbContext>();
            services.AddScoped<IDbContext>(_ => DbContext);
            services.RemoveAll<ICurrentUserService>();
            services.AddScoped<ICurrentUserService>(_ => currentUserService);
            configureServices?.Invoke(services);
        });
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
            link.UserId = _userId;
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