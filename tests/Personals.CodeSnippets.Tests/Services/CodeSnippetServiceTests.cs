using NSubstitute.ExceptionExtensions;
using Personals.CodeSnippets.Abstractions.Repositories;
using Personals.CodeSnippets.Entities;
using Personals.CodeSnippets.Extensions;
using Personals.CodeSnippets.Repositories;
using Personals.CodeSnippets.Services;
using Personals.Common.Contracts.CodeSnippets;
using Personals.Common.Enums;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using Personals.Tests.Base.Factories;

namespace Personals.CodeSnippets.Tests.Services;

public class CodeSnippetServiceTests
{
    private readonly IUnitOfWork _unitOfWorkStub = Substitute.For<IUnitOfWork>();
    private readonly ICodeSnippetRepository _codeSnippetRepositoryStub = Substitute.For<ICodeSnippetRepository>();
    private readonly ICurrentUserService _currentUserServiceStub = Substitute.For<ICurrentUserService>();

    private const string UserName = "user1";
    private static readonly Guid UserId = Guid.NewGuid();

    public CodeSnippetServiceTests()
    {
        _currentUserServiceStub.UserName.Returns(UserName);
        _currentUserServiceStub.UserId.Returns(UserId);
        _currentUserServiceStub.IsAuthenticated.Returns(true);
        _currentUserServiceStub.IsAdmin.Returns(true);
    }

    private CodeSnippetService CodeSnippetService
    {
        get
        {
            _unitOfWorkStub.Repository<CodeSnippet, ICodeSnippetRepository, CodeSnippetRepository>()
                .Returns(_codeSnippetRepositoryStub);
            return new CodeSnippetService(_unitOfWorkStub);
        }
    }
    
    [Fact]
    public async Task GetAllCodeSnippetsAsync_ShouldReturnCodeSnippets()
    {
        // Arrange
        var codeSnippets = new List<CodeSnippet>
        {
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId),
            CodeSnippetFactory.Create(Guid.NewGuid(), UserId, "console.log('Hello, World!');", Language.JavaScript, "Sample JS snippet"),
        };
        var codeSnippetModels = codeSnippets.Select(x => x.ToModel()).ToList();

        _codeSnippetRepositoryStub.GetAllCodeSnippetsAsync(1, 10).Returns(codeSnippetModels);
        _codeSnippetRepositoryStub.GetCodeSnippetsCountAsync().Returns(codeSnippets.Count);

        var expectedCodeSnippets = GetCodeSnippetResponses(codeSnippets);

        // Act
        var result = await CodeSnippetService.GetAllCodeSnippetsAsync(1, 10);

        // Assert
        result.Should().BeOfType<PaginatedResult<CodeSnippetResponse>>();
        result.TotalCount.Should().Be(codeSnippets.Count);
        result.Data.Should().BeEquivalentTo(expectedCodeSnippets);
    }
    
    [Fact]
    public async Task GetAllCodeSnippetsAsync_ShouldThrowArgumentException_WhenPageIsLessThanOne()
    {
        // Arrange
        const int page = 0;
        const int pageSize = 10;

        // Act
        Func<Task> action = async () => await CodeSnippetService.GetAllCodeSnippetsAsync(page, pageSize);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid page");
    }
    
    [Fact]
    public async Task GetAllCodeSnippetsAsync_ShouldThrowArgumentException_WhenPageSizeIsLessThanOne()
    {
        // Arrange
        const int page = 1;
        const int pageSize = 0;

        // Act
        Func<Task> action = async () => await CodeSnippetService.GetAllCodeSnippetsAsync(page, pageSize);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid page size");
    }
    
    [Fact]
    public async Task GetCodeSnippetByIdAsync_ShouldReturnCodeSnippet()
    {
        // Arrange
        var codeSnippet = CodeSnippetFactory.Create(Guid.NewGuid(), UserId);
        var codeSnippetModel = codeSnippet.ToModel();

        _codeSnippetRepositoryStub.GetCodeSnippetByIdAsync(codeSnippet.Id).Returns(codeSnippetModel);

        var expectedCodeSnippet = codeSnippetModel.ToResponse();

        // Act
        var result = await CodeSnippetService.GetCodeSnippetByIdAsync(codeSnippet.Id);

        // Assert
        result.Should().BeOfType<SuccessfulResult<CodeSnippetResponse>>();
        result.Data.Should().BeEquivalentTo(expectedCodeSnippet);
    }
    
    [Fact]
    public async Task CreateCodeSnippetAsync_ShouldReturnCodeSnippet()
    {
        // Arrange
        var createCodeSnippetRequest = new CreateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Remarks = "Prints Hello, World!",
            Tags = []
        };
        var createCodeSnippetModel = createCodeSnippetRequest.ToModel();
        var codeSnippetId = Guid.NewGuid();
        var codeSnippet = CodeSnippetFactory.Create(
            codeSnippetId,
            UserId,
            createCodeSnippetModel.Snippet,
            createCodeSnippetModel.Language,
            createCodeSnippetModel.Title,
            createCodeSnippetModel.Remarks,
            createCodeSnippetModel.Tags);

        _codeSnippetRepositoryStub.CreateCodeSnippetAsync(createCodeSnippetModel).Returns(codeSnippetId);
        _codeSnippetRepositoryStub.GetCodeSnippetByIdAsync(codeSnippetId).Returns(codeSnippet.ToModel());

        var expectedCodeSnippet = codeSnippet.ToModel().ToResponse();

        // Act
        var result = await CodeSnippetService.CreateCodeSnippetAsync(createCodeSnippetRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult<CodeSnippetResponse>>();
        result.Data.Should().BeEquivalentTo(expectedCodeSnippet);
    }
    
    [Fact]
    public async Task UpdateCodeSnippetAsync_ShouldReturnCodeSnippet()
    {
        // Arrange
        var codeSnippetId = Guid.NewGuid();
        var updateCodeSnippetRequest = new UpdateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
            Tags = ["Beginner", "Console"]
        };
        var updateCodeSnippetModel = updateCodeSnippetRequest.ToModel();
        var codeSnippet = CodeSnippetFactory.Create(codeSnippetId, UserId, updateCodeSnippetModel.Snippet, updateCodeSnippetModel.Language, updateCodeSnippetModel.Title);

        _codeSnippetRepositoryStub.UpdateCodeSnippetAsync(codeSnippetId, updateCodeSnippetModel).Returns(Task.CompletedTask);
        _codeSnippetRepositoryStub.GetCodeSnippetByIdAsync(codeSnippetId).Returns(codeSnippet.ToModel());

        var expectedCodeSnippet = codeSnippet.ToModel().ToResponse();

        // Act
        var result = await CodeSnippetService.UpdateCodeSnippetAsync(codeSnippetId, updateCodeSnippetRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult<CodeSnippetResponse>>();
        result.Data.Should().BeEquivalentTo(expectedCodeSnippet);
    }
    
    [Fact]
    public async Task UpdateCodeSnippetAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        // Arrange
        var updateCodeSnippetRequest = new UpdateCodeSnippetRequest
        {
            Title = "Hello, World!",
            Snippet = "Console.WriteLine(\"Hello, World!\");",
            Language = Language.CSharp,
        };

        // Act
        Func<Task> action = async () => await CodeSnippetService.UpdateCodeSnippetAsync(Guid.Empty, updateCodeSnippetRequest);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid code snippet id");
    }
    
    [Fact]
    public async Task DeleteCodeSnippetAsync_ShouldReturnSuccessfulResult()
    {
        // Arrange
        var codeSnippetId = Guid.NewGuid();
        var codeSnippet = CodeSnippetFactory.Create(codeSnippetId, UserId);

        _codeSnippetRepositoryStub.GetCodeSnippetByIdAsync(codeSnippetId).Returns(codeSnippet.ToModel());

        // Act
        var result = await CodeSnippetService.DeleteCodeSnippetAsync(codeSnippetId);

        // Assert
        result.Should().BeOfType<SuccessfulResult>();
    }
    
    [Fact]
    public async Task DeleteCodeSnippetAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        // Arrange

        // Act
        Func<Task> action = async () => await CodeSnippetService.DeleteCodeSnippetAsync(Guid.Empty);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid code snippet id");
    }
    
    [Fact]
    public async Task DeleteCodeSnippetAsync_ShouldThrowEntityNotFoundException_WhenCodeSnippetNotFound()
    {
        // Arrange
        var codeSnippetId = Guid.NewGuid();
        _codeSnippetRepositoryStub.GetCodeSnippetByIdAsync(codeSnippetId).Throws(new EntityNotFoundException("Code Snippet not found"));

        // Act
        Func<Task> action = async () => await CodeSnippetService.DeleteCodeSnippetAsync(codeSnippetId);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Code Snippet not found");
    }

    private static List<CodeSnippetResponse> GetCodeSnippetResponses(IList<CodeSnippet> codeSnippets)
    {
        var codeSnippetResponses = codeSnippets.Select(x => x.ToModel().ToResponse()).ToList();
        var serialNo = 1;
        codeSnippetResponses.ForEach(x => x.SerialNo = serialNo++);
        return codeSnippetResponses;
    }
}