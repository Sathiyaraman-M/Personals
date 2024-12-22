using Personals.Infrastructure.Abstractions.Entities;

namespace Personals.Links.Entities;

public class Link : IUserSpecificEntity
{
    public Guid Id { get; set; }
    
    public string Url { get; set; } = string.Empty;
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public string Tags { get; set; } = string.Empty;
    
    public Guid UserId { get; set; }
    
    public DateTime CreatedOnDate { get; set; }
    
    public DateTime? LastModifiedOnDate { get; set; }
}