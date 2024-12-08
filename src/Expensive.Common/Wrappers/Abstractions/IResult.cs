using System.Text.Json.Serialization;

namespace Expensive.Common.Wrappers.Abstractions;

public interface IResult
{
    [JsonPropertyName("messages")] List<string> Messages { get; }

    [JsonPropertyName("succeeded")] bool Succeeded { get; }
}

public interface IResult<out T> : IResult
{
    [JsonPropertyName("data")] T Data { get; }
}