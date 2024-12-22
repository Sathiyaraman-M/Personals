using NSubstitute.ExceptionExtensions;
using Personals.Common.Contracts.Links;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using Personals.Links.Abstractions.Repositories;
using Personals.Links.Entities;
using Personals.Links.Extensions;
using Personals.Links.Repositories;
using Personals.Links.Services;
using Personals.Tests.Base.Factories;

namespace Personals.Links.Tests.Services;

public class LinkServiceTests
{
    private readonly IUnitOfWork _unitOfWorkStub = Substitute.For<IUnitOfWork>();
    private readonly ILinkRepository _lookupTypeRepositoryStub = Substitute.For<ILinkRepository>();
    private readonly ICurrentUserService _currentUserServiceStub = Substitute.For<ICurrentUserService>();

    private const string UserName = "admin";
    private static readonly Guid UserId = Guid.NewGuid();

    public LinkServiceTests()
    {
        _currentUserServiceStub.UserName.Returns(UserName);
        _currentUserServiceStub.UserId.Returns(UserId);
        _currentUserServiceStub.IsAuthenticated.Returns(true);
        _currentUserServiceStub.IsAdmin.Returns(true);
    }

    private LinkService LinkService
    {
        get
        {
            _unitOfWorkStub.Repository<Link, ILinkRepository, LinkRepository>()
                .Returns(_lookupTypeRepositoryStub);
            return new LinkService(_unitOfWorkStub);
        }
    }
    
    [Fact]
    public async Task GetAllLinksAsync_ShouldReturnLinks()
    {
        // Arrange
        var links = new List<Link>
        {
            LinkFactory.Create(Guid.NewGuid(), UserId, "https://link1.com", "Link 1"),
            LinkFactory.Create(Guid.NewGuid(), UserId, "https://link2.com", "Link 2")
        };
        var linkModels = links.Select(x => x.ToModel()).ToList();

        _lookupTypeRepositoryStub.GetAllLinksAsync(1, 10).Returns(linkModels);
        _lookupTypeRepositoryStub.GetLinksCountAsync().Returns(links.Count);

        var expectedLinks = GetLinkResponses(links);

        // Act
        var result = await LinkService.GetAllLinksAsync(1, 10);

        // Assert
        result.Should().BeOfType<PaginatedResult<LinkResponse>>();
        result.TotalCount.Should().Be(links.Count);
        result.Data.Should().BeEquivalentTo(expectedLinks);
    }
    
    [Fact]
    public async Task GetAllLinksAsync_ShouldThrowArgumentException_WhenPageIsLessThanOne()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;

        // Act
        Func<Task> action = async () => await LinkService.GetAllLinksAsync(page, pageSize);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid page number");
    }
    
    [Fact]
    public async Task GetAllLinksAsync_ShouldThrowArgumentException_WhenPageSizeIsLessThanOne()
    {
        // Arrange
        const int page = 1;
        const int pageSize = 0;

        // Act
        Func<Task> action = async () => await LinkService.GetAllLinksAsync(page, pageSize);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid page size");
    }
    
    [Fact]
    public async Task GetLinkByIdAsync_ShouldReturnLink()
    {
        // Arrange
        var link = LinkFactory.Create(Guid.NewGuid(), UserId, "https://link1.com", "Link 1");
        var linkModel = link.ToModel();

        _lookupTypeRepositoryStub.GetLinkByIdAsync(link.Id).Returns(linkModel);

        var expectedLink = linkModel.ToResponse();

        // Act
        var result = await LinkService.GetLinkByIdAsync(link.Id);

        // Assert
        result.Should().BeOfType<SuccessfulResult<LinkResponse>>();
        result.Data.Should().BeEquivalentTo(expectedLink);
    }
    
    [Fact]
    public async Task CreateLinkAsync_ShouldReturnLink()
    {
        // Arrange
        var createLinkRequest = new CreateLinkRequest
        {
            Url = "https://link1.com",
            Title = "Link 1"
        };
        var createLinkModel = createLinkRequest.ToModel();
        var linkId = Guid.NewGuid();
        var link = LinkFactory.Create(linkId, UserId, createLinkModel.Url, createLinkModel.Title);

        _lookupTypeRepositoryStub.CreateLinkAsync(createLinkModel).Returns(linkId);
        _lookupTypeRepositoryStub.GetLinkByIdAsync(linkId).Returns(link.ToModel());

        var expectedLink = link.ToModel().ToResponse();

        // Act
        var result = await LinkService.CreateLinkAsync(createLinkRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult<LinkResponse>>();
        result.Data.Should().BeEquivalentTo(expectedLink);
    }
    
    [Fact]
    public async Task UpdateLinkAsync_ShouldReturnLink()
    {
        // Arrange
        var linkId = Guid.NewGuid();
        var updateLinkRequest = new UpdateLinkRequest
        {
            Url = "https://link1.com",
            Title = "Link 1"
        };
        var updateLinkModel = updateLinkRequest.ToModel();
        var link = LinkFactory.Create(linkId, UserId, updateLinkModel.Url, updateLinkModel.Title);

        _lookupTypeRepositoryStub.UpdateLinkAsync(linkId, updateLinkModel).Returns(Task.CompletedTask);
        _lookupTypeRepositoryStub.GetLinkByIdAsync(linkId).Returns(link.ToModel());

        var expectedLink = link.ToModel().ToResponse();

        // Act
        var result = await LinkService.UpdateLinkAsync(linkId, updateLinkRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult<LinkResponse>>();
        result.Data.Should().BeEquivalentTo(expectedLink);
    }
    
    [Fact]
    public async Task UpdateLinkAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        // Arrange
        var updateLinkRequest = new UpdateLinkRequest
        {
            Url = "https://link1.com",
            Title = "Link 1"
        };

        // Act
        Func<Task> action = async () => await LinkService.UpdateLinkAsync(Guid.Empty, updateLinkRequest);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid link id");
    }
    
    [Fact]
    public async Task DeleteLinkAsync_ShouldReturnSuccessfulResult()
    {
        // Arrange
        var linkId = Guid.NewGuid();
        var link = LinkFactory.Create(linkId, UserId, "https://link1.com", "Link 1");

        _lookupTypeRepositoryStub.GetLinkByIdAsync(linkId).Returns(link.ToModel());

        // Act
        var result = await LinkService.DeleteLinkAsync(linkId);

        // Assert
        result.Should().BeOfType<SuccessfulResult>();
    }
    
    [Fact]
    public async Task DeleteLinkAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        // Arrange

        // Act
        Func<Task> action = async () => await LinkService.DeleteLinkAsync(Guid.Empty);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid link id");
    }
    
    [Fact]
    public async Task DeleteLinkAsync_ShouldThrowEntityNotFoundException_WhenLinkNotFound()
    {
        // Arrange
        var linkId = Guid.NewGuid();
        _lookupTypeRepositoryStub.GetLinkByIdAsync(linkId).Throws(new EntityNotFoundException("Link not found"));

        // Act
        Func<Task> action = async () => await LinkService.DeleteLinkAsync(linkId);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Link not found");
    }

    private static List<LinkResponse> GetLinkResponses(IList<Link> lookupTypes)
    {
        var lookupTypeResponses = lookupTypes.Select(x => x.ToModel().ToResponse()).ToList();
        var serialNo = 1;
        lookupTypeResponses.ForEach(x => x.SerialNo = serialNo++);
        return lookupTypeResponses;
    }
}