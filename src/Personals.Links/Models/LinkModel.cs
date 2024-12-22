namespace Personals.Links.Models;

public class LinkModel
{
    public Guid Id { get; set; }
    
    public string Url { get; set; } = string.Empty;
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }

    public List<string> Tags { get; set; } = [];
    
    public DateTime CreatedOnDate { get; set; }
    
    public DateTime? LastModifiedOnDate { get; set; }
}