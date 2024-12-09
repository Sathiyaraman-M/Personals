using Personals.Common.Contracts.LookupTypes;
using Personals.Common.Enums;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.LookupTypes.Abstractions.Repositories;
using Personals.LookupTypes.Entities;
using Personals.LookupTypes.Extensions;
using Personals.LookupTypes.Models;
using Personals.LookupTypes.Repositories;
using Personals.LookupTypes.Services;
using Personals.Tests.Base.Factories;

namespace Personals.LookupTypes.Tests.Services;

public class LookupTypeServiceTests
{
    private readonly IUnitOfWork _unitOfWorkStub = Substitute.For<IUnitOfWork>();
    private readonly ILookupTypeRepository _lookupTypeRepositoryStub = Substitute.For<ILookupTypeRepository>();
    private readonly ICurrentUserService _currentUserServiceStub = Substitute.For<ICurrentUserService>();

    private const string AdminUserName = "admin";
    private static readonly Guid AdminUserId = Guid.NewGuid();

    public LookupTypeServiceTests()
    {
        _currentUserServiceStub.UserName.Returns(AdminUserName);
        _currentUserServiceStub.UserId.Returns(AdminUserId);
        _currentUserServiceStub.IsAuthenticated.Returns(true);
        _currentUserServiceStub.IsAdmin.Returns(true);
    }

    private LookupTypeService LookupTypeService
    {
        get
        {
            _unitOfWorkStub.Repository<LookupType, ILookupTypeRepository, LookupTypeRepository>()
                .Returns(_lookupTypeRepositoryStub);
            return new LookupTypeService(_unitOfWorkStub, _currentUserServiceStub);
        }
    }

    public static TheoryData<LookupTypeCategory> LookupTypeCategories() => new(Enum.GetValues<LookupTypeCategory>());

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetAllLookupTypesAsync_ShouldReturnLookupTypes(LookupTypeCategory category)
    {
        // Arrange
        var lookupTypes = new List<LookupType>
        {
            LookupTypeFactory.Create(Guid.NewGuid(), category, "CODE_1", "Look-up Type 1"),
            LookupTypeFactory.Create(Guid.NewGuid(), category, "CODE_2", "Look-up Type 2")
        };

        _lookupTypeRepositoryStub.GetAllLookupTypesAsync(category, 1, 10).Returns(lookupTypes);
        _lookupTypeRepositoryStub.GetLookupTypesCountAsync(category).Returns(lookupTypes.Count);

        var expectedLookupTypes = GetLookupTypeResponses(lookupTypes);

        // Act
        var result = await LookupTypeService.GetAllLookupTypesAsync(category, 1, 10);

        // Assert
        result.Should().BeOfType<PaginatedResult<LookupTypeResponse>>();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeEquivalentTo(expectedLookupTypes);

        await _lookupTypeRepositoryStub.Received(1).GetAllLookupTypesAsync(category, 1, 10);
        await _lookupTypeRepositoryStub.Received(1).GetLookupTypesCountAsync(category);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetAllLookupTypesAsync_WithPageLessThanOne_ShouldThrowArgumentException(
        LookupTypeCategory category)
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => LookupTypeService.GetAllLookupTypesAsync(category, 0, 10));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetAllLookupTypesAsync_WithPageSizeLessThanOne_ShouldThrowArgumentException(
        LookupTypeCategory category)
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => LookupTypeService.GetAllLookupTypesAsync(category, 1, 0));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task GetLookupTypeByIdAsync_ShouldReturnLookupType(LookupTypeCategory category)
    {
        // Arrange
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category);

        _lookupTypeRepositoryStub.GetLookupTypeByIdAsync(lookupType.Id).Returns(lookupType);

        var expectedLookupType = lookupType.ToResponse();

        // Act
        var result = await LookupTypeService.GetLookupTypeByIdAsync(lookupType.Id);

        // Assert
        result.Should().BeOfType<SuccessfulResult<LookupTypeResponse>>();
        result.Data.Should().BeEquivalentTo(expectedLookupType);

        await _lookupTypeRepositoryStub.Received(1).GetLookupTypeByIdAsync(lookupType.Id);
    }

    [Fact]
    public async Task GetLookupTypeByIdAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => LookupTypeService.GetLookupTypeByIdAsync(Guid.Empty));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task CreateLookupTypeAsync_ShouldReturnCreatedLookupType(LookupTypeCategory category)
    {
        // Arrange
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category);
        var createLookupTypeRequest =
            new CreateLookupTypeRequest(lookupType.Category, lookupType.Code, lookupType.Name);

        _lookupTypeRepositoryStub.CreateLookupTypeAsync(Arg.Any<CreateLookupTypeModel>()).Returns(lookupType.Id);
        _lookupTypeRepositoryStub.GetLookupTypeByIdAsync(lookupType.Id).Returns(lookupType);

        // Act
        var result = await LookupTypeService.CreateLookupTypeAsync(createLookupTypeRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult<LookupTypeResponse>>();
        result.Data.Should().BeEquivalentTo(lookupType.ToResponse());

        await _lookupTypeRepositoryStub.Received(1).CreateLookupTypeAsync(Arg.Any<CreateLookupTypeModel>());
        await _lookupTypeRepositoryStub.Received(1).GetLookupTypeByIdAsync(lookupType.Id);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task UpdateLookupTypeAsync_ShouldReturnUpdatedLookupType(LookupTypeCategory category)
    {
        // Arrange
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category);
        var updateLookupTypeRequest =
            new UpdateLookupTypeRequest(lookupType.Category, lookupType.Code, lookupType.Name);

        _lookupTypeRepositoryStub.GetLookupTypeByIdAsync(lookupType.Id).Returns(lookupType);

        // Act
        var result = await LookupTypeService.UpdateLookupTypeAsync(lookupType.Id, updateLookupTypeRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult<LookupTypeResponse>>();
        result.Data.Should().BeEquivalentTo(lookupType.ToResponse());

        await _lookupTypeRepositoryStub.Received(1)
            .UpdateLookupTypeAsync(lookupType.Id, Arg.Any<UpdateLookupTypeModel>());
        await _lookupTypeRepositoryStub.Received(1).GetLookupTypeByIdAsync(lookupType.Id);
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task UpdateLookupTypeAsync_WithEmptyId_ShouldThrowArgumentException(LookupTypeCategory category)
    {
        // Arrange
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category);
        var updateLookupTypeRequest =
            new UpdateLookupTypeRequest(lookupType.Category, lookupType.Code, lookupType.Name);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            LookupTypeService.UpdateLookupTypeAsync(Guid.Empty, updateLookupTypeRequest));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(LookupTypeCategories))]
    public async Task DeleteLookupTypeAsync_ShouldDeleteLookupType(LookupTypeCategory category)
    {
        // Arrange
        var lookupType = LookupTypeFactory.Create(Guid.NewGuid(), category);

        // Act
        var result = await LookupTypeService.DeleteLookupTypeAsync(lookupType.Id);

        // Assert
        result.Should().BeOfType<SuccessfulResult>();

        await _lookupTypeRepositoryStub.Received(1).DeleteLookupTypeAsync(lookupType.Id);
    }

    [Fact]
    public async Task DeleteLookupTypeAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => LookupTypeService.DeleteLookupTypeAsync(Guid.Empty));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentException>();
    }

    private static List<LookupTypeResponse> GetLookupTypeResponses(IList<LookupType> lookupTypes)
    {
        var lookupTypeResponses = lookupTypes.Select(x => x.ToResponse()).ToList();
        var serialNo = 1;
        lookupTypeResponses.ForEach(x => x.SerialNo = serialNo++);
        return lookupTypeResponses;
    }
}