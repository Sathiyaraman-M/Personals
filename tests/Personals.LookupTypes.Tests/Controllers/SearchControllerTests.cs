using Dapper;
using Personals.Common.Contracts.LookupTypes;
using Personals.Common.Enums;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Services;
using Personals.LookupTypes.Entities;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Personals.Server;
using System.Net.Http.Json;

namespace Personals.LookupTypes.Tests.Controllers;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class SearchControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _databaseFixture;

    public SearchControllerTests(WebApplicationFactory<Program> factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _userId = GetAdminUserId();
        _jwtBearer = TestJwtBearerBuilder
            .CreateWithDefaultClaims()
            .WithUserId(_userId.ToString())
            .Build();
    }

    private SqlServerDbContext DbContext => new(_databaseFixture.ConnectionString);

    private readonly Guid _userId;

    private readonly string _jwtBearer;

    [Fact]
    public async Task SearchPaymentMethodsAsync_ShouldReturnPaymentMethods()
    {
        // Arrange
        const string searchString = "Payment";
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, _userId, "CODE_1",
                "Payment Method 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, _userId, "CODE_2",
                "Payment Method 2"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, _userId, "CODE_3", "Expense Type 1")
        };

        await InsertLookupTypesAsync(lookupTypes);

        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/lookup-types/search/payment-methods?searchTerm={searchString}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<LookupTypeSearchResponse>>();

        // Assert
        var expectedLookupTypes = lookupTypes.Select(x => new LookupTypeSearchResponse(x.Id, x.Code, x.Name)).ToList();
        result.Should().BeEquivalentTo(expectedLookupTypes.Where(x => x.Name.Contains(searchString)));
    }

    [Fact]
    public async Task SearchPaymentMethodsAsync_ShouldReturnEmptyList_WhenNoPaymentMethodsFound()
    {
        // Arrange
        const string searchString = "search";

        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/lookup-types/search/payment-methods?searchTerm={searchString}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<LookupTypeSearchResponse>>();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchPaymentMethodsAsync_ShouldReturnPaymentMethods_WhenSearchTermIsEmpty()
    {
        // Arrange
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, _userId, "CODE_1",
                "Payment Method 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, _userId, "CODE_2",
                "Payment Method 2"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, _userId, "CODE_3", "Expense Type 1")
        };

        await InsertLookupTypesAsync(lookupTypes);

        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync("/api/lookup-types/search/payment-methods");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<LookupTypeSearchResponse>>();

        // Assert
        var expectedLookupTypes = lookupTypes
            .Where(x => x.Category == LookupTypeCategory.PaymentMethod)
            .Select(x => new LookupTypeSearchResponse(x.Id, x.Code, x.Name))
            .ToList();
        result.Should().BeEquivalentTo(expectedLookupTypes);
    }

    [Fact]
    public async Task SearchExpenseTypesAsync_ShouldReturnExpenseTypes()
    {
        // Arrange
        const string searchString = "Expense";
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, _userId, "CODE_1",
                "Payment Method 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, _userId, "CODE_2",
                "Payment Method 2"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, _userId, "CODE_3", "Expense Type 1")
        };

        await InsertLookupTypesAsync(lookupTypes);

        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/lookup-types/search/expense-types?searchTerm={searchString}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<LookupTypeSearchResponse>>();

        // Assert
        var expectedLookupTypes = lookupTypes.Select(x => new LookupTypeSearchResponse(x.Id, x.Code, x.Name)).ToList();
        result.Should().BeEquivalentTo(expectedLookupTypes.Where(x => x.Name.Contains(searchString)));
    }

    [Fact]
    public async Task SearchExpenseTypesAsync_ShouldReturnEmptyList_WhenNoExpenseTypesFound()
    {
        // Arrange
        const string searchString = "search";

        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync($"/api/lookup-types/search/expense-types?searchTerm={searchString}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<LookupTypeSearchResponse>>();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchExpenseTypesAsync_ShouldReturnExpenseTypes_WhenSearchTermIsEmpty()
    {
        // Arrange
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, _userId, "CODE_1",
                "Payment Method 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, _userId, "CODE_2",
                "Payment Method 2"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, _userId, "CODE_3", "Expense Type 1")
        };

        await InsertLookupTypesAsync(lookupTypes);

        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(_jwtBearer);

        // Act
        var response = await client.GetAsync("/api/lookup-types/search/expense-types");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<LookupTypeSearchResponse>>();

        // Assert
        var expectedLookupTypes = lookupTypes
            .Where(x => x.Category == LookupTypeCategory.ExpenseType)
            .Select(x => new LookupTypeSearchResponse(x.Id, x.Code, x.Name))
            .ToList();
        result.Should().BeEquivalentTo(expectedLookupTypes);
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