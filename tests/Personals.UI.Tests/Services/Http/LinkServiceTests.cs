using Personals.Common.Contracts.Links;
using Personals.Common.Wrappers;
using Personals.Links.Entities;
using Personals.Links.Extensions;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Fixtures;
using Personals.UI.Services.Http;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Personals.UI.Tests.Services.Http;

[Collection(nameof(WiremockCollectionFixture))]
public class LinkServiceTests(WiremockFixture wiremockFixture)
{
    private readonly HttpClient _httpClient = wiremockFixture.Server.CreateClient();
    
    private LinkService LinkService => new(_httpClient);

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        var links = new List<Link> { LinkFactory.Create(Guid.NewGuid(), Guid.NewGuid()) }
            .Select(x => x.ToModel().ToResponse()).ToList();
        var expectedResult = PaginatedResult<LinkResponse>.Create(links, 1, 10, links.Count);
        wiremockFixture.Server
            .Given(
                Request.Create().WithPath("/api/links")
                    .WithParam("page", "1")
                    .WithParam("pageSize", "10")
                    .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.GetAllAsync(1, 10);

        // Assert
        result.Should().NotBeNull().And.BeOfType<PaginatedResult<LinkResponse>>();
        result.Succeeded.Should().BeTrue();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
        result.Data.Should().NotBeNull().And.BeEquivalentTo(expectedResult.Data);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnResult()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var expectedResult = SuccessfulResult<LinkResponse>.Succeed(link.ToModel().ToResponse());
        wiremockFixture.Server
            .Given(
                Request.Create().WithPath($"/api/links/{link.Id}")
                    .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.GetByIdAsync(link.Id);

        // Assert
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult<LinkResponse>>();
        result.Succeeded.Should().BeTrue();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
        result.Data.Should().NotBeNull().And.BeEquivalentTo(expectedResult.Data);
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailedResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = GenericFailedResult<LinkResponse>.Fail("Link not found.");
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/links/{id}").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult<LinkResponse>>();
        result.Succeeded.Should().BeFalse();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnResult()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var request = new CreateLinkRequest
        {
            Title = link.Title, Url = link.Url, Description = link.Description, Tags = [.. link.Tags.Split(',')]
        };
        var response = link.ToModel().ToResponse();
        var expectedResult = SuccessfulResult<LinkResponse>.Succeed(response);
        wiremockFixture.Server
            .Given(Request.Create().WithPath("/api/links").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult>();
        result.Succeeded.Should().BeTrue();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldReturnResult()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var request = new UpdateLinkRequest
        {
            Title = link.Title, Url = link.Url, Description = link.Description, Tags = [.. link.Tags.Split(',')]
        };
        var expectedResult = SuccessfulResult.Succeed();
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/links/{link.Id}").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.UpdateAsync(link.Id, request);

        // Assert
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult>();
        result.Succeeded.Should().BeTrue();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldReturnFailedResult()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var request = new UpdateLinkRequest
        {
            Title = link.Title, Url = link.Url, Description = link.Description, Tags = [.. link.Tags.Split(',')]
        };
        var expectedResult = GenericFailedResult.Fail("Failed to update link");
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/links/{link.Id}").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(400).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.UpdateAsync(link.Id, request);

        // Assert
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>();
        result.Succeeded.Should().BeFalse();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldReturnFailedResult_WhenLinkNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateLinkRequest
        {
            Title = "Title", Url = "https://example.com", Description = "Description", Tags = ["tag1", "tag2"]
        };
        var expectedResult = GenericFailedResult.Fail("Link not found.");
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/links/{id}").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(404).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.UpdateAsync(id, request);

        // Assert
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>();
        result.Succeeded.Should().BeFalse();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldReturnResult()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var expectedResult = SuccessfulResult.Succeed();
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/links/{link.Id}").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.DeleteAsync(link.Id);

        // Assert
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult>();
        result.Succeeded.Should().BeTrue();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldReturnFailedResult()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var expectedResult = GenericFailedResult.Fail("Failed to delete link");
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/links/{link.Id}").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(400).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.DeleteAsync(link.Id);

        // Assert
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>();
        result.Succeeded.Should().BeFalse();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldReturnFailedResult_WhenLinkNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResult = GenericFailedResult.Fail("Link not found.");
        wiremockFixture.Server
            .Given(Request.Create().WithPath($"/api/links/{id}").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(404).WithBodyAsJson(expectedResult));

        // Act
        var result = await LinkService.DeleteAsync(id);

        // Assert
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>();
        result.Succeeded.Should().BeFalse();
        result.Messages.Should().BeEquivalentTo(expectedResult.Messages);
    }
}