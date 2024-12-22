using System.ComponentModel.DataAnnotations;

namespace Personals.Common.Contracts.Links;

public record UpdateLinkRequest
{
    public UpdateLinkRequest()
    {
    }
    
    public UpdateLinkRequest(string url, string? title, string? description, List<string> tags)
    {
        Url = url;
        Title = title;
        Description = description;
        Tags = tags;
    }

    [Required(ErrorMessage = "Please provide a URL")]
    public required string Url { get; set; } = string.Empty;
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public List<string> Tags { get; set; } = [];
}