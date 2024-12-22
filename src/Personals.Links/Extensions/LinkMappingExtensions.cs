using Personals.Common.Contracts.Links;
using Personals.Links.Entities;
using Personals.Links.Models;

namespace Personals.Links.Extensions;

public static class LinkMappingExtensions
{
    public static LinkModel ToModel(this Link link)
    {
        return new LinkModel
        {
            Id = link.Id,
            Url = link.Url,
            Title = link.Title,
            Description = link.Description,
            Tags = [.. link.Tags.Split(',')],
            CreatedOnDate = link.CreatedOnDate,
            LastModifiedOnDate = link.LastModifiedOnDate
        };
    }
    
    public static Link ToLink(this CreateLinkModel model, Guid userId, DateTime createdOnDate)
    {
        return new Link
        {
            Id = Guid.NewGuid(),
            Url = model.Url,
            Title = model.Title,
            Description = model.Description,
            Tags = string.Join(',', model.Tags),
            UserId = userId,
            CreatedOnDate = createdOnDate
        };
    }
    
    public static Link ToLink(this UpdateLinkModel model, DateTime lastModifiedOnDate)
    {
        return new Link
        {
            Url = model.Url,
            Title = model.Title,
            Description = model.Description,
            Tags = string.Join(',', model.Tags),
            LastModifiedOnDate = lastModifiedOnDate
        };
    }

    public static LinkResponse ToResponse(this LinkModel link)
    {
        return new LinkResponse(
            link.Id,
            link.Url,
            link.Title,
            link.Description,
            link.Tags,
            link.CreatedOnDate,
            link.LastModifiedOnDate
        );
    }
    
    public static CreateLinkModel ToModel(this CreateLinkRequest link)
    {
        return new CreateLinkModel
        {
            Url = link.Url,
            Title = link.Title,
            Description = link.Description,
            Tags = link.Tags,
        };
    }
    
    public static UpdateLinkModel ToModel(this UpdateLinkRequest link)
    {
        return new UpdateLinkModel
        {
            Url = link.Url,
            Title = link.Title,
            Description = link.Description,
            Tags = link.Tags,
        };
    }
}