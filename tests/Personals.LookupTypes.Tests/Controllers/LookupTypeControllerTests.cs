using Dapper;
using Personals.Common.Contracts.LookupTypes;
using Personals.Common.Enums;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using Personals.Infrastructure.Services;
using Personals.LookupTypes.Abstractions.Repositories;
using Personals.LookupTypes.Entities;
using Personals.LookupTypes.Extensions;
using Personals.LookupTypes.Models;
using Personals.LookupTypes.Repositories;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute.ExceptionExtensions;
using Personals.Server;
using System.Net;
using System.Net.Http.Json;

namespace Personals.LookupTypes.Tests.Controllers;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class LookupTypeControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private SqlServerDbContext DbContext => new(_databaseFixture.ConnectionString);
    
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _databaseFixture;
    private readonly Guid _userId;
    private readonly string _jwtBearer;

    public LookupTypeControllerTests(WebApplicationFactory<Program> factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _userId = GetAdminUserId();
        _jwtBearer = TestJwtBearerBuilder
            .CreateWithDefaultClaims()
            .WithUserId(_userId.ToString())
            .Build();
    }

    public static TheoryData<LookupTypeCategory, string> TestCasesPerCategory => new()
    {
        { LookupTypeCategory.ExpenseType, "/api/lookup-types/expense-types" },
        { LookupTypeCategory.PaymentMethod, "/api/lookup-types/payment-methods" },
    };

    public static TheoryData<LookupTypeCategory> LookupTypeCategories() => new(Enum.GetValues<LookupTypeCategory>());

    public static TheoryData<string> LookupTypeCategoriesUrls() =>
        new(TestCasesPerCategory.Select(x => x[1].ToString()!));

    [Theory]
    [MemberData(nameof(TestCasesPerCategory))]
    public async Task GetLookupTypesAsync_ReturnsPaginatedListOfLookupTypes_ForGivenCategory(
        LookupTypeCategory category, string uri)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_1", "Look-up Type 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_2", "Look-up Type 2")
        };
        lookupTypes = [.. lookupTypes.OrderBy(x => x.Name)];
        await InsertLookupTypesAsync(lookupTypes);
        var expectedLookupTypes = GetLookupTypeResponses(lookupTypes);

        // Act
        var response = await client.GetFromJsonAsync<PaginatedResult<LookupTypeResponse>>(uri);

        // Assert
        response.Should().NotBeNull();
        response!.Data.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(lookupTypes.Count)
            .And.BeEquivalentTo(expectedLookupTypes);
    }

    [Theory]
    [MemberData(nameof(TestCasesPerCategory))]
    public async Task GetLookupTypesAsync_ReturnsPaginatedListOfLookupTypes_WhenSearchTextIsProvided_ForGivenCategory(
        LookupTypeCategory category, string uri)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_1", "Look-up Type 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_2", "Look-up Type 2")
        };
        lookupTypes = [.. lookupTypes.OrderBy(x => x.Name)];
        await InsertLookupTypesAsync(lookupTypes);
        var expectedLookupTypes = GetLookupTypeResponses(lookupTypes.Where(x => x.Name.Contains("Type 1")).ToList());

        // Act
        var response = await client.GetFromJsonAsync<PaginatedResult<LookupTypeResponse>>($"{uri}?searchText=Type%201");

        // Assert
        response.Should().NotBeNull();
        response!.Data.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(expectedLookupTypes.Count)
            .And.BeEquivalentTo(expectedLookupTypes);
    }

    [Theory]
    [MemberData(nameof(TestCasesPerCategory))]
    public async Task
        GetLookupTypesAsync_ReturnsPaginatedListOfLookupTypes_WhenPageAndPageSizeAreProvided_ForGivenCategory(
            LookupTypeCategory category, string uri)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_1", "Look-up Type 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_2", "Look-up Type 2"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_3", "Look-up Type 3"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_4", "Look-up Type 4")
        };
        lookupTypes = [.. lookupTypes.OrderBy(x => x.Name)];
        await InsertLookupTypesAsync(lookupTypes);
        var expectedLookupTypes = GetLookupTypeResponses(lookupTypes.Take(2).ToList());

        // Act
        var response = await client.GetFromJsonAsync<PaginatedResult<LookupTypeResponse>>($"{uri}?page=1&pageSize=2");

        // Assert
        response.Should().NotBeNull();
        response!.Data.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(expectedLookupTypes.Count)
            .And.BeEquivalentTo(expectedLookupTypes);
    }

    [Theory]
    [MemberData(nameof(TestCasesPerCategory))]
    public async Task
        GetLookupTypesAsync_ReturnsPaginatedListOfLookupTypes_WhenPageAndPageSizeAndSearchTextAreProvided_ForGivenCategory(
            LookupTypeCategory category, string uri)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_1", "Look-up Type 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_2", "Look-up Type 2 - Special"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_3", "Look-up Type 3"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_4", "Look-up Type 4 - Special"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, _userId, "CODE_5", "Look-up Type 5 - Special")
        };
        await InsertLookupTypesAsync(lookupTypes);
        const string searchText = "Special";
        var expectedLookupTypes = lookupTypes
            .OrderBy(x => x.Name)
            .Where(x => x.Name.Contains(searchText))
            .ToList();
        var expectedLookupTypeResponses = GetLookupTypeResponses(expectedLookupTypes)
            .Skip(2).Take(2).ToList();

        // Act
        var response =
            await client.GetFromJsonAsync<PaginatedResult<LookupTypeResponse>>(
                $"{uri}?page=2&pageSize=2&searchText={searchText}");

        // Assert
        response.Should().NotBeNull();
        response!.Data.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(expectedLookupTypeResponses.Count)
            .And.BeEquivalentTo(expectedLookupTypeResponses);
    }

    [Fact]
    public async Task GetLookupTypesAsync_ReturnsBadRequest_WhenRequestedCategoryIsInvalid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync("/api/lookup-types/invalid-category");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategoriesUrls))]
    public async Task GetLookupTypesAsync_ReturnsBadRequest_WhenPageIsLessThanOne(string uri)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"{uri}?page=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategoriesUrls))]
    public async Task GetLookupTypesAsync_ReturnsBadRequest_WhenPageSizeIsLessThanOne(string uri)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"{uri}?pageSize=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(TestCasesPerCategory))]
    public async Task GetLookupTypesAsync_ReturnsInternalServerError_WhenAnErrorOccurs_ForGivenCategory(
        LookupTypeCategory category, string uri)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var lookupTypeRepository = Substitute.For<ILookupTypeRepository>();
            lookupTypeRepository.GetAllLookupTypesAsync(category, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<LookupType, ILookupTypeRepository, LookupTypeRepository>()
                .Returns(lookupTypeRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync(uri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetLookupTypeAsync_ReturnsLookupType_ForGivenId(LookupTypeCategory category)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category, _userId);
        await InsertLookupTypesAsync([lookupType]);

        // Act
        var response =
            await client.GetFromJsonAsync<SuccessfulResult<LookupTypeResponse>>($"/api/lookup-types/{lookupType.Id}");

        // Assert
        var result = response.Should().NotBeNull().And.BeOfType<SuccessfulResult<LookupTypeResponse>>().Subject;
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull().And.BeEquivalentTo(lookupType.ToResponse());
    }

    [Fact]
    public async Task GetLookupTypeAsync_ReturnsNotFound_WhenLookupTypeDoesNotExist()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/lookup-types/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLookupTypeAsync_ReturnsBadRequest_WhenLookupTypeIdIsEmptyGuid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/lookup-types/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLookupTypeAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var lookupTypeRepository = Substitute.For<ILookupTypeRepository>();
            lookupTypeRepository.GetLookupTypeByIdAsync(Arg.Any<Guid>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<LookupType, ILookupTypeRepository, LookupTypeRepository>()
                .Returns(lookupTypeRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/lookup-types/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task CreateLookupTypeAsync_ReturnsCreatedLookupType(LookupTypeCategory category)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupTypeRequest = new CreateLookupTypeRequest
        {
            Category = category, Code = "Code", Name = "Look-up Type"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/lookup-types", lookupTypeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult<LookupTypeResponse>>();
        result.Should().NotBeNull()
            .And.BeOfType<SuccessfulResult<LookupTypeResponse>>().Which.Succeeded.Should().BeTrue();
        var lookupType = result!.Data;
        lookupType.Name.Should().Be(lookupTypeRequest.Name);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task CreateLookupTypeAsync_ReturnsInternalServerError_WhenAnErrorOccurs(LookupTypeCategory category)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var lookupTypeRepository = Substitute.For<ILookupTypeRepository>();
            lookupTypeRepository.CreateLookupTypeAsync(Arg.Any<CreateLookupTypeModel>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<LookupType, ILookupTypeRepository, LookupTypeRepository>()
                .Returns(lookupTypeRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);
        var lookupTypeRequest = new CreateLookupTypeRequest
        {
            Category = category, Code = "Code", Name = "Look-up Type"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/lookup-types", lookupTypeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task UpdateLookupTypeAsync_ReturnsUpdatedLookupType(LookupTypeCategory category)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category, _userId);
        await InsertLookupTypesAsync([lookupType]);
        var lookupTypeRequest = new UpdateLookupTypeRequest
        {
            Category = category, Code = "Code", Name = "Look-up Type"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/lookup-types/{lookupType.Id}", lookupTypeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult<LookupTypeResponse>>();
        result.Should().NotBeNull()
            .And.BeOfType<SuccessfulResult<LookupTypeResponse>>().Which.Succeeded.Should().BeTrue();
        var updatedDesignation = result!.Data;
        updatedDesignation.Name.Should().Be(lookupTypeRequest.Name);
    }

    [Fact]
    public async Task UpdateLookupTypeAsync_ReturnsBadRequest_WhenCategoryIsDifferent()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, _userId);
        await InsertLookupTypesAsync([lookupType]);
        var lookupTypeRequest = new UpdateLookupTypeRequest
        {
            Category = LookupTypeCategory.PaymentMethod, Code = "Code Updated", Name = "Look-up Type Updated"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/lookup-types/{lookupType.Id}", lookupTypeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLookupTypeAsync_ReturnsNotFound_WhenLookupTypeDoesNotExist()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupTypeRequest = new UpdateLookupTypeRequest { Code = "Code", Name = "Look-up Type" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/lookup-types/{Guid.NewGuid()}", lookupTypeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateLookupTypeAsync_ReturnsBadRequest_WhenLookupTypeIdIsEmptyGuid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupTypeRequest = new UpdateLookupTypeRequest { Code = "Code", Name = "Look-up Type" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/lookup-types/{Guid.Empty}", lookupTypeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLookupTypeAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var lookupTypeRepository = Substitute.For<ILookupTypeRepository>();
            lookupTypeRepository.UpdateLookupTypeAsync(Arg.Any<Guid>(), Arg.Any<UpdateLookupTypeModel>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<LookupType, ILookupTypeRepository, LookupTypeRepository>()
                .Returns(lookupTypeRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);
        var lookupTypeRequest = new UpdateLookupTypeRequest { Code = "Code", Name = "Look-up Type" };

        // Act
        var response = await client.PutAsJsonAsync($"/api/lookup-types/{Guid.NewGuid()}", lookupTypeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task DeleteLookupTypeAsync_ReturnsNoContent(LookupTypeCategory category)
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category, _userId);
        await InsertLookupTypesAsync([lookupType]);

        // Act
        var response = await client.DeleteAsync($"/api/lookup-types/{lookupType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteLookupTypeAsync_ReturnsNotFound_WhenLookupTypeDoesNotExist()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/lookup-types/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteLookupTypeAsync_ReturnsBadRequest_WhenLookupTypeIdIsEmptyGuid()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/lookup-types/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteLookupTypeAsync_ReturnsInternalServerError_WhenAnErrorOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var lookupTypeRepository = Substitute.For<ILookupTypeRepository>();
            lookupTypeRepository.DeleteLookupTypeAsync(Arg.Any<Guid>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<LookupType, ILookupTypeRepository, LookupTypeRepository>()
                .Returns(lookupTypeRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/lookup-types/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    private static List<LookupTypeResponse> GetLookupTypeResponses(IList<LookupType> lookupTypes)
    {
        var lookupTypeResponses = lookupTypes.Select(x => x.ToResponse()).ToList();
        var serialNo = 1;
        lookupTypeResponses.ForEach(x => x.SerialNo = serialNo++);
        return lookupTypeResponses;
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

    private async Task InsertLookupTypesAsync(List<LookupType> lookupTypes)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var lookupType in lookupTypes)
        {
            lookupType.UserId = _userId;
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