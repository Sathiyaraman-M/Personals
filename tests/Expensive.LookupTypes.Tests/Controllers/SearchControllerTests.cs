using Dapper;
using Expensive.Common.Contracts.LookupTypes;
using Expensive.Common.Enums;
using Expensive.Infrastructure.Abstractions.Services;
using Expensive.Infrastructure.Services;
using Expensive.LookupTypes.Entities;
using Expensive.Server;
using Expensive.Tests.Base;
using Expensive.Tests.Base.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Json;

namespace Expensive.LookupTypes.Tests.Controllers;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class SearchControllerTests(
    WebApplicationFactory<Program> factory,
    DatabaseFixture databaseFixture
) : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private SqlServerDbContext DbContext => new(databaseFixture.ConnectionString);

    [Fact]
    public async Task SearchPaymentMethodsAsync_ShouldReturnPaymentMethods()
    {
        // Arrange
        const string searchString = "Payment";
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, "CODE_1",
                "Payment Method 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, "CODE_2",
                "Payment Method 2"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, "CODE_3", "Expense Type 1")
        };

        await InsertLookupTypesAsync(lookupTypes);

        var client = GetCustomWebApplicationFactory().CreateClient();

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

        var client = GetCustomWebApplicationFactory().CreateClient();

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
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, "CODE_1",
                "Payment Method 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, "CODE_2",
                "Payment Method 2"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, "CODE_3", "Expense Type 1")
        };

        await InsertLookupTypesAsync(lookupTypes);

        var client = GetCustomWebApplicationFactory().CreateClient();

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
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, "CODE_1",
                "Payment Method 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, "CODE_2",
                "Payment Method 2"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, "CODE_3", "Expense Type 1")
        };

        await InsertLookupTypesAsync(lookupTypes);

        var client = GetCustomWebApplicationFactory().CreateClient();

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

        var client = GetCustomWebApplicationFactory().CreateClient();

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
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, "CODE_1",
                "Payment Method 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.PaymentMethod, "CODE_2",
                "Payment Method 2"),
            LookupTypeFactory.Create(Guid.NewGuid(), LookupTypeCategory.ExpenseType, "CODE_3", "Expense Type 1")
        };

        await InsertLookupTypesAsync(lookupTypes);

        var client = GetCustomWebApplicationFactory().CreateClient();

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
        return factory.GetCustomWebApplicationFactory(services =>
        {
            services.RemoveAll<IDbContext>();
            services.AddScoped<IDbContext>(_ => DbContext);
            configureServices?.Invoke(services);
        });
    }

    private async Task InsertLookupTypesAsync(List<LookupType> lookupTypes)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var lookupType in lookupTypes)
        {
            await connection.ExecuteAsync(
                "INSERT INTO [dbo].[LookupTypes] (Id, Category, Code, Name, CreatedByUserName, CreatedByUserId, CreatedOnDate) VALUES (@Id, @Category, @Code, @Name, @CreatedByUserName, @CreatedByUserId, @CreatedOnDate);",
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