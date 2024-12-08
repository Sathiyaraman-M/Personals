using Expensive.Common.Wrappers.Abstractions;
using System.Text.Json.Serialization;

namespace Expensive.Common.Wrappers;

public class GenericFailedResult : IFailedResult
{
    public List<string> Messages { get; init; } = [];

    public bool Succeeded => false;

    public GenericFailedResult()
    {
    }

    protected GenericFailedResult(string message)
    {
        Messages = [message];
    }

    protected GenericFailedResult(List<string> messages)
    {
        Messages = messages;
    }

    public static IResult Fail()
    {
        return new GenericFailedResult();
    }

    public static IResult Fail(string message)
    {
        return new GenericFailedResult(message);
    }

    public static IResult Fail(List<string> messages)
    {
        return new GenericFailedResult(messages);
    }
}

public class GenericFailedResult<T> : GenericFailedResult, IFailedResult<T>
{
    [JsonIgnore] public T Data => default!;

    public GenericFailedResult()
    {
    }

    private GenericFailedResult(string message) : base(message)
    {
    }

    private GenericFailedResult(List<string> messages) : base(messages)
    {
    }

    public static new IResult<T> Fail()
    {
        return new GenericFailedResult<T>();
    }

    public static new IResult<T> Fail(string message)
    {
        return new GenericFailedResult<T>(message);
    }

    public static new IResult<T> Fail(List<string> messages)
    {
        return new GenericFailedResult<T>(messages);
    }
}