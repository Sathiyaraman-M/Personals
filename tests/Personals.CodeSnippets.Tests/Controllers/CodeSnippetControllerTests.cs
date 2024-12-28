using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute.ExceptionExtensions;
using Personals.CodeSnippets.Abstractions.Repositories;
using Personals.CodeSnippets.Entities;
using Personals.CodeSnippets.Extensions;
using Personals.CodeSnippets.Models;
using Personals.CodeSnippets.Repositories;
using Personals.Common.Contracts.CodeSnippets;
using Personals.Common.Enums;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using Personals.Infrastructure.Services;
using Personals.Server;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace Personals.CodeSnippets.Tests.Controllers;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class CodeSnippetControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private SqlServerDbContext DbContext => new(_databaseFixture.ConnectionString);

    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _databaseFixture;
    private readonly Guid _userId;
    private readonly string _jwtBearer;

    public CodeSnippetControllerTests(WebApplicationFactory<Program> factory, DatabaseFixture databaseFixture)
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
    public async Task GetCodeSnippetsAsync_ReturnsPaginatedListOfCodeSnippets()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippets = new List<CodeSnippet>
        {
            CodeSnippetFactory.Create(Guid.NewGuid(), _userId),
            CodeSnippetFactory.Create(Guid.NewGuid(), _userId, "console.log('Hello, World!');", Language.JavaScript,
                "Sample JS snippet"),
        };
        await InsertCodeSnippetsAsync(codeSnippets);
        var expectedResponses = codeSnippets
            .OrderByDescending(x => x.CreatedOnDate)
            .ThenBy(x => x.Snippet)
            .ToModels().ToResponses();

        // Act
        var response =
            await client.GetFromJsonAsync<PaginatedResult<CodeSnippetResponse>>(
                "/api/code-snippets?page=1&pageSize=10");

        // Assert
        var result = response.Should().NotBeNull().And.BeOfType<PaginatedResult<CodeSnippetResponse>>().Subject;
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull()
            .And.BeOfType<List<CodeSnippetResponse>>().Which.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetCodeSnippetsAsync_ReturnsPaginatedListOfCodeSnippets_WhenSearchQueryIsProvided()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippets = new List<CodeSnippet>
        {
            CodeSnippetFactory.Create(Guid.NewGuid(), _userId),
            CodeSnippetFactory.Create(Guid.NewGuid(), _userId, "console.log('Hello, World!');", Language.JavaScript,
                "Sample JS snippet"),
        };
        await InsertCodeSnippetsAsync(codeSnippets);
        var searchTerm = "JS";
        var expectedResponses = codeSnippets
            .Where(x => x.Title!.Contains(searchTerm))
            .OrderByDescending(x => x.CreatedOnDate)
            .ToModels().ToResponses();

        // Act
        var response =
            await client.GetFromJsonAsync<PaginatedResult<CodeSnippetResponse>>(
                $"/api/code-snippets?page=1&pageSize=10&search={searchTerm}");

        // Assert
        var result = response.Should().NotBeNull().And.BeOfType<PaginatedResult<CodeSnippetResponse>>().Subject;
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull()
            .And.BeOfType<List<CodeSnippetResponse>>().Which.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetCodeSnippetsAsync_ReturnsPaginatedListOfCodeSnippets_WhenPageIsLessThanOne()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync("/api/code-snippets?page=0&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCodeSnippetsAsync_ReturnsPaginatedListOfCodeSnippets_WhenPageSizeIsLessThanOne()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync("/api/code-snippets?page=1&pageSize=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCodeSnippetsAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/code-snippets?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCodeSnippetsAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var codeSnippetRepository = Substitute.For<ICodeSnippetRepository>();
            codeSnippetRepository.GetAllCodeSnippetsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<CodeSnippet, ICodeSnippetRepository, CodeSnippetRepository>()
                .Returns(codeSnippetRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync("/api/code-snippets?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task GetCodeSnippetAsync_ReturnsCodeSnippet_ForGivenId()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), _userId);
        await InsertCodeSnippetsAsync([codeSnippet]);

        // Act
        var response =
            await client.GetFromJsonAsync<SuccessfulResult<CodeSnippetResponse>>(
                $"/api/code-snippets/{codeSnippet.Id}");

        // Assert
        var result = response.Should().NotBeNull().And.BeOfType<SuccessfulResult<CodeSnippetResponse>>().Subject;
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull().And.BeEquivalentTo(codeSnippet.ToModel().ToResponse());
    }

    [Fact]
    public async Task GetCodeSnippetAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/code-snippets/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCodeSnippetAsync_ReturnsNotFound_WhenCodeSnippetDoesNotExist()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/code-snippets/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCodeSnippetAsync_ReturnsBadRequest_WhenCodeSnippetIdIsEmptyGuid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/code-snippets/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCodeSnippetAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var codeSnippetRepository = Substitute.For<ICodeSnippetRepository>();
            codeSnippetRepository.GetCodeSnippetByIdAsync(Arg.Any<Guid>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<CodeSnippet, ICodeSnippetRepository, CodeSnippetRepository>()
                .Returns(codeSnippetRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/code-snippets/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCodeSnippetAsync_ReturnsCreatedCodeSnippet()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippetRequest = new CreateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Remarks = "Sample C# snippet",
            Tags = ["Beginner", "Console"]
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/code-snippets", codeSnippetRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult<CodeSnippetResponse>>();
        result.Should().NotBeNull()
            .And.BeOfType<SuccessfulResult<CodeSnippetResponse>>().Which.Succeeded.Should().BeTrue();
        var codeSnippet = result!.Data;
        codeSnippet.Title.Should().Be(codeSnippetRequest.Title);
        codeSnippet.Snippet.Should().Be(codeSnippetRequest.Snippet);
        codeSnippet.Language.Should().Be(codeSnippetRequest.Language);
        codeSnippet.Remarks.Should().Be(codeSnippetRequest.Remarks);
        codeSnippet.Tags.Should().BeEquivalentTo(codeSnippetRequest.Tags);
    }

    [Fact]
    public async Task CreateCodeSnippetAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var codeSnippetRequest = new CreateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Remarks = "Sample C# snippet",
            Tags = ["Beginner", "Console"]
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/code-snippets", codeSnippetRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCodeSnippetAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var codeSnippetRepository = Substitute.For<ICodeSnippetRepository>();
            codeSnippetRepository.CreateCodeSnippetAsync(Arg.Any<CreateCodeSnippetModel>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<CodeSnippet, ICodeSnippetRepository, CodeSnippetRepository>()
                .Returns(codeSnippetRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippetRequest = new CreateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Remarks = "Sample C# snippet",
            Tags = ["Beginner", "Console"]
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/code-snippets", codeSnippetRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCodeSnippetAsync_ReturnsUpdatedCodeSnippet()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), _userId);
        await InsertCodeSnippetsAsync([codeSnippet]);
        var codeSnippetRequest = new UpdateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Remarks = "Sample C# snippet",
            Tags = ["Beginner", "Console"]
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/code-snippets/{codeSnippet.Id}", codeSnippetRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult<CodeSnippetResponse>>();
        result.Should().NotBeNull()
            .And.BeOfType<SuccessfulResult<CodeSnippetResponse>>().Which.Succeeded.Should().BeTrue();
        result!.Data.Title.Should().Be(codeSnippetRequest.Title);
        result.Data.Snippet.Should().Be(codeSnippetRequest.Snippet);
        result.Data.Language.Should().Be(codeSnippetRequest.Language);
        result.Data.Remarks.Should().Be(codeSnippetRequest.Remarks);
        result.Data.Tags.Should().BeEquivalentTo(codeSnippetRequest.Tags);
    }

    [Fact]
    public async Task UpdateCodeSnippetAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var codeSnippetRequest = new UpdateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Tags = ["Beginner", "Console"]
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/code-snippets/{Guid.NewGuid()}", codeSnippetRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCodeSnippetAsync_ReturnsNotFound_WhenCodeSnippetDoesNotExist()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippetRequest = new UpdateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Tags = ["Beginner", "Console"]
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/code-snippets/{Guid.NewGuid()}", codeSnippetRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCodeSnippetAsync_ReturnsBadRequest_WhenCodeSnippetIdIsEmptyGuid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippetRequest = new UpdateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Tags = ["Beginner", "Console"]
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/code-snippets/{Guid.Empty}", codeSnippetRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCodeSnippetAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var codeSnippetRepository = Substitute.For<ICodeSnippetRepository>();
            codeSnippetRepository.UpdateCodeSnippetAsync(Arg.Any<Guid>(), Arg.Any<UpdateCodeSnippetModel>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<CodeSnippet, ICodeSnippetRepository, CodeSnippetRepository>()
                .Returns(codeSnippetRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippetRequest = new UpdateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Tags = ["Beginner", "Console"]
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/code-snippets/{Guid.NewGuid()}", codeSnippetRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCodeSnippetAsync_ReturnsOk()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), _userId);
        await InsertCodeSnippetsAsync([codeSnippet]);

        // Act
        var response = await client.DeleteAsync($"/api/code-snippets/{codeSnippet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult>();
        result.Should().NotBeNull()
            .And.BeOfType<SuccessfulResult>().Which.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCodeSnippetAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/code-snippets/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCodeSnippetAsync_ReturnsNotFound_WhenCodeSnippetDoesNotExist()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/code-snippets/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCodeSnippetAsync_ReturnsBadRequest_WhenCodeSnippetIdIsEmptyGuid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/code-snippets/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCodeSnippetAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var codeSnippetRepository = Substitute.For<ICodeSnippetRepository>();
            codeSnippetRepository.DeleteCodeSnippetAsync(Arg.Any<Guid>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<CodeSnippet, ICodeSnippetRepository, CodeSnippetRepository>()
                .Returns(codeSnippetRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/code-snippets/{Guid.NewGuid()}");

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

    private async Task InsertCodeSnippetsAsync(List<CodeSnippet> codeSnippets)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var codeSnippet in codeSnippets)
        {
            codeSnippet.UserId = _userId;
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