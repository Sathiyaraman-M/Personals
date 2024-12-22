namespace Personals.Common.Contracts.Links;

public record LinkResponse(
    Guid Id,
    string Url,
    string? Title,
    string? Description,
    List<string> Tags,
    DateTime CreatedOn,
    DateTime? LastModifiedOn)
{
    public int SerialNo { get; set; } 
}