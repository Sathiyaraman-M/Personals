using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Links.Entities;
using Personals.Links.Models;

namespace Personals.Links.Abstractions.Repositories;

public interface ILinkRepository : IRepository<Link>
{
    Task<IEnumerable<LinkModel>> GetAllLinksAsync(int page, int pageSize, string? search = null);
    
    Task<long> GetLinksCountAsync(string? search = null);
    
    Task<LinkModel> GetLinkByIdAsync(Guid id);
    
    Task<Guid> CreateLinkAsync(CreateLinkModel model);
    
    Task UpdateLinkAsync(Guid id, UpdateLinkModel model);
    
    Task DeleteLinkAsync(Guid id);
}