using Personals.Common.Contracts.Links;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Links.Abstractions.Repositories;
using Personals.Links.Abstractions.Services;
using Personals.Links.Entities;
using Personals.Links.Extensions;
using Personals.Links.Repositories;

namespace Personals.Links.Services;

public class LinkService(IUnitOfWork unitOfWork) : ILinkService
{
    private readonly ILinkRepository _linkRepository = unitOfWork.Repository<Link, ILinkRepository, LinkRepository>();
    
    public async Task<PaginatedResult<LinkResponse>> GetAllLinksAsync(int page, int pageSize, string? search = null)
    {
        if (page < 1)
        {
            throw new ArgumentException("Invalid page number");
        }
        if (pageSize < 1)
        {
            throw new ArgumentException("Invalid page size");
        }
        var links = (await _linkRepository.GetAllLinksAsync(page, pageSize, search)).ToList();
        var count = await _linkRepository.GetLinksCountAsync(search);
        var linkResponses = links.Select(link => link.ToResponse()).ToList();
        var serialNo = (page - 1) * pageSize;
        linkResponses.ForEach(link => link.SerialNo = ++serialNo);
        return PaginatedResult<LinkResponse>.Create(linkResponses, page, pageSize, count);
    }

    public async Task<IResult<LinkResponse>> GetLinkByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid link id");
        }
        var link = await _linkRepository.GetLinkByIdAsync(id);
        return SuccessfulResult<LinkResponse>.Succeed(link.ToResponse());
    }

    public async Task<IResult<LinkResponse>> CreateLinkAsync(CreateLinkRequest request)
    {
        var model = request.ToModel();
        
        unitOfWork.BeginTransaction();
        var linkId = await _linkRepository.CreateLinkAsync(model);
        unitOfWork.CommitChanges();
        
        return await GetLinkByIdAsync(linkId);
    }

    public async Task<IResult<LinkResponse>> UpdateLinkAsync(Guid id, UpdateLinkRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid link id");
        }
        var model = request.ToModel();
        
        unitOfWork.BeginTransaction();
        await _linkRepository.UpdateLinkAsync(id, model);
        unitOfWork.CommitChanges();
        
        return await GetLinkByIdAsync(id);
    }

    public async Task<IResult> DeleteLinkAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid link id");
        }
        _ = await _linkRepository.GetLinkByIdAsync(id);
        
        unitOfWork.BeginTransaction();
        await _linkRepository.DeleteLinkAsync(id);
        unitOfWork.CommitChanges();
        
        return SuccessfulResult.Succeed("Link deleted successfully");
    }
}