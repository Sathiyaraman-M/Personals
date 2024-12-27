using Personals.Common.Contracts.Links;
using Personals.Links.Entities;
using Personals.Links.Extensions;
using Personals.Links.Models;

namespace Personals.Links.Tests;

public static class TestMappingExtensions
{
    public static IEnumerable<LinkResponse> ToResponses(this IEnumerable<LinkModel> models)
    {
        var responses = models.Select(x => x.ToResponse()).ToList();
        var serialNo = 1;
        responses.ForEach(x => x.SerialNo = serialNo++);
        return responses;
    }

    public static IEnumerable<LinkModel> ToModels(this IEnumerable<Link> links)
    {
        return links.Select(x => x.ToModel());
    }
}