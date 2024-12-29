using Personals.CodeSnippets.Entities;
using Personals.CodeSnippets.Extensions;
using Personals.Common.Contracts.CodeSnippets;
using Personals.Common.Enums;
using Personals.Common.Wrappers;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Fixtures;
using Personals.UI.Services.Http;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Personals.UI.Tests.Services.Http;

[Collection(nameof(WiremockCollectionFixture))]
public class CodeSnippetServiceTests(WiremockFixture wiremockFixture)
{
    private readonly HttpClient _httpClient = wiremockFixture.Server.CreateClient();

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        var codeSnippets = new List<CodeSnippet> { CodeSnippetFactory.Create(Guid.NewGuid(), Guid.NewGuid()) }
            .Select(x => x.ToModel().ToResponse()).ToList();
        var expectedResult = PaginatedResult<CodeSnippetResponse>.Create(codeSnippets, 1, 10, codeSnippets.Count);
        wiremockFixture.Server
            .Given(
                Request.Create().WithPath("/api/code-snippets")
                    .WithParam("page", "1")
                    .WithParam("pageSize", "10")
                    .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));
        var service = new CodeSnippetService(_httpClient);

        // Act
        var result = await service.GetAllAsync(1, 10);

        // Assert
        result.Should().NotBeNull().And.BeOfType<PaginatedResult<CodeSnippetResponse>>();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull().And.BeEquivalentTo(expectedResult.Data);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnResult()
    {
        // Arrange
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var expectedResult = SuccessfulResult<CodeSnippetResponse>.Succeed(codeSnippet.ToModel().ToResponse());
        wiremockFixture.Server
            .Given(
                Request.Create().WithPath($"/api/code-snippets/{codeSnippet.Id}")
                    .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));
        var service = new CodeSnippetService(_httpClient);

        // Act
        var result = await service.GetByIdAsync(codeSnippet.Id);

        // Assert
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult<CodeSnippetResponse>>();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull().And.BeEquivalentTo(expectedResult.Data);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailedResult_WhenCodeSnippetNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = GenericFailedResult<CodeSnippetResponse>.Fail("Code snippet not found.");
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/code-snippets/{id}").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404).WithBodyAsJson(expectedResult));
        var service = new CodeSnippetService(_httpClient);

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult<CodeSnippetResponse>>();
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnResult()
    {
        // Arrange
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var request = new CreateCodeSnippetRequest
        {
            Snippet = codeSnippet.Snippet,
            Language = codeSnippet.Language,
            Title = codeSnippet.Title,
            Remarks = codeSnippet.Remarks,
            Tags = [.. codeSnippet.Tags.Split(',')]
        };
        var response = codeSnippet.ToModel().ToResponse();
        var expectedResult = SuccessfulResult<CodeSnippetResponse>.Succeed(response);
        wiremockFixture.Server
            .Given(Request.Create().WithPath("/api/code-snippets").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));
        var service = new CodeSnippetService(_httpClient);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult>();
        result.Succeeded.Should().BeTrue();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnResult()
    {
        // Arrange
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var request = new UpdateCodeSnippetRequest
        {
            Snippet = codeSnippet.Snippet,
            Language = codeSnippet.Language,
            Title = codeSnippet.Title,
            Remarks = codeSnippet.Remarks,
            Tags = [.. codeSnippet.Tags.Split(',')]
        };
        var expectedResult = SuccessfulResult.Succeed();
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/code-snippets/{codeSnippet.Id}").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));
        var service = new CodeSnippetService(_httpClient);

        // Act
        var result = await service.UpdateAsync(codeSnippet.Id, request);

        // Assert
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult>();
        result.Succeeded.Should().BeTrue();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailedResult_WhenCodeSnippetNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateCodeSnippetRequest
        {
            Snippet = "Snippet",
            Language = Language.CSharp,
            Title = "Title",
            Remarks = "Remarks",
            Tags = ["tag1", "tag2"]
        };
        var expectedResult = GenericFailedResult.Fail("Code snippet not found.");
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/code-snippets/{id}").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(404).WithBodyAsJson(expectedResult));
        var service = new CodeSnippetService(_httpClient);

        // Act
        var result = await service.UpdateAsync(id, request);

        // Assert
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>();
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = SuccessfulResult.Succeed();
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/code-snippets/{id}").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));
        var service = new CodeSnippetService(_httpClient);

        // Act
        var result = await service.DeleteAsync(id);

        // Assert
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult>();
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailedResult_WhenCodeSnippetNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = GenericFailedResult.Fail("Code snippet not found.");
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/code-snippets/{id}").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(404).WithBodyAsJson(expectedResult));
        var service = new CodeSnippetService(_httpClient);

        // Act
        var result = await service.DeleteAsync(id);

        // Assert
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>();
        result.Succeeded.Should().BeFalse();
    }
}