using Personals.Common.Serialization;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Personals.UI.Extensions;

public static class ResultExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.Preserve
    };
    
    public static async Task<IResult> ToResult(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var responseAsString = await response.Content.ReadAsStringAsync(cancellationToken);
        return response.IsSuccessStatusCode
            ? JsonSerializer.Deserialize(responseAsString, ResultSerializationContext.Default.SuccessfulResult)!
            : JsonSerializer.Deserialize(responseAsString, ResultSerializationContext.Default.GenericFailedResult)!;
    }

    public static async Task<IResult<T>> ToResult<T>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var responseAsString = await response.Content.ReadAsStringAsync(cancellationToken);
        return response.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<SuccessfulResult<T>>(responseAsString, JsonSerializerOptions)!
            : JsonSerializer.Deserialize<GenericFailedResult<T>>(responseAsString, JsonSerializerOptions)!;
    }

    public static async Task<PaginatedResult<T>> ToPaginatedResult<T>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var responseAsString = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<PaginatedResult<T>>(responseAsString, JsonSerializerOptions)!;
    }
}