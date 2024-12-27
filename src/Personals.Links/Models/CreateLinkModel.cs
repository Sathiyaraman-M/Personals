namespace Personals.Links.Models;

public record CreateLinkModel
{
    public string Url { get; set; } = string.Empty;
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }

    public List<string> Tags { get; set; } = [];
}