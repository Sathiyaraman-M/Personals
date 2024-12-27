using Personals.Common.Contracts.CodeSnippets;
using Personals.CodeSnippets.Entities;
using Personals.CodeSnippets.Extensions;
using Personals.CodeSnippets.Models;

namespace Personals.CodeSnippets.Tests;

public static class TestMappingExtensions
{
    public static IEnumerable<CodeSnippetResponse> ToResponses(this IEnumerable<CodeSnippetModel> models)
    {
        var responses = models.Select(x => x.ToResponse()).ToList();
        var serialNo = 1;
        responses.ForEach(x => x.SerialNo = serialNo++);
        return responses;
    }

    public static IEnumerable<CodeSnippetModel> ToModels(this IEnumerable<CodeSnippet> links)
    {
        return links.Select(x => x.ToModel());
    }
}