using Microsoft.Extensions.DependencyInjection;
using Personals.CodeSnippets.Abstractions.Services;
using Personals.CodeSnippets.Services;

namespace Personals.CodeSnippets.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodeSnippetsModule(this IServiceCollection services)
    {
        services.AddScoped<ICodeSnippetService, CodeSnippetService>();
        return services;
    }
}